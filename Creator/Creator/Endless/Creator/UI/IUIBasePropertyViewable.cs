using System;

namespace Endless.Creator.UI
{
	// Token: 0x020001F9 RID: 505
	public interface IUIBasePropertyViewable
	{
		// Token: 0x1400000A RID: 10
		// (add) Token: 0x060007EA RID: 2026
		// (remove) Token: 0x060007EB RID: 2027
		event Action<object> OnUserChangedModel;
	}
}
