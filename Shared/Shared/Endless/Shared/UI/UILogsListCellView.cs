using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001AD RID: 429
	public class UILogsListCellView : UIBaseListCellView<UILog>
	{
		// Token: 0x06000B0C RID: 2828 RVA: 0x0003069C File Offset: 0x0002E89C
		public override void View(UIBaseListView<UILog> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.logTypeBackgroundImage.color = this.logTypeColorDictionary[base.Model.Type];
			this.logTypeText.text = base.Model.Type.ToString();
			this.conditionText.text = base.Model.Condition;
		}

		// Token: 0x0400071A RID: 1818
		[SerializeField]
		private TextMeshProUGUI logTypeText;

		// Token: 0x0400071B RID: 1819
		[SerializeField]
		private TextMeshProUGUI conditionText;

		// Token: 0x0400071C RID: 1820
		[SerializeField]
		private Image logTypeBackgroundImage;

		// Token: 0x0400071D RID: 1821
		[SerializeField]
		private UILogTypeColorDictionary logTypeColorDictionary;
	}
}
