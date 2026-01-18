using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class ConsumableHealingUsableDefinition : ConsumableUsableDefinition
{
	[SerializeField]
	private uint applyHealingFrame = 12u;

	[SerializeField]
	private uint commitToHealFrame = 9u;

	public override UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
	{
		if (playerReference.HealthComponent.CurrentHealth < playerReference.HealthComponent.MaxHealth)
		{
			return UseState.GetStateFromPool(guid, item) as SimpleEquipmentUseState;
		}
		return null;
	}

	protected override void ProcessEquipmentFrame_Internal(PlayerReferenceManager playerReference, SimpleEquipmentUseState simpleEus)
	{
		base.ProcessEquipmentFrame_Internal(playerReference, simpleEus);
		if (simpleEus.Frame == applyHealingFrame && NetworkManager.Singleton.IsServer)
		{
			int delta = 4;
			ConsumableHealingItem consumableHealingItem = (ConsumableHealingItem)simpleEus.Item;
			if ((bool)simpleEus.Item)
			{
				delta = consumableHealingItem.HealAmount;
			}
			playerReference.HittableComponent.ModifyHealth(new HealthModificationArgs(delta, playerReference.WorldObject.Context));
		}
	}

	public override void GetEventData(NetState state, UseState eus, double appearanceTime, ref EventData data)
	{
		base.GetEventData(state, eus, appearanceTime, ref data);
		if (eus != null)
		{
			SimpleEquipmentUseState simpleEquipmentUseState = (SimpleEquipmentUseState)eus;
			data.Available = false;
			data.InUse = true;
			data.UseFrame = NetClock.CurrentFrame - simpleEquipmentUseState.Frame;
		}
		else
		{
			data.Available = true;
		}
	}

	public override EquipmentShowPriority GetShowPriority(UseState eus)
	{
		if (eus != null)
		{
			return EquipmentShowPriority.MinorInUse;
		}
		return EquipmentShowPriority.MinorOutOfUse;
	}

	public override uint GetItemSwapDelayFrames(UseState currentUseState)
	{
		SimpleEquipmentUseState simpleEquipmentUseState = (SimpleEquipmentUseState)currentUseState;
		if (commitToHealFrame > simpleEquipmentUseState.Frame)
		{
			return 0u;
		}
		return (uint)(useFramesDuration - simpleEquipmentUseState.Frame);
	}
}
