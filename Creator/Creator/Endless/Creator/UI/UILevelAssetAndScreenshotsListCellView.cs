using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000130 RID: 304
	public class UILevelAssetAndScreenshotsListCellView : UIBaseListCellView<UILevelAssetAndScreenshotsListModelEntry>
	{
		// Token: 0x060004CC RID: 1228 RVA: 0x0001B54F File Offset: 0x0001974F
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.screenshotFileInstancesListModel.Set(new List<ScreenshotFileInstances>(), true);
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x0001B568 File Offset: 0x00019768
		public override void View(UIBaseListView<UILevelAssetAndScreenshotsListModelEntry> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.nameText.text = base.Model.LevelAsset.Name;
			UILevelAssetAndScreenshotsListModel uilevelAssetAndScreenshotsListModel = (UILevelAssetAndScreenshotsListModel)base.ListModel;
			this.screenshotFileInstancesListModel.SetExteriorSelected(uilevelAssetAndScreenshotsListModel.ExteriorSelected);
			this.screenshotFileInstancesListModel.Set(base.Model.LevelAsset.Screenshots, true);
		}

		// Token: 0x04000477 RID: 1143
		[Header("UILevelAssetAndScreenshotsListCellView")]
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x04000478 RID: 1144
		[SerializeField]
		private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;
	}
}
