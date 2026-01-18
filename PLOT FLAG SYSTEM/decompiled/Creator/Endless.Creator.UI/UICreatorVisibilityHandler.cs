using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UICreatorVisibilityHandler : UIGameObject
{
	[SerializeField]
	private UIDisplayAndHideHandler rootDisplayAndHideHandler;

	[SerializeField]
	private UIDisplayAndHideHandler[] displayOnCreatorStarted = new UIDisplayAndHideHandler[0];

	[SerializeField]
	private UIDisplayAndHideHandler[] hideOnCreatorEnded = new UIDisplayAndHideHandler[0];

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private IEnumerator Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		UIDisplayAndHideHandler[] array = hideOnCreatorEnded;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetToHideEnd(triggerUnityEvent: true);
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(OnCreatorStarted);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(OnCreatorEnded);
		yield return new WaitForEndOfFrame();
		rootDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
	}

	private void OnCreatorStarted()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnCreatorStarted");
		}
		UIDisplayAndHideHandler[] array = displayOnCreatorStarted;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Display();
		}
		UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(Hide));
		UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(Display));
		if (!MonoBehaviourSingleton<UIScreenManager>.Instance.IsDisplaying)
		{
			Display();
		}
	}

	private void OnCreatorEnded()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnCreatorEnded");
		}
		Hide();
		UIDisplayAndHideHandler[] array = hideOnCreatorEnded;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Hide();
		}
		UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemOpen, new Action(Hide));
		UIScreenManager.OnScreenSystemClose = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemClose, new Action(Display));
	}

	private void Display()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Display");
		}
		rootDisplayAndHideHandler.Display();
	}

	private void Hide()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Hide");
		}
		rootDisplayAndHideHandler.Hide();
	}
}
