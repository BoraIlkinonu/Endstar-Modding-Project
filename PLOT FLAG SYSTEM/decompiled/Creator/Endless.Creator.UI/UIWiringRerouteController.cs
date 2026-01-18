using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endless.Creator.UI;

public class UIWiringRerouteController : UIGameObject, IValidatable
{
	[SerializeField]
	private UIToggle rerouteSwitchToggle;

	[SerializeField]
	private TweenCollection rerouteSwitchHideTweens;

	[SerializeField]
	private UIButton rerouteUndoButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private WiringTool wiringTool;

	private UIWiringManager wiringManager;

	private UIWireConfirmationModalView wireConfirmationModalView;

	private UIWireCreatorController wireCreatorController;

	private UIWireEditorModalView wireEditorModal;

	private UIWireEditorController wireEditorController;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		rerouteSwitchToggle.OnChange.AddListener(ToggleRerouting);
		wiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
		wiringManager = MonoBehaviourSingleton<UIWiringManager>.Instance;
		wireConfirmationModalView = wiringManager.WireConfirmationModalView;
		wireCreatorController = wiringManager.WireCreatorController;
		wireEditorModal = wiringManager.WireEditorModal;
		wireEditorController = wiringManager.WireEditorController;
		rerouteSwitchHideTweens.OnAllTweenCompleted.AddListener(ToggleOff);
		rerouteUndoButton.onClick.AddListener(UndoReroute);
	}

	[ContextMenu("Validate")]
	public void Validate()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Validate");
		}
		rerouteSwitchHideTweens.ValidateForNumberOfTweens();
	}

	private void ToggleRerouting(bool state)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleRerouting", state);
		}
		if (state)
		{
			wiringTool.SetToolState(WiringTool.WiringToolState.Rerouting);
		}
		else
		{
			wiringTool.SetToolState(WiringTool.WiringToolState.Wiring);
		}
		switch (wiringManager.WiringState)
		{
		case UIWiringStates.CreateNew:
			if (state)
			{
				wireConfirmationModalView.Hide();
			}
			else
			{
				wireCreatorController.DisplayWireConfirmation();
			}
			break;
		case UIWiringStates.EditExisting:
			if (state)
			{
				wireEditorModal.Hide();
			}
			else
			{
				wireEditorModal.Display();
			}
			break;
		default:
			DebugUtility.LogError(this, "ToggleRerouting", string.Format("No support for a {0} of {1}", "WiringState", wiringManager.WiringState), state);
			break;
		case UIWiringStates.Nothing:
			break;
		}
	}

	private void ToggleOff()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleOff");
		}
		ToggleRerouting(state: false);
	}

	private void UndoReroute()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UndoReroute");
		}
		wiringTool.RequestPopLastRerouteNode(default(InputAction.CallbackContext));
	}
}
