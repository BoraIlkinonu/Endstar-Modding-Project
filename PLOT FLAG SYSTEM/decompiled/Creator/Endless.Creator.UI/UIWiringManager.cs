using System.Collections.Generic;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIWiringManager : MonoBehaviourSingleton<UIWiringManager>
{
	public UnityEvent<UIWiringStates> OnWiringStateChanged = new UnityEvent<UIWiringStates>();

	[SerializeField]
	private GameObject canvas;

	[SerializeField]
	private UIWiringInspectorPositioner wiringInspectorPositioner;

	[SerializeField]
	private UIWireCreatorController wireCreatorController;

	[SerializeField]
	private UIWireEditorController wireEditorController;

	[SerializeField]
	private UIWiringObjectInspectorView emitterInspector;

	[SerializeField]
	private UIWiringObjectInspectorView receiverInspector;

	[SerializeField]
	private UIWiresView wiresView;

	[SerializeField]
	private UIWiringRerouteView wiringRerouteView;

	[SerializeField]
	private UIWireConfirmationModalView wireConfirmationModalView;

	[SerializeField]
	private UIWireEditorModalView wireEditorModal;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private bool wiringToolActive;

	public UIWireCreatorController WireCreatorController => wireCreatorController;

	public UIWireEditorController WireEditorController => wireEditorController;

	public UIWiringObjectInspectorView EmitterInspector => emitterInspector;

	public UIWiringObjectInspectorView ReceiverInspector => receiverInspector;

	public UIWiresView WiresView => wiresView;

	public UIWiringRerouteView WiringRerouteView => wiringRerouteView;

	public UIWireConfirmationModalView WireConfirmationModalView => wireConfirmationModalView;

	public UIWireEditorModalView WireEditorModal => wireEditorModal;

	public UIWiringStates WiringState { get; private set; }

	public WiringTool WiringTool { get; private set; }

	public UnityEvent OnObjectSelected { get; } = new UnityEvent();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		WiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
		canvas.SetActive(value: false);
		WiringTool.OnEventObjectSelected.AddListener(OnEmitterObjectSelected);
		WiringTool.OnReceiverObjectSelected.AddListener(OnReceiverObjectSelected);
		WiringTool.OnWireConfirmed.AddListener(OnWireConfirmed);
		WiringTool.OnWireRemoved.AddListener(OnWireRemoved);
		MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(OnToolChange);
		NetworkBehaviourSingleton<CreatorManager>.Instance.LocalClientRoleChanged.AddListener(OnLocalClientRoleChanged);
	}

	public void HideWiringInspector(UIWiringObjectInspectorView wiringInspectorView, bool displayToolPrompt)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HideWiringInspector", wiringInspectorView.gameObject.name, displayToolPrompt);
		}
		if (WiringTool.ToolState == WiringTool.WiringToolState.Rerouting)
		{
			WiringTool.SetToolState(WiringTool.WiringToolState.Wiring);
		}
		bool flag = wiringInspectorView == emitterInspector;
		if (flag && WiringTool.ToolState == WiringTool.WiringToolState.Rerouting)
		{
			WiringTool.SetToolState(WiringTool.WiringToolState.Wiring);
		}
		wireCreatorController.Restart(!flag);
		wireEditorController.Restart(!flag);
		wiresView.DespawnAllWires();
		if (wiringInspectorView.IsOpen)
		{
			wiringInspectorView.Hide();
		}
		if (flag)
		{
			CloseAndResetEverything();
		}
		else
		{
			WiringTool.ReceiverSelectionCancelled();
		}
		if (displayToolPrompt)
		{
			MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
		}
	}

	public void SetWiringState(UIWiringStates newWiringState)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetWiringState", newWiringState);
		}
		if (WiringState != newWiringState)
		{
			WiringState = newWiringState;
			OnWiringStateChanged.Invoke(WiringState);
		}
	}

	private void OnEmitterObjectSelected(Transform targetTransform, SerializableGuid targetId, List<UIEndlessEventList> nodeEvents)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEmitterObjectSelected", targetTransform.name, targetId, nodeEvents.Count);
		}
		RectTransform leftWiringInspectorViewContainer = wiringInspectorPositioner.LeftWiringInspectorViewContainer;
		emitterInspector.Display(targetTransform, targetId, nodeEvents, isOnLeftOfScreen: true, isReceiver: false, leftWiringInspectorViewContainer);
		canvas.SetActive(value: true);
		MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
		OnObjectSelected.Invoke();
	}

	private void OnReceiverObjectSelected(Transform targetTransform, SerializableGuid targetId, List<UIEndlessEventList> nodeEvents)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnReceiverObjectSelected", targetTransform.name, targetId, nodeEvents.Count);
		}
		RectTransform reftWiringInspectorViewContainer = wiringInspectorPositioner.ReftWiringInspectorViewContainer;
		receiverInspector.Display(targetTransform, targetId, nodeEvents, isOnLeftOfScreen: false, isReceiver: true, reftWiringInspectorViewContainer);
		wiresView.DespawnAllWires();
		wiresView.Display();
		MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
		OnObjectSelected.Invoke();
	}

	private void CloseAndResetEverything()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CloseAndResetEverything");
		}
		wiringRerouteView.HideRerouteSwitch();
		wireCreatorController.Restart(displayToolPrompt: false);
		wireEditorController.Restart(displayToolPrompt: false);
		wiresView.DespawnAllWires();
		if (emitterInspector.IsOpen)
		{
			emitterInspector.Hide();
		}
		if (receiverInspector.IsOpen)
		{
			receiverInspector.Hide();
		}
		canvas.SetActive(value: false);
		WiringTool.ReceiverSelectionCancelled(closing: true);
		WiringTool.EmitterSelectionCancelled();
		wiringInspectorPositioner.enabled = false;
		SetWiringState(UIWiringStates.Nothing);
	}

	private void OnWireConfirmed()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnWireConfirmed");
		}
		wiringRerouteView.HideRerouteSwitch();
		wireCreatorController.Restart(displayToolPrompt: false);
		wireEditorController.Restart(displayToolPrompt: false);
		wiresView.DespawnAllWires();
		wiresView.Display();
		emitterInspector.UpdateWireCounts();
		ReceiverInspector.UpdateWireCounts();
		MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
	}

	private void OnWireRemoved()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnWireRemoved");
		}
		emitterInspector.UpdateWireCounts();
		ReceiverInspector.UpdateWireCounts();
	}

	private void OnToolChange(EndlessTool newTool)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnToolChange", (newTool != null) ? newTool.GetType().Name : "null");
		}
		bool flag = newTool is WiringTool;
		if (flag)
		{
			MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
		}
		else if (wiringToolActive)
		{
			HideWiringInspector(emitterInspector, displayToolPrompt: false);
		}
		wiringToolActive = flag;
	}

	private void OnLocalClientRoleChanged(Roles localClientLevelRole)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnLocalClientRoleChanged", localClientLevelRole);
		}
		if (localClientLevelRole.IsLessThan(Roles.Editor))
		{
			HideWiringInspector(emitterInspector, displayToolPrompt: false);
		}
	}
}
