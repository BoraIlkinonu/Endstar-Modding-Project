using System;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200018D RID: 397
	public class UITransformIdentifierListCellView : UIBaseListCellView<UITransformIdentifier>
	{
		// Token: 0x060005D1 RID: 1489 RVA: 0x0001E06D File Offset: 0x0001C26D
		public override void View(UIBaseListView<UITransformIdentifier> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.uniqueIdText.text = base.Model.DisplayName;
		}

		// Token: 0x04000513 RID: 1299
		[Header("UIClientCopyHistoryEntryListCellView")]
		[SerializeField]
		private TextMeshProUGUI uniqueIdText;
	}
}
