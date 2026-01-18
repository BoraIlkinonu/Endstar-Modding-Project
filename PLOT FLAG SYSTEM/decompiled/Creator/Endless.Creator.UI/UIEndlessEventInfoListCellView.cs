using Endless.Props.Scripting;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIEndlessEventInfoListCellView : UIBaseListCellView<EndlessEventInfo>
{
	[Header("UIEndlessEventInfoListCellView")]
	[SerializeField]
	private TextMeshProUGUI memberNameText;

	[SerializeField]
	private UIButton removeButton;

	[SerializeField]
	private Color emitterColor;

	[SerializeField]
	private Color receiverColor;

	public override void View(UIBaseListView<EndlessEventInfo> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		memberNameText.text = base.Model.MemberName;
		removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
		UIEndlessEventInfoListModel uIEndlessEventInfoListModel = (UIEndlessEventInfoListModel)base.ListModel;
		memberNameText.color = ((uIEndlessEventInfoListModel.Type == UIEndlessEventInfoListModel.Types.Emitter) ? emitterColor : receiverColor);
	}
}
