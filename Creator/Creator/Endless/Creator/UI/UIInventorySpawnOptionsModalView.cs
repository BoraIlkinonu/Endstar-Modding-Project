using System;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001B9 RID: 441
	public class UIInventorySpawnOptionsModalView : UIEscapableModalView
	{
		// Token: 0x06000690 RID: 1680 RVA: 0x00021DA3 File Offset: 0x0001FFA3
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				Debug.Log("OnDisable", this);
			}
			this.inventorySpawnOptionsListModel.Clear(true);
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x00021DC4 File Offset: 0x0001FFC4
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			UIInventorySpawnOptionsPresenter uiinventorySpawnOptionsPresenter = modalData[0] as UIInventorySpawnOptionsPresenter;
			this.inventorySpawnOptionsListModel.Initialize(uiinventorySpawnOptionsPresenter);
		}

		// Token: 0x040005E4 RID: 1508
		[SerializeField]
		private UIInventorySpawnOptionsListModel inventorySpawnOptionsListModel;
	}
}
