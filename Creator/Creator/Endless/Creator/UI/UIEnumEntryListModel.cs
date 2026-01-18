using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020000FA RID: 250
	public class UIEnumEntryListModel : UIBaseLocalFilterableListModel<EnumEntry>
	{
		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000409 RID: 1033 RVA: 0x0001933C File Offset: 0x0001753C
		protected override Comparison<EnumEntry> DefaultSort
		{
			get
			{
				return (EnumEntry x, EnumEntry y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);
			}
		}
	}
}
