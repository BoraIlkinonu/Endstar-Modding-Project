using System;

namespace Endless.Creator.UI
{
	// Token: 0x0200021D RID: 541
	public interface IInspectorReferenceViewable
	{
		// Token: 0x14000015 RID: 21
		// (add) Token: 0x060008BA RID: 2234
		// (remove) Token: 0x060008BB RID: 2235
		event Action OnClear;

		// Token: 0x14000016 RID: 22
		// (add) Token: 0x060008BC RID: 2236
		// (remove) Token: 0x060008BD RID: 2237
		event Action OnOpenIEnumerableWindow;
	}
}
