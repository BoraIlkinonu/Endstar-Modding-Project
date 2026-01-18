using System;

namespace Endless.Core
{
	// Token: 0x02000028 RID: 40
	public enum GameState
	{
		// Token: 0x0400005D RID: 93
		None = -1,
		// Token: 0x0400005E RID: 94
		Default,
		// Token: 0x0400005F RID: 95
		ValidatingLibrary,
		// Token: 0x04000060 RID: 96
		LoadingCreator,
		// Token: 0x04000061 RID: 97
		Creator,
		// Token: 0x04000062 RID: 98
		LoadingGameplay,
		// Token: 0x04000063 RID: 99
		LoadedGameplay,
		// Token: 0x04000064 RID: 100
		StartingGameplay,
		// Token: 0x04000065 RID: 101
		Gameplay,
		// Token: 0x04000066 RID: 102
		GameplayOutro
	}
}
