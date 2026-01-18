using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000116 RID: 278
	public class UIHorizontalCarouselView : UICarouselView
	{
		// Token: 0x060006B8 RID: 1720 RVA: 0x0001CB14 File Offset: 0x0001AD14
		protected override Vector2 IndexPosition(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "IndexPosition", new object[] { index });
			}
			return new Vector2(base.RectTransform.rect.size.x * (float)index * -1f, 0f);
		}
	}
}
