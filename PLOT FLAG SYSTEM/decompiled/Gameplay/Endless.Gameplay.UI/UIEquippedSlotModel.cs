using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI;

public class UIEquippedSlotModel : UIGameObject
{
	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	[field: SerializeField]
	public int Index { get; private set; }

	[field: SerializeField]
	public Item.InventorySlotType InventorySlotType { get; private set; }

	public UnityEvent OnChanged { get; } = new UnityEvent();

	public Inventory Inventory { get; private set; }

	public Item Item
	{
		get
		{
			if (!(Inventory == null))
			{
				return Inventory.GetEquippedItem(Index);
			}
			return null;
		}
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		PlayerReferenceManager localPlayerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject();
		if (localPlayerObject == null)
		{
			MonoBehaviourSingleton<PlayerManager>.Instance.OnOwnerRegistered.AddListener(OnOwnerRegistered);
		}
		else
		{
			OnOwnerRegistered(localPlayerObject.OwnerClientId, localPlayerObject);
		}
	}

	private void OnOwnerRegistered(ulong clientId, PlayerReferenceManager playerReferenceManager)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnOwnerRegistered", clientId, playerReferenceManager.IsOwner);
		}
		Inventory = playerReferenceManager.Inventory;
		if (!(Inventory == null))
		{
			Inventory.OnEquippedSlotsChanged.AddListener(OnEquippedSlotsChanged);
			Inventory.OnSlotsChanged.AddListener(OnSlotsChanged);
			OnChanged.Invoke();
		}
	}

	private void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEquippedSlotsChanged", changeEvent.Index);
		}
		OnChanged.Invoke();
	}

	private void OnSlotsChanged(NetworkListEvent<InventorySlot> changeEvent)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSlotsChanged", changeEvent.Index);
		}
		OnChanged.Invoke();
	}
}
