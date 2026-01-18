using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIGameLibraryFilterListView : UIBaseListView<UIGameAssetTypes>
{
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
