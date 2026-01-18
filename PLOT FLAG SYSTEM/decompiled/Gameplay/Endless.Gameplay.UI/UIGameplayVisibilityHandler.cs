using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIGameplayVisibilityHandler : UIGameObject
{
	[SerializeField]
	private UIDisplayAndHideHandler rootDisplayAndHideHandler;

	[SerializeField]
	private UIDisplayAndHideHandler[] displayOnGameplayStarted = Array.Empty<UIDisplayAndHideHandler>();

	[SerializeField]
	private UIDisplayAndHideHandler[] hideOnGameplayCleanup = Array.Empty<UIDisplayAndHideHandler>();

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private IEnumerator Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		UIDisplayAndHideHandler[] array = hideOnGameplayCleanup;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetToHideEnd(triggerUnityEvent: true);
		}
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(OnGameplayStarted);
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(OnGameplayCleanup);
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStopped.AddListener(OnGameplayStopped);
		yield return new WaitForEndOfFrame();
		rootDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
	}

	private void OnGameplayStarted()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnGameplayStarted");
		}
		UIDisplayAndHideHandler[] array = displayOnGameplayStarted;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Display();
		}
		UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(HideRootGameplayUi));
		UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(DisplayRootGameplayUi));
		if (!MonoBehaviourSingleton<UIScreenManager>.Instance.IsDisplaying)
		{
			DisplayRootGameplayUi();
		}
	}

	private void OnGameplayCleanup()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnGameplayCleanup");
		}
		HideAllGameplayUi();
	}

	private void OnGameplayStopped()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnGameplayStopped");
		}
		UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemOpen, new Action(HideRootGameplayUi));
		UIScreenManager.OnScreenSystemClose = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemClose, new Action(DisplayRootGameplayUi));
		HideAllGameplayUi();
	}

	private void DisplayRootGameplayUi()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayRootGameplayUi");
		}
		rootDisplayAndHideHandler.Display();
	}

	private void HideAllGameplayUi()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HideRootGameplayUi");
		}
		HideRootGameplayUi();
		UIDisplayAndHideHandler[] array = hideOnGameplayCleanup;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Hide();
		}
	}

	private void HideRootGameplayUi()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HideRootGameplayUi");
		}
		rootDisplayAndHideHandler.Hide();
	}
}
