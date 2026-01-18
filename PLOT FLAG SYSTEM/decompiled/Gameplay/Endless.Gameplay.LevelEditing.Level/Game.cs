using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level.UpgradeVersions;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Gameplay.LevelEditing;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class Game : Asset
{
	public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(1, 1, 0);

	public List<LevelReference> levels = new List<LevelReference>();

	[JsonProperty]
	public GameLibrary GameLibrary = new GameLibrary();

	[JsonProperty("screenshots")]
	public List<ScreenshotFileInstances> Screenshots = new List<ScreenshotFileInstances>();

	public int MininumNumberOfPlayers = 1;

	public int MaximumNumberOfPlayers = 10;

	public Game()
	{
		AssetType = "game";
		Name = "Game";
		InternalVersion = INTERNAL_VERSION.ToString();
	}

	public object GetAnonymousObjectForUpload(LevelState newLevelState)
	{
		object[] array = ((newLevelState == null) ? new object[levels.Count] : new object[levels.Count + 1]);
		for (int i = 0; i < levels.Count; i++)
		{
			array[i] = levels[i].GetAnonymousObject();
		}
		if (newLevelState != null)
		{
			array[levels.Count] = newLevelState.GetAnonymousObjectForUpload();
		}
		return new
		{
			asset_id = AssetID,
			internal_version = InternalVersion,
			asset_version = string.Empty,
			asset_type = "game",
			Name = Name,
			Description = Description,
			GameLibrary = GameLibrary,
			levels = array,
			MininumNumberOfPlayers = MininumNumberOfPlayers,
			MaximumNumberOfPlayers = MaximumNumberOfPlayers,
			revision_meta_data = RevisionMetaData,
			screenshots = Screenshots
		};
	}

	public Game Clone()
	{
		return JsonConvert.DeserializeObject<Game>(JsonConvert.SerializeObject(this));
	}

	public static Game Upgrade(Game_1_0 oldGame)
	{
		Game game = new Game();
		game.AssetID = oldGame.AssetID;
		game.AssetType = oldGame.AssetType;
		game.AssetVersion = oldGame.AssetVersion;
		game.Description = oldGame.Description;
		game.levels = oldGame.levels;
		game.MaximumNumberOfPlayers = oldGame.MaximumNumberOfPlayers;
		game.MininumNumberOfPlayers = oldGame.MininumNumberOfPlayers;
		game.Name = oldGame.Name;
		game.RevisionMetaData = oldGame.RevisionMetaData;
		game.Screenshots = oldGame.Screenshots;
		game.GameLibrary = oldGame.GameLibrary;
		foreach (TerrainUsage terrainEntry in game.GameLibrary.TerrainEntries)
		{
			if (terrainEntry.IsActive)
			{
				terrainEntry.TerrainAssetReference.AssetType = "terrain-tileset-cosmetic";
			}
		}
		game.InternalVersion = INTERNAL_VERSION.ToString();
		return game;
	}

	public LevelReference GetLevelReferenceById(SerializableGuid levelId)
	{
		return levels.First((LevelReference reference) => (SerializableGuid)reference.AssetID == levelId);
	}

	public List<AssetReference> GatherDependentAssetReferences()
	{
		return GameLibrary.PropReferences.AsEnumerable().Concat(GetActiveTerrainReferences()).ToList();
	}

	public List<AssetReference> GetActiveTerrainReferences()
	{
		return (from entry in GameLibrary.TerrainEntries
			where entry.IsActive
			select entry.TerrainAssetReference).ToList();
	}

	public bool ReorderLevels(List<LevelReference> newOrder)
	{
		if (levels.Count != newOrder.Count)
		{
			return false;
		}
		levels = newOrder;
		return true;
	}

	public void AddScreenshots(List<ScreenshotFileInstances> collection)
	{
		Screenshots.AddRange(collection);
	}

	public void RemoveScreenshotAt(int index)
	{
		Screenshots.RemoveAt(index);
	}

	public void ReorderScreenshot(List<ScreenshotFileInstances> newOrder)
	{
		Screenshots = newOrder;
	}
}
