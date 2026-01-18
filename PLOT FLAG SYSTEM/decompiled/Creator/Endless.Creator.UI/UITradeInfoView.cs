using Endless.Gameplay;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UITradeInfoView : UIBaseView<TradeInfo, UITradeInfoView.Styles>
{
	public enum Styles
	{
		Default
	}

	[Header("UITradeInfoView")]
	[SerializeField]
	private UIInventoryAndQuantityReferencePresenter cost1;

	[SerializeField]
	private UIInventoryAndQuantityReferencePresenter cost2;

	[SerializeField]
	private UIInventoryAndQuantityReferencePresenter reward1;

	[SerializeField]
	private UIInventoryAndQuantityReferencePresenter reward2;

	[field: Header("UITradeInfoView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public override void View(TradeInfo model)
	{
		cost1.SetModel(model.Cost1, triggerOnModelChanged: false);
		cost2.SetModel(model.Cost2, triggerOnModelChanged: false);
		reward1.SetModel(model.Reward1, triggerOnModelChanged: false);
		reward2.SetModel(model.Reward2, triggerOnModelChanged: false);
	}

	public override void Clear()
	{
		cost1.Clear();
		cost2.Clear();
		reward1.Clear();
		reward2.Clear();
	}
}
