using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200029A RID: 666
	public class UIBaseToolPanelController<T> : UIGameObject where T : EndlessTool
	{
		// Token: 0x17000163 RID: 355
		// (get) Token: 0x06000B04 RID: 2820 RVA: 0x00033C41 File Offset: 0x00031E41
		// (set) Token: 0x06000B05 RID: 2821 RVA: 0x00033C49 File Offset: 0x00031E49
		protected bool VerboseLogging { get; set; }

		// Token: 0x06000B06 RID: 2822 RVA: 0x00033C52 File Offset: 0x00031E52
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.Tool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<T>();
		}

		// Token: 0x04000957 RID: 2391
		[Header("UIBaseToolPanelController")]
		[SerializeField]
		protected UIBaseToolPanelView<T> View;

		// Token: 0x04000959 RID: 2393
		protected T Tool;
	}
}
