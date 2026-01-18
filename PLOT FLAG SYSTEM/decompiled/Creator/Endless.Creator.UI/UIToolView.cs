using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIToolView : UIGameObject
{
	[SerializeField]
	private ToolType toolType;

	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	[SerializeField]
	private Button setActiveToolButton;

	[SerializeField]
	private UIImageSpriteStateHandler imageSpriteStateHandler;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image[] imagesToColor = Array.Empty<Image>();

	[SerializeField]
	private UIToolTypeColorDictionary toolTypeColorDictionary;

	[SerializeField]
	private TextMeshProUGUI hotKeyText;

	[Header("Tweens")]
	[SerializeField]
	private TweenCollection interactableTweens;

	[SerializeField]
	private TweenCollection uninteractableTweens;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private EndlessTool tool;

	private SerializableGuid subscribedAssetId;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		tool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(toolType);
		int toolHotKey = MonoBehaviourSingleton<ToolManager>.Instance.GetToolHotKey(toolType);
		displayAndHideHandler.AddDisplayDelay(0.25f + (float)toolHotKey * 0.1f);
		iconImage.sprite = tool.Icon;
		Color color = toolTypeColorDictionary[tool.ToolType];
		Image[] array = imagesToColor;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = color;
		}
		hotKeyText.text = (toolHotKey + 1).ToString();
		MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(OnToolChange);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(OnEnteringCreator);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(OnLeavingCreator);
		displayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
		OnToolChange(MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool);
	}

	private void OnToolChange(EndlessTool activeTool)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnToolChange", activeTool.GetType().Name);
		}
		bool state = activeTool == tool;
		imageSpriteStateHandler.Set(state);
	}

	private void OnEnteringCreator()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnteringCreator");
		}
		subscribedAssetId = MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid;
		displayAndHideHandler.Display();
		MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(subscribedAssetId, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID, ClientRolesChanged);
	}

	private void OnLeavingCreator()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnLeavingCreator");
		}
		displayAndHideHandler.Hide();
		MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(subscribedAssetId, ClientRolesChanged);
	}

	private void ClientRolesChanged(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ClientRolesChanged", userRoles.Count);
		}
		int localUserId = EndlessServices.Instance.CloudService.ActiveUserId;
		UserRole userRole = userRoles.FirstOrDefault((UserRole role) => role.UserId == localUserId);
		if (userRole == null)
		{
			SetInteractable(interactable: false);
		}
		else
		{
			SetInteractable(userRole.Role.IsGreaterThanOrEqualTo(Roles.Editor));
		}
	}

	private void SetInteractable(bool interactable)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractable", interactable);
		}
		if (interactable)
		{
			interactableTweens.Tween();
		}
		else
		{
			uninteractableTweens.Tween();
		}
	}
}
