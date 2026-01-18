using System;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002D6 RID: 726
	public class ConsumableUsableDefinition : InventoryUsableDefinition
	{
		// Token: 0x06001073 RID: 4211 RVA: 0x0005328C File Offset: 0x0005148C
		public override UsableDefinition.UseState GetNewUseState()
		{
			return new ConsumableUsableDefinition.SimpleEquipmentUseState();
		}

		// Token: 0x06001074 RID: 4212 RVA: 0x00053293 File Offset: 0x00051493
		public override void GetEventData(NetState state, UsableDefinition.UseState eus, double appearanceTime, ref InventoryUsableDefinition.EventData data)
		{
			base.GetEventData(state, eus, appearanceTime, ref data);
			data.InUse = eus != null;
		}

		// Token: 0x06001075 RID: 4213 RVA: 0x000532AB File Offset: 0x000514AB
		public override UsableDefinition.UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
		{
			return UsableDefinition.UseState.GetStateFromPool(guid, item) as ConsumableUsableDefinition.SimpleEquipmentUseState;
		}

		// Token: 0x06001076 RID: 4214 RVA: 0x000532BC File Offset: 0x000514BC
		public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UsableDefinition.UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
		{
			if (!equipped || (this.cancelWhenDowned && (state.Downed || playerReference.HealthComponent.CurrentHealth < 1)))
			{
				return false;
			}
			ConsumableUsableDefinition.SimpleEquipmentUseState simpleEquipmentUseState = equipmentUseState as ConsumableUsableDefinition.SimpleEquipmentUseState;
			simpleEquipmentUseState.Frame += 1U;
			this.ProcessEquipmentFrame_Internal(playerReference, simpleEquipmentUseState);
			bool flag = (ulong)simpleEquipmentUseState.Frame > (ulong)((long)this.useFramesDuration);
			state.BlockItemInput = true;
			if (flag && NetworkManager.Singleton.IsServer)
			{
				StackableItem stackableItem = (StackableItem)simpleEquipmentUseState.Item;
				if (stackableItem)
				{
					stackableItem.ChangeStackCount(-1);
				}
			}
			return !flag;
		}

		// Token: 0x06001077 RID: 4215 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void ProcessEquipmentFrame_Internal(PlayerReferenceManager playerReference, ConsumableUsableDefinition.SimpleEquipmentUseState simpleEus)
		{
		}

		// Token: 0x04000E19 RID: 3609
		[SerializeField]
		protected int useFramesDuration = 24;

		// Token: 0x04000E1A RID: 3610
		[SerializeField]
		private bool cancelWhenDowned = true;

		// Token: 0x020002D7 RID: 727
		public class SimpleEquipmentUseState : UsableDefinition.UseState
		{
			// Token: 0x06001079 RID: 4217 RVA: 0x0005336A File Offset: 0x0005156A
			public override void Serialize<T>(BufferSerializer<T> serializer)
			{
				base.Serialize<T>(serializer);
				if (serializer.IsWriter)
				{
					Compression.SerializeUInt<T>(serializer, this.Frame);
					return;
				}
				this.Frame = Compression.DeserializeUInt<T>(serializer);
			}

			// Token: 0x04000E1B RID: 3611
			public uint Frame;
		}
	}
}
