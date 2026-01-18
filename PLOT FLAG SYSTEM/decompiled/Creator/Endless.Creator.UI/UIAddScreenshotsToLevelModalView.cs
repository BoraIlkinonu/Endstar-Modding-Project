using System;
using System.Collections.Generic;
using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIAddScreenshotsToLevelModalView : UIBaseModalView
{
	[Header("UIAddScreenshotsToLevelModalView")]
	[SerializeField]
	private UIAddScreenshotsToLevelModalModel model;

	[SerializeField]
	private UIInMemoryScreenshotListModel inMemoryScreenshotListModel;

	[SerializeField]
	private IntVariable screenshotLimit;

	[SerializeField]
	private UIButton doneButton;

	[SerializeField]
	private TextMeshProUGUI selectedText;

	public UnityEvent BackUnityEvent { get; } = new UnityEvent();

	protected override void Start()
	{
		base.Start();
		UIAddScreenshotsToLevelModalModel.SynchronizedAction = (Action)Delegate.Combine(UIAddScreenshotsToLevelModalModel.SynchronizedAction, new Action(OnSynchronized));
		inMemoryScreenshotListModel.SelectionChangedUnityEvent.AddListener(OnSelectionChange);
		LayoutRebuilder.MarkLayoutForRebuild(base.RectTransform);
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		List<ScreenshotAPI.InMemoryScreenShot> list = new List<ScreenshotAPI.InMemoryScreenShot>(MonoBehaviourSingleton<ScreenshotAPI>.Instance.InMemoryScreenshots);
		inMemoryScreenshotListModel.Set(list, triggerEvents: true);
		doneButton.interactable = false;
		selectedText.text = "Loading...";
		model.Synchronize();
	}

	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		BackUnityEvent.Invoke();
	}

	private void OnSynchronized()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSynchronized");
		}
		ViewSelection();
	}

	private void OnSelectionChange(int dataIndex, bool selected)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSelectionChange", dataIndex, selected);
		}
		ViewSelection();
	}

	private void ViewSelection()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewSelection");
		}
		int num = Mathf.Clamp(model.LevelState.Screenshots.Count, 0, screenshotLimit.Value);
		int num2 = Mathf.Clamp(screenshotLimit.Value - num, 0, screenshotLimit.Value);
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "screenshotCount", num), this);
			DebugUtility.Log(string.Format("{0}: {1}", "unusedScreenshots", num2), this);
		}
		doneButton.interactable = inMemoryScreenshotListModel.SelectedTypedList.Count <= num2;
		selectedText.text = $"{inMemoryScreenshotListModel.SelectedTypedList.Count}/{num2}";
	}
}
