using System;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000186 RID: 390
	public class UITilesetListCellView : UIBaseListCellView<Tileset>
	{
		// Token: 0x060005BC RID: 1468 RVA: 0x0001DE1F File Offset: 0x0001C01F
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.tilesetPresenter.Clear();
			base.HideSelectedVisuals();
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x0001DE38 File Offset: 0x0001C038
		public override void View(UIBaseListView<Tileset> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			if (this.IsAddButton)
			{
				return;
			}
			this.tilesetPresenter.SetModel(base.Model, true);
			UITilesetListView uitilesetListView = (UITilesetListView)base.ListView;
			if (uitilesetListView.ViewPaintingToolActiveTilesetIndexAsSelect && uitilesetListView.IsTilesetIndexActiveInPaintingTool(base.Model.Index))
			{
				base.ViewSelected(0);
			}
		}

		// Token: 0x0400050C RID: 1292
		[Header("UITilesetListCellView")]
		[SerializeField]
		private UITilesetPresenter tilesetPresenter;
	}
}
