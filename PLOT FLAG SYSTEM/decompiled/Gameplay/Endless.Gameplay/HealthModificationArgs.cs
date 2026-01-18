using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay;

public struct HealthModificationArgs
{
	public int Delta;

	public Context Source;

	public HealthChangeType HealthChangeType;

	public DamageType DamageType;

	public HealthModificationArgs(int delta, Context source, DamageType damageType = DamageType.Normal, HealthChangeType healthChangeType = HealthChangeType.Damage)
	{
		Delta = delta;
		Source = source;
		HealthChangeType = healthChangeType;
		DamageType = damageType;
	}

	public HealthModificationArgs(int delta, WorldObject source, DamageType damageType = DamageType.Normal, HealthChangeType healthChangeType = HealthChangeType.Damage)
	{
		Delta = delta;
		Source = source?.Context;
		HealthChangeType = healthChangeType;
		DamageType = damageType;
	}
}
