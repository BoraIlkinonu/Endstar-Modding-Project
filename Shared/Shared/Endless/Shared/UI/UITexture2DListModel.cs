using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001BE RID: 446
	public class UITexture2DListModel : UIBaseLocalFilterableListModel<Texture2D>
	{
		// Token: 0x1700021F RID: 543
		// (get) Token: 0x06000B3C RID: 2876 RVA: 0x00030BA2 File Offset: 0x0002EDA2
		protected override Comparison<Texture2D> DefaultSort
		{
			get
			{
				return (Texture2D x, Texture2D y) => string.Compare(x.name, y.name, StringComparison.Ordinal);
			}
		}
	}
}
