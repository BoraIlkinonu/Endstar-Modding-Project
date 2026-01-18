using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000296 RID: 662
	[RequireComponent(typeof(UIToolView))]
	public class UIToolController : UIGameObject
	{
		// Token: 0x06000AF7 RID: 2807 RVA: 0x00033864 File Offset: 0x00031A64
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.setActiveToolButton.onClick.AddListener(new UnityAction(this.SetActiveTool));
			this.endlessTool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(this.toolType);
		}

		// Token: 0x06000AF8 RID: 2808 RVA: 0x000338BC File Offset: 0x00031ABC
		private void SetActiveTool()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetActiveTool", Array.Empty<object>());
			}
			if (MonoBehaviourSingleton<UIWindowManager>.Instance.IsDisplaying && MonoBehaviourSingleton<UIWindowManager>.Instance.Displayed.GetType() == typeof(UIScriptWindowView))
			{
				return;
			}
			MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(this.endlessTool);
		}

		// Token: 0x04000945 RID: 2373
		[SerializeField]
		private ToolType toolType;

		// Token: 0x04000946 RID: 2374
		[SerializeField]
		private UIButton setActiveToolButton;

		// Token: 0x04000947 RID: 2375
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000948 RID: 2376
		private EndlessTool endlessTool;
	}
}
