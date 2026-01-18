using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000263 RID: 611
	public class UIBookendSpriteAndStringTabGroup : UISpriteAndStringTabGroup
	{
		// Token: 0x06000F8A RID: 3978 RVA: 0x00042D90 File Offset: 0x00040F90
		protected override UIBaseTab<SpriteAndString> GetTabSource(int index)
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

		// Token: 0x040009E9 RID: 2537
		[Header("UIBookendSpriteAndStringTabGroup")]
		[SerializeField]
		private UISpriteAndStringTab beginningTabSource;

		// Token: 0x040009EA RID: 2538
		[SerializeField]
		private UISpriteAndStringTab endTabSource;
	}
}
