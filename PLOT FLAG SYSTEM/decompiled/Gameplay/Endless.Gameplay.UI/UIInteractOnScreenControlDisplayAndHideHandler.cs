using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIInteractOnScreenControlDisplayAndHideHandler : UIGameObject
{
	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		UIInteractionPromptAnchor.OnInitializeAction = (Action)Delegate.Combine(UIInteractionPromptAnchor.OnInitializeAction, new Action(displayAndHideHandler.Display));
		UIInteractionPromptAnchor.OnCloseAction = (Action)Delegate.Combine(UIInteractionPromptAnchor.OnCloseAction, new Action(displayAndHideHandler.Hide));
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		displayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		UIInteractionPromptAnchor.OnInitializeAction = (Action)Delegate.Remove(UIInteractionPromptAnchor.OnInitializeAction, new Action(displayAndHideHandler.Display));
		UIInteractionPromptAnchor.OnCloseAction = (Action)Delegate.Remove(UIInteractionPromptAnchor.OnCloseAction, new Action(displayAndHideHandler.Hide));
	}
}
