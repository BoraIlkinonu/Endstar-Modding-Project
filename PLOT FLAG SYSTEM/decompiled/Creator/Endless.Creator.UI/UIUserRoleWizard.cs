using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIUserRoleWizard : UIMonoBehaviourSingleton<UIUserRoleWizard>, IUILoadingSpinnerViewCompatible
{
	public enum AssetTypes
	{
		Game,
		Level,
		Tileset,
		Prop,
		Audio
	}

	private enum Flows
	{
		Add,
		Change
	}

	private enum States
	{
		Uninitialized,
		UserSelection,
		RoleSelection
	}

	public UnityEvent OnComplete = new UnityEvent();

	[SerializeField]
	private UIGameLibraryAssetDetailModalView gameLibraryAssetDetailModalSource;

	[SerializeField]
	private UIUserRolesModalView userRolesModalSource;

	[SerializeField]
	private EndlessStudiosUserId endlessStudiosUserId;

	[TextArea]
	[SerializeField]
	private string ownerRoleConfirmationBodySource = "Are you <u>sure</u> you wish to give a role of <b>Owner</b> to <u>{0}</u> asset{1}?\nYou will not be able to change that!";

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private SerializableGuid assetId;

	private UIUserRolesModalModel.RoleValue assetRole;

	private AssetTypes assetType;

	private bool everyRoleValueIsNone;

	private Flows flow;

	private Roles localClientRole;

	private SerializableGuid prefabAssetId;

	private UIUserRolesModalModel.RoleValue prefabRole;

	private SerializableGuid scriptAssetId;

	private UIUserRolesModalModel.RoleValue scriptRole;

	private States state;

	private Roles targetAssetRole = Roles.None;

	private User targetUser;

	private HashSet<int> userIdsToOmitFromSelection;

	private UIUserRolesModel userRolesModel;

	private List<User> usersToDisplay = new List<User>();

	private UIBaseWindowView userSelectionWindow;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public void InitializeAddUserFlow(UIUserRolesModel userRolesModel, List<User> usersToDisplay)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "InitializeAddUserFlow", userRolesModel, usersToDisplay.DebugSafeCount());
		}
		targetAssetRole = Roles.None;
		userIdsToOmitFromSelection = new HashSet<int>();
		this.usersToDisplay = usersToDisplay;
		Initialize(userRolesModel);
		flow = Flows.Add;
		SetState(States.UserSelection);
	}

	public void InitializeChangeUserFlow(UIUserRolesModel userRolesModel, UserRole userRoleForAsset, string userName)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "InitializeChangeUserFlow", userRolesModel, userRoleForAsset, userName);
		}
		localClientRole = userRolesModel.LocalClientRole;
		userIdsToOmitFromSelection = new HashSet<int>(userRolesModel.UserIds);
		usersToDisplay = null;
		targetUser = new User(userRoleForAsset.UserId, null, userName);
		targetAssetRole = userRoleForAsset.Role;
		Initialize(userRolesModel);
		flow = Flows.Change;
		SetState(States.RoleSelection);
	}

	public void Cancel()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Cancel");
		}
		switch (state)
		{
		case States.RoleSelection:
			UIUserRolesModalController.ConfirmationAction = (Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>)Delegate.Remove(UIUserRolesModalController.ConfirmationAction, new Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>(FinalizeRoleApplications));
			break;
		default:
			DebugUtility.LogNoEnumSupportError(this, "Cancel", state);
			break;
		case States.Uninitialized:
		case States.UserSelection:
			break;
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		SetState(States.Uninitialized);
	}

	private void Initialize(UIUserRolesModel userRolesModel)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", userRolesModel);
		}
		assetId = userRolesModel.AssetId;
		assetType = userRolesModel.AssetType;
		this.userRolesModel = userRolesModel;
		localClientRole = userRolesModel.LocalClientRole;
		userIdsToOmitFromSelection = new HashSet<int>(userRolesModel.UserIds);
		UIModalManager.OnModalClosedByUser = (Action<UIBaseModalView>)Delegate.Combine(UIModalManager.OnModalClosedByUser, new Action<UIBaseModalView>(OnModalClosedByUser));
	}

	private void SetState(States state)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetState", state);
		}
		if (this.state == state)
		{
			DebugUtility.LogError(this, "SetState", "UIUserRolesModel is already in that state!", state);
			return;
		}
		this.state = state;
		switch (state)
		{
		case States.Uninitialized:
			UIModalManager.OnModalClosedByUser = (Action<UIBaseModalView>)Delegate.Remove(UIModalManager.OnModalClosedByUser, new Action<UIBaseModalView>(OnModalClosedByUser));
			break;
		case States.UserSelection:
		{
			if (assetType == AssetTypes.Level)
			{
				DisplayLevelRoleUserSelectionWindowAsync();
				break;
			}
			User item = new User(EndlessServices.Instance.CloudService.ActiveUserId, null, EndlessServices.Instance.CloudService.ActiveUserName);
			List<User> list = new List<User> { item };
			if (!Application.isEditor && !Debug.isDebugBuild)
			{
				list.Add(endlessStudiosUserId.User);
			}
			list.AddRange(userRolesModel.Users);
			UIUserSearchWindowModel model = new UIUserSearchWindowModel("Give Role", list, SelectionType.MustSelect1, SetTargetUserAndProceedStateToRoleSelection);
			if ((bool)userSelectionWindow)
			{
				ClearUserSearchWindowReference();
			}
			userSelectionWindow = UIUserSearchWindowView.Display(model);
			userSelectionWindow.CloseUnityEvent.AddListener(ClearUserSearchWindowReference);
			break;
		}
		case States.RoleSelection:
			UIUserRolesModalController.ConfirmationAction = (Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>)Delegate.Combine(UIUserRolesModalController.ConfirmationAction, new Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>(FinalizeRoleApplications));
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(userRolesModalSource, UIModalManagerStackActions.ClearStack, targetUser, localClientRole, assetId, assetType, targetAssetRole, userRolesModel.AssetName, userRolesModel.AssetContext, Roles.None);
			break;
		default:
			DebugUtility.LogNoEnumSupportError(this, "SetState", state, state);
			break;
		}
	}

	private async void DisplayLevelRoleUserSelectionWindowAsync()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayLevelRoleUserSelectionWindowAsync");
		}
		List<object> items = await GetUserWithGameRoleButNoLevelRoleAsync();
		UIIEnumerableWindowModel model = new UIIEnumerableWindowModel(-1, "Give Role", UIBaseIEnumerableView.ArrangementStyle.StraightVerticalVirtualized, SelectionType.MustSelect1, items, null, new List<object>(), null, SetTargetUserAndProceedStateToRoleSelection);
		if ((bool)userSelectionWindow)
		{
			ClearUserSearchWindowReference();
		}
		userSelectionWindow = UIIEnumerableWindowView.Display(model);
		userSelectionWindow.CloseUnityEvent.AddListener(ClearUserSearchWindowReference);
	}

	private async Task<List<object>> GetUserWithGameRoleButNoLevelRoleAsync()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetUserWithGameRoleButNoLevelRoleAsync");
		}
		GetAllRolesResult getAllRolesResult = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid);
		List<object> userWithGameRoleButNoLevelRole = new List<object>();
		foreach (UserRole userRole in getAllRolesResult.Roles)
		{
			if (userRole.InheritedFromParent)
			{
				string userName = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(userRole.UserId);
				User item = new User(userRole.UserId, null, userName);
				userWithGameRoleButNoLevelRole.Add(item);
			}
		}
		if (verboseLogging)
		{
			DebugUtility.DebugEnumerable("userWithGameRoleButNoLevelRole", userWithGameRoleButNoLevelRole, this);
		}
		return userWithGameRoleButNoLevelRole;
	}

	private void OnModalClosedByUser(UIBaseModalView modal)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnModalClosedByUser", modal);
		}
		switch (state)
		{
		case States.UserSelection:
			SetState(state - 1);
			break;
		case States.RoleSelection:
			UIUserRolesModalController.ConfirmationAction = (Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>)Delegate.Remove(UIUserRolesModalController.ConfirmationAction, new Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>(FinalizeRoleApplications));
			SetState((flow == Flows.Add) ? (state - 1) : States.Uninitialized);
			break;
		default:
			DebugUtility.LogNoEnumSupportError(this, "OnModalClosedByUser", state, modal);
			break;
		case States.Uninitialized:
			break;
		}
	}

	private void SetTargetUserAndProceedStateToRoleSelection(List<object> target)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetTargetUserAndProceedStateToRoleSelection", target.Count);
		}
		if (target.Count != 0)
		{
			User user = target[0] as User;
			targetUser = user;
			SetState(States.RoleSelection);
		}
	}

	private void FinalizeRoleApplications(bool everyRoleValueIsNone, UIUserRolesModalModel.RoleValue assetRole, UIUserRolesModalModel.RoleValue scriptRole, SerializableGuid scriptAssetId, UIUserRolesModalModel.RoleValue prefabRole, SerializableGuid prefabAssetId, IReadOnlyCollection<UIUserRolesModalModel.RoleValue> roles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "FinalizeRoleApplications", everyRoleValueIsNone, assetRole, scriptRole, scriptAssetId, prefabRole, prefabAssetId, roles.Count);
		}
		UIUserRolesModalController.ConfirmationAction = (Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>)Delegate.Remove(UIUserRolesModalController.ConfirmationAction, new Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>(FinalizeRoleApplications));
		this.everyRoleValueIsNone = everyRoleValueIsNone;
		this.assetRole = assetRole;
		this.scriptRole = scriptRole;
		this.scriptAssetId = scriptAssetId;
		this.prefabRole = prefabRole;
		this.prefabAssetId = prefabAssetId;
		int num = roles.Count((UIUserRolesModalModel.RoleValue role) => role.UserChanged && role.Value == Roles.Owner);
		if (num > 0)
		{
			string format = ownerRoleConfirmationBodySource;
			format = string.Format(format, num, (num > 1) ? "s" : string.Empty);
			MonoBehaviourSingleton<UIModalManager>.Instance.Confirm(format, OnConfirmationOfSetOwnerRoles, MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack);
		}
		else
		{
			ApplyRoles();
		}
	}

	private void OnConfirmationOfSetOwnerRoles()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnConfirmationOfSetOwnerRoles");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		ApplyRoles();
	}

	private async void ApplyRoles()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyRoles");
		}
		OnLoadingStarted.Invoke();
		bool isIrrelevantAddition = flow == Flows.Add && everyRoleValueIsNone;
		if (!isIrrelevantAddition && userRolesModel.AssetContext != AssetContexts.NewGame)
		{
			try
			{
				if (assetRole.UserChanged)
				{
					await ApplyRole(assetRole, assetId);
				}
			}
			catch (Exception exception)
			{
				DebugUtility.LogException(exception, this);
			}
			if (assetType == AssetTypes.Prop)
			{
				try
				{
					if (scriptRole.UserChanged)
					{
						await ApplyRole(scriptRole, scriptAssetId);
					}
				}
				catch (Exception exception2)
				{
					DebugUtility.LogException(exception2, this);
				}
				try
				{
					if (prefabRole.UserChanged)
					{
						await ApplyRole(prefabRole, prefabAssetId);
					}
				}
				catch (Exception exception3)
				{
					DebugUtility.LogException(exception3, this);
				}
			}
		}
		if (userRolesModel.gameObject.activeInHierarchy && (!isIrrelevantAddition || userRolesModel.AssetContext == AssetContexts.NewGame))
		{
			UserRole userRole = new UserRole(targetUser.Id, assetRole.Value);
			switch (flow)
			{
			case Flows.Add:
				if (userRolesModel.AssetContext == AssetContexts.NewGame && userRole.Role != Roles.None)
				{
					userRolesModel.AddToEnd(userRole);
				}
				break;
			case Flows.Change:
				if (userRole.Role == Roles.None)
				{
					if (userRolesModel.AssetContext == AssetContexts.NewGame)
					{
						userRolesModel.RemoveUser(userRole);
					}
				}
				else
				{
					userRolesModel.UpdateRole(userRole);
				}
				break;
			default:
				DebugUtility.LogNoEnumSupportError(this, "ApplyRoles", flow);
				break;
			}
		}
		OnLoadingEnded.Invoke();
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		SetState(States.Uninitialized);
		OnComplete.Invoke();
	}

	private Task ApplyRole(UIUserRolesModalModel.RoleValue roleValue, SerializableGuid targetAssetId)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyRole", roleValue, targetAssetId);
		}
		return MonoBehaviourSingleton<RightsManager>.Instance.SetRightForUserOnAsset(targetAssetId, targetUser.Id, roleValue.Value);
	}

	private void ClearUserSearchWindowReference()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ClearUserSearchWindowReference");
		}
		if (state == States.UserSelection)
		{
			SetState(States.Uninitialized);
		}
		userSelectionWindow.CloseUnityEvent.RemoveListener(ClearUserSearchWindowReference);
		userSelectionWindow = null;
	}
}
