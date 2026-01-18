using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class Hittable
{
	private readonly HittableComponent hittable;

	internal Hittable(HittableComponent hittable)
	{
		this.hittable = hittable;
	}

	public void SetIsDamageable(Context instigator, bool isDamageable)
	{
		hittable.IsDamageable = isDamageable;
	}

	public bool GetIsDamageable()
	{
		return hittable.IsDamageable;
	}

	public void SetIsTargetable(Context instigator, bool isTargetable)
	{
		hittable.SetIsTargetable(isTargetable);
	}

	public bool GetIsTargetable()
	{
		return hittable.IsTargetable;
	}

	public bool GetIsFullHealth()
	{
		return hittable.IsFullHealth;
	}

	public int ModifyHealth(Context instigator, int delta, int damageType)
	{
		HealthModificationArgs modificationArgs = new HealthModificationArgs(delta, instigator, (DamageType)damageType);
		return (int)hittable.ModifyHealth(modificationArgs);
	}

	public bool GetIsDowned()
	{
		if ((bool)hittable.healthComponent)
		{
			return hittable.healthComponent.CurrentHealth < 1;
		}
		return false;
	}

	public void ChangeThreatLevel(Context instigator, int threatLevel)
	{
		hittable.ThreatLevel = (ThreatLevel)threatLevel;
	}
}
