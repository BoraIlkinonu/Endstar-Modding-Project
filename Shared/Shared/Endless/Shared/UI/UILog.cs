using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001AB RID: 427
	public struct UILog
	{
		// Token: 0x06000B07 RID: 2823 RVA: 0x00030600 File Offset: 0x0002E800
		public UILog(string condition, string stackTrace, LogType type, int index)
		{
			this.Condition = condition;
			this.StackTrace = stackTrace;
			this.Type = type;
			this.Index = index;
		}

		// Token: 0x04000714 RID: 1812
		public string Condition;

		// Token: 0x04000715 RID: 1813
		public string StackTrace;

		// Token: 0x04000716 RID: 1814
		public LogType Type;

		// Token: 0x04000717 RID: 1815
		public int Index;
	}
}
