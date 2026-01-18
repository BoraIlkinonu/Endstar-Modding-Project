using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

[RequireComponent(typeof(UIWireEditorModalView))]
public class UIWireEditorModalController : UIGameObject
{
	[SerializeField]
	private UIToggle overrideEmitterContextualValueToggle;

	[SerializeField]
	private UIWirePropertyModifierView wiringPropertyModifier;

	[SerializeField]
	private UIWiringRerouteView wiringReroute;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIWireEditorModalView wireEditorView;

	private WiringTool wiringTool;

	private UIWiringManager WiringManager => MonoBehaviourSingleton<UIWiringManager>.Instance;

	private UIWireView Wire => wireEditorView.Wire;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		TryGetComponent<UIWireEditorModalView>(out wireEditorView);
		wiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
		overrideEmitterContextualValueToggle.OnChange.AddListener(SetOverrideEmitterContextualValue);
	}

	public void Cancel()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Cancel");
		}
		wiringReroute.HideRerouteSwitch();
		WiringManager.WireEditorController.Restart();
	}

	public void EditEmitter()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "EditEmitter");
		}
		WiringManager.WireEditorController.SetEmitterEventIndex(-1);
		wireEditorView.Hide();
	}

	public void EditReceiver()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "EditReceiver");
		}
		WiringManager.WireEditorController.SetReceiverEventIndex(-1);
		wireEditorView.Hide();
	}

	public void Delete()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Delete");
		}
		WiringManager.WireEditorController.DeleteWire(Wire);
		WiringManager.WireEditorController.Restart();
	}

	private void SetOverrideEmitterContextualValue(bool overrideEmitterContextualValue)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetOverrideEmitterContextualValue", overrideEmitterContextualValue);
		}
		WireEntry wireEntry = WiringUtilities.GetWireEntry(Wire.EmitterNode.InspectedObjectId, Wire.EmitterNode.MemberName, Wire.ReceiverNode.InspectedObjectId, Wire.ReceiverNode.MemberName);
		string[] array = (overrideEmitterContextualValueToggle.IsOn ? wiringPropertyModifier.StoredParameterValues : Array.Empty<string>());
		wiringTool.UpdateWire(Wire.WireId, Wire.EmitterNode.InspectedObjectId, Wire.EmitterNode.MemberName, Wire.EmitterNode.AssemblyQualifiedTypeName, Wire.ReceiverNode.InspectedObjectId, Wire.ReceiverNode.MemberName, Wire.ReceiverNode.AssemblyQualifiedTypeName, array, Array.Empty<SerializableGuid>(), (WireColor)wireEntry.WireColor);
		wireEditorView.SetWiringPropertyModifierVisibility(overrideEmitterContextualValue);
	}
}
