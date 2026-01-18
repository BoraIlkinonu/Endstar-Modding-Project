using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000308 RID: 776
	public class UIWireEditorController : UIBaseWireController
	{
		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x06000DB8 RID: 3512 RVA: 0x00041748 File Offset: 0x0003F948
		// (set) Token: 0x06000DB9 RID: 3513 RVA: 0x00041750 File Offset: 0x0003F950
		public UIWireView WireToEdit { get; private set; }

		// Token: 0x06000DBA RID: 3514 RVA: 0x0004175C File Offset: 0x0003F95C
		public void EditWire(SerializableGuid wireId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "EditWire", "wireId", wireId), this);
			}
			UIWireView wire = base.WiresView.GetWire(wireId);
			if (!wire)
			{
				return;
			}
			this.EditWire(wire);
		}

		// Token: 0x06000DBB RID: 3515 RVA: 0x000417B0 File Offset: 0x0003F9B0
		public void EditWire(UIWireView wire)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "EditWire", "wire", wire.WireId), this);
			}
			this.WireToEdit = wire;
			base.WiringTool.SetWireEditedByPlayer_ServerRpc(wire.WireId, default(ServerRpcParams));
			base.SetEmitterEventIndex(wire.EmitterNode.NodeIndex);
			base.SetReceiverEventIndex(wire.ReceiverNode.NodeIndex);
			if (base.WiringTool.ToolState == WiringTool.WiringToolState.Wiring)
			{
				this.wireEditorModal.InspectWire(wire);
			}
			base.WiresView.ToggleDarkMode(true, wire);
			base.WiringManager.SetWiringState(UIWiringStates.EditExisting);
		}

		// Token: 0x06000DBC RID: 3516 RVA: 0x00041860 File Offset: 0x0003FA60
		public void DeleteWire(UIWireView wire)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DeleteWire", new object[] { wire.WireId });
			}
			base.WiringTool.DeleteWire(wire.WireId);
			base.WiresView.DespawnWire(wire);
			if (this.wireEditorModal.IsOpen)
			{
				this.wireEditorModal.Hide();
			}
			base.WiresView.ToggleDarkMode(false, wire);
			base.WiringManager.SetWiringState(UIWiringStates.Nothing);
		}

		// Token: 0x06000DBD RID: 3517 RVA: 0x000418E2 File Offset: 0x0003FAE2
		public override void CreateWire(string[] storedParameterValues, WireColor wireColor)
		{
			base.CreateWire(storedParameterValues, wireColor);
			if (this.WireToEdit)
			{
				this.DeleteWire(this.WireToEdit);
				this.WireToEdit = null;
			}
		}

		// Token: 0x06000DBE RID: 3518 RVA: 0x0004190C File Offset: 0x0003FB0C
		public override void Restart(bool displayToolPrompt = true)
		{
			base.Restart(displayToolPrompt);
			this.WireToEdit = null;
			if (this.wireEditorModal.IsOpen)
			{
				this.wireEditorModal.Hide();
			}
		}

		// Token: 0x06000DBF RID: 3519 RVA: 0x00041934 File Offset: 0x0003FB34
		protected override void ResetEmitterEventIndex(bool displayToolPrompt = true)
		{
			base.ResetEmitterEventIndex(displayToolPrompt);
			if (this.wireEditorModal.IsOpen)
			{
				this.wireEditorModal.Hide();
			}
		}

		// Token: 0x06000DC0 RID: 3520 RVA: 0x00041955 File Offset: 0x0003FB55
		protected override void ResetReceiverEventIndex(bool displayToolPrompt = true)
		{
			base.ResetReceiverEventIndex(displayToolPrompt);
			if (this.wireEditorModal.IsOpen)
			{
				this.wireEditorModal.Hide();
			}
		}

		// Token: 0x04000BBE RID: 3006
		[Header("UIWireEditorController")]
		[SerializeField]
		private UIWireEditorModalView wireEditorModal;
	}
}
