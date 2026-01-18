using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class MeleeWeapon
{
	private MeleeWeaponItem meleeWeaponItem;

	internal MeleeWeapon(MeleeWeaponItem meleeWeaponItem)
	{
		this.meleeWeaponItem = meleeWeaponItem;
	}

	public void SetDamageOnHit(Context instigator, int damage)
	{
		meleeWeaponItem.DamageOnHit = damage;
	}
}
