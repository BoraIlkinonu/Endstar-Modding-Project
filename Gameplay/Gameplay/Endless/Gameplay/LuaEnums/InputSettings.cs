using System;

namespace Endless.Gameplay.LuaEnums
{
	// Token: 0x02000476 RID: 1142
	[Flags]
	public enum InputSettings
	{
		// Token: 0x040015FA RID: 5626
		None = 0,
		// Token: 0x040015FB RID: 5627
		Walk = 1,
		// Token: 0x040015FC RID: 5628
		Run = 2,
		// Token: 0x040015FD RID: 5629
		Jump = 4,
		// Token: 0x040015FE RID: 5630
		Equipment = 8,
		// Token: 0x040015FF RID: 5631
		Interaction = 16
	}
}
