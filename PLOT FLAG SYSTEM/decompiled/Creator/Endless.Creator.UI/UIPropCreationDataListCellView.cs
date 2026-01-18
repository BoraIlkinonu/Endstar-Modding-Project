using Endless.Creator.DynamicPropCreation;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPropCreationDataListCellView : UIBaseListCellView<PropCreationData>
{
	[Header("UIPropCreationDataListCellView")]
	[SerializeField]
	private UIPropCreationDataView propCreationData;

	public override void View(UIBaseListView<PropCreationData> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		if (!IsAddButton)
		{
			propCreationData.View(base.Model);
		}
	}
}
