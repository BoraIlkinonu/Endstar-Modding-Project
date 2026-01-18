using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIAddScreenshotsToGameModalModel : UIGameObject, IUILoadingSpinnerViewCompatible
{
	public static Action ScreenshotsToAddChangedAction;

	[SerializeField]
	private UILevelAssetAndScreenshotsListModel levelAssetAndScreenshotsListModel;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private readonly Dictionary<ScreenshotFileInstances, int> gameScreenshotDictionary = new Dictionary<ScreenshotFileInstances, int>();

	private Queue<LevelReference> levelsToLoad = new Queue<LevelReference>();

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public Game Game { get; private set; }

	public UnityEvent SynchronizedUnityEvent { get; } = new UnityEvent();

	public List<ScreenshotFileInstances> ScreenshotsToAdd { get; } = new List<ScreenshotFileInstances>();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		UIScreenshotFileInstancesGameAdditionListCellController.SelectAction = (Action<ScreenshotFileInstances>)Delegate.Combine(UIScreenshotFileInstancesGameAdditionListCellController.SelectAction, new Action<ScreenshotFileInstances>(ToggleScreenshotsToAdd));
	}

	public async void Synchronize()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Synchronize");
		}
		OnLoadingStarted.Invoke();
		ScreenshotsToAdd.Clear();
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID);
		if (graphQlResult.HasErrors)
		{
			OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UIAddScreenshotsToGameModalModel_GetGame, graphQlResult.GetErrorMessage());
		}
		else
		{
			Game = GameLoader.Load(graphQlResult.GetDataMember().ToString());
			levelsToLoad = new Queue<LevelReference>(Game.levels);
			LoadNextLevel();
		}
	}

	public void Clear()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		levelAssetAndScreenshotsListModel.Clear(triggerEvents: true);
	}

	private async void LoadNextLevel()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "LoadNextLevel");
		}
		if (levelsToLoad.Count == 0)
		{
			levelAssetAndScreenshotsListModel.SetExteriorSelected(gameScreenshotDictionary);
			OnLoadingEnded.Invoke();
			SynchronizedUnityEvent.Invoke();
			return;
		}
		LevelReference levelReference = levelsToLoad.Dequeue();
		if (verboseLogging)
		{
			DebugUtility.Log("levelToLoad: " + levelReference.AssetID, this);
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(levelReference.AssetID);
		if (graphQlResult.HasErrors)
		{
			OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UIAddScreenshotsToGameModalModel_GetLevel, graphQlResult.GetErrorMessage());
			return;
		}
		LevelState levelState = LevelStateLoader.Load(graphQlResult.GetDataMember().ToString());
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "levelState", levelState), this);
		}
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "Screenshots", levelState.Screenshots.Count), this);
		}
		for (int num = levelState.Screenshots.Count - 1; num >= 0; num--)
		{
			ScreenshotFileInstances screenshotFileInstances = levelState.Screenshots[num];
			int num2 = Game.Screenshots.IndexOf(screenshotFileInstances);
			if (num2 > -1)
			{
				gameScreenshotDictionary.Add(screenshotFileInstances, num2);
			}
		}
		UILevelAssetAndScreenshotsListModelEntry item = new UILevelAssetAndScreenshotsListModelEntry(levelState);
		levelAssetAndScreenshotsListModel.Add(item, triggerEvents: true);
		LoadNextLevel();
	}

	private void ToggleScreenshotsToAdd(ScreenshotFileInstances screenshotFileInstances)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleScreenshotsToAdd", screenshotFileInstances);
		}
		if (ScreenshotsToAdd.Contains(screenshotFileInstances))
		{
			ScreenshotsToAdd.Remove(screenshotFileInstances);
		}
		else
		{
			ScreenshotsToAdd.Add(screenshotFileInstances);
		}
		ScreenshotsToAddChangedAction?.Invoke();
	}
}
