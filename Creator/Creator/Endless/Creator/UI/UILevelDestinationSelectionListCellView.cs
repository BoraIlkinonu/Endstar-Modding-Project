using System;
using Endless.Gameplay;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000136 RID: 310
	public class UILevelDestinationSelectionListCellView : UIBaseListCellView<LevelDestination>
	{
		// Token: 0x060004DE RID: 1246 RVA: 0x0001B78C File Offset: 0x0001998C
		public override void View(UIBaseListView<LevelDestination> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			if (this.IsAddButton)
			{
				return;
			}
			UILevelDestinationSelectionListModel uilevelDestinationSelectionListModel = (UILevelDestinationSelectionListModel)base.ListModel;
			this.levelNameText.text = uilevelDestinationSelectionListModel.GetLevelName(base.Model.TargetLevelId);
		}

		// Token: 0x0400047F RID: 1151
		[Header("UILevelDestinationSelectionListCellView")]
		[SerializeField]
		private TextMeshProUGUI levelNameText;
	}
}
