using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200014F RID: 335
	public class UIPropEntryListModel : UIBaseLocalFilterableListModel<PropEntry>
	{
		// Token: 0x17000080 RID: 128
		// (get) Token: 0x0600051C RID: 1308 RVA: 0x0001C2F4 File Offset: 0x0001A4F4
		protected override Comparison<PropEntry> DefaultSort
		{
			get
			{
				return (PropEntry x, PropEntry y) => string.Compare(x.Label, y.Label, StringComparison.Ordinal);
			}
		}
	}
}
