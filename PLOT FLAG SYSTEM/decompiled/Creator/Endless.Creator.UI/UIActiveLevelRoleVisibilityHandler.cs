using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIActiveLevelRoleVisibilityHandler : UIGameObject
{
	private enum EvaluateBys
	{
		Equals,
		IsGreaterThan,
		IsGreaterThanOrEqualTo,
		IsLessThan,
		IsLessThanOrEqualTo
	}

	[SerializeField]
	private Roles role = Roles.Viewer;

	[SerializeField]
	private EvaluateBys evaluateBy;

	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private SerializableGuid activeLevelAssetId;

	private bool subscribed;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		displayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(OnCreatorStarted);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(OnCreatorEnded);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnLeavingSession.AddListener(OnLeavingSession);
	}

	private async void OnCreatorStarted()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnCreatorStarted");
		}
		activeLevelAssetId = MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid;
		Subscribe();
		GetAllRolesResult getAllRolesResult = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(activeLevelAssetId, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID, OnLevelUserRolesChanged);
		if (getAllRolesResult.WasChanged)
		{
			OnLevelUserRolesChanged(getAllRolesResult.Roles);
		}
	}

	private void OnCreatorEnded()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnCreatorEnded");
		}
		Unsubscribe();
	}

	private void OnLeavingSession()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnLeavingSession");
		}
		try
		{
			Unsubscribe();
			displayAndHideHandler.Hide();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void Subscribe()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Subscribe");
		}
		if (subscribed)
		{
			Unsubscribe();
		}
		MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(activeLevelAssetId, OnLevelUserRolesChanged);
		subscribed = true;
	}

	private void Unsubscribe()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Unsubscribe");
		}
		MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(activeLevelAssetId, OnLevelUserRolesChanged);
		subscribed = false;
	}

	private void OnLevelUserRolesChanged(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnLevelUserRolesChanged", userRoles.Count);
		}
		int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
		Roles roleForUserId = userRoles.GetRoleForUserId(activeUserId);
		View(roleForUserId);
	}

	private void View(Roles activeLevelLocalClientRole)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View", activeLevelLocalClientRole);
		}
		bool flag;
		switch (evaluateBy)
		{
		case EvaluateBys.Equals:
			flag = activeLevelLocalClientRole == role;
			break;
		case EvaluateBys.IsGreaterThan:
			flag = activeLevelLocalClientRole.IsGreaterThan(role);
			break;
		case EvaluateBys.IsGreaterThanOrEqualTo:
			flag = activeLevelLocalClientRole.IsGreaterThanOrEqualTo(role);
			break;
		case EvaluateBys.IsLessThan:
			flag = activeLevelLocalClientRole.IsLessThan(role);
			break;
		case EvaluateBys.IsLessThanOrEqualTo:
			flag = activeLevelLocalClientRole.IsLessThanOrEqualTo(role);
			break;
		default:
			DebugUtility.LogNoEnumSupportError(this, "View", evaluateBy, activeLevelLocalClientRole);
			flag = false;
			break;
		}
		if (flag)
		{
			displayAndHideHandler.Display();
		}
		else
		{
			displayAndHideHandler.Hide();
		}
	}
}
