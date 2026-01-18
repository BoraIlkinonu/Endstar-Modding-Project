using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000406 RID: 1030
	public class UIMajorEquipmentPrimaryActionOnScreenControl : UIBaseEquipmentOnScreenControl
	{
		// Token: 0x17000538 RID: 1336
		// (get) Token: 0x060019BE RID: 6590 RVA: 0x0001965C File Offset: 0x0001785C
		protected override Item.InventorySlotType InventorySlotType
		{
			get
			{
				return Item.InventorySlotType.Major;
			}
		}

		// Token: 0x060019BF RID: 6591 RVA: 0x0007653C File Offset: 0x0007473C
		protected override void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
		{
			base.OnEquippedSlotsChanged(changeEvent);
			if (!base.HasEquipment)
			{
				return;
			}
			Item item = base.Inventory.GetSlot(base.SlotIndex).Item;
			this.itemView.View(item);
		}

		// Token: 0x060019C0 RID: 6592 RVA: 0x0007657F File Offset: 0x0007477F
		protected override void Clear()
		{
			base.Clear();
			this.itemView.Clear();
			this.selectable.interactable = false;
		}

		// Token: 0x04001469 RID: 5225
		[Header("UIMajorEquipmentPrimaryActionOnScreenControl")]
		[SerializeField]
		private Selectable selectable;

		// Token: 0x0400146A RID: 5226
		[SerializeField]
		private UIItemView itemView;
	}
}
