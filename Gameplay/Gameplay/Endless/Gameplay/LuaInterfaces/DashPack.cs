using System;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000448 RID: 1096
	public class DashPack
	{
		// Token: 0x06001B76 RID: 7030 RVA: 0x0007C516 File Offset: 0x0007A716
		internal DashPack(DashPackItem dashPackItem)
		{
			this.dashPackItem = dashPackItem;
		}

		// Token: 0x040015AC RID: 5548
		private DashPackItem dashPackItem;
	}
}
