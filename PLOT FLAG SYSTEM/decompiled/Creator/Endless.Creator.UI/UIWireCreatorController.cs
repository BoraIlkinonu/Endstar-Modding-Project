using Endless.Creator.LevelEditing.Runtime;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI;

public class UIWireCreatorController : UIBaseWireController
{
	public override void Restart(bool displayToolPrompt = true)
	{
		base.Restart(displayToolPrompt);
		if (WireConfirmationModal.IsOpen)
		{
			WireConfirmationModal.Hide();
		}
	}

	public void DisplayWireConfirmation()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayWireConfirmation");
		}
		MonoBehaviourSingleton<UIToolPrompterManager>.Instance.Hide();
		string memberName = base.EmitterInspector.SpawnedNodes[base.EmitterEventIndex].MemberName;
		string memberName2 = base.ReceiverInspector.SpawnedNodes[base.ReceiverEventIndex].MemberName;
		EndlessEventInfo receiverEndlessEventInfo = base.ReceiverInspector.NodeEvents[base.ReceiverEventIndex];
		string assemblyQualifiedTypeName = base.EmitterInspector.SpawnedNodes[base.EmitterEventIndex].AssemblyQualifiedTypeName;
		string assemblyQualifiedTypeName2 = base.ReceiverInspector.SpawnedNodes[base.ReceiverEventIndex].AssemblyQualifiedTypeName;
		UIWireView uIWireView = base.WiresView.DisplayTempWire(assemblyQualifiedTypeName, memberName, assemblyQualifiedTypeName2, memberName2);
		if (base.WiringTool.ToolState == WiringTool.WiringToolState.Wiring)
		{
			WireConfirmationModal.Display(uIWireView, receiverEndlessEventInfo);
		}
		base.WiresView.ToggleDarkMode(state: true, uIWireView);
	}

	protected override void ResetEmitterEventIndex(bool displayToolPrompt = true)
	{
		base.ResetEmitterEventIndex(displayToolPrompt);
		if (WireConfirmationModal.IsOpen)
		{
			WireConfirmationModal.Hide();
		}
	}

	protected override void ResetReceiverEventIndex(bool displayToolPrompt = true)
	{
		base.ResetReceiverEventIndex(displayToolPrompt);
		if (WireConfirmationModal.IsOpen)
		{
			WireConfirmationModal.Hide();
		}
	}
}
