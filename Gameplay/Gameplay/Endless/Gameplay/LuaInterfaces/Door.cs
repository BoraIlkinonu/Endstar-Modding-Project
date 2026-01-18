using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200044A RID: 1098
	public class Door
	{
		// Token: 0x06001B78 RID: 7032 RVA: 0x0007C534 File Offset: 0x0007A734
		internal Door(Door door)
		{
			this.door = door;
		}

		// Token: 0x06001B79 RID: 7033 RVA: 0x0007C543 File Offset: 0x0007A743
		public void Open(Context instigator, bool forward)
		{
			this.door.Open(instigator, forward);
		}

		// Token: 0x06001B7A RID: 7034 RVA: 0x0007C552 File Offset: 0x0007A752
		public void OpenFromUser(Context instigator)
		{
			this.door.OpenFromUser(instigator);
		}

		// Token: 0x06001B7B RID: 7035 RVA: 0x0007C560 File Offset: 0x0007A760
		public void ToggleOpenFromUser(Context instigator)
		{
			this.door.ToggleOpenFromUser(instigator);
		}

		// Token: 0x06001B7C RID: 7036 RVA: 0x0007C56E File Offset: 0x0007A76E
		public void Close(Context instigator)
		{
			this.door.Close(instigator);
		}

		// Token: 0x040015AE RID: 5550
		private readonly Door door;
	}
}
