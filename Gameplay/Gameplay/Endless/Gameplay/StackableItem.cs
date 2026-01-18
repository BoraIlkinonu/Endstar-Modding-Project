using System;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002B8 RID: 696
	public abstract class StackableItem : Item
	{
		// Token: 0x17000313 RID: 787
		// (get) Token: 0x06000FC4 RID: 4036 RVA: 0x00017586 File Offset: 0x00015786
		public override bool IsStackable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000314 RID: 788
		// (get) Token: 0x06000FC5 RID: 4037 RVA: 0x0005183C File Offset: 0x0004FA3C
		public override int StackCount
		{
			get
			{
				return this.stackCount.Value;
			}
		}

		// Token: 0x17000315 RID: 789
		// (get) Token: 0x06000FC6 RID: 4038 RVA: 0x00051849 File Offset: 0x0004FA49
		// (set) Token: 0x06000FC7 RID: 4039 RVA: 0x00051851 File Offset: 0x0004FA51
		public int StartingStackCount
		{
			get
			{
				return this.StackCount;
			}
			set
			{
				if (base.IsServer)
				{
					this.stackCount.Value = value;
				}
			}
		}

		// Token: 0x06000FC8 RID: 4040 RVA: 0x00051867 File Offset: 0x0004FA67
		public override void CopyToItem(Item item)
		{
			(item as StackableItem).stackCount.Value = this.stackCount.Value;
			base.CopyToItem(item);
		}

		// Token: 0x06000FC9 RID: 4041 RVA: 0x0005188C File Offset: 0x0004FA8C
		public void ForceStackCount(int value)
		{
			if (base.IsServer)
			{
				if (value < 1)
				{
					this.stackCount.Value = 0;
					PlayerReferenceManager carrier = base.Carrier;
					if (carrier != null)
					{
						carrier.Inventory.StackCountChanged(this.inventoryUsableDefinition.Guid);
					}
					base.Destroy();
					return;
				}
				this.stackCount.Value = value;
				PlayerReferenceManager carrier2 = base.Carrier;
				if (carrier2 == null)
				{
					return;
				}
				carrier2.Inventory.StackCountChanged(this.inventoryUsableDefinition.Guid);
			}
		}

		// Token: 0x06000FCA RID: 4042 RVA: 0x00051908 File Offset: 0x0004FB08
		public void ChangeStackCount(int delta)
		{
			if (base.IsServer)
			{
				int num = Mathf.Max(0, this.stackCount.Value + delta);
				if (num < 1)
				{
					this.stackCount.Value = 0;
					PlayerReferenceManager carrier = base.Carrier;
					if (carrier != null)
					{
						carrier.Inventory.StackCountChanged(this.inventoryUsableDefinition.Guid);
					}
					base.Destroy();
					return;
				}
				if (num != this.stackCount.Value)
				{
					this.stackCount.Value = num;
					PlayerReferenceManager carrier2 = base.Carrier;
					if (carrier2 == null)
					{
						return;
					}
					carrier2.Inventory.StackCountChanged(this.inventoryUsableDefinition.Guid);
				}
			}
		}

		// Token: 0x06000FCB RID: 4043 RVA: 0x00051851 File Offset: 0x0004FA51
		protected override void LoadedCount(int count)
		{
			if (base.IsServer)
			{
				this.stackCount.Value = count;
			}
		}

		// Token: 0x06000FCC RID: 4044 RVA: 0x000519A6 File Offset: 0x0004FBA6
		public override void OnNetworkSpawn()
		{
			NetworkVariable<int> networkVariable = this.stackCount;
			networkVariable.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(this.HandleStackCountChanged));
			base.OnNetworkSpawn();
		}

		// Token: 0x06000FCD RID: 4045 RVA: 0x00002DB0 File Offset: 0x00000FB0
		private void HandleStackCountChanged(int previousValue, int value)
		{
		}

		// Token: 0x06000FCF RID: 4047 RVA: 0x000519EC File Offset: 0x0004FBEC
		protected override void __initializeVariables()
		{
			bool flag = this.stackCount == null;
			if (flag)
			{
				throw new Exception("StackableItem.stackCount cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.stackCount.Initialize(this);
			base.__nameNetworkVariable(this.stackCount, "stackCount");
			this.NetworkVariableFields.Add(this.stackCount);
			base.__initializeVariables();
		}

		// Token: 0x06000FD0 RID: 4048 RVA: 0x0004F016 File Offset: 0x0004D216
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000FD1 RID: 4049 RVA: 0x00051A4F File Offset: 0x0004FC4F
		protected internal override string __getTypeName()
		{
			return "StackableItem";
		}

		// Token: 0x04000DB6 RID: 3510
		private NetworkVariable<int> stackCount = new NetworkVariable<int>(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	}
}
