using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UITransformIdentifierListCellView : UIBaseListCellView<UITransformIdentifier>
{
	[Header("UIClientCopyHistoryEntryListCellView")]
	[SerializeField]
	private TextMeshProUGUI uniqueIdText;

	public override void View(UIBaseListView<UITransformIdentifier> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		uniqueIdText.text = base.Model.DisplayName;
	}
}
