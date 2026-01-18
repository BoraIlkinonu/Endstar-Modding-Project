using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Gameplay.LevelEditing;

namespace Endless.Gameplay.LevelEditing.Level.UpgradeVersions
{
	// Token: 0x0200059B RID: 1435
	[Serializable]
	public class Game_1_0 : Asset
	{
		// Token: 0x06002289 RID: 8841 RVA: 0x0009E888 File Offset: 0x0009CA88
		public static Game_1_0 Upgrade(Game_0_0 oldGame)
		{
			return new Game_1_0
			{
				AssetID = oldGame.AssetID,
				AssetType = oldGame.AssetType,
				AssetVersion = oldGame.AssetVersion,
				Description = oldGame.Description,
				levels = oldGame.levels,
				MaximumNumberOfPlayers = oldGame.MaximumNumberOfPlayers,
				MininumNumberOfPlayers = oldGame.MininumNumberOfPlayers,
				Name = oldGame.Name,
				RevisionMetaData = oldGame.RevisionMetaData,
				Screenshots = oldGame.Screenshots,
				GameLibrary = GameLibrary.Upgrade(oldGame.GameLibrary),
				InternalVersion = Game_1_0.INTERNAL_VERSION.ToString()
			};
		}

		// Token: 0x04001B79 RID: 7033
		public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(1, 0, 0);

		// Token: 0x04001B7A RID: 7034
		public List<LevelReference> levels = new List<LevelReference>();

		// Token: 0x04001B7B RID: 7035
		[JsonProperty]
		public GameLibrary GameLibrary = new GameLibrary();

		// Token: 0x04001B7C RID: 7036
		[JsonProperty("screenshots")]
		public List<ScreenshotFileInstances> Screenshots = new List<ScreenshotFileInstances>();

		// Token: 0x04001B7D RID: 7037
		public int MininumNumberOfPlayers = 1;

		// Token: 0x04001B7E RID: 7038
		public int MaximumNumberOfPlayers = 10;
	}
}
