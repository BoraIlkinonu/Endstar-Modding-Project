using Endless.Gameplay;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelDestinationSelectionListCellView : UIBaseListCellView<LevelDestination>
{
	[Header("UILevelDestinationSelectionListCellView")]
	[SerializeField]
	private TextMeshProUGUI levelNameText;

	public override void View(UIBaseListView<LevelDestination> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		if (!IsAddButton)
		{
			UILevelDestinationSelectionListModel uILevelDestinationSelectionListModel = (UILevelDestinationSelectionListModel)base.ListModel;
			levelNameText.text = uILevelDestinationSelectionListModel.GetLevelName(base.Model.TargetLevelId);
		}
	}
}
