using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Level.UpgradeVersions;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Assets
{
	// Token: 0x02000056 RID: 86
	public static class GameLoader
	{
		// Token: 0x06000148 RID: 328 RVA: 0x00007954 File Offset: 0x00005B54
		public static Game Load(string assetJson)
		{
			SemanticVersion semanticVersion = SemanticVersion.Parse(JsonConvert.DeserializeObject<Asset>(assetJson).InternalVersion);
			if (semanticVersion.Major >= Game.INTERNAL_VERSION.Major && semanticVersion.Minor >= Game.INTERNAL_VERSION.Minor)
			{
				return JsonConvert.DeserializeObject<Game>(assetJson);
			}
			if (semanticVersion == Game_1_0.INTERNAL_VERSION)
			{
				return Game.Upgrade(JsonConvert.DeserializeObject<Game_1_0>(assetJson));
			}
			return GameLoader.Load(JsonConvert.SerializeObject(Game_1_0.Upgrade(JsonConvert.DeserializeObject<Game_0_0>(assetJson))));
		}
	}
}
