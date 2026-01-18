using System;
using Endless.Shared;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200015E RID: 350
	public class UIRolesListCellView : UIBaseListCellView<Roles>
	{
		// Token: 0x0600053D RID: 1341 RVA: 0x0001C790 File Offset: 0x0001A990
		public override void View(UIBaseListView<Roles> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.roleText.text = base.Model.ToString();
			this.descriptionText.text = this.rolesDescriptionsDictionary[base.Model].Game;
			UIRolesListModel uirolesListModel = (UIRolesListModel)base.ListModel;
			bool flag = uirolesListModel.LocalClientRole == Roles.Owner || uirolesListModel.LocalClientRole.IsGreaterThanOrEqualTo(base.Model);
			this.permissionDeniedCover.gameObject.SetActive(!flag);
		}

		// Token: 0x040004BD RID: 1213
		[Header("UIRolesListCellView")]
		[SerializeField]
		private TextMeshProUGUI roleText;

		// Token: 0x040004BE RID: 1214
		[SerializeField]
		private TextMeshProUGUI descriptionText;

		// Token: 0x040004BF RID: 1215
		[SerializeField]
		private UIRolesDescriptionsDictionary rolesDescriptionsDictionary;

		// Token: 0x040004C0 RID: 1216
		[SerializeField]
		private GameObject permissionDeniedCover;
	}
}
