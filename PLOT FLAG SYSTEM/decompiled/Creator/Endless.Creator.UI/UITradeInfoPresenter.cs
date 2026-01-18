using Endless.Gameplay;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UITradeInfoPresenter : UIBasePresenter<TradeInfo>
{
	[Header("UITradeInfoPresenter")]
	[SerializeField]
	private UIInventoryAndQuantityReferencePresenter cost1;

	[SerializeField]
	private UIInventoryAndQuantityReferencePresenter cost2;

	[SerializeField]
	private UIInventoryAndQuantityReferencePresenter reward1;

	[SerializeField]
	private UIInventoryAndQuantityReferencePresenter reward2;

	protected override void Start()
	{
		base.Start();
		cost1.OnModelChanged += SetCost1;
		cost2.OnModelChanged += SetCost2;
		reward1.OnModelChanged += SetReward1;
		reward2.OnModelChanged += SetReward2;
	}

	private void OnDestroy()
	{
		cost1.OnModelChanged -= SetCost1;
		cost2.OnModelChanged -= SetCost2;
		reward1.OnModelChanged -= SetReward1;
		reward2.OnModelChanged -= SetReward2;
	}

	private void SetCost1(object item)
	{
		TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = item as TradeInfo.InventoryAndQuantityReference;
		base.Model.Cost1 = inventoryAndQuantityReference;
		InvokeOnModelChanged();
	}

	private void SetCost2(object item)
	{
		TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = item as TradeInfo.InventoryAndQuantityReference;
		base.Model.Cost2 = inventoryAndQuantityReference;
		InvokeOnModelChanged();
	}

	private void SetReward1(object item)
	{
		TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = item as TradeInfo.InventoryAndQuantityReference;
		base.Model.Reward1 = inventoryAndQuantityReference;
		InvokeOnModelChanged();
	}

	private void SetReward2(object item)
	{
		TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = item as TradeInfo.InventoryAndQuantityReference;
		base.Model.Reward2 = inventoryAndQuantityReference;
		InvokeOnModelChanged();
	}
}
