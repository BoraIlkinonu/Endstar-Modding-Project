using Endless.Shared.Debugging;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class UIMajorEquipmentAlternateActionOnScreenControl : UIBaseEquipmentOnScreenControl
{
	[Header("UIMajorEquipmentAlternateActionOnScreenControl")]
	[SerializeField]
	private Selectable selectable;

	[SerializeField]
	private UIOnScreenStickHandler onScreenStick;

	[SerializeField]
	private UIOnScreenButtonHandler onScreenButtonHandler;

	protected override Item.InventorySlotType InventorySlotType => Item.InventorySlotType.Major;

	protected override void Start()
	{
		base.Start();
		onScreenStick.OnPointerDownUnityEvent.AddListener(OnPointerDown);
		onScreenStick.OnPointerUpUnityEvent.AddListener(OnPointerUp);
	}

	protected override void Clear()
	{
		base.Clear();
		SetSelectableAndOnScreenStickInteractable(interactable: false);
	}

	protected override void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1} {{ {2}: {3}, {4}: {5}, {6}: {7} }} )", "OnEquippedSlotsChanged", "changeEvent", "Index", changeEvent.Index, "Value", changeEvent.Value, "PreviousValue", changeEvent.PreviousValue), this);
		}
		if (!base.HasEquipment)
		{
			Clear();
			return;
		}
		if (base.Inventory.GetSlot(base.SlotIndex).Item.InventoryUsableDefinition.MobileUIMajorLayout != InventoryUsableDefinition.MobileUI_MajorLayoutType.Ranged)
		{
			Clear();
			return;
		}
		Display();
		SetSelectableAndOnScreenStickInteractable(interactable: true);
	}

	private void SetSelectableAndOnScreenStickInteractable(bool interactable)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetSelectableAndOnScreenStickInteractable", interactable);
		}
		onScreenStick.SetInteractable(interactable);
		selectable.interactable = interactable;
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
}
