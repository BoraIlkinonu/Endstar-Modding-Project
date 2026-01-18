using System;

namespace Endless.Gameplay;

public static class WeaponExtensions
{
	public static int GetWeaponAnimationNumber(this NpcEnum.Weapon weapon)
	{
		return weapon switch
		{
			NpcEnum.Weapon.None => 0, 
			NpcEnum.Weapon.Sword1H => 1, 
			NpcEnum.Weapon.Sword2H => 1, 
			NpcEnum.Weapon.Ranged1H => 3, 
			NpcEnum.Weapon.Ranged2H => 4, 
			NpcEnum.Weapon.RocketLauncher => 5, 
			_ => throw new ArgumentOutOfRangeException("weapon", weapon, null), 
		};
	}
}
