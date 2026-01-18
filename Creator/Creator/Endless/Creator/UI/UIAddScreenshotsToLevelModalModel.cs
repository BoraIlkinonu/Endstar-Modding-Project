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

namespace Endless.Creator.UI
{
	// Token: 0x02000286 RID: 646
	public class UIAddScreenshotsToLevelModalModel : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000159 RID: 345
		// (get) Token: 0x06000AB2 RID: 2738 RVA: 0x000322CA File Offset: 0x000304CA
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x06000AB3 RID: 2739 RVA: 0x000322D2 File Offset: 0x000304D2
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x06000AB4 RID: 2740 RVA: 0x000322DA File Offset: 0x000304DA
		// (set) Token: 0x06000AB5 RID: 2741 RVA: 0x000322E2 File Offset: 0x000304E2
		public LevelState LevelState { get; private set; }

		// Token: 0x06000AB6 RID: 2742 RVA: 0x000322EC File Offset: 0x000304EC
		public async void Synchronize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Synchronize", Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID, "", null, false, 10);
			this.OnLoadingEnded.Invoke();
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UIAddScreenshotsToLevelModalModel_GetLevel, graphQlResult.GetErrorMessage(0), true, false);
			}
			else
			{
				this.LevelState = LevelStateLoader.Load(graphQlResult.GetDataMember().ToString());
				int num = Mathf.Clamp(this.LevelState.Screenshots.Count, 0, this.screenshotLimit.Value);
				int num2 = Mathf.Clamp(this.screenshotLimit.Value - num, 0, this.screenshotLimit.Value);
				int num3 = Mathf.Clamp(num2, 0, this.inMemoryScreenshotListModel.Count);
				if (this.verboseLogging)
				{
					DebugUtility.Log(string.Format("{0}.{1}: {2}", "LevelState", "Screenshots", this.LevelState.Screenshots.Count), this);
					DebugUtility.Log(string.Format("{0}: {1}", "screenshotLimit", this.screenshotLimit.Value), this);
					DebugUtility.Log(string.Format("{0}: {1}", "screenshotCount", num), this);
					DebugUtility.Log(string.Format("{0}: {1}", "unusedScreenshots", num2), this);
					DebugUtility.Log(string.Format("{0}: {1}", "inMemoryScreenshotListModel", this.inMemoryScreenshotListModel.Count), this);
					DebugUtility.Log(string.Format("{0}: {1}", "autoSelect", num3), this);
				}
				for (int i = 0; i < num3; i++)
				{
					this.inMemoryScreenshotListModel.Select(i, true);
				}
				Action synchronizedAction = UIAddScreenshotsToLevelModalModel.SynchronizedAction;
				if (synchronizedAction != null)
				{
					synchronizedAction();
				}
			}
		}

		// Token: 0x040008FA RID: 2298
		public static Action SynchronizedAction;

		// Token: 0x040008FB RID: 2299
		[SerializeField]
		private IntVariable screenshotLimit;

		// Token: 0x040008FC RID: 2300
		[SerializeField]
		private UIInMemoryScreenshotListModel inMemoryScreenshotListModel;

		// Token: 0x040008FD RID: 2301
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
