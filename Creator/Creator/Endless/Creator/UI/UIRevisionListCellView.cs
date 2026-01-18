using System;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000153 RID: 339
	public class UIRevisionListCellView : UIBaseListCellView<string>
	{
		// Token: 0x06000523 RID: 1315 RVA: 0x0001C34D File Offset: 0x0001A54D
		public override void View(UIBaseListView<string> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.revisionText.text = base.Model;
		}

		// Token: 0x040004AA RID: 1194
		[Header("UIRevisionListCellView")]
		[SerializeField]
		private TextMeshProUGUI revisionText;
	}
}
