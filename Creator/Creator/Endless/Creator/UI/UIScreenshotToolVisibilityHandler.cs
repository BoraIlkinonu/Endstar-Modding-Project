using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000292 RID: 658
	public class UIScreenshotToolVisibilityHandler : UIGameObject
	{
		// Token: 0x06000AF2 RID: 2802 RVA: 0x000337A3 File Offset: 0x000319A3
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(new UnityAction<EndlessTool>(this.OnToolChange));
		}

		// Token: 0x06000AF3 RID: 2803 RVA: 0x000337D8 File Offset: 0x000319D8
		private void OnToolChange(EndlessTool activeTool)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnToolChange", new object[] { activeTool.ToolTypeName });
			}
			if (activeTool.GetType() == typeof(ScreenshotTool))
			{
				this.displayAndHideHandler.Hide();
				this.canvasGroup.blocksRaycasts = false;
				return;
			}
			this.displayAndHideHandler.Display();
			this.canvasGroup.interactable = true;
			this.canvasGroup.blocksRaycasts = true;
		}

		// Token: 0x0400093B RID: 2363
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x0400093C RID: 2364
		[SerializeField]
		private CanvasGroup canvasGroup;

		// Token: 0x0400093D RID: 2365
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
