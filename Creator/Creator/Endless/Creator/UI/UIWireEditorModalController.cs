using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000309 RID: 777
	[RequireComponent(typeof(UIWireEditorModalView))]
	public class UIWireEditorModalController : UIGameObject
	{
		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x06000DC2 RID: 3522 RVA: 0x0004061F File Offset: 0x0003E81F
		private UIWiringManager WiringManager
		{
			get
			{
				return MonoBehaviourSingleton<UIWiringManager>.Instance;
			}
		}

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x06000DC3 RID: 3523 RVA: 0x00041976 File Offset: 0x0003FB76
		private UIWireView Wire
		{
			get
			{
				return this.wireEditorView.Wire;
			}
		}

		// Token: 0x06000DC4 RID: 3524 RVA: 0x00041984 File Offset: 0x0003FB84
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.TryGetComponent<UIWireEditorModalView>(out this.wireEditorView);
			this.wiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
			this.overrideEmitterContextualValueToggle.OnChange.AddListener(new UnityAction<bool>(this.SetOverrideEmitterContextualValue));
		}

		// Token: 0x06000DC5 RID: 3525 RVA: 0x000419E2 File Offset: 0x0003FBE2
		public void Cancel()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Cancel", Array.Empty<object>());
			}
			this.wiringReroute.HideRerouteSwitch();
			this.WiringManager.WireEditorController.Restart(true);
		}

		// Token: 0x06000DC6 RID: 3526 RVA: 0x00041A18 File Offset: 0x0003FC18
		public void EditEmitter()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EditEmitter", Array.Empty<object>());
			}
			this.WiringManager.WireEditorController.SetEmitterEventIndex(-1);
			this.wireEditorView.Hide();
		}

		// Token: 0x06000DC7 RID: 3527 RVA: 0x00041A4E File Offset: 0x0003FC4E
		public void EditReceiver()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EditReceiver", Array.Empty<object>());
			}
			this.WiringManager.WireEditorController.SetReceiverEventIndex(-1);
			this.wireEditorView.Hide();
		}

		// Token: 0x06000DC8 RID: 3528 RVA: 0x00041A84 File Offset: 0x0003FC84
		public void Delete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Delete", Array.Empty<object>());
			}
			this.WiringManager.WireEditorController.DeleteWire(this.Wire);
			this.WiringManager.WireEditorController.Restart(true);
		}

		// Token: 0x06000DC9 RID: 3529 RVA: 0x00041AD0 File Offset: 0x0003FCD0
		private void SetOverrideEmitterContextualValue(bool overrideEmitterContextualValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOverrideEmitterContextualValue", new object[] { overrideEmitterContextualValue });
			}
			WireEntry wireEntry = WiringUtilities.GetWireEntry(this.Wire.EmitterNode.InspectedObjectId, this.Wire.EmitterNode.MemberName, this.Wire.ReceiverNode.InspectedObjectId, this.Wire.ReceiverNode.MemberName);
			string[] array = (this.overrideEmitterContextualValueToggle.IsOn ? this.wiringPropertyModifier.StoredParameterValues : Array.Empty<string>());
			this.wiringTool.UpdateWire(this.Wire.WireId, this.Wire.EmitterNode.InspectedObjectId, this.Wire.EmitterNode.MemberName, this.Wire.EmitterNode.AssemblyQualifiedTypeName, this.Wire.ReceiverNode.InspectedObjectId, this.Wire.ReceiverNode.MemberName, this.Wire.ReceiverNode.AssemblyQualifiedTypeName, array, Array.Empty<SerializableGuid>(), (WireColor)wireEntry.WireColor);
			this.wireEditorView.SetWiringPropertyModifierVisibility(overrideEmitterContextualValue);
		}

		// Token: 0x04000BC0 RID: 3008
		[SerializeField]
		private UIToggle overrideEmitterContextualValueToggle;

		// Token: 0x04000BC1 RID: 3009
		[SerializeField]
		private UIWirePropertyModifierView wiringPropertyModifier;

		// Token: 0x04000BC2 RID: 3010
		[SerializeField]
		private UIWiringRerouteView wiringReroute;

		// Token: 0x04000BC3 RID: 3011
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000BC4 RID: 3012
		private UIWireEditorModalView wireEditorView;

		// Token: 0x04000BC5 RID: 3013
		private WiringTool wiringTool;
	}
}
