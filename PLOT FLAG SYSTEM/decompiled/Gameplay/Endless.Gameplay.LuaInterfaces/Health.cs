using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class Health
{
	private readonly HealthComponent health;

	public LuaInterfaceEvent OnHealthChanged = new LuaInterfaceEvent();

	public readonly LuaInterfaceEvent OnDefeated = new LuaInterfaceEvent();

	internal Health(HealthComponent healthComponent)
	{
		health = healthComponent;
	}

	public void SetHealthZeroedBehavior(Context instigator, int behavior)
	{
		health.healthZeroedBehavior = (HealthZeroedBehavior)behavior;
	}

	public int GetHealth()
	{
		return health.currentHealth.Value;
	}

	public void SetHealth(Context instigator, int newValue)
	{
		health.SetHealth(newValue);
	}

	public void ChangeHealth(Context instigator, int delta)
	{
		HealthModificationArgs args = new HealthModificationArgs(delta, instigator, DamageType.Normal, HealthChangeType.HealthChanged);
		health.ModifyHealth(args);
	}

	public int GetMaxHealth()
	{
		return health.maxHealth.Value;
	}

	public void SetMaxHealth(Context instigator, int newValue)
	{
		health.SetMaxHealth(instigator, newValue);
	}

	public void ChangeMaxHealth(Context instigator, int delta)
	{
		SetMaxHealth(instigator, health.maxHealth.Value + delta);
	}
}
