using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000403 RID: 1027
	public abstract class UIBaseEquipmentOnScreenControl : UIGameObject
	{
		// Token: 0x17000532 RID: 1330
		// (get) Token: 0x060019A6 RID: 6566 RVA: 0x000760E3 File Offset: 0x000742E3
		// (set) Token: 0x060019A7 RID: 6567 RVA: 0x000760EB File Offset: 0x000742EB
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000533 RID: 1331
		// (get) Token: 0x060019A8 RID: 6568
		protected abstract Item.InventorySlotType InventorySlotType { get; }

		// Token: 0x17000534 RID: 1332
		// (get) Token: 0x060019A9 RID: 6569 RVA: 0x000760F4 File Offset: 0x000742F4
		// (set) Token: 0x060019AA RID: 6570 RVA: 0x000760FC File Offset: 0x000742FC
		private protected Inventory Inventory { protected get; private set; }

		// Token: 0x17000535 RID: 1333
		// (get) Token: 0x060019AB RID: 6571 RVA: 0x00076105 File Offset: 0x00074305
		protected bool HasEquipment
		{
			get
			{
				return this.SlotIndex > -1;
			}
		}

		// Token: 0x17000536 RID: 1334
		// (get) Token: 0x060019AC RID: 6572 RVA: 0x00076110 File Offset: 0x00074310
		protected int SlotIndex
		{
			get
			{
				return this.Inventory.GetSlotIndexFromEquippedSlotsIndex((int)this.InventorySlotType);
			}
		}

		// Token: 0x060019AD RID: 6573 RVA: 0x00076124 File Offset: 0x00074324
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.Clear();
			PlayerReferenceManager localPlayerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject();
			this.Inventory = localPlayerObject.Inventory;
			this.Inventory.OnEquippedSlotsChanged.AddListener(new UnityAction<NetworkListEvent<Inventory.EquipmentSlot>>(this.OnEquippedSlotsChanged));
			this.OnEquippedSlotsChanged(default(NetworkListEvent<Inventory.EquipmentSlot>));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.Clear));
			this.displayAndHideHandler.SetToDisplayEnd(true);
		}

		// Token: 0x060019AE RID: 6574 RVA: 0x000761B8 File Offset: 0x000743B8
		protected virtual void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1} {{ {2}: {3}, {4}: {5}, {6}: {7} }} )", new object[] { "OnEquippedSlotsChanged", "changeEvent", "Index", changeEvent.Index, "Value", changeEvent.Value, "PreviousValue", changeEvent.PreviousValue }), this);
			}
			if (this.HasEquipment)
			{
				this.Display();
				return;
			}
			this.Clear();
		}

		// Token: 0x060019AF RID: 6575 RVA: 0x0007624A File Offset: 0x0007444A
		protected void Display()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Display", this);
			}
			this.displayAndHideHandler.Display();
		}

		// Token: 0x060019B0 RID: 6576 RVA: 0x0007626A File Offset: 0x0007446A
		protected virtual void Clear()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			this.displayAndHideHandler.Hide();
		}

		// Token: 0x04001461 RID: 5217
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;
	}
}
