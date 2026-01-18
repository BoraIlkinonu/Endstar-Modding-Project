using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Creator.UI
{
	// Token: 0x020002F2 RID: 754
	public class UIWiringRerouteController : UIGameObject, IValidatable
	{
		// Token: 0x06000D06 RID: 3334 RVA: 0x0003E704 File Offset: 0x0003C904
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.rerouteSwitchToggle.OnChange.AddListener(new UnityAction<bool>(this.ToggleRerouting));
			this.wiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
			this.wiringManager = MonoBehaviourSingleton<UIWiringManager>.Instance;
			this.wireConfirmationModalView = this.wiringManager.WireConfirmationModalView;
			this.wireCreatorController = this.wiringManager.WireCreatorController;
			this.wireEditorModal = this.wiringManager.WireEditorModal;
			this.wireEditorController = this.wiringManager.WireEditorController;
			this.rerouteSwitchHideTweens.OnAllTweenCompleted.AddListener(new UnityAction(this.ToggleOff));
			this.rerouteUndoButton.onClick.AddListener(new UnityAction(this.UndoReroute));
		}

		// Token: 0x06000D07 RID: 3335 RVA: 0x0003E7DC File Offset: 0x0003C9DC
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			this.rerouteSwitchHideTweens.ValidateForNumberOfTweens(1);
		}

		// Token: 0x06000D08 RID: 3336 RVA: 0x0003E804 File Offset: 0x0003CA04
		private void ToggleRerouting(bool state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleRerouting", new object[] { state });
			}
			if (state)
			{
				this.wiringTool.SetToolState(WiringTool.WiringToolState.Rerouting);
			}
			else
			{
				this.wiringTool.SetToolState(WiringTool.WiringToolState.Wiring);
			}
			switch (this.wiringManager.WiringState)
			{
			case UIWiringStates.Nothing:
				break;
			case UIWiringStates.CreateNew:
				if (state)
				{
					this.wireConfirmationModalView.Hide();
					return;
				}
				this.wireCreatorController.DisplayWireConfirmation();
				return;
			case UIWiringStates.EditExisting:
				if (state)
				{
					this.wireEditorModal.Hide();
					return;
				}
				this.wireEditorModal.Display();
				return;
			default:
				DebugUtility.LogError(this, "ToggleRerouting", string.Format("No support for a {0} of {1}", "WiringState", this.wiringManager.WiringState), new object[] { state });
				break;
			}
		}

		// Token: 0x06000D09 RID: 3337 RVA: 0x0003E8DF File Offset: 0x0003CADF
		private void ToggleOff()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleOff", Array.Empty<object>());
			}
			this.ToggleRerouting(false);
		}

		// Token: 0x06000D0A RID: 3338 RVA: 0x0003E900 File Offset: 0x0003CB00
		private void UndoReroute()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UndoReroute", Array.Empty<object>());
			}
			this.wiringTool.RequestPopLastRerouteNode(default(InputAction.CallbackContext));
		}

		// Token: 0x04000B32 RID: 2866
		[SerializeField]
		private UIToggle rerouteSwitchToggle;

		// Token: 0x04000B33 RID: 2867
		[SerializeField]
		private TweenCollection rerouteSwitchHideTweens;

		// Token: 0x04000B34 RID: 2868
		[SerializeField]
		private UIButton rerouteUndoButton;

		// Token: 0x04000B35 RID: 2869
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000B36 RID: 2870
		private WiringTool wiringTool;

		// Token: 0x04000B37 RID: 2871
		private UIWiringManager wiringManager;

		// Token: 0x04000B38 RID: 2872
		private UIWireConfirmationModalView wireConfirmationModalView;

		// Token: 0x04000B39 RID: 2873
		private UIWireCreatorController wireCreatorController;

		// Token: 0x04000B3A RID: 2874
		private UIWireEditorModalView wireEditorModal;

		// Token: 0x04000B3B RID: 2875
		private UIWireEditorController wireEditorController;
	}
}
