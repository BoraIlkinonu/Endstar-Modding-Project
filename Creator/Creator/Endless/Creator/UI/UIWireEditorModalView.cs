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
	// Token: 0x0200030A RID: 778
	public class UIWireEditorModalView : UIGameObject, IBackable, IValidatable
	{
		// Token: 0x170001EB RID: 491
		// (get) Token: 0x06000DCB RID: 3531 RVA: 0x00041BF8 File Offset: 0x0003FDF8
		// (set) Token: 0x06000DCC RID: 3532 RVA: 0x00041C00 File Offset: 0x0003FE00
		public bool IsOpen { get; private set; }

		// Token: 0x170001EC RID: 492
		// (get) Token: 0x06000DCD RID: 3533 RVA: 0x00041C09 File Offset: 0x0003FE09
		// (set) Token: 0x06000DCE RID: 3534 RVA: 0x00041C11 File Offset: 0x0003FE11
		public UIWireView Wire { get; private set; }

		// Token: 0x06000DCF RID: 3535 RVA: 0x00041C1C File Offset: 0x0003FE1C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.hideTweens.OnAllTweenCompleted.AddListener(new UnityAction(this.SetInactive));
			this.hideTweens.SetToEnd();
			base.gameObject.SetActive(false);
			MonoBehaviourSingleton<UIWiringManager>.Instance.OnObjectSelected.AddListener(new UnityAction(this.OnWireObjectSelected));
		}

		// Token: 0x06000DD0 RID: 3536 RVA: 0x00041C8F File Offset: 0x0003FE8F
		public void OnBack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			this.Hide();
		}

		// Token: 0x06000DD1 RID: 3537 RVA: 0x00041CAF File Offset: 0x0003FEAF
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			this.hideTweens.ValidateForNumberOfTweens(1);
		}

		// Token: 0x06000DD2 RID: 3538 RVA: 0x00041CD8 File Offset: 0x0003FED8
		public void InspectWire(UIWireView wire)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "InspectWire", new object[] { wire.WireId });
				DebugUtility.Log(string.Format("Emitter Param Count: {0}, Receiver Param Count: {1}", wire.EmitterNode.NodeEvent.ParamList.Count, wire.ReceiverNode.NodeEvent.ParamList.Count), this);
				foreach (EndlessParameterInfo endlessParameterInfo in wire.ReceiverNode.NodeEvent.ParamList)
				{
					DebugUtility.Log(string.Format("DisplayName: {0}, DataType: {1}", endlessParameterInfo.DisplayName, endlessParameterInfo.DataType), this);
				}
			}
			this.Wire = wire;
			this.emitterNameText.text = wire.EmitterNode.NodeEvent.MemberName;
			this.receiverNameText.text = wire.ReceiverNode.NodeEvent.MemberName;
			bool flag = UIWireUtility.CanOverrideEmitterContextualValue(wire.EmitterNode.NodeEvent.ParamList, wire.ReceiverNode.NodeEvent.ParamList);
			this.overrideEmitterContextualValueContainer.gameObject.SetActive(flag);
			bool flag2 = WiringUtilities.GetWireEntry(wire.EmitterNode.InspectedObjectId, wire.EmitterNode.MemberName, wire.ReceiverNode.InspectedObjectId, wire.ReceiverNode.MemberName).StaticParameters.Length != 0;
			this.overrideEmitterContextualValueToggle.SetIsOn(flag2, true, true);
			this.wiringPropertyModifier.DisplayExistingWire(wire);
			this.SetWiringPropertyModifierVisibility(flag2);
			this.Display();
			this.RequestLayout();
		}

		// Token: 0x06000DD3 RID: 3539 RVA: 0x00041E9C File Offset: 0x0004009C
		public void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			if (this.hideTweens.IsAnyTweening())
			{
				this.hideTweens.Cancel();
			}
			this.IsOpen = true;
			this.displayTweens.Tween();
			base.gameObject.SetActive(true);
			this.OnDisplay.Invoke();
			if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			}
		}

		// Token: 0x06000DD4 RID: 3540 RVA: 0x00041F1C File Offset: 0x0004011C
		public void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			if (this.displayTweens.IsAnyTweening())
			{
				this.displayTweens.Cancel();
			}
			this.IsOpen = false;
			this.hideTweens.Tween();
			this.OnHide.Invoke();
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}

		// Token: 0x06000DD5 RID: 3541 RVA: 0x00041F81 File Offset: 0x00040181
		public void SetWiringPropertyModifierVisibility(bool visible)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetWiringPropertyModifierVisibility", new object[] { visible });
			}
			this.wiringPropertyModifier.gameObject.SetActive(visible);
		}

		// Token: 0x06000DD6 RID: 3542 RVA: 0x00041FB6 File Offset: 0x000401B6
		private void OnWireObjectSelected()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnWireObjectSelected", Array.Empty<object>());
			}
			if (!this.IsOpen)
			{
				return;
			}
			this.Hide();
		}

		// Token: 0x06000DD7 RID: 3543 RVA: 0x00041FDF File Offset: 0x000401DF
		private void RequestLayout()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RequestLayout", Array.Empty<object>());
			}
			this.layoutables.CollectLayoutGroupChildren();
			this.layoutables.RequestLayout();
		}

		// Token: 0x06000DD8 RID: 3544 RVA: 0x0004200F File Offset: 0x0004020F
		private void SetInactive()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInactive", Array.Empty<object>());
			}
			base.gameObject.SetActive(false);
			this.wiringPropertyModifier.Clean();
		}

		// Token: 0x04000BC6 RID: 3014
		[HideInInspector]
		public UnityEvent OnDisplay = new UnityEvent();

		// Token: 0x04000BC7 RID: 3015
		[HideInInspector]
		public UnityEvent OnHide = new UnityEvent();

		// Token: 0x04000BC8 RID: 3016
		[SerializeField]
		private TextMeshProUGUI emitterNameText;

		// Token: 0x04000BC9 RID: 3017
		[SerializeField]
		private TextMeshProUGUI receiverNameText;

		// Token: 0x04000BCA RID: 3018
		[SerializeField]
		private GameObject overrideEmitterContextualValueContainer;

		// Token: 0x04000BCB RID: 3019
		[SerializeField]
		private UIToggle overrideEmitterContextualValueToggle;

		// Token: 0x04000BCC RID: 3020
		[SerializeField]
		private UIWirePropertyModifierView wiringPropertyModifier;

		// Token: 0x04000BCD RID: 3021
		[SerializeField]
		private TweenCollection displayTweens;

		// Token: 0x04000BCE RID: 3022
		[SerializeField]
		private TweenCollection hideTweens;

		// Token: 0x04000BCF RID: 3023
		[SerializeField]
		private InterfaceReference<IUIChildLayoutable>[] layoutables = Array.Empty<InterfaceReference<IUIChildLayoutable>>();

		// Token: 0x04000BD0 RID: 3024
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
