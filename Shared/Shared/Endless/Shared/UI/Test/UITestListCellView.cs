using System;
using UnityEngine;

namespace Endless.Shared.UI.Test
{
	// Token: 0x02000299 RID: 665
	public class UITestListCellView : UIBaseListCellView<int>
	{
		// Token: 0x06001091 RID: 4241 RVA: 0x00046BB4 File Offset: 0x00044DB4
		public override void View(UIBaseListView<int> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			int num = listView.Model[dataIndex];
			this.test.View(num);
			this.removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
		}

		// Token: 0x04000A77 RID: 2679
		[Header("UITestListCellView")]
		[SerializeField]
		private UITestView test;

		// Token: 0x04000A78 RID: 2680
		[SerializeField]
		private UIButton removeButton;
	}
}
