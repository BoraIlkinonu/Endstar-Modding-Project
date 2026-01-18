using System;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x02000050 RID: 80
	public class UIClientDataListCellView : UIBaseListCellView<ClientData>
	{
		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000184 RID: 388 RVA: 0x00009DE8 File Offset: 0x00007FE8
		private ClientData LocalClientData
		{
			get
			{
				return MatchmakingClientController.Instance.LocalClientData.Value;
			}
		}

		// Token: 0x06000185 RID: 389 RVA: 0x00009E08 File Offset: 0x00008008
		public override void View(UIBaseListView<ClientData> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			UIClientDataListModel uiclientDataListModel = (UIClientDataListModel)listView.Model;
			ClientData clientData = uiclientDataListModel[dataIndex];
			this.clientDataView.Display(clientData.CoreData);
			this.removeButton.gameObject.SetActive(uiclientDataListModel.CanRemove && !clientData.CoreDataEquals(this.LocalClientData));
		}

		// Token: 0x04000115 RID: 277
		[Header("UIClientDataListCellView")]
		[SerializeField]
		private UIClientDataView clientDataView;

		// Token: 0x04000116 RID: 278
		[SerializeField]
		private UIButton removeButton;
	}
}
