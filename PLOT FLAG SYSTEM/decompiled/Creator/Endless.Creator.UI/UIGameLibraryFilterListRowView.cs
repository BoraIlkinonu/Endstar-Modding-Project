using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIGameLibraryFilterListRowView : UIBaseListRowView<UIGameAssetTypes>
{
	public override void View(UIBaseListView<UIGameAssetTypes> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		base.Cells[0].gameObject.SetActive(dataIndex != 0);
	}
}
