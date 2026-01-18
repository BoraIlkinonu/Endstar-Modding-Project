using System;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay
{
	// Token: 0x020002D8 RID: 728
	public class GenericUsableDefinition : InventoryUsableDefinition
	{
		// Token: 0x0600107B RID: 4219 RVA: 0x00053395 File Offset: 0x00051595
		public override UsableDefinition.UseState GetNewUseState()
		{
			return new UsableDefinition.BlankUseState();
		}

		// Token: 0x0600107C RID: 4220 RVA: 0x0005339C File Offset: 0x0005159C
		public override void GetEventData(NetState state, UsableDefinition.UseState eus, double appearanceTime, ref InventoryUsableDefinition.EventData data)
		{
			base.GetEventData(state, eus, appearanceTime, ref data);
			data.InUse = eus != null;
			data.UseFrame = (data.InUse ? NetClock.CurrentFrame : 0U);
		}

		// Token: 0x0600107D RID: 4221 RVA: 0x000533CC File Offset: 0x000515CC
		public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UsableDefinition.UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
		{
			if (pressed && !state.BlockItemInput && equipped)
			{
				state.BlockItemInput = true;
				return true;
			}
			return false;
		}

		// Token: 0x0600107E RID: 4222 RVA: 0x000533ED File Offset: 0x000515ED
		public override UsableDefinition.UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
		{
			if (state.Grounded)
			{
				return UsableDefinition.UseState.GetStateFromPool(guid, item) as UsableDefinition.BlankUseState;
			}
			return null;
		}
	}
}
