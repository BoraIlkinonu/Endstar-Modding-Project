using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Screenshotting;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200028F RID: 655
	public class UIScreenshotToolView : UIGameObject, IRoleInteractable, IValidatable
	{
		// Token: 0x06000AE3 RID: 2787 RVA: 0x0003327C File Offset: 0x0003147C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.TryGetComponent<UIDisplayAndHideHandler>(out this.displayAndHideHandler);
			this.displayAndHideHandler.SetToHideEnd(true);
			this.screenshotTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<ScreenshotTool>();
			MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(new UnityAction<EndlessTool>(this.OnToolChange));
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.AddListener(new UnityAction(this.DisplayScreenshotCount));
		}

		// Token: 0x06000AE4 RID: 2788 RVA: 0x00033300 File Offset: 0x00031500
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			DebugUtility.DebugHasComponent<UIDisplayAndHideHandler>(base.gameObject);
		}

		// Token: 0x06000AE5 RID: 2789 RVA: 0x00033326 File Offset: 0x00031526
		public void EnableCanvas(bool enabled)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EnableCanvas", new object[] { enabled });
			}
			this.canvas.enabled = enabled;
		}

		// Token: 0x06000AE6 RID: 2790 RVA: 0x00033356 File Offset: 0x00031556
		public void SetLocalUserCanInteract(bool localUserCanInteract)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetLocalUserCanInteract", new object[] { localUserCanInteract });
			}
			this.screenshotButton.interactable = localUserCanInteract;
		}

		// Token: 0x06000AE7 RID: 2791 RVA: 0x00033388 File Offset: 0x00031588
		private void OnToolChange(EndlessTool activeTool)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnToolChange", new object[] { activeTool.name });
			}
			if (activeTool.GetType() == typeof(ScreenshotTool))
			{
				if (!this.displayAndHideHandler.IsDisplaying)
				{
					this.Display();
					return;
				}
			}
			else if (this.displayAndHideHandler.IsDisplaying)
			{
				this.Hide();
			}
		}

		// Token: 0x06000AE8 RID: 2792 RVA: 0x000333F8 File Offset: 0x000315F8
		private async void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			this.displayAndHideHandler.Display();
			this.characterVisibilityToggle.SetIsOn(this.screenshotTool.ScreenshotOptions.HideCharacter, true, true);
			this.screenshotCountText.text = "Loading...";
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID, "", null, false, 10);
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UIScreenshotToolView_GetLevel, graphQlResult.GetErrorMessage(0), true, false);
			}
			else
			{
				this.levelScreenshotCount = LevelStateLoader.Load(graphQlResult.GetDataMember().ToString()).Screenshots.Count;
				this.DisplayScreenshotCount();
			}
		}

		// Token: 0x06000AE9 RID: 2793 RVA: 0x00033430 File Offset: 0x00031630
		private void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.displayAndHideHandler.Hide();
			if (MonoBehaviourSingleton<ScreenshotAPI>.Instance.InMemoryScreenshots.Count > 0)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.screenshotReviewModalView, UIModalManagerStackActions.ClearStack, Array.Empty<object>());
			}
		}

		// Token: 0x06000AEA RID: 2794 RVA: 0x00033488 File Offset: 0x00031688
		private void DisplayScreenshotCount()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayScreenshotCount", Array.Empty<object>());
			}
			int num = Mathf.Clamp(this.screenshotLimit.Value - this.levelScreenshotCount, 0, this.screenshotLimit.Value);
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "levelScreenshotCount", this.levelScreenshotCount), this);
				DebugUtility.Log(string.Format("{0}: {1}", "unusedScreenshots", num), this);
			}
			this.screenshotCountText.text = string.Format("{0}/{1}", MonoBehaviourSingleton<ScreenshotAPI>.Instance.InMemoryScreenshots.Count, num);
			if (MonoBehaviourSingleton<ScreenshotAPI>.Instance.InMemoryScreenshots.Count > 0)
			{
				this.screenshotCountChangedTweens.Tween();
			}
		}

		// Token: 0x0400092A RID: 2346
		[SerializeField]
		private Canvas canvas;

		// Token: 0x0400092B RID: 2347
		[SerializeField]
		private UIToggle characterVisibilityToggle;

		// Token: 0x0400092C RID: 2348
		[SerializeField]
		private IntVariable screenshotLimit;

		// Token: 0x0400092D RID: 2349
		[SerializeField]
		private TextMeshProUGUI screenshotCountText;

		// Token: 0x0400092E RID: 2350
		[SerializeField]
		private TweenCollection screenshotCountChangedTweens;

		// Token: 0x0400092F RID: 2351
		[SerializeField]
		private UIButton screenshotButton;

		// Token: 0x04000930 RID: 2352
		[SerializeField]
		private UIAddScreenshotsToLevelModalView screenshotReviewModalView;

		// Token: 0x04000931 RID: 2353
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000932 RID: 2354
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04000933 RID: 2355
		private ScreenshotTool screenshotTool;

		// Token: 0x04000934 RID: 2356
		private int levelScreenshotCount;
	}
}
