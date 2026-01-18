using Endless.Props.Assets;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScriptReferenceListCellView : UIBaseListCellView<ScriptReference>
{
	[Header("UIScriptReferenceListCellView")]
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private UIButton removeButton;

	public override void View(UIBaseListView<ScriptReference> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		displayNameText.text = base.Model.NameInCode;
		removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
	}
}
