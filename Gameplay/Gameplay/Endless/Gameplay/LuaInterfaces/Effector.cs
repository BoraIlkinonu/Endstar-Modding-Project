using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200044B RID: 1099
	public class Effector
	{
		// Token: 0x06001B7D RID: 7037 RVA: 0x0007C57C File Offset: 0x0007A77C
		internal Effector(PeriodicEffector periodicEffector)
		{
			this.periodicEffector = periodicEffector;
		}

		// Token: 0x06001B7E RID: 7038 RVA: 0x0007C58B File Offset: 0x0007A78B
		public void AddContext(Context instigator, Context target)
		{
			this.periodicEffector.AddContext(target);
		}

		// Token: 0x06001B7F RID: 7039 RVA: 0x0007C599 File Offset: 0x0007A799
		public void RemoveContext(Context instigator, Context target)
		{
			this.periodicEffector.RemoveContext(target);
		}

		// Token: 0x06001B80 RID: 7040 RVA: 0x0007C5A7 File Offset: 0x0007A7A7
		public void SetInitialInterval(Context instigator, float newInterval)
		{
			this.periodicEffector.InitialInterval = newInterval;
		}

		// Token: 0x06001B81 RID: 7041 RVA: 0x0007C5B5 File Offset: 0x0007A7B5
		public void SetIntervalScalar(Context instigator, float newScalar)
		{
			this.periodicEffector.IntervalScalar = newScalar;
		}

		// Token: 0x06001B82 RID: 7042 RVA: 0x0007C5C3 File Offset: 0x0007A7C3
		public void Deactivate(Context instigator)
		{
			this.periodicEffector.DeactivateEffector(instigator);
		}

		// Token: 0x06001B83 RID: 7043 RVA: 0x0007C5D1 File Offset: 0x0007A7D1
		public void Activate(Context instigator)
		{
			this.periodicEffector.ActivateEffector(instigator);
		}

		// Token: 0x040015AF RID: 5551
		private readonly PeriodicEffector periodicEffector;
	}
}
