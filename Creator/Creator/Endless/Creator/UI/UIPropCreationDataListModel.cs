using System;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200014A RID: 330
	public class UIPropCreationDataListModel : UIBaseLocalFilterableListModel<PropCreationData>
	{
		// Token: 0x1700007F RID: 127
		// (get) Token: 0x0600050F RID: 1295 RVA: 0x0001C1BF File Offset: 0x0001A3BF
		protected override Comparison<PropCreationData> DefaultSort
		{
			get
			{
				return delegate(PropCreationData x, PropCreationData y)
				{
					if (x == null && y == null)
					{
						return 0;
					}
					if (x == null)
					{
						return -1;
					}
					if (y == null)
					{
						return 1;
					}
					return string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
				};
			}
		}
	}
}
