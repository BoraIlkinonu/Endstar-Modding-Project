using System;
using Endless.Gameplay.LevelEditing;
using Endless.Shared.UI;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000169 RID: 361
	public class UIRuntimePropInfoListView : UIBaseListView<PropLibrary.RuntimePropInfo>
	{
		// Token: 0x17000088 RID: 136
		// (get) Token: 0x0600055E RID: 1374 RVA: 0x0001CF2E File Offset: 0x0001B12E
		// (set) Token: 0x0600055F RID: 1375 RVA: 0x0001CF36 File Offset: 0x0001B136
		public UnityEvent<PropLibrary.RuntimePropInfo> OnCellSelected { get; private set; } = new UnityEvent<PropLibrary.RuntimePropInfo>();
	}
}
