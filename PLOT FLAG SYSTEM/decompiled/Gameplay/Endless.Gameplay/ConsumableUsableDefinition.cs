using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class ConsumableUsableDefinition : InventoryUsableDefinition
{
	public class SimpleEquipmentUseState : UseState
	{
		public uint Frame;

		public override void Serialize<T>(BufferSerializer<T> serializer)
		{
			base.Serialize(serializer);
			if (serializer.IsWriter)
			{
				Compression.SerializeUInt(serializer, Frame);
			}
			else
			{
				Frame = Compression.DeserializeUInt(serializer);
			}
		}
	}

	[SerializeField]
	protected int useFramesDuration = 24;

	[SerializeField]
	private bool cancelWhenDowned = true;

	public override UseState GetNewUseState()
	{
		return new SimpleEquipmentUseState();
	}

	public override void GetEventData(NetState state, UseState eus, double appearanceTime, ref EventData data)
	{
		base.GetEventData(state, eus, appearanceTime, ref data);
		data.InUse = eus != null;
	}

	public override UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
	{
		return UseState.GetStateFromPool(guid, item) as SimpleEquipmentUseState;
	}

	public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
	{
		if (!equipped || (cancelWhenDowned && (state.Downed || playerReference.HealthComponent.CurrentHealth < 1)))
		{
			return false;
		}
		SimpleEquipmentUseState simpleEquipmentUseState = equipmentUseState as SimpleEquipmentUseState;
		simpleEquipmentUseState.Frame++;
		ProcessEquipmentFrame_Internal(playerReference, simpleEquipmentUseState);
		bool num = simpleEquipmentUseState.Frame > useFramesDuration;
		state.BlockItemInput = true;
		if (num && NetworkManager.Singleton.IsServer)
		{
			StackableItem stackableItem = (StackableItem)simpleEquipmentUseState.Item;
			if ((bool)stackableItem)
			{
				stackableItem.ChangeStackCount(-1);
			}
		}
		if (!num)
		{
			return true;
		}
		return false;
	}

	protected virtual void ProcessEquipmentFrame_Internal(PlayerReferenceManager playerReference, SimpleEquipmentUseState simpleEus)
	{
	}
}
