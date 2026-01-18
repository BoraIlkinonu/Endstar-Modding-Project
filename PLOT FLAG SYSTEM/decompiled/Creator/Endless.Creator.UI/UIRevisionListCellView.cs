using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIRevisionListCellView : UIBaseListCellView<string>
{
	[Header("UIRevisionListCellView")]
	[SerializeField]
	private TextMeshProUGUI revisionText;

	public override void View(UIBaseListView<string> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		revisionText.text = base.Model;
	}
}
