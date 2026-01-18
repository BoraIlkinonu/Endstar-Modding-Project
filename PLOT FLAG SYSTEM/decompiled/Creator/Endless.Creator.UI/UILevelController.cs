using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelController : UIAssetWithScreenshotsController
{
	[Header("UILevelController")]
	[SerializeField]
	private UILevelView levelView;

	[SerializeField]
	private UIScreenshotView mainScreenshot;

	[SerializeField]
	private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;

	[SerializeField]
	private UIDropHandler removeScreenshotDropHandler;

	private ScreenshotTool screenshotTool;

	protected override void Start()
	{
		base.Start();
		screenshotTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<ScreenshotTool>();
		screenshotFileInstancesListModel.OnItemMovedUnityEvent.AddListener(RearrangeScreenshots);
		removeScreenshotDropHandler.DropWithGameObjectUnityEvent.AddListener(OnDroppedScreenshot);
	}

	protected override void SetName(string newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetName", newValue);
		}
		if (!(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name == newValue))
		{
			if (newValue.Replace(" ", string.Empty).IsNullOrEmptyOrWhiteSpace())
			{
				base.NameInputField.PlayInvalidInputTweens();
			}
			else
			{
				NetworkBehaviourSingleton<CreatorManager>.Instance.LevelEditor.UpdateLevelName_ServerRpc(newValue);
			}
		}
	}

	protected override void SetDescription(string newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetDescription", newValue);
		}
		if (!(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Description == newValue))
		{
			NetworkBehaviourSingleton<CreatorManager>.Instance.LevelEditor.UpdateLevelDescription_ServerRpc(newValue);
		}
	}

	protected override void RemoveScreenshot(int index)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RemoveScreenshot", index);
		}
		DebugUtility.LogMethod(this, "RemoveScreenshot", index);
		screenshotTool.RemoveScreenshotFromLevel_ServerRPC(index);
		bool triggerEvents = index > 0;
		screenshotFileInstancesListModel.RemoveAt(index, triggerEvents);
		if (index == 0)
		{
			levelView.DisplayScreenshots(screenshotFileInstancesListModel.ReadOnlyList.ToList());
		}
	}

	protected override void RearrangeScreenshots(int oldIndex, int newIndex)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RearrangeScreenshots", oldIndex, newIndex);
		}
		List<ScreenshotFileInstances> list = screenshotFileInstancesListModel.ReadOnlyList.ToList();
		screenshotTool.RearrangeScreenshotsToLevel_ServerRPC(list.ToArray());
		levelView.DisplayScreenshots(list);
	}

	private void OnDroppedScreenshot(GameObject droppedGameObject)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDroppedScreenshot", droppedGameObject.DebugSafeName());
		}
		if (droppedGameObject.TryGetComponent<UIScreenshotFileInstancesListCellView>(out var component))
		{
			int num = screenshotFileInstancesListModel.ReadOnlyList.IndexOf(component.Model);
			if (num < 0)
			{
				throw new IndexOutOfRangeException("Could not get index of " + component.Model.GetType().Namespace + " from screenshotFileInstancesListModel!");
			}
			RemoveScreenshot(num);
			return;
		}
		throw new NullReferenceException("Could not get UIScreenshotFileInstancesListCellView from droppedGameObject!");
	}
}
