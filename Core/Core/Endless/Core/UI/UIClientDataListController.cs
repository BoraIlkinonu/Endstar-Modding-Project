using System;
using Endless.Networking;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Core.UI
{
	// Token: 0x02000051 RID: 81
	public class UIClientDataListController : UIBaseLocalFilterableListController<ClientData>
	{
		// Token: 0x06000187 RID: 391 RVA: 0x00009E74 File Offset: 0x00008074
		protected override bool IncludeInFilteredResults(ClientData item)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "IncludeInFilteredResults", new object[] { item });
			}
			string text = item.ToString();
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
