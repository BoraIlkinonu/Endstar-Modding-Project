using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.PlayerInventory;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000E3 RID: 227
	public class Inventory : NetworkBehaviour
	{
		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x060004F3 RID: 1267 RVA: 0x000191F4 File Offset: 0x000173F4
		// (set) Token: 0x060004F4 RID: 1268 RVA: 0x000191FC File Offset: 0x000173FC
		public bool Initialized { get; private set; }

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x060004F5 RID: 1269 RVA: 0x00019205 File Offset: 0x00017405
		public int TotalInventorySlotCount
		{
			get
			{
				return this.slots.Count;
			}
		}

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x060004F6 RID: 1270 RVA: 0x00019212 File Offset: 0x00017412
		public bool HasEquipmentChangeQueued
		{
			get
			{
				return this.queuedItemSwap.slotIndex > -1 && this.queuedItemSwap.targetItem != null;
			}
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x00019235 File Offset: 0x00017435
		private void Awake()
		{
			this.equippedSlots = new NetworkList<Inventory.EquipmentSlot>();
			this.slots = new NetworkList<InventorySlot>();
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x00019250 File Offset: 0x00017450
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (base.IsServer)
			{
				for (int i = 0; i < this.equippedSlotCount; i++)
				{
					this.equippedSlots.Add(new Inventory.EquipmentSlot(-1, 0U));
				}
				for (int j = 0; j < this.totalInventorySlots; j++)
				{
					InventorySlot inventorySlot = new InventorySlot(null, false);
					this.slots.Add(inventorySlot);
				}
			}
			this.equippedSlots.OnListChanged += this.HandleEquippedSlotsChanged;
			if (base.IsClient)
			{
				this.slots.OnListChanged += this.Slots_OnListChanged;
			}
			base.StartCoroutine(this.DelayedInitializeInventory());
		}

		// Token: 0x060004F9 RID: 1273 RVA: 0x000192F7 File Offset: 0x000174F7
		private IEnumerator DelayedInitializeInventory()
		{
			yield return null;
			this.InitializeInventory();
			yield break;
		}

		// Token: 0x060004FA RID: 1274 RVA: 0x00019308 File Offset: 0x00017508
		public void SetTotalInventorySlots(int newValue)
		{
			newValue = Mathf.Clamp(newValue, 0, 10);
			int num = newValue - this.totalInventorySlots;
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					InventorySlot inventorySlot = new InventorySlot(null, false);
					this.slots.Add(inventorySlot);
				}
			}
			else if (num < 0)
			{
				for (int j = 0; j > num; j--)
				{
					this.slots.RemoveAt(this.slots.Count - 1);
				}
			}
			this.totalInventorySlots = newValue;
			this.OnSlotsCountChanged.Invoke(num);
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x0001938B File Offset: 0x0001758B
		public int GetTotalInventorySlots()
		{
			return this.totalInventorySlots;
		}

		// Token: 0x060004FC RID: 1276 RVA: 0x00019394 File Offset: 0x00017594
		public int GetEquippedSlotIndex(int carriedSlotIndex)
		{
			for (int i = 0; i < this.equippedSlots.Count; i++)
			{
				if (this.equippedSlots[i].inventorySlot == carriedSlotIndex)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x000193CE File Offset: 0x000175CE
		public bool IsEquipmentIndex(int carriedSlotIndex)
		{
			return this.GetEquippedSlotIndex(carriedSlotIndex) > -1;
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x000193DA File Offset: 0x000175DA
		public InventorySlot GetSlot(int index)
		{
			return this.slots[index];
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x000193E8 File Offset: 0x000175E8
		public int GetSlotIndexFromEquippedSlotsIndex(int equippedSlotsIndex)
		{
			if (this.equippedSlots.Count <= equippedSlotsIndex)
			{
				return -1;
			}
			return this.equippedSlots[equippedSlotsIndex].inventorySlot;
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x0001940C File Offset: 0x0001760C
		private void InitializeInventory()
		{
			if (base.IsHost || base.IsServer)
			{
				foreach (StartingInventorySettings startingInventorySettings in this.startingItemSettings)
				{
					this.AttemptPickupItem(startingInventorySettings.InventoryDefintion, startingInventorySettings.Count, startingInventorySettings.LockItem);
				}
			}
			this.OnInitialized.Invoke();
			this.Initialized = true;
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x00019494 File Offset: 0x00017694
		private void Slots_OnListChanged(NetworkListEvent<InventorySlot> changeEvent)
		{
			this.OnSlotsChanged.Invoke(changeEvent);
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x000194A4 File Offset: 0x000176A4
		private void HandleEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
		{
			if (base.IsClient)
			{
				if (changeEvent.Type == NetworkListEvent<Inventory.EquipmentSlot>.EventType.Value && !changeEvent.Value.ignoreEquip)
				{
					Item item = ((changeEvent.PreviousValue.inventorySlot > -1) ? this.slots[changeEvent.PreviousValue.inventorySlot].Item : null);
					float num = (changeEvent.Value.swapFrame - NetClock.CurrentFrame) * NetClock.FixedDeltaTime;
					if (item != null)
					{
						this.equipmentManager.HideEquipmentVisuals(item, num);
					}
					Item item2 = ((changeEvent.Value.inventorySlot > -1) ? this.slots[changeEvent.Value.inventorySlot].Item : null);
					if (item2 != null)
					{
						this.equipmentManager.ShowEquipmentVisuals(item2, num, changeEvent.PreviousValue.inventorySlot == -1);
					}
				}
				this.OnEquippedSlotsChanged.Invoke(changeEvent);
			}
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x0001959C File Offset: 0x0001779C
		public void HandleEquipmentQueue(NetState state)
		{
			if (state.AnyEquipmentSwapActive)
			{
				return;
			}
			if (this.HasEquipmentChangeQueued)
			{
				this.ServerEquipSlot(this.queuedItemSwap.slotIndex, (int)this.slots[this.queuedItemSwap.slotIndex].Item.InventorySlot);
				this.queuedItemSwap.Clear();
			}
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x000195FC File Offset: 0x000177FC
		public void HandleCharacterCosmeticsChanged()
		{
			foreach (InventorySlot inventorySlot in this.slots)
			{
				if (inventorySlot.Item)
				{
					inventorySlot.Item.ResetAppearance();
				}
			}
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x0001965C File Offset: 0x0001785C
		public bool AttemptPickupItem(InventoryUsableDefinition definition, int count = 1, bool lockItem = false)
		{
			return false;
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x00019660 File Offset: 0x00017860
		public bool AttemptPickupItem(Item item, bool lockItem = false)
		{
			if (!item.IsPickupable)
			{
				return false;
			}
			if (item.IsStackable)
			{
				for (int i = 0; i < this.slots.Count; i++)
				{
					if (this.slots[i].AssetID == item.AssetID)
					{
						StackableItem stackableItem = (StackableItem)this.slots[i].Item;
						if (item)
						{
							stackableItem.ChangeStackCount(item.StackCount);
							item.LevelItemPickupFinished(this.equipmentManager.PlayerReferenceManager, item);
							return true;
						}
					}
				}
			}
			for (int j = 0; j < this.slots.Count; j++)
			{
				if (this.slots[j].DefinitionGuid == SerializableGuid.Empty)
				{
					Item item2 = item;
					item = item.Pickup(this.equipmentManager.PlayerReferenceManager);
					this.slots[j] = new InventorySlot(item, lockItem);
					int num = ((item.InventorySlot == Item.InventorySlotType.Major) ? 0 : 1);
					if (this.equippedSlots[num].inventorySlot == -1)
					{
						this.equippedSlots[num] = new Inventory.EquipmentSlot(j, 0U);
					}
					if (item2 != item)
					{
						item2.LevelItemPickupFinished(this.equipmentManager.PlayerReferenceManager, item);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x000197B4 File Offset: 0x000179B4
		public void CancelEquipmentSwapQueue()
		{
			this.queuedItemSwap.Clear();
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x000197C4 File Offset: 0x000179C4
		public SerializableGuid GetInventoryId(int index)
		{
			if (this.slots.Count <= index)
			{
				return SerializableGuid.Empty;
			}
			return this.slots[index].AssetID;
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x000197FC File Offset: 0x000179FC
		public Item GetEquippedItem(int equippedSlotIndex)
		{
			int num = ((this.equippedSlots.Count > equippedSlotIndex) ? this.equippedSlots[equippedSlotIndex].inventorySlot : (-1));
			if (num == -1)
			{
				return null;
			}
			if (this.slots.Count <= num)
			{
				return null;
			}
			return this.slots[num].Item;
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x00019858 File Offset: 0x00017A58
		public SerializableGuid GetEquippedId(int equippedSlotIndex)
		{
			int num = ((this.equippedSlots.Count > equippedSlotIndex) ? this.equippedSlots[equippedSlotIndex].inventorySlot : (-1));
			if (num == -1)
			{
				return SerializableGuid.Empty;
			}
			return this.GetInventoryId(num);
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x00019899 File Offset: 0x00017A99
		public void Swap(int oldIndex, int newIndex)
		{
			this.Swap_ServerRPC(oldIndex, newIndex);
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x000198A4 File Offset: 0x00017AA4
		[ServerRpc]
		private void Swap_ServerRPC(int indexA, int indexB)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				if (base.OwnerClientId != networkManager.LocalClientId)
				{
					if (networkManager.LogLevel <= LogLevel.Normal)
					{
						Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
					}
					return;
				}
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(4078645071U, serverRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, indexA);
				BytePacker.WriteValueBitPacked(fastBufferWriter, indexB);
				base.__endSendServerRpc(ref fastBufferWriter, 4078645071U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			InventorySlot inventorySlot = this.slots[indexA];
			InventorySlot inventorySlot2 = this.slots[indexB];
			this.slots[indexA] = inventorySlot2;
			this.slots[indexB] = inventorySlot;
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < this.equippedSlots.Count; i++)
			{
				bool flag3 = false;
				if (!flag && this.equippedSlots[i].inventorySlot == indexA)
				{
					this.equippedSlots[i] = new Inventory.EquipmentSlot(indexB, 0U, true);
					flag = true;
					flag3 = true;
				}
				if (!flag3 && !flag2 && this.equippedSlots[i].inventorySlot == indexB)
				{
					this.equippedSlots[i] = new Inventory.EquipmentSlot(indexA, 0U, true);
					flag2 = true;
				}
			}
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x00019A91 File Offset: 0x00017C91
		public void DropItemFromSlot(int slotIndex)
		{
			this.DropItemFromSlot_ServerRPC(slotIndex);
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x00019A9C File Offset: 0x00017C9C
		public void EnsureItemIsDropped(Item targetItem)
		{
			int num = -1;
			for (int i = 0; i < this.slots.Count; i++)
			{
				if (this.slots[i].Item == targetItem)
				{
					num = i;
					break;
				}
			}
			if (this.slots[num].Item && this.slots[num].Count > 0 && !this.slots[num].Locked)
			{
				Item item = this.slots[num].Item;
				for (int j = 0; j < this.equippedSlots.Count; j++)
				{
					if (this.equippedSlots[j].inventorySlot == num)
					{
						this.equippedSlots[j] = new Inventory.EquipmentSlot(-1, 0U);
					}
				}
				this.slots[num] = new InventorySlot(null, false);
			}
		}

		// Token: 0x0600050F RID: 1295 RVA: 0x00019B90 File Offset: 0x00017D90
		[ServerRpc]
		private void DropItemFromSlot_ServerRPC(int slotIndex)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				if (base.OwnerClientId != networkManager.LocalClientId)
				{
					if (networkManager.LogLevel <= LogLevel.Normal)
					{
						Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
					}
					return;
				}
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2243252577U, serverRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, slotIndex);
				base.__endSendServerRpc(ref fastBufferWriter, 2243252577U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.slots[slotIndex].Item && this.slots[slotIndex].Count > 0 && !this.slots[slotIndex].Locked)
			{
				Item item = this.slots[slotIndex].Item;
				for (int i = 0; i < this.equippedSlots.Count; i++)
				{
					if (this.equippedSlots[i].inventorySlot == slotIndex)
					{
						this.equippedSlots[i] = new Inventory.EquipmentSlot(-1, 0U);
					}
				}
				this.slots[slotIndex] = new InventorySlot(null, false);
				item.Drop(this.equipmentManager.PlayerReferenceManager);
			}
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x00019D78 File Offset: 0x00017F78
		private void DebugLogInventory()
		{
			Debug.Log("-------Inventory-------");
			for (int i = 0; i < this.slots.Count; i++)
			{
				if (this.slots[i].DefinitionGuid != SerializableGuid.Empty)
				{
					InventoryUsableDefinition inventoryUsableDefinition = (InventoryUsableDefinition)RuntimeDatabase.GetUsableDefinition(this.slots[i].DefinitionGuid);
					Debug.LogFormat("Item '{0}' is in slot {1}. Count: {2}", new object[]
					{
						inventoryUsableDefinition.name,
						i,
						this.slots[i].Count
					});
				}
			}
			Debug.Log("-------Equipped Slots-------");
			for (int j = 0; j < this.equippedSlots.Count; j++)
			{
				Debug.LogFormat("Equipped Slot [ {0} ] = {1}", new object[]
				{
					j,
					this.equippedSlots[j]
				});
			}
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x00019E78 File Offset: 0x00018078
		public void EquipSlot(int slotIndex)
		{
			if (this.slots.Count > slotIndex && slotIndex >= 0 && this.slots[slotIndex].Item != null && (this.slots[slotIndex].Item.InventorySlot == Item.InventorySlotType.Major || this.slots[slotIndex].Item.InventorySlot == Item.InventorySlotType.Minor))
			{
				this.EquipSlot(slotIndex, (int)this.slots[slotIndex].Item.InventoryUsableDefinition.InventoryType);
			}
		}

		// Token: 0x06000512 RID: 1298 RVA: 0x00019F10 File Offset: 0x00018110
		public Item GetItemFromSlot(int slotIndex)
		{
			if (this.slots.Count > slotIndex && slotIndex >= 0)
			{
				return this.slots[slotIndex].Item;
			}
			return null;
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x00019F45 File Offset: 0x00018145
		public void EquipSlot(int slotIndex, int equipmentSlot)
		{
			this.EquipSlot_ServerRPC(slotIndex, equipmentSlot);
		}

		// Token: 0x06000514 RID: 1300 RVA: 0x00019F50 File Offset: 0x00018150
		[ServerRpc]
		private void EquipSlot_ServerRPC(int slotIndex, int equipmentSlot)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				if (base.OwnerClientId != networkManager.LocalClientId)
				{
					if (networkManager.LogLevel <= LogLevel.Normal)
					{
						Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
					}
					return;
				}
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(457326218U, serverRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, slotIndex);
				BytePacker.WriteValueBitPacked(fastBufferWriter, equipmentSlot);
				base.__endSendServerRpc(ref fastBufferWriter, 457326218U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.slots[slotIndex].Count == 0 || this.slots[slotIndex].DefinitionGuid == SerializableGuid.Empty)
			{
				return;
			}
			int num = ((RuntimeDatabase.GetUsableDefinition<InventoryUsableDefinition>(this.slots[slotIndex].DefinitionGuid).InventoryType == InventoryUsableDefinition.InventoryTypes.Major) ? 0 : 1);
			if (equipmentSlot != num)
			{
				return;
			}
			if (!this.equipmentManager.PlayerReferenceManager.PlayerController.CurrentState.AnyEquipmentSwapActive)
			{
				this.ServerEquipSlot(slotIndex, equipmentSlot);
				return;
			}
			if (this.equipmentManager.PlayerReferenceManager.PlayerController.CurrentState.PrimarySwapBlockingFrames > 2U || this.equipmentManager.PlayerReferenceManager.PlayerController.CurrentState.SecondarySwapBlockingFrames > 2U)
			{
				this.ServerEquipSlot(slotIndex, equipmentSlot);
				return;
			}
			this.queuedItemSwap.Set(slotIndex, this.slots[slotIndex].Item);
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x0001A170 File Offset: 0x00018370
		private void ServerEquipSlot(int slotIndex, int equipmentSlot)
		{
			if (this.slots[slotIndex].Count == 0 || this.slots[slotIndex].DefinitionGuid == SerializableGuid.Empty)
			{
				return;
			}
			int num = ((RuntimeDatabase.GetUsableDefinition<InventoryUsableDefinition>(this.slots[slotIndex].DefinitionGuid).InventoryType == InventoryUsableDefinition.InventoryTypes.Major) ? 0 : 1);
			if (equipmentSlot != num)
			{
				return;
			}
			uint num2 = 0U;
			if (this.equippedSlots[equipmentSlot].inventorySlot != -1)
			{
				if (slotIndex == this.equippedSlots[equipmentSlot].inventorySlot)
				{
					return;
				}
				InventorySlot inventorySlot = this.slots[this.equippedSlots[equipmentSlot].inventorySlot];
				UsableDefinition usableDefinition = RuntimeDatabase.GetUsableDefinition(inventorySlot.DefinitionGuid);
				foreach (UsableDefinition.UseState useState in this.equipmentManager.PlayerReferenceManager.PlayerController.CurrentState.ActiveUseStates)
				{
					if (useState.EquipmentGuid == inventorySlot.DefinitionGuid)
					{
						if (usableDefinition)
						{
							num2 = usableDefinition.GetItemSwapDelayFrames(useState);
						}
						else
						{
							Debug.LogWarning("Usable definition not found...");
						}
					}
				}
				this.CheckAndCancelReload();
			}
			if (num2 < 2U)
			{
				num2 = 2U;
			}
			uint num3 = num2 + 20U;
			uint num4 = NetClock.CurrentFrame + num3;
			this.equippedSlots[equipmentSlot] = new Inventory.EquipmentSlot(slotIndex, num4);
			this.equipmentManager.PlayerReferenceManager.PlayerController.ServerSetInventorySwapBlockFrames(this.slots[slotIndex].Item.InventoryUsableDefinition.InventoryType, num3);
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x0001A328 File Offset: 0x00018528
		public void StackCountChanged(SerializableGuid guid)
		{
			if (base.IsServer)
			{
				for (int i = 0; i < this.slots.Count; i++)
				{
					if (this.slots[i].DefinitionGuid == guid)
					{
						if (this.slots[i].Count > 0)
						{
							this.slots[i] = new InventorySlot(this.slots[i].Item, this.slots[i].Locked);
						}
						else
						{
							this.slots[i] = new InventorySlot(null, false);
							if (this.IsEquipmentIndex(i))
							{
								for (int j = 0; j < this.equippedSlots.Count; j++)
								{
									if (this.equippedSlots[j].inventorySlot == i)
									{
										this.equippedSlots[j] = new Inventory.EquipmentSlot(-1, 0U);
										break;
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x0001A428 File Offset: 0x00018628
		public Item GetEquippedItem(SerializableGuid assetID)
		{
			for (int i = 0; i < this.equippedSlotCount; i++)
			{
				if (this.equippedSlots[i].inventorySlot > -1)
				{
					InventorySlot inventorySlot = this.slots[this.equippedSlots[i].inventorySlot];
					if (inventorySlot.AssetID == assetID)
					{
						return inventorySlot.Item;
					}
				}
			}
			return null;
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x0001A490 File Offset: 0x00018690
		public bool HasItem(InventoryLibraryReference reference, int count)
		{
			if (count < 1)
			{
				return true;
			}
			try
			{
				return this.GetItemCount(reference) >= count;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return false;
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x0001A4D0 File Offset: 0x000186D0
		public int GetItemCount(InventoryLibraryReference reference)
		{
			int num = 0;
			for (int i = 0; i < this.slots.Count; i++)
			{
				if (this.slots[i].AssetID == reference.Id)
				{
					StackableItem stackableItem = this.slots[i].Item as StackableItem;
					if (stackableItem != null)
					{
						num += stackableItem.StackCount;
					}
					else
					{
						num++;
					}
				}
			}
			return num;
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x0001A544 File Offset: 0x00018744
		internal int GetSlotDeltaFromTrade(TradeInfo tradeInfo)
		{
			int num = 0;
			List<TradeInfo.InventoryAndQuantityReference> list;
			List<TradeInfo.InventoryAndQuantityReference> list2;
			Inventory.CollapseTrade(tradeInfo, out list, out list2);
			list = list.Where((TradeInfo.InventoryAndQuantityReference cost) => cost.GetInventoryReferenceType() == InventoryLibraryReference.InventoryReferenceType.Item).ToList<TradeInfo.InventoryAndQuantityReference>();
			list2 = list2.Where((TradeInfo.InventoryAndQuantityReference reward) => reward.GetInventoryReferenceType() == InventoryLibraryReference.InventoryReferenceType.Item).ToList<TradeInfo.InventoryAndQuantityReference>();
			for (int i = 0; i < this.slots.Count; i++)
			{
				InventorySlot inventorySlot = this.slots[i];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = list[j];
					if (inventoryAndQuantityReference.Matches(inventorySlot))
					{
						if (inventorySlot.Item.IsStackable)
						{
							if (inventorySlot.Item.StackCount == inventoryAndQuantityReference.Quantity)
							{
								num--;
								list.RemoveAt(j);
								break;
							}
							if (inventorySlot.Item.StackCount > inventoryAndQuantityReference.Quantity)
							{
								list.RemoveAt(j);
								break;
							}
							num--;
							inventoryAndQuantityReference.Quantity -= inventorySlot.Item.StackCount;
						}
						else
						{
							num--;
							inventoryAndQuantityReference.Quantity--;
							if (inventoryAndQuantityReference.Quantity <= 0)
							{
								list.RemoveAt(j);
								break;
							}
						}
					}
				}
				for (int k = list2.Count - 1; k >= 0; k--)
				{
					if (list2[k].Matches(inventorySlot) && inventorySlot.Item.IsStackable)
					{
						list2.RemoveAt(k);
						break;
					}
				}
			}
			foreach (TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference2 in list2)
			{
				if (inventoryAndQuantityReference2.IsStackable())
				{
					num++;
				}
				else
				{
					num += inventoryAndQuantityReference2.Quantity;
				}
			}
			return num;
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x0001A740 File Offset: 0x00018940
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
			for (int i = costs.Count - 1; i >= 0; i--)
			{
				TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference = costs[i];
				for (int j = rewards.Count - 1; j >= 0; j--)
				{
					TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference2 = rewards[j];
					if (inventoryAndQuantityReference.Matches(inventoryAndQuantityReference2))
					{
						if (inventoryAndQuantityReference.Quantity > inventoryAndQuantityReference2.Quantity)
						{
							inventoryAndQuantityReference.Quantity -= inventoryAndQuantityReference2.Quantity;
							rewards.RemoveAt(j);
						}
						else
						{
							if (inventoryAndQuantityReference.Quantity == inventoryAndQuantityReference2.Quantity)
							{
								rewards.RemoveAt(j);
								costs.RemoveAt(i);
								break;
							}
							costs.RemoveAt(i);
							inventoryAndQuantityReference2.Quantity -= inventoryAndQuantityReference.Quantity;
							break;
						}
					}
				}
			}
		}

		// Token: 0x0600051C RID: 1308 RVA: 0x0001A8DC File Offset: 0x00018ADC
		public bool ConsumeItem(InventoryLibraryReference reference, int quantity)
		{
			List<int> list = new List<int>();
			int num = -1;
			for (int i = 0; i < this.slots.Count; i++)
			{
				if (this.slots[i].AssetID == reference.Id)
				{
					StackableItem stackableItem = this.slots[i].Item as StackableItem;
					if (stackableItem != null)
					{
						if (stackableItem.StackCount >= quantity)
						{
							stackableItem.ChangeStackCount(-quantity);
							return true;
						}
						return false;
					}
					else
					{
						list.Add(i);
						if (num < 0 && this.GetEquippedSlotIndex(i) != -1)
						{
							num = i;
						}
					}
				}
			}
			if (list.Count >= quantity)
			{
				if (list.Count > quantity && num > -1)
				{
					list.Remove(num);
				}
				for (int j = list.Count - 1; j > list.Count - quantity - 1; j--)
				{
					int num2 = list[j];
					this.slots[num2].Item.Destroy();
					int equippedSlotIndex = this.GetEquippedSlotIndex(num2);
					if (equippedSlotIndex != -1)
					{
						this.equippedSlots[equippedSlotIndex] = new Inventory.EquipmentSlot(-1, 0U);
					}
					this.slots[num2] = new InventorySlot(null, false);
				}
				return true;
			}
			return false;
		}

		// Token: 0x0600051D RID: 1309 RVA: 0x0001AA14 File Offset: 0x00018C14
		public void ClearAllItems(bool includeLockedItems)
		{
			for (int i = 0; i < this.slots.Count; i++)
			{
				if (this.slots[i].Item && (includeLockedItems || !this.slots[i].Locked))
				{
					this.slots[i].Item.Destroy();
					this.slots[i] = new InventorySlot(null, false);
				}
			}
			for (int j = 0; j < this.equippedSlots.Count; j++)
			{
				if (this.equippedSlots[j].inventorySlot != -1 && (includeLockedItems || !this.slots[this.equippedSlots[j].inventorySlot].Locked))
				{
					this.equippedSlots[j] = new Inventory.EquipmentSlot(-1, 0U);
				}
			}
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x0001AAF8 File Offset: 0x00018CF8
		public int AttemptGiveAsPossible(InventoryLibraryReference reference, bool lockItem, int quantity)
		{
			if (reference.IsReferenceEmpty() || quantity < 1)
			{
				return 0;
			}
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(reference.Id, out runtimePropInfo))
			{
				return quantity;
			}
			Item item = runtimePropInfo.EndlessProp.WorldObject.BaseType as Item;
			if (!item.IsStackable)
			{
				int emptySlotCount = this.GetEmptySlotCount();
				for (int i = 0; i < emptySlotCount; i++)
				{
					this.GiveItem(item, lockItem);
					quantity--;
					if (quantity == 0)
					{
						return 0;
					}
				}
				return quantity;
			}
			int num = -1;
			for (int j = 0; j < this.slots.Count; j++)
			{
				if (reference.Matches(this.slots[j]))
				{
					StackableItem stackableItem = (StackableItem)this.slots[j].Item;
					if (item)
					{
						stackableItem.ChangeStackCount(quantity);
						return 0;
					}
				}
				else if (num < 0 && this.slots[j].AssetID == SerializableGuid.Empty)
				{
					num = j;
				}
			}
			if (num < 0)
			{
				return quantity;
			}
			this.GiveItem(item, lockItem);
			StackableItem stackableItem2 = (StackableItem)this.slots[num].Item;
			if (item)
			{
				stackableItem2.ForceStackCount(quantity);
				return 0;
			}
			return quantity;
		}

		// Token: 0x0600051F RID: 1311 RVA: 0x0001AC40 File Offset: 0x00018E40
		public bool AttemptGiveItem(InventoryLibraryReference reference, bool lockItem, int quantity)
		{
			if (reference.IsReferenceEmpty() || quantity < 1)
			{
				return false;
			}
			try
			{
				PropLibrary.RuntimePropInfo runtimePropInfo;
				if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(reference.Id, out runtimePropInfo))
				{
					Item item = runtimePropInfo.EndlessProp.WorldObject.BaseType as Item;
					if (item.IsStackable)
					{
						int num = -1;
						for (int i = 0; i < this.slots.Count; i++)
						{
							if (this.slots[i].AssetID == item.AssetID)
							{
								StackableItem stackableItem = (StackableItem)this.slots[i].Item;
								if (item)
								{
									stackableItem.ChangeStackCount(quantity);
									return true;
								}
							}
							else if (num < 0 && this.slots[i].AssetID == SerializableGuid.Empty)
							{
								num = i;
							}
						}
						if (num < 0)
						{
							return false;
						}
						this.GiveItem(item, lockItem);
						StackableItem stackableItem2 = (StackableItem)this.slots[num].Item;
						if (item)
						{
							stackableItem2.ForceStackCount(quantity);
							return true;
						}
						return true;
					}
					else
					{
						if (this.GetEmptySlotCount() < 1)
						{
							return false;
						}
						this.GiveItem(item, lockItem);
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return false;
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x0001ADC0 File Offset: 0x00018FC0
		private void GiveItem(Item item, bool lockItem)
		{
			Item item2 = null;
			try
			{
				int firstEmptySlot = this.GetFirstEmptySlot();
				if (firstEmptySlot != -1)
				{
					item2 = item.SpawnFromReference(this.equipmentManager.PlayerReferenceManager);
					this.slots[firstEmptySlot] = new InventorySlot(item2, lockItem);
					int num = ((item2.InventorySlot == Item.InventorySlotType.Major) ? 0 : 1);
					if (this.equippedSlots[num].inventorySlot == -1)
					{
						this.equippedSlots[num] = new Inventory.EquipmentSlot(firstEmptySlot, 0U);
						this.equipmentManager.ShowEquipmentVisuals(item2);
					}
				}
				else
				{
					Debug.LogWarning("No slot for given item. Empty slot/stackable should have been confirmed..");
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				if (item2 != null)
				{
					global::UnityEngine.Object.Destroy(item2.gameObject);
				}
			}
		}

		// Token: 0x06000521 RID: 1313 RVA: 0x0001AE78 File Offset: 0x00019078
		public int GetFirstEmptySlot()
		{
			for (int i = 0; i < this.slots.Count; i++)
			{
				if (this.slots[i].DefinitionGuid == SerializableGuid.Empty)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x0001AEC0 File Offset: 0x000190C0
		public int GetEmptySlotCount()
		{
			int num = 0;
			for (int i = 0; i < this.slots.Count; i++)
			{
				if (this.slots[i].DefinitionGuid == SerializableGuid.Empty)
				{
					num++;
				}
			}
			return num;
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x0001AF0A File Offset: 0x0001910A
		public bool HasEmptySlot()
		{
			return this.GetFirstEmptySlot() != -1;
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x0001AF18 File Offset: 0x00019118
		public void CheckAndCancelReload()
		{
			RangedWeaponItem rangedWeaponItem = this.GetEquippedItem(0) as RangedWeaponItem;
			if (rangedWeaponItem != null && (rangedWeaponItem.ReloadRequested || rangedWeaponItem.ReloadStarted))
			{
				rangedWeaponItem.ServerCancelReload();
			}
		}

		// Token: 0x06000526 RID: 1318 RVA: 0x0001AFA0 File Offset: 0x000191A0
		protected override void __initializeVariables()
		{
			bool flag = this.slots == null;
			if (flag)
			{
				throw new Exception("Inventory.slots cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.slots.Initialize(this);
			base.__nameNetworkVariable(this.slots, "slots");
			this.NetworkVariableFields.Add(this.slots);
			flag = this.equippedSlots == null;
			if (flag)
			{
				throw new Exception("Inventory.equippedSlots cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.equippedSlots.Initialize(this);
			base.__nameNetworkVariable(this.equippedSlots, "equippedSlots");
			this.NetworkVariableFields.Add(this.equippedSlots);
			base.__initializeVariables();
		}

		// Token: 0x06000527 RID: 1319 RVA: 0x0001B050 File Offset: 0x00019250
		protected override void __initializeRpcs()
		{
			base.__registerRpc(4078645071U, new NetworkBehaviour.RpcReceiveHandler(Inventory.__rpc_handler_4078645071), "Swap_ServerRPC");
			base.__registerRpc(2243252577U, new NetworkBehaviour.RpcReceiveHandler(Inventory.__rpc_handler_2243252577), "DropItemFromSlot_ServerRPC");
			base.__registerRpc(457326218U, new NetworkBehaviour.RpcReceiveHandler(Inventory.__rpc_handler_457326218), "EquipSlot_ServerRPC");
			base.__initializeRpcs();
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x0001B0BC File Offset: 0x000192BC
		private static void __rpc_handler_4078645071(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
			{
				if (networkManager.LogLevel <= LogLevel.Normal)
				{
					Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
				}
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			int num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Inventory)target).Swap_ServerRPC(num, num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x0001B17C File Offset: 0x0001937C
		private static void __rpc_handler_2243252577(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
			{
				if (networkManager.LogLevel <= LogLevel.Normal)
				{
					Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
				}
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Inventory)target).DropItemFromSlot_ServerRPC(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x0001B22C File Offset: 0x0001942C
		private static void __rpc_handler_457326218(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
			{
				if (networkManager.LogLevel <= LogLevel.Normal)
				{
					Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
				}
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			int num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Inventory)target).EquipSlot_ServerRPC(num, num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600052B RID: 1323 RVA: 0x0001B2EC File Offset: 0x000194EC
		protected internal override string __getTypeName()
		{
			return "Inventory";
		}

		// Token: 0x040003F3 RID: 1011
		private const uint INVENTORY_SWAP_ANIMATION_FRAMES = 20U;

		// Token: 0x040003F4 RID: 1012
		private const uint MINIMUM_SWAP_REQUEST_FRAMES = 2U;

		// Token: 0x040003F5 RID: 1013
		public UnityEvent OnInitialized = new UnityEvent();

		// Token: 0x040003F6 RID: 1014
		public UnityEvent<NetworkListEvent<InventorySlot>> OnSlotsChanged = new UnityEvent<NetworkListEvent<InventorySlot>>();

		// Token: 0x040003F7 RID: 1015
		public UnityEvent<int> OnSlotsCountChanged = new UnityEvent<int>();

		// Token: 0x040003F8 RID: 1016
		public UnityEvent<NetworkListEvent<Inventory.EquipmentSlot>> OnEquippedSlotsChanged = new UnityEvent<NetworkListEvent<Inventory.EquipmentSlot>>();

		// Token: 0x040003F9 RID: 1017
		[SerializeField]
		private PlayerEquipmentManager equipmentManager;

		// Token: 0x040003FA RID: 1018
		[SerializeField]
		private int equippedSlotCount = 2;

		// Token: 0x040003FB RID: 1019
		[SerializeField]
		private int totalInventorySlots;

		// Token: 0x040003FC RID: 1020
		[SerializeField]
		private List<StartingInventorySettings> startingItemSettings = new List<StartingInventorySettings>();

		// Token: 0x040003FD RID: 1021
		private NetworkList<InventorySlot> slots;

		// Token: 0x040003FF RID: 1023
		private NetworkList<Inventory.EquipmentSlot> equippedSlots;

		// Token: 0x04000400 RID: 1024
		private Inventory.QueuedItemSwap queuedItemSwap;

		// Token: 0x020000E4 RID: 228
		public struct QueuedItemSwap
		{
			// Token: 0x0600052C RID: 1324 RVA: 0x0001B2F3 File Offset: 0x000194F3
			public void Set(int slot, Item item)
			{
				this.slotIndex = slot;
				this.targetItem = item;
			}

			// Token: 0x0600052D RID: 1325 RVA: 0x0001B303 File Offset: 0x00019503
			public void Clear()
			{
				this.slotIndex = -1;
				this.targetItem = null;
			}

			// Token: 0x04000401 RID: 1025
			public int slotIndex;

			// Token: 0x04000402 RID: 1026
			public Item targetItem;
		}

		// Token: 0x020000E5 RID: 229
		public struct EquipmentSlot : INetworkSerializable, IEquatable<Inventory.EquipmentSlot>
		{
			// Token: 0x170000D7 RID: 215
			// (get) Token: 0x0600052E RID: 1326 RVA: 0x0001B313 File Offset: 0x00019513
			public uint swapFrame
			{
				get
				{
					if (this.equippedFrame <= 20U)
					{
						return 0U;
					}
					return this.equippedFrame - 20U;
				}
			}

			// Token: 0x0600052F RID: 1327 RVA: 0x0001B32A File Offset: 0x0001952A
			public EquipmentSlot(int invSlot, uint frame)
			{
				this.inventorySlot = invSlot;
				this.equippedFrame = frame;
				this.ignoreEquip = false;
			}

			// Token: 0x06000530 RID: 1328 RVA: 0x0001B341 File Offset: 0x00019541
			public EquipmentSlot(int invSlot, uint frame, bool ignoreEquip)
			{
				this.inventorySlot = invSlot;
				this.equippedFrame = frame;
				this.ignoreEquip = ignoreEquip;
			}

			// Token: 0x06000531 RID: 1329 RVA: 0x0001B358 File Offset: 0x00019558
			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				if (serializer.IsWriter)
				{
					Compression.SerializeInt<T>(serializer, this.inventorySlot);
					Compression.SerializeUInt<T>(serializer, this.equippedFrame);
					return;
				}
				this.inventorySlot = Compression.DeserializeInt<T>(serializer);
				this.equippedFrame = Compression.DeserializeUInt<T>(serializer);
			}

			// Token: 0x06000532 RID: 1330 RVA: 0x0001965C File Offset: 0x0001785C
			public bool Equals(Inventory.EquipmentSlot other)
			{
				return false;
			}

			// Token: 0x04000403 RID: 1027
			public int inventorySlot;

			// Token: 0x04000404 RID: 1028
			public uint equippedFrame;

			// Token: 0x04000405 RID: 1029
			public bool ignoreEquip;
		}
	}
}
