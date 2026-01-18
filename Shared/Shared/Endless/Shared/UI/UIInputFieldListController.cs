using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x020001A8 RID: 424
	public class UIInputFieldListController : UIBaseLocalFilterableListController<string>
	{
		// Token: 0x06000AF6 RID: 2806 RVA: 0x000303A0 File Offset: 0x0002E5A0
		protected override bool IncludeInFilteredResults(string item)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "IncludeInFilteredResults", new object[] { item });
			}
			if (item.IsNullOrEmptyOrWhiteSpace())
			{
				return false;
			}
			if (!base.CaseSensitive)
			{
				item = item.ToLower();
			}
			string text = base.StringFilter;
			if (!base.CaseSensitive)
			{
				text = text.ToLower();
			}
			return item.Contains(text);
		}
	}
}
