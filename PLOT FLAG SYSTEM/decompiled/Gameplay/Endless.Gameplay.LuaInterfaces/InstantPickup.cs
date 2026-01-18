using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class InstantPickup
{
	private Endless.Gameplay.InstantPickup instantPickup;

	public bool GetAllowPickupWhileDowned()
	{
		return instantPickup.AllowPickupWhileDowned;
	}

	public void SetAllowPickupWhileDowned(Context instigator, bool allow)
	{
		instantPickup.AllowPickupWhileDowned = allow;
	}

	public PickupFilter GetPickupFilter()
	{
		return instantPickup.CurrentPickupFilter;
	}

	public void SetPickupFilter(Context instigator, int newFilter)
	{
		instantPickup.CurrentPickupFilter = (PickupFilter)newFilter;
	}

	public void Respawn(Context instigator)
	{
		instantPickup.Respawn();
	}

	public void ForceCollect(Context instigator)
	{
		instantPickup.ForceCollect();
	}

	internal InstantPickup(Endless.Gameplay.InstantPickup instantPickup)
	{
		this.instantPickup = instantPickup;
	}
}
