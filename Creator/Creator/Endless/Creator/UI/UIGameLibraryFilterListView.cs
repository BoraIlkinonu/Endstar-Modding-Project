using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000117 RID: 279
	public class UIGameLibraryFilterListView : UIBaseListView<UIGameAssetTypes>
	{
		// Token: 0x06000470 RID: 1136 RVA: 0x0001A4B8 File Offset: 0x000186B8
		public override float GetCellViewSize(int dataIndex)
		{
			if (dataIndex > 0 || base.ActiveCellSourceIsRow)
			{
				return base.GetCellViewSize(dataIndex);
			}
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetCellViewSize", "dataIndex", dataIndex), this);
			}
			return 0f;
		}
	}
}
