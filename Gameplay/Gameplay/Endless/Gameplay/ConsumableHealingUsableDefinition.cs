using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002D5 RID: 725
	public class ConsumableHealingUsableDefinition : ConsumableUsableDefinition
	{
		// Token: 0x0600106D RID: 4205 RVA: 0x00053152 File Offset: 0x00051352
		public override UsableDefinition.UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
		{
			if (playerReference.HealthComponent.CurrentHealth < playerReference.HealthComponent.MaxHealth)
			{
				return UsableDefinition.UseState.GetStateFromPool(guid, item) as ConsumableUsableDefinition.SimpleEquipmentUseState;
			}
			return null;
		}

		// Token: 0x0600106E RID: 4206 RVA: 0x00053180 File Offset: 0x00051380
		protected override void ProcessEquipmentFrame_Internal(PlayerReferenceManager playerReference, ConsumableUsableDefinition.SimpleEquipmentUseState simpleEus)
		{
			base.ProcessEquipmentFrame_Internal(playerReference, simpleEus);
			if (simpleEus.Frame == this.applyHealingFrame && NetworkManager.Singleton.IsServer)
			{
				int num = 4;
				ConsumableHealingItem consumableHealingItem = (ConsumableHealingItem)simpleEus.Item;
				if (simpleEus.Item)
				{
					num = consumableHealingItem.HealAmount;
				}
				playerReference.HittableComponent.ModifyHealth(new HealthModificationArgs(num, playerReference.WorldObject.Context, DamageType.Normal, HealthChangeType.Damage));
			}
		}

		// Token: 0x0600106F RID: 4207 RVA: 0x000531F0 File Offset: 0x000513F0
		public override void GetEventData(NetState state, UsableDefinition.UseState eus, double appearanceTime, ref InventoryUsableDefinition.EventData data)
		{
			base.GetEventData(state, eus, appearanceTime, ref data);
			if (eus != null)
			{
				ConsumableUsableDefinition.SimpleEquipmentUseState simpleEquipmentUseState = (ConsumableUsableDefinition.SimpleEquipmentUseState)eus;
				data.Available = false;
				data.InUse = true;
				data.UseFrame = NetClock.CurrentFrame - simpleEquipmentUseState.Frame;
				return;
			}
			data.Available = true;
		}

		// Token: 0x06001070 RID: 4208 RVA: 0x00053092 File Offset: 0x00051292
		public override InventoryUsableDefinition.EquipmentShowPriority GetShowPriority(UsableDefinition.UseState eus)
		{
			if (eus != null)
			{
				return InventoryUsableDefinition.EquipmentShowPriority.MinorInUse;
			}
			return InventoryUsableDefinition.EquipmentShowPriority.MinorOutOfUse;
		}

		// Token: 0x06001071 RID: 4209 RVA: 0x00053240 File Offset: 0x00051440
		public override uint GetItemSwapDelayFrames(UsableDefinition.UseState currentUseState)
		{
			ConsumableUsableDefinition.SimpleEquipmentUseState simpleEquipmentUseState = (ConsumableUsableDefinition.SimpleEquipmentUseState)currentUseState;
			if (this.commitToHealFrame > simpleEquipmentUseState.Frame)
			{
				return 0U;
			}
			return (uint)((long)this.useFramesDuration - (long)((ulong)simpleEquipmentUseState.Frame));
		}

		// Token: 0x04000E17 RID: 3607
		[SerializeField]
		private uint applyHealingFrame = 12U;

		// Token: 0x04000E18 RID: 3608
		[SerializeField]
		private uint commitToHealFrame = 9U;
	}
}
