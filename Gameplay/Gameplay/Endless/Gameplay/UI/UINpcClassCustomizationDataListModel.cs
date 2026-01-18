using System;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003CE RID: 974
	public class UINpcClassCustomizationDataListModel : UIBaseLocalFilterableListModel<NpcClassCustomizationData>
	{
		// Token: 0x1700050C RID: 1292
		// (get) Token: 0x060018A7 RID: 6311 RVA: 0x0007249C File Offset: 0x0007069C
		protected override Comparison<NpcClassCustomizationData> DefaultSort
		{
			get
			{
				return (NpcClassCustomizationData x, NpcClassCustomizationData y) => string.Compare(x.ClassName, y.ClassName, StringComparison.Ordinal);
			}
		}
	}
}
