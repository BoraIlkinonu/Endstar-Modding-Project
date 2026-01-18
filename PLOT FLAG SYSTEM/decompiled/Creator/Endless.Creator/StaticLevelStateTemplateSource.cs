using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Gameplay.LevelEditing;
using UnityEngine;

namespace Endless.Creator;

[CreateAssetMenu(menuName = "ScriptableObject/Level States/Level State Template", fileName = "Level State Template")]
public class StaticLevelStateTemplateSource : LevelStateTemplateSourceBase
{
	private StaticLevelTemplateAsset staticLevelTemplateAsset;

	private LevelState levelState;

	private GameLibrary gameLibrary;

	[field: SerializeField]
	private string DisplayName { get; set; }

	[field: SerializeField]
	private Sprite DisplaySprite { get; set; }

	[field: SerializeField]
	private TextAsset LevelFile { get; set; }

	public override async Task<string> GetDisplayName()
	{
		return DisplayName;
	}

	public override async Task<string> GetDescription()
	{
		throw new NotImplementedException();
	}

	public override async Task<Sprite> GetDisplaySprite()
	{
		return DisplaySprite;
	}

	public override async Task<LevelState> GetLevelState(Game game)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		List<TerrainUsage> list = game.GameLibrary.TerrainEntries.Where((TerrainUsage terrain) => terrain.IsActive).ToList();
		foreach (TerrainEntry terrainEntry in levelState.TerrainEntries)
		{
			if (!dictionary.ContainsKey(terrainEntry.TilesetId))
			{
				TerrainUsage originalUsage = gameLibrary.GetActiveTerrainUsageFromIndex(terrainEntry.TilesetId);
				TerrainUsage terrainUsage = list.FirstOrDefault((TerrainUsage gameTerrain) => (SerializableGuid)gameTerrain.TerrainAssetReference.AssetID == originalUsage.TilesetId);
				if (terrainUsage == null)
				{
					dictionary.Add(terrainEntry.TilesetId, 0);
				}
				else
				{
					dictionary.Add(terrainEntry.TilesetId, list.IndexOf(terrainUsage));
				}
			}
			terrainEntry.TilesetId = dictionary[terrainEntry.TilesetId];
		}
		return levelState;
	}

	public override async Task<List<SerializableGuid>> GetRequiredTerrainAssets(Game game)
	{
		return (from usage in gameLibrary.GetTerrainUsagesInLevel(levelState.GetUsedTileSetIds(gameLibrary))
			select usage.TilesetId).Except(game.GameLibrary.TerrainEntries.Select((TerrainUsage entry) => entry.TilesetId)).ToList();
	}

	public override async Task<List<SerializableGuid>> GetRequiredPropAssets(Game game)
	{
		IEnumerable<SerializableGuid> second = game.GameLibrary.PropReferences.Select((AssetReference reference) => reference.AssetID).Select((Func<string, SerializableGuid>)((string id) => id));
		return levelState.GetUsedPropIds().Except(second).ToList();
	}

	public override async Task Prepare()
	{
		staticLevelTemplateAsset = JsonConvert.DeserializeObject<StaticLevelTemplateAsset>(LevelFile.text);
		levelState = staticLevelTemplateAsset.LevelState;
		gameLibrary = staticLevelTemplateAsset.GameLibrary;
	}
}
