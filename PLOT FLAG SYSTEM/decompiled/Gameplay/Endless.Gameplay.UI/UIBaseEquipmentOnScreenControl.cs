using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay.UI;

public abstract class UIBaseEquipmentOnScreenControl : UIGameObject
{
	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	[field: Header("Debugging")]
	[field: SerializeField]
	protected bool VerboseLogging { get; set; }

	protected abstract Item.InventorySlotType InventorySlotType { get; }

	protected Inventory Inventory { get; private set; }

	protected bool HasEquipment => SlotIndex > -1;

	protected int SlotIndex => Inventory.GetSlotIndexFromEquippedSlotsIndex((int)InventorySlotType);

	protected virtual void Start()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		Clear();
		PlayerReferenceManager localPlayerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject();
		Inventory = localPlayerObject.Inventory;
		Inventory.OnEquippedSlotsChanged.AddListener(OnEquippedSlotsChanged);
		OnEquippedSlotsChanged(default(NetworkListEvent<Inventory.EquipmentSlot>));
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(Clear);
		displayAndHideHandler.SetToDisplayEnd(triggerUnityEvent: true);
	}

	protected virtual void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1} {{ {2}: {3}, {4}: {5}, {6}: {7} }} )", "OnEquippedSlotsChanged", "changeEvent", "Index", changeEvent.Index, "Value", changeEvent.Value, "PreviousValue", changeEvent.PreviousValue), this);
		}
		if (HasEquipment)
		{
			Display();
		}
		else
		{
			Clear();
		}
	}

	protected void Display()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Display", this);
		}
		displayAndHideHandler.Display();
	}

	protected virtual void Clear()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Clear", this);
		}
		displayAndHideHandler.Hide();
	}
}
