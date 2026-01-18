using Endless.Shared.DataTypes;

namespace Endless.Gameplay;

public class GenericUsableDefinition : InventoryUsableDefinition
{
	public override UseState GetNewUseState()
	{
		return new BlankUseState();
	}

	public override void GetEventData(NetState state, UseState eus, double appearanceTime, ref EventData data)
	{
		base.GetEventData(state, eus, appearanceTime, ref data);
		data.InUse = eus != null;
		data.UseFrame = (data.InUse ? NetClock.CurrentFrame : 0u);
	}

	public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
	{
		if (pressed && !state.BlockItemInput && equipped)
		{
			state.BlockItemInput = true;
			return true;
		}
		return false;
	}

	public override UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
	{
		if (state.Grounded)
		{
			return UseState.GetStateFromPool(guid, item) as BlankUseState;
		}
		return null;
	}
}
