using System;
using Endless.Gameplay;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200025A RID: 602
	public class UITradeInfoPresenter : UIBasePresenter<TradeInfo>
	{
		// Token: 0x060009D1 RID: 2513 RVA: 0x0002D5F8 File Offset: 0x0002B7F8
		protected override void Start()
		{
			base.Start();
			this.cost1.OnModelChanged += this.SetCost1;
			this.cost2.OnModelChanged += this.SetCost2;
			this.reward1.OnModelChanged += this.SetReward1;
			this.reward2.OnModelChanged += this.SetReward2;
		}

		// Token: 0x060009D2 RID: 2514 RVA: 0x0002D668 File Offset: 0x0002B868
		private void OnDestroy()
		{
			this.cost1.OnModelChanged -= this.SetCost1;
			this.cost2.OnModelChanged -= this.SetCost2;
			this.reward1.OnModelChanged -= this.SetReward1;
			this.reward2.OnModelChanged -= this.SetReward2;
		}

		// Token: 0x060009D3 RID: 2515 RVA: 0x0002D6D4 File Offset: 0x0002B8D4
		private void SetCost1(object item)
		{
			TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = item as TradeInfo.InventoryAndQuantityReference;
			base.Model.Cost1 = inventoryAndQuantityReference;
			base.InvokeOnModelChanged();
		}

		// Token: 0x060009D4 RID: 2516 RVA: 0x0002D6FC File Offset: 0x0002B8FC
		private void SetCost2(object item)
		{
			TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = item as TradeInfo.InventoryAndQuantityReference;
			base.Model.Cost2 = inventoryAndQuantityReference;
			base.InvokeOnModelChanged();
		}

		// Token: 0x060009D5 RID: 2517 RVA: 0x0002D724 File Offset: 0x0002B924
		private void SetReward1(object item)
		{
			TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = item as TradeInfo.InventoryAndQuantityReference;
			base.Model.Reward1 = inventoryAndQuantityReference;
			base.InvokeOnModelChanged();
		}

		// Token: 0x060009D6 RID: 2518 RVA: 0x0002D74C File Offset: 0x0002B94C
		private void SetReward2(object item)
		{
			TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = item as TradeInfo.InventoryAndQuantityReference;
			base.Model.Reward2 = inventoryAndQuantityReference;
			base.InvokeOnModelChanged();
		}

		// Token: 0x0400080A RID: 2058
		[Header("UITradeInfoPresenter")]
		[SerializeField]
		private UIInventoryAndQuantityReferencePresenter cost1;

		// Token: 0x0400080B RID: 2059
		[SerializeField]
		private UIInventoryAndQuantityReferencePresenter cost2;

		// Token: 0x0400080C RID: 2060
		[SerializeField]
		private UIInventoryAndQuantityReferencePresenter reward1;

		// Token: 0x0400080D RID: 2061
		[SerializeField]
		private UIInventoryAndQuantityReferencePresenter reward2;
	}
}
