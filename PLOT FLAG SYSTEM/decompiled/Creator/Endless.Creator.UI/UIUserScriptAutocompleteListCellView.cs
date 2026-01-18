using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIUserScriptAutocompleteListCellView : UIBaseListCellView<UIUserScriptAutocompleteListModelItem>
{
	[Header("UIUserScriptAutocompleteListCellView")]
	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private UITokenGroupTypeSpriteDictionary tokenGroupTypeSpriteDictionary;

	[SerializeField]
	private TextMeshProUGUI autoCompleteText;

	public override void View(UIBaseListView<UIUserScriptAutocompleteListModelItem> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		iconImage.sprite = tokenGroupTypeSpriteDictionary[base.Model.Type];
		autoCompleteText.text = base.Model.Value;
	}
}
