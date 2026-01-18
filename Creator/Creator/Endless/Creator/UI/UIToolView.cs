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
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000297 RID: 663
	public class UIToolView : UIGameObject
	{
		// Token: 0x06000AFA RID: 2810 RVA: 0x00033920 File Offset: 0x00031B20
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.tool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(this.toolType);
			int toolHotKey = MonoBehaviourSingleton<ToolManager>.Instance.GetToolHotKey(this.toolType);
			this.displayAndHideHandler.AddDisplayDelay(0.25f + (float)toolHotKey * 0.1f);
			this.iconImage.sprite = this.tool.Icon;
			Color color = this.toolTypeColorDictionary[this.tool.ToolType];
			Image[] array = this.imagesToColor;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = color;
			}
			this.hotKeyText.text = (toolHotKey + 1).ToString();
			MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(new UnityAction<EndlessTool>(this.OnToolChange));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(new UnityAction(this.OnEnteringCreator));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(new UnityAction(this.OnLeavingCreator));
			this.displayAndHideHandler.SetToHideEnd(true);
			this.OnToolChange(MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool);
		}

		// Token: 0x06000AFB RID: 2811 RVA: 0x00033A54 File Offset: 0x00031C54
		private void OnToolChange(EndlessTool activeTool)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnToolChange", new object[] { activeTool.GetType().Name });
			}
			bool flag = activeTool == this.tool;
			this.imageSpriteStateHandler.Set(flag);
		}

		// Token: 0x06000AFC RID: 2812 RVA: 0x00033AA4 File Offset: 0x00031CA4
		private void OnEnteringCreator()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnteringCreator", Array.Empty<object>());
			}
			this.subscribedAssetId = MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid;
			this.displayAndHideHandler.Display();
			MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(this.subscribedAssetId, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID, new Action<IReadOnlyList<UserRole>>(this.ClientRolesChanged));
		}

		// Token: 0x06000AFD RID: 2813 RVA: 0x00033B14 File Offset: 0x00031D14
		private void OnLeavingCreator()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLeavingCreator", Array.Empty<object>());
			}
			this.displayAndHideHandler.Hide();
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.subscribedAssetId, new Action<IReadOnlyList<UserRole>>(this.ClientRolesChanged));
		}

		// Token: 0x06000AFE RID: 2814 RVA: 0x00033B60 File Offset: 0x00031D60
		private void ClientRolesChanged(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClientRolesChanged", new object[] { userRoles.Count });
			}
			int localUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			UserRole userRole = userRoles.FirstOrDefault((UserRole role) => role.UserId == localUserId);
			if (userRole == null)
			{
				this.SetInteractable(false);
				return;
			}
			this.SetInteractable(userRole.Role.IsGreaterThanOrEqualTo(Roles.Editor));
		}

		// Token: 0x06000AFF RID: 2815 RVA: 0x00033BE0 File Offset: 0x00031DE0
		private void SetInteractable(bool interactable)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			if (interactable)
			{
				this.interactableTweens.Tween();
				return;
			}
			this.uninteractableTweens.Tween();
		}

		// Token: 0x04000949 RID: 2377
		[SerializeField]
		private ToolType toolType;

		// Token: 0x0400094A RID: 2378
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x0400094B RID: 2379
		[SerializeField]
		private Button setActiveToolButton;

		// Token: 0x0400094C RID: 2380
		[SerializeField]
		private UIImageSpriteStateHandler imageSpriteStateHandler;

		// Token: 0x0400094D RID: 2381
		[SerializeField]
		private Image iconImage;

		// Token: 0x0400094E RID: 2382
		[SerializeField]
		private Image[] imagesToColor = Array.Empty<Image>();

		// Token: 0x0400094F RID: 2383
		[SerializeField]
		private UIToolTypeColorDictionary toolTypeColorDictionary;

		// Token: 0x04000950 RID: 2384
		[SerializeField]
		private TextMeshProUGUI hotKeyText;

		// Token: 0x04000951 RID: 2385
		[Header("Tweens")]
		[SerializeField]
		private TweenCollection interactableTweens;

		// Token: 0x04000952 RID: 2386
		[SerializeField]
		private TweenCollection uninteractableTweens;

		// Token: 0x04000953 RID: 2387
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000954 RID: 2388
		private EndlessTool tool;

		// Token: 0x04000955 RID: 2389
		private SerializableGuid subscribedAssetId;
	}
}
