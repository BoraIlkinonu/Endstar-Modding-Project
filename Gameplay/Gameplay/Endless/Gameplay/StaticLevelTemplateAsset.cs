using System;
using Endless.Gameplay.LevelEditing.Level;
using Newtonsoft.Json;
using Runtime.Gameplay.LevelEditing;

namespace Endless.Gameplay
{
	// Token: 0x020000ED RID: 237
	public class StaticLevelTemplateAsset
	{
		// Token: 0x06000553 RID: 1363 RVA: 0x0001B5AD File Offset: 0x000197AD
		public StaticLevelTemplateAsset(LevelState levelState, GameLibrary gameLibrary)
		{
			this.LevelState = levelState;
			this.GameLibrary = gameLibrary;
		}

		// Token: 0x04000411 RID: 1041
		[JsonProperty("levelState")]
		public readonly LevelState LevelState;

		// Token: 0x04000412 RID: 1042
		[JsonProperty("gameLibrary")]
		public readonly GameLibrary GameLibrary;
	}
}
