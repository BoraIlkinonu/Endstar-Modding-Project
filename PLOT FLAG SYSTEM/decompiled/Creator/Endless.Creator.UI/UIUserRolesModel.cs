using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Endless.Gameplay;
using Endless.Gameplay.RightsManagement;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIUserRolesModel : UIGameObject, IUILoadingSpinnerViewCompatible
{
	public enum Filters
	{
		None,
		NotInheritedFromParent,
		InheritedFromParent
	}

	public UnityEvent<List<UserRole>> OnUserRolesSet = new UnityEvent<List<UserRole>>();

	public UnityEvent<Roles> OnLocalClientRoleSet = new UnityEvent<Roles>();

	[SerializeField]
	private Filters filter;

	[SerializeField]
	private UIUserRoleListModel userRoleListModel;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private readonly HashSet<int> userIds = new HashSet<int>();

	private readonly List<User> users = new List<User>();

	private CancellationTokenSource addToEndAsyncCancellationTokenSource;

	private CancellationTokenSource setUserRolesAsyncCancellationTokenSource;

	[field: SerializeField]
	public AssetContexts AssetContext { get; private set; } = AssetContexts.GameInspectorPlay;

	[field: SerializeField]
	public UIUserRoleWizard.AssetTypes AssetType { get; private set; }

	public SerializableGuid AssetId { get; private set; }

	public string AssetName { get; private set; }

	public Roles LocalClientRole { get; private set; } = Roles.None;

	public IReadOnlyCollection<int> UserIds => userIds;

	public IReadOnlyCollection<User> Users => users;

	public IReadOnlyCollection<UserRole> UserRoles => userRoleListModel.UserRolesModel.userRoleListModel.ReadOnlyList;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public void Initialize(SerializableGuid assetId, string assetName, SerializableGuid ancestorId, AssetContexts context)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", assetId, assetName, ancestorId, context);
		}
		AssetContext = context;
		if (!AssetId.IsEmpty && AssetId != assetId)
		{
			Clear();
		}
		AssetId = assetId;
		AssetName = assetName;
		if (AssetContext == AssetContexts.NewGame)
		{
			LocalClientRole = Roles.Owner;
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			UserRole item = new UserRole(activeUserId, LocalClientRole);
			List<UserRole> list = new List<UserRole> { item };
			userRoleListModel.Set(list, triggerEvents: true);
			userIds.Clear();
			userIds.Add(activeUserId);
			users.Clear();
			OnUserRolesSet.Invoke(list);
			OnLocalClientRoleSet.Invoke(LocalClientRole);
		}
		else
		{
			MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(assetId, OnRoleChanged);
			MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(assetId, OnRoleChanged);
		}
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}, ", "userRoleListModel", userRoleListModel.Count) + string.Format("{0}: {1}, ", "AssetId", AssetId) + string.Format("{0}: {1}, ", "LocalClientRole", LocalClientRole) + string.Format("{0}: {1}, ", "userIds", userIds.Count) + string.Format("{0}: {1}, ", "AssetContext", AssetContext) + string.Format("{0}: {1}", "AssetType", AssetType);
	}

	public void AddToEnd(UserRole newUserRole)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "AddToEnd", newUserRole);
		}
		AddToEndAsync(newUserRole);
	}

	private async Task AddToEndAsync(UserRole newUserRole)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "AddToEndAsync", "newUserRole", newUserRole), this);
		}
		CancellationTokenSourceUtility.RecreateTokenSource(ref addToEndAsyncCancellationTokenSource);
		CancellationToken cancellationToken = addToEndAsyncCancellationTokenSource.Token;
		OnLoadingStarted.Invoke();
		try
		{
			userRoleListModel.Add(newUserRole, triggerEvents: true);
			userIds.Add(newUserRole.UserId);
			string userName = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserNameAsync(newUserRole.UserId, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			User item = new User(newUserRole.UserId, null, userName);
			users.Add(item);
		}
		catch (OperationCanceledException)
		{
			if (verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} ) was cancelled.", "AddToEndAsync", "newUserRole", newUserRole), this);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
		finally
		{
			OnLoadingEnded.Invoke();
		}
	}

	public void UpdateRole(UserRole newUserRole)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateRole", newUserRole);
		}
		int num = userRoleListModel.IndexOf(newUserRole.UserId);
		if (num > -1)
		{
			userRoleListModel.SetItem(num, newUserRole, triggerEvents: true);
		}
		else
		{
			DebugUtility.LogError("Could not find newUserRole in userRoleListModel!", this);
		}
	}

	public void RemoveUser(UserRole target)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "RemoveUser", target);
		}
		userIds.Remove(target.UserId);
		int num = IndexOf(users, target.UserId);
		if (num > -1)
		{
			users.RemoveAt(num);
		}
		else
		{
			DebugUtility.LogError("Could not find target in users!", this);
		}
		int num2 = userRoleListModel.IndexOf(target.UserId);
		if (num2 > -1)
		{
			userRoleListModel.RemoveAt(num2, triggerEvents: true);
		}
		else
		{
			DebugUtility.LogError("Could not find target in userRoleListModel!", this);
		}
	}

	public void SetAssetType(UIUserRoleWizard.AssetTypes assetType)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetAssetType", assetType);
		}
		AssetType = assetType;
	}

	public void SetAssetName(string assetName)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetAssetName", assetName);
		}
		AssetName = assetName;
	}

	public void Clear()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		userRoleListModel.Clear(triggerEvents: true);
		MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(AssetId, OnRoleChanged);
		SetUserRoles(new List<UserRole>());
		CancellationTokenSourceUtility.CancelAndCleanup(ref addToEndAsyncCancellationTokenSource);
	}

	private void SetUserRoles(List<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetUserRoles", userRoles.Count);
			DebugUtility.DebugEnumerable("userRoles", userRoles, this);
		}
		SetUserRolesAsync(userRoles);
	}

	private async Task SetUserRolesAsync(List<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "AddToEndAsync", "SetUserRolesAsync", userRoles.Count), this);
			DebugUtility.DebugEnumerable("userRoles", userRoles, this);
		}
		CancellationTokenSourceUtility.RecreateTokenSource(ref setUserRolesAsyncCancellationTokenSource);
		CancellationToken cancellationToken = setUserRolesAsyncCancellationTokenSource.Token;
		OnLoadingStarted.Invoke();
		try
		{
			int localClientUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			LocalClientRole = Roles.None;
			userIds.Clear();
			users.Clear();
			switch (filter)
			{
			case Filters.NotInheritedFromParent:
			{
				int count2 = userRoles.Count;
				userRoles = userRoles.Where((UserRole userRole2) => !userRole2.InheritedFromParent).ToList();
				if (verboseLogging)
				{
					DebugUtility.Log($"{base.gameObject.name} | Removed {count2 - userRoles.Count} userRoles that were explicitly assigned", this);
				}
				break;
			}
			case Filters.InheritedFromParent:
			{
				int count = userRoles.Count;
				userRoles = userRoles.Where((UserRole userRole2) => userRole2.InheritedFromParent).ToList();
				if (verboseLogging)
				{
					DebugUtility.Log($"{base.gameObject.name} | Removed {count - userRoles.Count} userRoles that were NOT explicitly assigned", this);
				}
				break;
			}
			default:
				DebugUtility.LogNoEnumSupportError(this, "SetUserRoles", filter, userRoles.Count);
				break;
			case Filters.None:
				break;
			}
			foreach (UserRole userRole in userRoles)
			{
				if (userRole.UserId == localClientUserId)
				{
					LocalClientRole = userRole.Role;
				}
				userIds.Add(userRole.UserId);
				string userName = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserNameAsync(userRole.UserId, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				User item = new User(userRole.UserId, null, userName);
				users.Add(item);
			}
			userRoleListModel.Set(userRoles, triggerEvents: true);
			OnUserRolesSet.Invoke(userRoles);
			OnLocalClientRoleSet.Invoke(LocalClientRole);
		}
		catch (OperationCanceledException)
		{
			if (verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} ) was cancelled.", "AddToEndAsync", "SetUserRolesAsync", userRoles.Count), this);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
		finally
		{
			OnLoadingEnded.Invoke();
		}
	}

	private void OnRoleChanged(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnRoleChanged", userRoles.Count);
		}
		List<UserRole> userRoles2 = userRoles.ToList();
		SetUserRoles(userRoles2);
	}

	private int IndexOf(List<User> usersList, int userId)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "IndexOf", usersList.Count(), userId);
		}
		for (int i = 0; i < usersList.Count; i++)
		{
			if (usersList[i].Id == userId)
			{
				return i;
			}
		}
		return -1;
	}
}
