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

namespace Endless.Creator.UI
{
	// Token: 0x02000280 RID: 640
	public class UIAddScreenshotsToGameModalModel : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000150 RID: 336
		// (get) Token: 0x06000A8A RID: 2698 RVA: 0x00031576 File Offset: 0x0002F776
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000A8B RID: 2699 RVA: 0x0003157E File Offset: 0x0002F77E
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000A8C RID: 2700 RVA: 0x00031586 File Offset: 0x0002F786
		// (set) Token: 0x06000A8D RID: 2701 RVA: 0x0003158E File Offset: 0x0002F78E
		public Game Game { get; private set; }

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x06000A8E RID: 2702 RVA: 0x00031597 File Offset: 0x0002F797
		public UnityEvent SynchronizedUnityEvent { get; } = new UnityEvent();

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000A8F RID: 2703 RVA: 0x0003159F File Offset: 0x0002F79F
		public List<ScreenshotFileInstances> ScreenshotsToAdd { get; } = new List<ScreenshotFileInstances>();

		// Token: 0x06000A90 RID: 2704 RVA: 0x000315A7 File Offset: 0x0002F7A7
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIScreenshotFileInstancesGameAdditionListCellController.SelectAction = (Action<ScreenshotFileInstances>)Delegate.Combine(UIScreenshotFileInstancesGameAdditionListCellController.SelectAction, new Action<ScreenshotFileInstances>(this.ToggleScreenshotsToAdd));
		}

		// Token: 0x06000A91 RID: 2705 RVA: 0x000315E4 File Offset: 0x0002F7E4
		public async void Synchronize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Synchronize", Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
			this.ScreenshotsToAdd.Clear();
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID, "", null, false, 10);
			if (graphQlResult.HasErrors)
			{
				this.OnLoadingEnded.Invoke();
				ErrorHandler.HandleError(ErrorCodes.UIAddScreenshotsToGameModalModel_GetGame, graphQlResult.GetErrorMessage(0), true, false);
			}
			else
			{
				this.Game = GameLoader.Load(graphQlResult.GetDataMember().ToString());
				this.levelsToLoad = new Queue<LevelReference>(this.Game.levels);
				this.LoadNextLevel();
			}
		}

		// Token: 0x06000A92 RID: 2706 RVA: 0x0003161B File Offset: 0x0002F81B
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.levelAssetAndScreenshotsListModel.Clear(true);
		}

		// Token: 0x06000A93 RID: 2707 RVA: 0x00031644 File Offset: 0x0002F844
		private async void LoadNextLevel()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "LoadNextLevel", Array.Empty<object>());
			}
			if (this.levelsToLoad.Count == 0)
			{
				this.levelAssetAndScreenshotsListModel.SetExteriorSelected(this.gameScreenshotDictionary);
				this.OnLoadingEnded.Invoke();
				this.SynchronizedUnityEvent.Invoke();
			}
			else
			{
				LevelReference levelReference = this.levelsToLoad.Dequeue();
				if (this.verboseLogging)
				{
					DebugUtility.Log("levelToLoad: " + levelReference.AssetID, this);
				}
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(levelReference.AssetID, "", null, false, 10);
				if (graphQlResult.HasErrors)
				{
					this.OnLoadingEnded.Invoke();
					ErrorHandler.HandleError(ErrorCodes.UIAddScreenshotsToGameModalModel_GetLevel, graphQlResult.GetErrorMessage(0), true, false);
				}
				else
				{
					LevelState levelState = LevelStateLoader.Load(graphQlResult.GetDataMember().ToString());
					if (this.verboseLogging)
					{
						DebugUtility.Log(string.Format("{0}: {1}", "levelState", levelState), this);
					}
					if (this.verboseLogging)
					{
						DebugUtility.Log(string.Format("{0}: {1}", "Screenshots", levelState.Screenshots.Count), this);
					}
					for (int i = levelState.Screenshots.Count - 1; i >= 0; i--)
					{
						ScreenshotFileInstances screenshotFileInstances = levelState.Screenshots[i];
						int num = this.Game.Screenshots.IndexOf(screenshotFileInstances);
						if (num > -1)
						{
							this.gameScreenshotDictionary.Add(screenshotFileInstances, num);
						}
					}
					UILevelAssetAndScreenshotsListModelEntry uilevelAssetAndScreenshotsListModelEntry = new UILevelAssetAndScreenshotsListModelEntry(levelState);
					this.levelAssetAndScreenshotsListModel.Add(uilevelAssetAndScreenshotsListModelEntry, true);
					this.LoadNextLevel();
				}
			}
		}

		// Token: 0x06000A94 RID: 2708 RVA: 0x0003167C File Offset: 0x0002F87C
		private void ToggleScreenshotsToAdd(ScreenshotFileInstances screenshotFileInstances)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleScreenshotsToAdd", new object[] { screenshotFileInstances });
			}
			if (this.ScreenshotsToAdd.Contains(screenshotFileInstances))
			{
				this.ScreenshotsToAdd.Remove(screenshotFileInstances);
			}
			else
			{
				this.ScreenshotsToAdd.Add(screenshotFileInstances);
			}
			Action screenshotsToAddChangedAction = UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction;
			if (screenshotsToAddChangedAction == null)
			{
				return;
			}
			screenshotsToAddChangedAction();
		}

		// Token: 0x040008D2 RID: 2258
		public static Action ScreenshotsToAddChangedAction;

		// Token: 0x040008D3 RID: 2259
		[SerializeField]
		private UILevelAssetAndScreenshotsListModel levelAssetAndScreenshotsListModel;

		// Token: 0x040008D4 RID: 2260
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040008D5 RID: 2261
		private readonly Dictionary<ScreenshotFileInstances, int> gameScreenshotDictionary = new Dictionary<ScreenshotFileInstances, int>();

		// Token: 0x040008D6 RID: 2262
		private Queue<LevelReference> levelsToLoad = new Queue<LevelReference>();
	}
}
