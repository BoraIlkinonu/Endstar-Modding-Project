using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000459 RID: 1113
	public class Lockable
	{
		// Token: 0x06001BD5 RID: 7125 RVA: 0x0007CBA5 File Offset: 0x0007ADA5
		internal Lockable(Lockable lockable)
		{
			this.lockable = lockable;
		}

		// Token: 0x06001BD6 RID: 7126 RVA: 0x0007CBB4 File Offset: 0x0007ADB4
		public KeyLibraryReference GetKeyReference()
		{
			return this.lockable.KeyReference;
		}

		// Token: 0x06001BD7 RID: 7127 RVA: 0x0007CBC1 File Offset: 0x0007ADC1
		public void Unlock(Context instigator)
		{
			this.lockable.Unlock(instigator);
		}

		// Token: 0x06001BD8 RID: 7128 RVA: 0x0007CBCF File Offset: 0x0007ADCF
		public bool GetIsLocked()
		{
			return this.lockable.IsLocked;
		}

		// Token: 0x040015BB RID: 5563
		private Lockable lockable;
	}
}
