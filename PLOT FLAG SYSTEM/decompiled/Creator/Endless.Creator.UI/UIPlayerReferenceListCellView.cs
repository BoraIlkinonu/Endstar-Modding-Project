using Endless.Gameplay;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPlayerReferenceListCellView : UIBaseListCellView<PlayerReference>
{
	[Header("UIPlayerReferenceListCellView")]
	[SerializeField]
	private UIPlayerReferencePresenter playerReference;

	[SerializeField]
	private UIButton removeButton;

	public override void View(UIBaseListView<PlayerReference> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		if (!IsAddButton)
		{
			playerReference.SetModel(base.Model, triggerOnModelChanged: false);
			removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
		}
	}
}
