using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIGameController : UIAssetWithScreenshotsController
{
	[Header("UIGameController")]
	[SerializeField]
	private UIGameView gameView;

	[SerializeField]
	private UIGameModelHandler modelHandler;

	[SerializeField]
	private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;

	[SerializeField]
	private UIButton[] addScreenshotButtons = Array.Empty<UIButton>();

	[SerializeField]
	private UIAddScreenshotsToGameModalView addLevelScreenshotsToGameModalSource;

	[SerializeField]
	private UIDropHandler removeScreenshotDropHandler;

	private UnityAction<List<ScreenshotFileInstances>> onScreenshotsToAddedUnityAction;

	protected override void Start()
	{
		base.Start();
		UIButton[] array = addScreenshotButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].onClick.AddListener(OpenAddLevelScreenshotsToGameModal);
		}
		screenshotFileInstancesListModel.OnItemMovedUnityEvent.AddListener(RearrangeScreenshots);
		removeScreenshotDropHandler.DropWithGameObjectUnityEvent.AddListener(OnDroppedScreenshot);
		onScreenshotsToAddedUnityAction = gameView.AddScreenshots;
	}

	protected override async void SetName(string newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetName", newValue);
		}
		if (!(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.Name == newValue))
		{
			if (newValue.Replace(" ", string.Empty).IsNullOrEmptyOrWhiteSpace())
			{
				base.NameInputField.PlayInvalidInputTweens();
			}
			else
			{
				await MonoBehaviourSingleton<GameEditor>.Instance.UpdateGameName(newValue);
			}
		}
	}

	protected override async void SetDescription(string newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetDescription", newValue);
		}
		if (!(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.Description == newValue))
		{
			await MonoBehaviourSingleton<GameEditor>.Instance.UpdateDescription(newValue);
		}
	}

	protected override async void RemoveScreenshot(int index)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RemoveScreenshot", index);
		}
		base.OnLoadingStarted.Invoke();
		bool num = await MonoBehaviourSingleton<GameEditor>.Instance.RemoveGameScreenshotAt(index);
		base.OnLoadingEnded.Invoke();
		if (num)
		{
			bool triggerEvents = index > 0;
			screenshotFileInstancesListModel.RemoveAt(index, triggerEvents);
			if (index == 0)
			{
				gameView.DisplayScreenshots(screenshotFileInstancesListModel.ReadOnlyList.ToList());
			}
		}
	}

	protected override async void RearrangeScreenshots(int oldIndex, int newIndex)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RearrangeScreenshots", oldIndex, newIndex);
		}
		base.OnLoadingStarted.Invoke();
		await MonoBehaviourSingleton<GameEditor>.Instance.ReorderGameScreenshot(screenshotFileInstancesListModel.ReadOnlyList.ToList());
		base.OnLoadingEnded.Invoke();
		modelHandler.GetAsset();
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

	private void OpenAddLevelScreenshotsToGameModal()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OpenAddLevelScreenshotsToGameModal");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(addLevelScreenshotsToGameModalSource, UIModalManagerStackActions.ClearStack, onScreenshotsToAddedUnityAction);
	}
}
