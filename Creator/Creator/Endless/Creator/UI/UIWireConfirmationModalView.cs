using System;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000305 RID: 773
	public class UIWireConfirmationModalView : UIGameObject, IBackable, IValidatable
	{
		// Token: 0x170001DB RID: 475
		// (get) Token: 0x06000D8C RID: 3468 RVA: 0x00040BE2 File Offset: 0x0003EDE2
		// (set) Token: 0x06000D8D RID: 3469 RVA: 0x00040BEA File Offset: 0x0003EDEA
		public bool IsOpen { get; private set; }

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x06000D8E RID: 3470 RVA: 0x00040BF3 File Offset: 0x0003EDF3
		// (set) Token: 0x06000D8F RID: 3471 RVA: 0x00040BFB File Offset: 0x0003EDFB
		public UIWireView Wire { get; private set; }

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x06000D90 RID: 3472 RVA: 0x00040C04 File Offset: 0x0003EE04
		// (set) Token: 0x06000D91 RID: 3473 RVA: 0x00040C0C File Offset: 0x0003EE0C
		public bool CanOverrideEmitterContextualValue { get; private set; }

		// Token: 0x170001DE RID: 478
		// (get) Token: 0x06000D92 RID: 3474 RVA: 0x0004061F File Offset: 0x0003E81F
		private UIWiringManager WiringManager
		{
			get
			{
				return MonoBehaviourSingleton<UIWiringManager>.Instance;
			}
		}

		// Token: 0x06000D93 RID: 3475 RVA: 0x00040C18 File Offset: 0x0003EE18
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.hideTweens.OnAllTweenCompleted.AddListener(new UnityAction(this.ToggleOff));
			this.hideTweens.SetToEnd();
			base.gameObject.SetActive(false);
		}

		// Token: 0x06000D94 RID: 3476 RVA: 0x00040C70 File Offset: 0x0003EE70
		public void OnBack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			this.wiringReroute.HideRerouteSwitch();
			this.WiringManager.WireCreatorController.Restart(true);
		}

		// Token: 0x06000D95 RID: 3477 RVA: 0x00040CA6 File Offset: 0x0003EEA6
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			this.hideTweens.ValidateForNumberOfTweens(1);
		}

		// Token: 0x06000D96 RID: 3478 RVA: 0x00040CCC File Offset: 0x0003EECC
		public void Display(UIWireView wire, EndlessEventInfo receiverEndlessEventInfo)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", new object[]
				{
					wire.WireId,
					receiverEndlessEventInfo.ParamList.Count
				});
				DebugUtility.Log(string.Format("Emitter Param Count: {0}, Receiver Param Count: {1}", wire.EmitterNode.NodeEvent.ParamList.Count, wire.ReceiverNode.NodeEvent.ParamList.Count), this);
				foreach (EndlessParameterInfo endlessParameterInfo in wire.ReceiverNode.NodeEvent.ParamList)
				{
					DebugUtility.Log(string.Format("DisplayName: {0}, DataType: {1}", endlessParameterInfo.DisplayName, endlessParameterInfo.DataType), this);
				}
			}
			this.Wire = wire;
			this.IsOpen = true;
			this.emitterNameText.text = wire.EmitterNode.MemberName;
			this.receiverNameText.text = wire.ReceiverNode.MemberName;
			this.wiringPropertyModifier.DisplayDefaultParameters(receiverEndlessEventInfo, null);
			this.CanOverrideEmitterContextualValue = UIWireUtility.CanOverrideEmitterContextualValue(wire.EmitterNode.NodeEvent.ParamList, wire.ReceiverNode.NodeEvent.ParamList);
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "CanOverrideEmitterContextualValue", this.CanOverrideEmitterContextualValue), this);
			}
			this.overrideEmitterContextualValueContainer.SetActive(this.CanOverrideEmitterContextualValue);
			this.overrideEmitterContextualValueToggle.SetIsOn(false, true, true);
			if (this.CanOverrideEmitterContextualValue)
			{
				this.SetWiringPropertyModifierVisibility(false);
			}
			this.displayTweens.Tween();
			base.gameObject.SetActive(true);
			this.OnDisplay.Invoke();
			if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			}
			this.RequestLayout();
		}

		// Token: 0x06000D97 RID: 3479 RVA: 0x00040ECC File Offset: 0x0003F0CC
		public void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.IsOpen = false;
			this.hideTweens.Tween();
			this.WiringManager.WiresView.ToggleDarkMode(false, this.WiringManager.WireEditorController.WireToEdit);
			this.OnHide.Invoke();
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}

		// Token: 0x06000D98 RID: 3480 RVA: 0x00040F3A File Offset: 0x0003F13A
		public void SetWiringPropertyModifierVisibility(bool visible)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetWiringPropertyModifierVisibility", new object[] { visible });
			}
			this.wiringPropertyModifier.gameObject.SetActive(visible);
		}

		// Token: 0x06000D99 RID: 3481 RVA: 0x00040F6F File Offset: 0x0003F16F
		private void RequestLayout()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RequestLayout", Array.Empty<object>());
			}
			this.layoutables.CollectLayoutGroupChildren();
			this.layoutables.RequestLayout();
		}

		// Token: 0x06000D9A RID: 3482 RVA: 0x00040F9F File Offset: 0x0003F19F
		private void ToggleOff()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleOff", Array.Empty<object>());
			}
			this.colorDropdown.Clear();
			base.gameObject.SetActive(false);
			this.wiringPropertyModifier.Clean();
		}

		// Token: 0x04000BA9 RID: 2985
		public UnityEvent OnDisplay = new UnityEvent();

		// Token: 0x04000BAA RID: 2986
		public UnityEvent OnHide = new UnityEvent();

		// Token: 0x04000BAB RID: 2987
		[SerializeField]
		private TextMeshProUGUI emitterNameText;

		// Token: 0x04000BAC RID: 2988
		[SerializeField]
		private TextMeshProUGUI receiverNameText;

		// Token: 0x04000BAD RID: 2989
		[SerializeField]
		private GameObject overrideEmitterContextualValueContainer;

		// Token: 0x04000BAE RID: 2990
		[SerializeField]
		private UIToggle overrideEmitterContextualValueToggle;

		// Token: 0x04000BAF RID: 2991
		[SerializeField]
		private UIWirePropertyModifierView wiringPropertyModifier;

		// Token: 0x04000BB0 RID: 2992
		[SerializeField]
		private UIWireColorDropdown colorDropdown;

		// Token: 0x04000BB1 RID: 2993
		[SerializeField]
		private TweenCollection displayTweens;

		// Token: 0x04000BB2 RID: 2994
		[SerializeField]
		private TweenCollection hideTweens;

		// Token: 0x04000BB3 RID: 2995
		[SerializeField]
		private UIWiringRerouteView wiringReroute;

		// Token: 0x04000BB4 RID: 2996
		[SerializeField]
		private InterfaceReference<IUIChildLayoutable>[] layoutables = Array.Empty<InterfaceReference<IUIChildLayoutable>>();

		// Token: 0x04000BB5 RID: 2997
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
