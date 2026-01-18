using System;

namespace Endless.Core
{
	// Token: 0x02000029 RID: 41
	public static class GameStateExtensions
	{
		// Token: 0x0600008A RID: 138 RVA: 0x00004A38 File Offset: 0x00002C38
		public static bool IsInCreatorCategory(this GameState state)
		{
			return state == GameState.Creator || state == GameState.LoadingCreator;
		}
	}
}
