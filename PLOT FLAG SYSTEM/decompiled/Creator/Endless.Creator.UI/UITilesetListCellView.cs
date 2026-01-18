using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UITilesetListCellView : UIBaseListCellView<Tileset>
{
	[Header("UITilesetListCellView")]
	[SerializeField]
	private UITilesetPresenter tilesetPresenter;

	public override void OnDespawn()
	{
		base.OnDespawn();
		tilesetPresenter.Clear();
		HideSelectedVisuals();
	}

	public override void View(UIBaseListView<Tileset> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		if (!IsAddButton)
		{
			tilesetPresenter.SetModel(base.Model, triggerOnModelChanged: true);
			UITilesetListView uITilesetListView = (UITilesetListView)base.ListView;
			if (uITilesetListView.ViewPaintingToolActiveTilesetIndexAsSelect && uITilesetListView.IsTilesetIndexActiveInPaintingTool(base.Model.Index))
			{
				ViewSelected(0);
			}
		}
	}
}
