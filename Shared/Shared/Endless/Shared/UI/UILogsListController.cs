using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x020001AE RID: 430
	public class UILogsListController : UIBaseLocalFilterableListController<UILog>
	{
		// Token: 0x06000B0E RID: 2830 RVA: 0x00030714 File Offset: 0x0002E914
		protected override bool IncludeInFilteredResults(UILog item)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "IncludeInFilteredResults", new object[] { item });
			}
			string text = item.Condition;
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
