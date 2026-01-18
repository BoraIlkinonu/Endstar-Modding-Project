using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200011D RID: 285
	[Serializable]
	public struct UIToggleGroupOption
	{
		// Token: 0x06000706 RID: 1798 RVA: 0x0001DCE6 File Offset: 0x0001BEE6
		public UIToggleGroupOption(string key)
		{
			this.Key = key;
			this.Icon = null;
		}

		// Token: 0x06000707 RID: 1799 RVA: 0x0001DCF6 File Offset: 0x0001BEF6
		public UIToggleGroupOption(string key, Sprite icon)
		{
			this.Key = key;
			this.Icon = icon;
		}

		// Token: 0x04000416 RID: 1046
		public string Key;

		// Token: 0x04000417 RID: 1047
		public Sprite Icon;
	}
}
