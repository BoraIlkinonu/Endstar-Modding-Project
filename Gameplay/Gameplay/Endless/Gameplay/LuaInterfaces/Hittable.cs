using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000452 RID: 1106
	public class Hittable
	{
		// Token: 0x06001BA2 RID: 7074 RVA: 0x0007C7F9 File Offset: 0x0007A9F9
		internal Hittable(HittableComponent hittable)
		{
			this.hittable = hittable;
		}

		// Token: 0x06001BA3 RID: 7075 RVA: 0x0007C808 File Offset: 0x0007AA08
		public void SetIsDamageable(Context instigator, bool isDamageable)
		{
			this.hittable.IsDamageable = isDamageable;
		}

		// Token: 0x06001BA4 RID: 7076 RVA: 0x0007C816 File Offset: 0x0007AA16
		public bool GetIsDamageable()
		{
			return this.hittable.IsDamageable;
		}

		// Token: 0x06001BA5 RID: 7077 RVA: 0x0007C823 File Offset: 0x0007AA23
		public void SetIsTargetable(Context instigator, bool isTargetable)
		{
			this.hittable.SetIsTargetable(isTargetable);
		}

		// Token: 0x06001BA6 RID: 7078 RVA: 0x0007C831 File Offset: 0x0007AA31
		public bool GetIsTargetable()
		{
			return this.hittable.IsTargetable;
		}

		// Token: 0x06001BA7 RID: 7079 RVA: 0x0007C83E File Offset: 0x0007AA3E
		public bool GetIsFullHealth()
		{
			return this.hittable.IsFullHealth;
		}

		// Token: 0x06001BA8 RID: 7080 RVA: 0x0007C84C File Offset: 0x0007AA4C
		public int ModifyHealth(Context instigator, int delta, int damageType)
		{
			HealthModificationArgs healthModificationArgs = new HealthModificationArgs(delta, instigator, (DamageType)damageType, HealthChangeType.Damage);
			return (int)this.hittable.ModifyHealth(healthModificationArgs);
		}

		// Token: 0x06001BA9 RID: 7081 RVA: 0x0007C870 File Offset: 0x0007AA70
		public bool GetIsDowned()
		{
			return this.hittable.healthComponent && this.hittable.healthComponent.CurrentHealth < 1;
		}

		// Token: 0x06001BAA RID: 7082 RVA: 0x0007C899 File Offset: 0x0007AA99
		public void ChangeThreatLevel(Context instigator, int threatLevel)
		{
			this.hittable.ThreatLevel = (ThreatLevel)threatLevel;
		}

		// Token: 0x040015B5 RID: 5557
		private readonly HittableComponent hittable;
	}
}
