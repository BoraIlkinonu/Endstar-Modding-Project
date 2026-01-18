using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Assets;
using Endless.Gameplay.RightsManagement;
using Endless.Shared;
using Endless.Shared.Debugging;
using Runtime.Shared.Matchmaking;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public abstract class UIAssetWithRolesModelHandler<T> : UIAssetModelHandler<T> where T : Asset
{
	public UnityEvent<Roles> OnLocalClientRoleSet = new UnityEvent<Roles>();

	public UnityEvent<IEnumerable<UserRole>> OnUserRolesSet = new UnityEvent<IEnumerable<UserRole>>();

	public Roles LocalClientRole { get; private set; } = Roles.None;

	public IEnumerable<UserRole> UserRoles { get; private set; }

	public override void Clear()
	{
		base.Clear();
		if (base.Model != null)
		{
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(base.Model.AssetID, AssetRightsUpdated);
		}
		SetLocalClientRole(Roles.None);
		SetUserRoles(Array.Empty<UserRole>());
	}

	public void GetAllUsersWithRolesForAsset()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("GetAllUsersWithRolesForAsset", this);
		}
		base.OnLoadingStarted.Invoke();
		MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(base.Model.AssetID, AssetRightsUpdated);
	}

	private void AssetRightsUpdated(IReadOnlyList<UserRole> userRoles)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "AssetRightsUpdated", "userRoles", userRoles.Count), this);
		}
		int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
		UserRole userRole = userRoles.FirstOrDefault((UserRole role) => role.UserId == activeUserId);
		base.OnLoadingEnded.Invoke();
		if (userRole != null)
		{
			LocalClientRole = userRole.Role;
		}
		else
		{
			LocalClientRole = Roles.None;
		}
		OnLocalClientRoleSet.Invoke(LocalClientRole);
	}

	private void SetLocalClientRole(Roles newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetLocalClientRole", "newValue", newValue), this);
		}
		LocalClientRole = newValue;
		OnLocalClientRoleSet.Invoke(LocalClientRole);
	}

	private void SetUserRoles(IEnumerable<UserRole> newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetUserRoles", "newValue", newValue), this);
		}
		UserRoles = newValue;
		OnUserRolesSet.Invoke(UserRoles);
	}
}
