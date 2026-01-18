using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class ResourcePickup
{
	private Endless.Gameplay.ResourcePickup instantPickup;

	public bool GetAllowPickupWhileDowned()
	{
		return instantPickup.AllowPickupWhileDowned;
	}

	public void SetAllowPickupWhileDowned(Context instigator, bool allow)
	{
		instantPickup.AllowPickupWhileDowned = allow;
	}

	internal ResourcePickup(Endless.Gameplay.ResourcePickup instantPickup)
	{
		this.instantPickup = instantPickup;
	}
}
