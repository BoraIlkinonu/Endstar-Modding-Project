using System;
using System.Collections;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200028D RID: 653
	public class UIScreenshotToolController : UIGameObject, IValidatable
	{
		// Token: 0x06000AD5 RID: 2773 RVA: 0x00032FF8 File Offset: 0x000311F8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.TryGetComponent<UIScreenshotToolView>(out this.view);
			this.screenshotTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<ScreenshotTool>();
			this.closeButton.onClick.AddListener(new UnityAction(this.Close));
			this.hideUiToggle.OnChange.AddListener(new UnityAction<bool>(this.SetHideUi));
			this.hideCharacterToggle.OnChange.AddListener(new UnityAction<bool>(this.SetHideCharacter));
			this.screenshotButton.onClick.AddListener(new UnityAction(this.TakeScreenshot));
		}

		// Token: 0x06000AD6 RID: 2774 RVA: 0x000330AA File Offset: 0x000312AA
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			DebugUtility.DebugHasComponent<UIScreenshotToolView>(base.gameObject);
			this.screenshotEffectTweens.ValidateForNumberOfTweens(1);
		}

		// Token: 0x06000AD7 RID: 2775 RVA: 0x000330DC File Offset: 0x000312DC
		private void Close()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Close", Array.Empty<object>());
			}
			MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
		}

		// Token: 0x06000AD8 RID: 2776 RVA: 0x00033101 File Offset: 0x00031301
		private void SetHideUi(bool hide)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetHideUi", new object[] { hide });
			}
			this.screenshotTool.SetHideUi(hide);
		}

		// Token: 0x06000AD9 RID: 2777 RVA: 0x00033131 File Offset: 0x00031331
		private void SetHideCharacter(bool hide)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetHideCharacter", new object[] { hide });
			}
			this.screenshotTool.SetHideCharacter(hide);
		}

		// Token: 0x06000ADA RID: 2778 RVA: 0x00033164 File Offset: 0x00031364
		private void TakeScreenshot()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TakeScreenshot", Array.Empty<object>());
			}
			this.view.EnableCanvas(false);
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.RequestSimpleScreenshot(this.screenshotTool.ScreenshotOptions);
			base.StartCoroutine(this.TakeScreenshotCoroutine());
		}

		// Token: 0x06000ADB RID: 2779 RVA: 0x000331B8 File Offset: 0x000313B8
		private IEnumerator TakeScreenshotCoroutine()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TakeScreenshotCoroutine", Array.Empty<object>());
			}
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			this.view.EnableCanvas(true);
			this.screenshotEffectTweens.Tween();
			yield break;
		}

		// Token: 0x0400091F RID: 2335
		[SerializeField]
		private UIButton closeButton;

		// Token: 0x04000920 RID: 2336
		[SerializeField]
		private UIToggle hideUiToggle;

		// Token: 0x04000921 RID: 2337
		[SerializeField]
		private UIToggle hideCharacterToggle;

		// Token: 0x04000922 RID: 2338
		[SerializeField]
		private UIButton screenshotButton;

		// Token: 0x04000923 RID: 2339
		[SerializeField]
		private TweenCollection screenshotEffectTweens;

		// Token: 0x04000924 RID: 2340
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000925 RID: 2341
		private UIScreenshotToolView view;

		// Token: 0x04000926 RID: 2342
		private ScreenshotTool screenshotTool;
	}
}
