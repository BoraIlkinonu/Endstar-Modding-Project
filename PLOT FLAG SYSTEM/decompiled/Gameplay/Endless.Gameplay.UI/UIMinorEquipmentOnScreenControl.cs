using Endless.Shared.Debugging;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class UIMinorEquipmentOnScreenControl : UIBaseEquipmentOnScreenControl
{
	[Header("UIMinorEquipmentOnScreenControl")]
	[SerializeField]
	private Selectable selectable;

	[SerializeField]
	private UIOnScreenStickHandler onScreenStick;

	[SerializeField]
	private UIOnScreenButtonHandler onScreenButtonHandler;

	[SerializeField]
	private UIItemView itemView;

	private InventoryUsableDefinition.MobileUI_MinorLayoutType layoutType;

	protected override Item.InventorySlotType InventorySlotType => Item.InventorySlotType.Minor;

	protected override void Start()
	{
		base.Start();
		onScreenStick.OnPointerDownUnityEvent.AddListener(OnPointerDown);
		onScreenStick.OnPointerUpUnityEvent.AddListener(OnPointerUp);
	}

	protected override void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
	{
		base.OnEquippedSlotsChanged(changeEvent);
		SetInteractable(base.HasEquipment);
		if (base.HasEquipment)
		{
			Item item = base.Inventory.GetSlot(base.SlotIndex).Item;
			itemView.View(item);
			layoutType = item.InventoryUsableDefinition.MobileUIMinorLayout;
			float movementRange = ((layoutType == InventoryUsableDefinition.MobileUI_MinorLayoutType.ButtonWithJoystick) ? 50 : 0);
			onScreenStick.SetMovementRange(movementRange);
		}
	}

	protected override void Clear()
	{
		base.Clear();
		itemView.Clear();
		SetInteractable(interactable: false);
	}

	private void OnPointerDown()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPointerDown");
		}
		onScreenButtonHandler.SetButtonState(down: true);
	}

	private void OnPointerUp()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPointerUp");
		}
		onScreenButtonHandler.SetButtonState(down: false);
	}

	private void SetInteractable(bool interactable)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractable", interactable);
		}
		selectable.interactable = interactable;
		onScreenStick.SetInteractable(interactable);
	}
}
