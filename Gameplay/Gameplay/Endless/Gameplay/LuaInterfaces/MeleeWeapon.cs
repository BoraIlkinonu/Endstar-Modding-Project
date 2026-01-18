using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200045A RID: 1114
	public class MeleeWeapon
	{
		// Token: 0x06001BD9 RID: 7129 RVA: 0x0007CBDC File Offset: 0x0007ADDC
		internal MeleeWeapon(MeleeWeaponItem meleeWeaponItem)
		{
			this.meleeWeaponItem = meleeWeaponItem;
		}

		// Token: 0x06001BDA RID: 7130 RVA: 0x0007CBEB File Offset: 0x0007ADEB
		public void SetDamageOnHit(Context instigator, int damage)
		{
			this.meleeWeaponItem.DamageOnHit = damage;
		}

		// Token: 0x040015BC RID: 5564
		private MeleeWeaponItem meleeWeaponItem;
	}
}
