using System;
using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000291 RID: 657
	public class UIScreenshotCanvasVisibilityHandler : UIGameObject
	{
		// Token: 0x06000AEE RID: 2798 RVA: 0x000336FC File Offset: 0x000318FC
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.AddListener(new UnityAction(this.DisableCanvas));
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.AddListener(new UnityAction(this.EnableCanvas));
		}

		// Token: 0x06000AEF RID: 2799 RVA: 0x00033757 File Offset: 0x00031957
		private void DisableCanvas()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisableCanvas", Array.Empty<object>());
			}
			this.canvas.enabled = false;
		}

		// Token: 0x06000AF0 RID: 2800 RVA: 0x0003377D File Offset: 0x0003197D
		private void EnableCanvas()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EnableCanvas", Array.Empty<object>());
			}
			this.canvas.enabled = true;
		}

		// Token: 0x04000939 RID: 2361
		[SerializeField]
		private Canvas canvas;

		// Token: 0x0400093A RID: 2362
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
