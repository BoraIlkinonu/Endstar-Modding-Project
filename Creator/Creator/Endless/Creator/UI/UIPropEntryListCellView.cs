using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200014E RID: 334
	public class UIPropEntryListCellView : UIBaseListCellView<PropEntry>
	{
		// Token: 0x06000519 RID: 1305 RVA: 0x0001C2BD File Offset: 0x0001A4BD
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.propEntryPresenter.Clear();
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x0001C2D0 File Offset: 0x0001A4D0
		public override void View(UIBaseListView<PropEntry> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.propEntryPresenter.SetModel(base.Model, true);
		}

		// Token: 0x040004A7 RID: 1191
		[Header("UIPropEntryListCellView")]
		[SerializeField]
		private UIPropEntryPresenter propEntryPresenter;
	}
}
