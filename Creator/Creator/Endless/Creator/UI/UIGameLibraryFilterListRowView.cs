using System;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000116 RID: 278
	public class UIGameLibraryFilterListRowView : UIBaseListRowView<UIGameAssetTypes>
	{
		// Token: 0x0600046E RID: 1134 RVA: 0x0001A490 File Offset: 0x00018690
		public override void View(UIBaseListView<UIGameAssetTypes> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			base.Cells[0].gameObject.SetActive(dataIndex != 0);
		}
	}
}
