using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Gameplay.LevelEditing;

namespace Endless.Gameplay.LevelEditing.Level.UpgradeVersions;

[Serializable]
public class Game_1_0 : Asset
{
	public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(1, 0, 0);

	public List<LevelReference> levels = new List<LevelReference>();

	[JsonProperty]
	public GameLibrary GameLibrary = new GameLibrary();

	[JsonProperty("screenshots")]
	public List<ScreenshotFileInstances> Screenshots = new List<ScreenshotFileInstances>();

	public int MininumNumberOfPlayers = 1;

	public int MaximumNumberOfPlayers = 10;

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
			InternalVersion = INTERNAL_VERSION.ToString()
		};
	}
}
