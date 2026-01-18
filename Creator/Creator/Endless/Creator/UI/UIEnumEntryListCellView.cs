using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000F9 RID: 249
	public class UIEnumEntryListCellView : UIBaseListCellView<EnumEntry>
	{
		// Token: 0x06000407 RID: 1031 RVA: 0x000192F9 File Offset: 0x000174F9
		public override void View(UIBaseListView<EnumEntry> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.nameText.text = base.Model.Name;
			this.removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
		}

		// Token: 0x04000417 RID: 1047
		[Header("UIEnumEntryListCellView")]
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x04000418 RID: 1048
		[SerializeField]
		private UIButton removeButton;
	}
}
