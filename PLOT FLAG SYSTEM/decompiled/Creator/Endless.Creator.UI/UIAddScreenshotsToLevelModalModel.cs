using System;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIAddScreenshotsToLevelModalModel : UIGameObject, IUILoadingSpinnerViewCompatible
{
	public static Action SynchronizedAction;

	[SerializeField]
	private IntVariable screenshotLimit;

	[SerializeField]
	private UIInMemoryScreenshotListModel inMemoryScreenshotListModel;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public LevelState LevelState { get; private set; }

	public async void Synchronize()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Synchronize");
		}
		OnLoadingStarted.Invoke();
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID);
		OnLoadingEnded.Invoke();
		if (graphQlResult.HasErrors)
		{
			ErrorHandler.HandleError(ErrorCodes.UIAddScreenshotsToLevelModalModel_GetLevel, graphQlResult.GetErrorMessage());
			return;
		}
		LevelState = LevelStateLoader.Load(graphQlResult.GetDataMember().ToString());
		int num = Mathf.Clamp(LevelState.Screenshots.Count, 0, screenshotLimit.Value);
		int num2 = Mathf.Clamp(screenshotLimit.Value - num, 0, screenshotLimit.Value);
		int num3 = Mathf.Clamp(num2, 0, inMemoryScreenshotListModel.Count);
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0}.{1}: {2}", "LevelState", "Screenshots", LevelState.Screenshots.Count), this);
			DebugUtility.Log(string.Format("{0}: {1}", "screenshotLimit", screenshotLimit.Value), this);
			DebugUtility.Log(string.Format("{0}: {1}", "screenshotCount", num), this);
			DebugUtility.Log(string.Format("{0}: {1}", "unusedScreenshots", num2), this);
			DebugUtility.Log(string.Format("{0}: {1}", "inMemoryScreenshotListModel", inMemoryScreenshotListModel.Count), this);
			DebugUtility.Log(string.Format("{0}: {1}", "autoSelect", num3), this);
		}
		for (int i = 0; i < num3; i++)
		{
			inMemoryScreenshotListModel.Select(i, triggerEvents: true);
		}
		SynchronizedAction?.Invoke();
	}
}
