using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInventoryAndQuantityReferencePresenter : UIBasePresenter<TradeInfo.InventoryAndQuantityReference>
{
	[Header("UIInventoryAndQuantityReferencePresenter")]
	[SerializeField]
	private UIInventoryAndQuantityReferenceView UIInventoryAndQuantityReferenceView;

	private UIInventoryAndQuantityReferenceWindowView inventoryAndQuantityReferenceWindow;

	protected override void Start()
	{
		base.Start();
		UIInventoryAndQuantityReferenceView.OpenWindow += OpenWindow;
	}

	private void OnDestroy()
	{
		UIInventoryAndQuantityReferenceView.OpenWindow -= OpenWindow;
		Clear();
	}

	public override void Clear()
	{
		base.Clear();
		if ((bool)inventoryAndQuantityReferenceWindow)
		{
			inventoryAndQuantityReferenceWindow.Close();
		}
	}

	private void OpenWindow()
	{
		if (!inventoryAndQuantityReferenceWindow && !MonoBehaviourSingleton<UIWindowManager>.Instance.IsDisplayingType<UIInventoryAndQuantityReferenceWindowView>())
		{
			inventoryAndQuantityReferenceWindow = UIInventoryAndQuantityReferenceWindowView.Display(base.Model, base.SetModelAndTriggerOnModelChanged);
			inventoryAndQuantityReferenceWindow.CloseUnityEvent.AddListener(RemoveWindowListenerAndClearReference);
		}
	}

	private void RemoveWindowListenerAndClearReference()
	{
		if ((bool)inventoryAndQuantityReferenceWindow)
		{
			inventoryAndQuantityReferenceWindow.CloseUnityEvent.RemoveListener(RemoveWindowListenerAndClearReference);
			inventoryAndQuantityReferenceWindow = null;
		}
	}
}
