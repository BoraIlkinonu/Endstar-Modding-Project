using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public abstract class UIBaseWireController : UIGameObject
{
	[SerializeField]
	protected UIWireConfirmationModalView WireConfirmationModal;

	[field: Header("Debugging")]
	[field: SerializeField]
	protected bool VerboseLogging { get; set; }

	public int EmitterEventIndex { get; private set; } = -1;

	public int ReceiverEventIndex { get; private set; } = -1;

	public bool CanCreateWire
	{
		get
		{
			bool flag = EmitterEventIndex > -1 && ReceiverEventIndex > -1;
			if (flag)
			{
				SerializableGuid targetId = EmitterInspector.TargetId;
				string memberName = EmitterInspector.NodeEvents[EmitterEventIndex].MemberName;
				SerializableGuid targetId2 = ReceiverInspector.TargetId;
				string memberName2 = ReceiverInspector.NodeEvents[ReceiverEventIndex].MemberName;
				if (WiringUtilities.GetWiringId(targetId, memberName, targetId2, memberName2) != SerializableGuid.Empty)
				{
					WiresView.DespawnTempWire();
					WiresView.ToggleDarkMode(state: false, null);
					ResetEmitterEventIndex();
					ResetReceiverEventIndex();
					return false;
				}
			}
			return flag;
		}
	}

	protected WiringTool WiringTool { get; private set; }

	protected UIWiringManager WiringManager => MonoBehaviourSingleton<UIWiringManager>.Instance;

	protected UIWiringObjectInspectorView EmitterInspector => WiringManager.EmitterInspector;

	protected UIWiringObjectInspectorView ReceiverInspector => WiringManager.ReceiverInspector;

	protected UIWiresView WiresView => WiringManager.WiresView;

	private void Start()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		WiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
		EmitterInspector.OnDisplay.AddListener(OnEmitterInspectorDisplayed);
		ReceiverInspector.OnDisplay.AddListener(OnReceiverInspectorDisplayed);
	}

	public void SetEmitterEventIndex(int newValue)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetEmitterEventIndex", "newValue", newValue), this);
		}
		if (EmitterEventIndex == newValue || newValue == -1)
		{
			ResetEmitterEventIndex();
			return;
		}
		int emitterEventIndex = EmitterEventIndex;
		if (emitterEventIndex > -1)
		{
			EmitterInspector.ToggleNodeSelectedVisuals(emitterEventIndex, state: false);
		}
		EmitterEventIndex = newValue;
		EmitterInspector.ToggleNodeSelectedVisuals(newValue, state: true);
	}

	public void SetReceiverEventIndex(int newValue)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetReceiverEventIndex", "newValue", newValue), this);
		}
		if (ReceiverEventIndex == newValue || newValue == -1)
		{
			ResetReceiverEventIndex();
			return;
		}
		int receiverEventIndex = ReceiverEventIndex;
		if (receiverEventIndex > -1)
		{
			ReceiverInspector.ToggleNodeSelectedVisuals(receiverEventIndex, state: false);
		}
		ReceiverEventIndex = newValue;
		ReceiverInspector.ToggleNodeSelectedVisuals(newValue, state: true);
	}

	public virtual void CreateWire(string[] storedParameterValues, WireColor wireColor)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", "CreateWire", "storedParameterValues", storedParameterValues.Length, "wireColor", wireColor), this);
			for (int i = 0; i < storedParameterValues.Length; i++)
			{
				DebugUtility.Log(string.Format("{0}[{1}]: {2}", "storedParameterValues", i, storedParameterValues[i]), this);
			}
		}
		if (EmitterEventIndex < 0 || EmitterEventIndex >= EmitterInspector.SpawnedNodes.Count)
		{
			DebugUtility.LogError(string.Format("{0} {1} is out of range for {2}.{3} with a Count of {4}", "EmitterEventIndex", EmitterEventIndex, "EmitterInspector", "SpawnedNodes", EmitterInspector.SpawnedNodes.Count), this);
		}
		else if (ReceiverEventIndex < 0 || ReceiverEventIndex >= ReceiverInspector.SpawnedNodes.Count)
		{
			DebugUtility.LogError(string.Format("{0} {1} is out of range for {2}.{3} with a Count of {4}", "ReceiverEventIndex", ReceiverEventIndex, "ReceiverInspector", "SpawnedNodes", ReceiverInspector.SpawnedNodes.Count), this);
		}
		else
		{
			UIWireNodeView uIWireNodeView = EmitterInspector.SpawnedNodes[EmitterEventIndex];
			UIWireNodeView uIWireNodeView2 = ReceiverInspector.SpawnedNodes[ReceiverEventIndex];
			WiringTool.EventConfirmed(uIWireNodeView.NodeEvent, uIWireNodeView.AssemblyQualifiedTypeName, uIWireNodeView2.NodeEvent, uIWireNodeView2.AssemblyQualifiedTypeName, storedParameterValues, wireColor);
			WiresView.DespawnAllWires();
			WiresView.Display();
		}
	}

	public virtual void Restart(bool displayToolPrompt = true)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Restart", "displayToolPrompt", displayToolPrompt), this);
		}
		ResetEmitterEventIndex(displayToolPrompt);
		ResetReceiverEventIndex(displayToolPrompt);
		WiresView.DespawnTempWire();
		WiresView.ToggleDarkMode(state: false, null);
		WiringManager.SetWiringState(UIWiringStates.Nothing);
		if (displayToolPrompt)
		{
			MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
		}
	}

	protected virtual void ResetEmitterEventIndex(bool displayToolPrompt = true)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ResetEmitterEventIndex", "displayToolPrompt", displayToolPrompt), this);
		}
		if (EmitterEventIndex > -1)
		{
			EmitterInspector.ToggleNodeSelectedVisuals(EmitterEventIndex, state: false);
		}
		EmitterEventIndex = -1;
		if (displayToolPrompt)
		{
			MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
		}
	}

	protected virtual void ResetReceiverEventIndex(bool displayToolPrompt = true)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ResetReceiverEventIndex", "displayToolPrompt", displayToolPrompt), this);
		}
		if (ReceiverEventIndex > -1)
		{
			ReceiverInspector.ToggleNodeSelectedVisuals(ReceiverEventIndex, state: false);
		}
		ReceiverEventIndex = -1;
		if (displayToolPrompt)
		{
			MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
		}
	}

	private void OnEmitterInspectorDisplayed()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("OnEmitterInspectorDisplayed", this);
		}
		ResetReceiverEventIndex();
		ResetEmitterEventIndex();
	}

	private void OnReceiverInspectorDisplayed()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("OnReceiverInspectorDisplayed", this);
		}
		ResetReceiverEventIndex();
	}
}
