using System;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200023F RID: 575
	public class UIInventoryAndQuantityReferenceView : UIBaseView<TradeInfo.InventoryAndQuantityReference, UIInventoryAndQuantityReferenceView.Styles>, IUIInteractable
	{
		// Token: 0x1400001C RID: 28
		// (add) Token: 0x06000949 RID: 2377 RVA: 0x0002B86C File Offset: 0x00029A6C
		// (remove) Token: 0x0600094A RID: 2378 RVA: 0x0002B8A4 File Offset: 0x00029AA4
		public event Action OpenWindow;

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x0600094B RID: 2379 RVA: 0x0002B8D9 File Offset: 0x00029AD9
		// (set) Token: 0x0600094C RID: 2380 RVA: 0x0002B8E1 File Offset: 0x00029AE1
		public override UIInventoryAndQuantityReferenceView.Styles Style { get; protected set; }

		// Token: 0x0600094D RID: 2381 RVA: 0x0002B8EA File Offset: 0x00029AEA
		private void Start()
		{
			this.openWindowButton.onClick.AddListener(new UnityAction(this.InvokeOpenWindow));
		}

		// Token: 0x0600094E RID: 2382 RVA: 0x0002B908 File Offset: 0x00029B08
		public override void View(TradeInfo.InventoryAndQuantityReference model)
		{
			if (model == null)
			{
				model = new TradeInfo.InventoryAndQuantityReference();
			}
			this.iconImage.enabled = false;
			if (model.IsReferenceEmpty())
			{
				this.stackableQuantityContainer.SetActive(false);
				return;
			}
			SerializableGuid id = InspectorReferenceUtility.GetId(model);
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(id, out runtimePropInfo))
			{
				return;
			}
			this.iconImage.sprite = (runtimePropInfo.IsMissingObject ? this.missingIconSprite : runtimePropInfo.Icon);
			this.iconImage.enabled = true;
			this.stackableQuantityContainer.SetActive(model.Quantity > 1);
			string text = "x" + StringUtility.AbbreviateQuantity(model.Quantity);
			UIText[] array = this.stackableQuantity;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Value = text;
			}
		}

		// Token: 0x0600094F RID: 2383 RVA: 0x0002B9D6 File Offset: 0x00029BD6
		public override void Clear()
		{
			this.iconImage.sprite = null;
			this.stackableQuantityContainer.SetActive(false);
		}

		// Token: 0x06000950 RID: 2384 RVA: 0x0002B9F0 File Offset: 0x00029BF0
		public void SetInteractable(bool interactable)
		{
			this.openWindowButton.interactable = interactable;
		}

		// Token: 0x06000951 RID: 2385 RVA: 0x0002B9FE File Offset: 0x00029BFE
		private void InvokeOpenWindow()
		{
			Action openWindow = this.OpenWindow;
			if (openWindow == null)
			{
				return;
			}
			openWindow();
		}

		// Token: 0x040007A7 RID: 1959
		[Header("UIInventoryAndQuantityReferenceView")]
		[SerializeField]
		private Image iconImage;

		// Token: 0x040007A8 RID: 1960
		[SerializeField]
		private Sprite missingIconSprite;

		// Token: 0x040007A9 RID: 1961
		[SerializeField]
		private GameObject stackableQuantityContainer;

		// Token: 0x040007AA RID: 1962
		[SerializeField]
		private UIText[] stackableQuantity = Array.Empty<UIText>();

		// Token: 0x040007AB RID: 1963
		[SerializeField]
		private UIButton openWindowButton;

		// Token: 0x02000240 RID: 576
		public enum Styles
		{
			// Token: 0x040007AE RID: 1966
			Default
		}
	}
}
