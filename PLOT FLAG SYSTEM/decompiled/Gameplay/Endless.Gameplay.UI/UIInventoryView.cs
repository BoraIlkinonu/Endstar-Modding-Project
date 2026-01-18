using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIInventoryView : UIMonoBehaviourSingleton<UIInventoryView>
{
	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	[SerializeField]
	private UIInventorySlotView inventorySlotSource;

	private readonly List<UIInventorySlotView> inventorySlots = new List<UIInventorySlotView>();

	private readonly float inventorySlotInitializationDelay = 0.25f;

	public Inventory Inventory { get; private set; }

	public IReadOnlyList<UIInventorySlotView> InventorySlots => inventorySlots;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		MonoBehaviourSingleton<PlayerManager>.Instance.OnOwnerRegistered.AddListener(OnOwnerRegistered);
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(Clear);
	}

	public Vector3 GetItemPosition(Item item)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetItemPosition", JsonUtility.ToJson(item));
		}
		foreach (UIInventorySlotView inventorySlot in inventorySlots)
		{
			if (inventorySlot.Model.Item == item)
			{
				return inventorySlot.ItemPosition;
			}
		}
		DebugUtility.LogError(this, "GetItemPosition", "Could not find a slot with that item!", JsonUtility.ToJson(item));
		return Vector3.zero;
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
			Inventory.OnInitialized.AddListener(OnInventoryInitialized);
		}
	}

	private void OnInventoryInitialized()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnInventoryInitialized");
		}
		Inventory.OnSlotsCountChanged.AddListener(OnSlotsCountChanged);
		Inventory.OnSlotsChanged.AddListener(OnSlotsChanged);
		Inventory.OnEquippedSlotsChanged.AddListener(OnEquippedSlotsChanged);
		OnSlotsCountChanged(Inventory.TotalInventorySlotCount);
	}

	private void OnSlotsCountChanged(int newCount)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSlotsCountChanged", newCount);
		}
		if (inventorySlots.Count < newCount)
		{
			int num = 1;
			for (int i = inventorySlots.Count; i < newCount; i++)
			{
				PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UIInventorySlotView prefab = inventorySlotSource;
				Transform parent = base.RectTransform;
				UIInventorySlotView uIInventorySlotView = instance.Spawn(prefab, default(Vector3), default(Quaternion), parent);
				if (uIInventorySlotView == null)
				{
					DebugUtility.LogError(this, "OnSlotsCountChanged", "Failed to spawn UIInventorySlotView!", newCount);
					continue;
				}
				InventorySlot slot = Inventory.GetSlot(i);
				float delay = (float)num * inventorySlotInitializationDelay;
				uIInventorySlotView.Initialize(slot, delay, i);
				inventorySlots.Add(uIInventorySlotView);
				num++;
			}
		}
		else if (inventorySlots.Count > newCount)
		{
			int num2 = inventorySlots.Count - newCount;
			for (int num3 = inventorySlots.Count - 1; num3 >= num2; num3--)
			{
				inventorySlots[num3].Close();
				inventorySlots.RemoveAt(num3);
			}
		}
	}

	private void OnSlotsChanged(NetworkListEvent<InventorySlot> changeEvent)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSlotsChanged", changeEvent.Index);
		}
		int index = changeEvent.Index;
		ViewSlot(index);
		foreach (UIInventorySlotView inventorySlot in inventorySlots)
		{
			inventorySlot.ViewDropFeedback(dropIsValid: false);
		}
	}

	private void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEquippedSlotsChanged", string.Format("{0} {{ {1}: {2}, {3}: {4}, {5}: {6} }}", "changeEvent", "Index", changeEvent.Index, "Value", changeEvent.Value, "PreviousValue", changeEvent.PreviousValue));
		}
		for (int i = 0; i < inventorySlots.Count; i++)
		{
			ViewSlot(i);
		}
	}

	private void ViewSlot(int index)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewSlot", index);
		}
		if (index >= inventorySlots.Count)
		{
			DebugUtility.LogException(new IndexOutOfRangeException(string.Format("{0}: {1}, {2}.{3}: {4}", "index", index, "inventorySlots", "Count", inventorySlots.Count)), this);
		}
		else
		{
			InventorySlot slot = Inventory.GetSlot(index);
			UIInventorySlotView uIInventorySlotView = inventorySlots[index];
			uIInventorySlotView.SetSetDisplayDelay(0f);
			uIInventorySlotView.View(slot, index);
		}
	}

	private void Clear()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		foreach (UIInventorySlotView inventorySlot in inventorySlots)
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(inventorySlot);
		}
		inventorySlots.Clear();
	}
}
