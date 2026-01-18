using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200045F RID: 1119
	public class ProjectileShooter
	{
		// Token: 0x06001C06 RID: 7174 RVA: 0x0007D2CD File Offset: 0x0007B4CD
		internal ProjectileShooter(ProjectileShooterComponent projectileShooterComponent)
		{
			this.component = projectileShooterComponent;
		}

		// Token: 0x06001C07 RID: 7175 RVA: 0x0007D2DC File Offset: 0x0007B4DC
		public void Shoot(Context instigator)
		{
			this.component.Shoot();
		}

		// Token: 0x040015C1 RID: 5569
		private readonly ProjectileShooterComponent component;
	}
}
