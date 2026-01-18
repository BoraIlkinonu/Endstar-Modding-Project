using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIWiringRerouteView : UIGameObject
{
	[SerializeField]
	private UIToggle rerouteSwitchToggle;

	[SerializeField]
	private TweenCollection rerouteSwitchDisplayTweens;

	[SerializeField]
	private TweenCollection rerouteSwitchHideTweens;

	[SerializeField]
	private UIButton rerouteUndoButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private WiringTool wiringTool;

	private UIWiringManager wiringManager;

	private UIWireConfirmationModalView wireConfirmation;

	private UIWireEditorModalView wireEditor;

	private UIWiringObjectInspectorView emitterInspector;

	private UIWiringObjectInspectorView receiverInspector;

	private bool isOpen;

	private bool ShouldDisplayToggle
	{
		get
		{
			if (!emitterInspector.IsOpen || !receiverInspector.IsOpen)
			{
				return false;
			}
			switch (wiringManager.WiringState)
			{
			case UIWiringStates.Nothing:
				return false;
			case UIWiringStates.CreateNew:
				return wiringManager.WireCreatorController.CanCreateWire;
			case UIWiringStates.EditExisting:
				return wiringManager.WireEditorController.CanCreateWire;
			default:
				Debug.LogException(new Exception(string.Format("{0} does not have support for a {1} of {2}", "UIWiringRerouteView", "WiringState", wiringManager.WiringState)), this);
				return false;
			}
		}
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		wiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
		wiringTool.OnToolStateChanged.AddListener(OnWiringToolStateChanged);
		wiringManager = MonoBehaviourSingleton<UIWiringManager>.Instance;
		wireConfirmation = wiringManager.WireConfirmationModalView;
		wireEditor = wiringManager.WireEditorModal;
		emitterInspector = wiringManager.EmitterInspector;
		receiverInspector = wiringManager.ReceiverInspector;
		emitterInspector.OnDisplay.AddListener(ToggleRerouteSwitchVisibility);
		receiverInspector.OnDisplay.AddListener(ToggleRerouteSwitchVisibility);
		emitterInspector.OnHide.AddListener(ToggleRerouteSwitchVisibility);
		receiverInspector.OnHide.AddListener(ToggleRerouteSwitchVisibility);
		wireConfirmation.OnDisplay.AddListener(DisplayRerouteSwitch);
		wireEditor.OnDisplay.AddListener(DisplayRerouteSwitch);
		wiringTool.EnableRerouteUndo.AddListener(EnableRerouteUndoButton);
		wiringTool.DisableRerouteUndo.AddListener(DisableRerouteUndoButton);
		rerouteSwitchDisplayTweens.SetToStart();
	}

	public void HideRerouteSwitch()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HideRerouteSwitch");
		}
		isOpen = false;
		rerouteSwitchHideTweens.Tween();
		DisableRerouteUndoButton();
	}

	private void ToggleRerouteSwitchVisibility()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleRerouteSwitchVisibility");
		}
		if (!isOpen && ShouldDisplayToggle)
		{
			DisplayRerouteSwitch();
		}
		else if (isOpen && !ShouldDisplayToggle)
		{
			HideRerouteSwitch();
		}
	}

	private void OnWiringToolStateChanged(WiringTool.WiringToolState wiringToolState)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnWiringToolStateChanged", wiringToolState);
		}
		bool state = wiringToolState == WiringTool.WiringToolState.Rerouting;
		rerouteSwitchToggle.SetIsOn(state, suppressOnChange: false);
	}

	private void DisplayRerouteSwitch()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayRerouteSwitch");
		}
		if (rerouteSwitchHideTweens.IsAnyTweening())
		{
			rerouteSwitchHideTweens.Cancel();
		}
		isOpen = true;
		rerouteSwitchDisplayTweens.Tween();
	}

	private void EnableRerouteUndoButton()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "EnableRerouteUndoButton");
		}
		rerouteUndoButton.interactable = true;
	}

	private void DisableRerouteUndoButton()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisableRerouteUndoButton");
		}
		rerouteUndoButton.interactable = false;
	}
}
