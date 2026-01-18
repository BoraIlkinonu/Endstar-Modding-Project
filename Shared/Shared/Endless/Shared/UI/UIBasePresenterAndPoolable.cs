using System;

namespace Endless.Shared.UI
{
	// Token: 0x0200022C RID: 556
	public class UIBasePresenterAndPoolable
	{
		// Token: 0x06000E22 RID: 3618 RVA: 0x0003D469 File Offset: 0x0003B669
		public UIBasePresenterAndPoolable(IUIPresentable presenter, IPoolableT poolable)
		{
			this.Presenter = presenter;
			this.Poolable = poolable;
		}

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x06000E23 RID: 3619 RVA: 0x0003D47F File Offset: 0x0003B67F
		// (set) Token: 0x06000E24 RID: 3620 RVA: 0x0003D487 File Offset: 0x0003B687
		public IUIPresentable Presenter { get; private set; }

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x06000E25 RID: 3621 RVA: 0x0003D490 File Offset: 0x0003B690
		// (set) Token: 0x06000E26 RID: 3622 RVA: 0x0003D498 File Offset: 0x0003B698
		public IPoolableT Poolable { get; private set; }
	}
}
