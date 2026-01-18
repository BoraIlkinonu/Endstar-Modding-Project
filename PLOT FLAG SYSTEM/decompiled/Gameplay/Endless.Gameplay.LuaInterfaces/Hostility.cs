using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class Hostility
{
	private readonly HostilityComponent hostilityComponent;

	internal Hostility(HostilityComponent hostility)
	{
		hostilityComponent = hostility;
	}

	public void SetHostilityLossRate(Context instigator, float value)
	{
		hostilityComponent.HostilityLossRate = value;
	}

	public void SetHostilityDamageAddend(Context instigator, float value)
	{
		hostilityComponent.HostilityDamageAddend = value;
	}
}
