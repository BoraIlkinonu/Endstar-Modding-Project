using System;
using Endless.Gameplay;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000145 RID: 325
	public class UIPlayerReferenceListCellView : UIBaseListCellView<PlayerReference>
	{
		// Token: 0x06000505 RID: 1285 RVA: 0x0001C004 File Offset: 0x0001A204
		public override void View(UIBaseListView<PlayerReference> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			if (this.IsAddButton)
			{
				return;
			}
			this.playerReference.SetModel(base.Model, false);
			this.removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
		}

		// Token: 0x0400049C RID: 1180
		[Header("UIPlayerReferenceListCellView")]
		[SerializeField]
		private UIPlayerReferencePresenter playerReference;

		// Token: 0x0400049D RID: 1181
		[SerializeField]
		private UIButton removeButton;
	}
}
