using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIWiringPrompterManager : UIMonoBehaviourSingleton<UIWiringPrompterManager>
{
	[SerializeField]
	[TextArea]
	private string emitterObjectRequiredPrompt = "Select an object to emit an event";

	[SerializeField]
	[TextArea]
	private string receiverObjectRequiredPrompt = "Select an object to receive an event";

	[SerializeField]
	[TextArea]
	private string emitterEventRequiredPrompt = "Select an Emitter event";

	[SerializeField]
	[TextArea]
	private string receiverEventRequiredPrompt = "Select a Receiver event";

	[SerializeField]
	[TextArea]
	private string noInProgressOrExistingWiresPrompt = "Select an Event Node to begin a wire";

	[SerializeField]
	[TextArea]
	private string noInProgressWiresPrompt = "Select an Event Node to begin\na wire or an existing wire to edit";

	[SerializeField]
	private UIWiresView wiresView;

	[SerializeField]
	private UIWiringObjectInspectorView emitterWiringInspectorView;

	[SerializeField]
	private UIWiringObjectInspectorView receiverWiringInspectorView;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIWiringManager wiringManager;

	private UIToolPrompterManager toolPrompter;

	private UIWireCreatorController wireCreatorController;

	private UIWireEditorController wireEditorController;

	private int EmitterIndex
	{
		get
		{
			if (wiringManager.WiringState != UIWiringStates.CreateNew)
			{
				return wireEditorController.EmitterEventIndex;
			}
			return wireCreatorController.EmitterEventIndex;
		}
	}

	private int ReceiverIndex
	{
		get
		{
			if (wiringManager.WiringState != UIWiringStates.CreateNew)
			{
				return wireEditorController.ReceiverEventIndex;
			}
			return wireCreatorController.ReceiverEventIndex;
		}
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		wiringManager = MonoBehaviourSingleton<UIWiringManager>.Instance;
		toolPrompter = MonoBehaviourSingleton<UIToolPrompterManager>.Instance;
		wireCreatorController = wiringManager.WireCreatorController;
		wireEditorController = wiringManager.WireEditorController;
	}

	public void DisplayToolPrompt()
	{
		if (verboseLogging)
		{
			Debug.Log("DisplayToolPrompt", this);
		}
		string prompt = GetPrompt();
		toolPrompter.Display(prompt);
	}

	private string GetPrompt()
	{
		if (!emitterWiringInspectorView.TargetTransform)
		{
			return emitterObjectRequiredPrompt;
		}
		if (!receiverWiringInspectorView.TargetTransform)
		{
			return receiverObjectRequiredPrompt;
		}
		if (EmitterIndex <= -1 && ReceiverIndex <= -1)
		{
			if (wiresView.SpawnedWiresCount == 0)
			{
				return noInProgressOrExistingWiresPrompt;
			}
			return noInProgressWiresPrompt;
		}
		if (EmitterIndex <= -1)
		{
			return emitterEventRequiredPrompt;
		}
		if (ReceiverIndex <= -1)
		{
			return receiverEventRequiredPrompt;
		}
		Debug.LogWarning("UIWiringPrompter does not know what prompt to display! ", this);
		return string.Empty;
	}
}
