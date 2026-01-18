using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI.Test
{
	// Token: 0x020002A0 RID: 672
	public class UITestRearrangeableListModel : UIBaseRearrangeableListModel<int>
	{
		// Token: 0x060010A3 RID: 4259 RVA: 0x00046E18 File Offset: 0x00045018
		[ContextMenu("Generate")]
		private void Generate()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Generate", Array.Empty<object>());
			}
			List<int> list = new List<int>();
			while (this.count > 0)
			{
				list.Add(list.Count + 1);
				this.count--;
			}
			this.Set(list, true);
		}

		// Token: 0x060010A4 RID: 4260 RVA: 0x00046E72 File Offset: 0x00045072
		[ContextMenu("TriggerClear")]
		private void TriggerClear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "TriggerClear", Array.Empty<object>());
			}
			this.Clear(true);
		}

		// Token: 0x04000A83 RID: 2691
		[Header("UITestRearrangeableListModel")]
		[Min(0f)]
		[NonSerialized]
		private int count = 999;
	}
}
