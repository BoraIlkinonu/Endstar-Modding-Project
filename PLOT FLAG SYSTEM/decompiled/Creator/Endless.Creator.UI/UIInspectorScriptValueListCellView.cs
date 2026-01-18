using Endless.Props.Scripting;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInspectorScriptValueListCellView : UIBaseListCellView<InspectorScriptValue>
{
	[Header("UIInspectorScriptValueListCellView")]
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private UIButton removeButton;

	public override void View(UIBaseListView<InspectorScriptValue> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		nameText.text = base.Model.Name;
		removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
	}
}
