using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIRoleInteractabilityHandler : UIGameObject
{
	private enum Sources
	{
		ActiveGame,
		ActiveLevel
	}

	[SerializeField]
	private InterfaceReference<IRoleInteractable> interfaceTarget;

	[SerializeField]
	private Sources source;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private SerializableGuid subscribedAssetId;

	private SerializableGuid ancestorId;

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		if (!interfaceTarget)
		{
			base.enabled = false;
		}
		else if (!MatchmakingClientController.Instance.ActiveGameId.IsEmpty)
		{
			switch (source)
			{
			case Sources.ActiveGame:
				subscribedAssetId = MatchmakingClientController.Instance.ActiveGameId;
				ancestorId = SerializableGuid.Empty;
				break;
			case Sources.ActiveLevel:
				subscribedAssetId = MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid;
				ancestorId = MatchmakingClientController.Instance.ActiveGameId;
				break;
			default:
				DebugUtility.LogNoEnumSupportError(this, source);
				return;
			}
			MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(subscribedAssetId, ancestorId, OnUserRolesChanged);
		}
		else
		{
			subscribedAssetId = SerializableGuid.Empty;
			ancestorId = SerializableGuid.Empty;
			OnUserRolesChanged(Array.Empty<UserRole>());
		}
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		if (interfaceTarget != null && !subscribedAssetId.IsEmpty)
		{
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(subscribedAssetId, OnUserRolesChanged);
			subscribedAssetId = SerializableGuid.Empty;
		}
	}

	private void OnUserRolesChanged(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnUserRolesChanged", userRoles.Count);
		}
		Roles a;
		if (EndlessServices.Instance == null)
		{
			a = Roles.None;
		}
		else
		{
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			a = userRoles.GetRoleForUserId(activeUserId);
		}
		bool flag = a.IsGreaterThanOrEqualTo(Roles.Editor);
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "localUserCanInteract", flag), this);
		}
		interfaceTarget.Interface.SetLocalUserCanInteract(flag);
	}
}
