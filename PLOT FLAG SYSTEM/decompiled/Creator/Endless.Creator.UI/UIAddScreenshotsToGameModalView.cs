using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIAddScreenshotsToGameModalView : UIEscapableModalView
{
	[SerializeField]
	private IntVariable screenshotLimit;

	[SerializeField]
	private UIButton doneButton;

	[SerializeField]
	private TextMeshProUGUI selectedText;

	[field: Header("UIAddScreenshotsToGameModalView")]
	[field: SerializeField]
	public UIAddScreenshotsToGameModalModel Model { get; private set; }

	public UnityEvent<List<ScreenshotFileInstances>> OnScreenshotsToAdded { get; } = new UnityEvent<List<ScreenshotFileInstances>>();

	protected override void Start()
	{
		base.Start();
		Model.SynchronizedUnityEvent.AddListener(OnScreenshotsToAddChangedAction);
		UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction = (Action)Delegate.Combine(UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction, new Action(OnScreenshotsToAddChangedAction));
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		UnityAction<List<ScreenshotFileInstances>> call = (UnityAction<List<ScreenshotFileInstances>>)modalData[0];
		OnScreenshotsToAdded.AddListener(call);
		doneButton.interactable = false;
		selectedText.text = "Loading...";
		Model.Synchronize();
	}

	public override void Close()
	{
		base.Close();
		Model.Clear();
		OnScreenshotsToAdded.RemoveAllListeners();
	}

	private void OnScreenshotsToAddChangedAction()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnScreenshotsToAddChangedAction");
		}
		int num = Mathf.Clamp(Model.Game.Screenshots.Count, 0, screenshotLimit.Value);
		int num2 = Mathf.Clamp(screenshotLimit.Value - num, 0, screenshotLimit.Value);
		List<ScreenshotFileInstances> screenshotsToAdd = Model.ScreenshotsToAdd;
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "screenshotCount", num), this);
			DebugUtility.Log(string.Format("{0}: {1}", "unusedScreenshots", num2), this);
			DebugUtility.Log(string.Format("{0}: {1}", "screenshotsToAdd", screenshotsToAdd.Count), this);
		}
		doneButton.interactable = screenshotsToAdd.Count <= num2;
		selectedText.text = $"{screenshotsToAdd.Count}/{num2}";
	}
}
