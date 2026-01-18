using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class UIMajorEquipmentPrimaryActionOnScreenControl : UIBaseEquipmentOnScreenControl
{
	[Header("UIMajorEquipmentPrimaryActionOnScreenControl")]
	[SerializeField]
	private Selectable selectable;

	[SerializeField]
	private UIItemView itemView;

	protected override Item.InventorySlotType InventorySlotType => Item.InventorySlotType.Major;

	protected override void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
	{
		base.OnEquippedSlotsChanged(changeEvent);
		if (base.HasEquipment)
		{
			Item item = base.Inventory.GetSlot(base.SlotIndex).Item;
			itemView.View(item);
		}
	}

	protected override void Clear()
	{
		base.Clear();
		itemView.Clear();
		selectable.interactable = false;
	}
}
