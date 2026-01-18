using System;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000149 RID: 329
	public class UIPropCreationDataListCellView : UIBaseListCellView<PropCreationData>
	{
		// Token: 0x0600050D RID: 1293 RVA: 0x0001C193 File Offset: 0x0001A393
		public override void View(UIBaseListView<PropCreationData> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			if (this.IsAddButton)
			{
				return;
			}
			this.propCreationData.View(base.Model);
		}

		// Token: 0x040004A3 RID: 1187
		[Header("UIPropCreationDataListCellView")]
		[SerializeField]
		private UIPropCreationDataView propCreationData;
	}
}
