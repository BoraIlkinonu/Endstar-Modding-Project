using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000451 RID: 1105
	public class Health
	{
		// Token: 0x06001B9A RID: 7066 RVA: 0x0007C742 File Offset: 0x0007A942
		internal Health(HealthComponent healthComponent)
		{
			this.health = healthComponent;
		}

		// Token: 0x06001B9B RID: 7067 RVA: 0x0007C767 File Offset: 0x0007A967
		public void SetHealthZeroedBehavior(Context instigator, int behavior)
		{
			this.health.healthZeroedBehavior = (HealthZeroedBehavior)behavior;
		}

		// Token: 0x06001B9C RID: 7068 RVA: 0x0007C775 File Offset: 0x0007A975
		public int GetHealth()
		{
			return this.health.currentHealth.Value;
		}

		// Token: 0x06001B9D RID: 7069 RVA: 0x0007C787 File Offset: 0x0007A987
		public void SetHealth(Context instigator, int newValue)
		{
			this.health.SetHealth(newValue);
		}

		// Token: 0x06001B9E RID: 7070 RVA: 0x0007C798 File Offset: 0x0007A998
		public void ChangeHealth(Context instigator, int delta)
		{
			HealthModificationArgs healthModificationArgs = new HealthModificationArgs(delta, instigator, DamageType.Normal, HealthChangeType.HealthChanged);
			this.health.ModifyHealth(healthModificationArgs);
		}

		// Token: 0x06001B9F RID: 7071 RVA: 0x0007C7BD File Offset: 0x0007A9BD
		public int GetMaxHealth()
		{
			return this.health.maxHealth.Value;
		}

		// Token: 0x06001BA0 RID: 7072 RVA: 0x0007C7CF File Offset: 0x0007A9CF
		public void SetMaxHealth(Context instigator, int newValue)
		{
			this.health.SetMaxHealth(instigator, newValue);
		}

		// Token: 0x06001BA1 RID: 7073 RVA: 0x0007C7DE File Offset: 0x0007A9DE
		public void ChangeMaxHealth(Context instigator, int delta)
		{
			this.SetMaxHealth(instigator, this.health.maxHealth.Value + delta);
		}

		// Token: 0x040015B2 RID: 5554
		private readonly HealthComponent health;

		// Token: 0x040015B3 RID: 5555
		public LuaInterfaceEvent OnHealthChanged = new LuaInterfaceEvent();

		// Token: 0x040015B4 RID: 5556
		public readonly LuaInterfaceEvent OnDefeated = new LuaInterfaceEvent();
	}
}
