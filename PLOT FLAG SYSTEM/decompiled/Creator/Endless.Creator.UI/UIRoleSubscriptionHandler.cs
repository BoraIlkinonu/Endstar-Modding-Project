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

public class UIRoleSubscriptionHandler : UIGameObject
{
	private enum Sources
	{
		ActiveGame,
		ActiveLevel
	}

	[SerializeField]
	private InterfaceReference<IUIRoleSubscribable> roleSubscribable;

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
		if (!MatchmakingClientController.Instance.ActiveGameId.IsEmpty)
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
			MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(subscribedAssetId, ancestorId, OnLevelUserRolesChanged);
		}
		else
		{
			subscribedAssetId = SerializableGuid.Empty;
			ancestorId = SerializableGuid.Empty;
			OnLevelUserRolesChanged(Array.Empty<UserRole>());
		}
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		if (!subscribedAssetId.IsEmpty)
		{
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(subscribedAssetId, OnLevelUserRolesChanged);
			subscribedAssetId = SerializableGuid.Empty;
		}
	}

	private void OnLevelUserRolesChanged(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnLevelUserRolesChanged", userRoles.Count);
		}
		Roles localClientRole;
		if (EndlessServices.Instance == null)
		{
			localClientRole = Roles.None;
		}
		else
		{
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			localClientRole = userRoles.GetRoleForUserId(activeUserId);
		}
		roleSubscribable.Interface.OnLocalClientRoleChanged(localClientRole);
	}
}
