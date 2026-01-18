using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000F0 RID: 240
	public class UIEndlessEventInfoListCellView : UIBaseListCellView<EndlessEventInfo>
	{
		// Token: 0x060003F6 RID: 1014 RVA: 0x000190C4 File Offset: 0x000172C4
		public override void View(UIBaseListView<EndlessEventInfo> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.memberNameText.text = base.Model.MemberName;
			this.removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
			UIEndlessEventInfoListModel uiendlessEventInfoListModel = (UIEndlessEventInfoListModel)base.ListModel;
			this.memberNameText.color = ((uiendlessEventInfoListModel.Type == UIEndlessEventInfoListModel.Types.Emitter) ? this.emitterColor : this.receiverColor);
		}

		// Token: 0x0400040A RID: 1034
		[Header("UIEndlessEventInfoListCellView")]
		[SerializeField]
		private TextMeshProUGUI memberNameText;

		// Token: 0x0400040B RID: 1035
		[SerializeField]
		private UIButton removeButton;

		// Token: 0x0400040C RID: 1036
		[SerializeField]
		private Color emitterColor;

		// Token: 0x0400040D RID: 1037
		[SerializeField]
		private Color receiverColor;
	}
}
