using Endless.Props.Scripting;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIEnumEntryListCellView : UIBaseListCellView<EnumEntry>
{
	[Header("UIEnumEntryListCellView")]
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private UIButton removeButton;

	public override void View(UIBaseListView<EnumEntry> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		nameText.text = base.Model.Name;
		removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
	}
}
