using System;
using Endless.Gameplay;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200025B RID: 603
	public class UITradeInfoView : UIBaseView<TradeInfo, UITradeInfoView.Styles>
	{
		// Token: 0x1700013D RID: 317
		// (get) Token: 0x060009D8 RID: 2520 RVA: 0x0002D77A File Offset: 0x0002B97A
		// (set) Token: 0x060009D9 RID: 2521 RVA: 0x0002D782 File Offset: 0x0002B982
		public override UITradeInfoView.Styles Style { get; protected set; }

		// Token: 0x060009DA RID: 2522 RVA: 0x0002D78C File Offset: 0x0002B98C
		public override void View(TradeInfo model)
		{
			this.cost1.SetModel(model.Cost1, false);
			this.cost2.SetModel(model.Cost2, false);
			this.reward1.SetModel(model.Reward1, false);
			this.reward2.SetModel(model.Reward2, false);
		}

		// Token: 0x060009DB RID: 2523 RVA: 0x0002D7E1 File Offset: 0x0002B9E1
		public override void Clear()
		{
			this.cost1.Clear();
			this.cost2.Clear();
			this.reward1.Clear();
			this.reward2.Clear();
		}

		// Token: 0x0400080E RID: 2062
		[Header("UITradeInfoView")]
		[SerializeField]
		private UIInventoryAndQuantityReferencePresenter cost1;

		// Token: 0x0400080F RID: 2063
		[SerializeField]
		private UIInventoryAndQuantityReferencePresenter cost2;

		// Token: 0x04000810 RID: 2064
		[SerializeField]
		private UIInventoryAndQuantityReferencePresenter reward1;

		// Token: 0x04000811 RID: 2065
		[SerializeField]
		private UIInventoryAndQuantityReferencePresenter reward2;

		// Token: 0x0200025C RID: 604
		public enum Styles
		{
			// Token: 0x04000814 RID: 2068
			Default
		}
	}
}
