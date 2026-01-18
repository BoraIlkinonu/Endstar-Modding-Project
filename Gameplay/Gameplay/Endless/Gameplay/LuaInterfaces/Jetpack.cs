using System;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000458 RID: 1112
	public class Jetpack
	{
		// Token: 0x06001BD4 RID: 7124 RVA: 0x0007CB96 File Offset: 0x0007AD96
		internal Jetpack(JetpackItem jetpackItem)
		{
			this.jetpackItem = jetpackItem;
		}

		// Token: 0x040015BA RID: 5562
		private JetpackItem jetpackItem;
	}
}
