using System;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000FF RID: 255
	public class UIGameAssetListCellView : UIBaseListCellView<UIGameAsset>
	{
		// Token: 0x06000417 RID: 1047 RVA: 0x00019588 File Offset: 0x00017788
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.gameAssetView.Clear();
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x0001959C File Offset: 0x0001779C
		public override void View(UIBaseListView<UIGameAsset> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			if (this.IsAddButton)
			{
				return;
			}
			this.gameAssetView.View(base.Model);
			this.gameAssetView.SetBackgroundColor(dataIndex % 2 == 0);
			if ((listView as UIGameAssetListView).ViewInLibraryMarker && base.Model != null)
			{
				this.gameAssetView.ViewInLibraryMarker(base.Model);
			}
		}

		// Token: 0x04000420 RID: 1056
		[Header("UIGameAssetListCellView")]
		[SerializeField]
		private UIGameAssetSummaryView gameAssetView;
	}
}
