using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameAssetListCellView : UIBaseListCellView<UIGameAsset>
{
	[Header("UIGameAssetListCellView")]
	[SerializeField]
	private UIGameAssetSummaryView gameAssetView;

	public override void OnDespawn()
	{
		base.OnDespawn();
		gameAssetView.Clear();
	}

	public override void View(UIBaseListView<UIGameAsset> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		if (!IsAddButton)
		{
			gameAssetView.View(base.Model);
			gameAssetView.SetBackgroundColor(dataIndex % 2 == 0);
			if ((listView as UIGameAssetListView).ViewInLibraryMarker && base.Model != null)
			{
				gameAssetView.ViewInLibraryMarker(base.Model);
			}
		}
	}
}
