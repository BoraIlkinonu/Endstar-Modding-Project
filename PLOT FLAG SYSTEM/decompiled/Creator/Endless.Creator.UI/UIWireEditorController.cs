using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIWireEditorController : UIBaseWireController
{
	[Header("UIWireEditorController")]
	[SerializeField]
	private UIWireEditorModalView wireEditorModal;

	public UIWireView WireToEdit { get; private set; }

	public void EditWire(SerializableGuid wireId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "EditWire", "wireId", wireId), this);
		}
		UIWireView wire = base.WiresView.GetWire(wireId);
		if ((bool)wire)
		{
			EditWire(wire);
		}
	}

	public void EditWire(UIWireView wire)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "EditWire", "wire", wire.WireId), this);
		}
		WireToEdit = wire;
		base.WiringTool.SetWireEditedByPlayer_ServerRpc(wire.WireId);
		SetEmitterEventIndex(wire.EmitterNode.NodeIndex);
		SetReceiverEventIndex(wire.ReceiverNode.NodeIndex);
		if (base.WiringTool.ToolState == WiringTool.WiringToolState.Wiring)
		{
			wireEditorModal.InspectWire(wire);
		}
		base.WiresView.ToggleDarkMode(state: true, wire);
		base.WiringManager.SetWiringState(UIWiringStates.EditExisting);
	}

	public void DeleteWire(UIWireView wire)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "DeleteWire", wire.WireId);
		}
		base.WiringTool.DeleteWire(wire.WireId);
		base.WiresView.DespawnWire(wire);
		if (wireEditorModal.IsOpen)
		{
			wireEditorModal.Hide();
		}
		base.WiresView.ToggleDarkMode(state: false, wire);
		base.WiringManager.SetWiringState(UIWiringStates.Nothing);
	}

	public override void CreateWire(string[] storedParameterValues, WireColor wireColor)
	{
		base.CreateWire(storedParameterValues, wireColor);
		if ((bool)WireToEdit)
		{
			DeleteWire(WireToEdit);
			WireToEdit = null;
		}
	}

	public override void Restart(bool displayToolPrompt = true)
	{
		base.Restart(displayToolPrompt);
		WireToEdit = null;
		if (wireEditorModal.IsOpen)
		{
			wireEditorModal.Hide();
		}
	}

	protected override void ResetEmitterEventIndex(bool displayToolPrompt = true)
	{
		base.ResetEmitterEventIndex(displayToolPrompt);
		if (wireEditorModal.IsOpen)
		{
			wireEditorModal.Hide();
		}
	}

	protected override void ResetReceiverEventIndex(bool displayToolPrompt = true)
	{
		base.ResetReceiverEventIndex(displayToolPrompt);
		if (wireEditorModal.IsOpen)
		{
			wireEditorModal.Hide();
		}
	}
}
