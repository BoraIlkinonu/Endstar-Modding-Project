using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003F2 RID: 1010
	public abstract class UIBaseSocialView<T> : UIBaseView<T, UIBaseSocialView<T>.Styles>
	{
		// Token: 0x17000527 RID: 1319
		// (get) Token: 0x06001947 RID: 6471 RVA: 0x00074B32 File Offset: 0x00072D32
		// (set) Token: 0x06001948 RID: 6472 RVA: 0x00074B3A File Offset: 0x00072D3A
		public override UIBaseSocialView<T>.Styles Style { get; protected set; }

		// Token: 0x06001949 RID: 6473 RVA: 0x00074B43 File Offset: 0x00072D43
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
		}

		// Token: 0x020003F3 RID: 1011
		public enum Styles
		{
			// Token: 0x04001434 RID: 5172
			LineItem,
			// Token: 0x04001435 RID: 5173
			Card,
			// Token: 0x04001436 RID: 5174
			PortraitOnly
		}
	}
}
