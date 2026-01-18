using System;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000108 RID: 264
	public class UIGameAssetListModel : UIBaseLocalFilterableListModel<UIGameAsset>
	{
		// Token: 0x17000062 RID: 98
		// (get) Token: 0x0600043B RID: 1083 RVA: 0x00019A37 File Offset: 0x00017C37
		protected override Comparison<UIGameAsset> DefaultSort
		{
			get
			{
				return (UIGameAsset x, UIGameAsset y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);
			}
		}
	}
}
