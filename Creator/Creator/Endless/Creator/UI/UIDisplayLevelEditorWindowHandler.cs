using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002D7 RID: 727
	public class UIDisplayLevelEditorWindowHandler : UIGameObject
	{
		// Token: 0x06000C57 RID: 3159 RVA: 0x0003B1B1 File Offset: 0x000393B1
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(new UnityAction<EndlessTool>(this.OnToolChange));
		}

		// Token: 0x06000C58 RID: 3160 RVA: 0x0003B1E8 File Offset: 0x000393E8
		private void OnToolChange(EndlessTool activeTool)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnToolChange", new object[] { activeTool.ToolTypeName });
			}
			if (activeTool.GetType() == typeof(LevelEditorTool))
			{
				UILevelEditorWindowView.Display(this.windowParent);
				return;
			}
			MonoBehaviourSingleton<UIWindowManager>.Instance.CloseAllInstancesOf<UILevelEditorWindowView>();
		}

		// Token: 0x04000AA5 RID: 2725
		[SerializeField]
		private Transform windowParent;

		// Token: 0x04000AA6 RID: 2726
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
