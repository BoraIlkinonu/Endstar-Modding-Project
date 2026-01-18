using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000264 RID: 612
	public class UIBookendStringTabGroup : UIStringTabGroup
	{
		// Token: 0x06000F8C RID: 3980 RVA: 0x00042DEC File Offset: 0x00040FEC
		protected override UIBaseTab<string> GetTabSource(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetTabSource", new object[] { index });
			}
			if (index == 0)
			{
				return this.beginningTabSource;
			}
			if (index == base.OptionsLength - 1)
			{
				return this.endTabSource;
			}
			return base.GetTabSource(index);
		}

		// Token: 0x040009EB RID: 2539
		[Header("UIBookendStringTabGroup")]
		[SerializeField]
		private UIStringTab beginningTabSource;

		// Token: 0x040009EC RID: 2540
		[SerializeField]
		private UIStringTab endTabSource;
	}
}
