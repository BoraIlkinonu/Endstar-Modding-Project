using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003A0 RID: 928
	public class UIEquippedSlotModel : UIGameObject
	{
		// Token: 0x170004DD RID: 1245
		// (get) Token: 0x06001799 RID: 6041 RVA: 0x0006DC43 File Offset: 0x0006BE43
		// (set) Token: 0x0600179A RID: 6042 RVA: 0x0006DC4B File Offset: 0x0006BE4B
		public int Index { get; private set; }

		// Token: 0x170004DE RID: 1246
		// (get) Token: 0x0600179B RID: 6043 RVA: 0x0006DC54 File Offset: 0x0006BE54
		// (set) Token: 0x0600179C RID: 6044 RVA: 0x0006DC5C File Offset: 0x0006BE5C
		public Item.InventorySlotType InventorySlotType { get; private set; }

		// Token: 0x170004DF RID: 1247
		// (get) Token: 0x0600179D RID: 6045 RVA: 0x0006DC65 File Offset: 0x0006BE65
		public UnityEvent OnChanged { get; } = new UnityEvent();

		// Token: 0x170004E0 RID: 1248
		// (get) Token: 0x0600179E RID: 6046 RVA: 0x0006DC6D File Offset: 0x0006BE6D
		// (set) Token: 0x0600179F RID: 6047 RVA: 0x0006DC75 File Offset: 0x0006BE75
		public Inventory Inventory { get; private set; }

		// Token: 0x170004E1 RID: 1249
		// (get) Token: 0x060017A0 RID: 6048 RVA: 0x0006DC7E File Offset: 0x0006BE7E
		public Item Item
		{
			get
			{
				if (!(this.Inventory == null))
				{
					return this.Inventory.GetEquippedItem(this.Index);
				}
				return null;
			}
		}

		// Token: 0x060017A1 RID: 6049 RVA: 0x0006DCA4 File Offset: 0x0006BEA4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			PlayerReferenceManager localPlayerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject();
			if (localPlayerObject == null)
			{
				MonoBehaviourSingleton<PlayerManager>.Instance.OnOwnerRegistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.OnOwnerRegistered));
				return;
			}
			this.OnOwnerRegistered(localPlayerObject.OwnerClientId, localPlayerObject);
		}

		// Token: 0x060017A2 RID: 6050 RVA: 0x0006DD08 File Offset: 0x0006BF08
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
			this.Inventory.OnEquippedSlotsChanged.AddListener(new UnityAction<NetworkListEvent<Inventory.EquipmentSlot>>(this.OnEquippedSlotsChanged));
			this.Inventory.OnSlotsChanged.AddListener(new UnityAction<NetworkListEvent<InventorySlot>>(this.OnSlotsChanged));
			this.OnChanged.Invoke();
		}

		// Token: 0x060017A3 RID: 6051 RVA: 0x0006DDA3 File Offset: 0x0006BFA3
		private void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEquippedSlotsChanged", new object[] { changeEvent.Index });
			}
			this.OnChanged.Invoke();
		}

		// Token: 0x060017A4 RID: 6052 RVA: 0x0006DDD7 File Offset: 0x0006BFD7
		private void OnSlotsChanged(NetworkListEvent<InventorySlot> changeEvent)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSlotsChanged", new object[] { changeEvent.Index });
			}
			this.OnChanged.Invoke();
		}

		// Token: 0x04001304 RID: 4868
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
