using System;

namespace Endless.Gameplay
{
	// Token: 0x0200015B RID: 347
	public static class WeaponExtensions
	{
		// Token: 0x06000826 RID: 2086 RVA: 0x000265C8 File Offset: 0x000247C8
		public static int GetWeaponAnimationNumber(this NpcEnum.Weapon weapon)
		{
			int num;
			switch (weapon)
			{
			case NpcEnum.Weapon.None:
				num = 0;
				break;
			case NpcEnum.Weapon.Sword1H:
				num = 1;
				break;
			case NpcEnum.Weapon.Sword2H:
				num = 1;
				break;
			case NpcEnum.Weapon.Ranged1H:
				num = 3;
				break;
			case NpcEnum.Weapon.Ranged2H:
				num = 4;
				break;
			case NpcEnum.Weapon.RocketLauncher:
				num = 5;
				break;
			default:
				throw new ArgumentOutOfRangeException("weapon", weapon, null);
			}
			return num;
		}
	}
}
