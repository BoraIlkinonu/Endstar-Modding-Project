using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000444 RID: 1092
	public class BouncePad
	{
		// Token: 0x06001B57 RID: 6999 RVA: 0x0007C2A4 File Offset: 0x0007A4A4
		internal BouncePad(BouncePad bouncePad)
		{
			this.bouncePad = bouncePad;
		}

		// Token: 0x06001B58 RID: 7000 RVA: 0x0007C2B3 File Offset: 0x0007A4B3
		public void SetBounceHeight(Context instigator, int value)
		{
			this.bouncePad.SetBounceHeight(value);
		}

		// Token: 0x06001B59 RID: 7001 RVA: 0x0007C2C1 File Offset: 0x0007A4C1
		public int GetBounceHeight()
		{
			return this.bouncePad.heightInfo.Value.Height;
		}

		// Token: 0x06001B5A RID: 7002 RVA: 0x0007C2D8 File Offset: 0x0007A4D8
		public void Activate(Context instigator)
		{
			this.bouncePad.Activate();
		}

		// Token: 0x06001B5B RID: 7003 RVA: 0x0007C2E5 File Offset: 0x0007A4E5
		public void Deactivate(Context instigator)
		{
			this.bouncePad.Deactivate();
		}

		// Token: 0x06001B5C RID: 7004 RVA: 0x0007C2F2 File Offset: 0x0007A4F2
		public bool GetActive()
		{
			return this.bouncePad.toggleInfo.Value.Active;
		}

		// Token: 0x040015A9 RID: 5545
		private BouncePad bouncePad;
	}
}
