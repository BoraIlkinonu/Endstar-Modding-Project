using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000262 RID: 610
	public class UIBookendSpriteAndEnumTabGroup : UISpriteAndEnumTabGroup
	{
		// Token: 0x06000F88 RID: 3976 RVA: 0x00042D34 File Offset: 0x00040F34
		protected override UIBaseTab<SpriteAndEnum> GetTabSource(int index)
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

		// Token: 0x040009E7 RID: 2535
		[Header("UIBookendSpriteAndEnumTabGroup")]
		[SerializeField]
		private UISpriteAndEnumTab beginningTabSource;

		// Token: 0x040009E8 RID: 2536
		[SerializeField]
		private UISpriteAndEnumTab endTabSource;
	}
}
