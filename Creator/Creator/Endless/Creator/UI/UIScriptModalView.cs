using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001D8 RID: 472
	public abstract class UIScriptModalView : UIBaseModalView
	{
		// Token: 0x06000718 RID: 1816 RVA: 0x00023DE0 File Offset: 0x00021FE0
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(new UnityAction<EndlessTool>(this.OnToolChange));
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x00023E04 File Offset: 0x00022004
		public override void Close()
		{
			base.Close();
			MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.RemoveListener(new UnityAction<EndlessTool>(this.OnToolChange));
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x00023E28 File Offset: 0x00022028
		private void OnToolChange(EndlessTool activeTool)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnToolChange", new object[] { activeTool.name });
			}
			if (activeTool.GetType() == typeof(PropTool))
			{
				return;
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}
	}
}
