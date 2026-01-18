using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000102 RID: 258
	public class UIGameLibraryListController : UIBaseLocalFilterableListController<UIGameAsset>
	{
		// Token: 0x0600041C RID: 1052 RVA: 0x00019620 File Offset: 0x00017820
		protected override bool IncludeInFilteredResults(UIGameAsset item)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "IncludeInFilteredResults", new object[] { item });
			}
			string text = item.Name;
			if (!base.CaseSensitive)
			{
				text = text.ToLower();
			}
			string text2 = base.StringFilter;
			if (!base.CaseSensitive)
			{
				text2 = text2.ToLower();
			}
			return text.Contains(text2);
		}
	}
}
