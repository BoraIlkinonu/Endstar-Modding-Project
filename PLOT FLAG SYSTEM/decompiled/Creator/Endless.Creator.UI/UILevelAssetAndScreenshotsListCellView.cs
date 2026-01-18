using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelAssetAndScreenshotsListCellView : UIBaseListCellView<UILevelAssetAndScreenshotsListModelEntry>
{
	[Header("UILevelAssetAndScreenshotsListCellView")]
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;

	public override void OnDespawn()
	{
		base.OnDespawn();
		screenshotFileInstancesListModel.Set(new List<ScreenshotFileInstances>(), triggerEvents: true);
	}

	public override void View(UIBaseListView<UILevelAssetAndScreenshotsListModelEntry> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		nameText.text = base.Model.LevelAsset.Name;
		UILevelAssetAndScreenshotsListModel uILevelAssetAndScreenshotsListModel = (UILevelAssetAndScreenshotsListModel)base.ListModel;
		screenshotFileInstancesListModel.SetExteriorSelected(uILevelAssetAndScreenshotsListModel.ExteriorSelected);
		screenshotFileInstancesListModel.Set(base.Model.LevelAsset.Screenshots, triggerEvents: true);
	}
}
