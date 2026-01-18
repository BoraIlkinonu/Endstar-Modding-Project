using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000117 RID: 279
	public class UIVerticalCarouselView : UICarouselView
	{
		// Token: 0x060006BA RID: 1722 RVA: 0x0001CB78 File Offset: 0x0001AD78
		protected override Vector2 IndexPosition(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "IndexPosition", new object[] { index });
			}
			float num = base.RectTransform.rect.size.y * (float)index;
			return new Vector2(0f, num);
		}
	}
}
