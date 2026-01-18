using System;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIPropCreationDataListModel : UIBaseLocalFilterableListModel<PropCreationData>
{
	protected override Comparison<PropCreationData> DefaultSort => delegate(PropCreationData x, PropCreationData y)
	{
		if (x == null && y == null)
		{
			return 0;
		}
		if (x == null)
		{
			return -1;
		}
		return (y == null) ? 1 : string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
	};
}
