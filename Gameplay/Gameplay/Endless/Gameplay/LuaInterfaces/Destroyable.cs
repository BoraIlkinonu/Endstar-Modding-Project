using System;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000449 RID: 1097
	internal class Destroyable
	{
		// Token: 0x06001B77 RID: 7031 RVA: 0x0007C525 File Offset: 0x0007A725
		internal Destroyable(DestroyableComponent destroyableComponent)
		{
			this.destroyable = destroyableComponent;
		}

		// Token: 0x040015AD RID: 5549
		private readonly DestroyableComponent destroyable;
	}
}
