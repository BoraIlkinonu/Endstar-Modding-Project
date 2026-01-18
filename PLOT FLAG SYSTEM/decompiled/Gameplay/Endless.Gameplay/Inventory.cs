using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.PlayerInventory;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class Inventory : NetworkBehaviour
{
	public struct QueuedItemSwap
	{
		public int slotIndex;

		public Item targetItem;

		public void Set(int slot, Item item)
		{
			slotIndex = slot;
			targetItem = item;
		}

		public void Clear()
		{
			slotIndex = -1;
			targetItem = null;
		}
	}

	public struct EquipmentSlot : INetworkSerializable, IEquatable<EquipmentSlot>
	{
		public int inventorySlot;

		public uint equippedFrame;

		public bool ignoreEquip;

		public uint swapFrame
		{
			get
			{
				if (equippedFrame <= 20)
				{
					return 0u;
				}
				return equippedFrame - 20;
			}
		}

		public EquipmentSlot(int invSlot, uint frame)
		{
			inventorySlot = invSlot;
			equippedFrame = frame;
			ignoreEquip = false;
		}

		public EquipmentSlot(int invSlot, uint frame, bool ignoreEquip)
		{
			inventorySlot = invSlot;
			equippedFrame = frame;
			this.ignoreEquip = ignoreEquip;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			if (serializer.IsWriter)
			{
				Compression.SerializeInt(serializer, inventorySlot);
				Compression.SerializeUInt(serializer, equippedFrame);
			}
			else
			{
				inventorySlot = Compression.DeserializeInt(serializer);
				equippedFrame = Compression.DeserializeUInt(serializer);
			}
		}

		public bool Equals(EquipmentSlot other)
		{
			return false;
		}
	}

	private const uint INVENTORY_SWAP_ANIMATION_FRAMES = 20u;

	private const uint MINIMUM_SWAP_REQUEST_FRAMES = 2u;

	public UnityEvent OnInitialized = new UnityEvent();

	public UnityEvent<NetworkListEvent<InventorySlot>> OnSlotsChanged = new UnityEvent<NetworkListEvent<InventorySlot>>();

	public UnityEvent<int> OnSlotsCountChanged = new UnityEvent<int>();

	public UnityEvent<NetworkListEvent<EquipmentSlot>> OnEquippedSlotsChanged = new UnityEvent<NetworkListEvent<EquipmentSlot>>();

	[SerializeField]
	private PlayerEquipmentManager equipmentManager;

	[SerializeField]
	private int equippedSlotCount = 2;

	[SerializeField]
	private int totalInventorySlots;

	[SerializeField]
	private List<StartingInventorySettings> startingItemSettings = new List<StartingInventorySettings>();

	private NetworkList<InventorySlot> slots;

	private NetworkList<EquipmentSlot> equippedSlots;

	private QueuedItemSwap queuedItemSwap;

	public bool Initialized { get; private set; }

	public int TotalInventorySlotCount => slots.Count;

	public bool HasEquipmentChangeQueued
	{
		get
		{
			if (queuedItemSwap.slotIndex > -1)
			{
				return queuedItemSwap.targetItem != null;
			}
			return false;
		}
	}

	private void Awake()
	{
		equippedSlots = new NetworkList<EquipmentSlot>();
		slots = new NetworkList<InventorySlot>();
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (base.IsServer)
		{
			for (int i = 0; i < equippedSlotCount; i++)
			{
				equippedSlots.Add(new EquipmentSlot(-1, 0u));
			}
			for (int j = 0; j < totalInventorySlots; j++)
			{
				InventorySlot item = new InventorySlot(null);
				slots.Add(item);
			}
		}
		equippedSlots.OnListChanged += HandleEquippedSlotsChanged;
		if (base.IsClient)
		{
			slots.OnListChanged += Slots_OnListChanged;
		}
		StartCoroutine(DelayedInitializeInventory());
	}

	private IEnumerator DelayedInitializeInventory()
	{
		yield return null;
		InitializeInventory();
	}

	public void SetTotalInventorySlots(int newValue)
	{
		newValue = Mathf.Clamp(newValue, 0, 10);
		int num = newValue - totalInventorySlots;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				InventorySlot item = new InventorySlot(null);
				slots.Add(item);
			}
		}
		else if (num < 0)
		{
			for (int num2 = 0; num2 > num; num2--)
			{
				slots.RemoveAt(slots.Count - 1);
			}
		}
		totalInventorySlots = newValue;
		OnSlotsCountChanged.Invoke(num);
	}

	public int GetTotalInventorySlots()
	{
		return totalInventorySlots;
	}

	public int GetEquippedSlotIndex(int carriedSlotIndex)
	{
		for (int i = 0; i < equippedSlots.Count; i++)
		{
			if (equippedSlots[i].inventorySlot == carriedSlotIndex)
			{
				return i;
			}
		}
		return -1;
	}

	public bool IsEquipmentIndex(int carriedSlotIndex)
	{
		return GetEquippedSlotIndex(carriedSlotIndex) > -1;
	}

	public InventorySlot GetSlot(int index)
	{
		return slots[index];
	}

	public int GetSlotIndexFromEquippedSlotsIndex(int equippedSlotsIndex)
	{
		if (equippedSlots.Count <= equippedSlotsIndex)
		{
			return -1;
		}
		return equippedSlots[equippedSlotsIndex].inventorySlot;
	}

	private void InitializeInventory()
	{
		if (base.IsHost || base.IsServer)
		{
			foreach (StartingInventorySettings startingItemSetting in startingItemSettings)
			{
				AttemptPickupItem(startingItemSetting.InventoryDefintion, startingItemSetting.Count, startingItemSetting.LockItem);
			}
		}
		OnInitialized.Invoke();
		Initialized = true;
	}

	private void Slots_OnListChanged(NetworkListEvent<InventorySlot> changeEvent)
	{
		OnSlotsChanged.Invoke(changeEvent);
	}

	private void HandleEquippedSlotsChanged(NetworkListEvent<EquipmentSlot> changeEvent)
	{
		if (!base.IsClient)
		{
			return;
		}
		if ((uint)changeEvent.Type == 4u && !changeEvent.Value.ignoreEquip)
		{
			Item item = ((changeEvent.PreviousValue.inventorySlot > -1) ? slots[changeEvent.PreviousValue.inventorySlot].Item : null);
			float delay = ((float)changeEvent.Value.swapFrame - (float)NetClock.CurrentFrame) * NetClock.FixedDeltaTime;
			if (item != null)
			{
				equipmentManager.HideEquipmentVisuals(item, delay);
			}
			Item item2 = ((changeEvent.Value.inventorySlot > -1) ? slots[changeEvent.Value.inventorySlot].Item : null);
			if (item2 != null)
			{
				equipmentManager.ShowEquipmentVisuals(item2, delay, changeEvent.PreviousValue.inventorySlot == -1);
			}
		}
		OnEquippedSlotsChanged.Invoke(changeEvent);
	}

	public void HandleEquipmentQueue(NetState state)
	{
		if (!state.AnyEquipmentSwapActive && HasEquipmentChangeQueued)
		{
			ServerEquipSlot(queuedItemSwap.slotIndex, (int)slots[queuedItemSwap.slotIndex].Item.InventorySlot);
			queuedItemSwap.Clear();
		}
	}

	public void HandleCharacterCosmeticsChanged()
	{
		foreach (InventorySlot slot in slots)
		{
			if ((bool)slot.Item)
			{
				slot.Item.ResetAppearance();
			}
		}
	}

	public bool AttemptPickupItem(InventoryUsableDefinition definition, int count = 1, bool lockItem = false)
	{
		return false;
	}

	public bool AttemptPickupItem(Item item, bool lockItem = false)
	{
		if (!item.IsPickupable)
		{
			return false;
		}
		if (item.IsStackable)
		{
			for (int i = 0; i < slots.Count; i++)
			{
				if (slots[i].AssetID == item.AssetID)
				{
					StackableItem stackableItem = (StackableItem)slots[i].Item;
					if ((bool)item)
					{
						stackableItem.ChangeStackCount(item.StackCount);
						item.LevelItemPickupFinished(equipmentManager.PlayerReferenceManager, item);
						return true;
					}
				}
			}
		}
		for (int j = 0; j < slots.Count; j++)
		{
			if (slots[j].DefinitionGuid == SerializableGuid.Empty)
			{
				Item item2 = item;
				item = item.Pickup(equipmentManager.PlayerReferenceManager);
				slots[j] = new InventorySlot(item, lockItem);
				int index = ((item.InventorySlot != Item.InventorySlotType.Major) ? 1 : 0);
				if (equippedSlots[index].inventorySlot == -1)
				{
					equippedSlots[index] = new EquipmentSlot(j, 0u);
				}
				if (item2 != item)
				{
					item2.LevelItemPickupFinished(equipmentManager.PlayerReferenceManager, item);
				}
				return true;
			}
		}
		return false;
	}

	public void CancelEquipmentSwapQueue()
	{
		queuedItemSwap.Clear();
	}

	public SerializableGuid GetInventoryId(int index)
	{
		if (slots.Count <= index)
		{
			return SerializableGuid.Empty;
		}
		return slots[index].AssetID;
	}

	public Item GetEquippedItem(int equippedSlotIndex)
	{
		int num = ((equippedSlots.Count > equippedSlotIndex) ? equippedSlots[equippedSlotIndex].inventorySlot : (-1));
		if (num == -1)
		{
			return null;
		}
		if (slots.Count <= num)
		{
			return null;
		}
		return slots[num].Item;
	}

	public SerializableGuid GetEquippedId(int equippedSlotIndex)
	{
		int num = ((equippedSlots.Count > equippedSlotIndex) ? equippedSlots[equippedSlotIndex].inventorySlot : (-1));
		if (num == -1)
		{
			return SerializableGuid.Empty;
		}
		return GetInventoryId(num);
	}

	public void Swap(int oldIndex, int newIndex)
	{
		Swap_ServerRPC(oldIndex, newIndex);
	}

	[ServerRpc]
	private void Swap_ServerRPC(int indexA, int indexB)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			if (base.OwnerClientId != networkManager.LocalClientId)
			{
				if (networkManager.LogLevel <= LogLevel.Normal)
				{
					Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
				}
				return;
			}
			ServerRpcParams serverRpcParams = default(ServerRpcParams);
			FastBufferWriter bufferWriter = __beginSendServerRpc(4078645071u, serverRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, indexA);
			BytePacker.WriteValueBitPacked(bufferWriter, indexB);
			__endSendServerRpc(ref bufferWriter, 4078645071u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		InventorySlot value = slots[indexA];
		InventorySlot value2 = slots[indexB];
		slots[indexA] = value2;
		slots[indexB] = value;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < equippedSlots.Count; i++)
		{
			bool flag3 = false;
			if (!flag && equippedSlots[i].inventorySlot == indexA)
			{
				equippedSlots[i] = new EquipmentSlot(indexB, 0u, ignoreEquip: true);
				flag = true;
				flag3 = true;
			}
			if (!flag3 && !flag2 && equippedSlots[i].inventorySlot == indexB)
			{
				equippedSlots[i] = new EquipmentSlot(indexA, 0u, ignoreEquip: true);
				flag2 = true;
			}
		}
	}

	public void DropItemFromSlot(int slotIndex)
	{
		DropItemFromSlot_ServerRPC(slotIndex);
	}

	public void EnsureItemIsDropped(Item targetItem)
	{
		int num = -1;
		for (int i = 0; i < slots.Count; i++)
		{
			if (slots[i].Item == targetItem)
			{
				num = i;
				break;
			}
		}
		if (!slots[num].Item || slots[num].Count <= 0 || slots[num].Locked)
		{
			return;
		}
		_ = slots[num].Item;
		for (int j = 0; j < equippedSlots.Count; j++)
		{
			if (equippedSlots[j].inventorySlot == num)
			{
				equippedSlots[j] = new EquipmentSlot(-1, 0u);
			}
		}
		slots[num] = new InventorySlot(null);
	}

	[ServerRpc]
	private void DropItemFromSlot_ServerRPC(int slotIndex)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			if (base.OwnerClientId != networkManager.LocalClientId)
			{
				if (networkManager.LogLevel <= LogLevel.Normal)
				{
					Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
				}
				return;
			}
			ServerRpcParams serverRpcParams = default(ServerRpcParams);
			FastBufferWriter bufferWriter = __beginSendServerRpc(2243252577u, serverRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, slotIndex);
			__endSendServerRpc(ref bufferWriter, 2243252577u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (!slots[slotIndex].Item || slots[slotIndex].Count <= 0 || slots[slotIndex].Locked)
		{
			return;
		}
		Item item = slots[slotIndex].Item;
		for (int i = 0; i < equippedSlots.Count; i++)
		{
			if (equippedSlots[i].inventorySlot == slotIndex)
			{
				equippedSlots[i] = new EquipmentSlot(-1, 0u);
			}
		}
		slots[slotIndex] = new InventorySlot(null);
		item.Drop(equipmentManager.PlayerReferenceManager);
	}

	private void DebugLogInventory()
	{
		Debug.Log("-------Inventory-------");
		for (int i = 0; i < slots.Count; i++)
		{
			if (slots[i].DefinitionGuid != SerializableGuid.Empty)
			{
				InventoryUsableDefinition inventoryUsableDefinition = (InventoryUsableDefinition)RuntimeDatabase.GetUsableDefinition(slots[i].DefinitionGuid);
				Debug.LogFormat("Item '{0}' is in slot {1}. Count: {2}", inventoryUsableDefinition.name, i, slots[i].Count);
			}
		}
		Debug.Log("-------Equipped Slots-------");
		for (int j = 0; j < equippedSlots.Count; j++)
		{
			Debug.LogFormat("Equipped Slot [ {0} ] = {1}", j, equippedSlots[j]);
		}
	}

	public void EquipSlot(int slotIndex)
	{
		if (slots.Count > slotIndex && slotIndex >= 0 && slots[slotIndex].Item != null && (slots[slotIndex].Item.InventorySlot == Item.InventorySlotType.Major || slots[slotIndex].Item.InventorySlot == Item.InventorySlotType.Minor))
		{
			EquipSlot(slotIndex, (int)slots[slotIndex].Item.InventoryUsableDefinition.InventoryType);
		}
	}

	public Item GetItemFromSlot(int slotIndex)
	{
		if (slots.Count > slotIndex && slotIndex >= 0)
		{
			return slots[slotIndex].Item;
		}
		return null;
	}

	public void EquipSlot(int slotIndex, int equipmentSlot)
	{
		EquipSlot_ServerRPC(slotIndex, equipmentSlot);
	}

	[ServerRpc]
	private void EquipSlot_ServerRPC(int slotIndex, int equipmentSlot)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			if (base.OwnerClientId != networkManager.LocalClientId)
			{
				if (networkManager.LogLevel <= LogLevel.Normal)
				{
					Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
				}
				return;
			}
			ServerRpcParams serverRpcParams = default(ServerRpcParams);
			FastBufferWriter bufferWriter = __beginSendServerRpc(457326218u, serverRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, slotIndex);
			BytePacker.WriteValueBitPacked(bufferWriter, equipmentSlot);
			__endSendServerRpc(ref bufferWriter, 457326218u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (slots[slotIndex].Count == 0 || slots[slotIndex].DefinitionGuid == SerializableGuid.Empty)
		{
			return;
		}
		int num = ((RuntimeDatabase.GetUsableDefinition<InventoryUsableDefinition>(slots[slotIndex].DefinitionGuid).InventoryType != InventoryUsableDefinition.InventoryTypes.Major) ? 1 : 0);
		if (equipmentSlot != num)
		{
			return;
		}
		if (equipmentManager.PlayerReferenceManager.PlayerController.CurrentState.AnyEquipmentSwapActive)
		{
			if (equipmentManager.PlayerReferenceManager.PlayerController.CurrentState.PrimarySwapBlockingFrames > 2 || equipmentManager.PlayerReferenceManager.PlayerController.CurrentState.SecondarySwapBlockingFrames > 2)
			{
				ServerEquipSlot(slotIndex, equipmentSlot);
			}
			else
			{
				queuedItemSwap.Set(slotIndex, slots[slotIndex].Item);
			}
		}
		else
		{
			ServerEquipSlot(slotIndex, equipmentSlot);
		}
	}

	private void ServerEquipSlot(int slotIndex, int equipmentSlot)
	{
		if (slots[slotIndex].Count == 0 || slots[slotIndex].DefinitionGuid == SerializableGuid.Empty)
		{
			return;
		}
		int num = ((RuntimeDatabase.GetUsableDefinition<InventoryUsableDefinition>(slots[slotIndex].DefinitionGuid).InventoryType != InventoryUsableDefinition.InventoryTypes.Major) ? 1 : 0);
		if (equipmentSlot != num)
		{
			return;
		}
		uint num2 = 0u;
		if (equippedSlots[equipmentSlot].inventorySlot != -1)
		{
			if (slotIndex == equippedSlots[equipmentSlot].inventorySlot)
			{
				return;
			}
			InventorySlot inventorySlot = slots[equippedSlots[equipmentSlot].inventorySlot];
			UsableDefinition usableDefinition = RuntimeDatabase.GetUsableDefinition(inventorySlot.DefinitionGuid);
			foreach (UsableDefinition.UseState activeUseState in equipmentManager.PlayerReferenceManager.PlayerController.CurrentState.ActiveUseStates)
			{
				if (activeUseState.EquipmentGuid == inventorySlot.DefinitionGuid)
				{
					if ((bool)usableDefinition)
					{
						num2 = usableDefinition.GetItemSwapDelayFrames(activeUseState);
					}
					else
					{
						Debug.LogWarning("Usable definition not found...");
					}
				}
			}
			CheckAndCancelReload();
		}
		if (num2 < 2)
		{
			num2 = 2u;
		}
		uint num3 = num2 + 20;
		uint frame = NetClock.CurrentFrame + num3;
		equippedSlots[equipmentSlot] = new EquipmentSlot(slotIndex, frame);
		equipmentManager.PlayerReferenceManager.PlayerController.ServerSetInventorySwapBlockFrames(slots[slotIndex].Item.InventoryUsableDefinition.InventoryType, num3);
	}

	public void StackCountChanged(SerializableGuid guid)
	{
		if (!base.IsServer)
		{
			return;
		}
		for (int i = 0; i < slots.Count; i++)
		{
			if (!(slots[i].DefinitionGuid == guid))
			{
				continue;
			}
			if (slots[i].Count > 0)
			{
				slots[i] = new InventorySlot(slots[i].Item, slots[i].Locked);
				continue;
			}
			slots[i] = new InventorySlot(null);
			if (!IsEquipmentIndex(i))
			{
				continue;
			}
			for (int j = 0; j < equippedSlots.Count; j++)
			{
				if (equippedSlots[j].inventorySlot == i)
				{
					equippedSlots[j] = new EquipmentSlot(-1, 0u);
					break;
				}
			}
		}
	}

	public Item GetEquippedItem(SerializableGuid assetID)
	{
		for (int i = 0; i < equippedSlotCount; i++)
		{
			if (equippedSlots[i].inventorySlot > -1)
			{
				InventorySlot inventorySlot = slots[equippedSlots[i].inventorySlot];
				if (inventorySlot.AssetID == assetID)
				{
					return inventorySlot.Item;
				}
			}
		}
		return null;
	}

	public bool HasItem(InventoryLibraryReference reference, int count)
	{
		if (count < 1)
		{
			return true;
		}
		try
		{
			return GetItemCount(reference) >= count;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return false;
	}

	public int GetItemCount(InventoryLibraryReference reference)
	{
		int num = 0;
		for (int i = 0; i < slots.Count; i++)
		{
			if (slots[i].AssetID == reference.Id)
			{
				num = ((!(slots[i].Item is StackableItem stackableItem)) ? (num + 1) : (num + stackableItem.StackCount));
			}
		}
		return num;
	}

	internal int GetSlotDeltaFromTrade(TradeInfo tradeInfo)
	{
		int num = 0;
		CollapseTrade(tradeInfo, out var costs, out var rewards);
		costs = costs.Where((TradeInfo.InventoryAndQuantityReference cost) => cost.GetInventoryReferenceType() == InventoryLibraryReference.InventoryReferenceType.Item).ToList();
		rewards = rewards.Where((TradeInfo.InventoryAndQuantityReference reward) => reward.GetInventoryReferenceType() == InventoryLibraryReference.InventoryReferenceType.Item).ToList();
		for (int num2 = 0; num2 < slots.Count; num2++)
		{
			InventorySlot slot = slots[num2];
			for (int num3 = costs.Count - 1; num3 >= 0; num3--)
			{
				TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = costs[num3];
				if (inventoryAndQuantityReference.Matches(slot))
				{
					if (slot.Item.IsStackable)
					{
						if (slot.Item.StackCount == inventoryAndQuantityReference.Quantity)
						{
							num--;
							costs.RemoveAt(num3);
							break;
						}
						if (slot.Item.StackCount > inventoryAndQuantityReference.Quantity)
						{
							costs.RemoveAt(num3);
							break;
						}
						num--;
						inventoryAndQuantityReference.Quantity -= slot.Item.StackCount;
					}
					else
					{
						num--;
						inventoryAndQuantityReference.Quantity--;
						if (inventoryAndQuantityReference.Quantity <= 0)
						{
							costs.RemoveAt(num3);
							break;
						}
					}
				}
			}
			for (int num4 = rewards.Count - 1; num4 >= 0; num4--)
			{
				if (rewards[num4].Matches(slot) && slot.Item.IsStackable)
				{
					rewards.RemoveAt(num4);
					break;
				}
			}
		}
		foreach (TradeInfo.InventoryAndQuantityReference item in rewards)
		{
			num = ((!item.IsStackable()) ? (num + item.Quantity) : (num + 1));
		}
		return num;
	}

	private static void CollapseTrade(TradeInfo tradeInfo, out List<TradeInfo.InventoryAndQuantityReference> costs, out List<TradeInfo.InventoryAndQuantityReference> rewards)
	{
		costs = new List<TradeInfo.InventoryAndQuantityReference>();
		costs.Add(new TradeInfo.InventoryAndQuantityReference(tradeInfo.Cost1));
		if (tradeInfo.Cost1.Matches(tradeInfo.Cost2))
		{
			costs[0].Quantity += tradeInfo.Cost2.Quantity;
		}
		else
		{
			costs.Add(new TradeInfo.InventoryAndQuantityReference(tradeInfo.Cost2));
		}
		rewards = new List<TradeInfo.InventoryAndQuantityReference>();
		if (tradeInfo.Reward1.IsReferenceSet())
		{
			rewards.Add(new TradeInfo.InventoryAndQuantityReference(tradeInfo.Reward1));
			if (tradeInfo.Reward1.Matches(tradeInfo.Reward2))
			{
				rewards[0].Quantity += tradeInfo.Reward2.Quantity;
			}
			else if (tradeInfo.Reward2.IsReferenceSet())
			{
				rewards.Add(new TradeInfo.InventoryAndQuantityReference(tradeInfo.Reward2));
			}
		}
		for (int num = costs.Count - 1; num >= 0; num--)
		{
			TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = costs[num];
			for (int num2 = rewards.Count - 1; num2 >= 0; num2--)
			{
				TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference2 = rewards[num2];
				if (inventoryAndQuantityReference.Matches(inventoryAndQuantityReference2))
				{
					if (inventoryAndQuantityReference.Quantity <= inventoryAndQuantityReference2.Quantity)
					{
						if (inventoryAndQuantityReference.Quantity == inventoryAndQuantityReference2.Quantity)
						{
							rewards.RemoveAt(num2);
							costs.RemoveAt(num);
						}
						else
						{
							costs.RemoveAt(num);
							inventoryAndQuantityReference2.Quantity -= inventoryAndQuantityReference.Quantity;
						}
						break;
					}
					inventoryAndQuantityReference.Quantity -= inventoryAndQuantityReference2.Quantity;
					rewards.RemoveAt(num2);
				}
			}
		}
	}

	public bool ConsumeItem(InventoryLibraryReference reference, int quantity)
	{
		List<int> list = new List<int>();
		int num = -1;
		for (int i = 0; i < slots.Count; i++)
		{
			if (!(slots[i].AssetID == reference.Id))
			{
				continue;
			}
			if (slots[i].Item is StackableItem stackableItem)
			{
				if (stackableItem.StackCount >= quantity)
				{
					stackableItem.ChangeStackCount(-quantity);
					return true;
				}
				return false;
			}
			list.Add(i);
			if (num < 0 && GetEquippedSlotIndex(i) != -1)
			{
				num = i;
			}
		}
		if (list.Count >= quantity)
		{
			if (list.Count > quantity && num > -1)
			{
				list.Remove(num);
			}
			for (int num2 = list.Count - 1; num2 > list.Count - quantity - 1; num2--)
			{
				int num3 = list[num2];
				slots[num3].Item.Destroy();
				int equippedSlotIndex = GetEquippedSlotIndex(num3);
				if (equippedSlotIndex != -1)
				{
					equippedSlots[equippedSlotIndex] = new EquipmentSlot(-1, 0u);
				}
				slots[num3] = new InventorySlot(null);
			}
			return true;
		}
		return false;
	}

	public void ClearAllItems(bool includeLockedItems)
	{
		for (int i = 0; i < slots.Count; i++)
		{
			if ((bool)slots[i].Item && (includeLockedItems || !slots[i].Locked))
			{
				slots[i].Item.Destroy();
				slots[i] = new InventorySlot(null);
			}
		}
		for (int j = 0; j < equippedSlots.Count; j++)
		{
			if (equippedSlots[j].inventorySlot != -1 && (includeLockedItems || !slots[equippedSlots[j].inventorySlot].Locked))
			{
				equippedSlots[j] = new EquipmentSlot(-1, 0u);
			}
		}
	}

	public int AttemptGiveAsPossible(InventoryLibraryReference reference, bool lockItem, int quantity)
	{
		if (reference.IsReferenceEmpty() || quantity < 1)
		{
			return 0;
		}
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(reference.Id, out var metadata))
		{
			Item item = metadata.EndlessProp.WorldObject.BaseType as Item;
			if (item.IsStackable)
			{
				int num = -1;
				for (int i = 0; i < slots.Count; i++)
				{
					if (reference.Matches(slots[i]))
					{
						StackableItem stackableItem = (StackableItem)slots[i].Item;
						if ((bool)item)
						{
							stackableItem.ChangeStackCount(quantity);
							return 0;
						}
					}
					else if (num < 0 && slots[i].AssetID == SerializableGuid.Empty)
					{
						num = i;
					}
				}
				if (num >= 0)
				{
					GiveItem(item, lockItem);
					StackableItem stackableItem2 = (StackableItem)slots[num].Item;
					if ((bool)item)
					{
						stackableItem2.ForceStackCount(quantity);
						return 0;
					}
					return quantity;
				}
				return quantity;
			}
			int emptySlotCount = GetEmptySlotCount();
			for (int j = 0; j < emptySlotCount; j++)
			{
				GiveItem(item, lockItem);
				quantity--;
				if (quantity == 0)
				{
					return 0;
				}
			}
			return quantity;
		}
		return quantity;
	}

	public bool AttemptGiveItem(InventoryLibraryReference reference, bool lockItem, int quantity)
	{
		if (reference.IsReferenceEmpty() || quantity < 1)
		{
			return false;
		}
		try
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(reference.Id, out var metadata))
			{
				Item item = metadata.EndlessProp.WorldObject.BaseType as Item;
				if (item.IsStackable)
				{
					int num = -1;
					for (int i = 0; i < slots.Count; i++)
					{
						if (slots[i].AssetID == item.AssetID)
						{
							StackableItem stackableItem = (StackableItem)slots[i].Item;
							if ((bool)item)
							{
								stackableItem.ChangeStackCount(quantity);
								return true;
							}
						}
						else if (num < 0 && slots[i].AssetID == SerializableGuid.Empty)
						{
							num = i;
						}
					}
					if (num >= 0)
					{
						GiveItem(item, lockItem);
						StackableItem stackableItem2 = (StackableItem)slots[num].Item;
						if ((bool)item)
						{
							stackableItem2.ForceStackCount(quantity);
							return true;
						}
						return true;
					}
					return false;
				}
				if (GetEmptySlotCount() < 1)
				{
					return false;
				}
				GiveItem(item, lockItem);
				return true;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return false;
	}

	private void GiveItem(Item item, bool lockItem)
	{
		Item item2 = null;
		try
		{
			int firstEmptySlot = GetFirstEmptySlot();
			if (firstEmptySlot != -1)
			{
				item2 = item.SpawnFromReference(equipmentManager.PlayerReferenceManager);
				slots[firstEmptySlot] = new InventorySlot(item2, lockItem);
				int index = ((item2.InventorySlot != Item.InventorySlotType.Major) ? 1 : 0);
				if (equippedSlots[index].inventorySlot == -1)
				{
					equippedSlots[index] = new EquipmentSlot(firstEmptySlot, 0u);
					equipmentManager.ShowEquipmentVisuals(item2);
				}
			}
			else
			{
				Debug.LogWarning("No slot for given item. Empty slot/stackable should have been confirmed..");
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			if (item2 != null)
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
		}
	}

	public int GetFirstEmptySlot()
	{
		for (int i = 0; i < slots.Count; i++)
		{
			if (slots[i].DefinitionGuid == SerializableGuid.Empty)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetEmptySlotCount()
	{
		int num = 0;
		for (int i = 0; i < slots.Count; i++)
		{
			if (slots[i].DefinitionGuid == SerializableGuid.Empty)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasEmptySlot()
	{
		return GetFirstEmptySlot() != -1;
	}

	public void CheckAndCancelReload()
	{
		if (GetEquippedItem(0) is RangedWeaponItem rangedWeaponItem && (rangedWeaponItem.ReloadRequested || rangedWeaponItem.ReloadStarted))
		{
			rangedWeaponItem.ServerCancelReload();
		}
	}

	protected override void __initializeVariables()
	{
		if (slots == null)
		{
			throw new Exception("Inventory.slots cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		slots.Initialize(this);
		__nameNetworkVariable(slots, "slots");
		NetworkVariableFields.Add(slots);
		if (equippedSlots == null)
		{
			throw new Exception("Inventory.equippedSlots cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		equippedSlots.Initialize(this);
		__nameNetworkVariable(equippedSlots, "equippedSlots");
		NetworkVariableFields.Add(equippedSlots);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(4078645071u, __rpc_handler_4078645071, "Swap_ServerRPC");
		__registerRpc(2243252577u, __rpc_handler_2243252577, "DropItemFromSlot_ServerRPC");
		__registerRpc(457326218u, __rpc_handler_457326218, "EquipSlot_ServerRPC");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_4078645071(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
		{
			if (networkManager.LogLevel <= LogLevel.Normal)
			{
				Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
			}
		}
		else
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			ByteUnpacker.ReadValueBitPacked(reader, out int value2);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Inventory)target).Swap_ServerRPC(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2243252577(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
		{
			if (networkManager.LogLevel <= LogLevel.Normal)
			{
				Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
			}
		}
		else
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Inventory)target).DropItemFromSlot_ServerRPC(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_457326218(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
		{
			if (networkManager.LogLevel <= LogLevel.Normal)
			{
				Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
			}
		}
		else
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			ByteUnpacker.ReadValueBitPacked(reader, out int value2);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Inventory)target).EquipSlot_ServerRPC(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "Inventory";
	}
}
