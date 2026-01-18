using System;
using Endless.Gameplay;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000124 RID: 292
	public class UIInventorySpawnOptionsListCellView : UIBaseListCellView<InventorySpawnOption>
	{
		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000496 RID: 1174 RVA: 0x0001A8E7 File Offset: 0x00018AE7
		private UIInventorySpawnOptionsListModel TypedListModel
		{
			get
			{
				return (UIInventorySpawnOptionsListModel)base.ListModel;
			}
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x0001A8F4 File Offset: 0x00018AF4
		public override void OnDespawn()
		{
			base.OnDespawn();
			if (this.lockDisplayTweens.IsAnyTweening())
			{
				this.lockDisplayTweens.Cancel();
			}
			if (this.lockHideTweens.IsAnyTweening())
			{
				this.lockHideTweens.Cancel();
			}
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x0001A92C File Offset: 0x00018B2C
		public override void View(UIBaseListView<InventorySpawnOption> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			InventoryUsableDefinition inventoryUsableDefinition = this.TypedListModel.InventoryDefinitionLookUp[base.Model.AssetId];
			this.displayIcon.sprite = inventoryUsableDefinition.Sprite;
			this.displayName.text = inventoryUsableDefinition.DisplayName;
			this.lockToggle.SetIsOn(base.Model.LockItem, true, false);
			this.quantityToggle.gameObject.SetActive(!inventoryUsableDefinition.IsStackable);
			this.quantityStepper.gameObject.SetActive(inventoryUsableDefinition.IsStackable);
			this.quantityToggle.SetIsOn(base.Model.Quantity > 0, true, false);
			this.quantityStepper.SetValue(base.Model.Quantity, true);
			this.lockCanvasGroup.alpha = (float)((base.Model.Quantity == 0) ? 0 : 1);
		}

		// Token: 0x04000456 RID: 1110
		[Header("UIInventorySpawnOptionsListCellView")]
		[SerializeField]
		private Image displayIcon;

		// Token: 0x04000457 RID: 1111
		[SerializeField]
		private TextMeshProUGUI displayName;

		// Token: 0x04000458 RID: 1112
		[SerializeField]
		private CanvasGroup lockCanvasGroup;

		// Token: 0x04000459 RID: 1113
		[SerializeField]
		private UIToggle lockToggle;

		// Token: 0x0400045A RID: 1114
		[SerializeField]
		private UIToggle quantityToggle;

		// Token: 0x0400045B RID: 1115
		[SerializeField]
		private UIStepper quantityStepper;

		// Token: 0x0400045C RID: 1116
		[SerializeField]
		private TweenCollection lockDisplayTweens;

		// Token: 0x0400045D RID: 1117
		[SerializeField]
		private TweenCollection lockHideTweens;
	}
}
