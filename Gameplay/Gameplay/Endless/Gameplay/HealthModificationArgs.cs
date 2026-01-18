using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay
{
	// Token: 0x020000CF RID: 207
	public struct HealthModificationArgs
	{
		// Token: 0x06000425 RID: 1061 RVA: 0x00016A29 File Offset: 0x00014C29
		public HealthModificationArgs(int delta, Context source, DamageType damageType = DamageType.Normal, HealthChangeType healthChangeType = HealthChangeType.Damage)
		{
			this.Delta = delta;
			this.Source = source;
			this.HealthChangeType = healthChangeType;
			this.DamageType = damageType;
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x00016A48 File Offset: 0x00014C48
		public HealthModificationArgs(int delta, WorldObject source, DamageType damageType = DamageType.Normal, HealthChangeType healthChangeType = HealthChangeType.Damage)
		{
			this.Delta = delta;
			this.Source = ((source != null) ? source.Context : null);
			this.HealthChangeType = healthChangeType;
			this.DamageType = damageType;
		}

		// Token: 0x040003A6 RID: 934
		public int Delta;

		// Token: 0x040003A7 RID: 935
		public Context Source;

		// Token: 0x040003A8 RID: 936
		public HealthChangeType HealthChangeType;

		// Token: 0x040003A9 RID: 937
		public DamageType DamageType;
	}
}
