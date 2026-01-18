using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200023E RID: 574
	public class UIInventoryAndQuantityReferencePresenter : UIBasePresenter<TradeInfo.InventoryAndQuantityReference>
	{
		// Token: 0x06000943 RID: 2371 RVA: 0x0002B771 File Offset: 0x00029971
		protected override void Start()
		{
			base.Start();
			this.UIInventoryAndQuantityReferenceView.OpenWindow += this.OpenWindow;
		}

		// Token: 0x06000944 RID: 2372 RVA: 0x0002B790 File Offset: 0x00029990
		private void OnDestroy()
		{
			this.UIInventoryAndQuantityReferenceView.OpenWindow -= this.OpenWindow;
			this.Clear();
		}

		// Token: 0x06000945 RID: 2373 RVA: 0x0002B7AF File Offset: 0x000299AF
		public override void Clear()
		{
			base.Clear();
			if (this.inventoryAndQuantityReferenceWindow)
			{
				this.inventoryAndQuantityReferenceWindow.Close();
			}
		}

		// Token: 0x06000946 RID: 2374 RVA: 0x0002B7D0 File Offset: 0x000299D0
		private void OpenWindow()
		{
			if (this.inventoryAndQuantityReferenceWindow || MonoBehaviourSingleton<UIWindowManager>.Instance.IsDisplayingType<UIInventoryAndQuantityReferenceWindowView>())
			{
				return;
			}
			this.inventoryAndQuantityReferenceWindow = UIInventoryAndQuantityReferenceWindowView.Display(base.Model, new Action<TradeInfo.InventoryAndQuantityReference>(base.SetModelAndTriggerOnModelChanged), null);
			this.inventoryAndQuantityReferenceWindow.CloseUnityEvent.AddListener(new UnityAction(this.RemoveWindowListenerAndClearReference));
		}

		// Token: 0x06000947 RID: 2375 RVA: 0x0002B831 File Offset: 0x00029A31
		private void RemoveWindowListenerAndClearReference()
		{
			if (!this.inventoryAndQuantityReferenceWindow)
			{
				return;
			}
			this.inventoryAndQuantityReferenceWindow.CloseUnityEvent.RemoveListener(new UnityAction(this.RemoveWindowListenerAndClearReference));
			this.inventoryAndQuantityReferenceWindow = null;
		}

		// Token: 0x040007A4 RID: 1956
		[Header("UIInventoryAndQuantityReferencePresenter")]
		[SerializeField]
		private UIInventoryAndQuantityReferenceView UIInventoryAndQuantityReferenceView;

		// Token: 0x040007A5 RID: 1957
		private UIInventoryAndQuantityReferenceWindowView inventoryAndQuantityReferenceWindow;
	}
}
