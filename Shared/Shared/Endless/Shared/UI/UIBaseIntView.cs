using System;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000214 RID: 532
	public abstract class UIBaseIntView : UIBaseIntNumericView<int, UIBaseIntView.Styles>
	{
		// Token: 0x17000285 RID: 645
		// (get) Token: 0x06000DC2 RID: 3522 RVA: 0x0003C29D File Offset: 0x0003A49D
		// (set) Token: 0x06000DC3 RID: 3523 RVA: 0x0003C2A5 File Offset: 0x0003A4A5
		public override UIBaseIntView.Styles Style { get; protected set; }

		// Token: 0x17000286 RID: 646
		// (get) Token: 0x06000DC4 RID: 3524 RVA: 0x0003C2AE File Offset: 0x0003A4AE
		public UnityEvent<int> OnValueChanged { get; } = new UnityEvent<int>();

		// Token: 0x02000215 RID: 533
		public enum Styles
		{
			// Token: 0x040008D2 RID: 2258
			Default,
			// Token: 0x040008D3 RID: 2259
			Hearts
		}
	}
}
