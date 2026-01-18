using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003AF RID: 943
	public class UIInventoryView : UIMonoBehaviourSingleton<UIInventoryView>
	{
		// Token: 0x170004F3 RID: 1267
		// (get) Token: 0x0600180D RID: 6157 RVA: 0x0006FDD7 File Offset: 0x0006DFD7
		// (set) Token: 0x0600180E RID: 6158 RVA: 0x0006FDDF File Offset: 0x0006DFDF
		public Inventory Inventory { get; private set; }

		// Token: 0x170004F4 RID: 1268
		// (get) Token: 0x0600180F RID: 6159 RVA: 0x0006FDE8 File Offset: 0x0006DFE8
		public IReadOnlyList<UIInventorySlotView> InventorySlots
		{
			get
			{
				return this.inventorySlots;
			}
		}

		// Token: 0x06001810 RID: 6160 RVA: 0x0006FDF0 File Offset: 0x0006DFF0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PlayerManager>.Instance.OnOwnerRegistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.OnOwnerRegistered));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.Clear));
		}

		// Token: 0x06001811 RID: 6161 RVA: 0x0006FE4C File Offset: 0x0006E04C
		public Vector3 GetItemPosition(Item item)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetItemPosition", new object[] { JsonUtility.ToJson(item) });
			}
			foreach (UIInventorySlotView uiinventorySlotView in this.inventorySlots)
			{
				if (uiinventorySlotView.Model.Item == item)
				{
					return uiinventorySlotView.ItemPosition;
				}
			}
			DebugUtility.LogError(this, "GetItemPosition", "Could not find a slot with that item!", new object[] { JsonUtility.ToJson(item) });
			return Vector3.zero;
		}

		// Token: 0x06001812 RID: 6162 RVA: 0x0006FF00 File Offset: 0x0006E100
		private void OnOwnerRegistered(ulong clientId, PlayerReferenceManager playerReferenceManager)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnOwnerRegistered", new object[] { clientId, playerReferenceManager.IsOwner });
			}
			this.Inventory = playerReferenceManager.Inventory;
			if (this.Inventory == null)
			{
				return;
			}
			this.Inventory.OnInitialized.AddListener(new UnityAction(this.OnInventoryInitialized));
		}

		// Token: 0x06001813 RID: 6163 RVA: 0x0006FF74 File Offset: 0x0006E174
		private void OnInventoryInitialized()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnInventoryInitialized", Array.Empty<object>());
			}
			this.Inventory.OnSlotsCountChanged.AddListener(new UnityAction<int>(this.OnSlotsCountChanged));
			this.Inventory.OnSlotsChanged.AddListener(new UnityAction<NetworkListEvent<InventorySlot>>(this.OnSlotsChanged));
			this.Inventory.OnEquippedSlotsChanged.AddListener(new UnityAction<NetworkListEvent<Inventory.EquipmentSlot>>(this.OnEquippedSlotsChanged));
			this.OnSlotsCountChanged(this.Inventory.TotalInventorySlotCount);
		}

		// Token: 0x06001814 RID: 6164 RVA: 0x00070000 File Offset: 0x0006E200
		private void OnSlotsCountChanged(int newCount)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSlotsCountChanged", new object[] { newCount });
			}
			if (this.inventorySlots.Count < newCount)
			{
				int num = 1;
				for (int i = this.inventorySlots.Count; i < newCount; i++)
				{
					PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
					UIInventorySlotView uiinventorySlotView = this.inventorySlotSource;
					Transform rectTransform = base.RectTransform;
					UIInventorySlotView uiinventorySlotView2 = instance.Spawn<UIInventorySlotView>(uiinventorySlotView, default(Vector3), default(Quaternion), rectTransform);
					if (uiinventorySlotView2 == null)
					{
						DebugUtility.LogError(this, "OnSlotsCountChanged", "Failed to spawn UIInventorySlotView!", new object[] { newCount });
					}
					else
					{
						InventorySlot slot = this.Inventory.GetSlot(i);
						float num2 = (float)num * this.inventorySlotInitializationDelay;
						uiinventorySlotView2.Initialize(slot, num2, i);
						this.inventorySlots.Add(uiinventorySlotView2);
						num++;
					}
				}
				return;
			}
			if (this.inventorySlots.Count > newCount)
			{
				int num3 = this.inventorySlots.Count - newCount;
				for (int j = this.inventorySlots.Count - 1; j >= num3; j--)
				{
					this.inventorySlots[j].Close();
					this.inventorySlots.RemoveAt(j);
				}
			}
		}

		// Token: 0x06001815 RID: 6165 RVA: 0x00070144 File Offset: 0x0006E344
		private void OnSlotsChanged(NetworkListEvent<InventorySlot> changeEvent)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSlotsChanged", new object[] { changeEvent.Index });
			}
			int index = changeEvent.Index;
			this.ViewSlot(index);
			foreach (UIInventorySlotView uiinventorySlotView in this.inventorySlots)
			{
				uiinventorySlotView.ViewDropFeedback(false);
			}
		}

		// Token: 0x06001816 RID: 6166 RVA: 0x000701CC File Offset: 0x0006E3CC
		private void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEquippedSlotsChanged", new object[] { string.Format("{0} {{ {1}: {2}, {3}: {4}, {5}: {6} }}", new object[] { "changeEvent", "Index", changeEvent.Index, "Value", changeEvent.Value, "PreviousValue", changeEvent.PreviousValue }) });
			}
			for (int i = 0; i < this.inventorySlots.Count; i++)
			{
				this.ViewSlot(i);
			}
		}

		// Token: 0x06001817 RID: 6167 RVA: 0x0007026C File Offset: 0x0006E46C
		private void ViewSlot(int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewSlot", new object[] { index });
			}
			if (index >= this.inventorySlots.Count)
			{
				DebugUtility.LogException(new IndexOutOfRangeException(string.Format("{0}: {1}, {2}.{3}: {4}", new object[]
				{
					"index",
					index,
					"inventorySlots",
					"Count",
					this.inventorySlots.Count
				})), this);
				return;
			}
			InventorySlot slot = this.Inventory.GetSlot(index);
			UIInventorySlotView uiinventorySlotView = this.inventorySlots[index];
			uiinventorySlotView.SetSetDisplayDelay(0f);
			uiinventorySlotView.View(slot, index);
		}

		// Token: 0x06001818 RID: 6168 RVA: 0x00070324 File Offset: 0x0006E524
		private void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			foreach (UIInventorySlotView uiinventorySlotView in this.inventorySlots)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIInventorySlotView>(uiinventorySlotView);
			}
			this.inventorySlots.Clear();
		}

		// Token: 0x0400134C RID: 4940
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400134D RID: 4941
		[SerializeField]
		private UIInventorySlotView inventorySlotSource;

		// Token: 0x0400134E RID: 4942
		private readonly List<UIInventorySlotView> inventorySlots = new List<UIInventorySlotView>();

		// Token: 0x0400134F RID: 4943
		private readonly float inventorySlotInitializationDelay = 0.25f;
	}
}
