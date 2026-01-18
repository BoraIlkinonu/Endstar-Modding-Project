using System;

namespace Endless.Gameplay
{
	// Token: 0x020000D2 RID: 210
	[Flags]
	public enum ReferenceFilter
	{
		// Token: 0x040003AC RID: 940
		None = 0,
		// Token: 0x040003AD RID: 941
		NonStatic = 1,
		// Token: 0x040003AE RID: 942
		Npc = 2,
		// Token: 0x040003AF RID: 943
		PhysicsObject = 4,
		// Token: 0x040003B0 RID: 944
		InventoryItem = 8,
		// Token: 0x040003B1 RID: 945
		Key = 16,
		// Token: 0x040003B2 RID: 946
		Resource = 32
	}
}
