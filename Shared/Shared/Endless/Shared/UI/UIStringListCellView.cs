using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001B9 RID: 441
	public class UIStringListCellView : UIBaseListCellView<string>
	{
		// Token: 0x06000B28 RID: 2856 RVA: 0x000309C0 File Offset: 0x0002EBC0
		public override void View(UIBaseListView<string> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.inputField.SetTextWithoutNotify(base.Model);
			UIStringListModel uistringListModel = (UIStringListModel)base.ListModel;
			this.inputField.interactable = uistringListModel.CanEditEntryValue;
			this.removeButton.gameObject.SetActive(uistringListModel.CanRemove);
		}

		// Token: 0x04000728 RID: 1832
		[Header("UIStringListCellView")]
		[SerializeField]
		private UIInputField inputField;

		// Token: 0x04000729 RID: 1833
		[SerializeField]
		private UIButton removeButton;
	}
}
