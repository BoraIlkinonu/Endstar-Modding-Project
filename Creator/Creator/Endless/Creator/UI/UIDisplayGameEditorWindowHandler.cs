using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002CD RID: 717
	public class UIDisplayGameEditorWindowHandler : UIGameObject
	{
		// Token: 0x06000C1E RID: 3102 RVA: 0x00039FDC File Offset: 0x000381DC
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(new UnityAction<EndlessTool>(this.OnToolChange));
		}

		// Token: 0x06000C1F RID: 3103 RVA: 0x0003A014 File Offset: 0x00038214
		private void OnToolChange(EndlessTool activeTool)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnToolChange", new object[] { activeTool.ToolTypeName });
			}
			if (activeTool.GetType() == typeof(GameEditorTool))
			{
				UIGameEditorWindowView.Display(this.windowParent);
				return;
			}
			MonoBehaviourSingleton<UIWindowManager>.Instance.CloseAllInstancesOf<UIGameEditorWindowView>();
		}

		// Token: 0x04000A77 RID: 2679
		[SerializeField]
		private Transform windowParent;

		// Token: 0x04000A78 RID: 2680
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
