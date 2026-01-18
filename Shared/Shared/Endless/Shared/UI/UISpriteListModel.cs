using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001B5 RID: 437
	public class UISpriteListModel : UIBaseLocalFilterableListModel<Sprite>
	{
		// Token: 0x1700021A RID: 538
		// (get) Token: 0x06000B1E RID: 2846 RVA: 0x000308A9 File Offset: 0x0002EAA9
		protected override Comparison<Sprite> DefaultSort
		{
			get
			{
				return (Sprite x, Sprite y) => string.Compare(x.name, y.name, StringComparison.Ordinal);
			}
		}
	}
}
