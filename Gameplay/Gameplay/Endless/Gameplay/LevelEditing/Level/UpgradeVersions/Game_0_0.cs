using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level.UpgradeVersions
{
	// Token: 0x02000598 RID: 1432
	[Serializable]
	public class Game_0_0 : Asset
	{
		// Token: 0x04001B6E RID: 7022
		public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(0, 0, 0);

		// Token: 0x04001B6F RID: 7023
		public List<LevelReference> levels = new List<LevelReference>();

		// Token: 0x04001B70 RID: 7024
		[JsonProperty]
		public Game_0_0.GameLibrary_0_0 GameLibrary = new Game_0_0.GameLibrary_0_0();

		// Token: 0x04001B71 RID: 7025
		[JsonProperty("screenshots")]
		public List<ScreenshotFileInstances> Screenshots = new List<ScreenshotFileInstances>();

		// Token: 0x04001B72 RID: 7026
		public int MininumNumberOfPlayers = 1;

		// Token: 0x04001B73 RID: 7027
		public int MaximumNumberOfPlayers = 10;

		// Token: 0x02000599 RID: 1433
		[Serializable]
		public class GameLibrary_0_0
		{
			// Token: 0x04001B74 RID: 7028
			[SerializeField]
			[JsonProperty]
			public List<Game_0_0.TerrainUsage_0_0> terrainEntries = new List<Game_0_0.TerrainUsage_0_0>();
		}

		// Token: 0x0200059A RID: 1434
		[Serializable]
		public class TerrainUsage_0_0
		{
			// Token: 0x04001B75 RID: 7029
			[SerializeField]
			[JsonProperty("TerrainId")]
			public SerializableGuid terrainId;

			// Token: 0x04001B76 RID: 7030
			public string TerrainVersion;

			// Token: 0x04001B77 RID: 7031
			public bool IsActive;

			// Token: 0x04001B78 RID: 7032
			public int RedirectIndex;
		}
	}
}
