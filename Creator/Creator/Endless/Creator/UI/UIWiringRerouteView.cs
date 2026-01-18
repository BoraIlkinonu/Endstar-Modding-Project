using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002F3 RID: 755
	public class UIWiringRerouteView : UIGameObject
	{
		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x06000D0C RID: 3340 RVA: 0x0003E93C File Offset: 0x0003CB3C
		private bool ShouldDisplayToggle
		{
			get
			{
				if (!this.emitterInspector.IsOpen || !this.receiverInspector.IsOpen)
				{
					return false;
				}
				switch (this.wiringManager.WiringState)
				{
				case UIWiringStates.Nothing:
					return false;
				case UIWiringStates.CreateNew:
					return this.wiringManager.WireCreatorController.CanCreateWire;
				case UIWiringStates.EditExisting:
					return this.wiringManager.WireEditorController.CanCreateWire;
				default:
					Debug.LogException(new Exception(string.Format("{0} does not have support for a {1} of {2}", "UIWiringRerouteView", "WiringState", this.wiringManager.WiringState)), this);
					return false;
				}
			}
		}

		// Token: 0x06000D0D RID: 3341 RVA: 0x0003E9DC File Offset: 0x0003CBDC
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.wiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
			this.wiringTool.OnToolStateChanged.AddListener(new UnityAction<WiringTool.WiringToolState>(this.OnWiringToolStateChanged));
			this.wiringManager = MonoBehaviourSingleton<UIWiringManager>.Instance;
			this.wireConfirmation = this.wiringManager.WireConfirmationModalView;
			this.wireEditor = this.wiringManager.WireEditorModal;
			this.emitterInspector = this.wiringManager.EmitterInspector;
			this.receiverInspector = this.wiringManager.ReceiverInspector;
			this.emitterInspector.OnDisplay.AddListener(new UnityAction(this.ToggleRerouteSwitchVisibility));
			this.receiverInspector.OnDisplay.AddListener(new UnityAction(this.ToggleRerouteSwitchVisibility));
			this.emitterInspector.OnHide.AddListener(new UnityAction(this.ToggleRerouteSwitchVisibility));
			this.receiverInspector.OnHide.AddListener(new UnityAction(this.ToggleRerouteSwitchVisibility));
			this.wireConfirmation.OnDisplay.AddListener(new UnityAction(this.DisplayRerouteSwitch));
			this.wireEditor.OnDisplay.AddListener(new UnityAction(this.DisplayRerouteSwitch));
			this.wiringTool.EnableRerouteUndo.AddListener(new UnityAction(this.EnableRerouteUndoButton));
			this.wiringTool.DisableRerouteUndo.AddListener(new UnityAction(this.DisableRerouteUndoButton));
			this.rerouteSwitchDisplayTweens.SetToStart();
		}

		// Token: 0x06000D0E RID: 3342 RVA: 0x0003EB67 File Offset: 0x0003CD67
		public void HideRerouteSwitch()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HideRerouteSwitch", Array.Empty<object>());
			}
			this.isOpen = false;
			this.rerouteSwitchHideTweens.Tween();
			this.DisableRerouteUndoButton();
		}

		// Token: 0x06000D0F RID: 3343 RVA: 0x0003EB9C File Offset: 0x0003CD9C
		private void ToggleRerouteSwitchVisibility()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleRerouteSwitchVisibility", Array.Empty<object>());
			}
			if (!this.isOpen && this.ShouldDisplayToggle)
			{
				this.DisplayRerouteSwitch();
				return;
			}
			if (this.isOpen && !this.ShouldDisplayToggle)
			{
				this.HideRerouteSwitch();
			}
		}

		// Token: 0x06000D10 RID: 3344 RVA: 0x0003EBF0 File Offset: 0x0003CDF0
		private void OnWiringToolStateChanged(WiringTool.WiringToolState wiringToolState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnWiringToolStateChanged", new object[] { wiringToolState });
			}
			bool flag = wiringToolState == WiringTool.WiringToolState.Rerouting;
			this.rerouteSwitchToggle.SetIsOn(flag, false, true);
		}

		// Token: 0x06000D11 RID: 3345 RVA: 0x0003EC34 File Offset: 0x0003CE34
		private void DisplayRerouteSwitch()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayRerouteSwitch", Array.Empty<object>());
			}
			if (this.rerouteSwitchHideTweens.IsAnyTweening())
			{
				this.rerouteSwitchHideTweens.Cancel();
			}
			this.isOpen = true;
			this.rerouteSwitchDisplayTweens.Tween();
		}

		// Token: 0x06000D12 RID: 3346 RVA: 0x0003EC83 File Offset: 0x0003CE83
		private void EnableRerouteUndoButton()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EnableRerouteUndoButton", Array.Empty<object>());
			}
			this.rerouteUndoButton.interactable = true;
		}

		// Token: 0x06000D13 RID: 3347 RVA: 0x0003ECA9 File Offset: 0x0003CEA9
		private void DisableRerouteUndoButton()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisableRerouteUndoButton", Array.Empty<object>());
			}
			this.rerouteUndoButton.interactable = false;
		}

		// Token: 0x04000B3C RID: 2876
		[SerializeField]
		private UIToggle rerouteSwitchToggle;

		// Token: 0x04000B3D RID: 2877
		[SerializeField]
		private TweenCollection rerouteSwitchDisplayTweens;

		// Token: 0x04000B3E RID: 2878
		[SerializeField]
		private TweenCollection rerouteSwitchHideTweens;

		// Token: 0x04000B3F RID: 2879
		[SerializeField]
		private UIButton rerouteUndoButton;

		// Token: 0x04000B40 RID: 2880
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000B41 RID: 2881
		private WiringTool wiringTool;

		// Token: 0x04000B42 RID: 2882
		private UIWiringManager wiringManager;

		// Token: 0x04000B43 RID: 2883
		private UIWireConfirmationModalView wireConfirmation;

		// Token: 0x04000B44 RID: 2884
		private UIWireEditorModalView wireEditor;

		// Token: 0x04000B45 RID: 2885
		private UIWiringObjectInspectorView emitterInspector;

		// Token: 0x04000B46 RID: 2886
		private UIWiringObjectInspectorView receiverInspector;

		// Token: 0x04000B47 RID: 2887
		private bool isOpen;
	}
}
