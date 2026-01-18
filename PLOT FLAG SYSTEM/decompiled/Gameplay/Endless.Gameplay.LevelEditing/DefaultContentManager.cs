using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using Runtime.Gameplay.LevelEditing;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing;

public class DefaultContentManager : MonoBehaviourSingleton<DefaultContentManager>
{
	[SerializeField]
	private DefaultPalette defaultTerrain;

	[SerializeField]
	private DefaultPalette defaultProps;

	[SerializeField]
	private Sprite missingPropDisplayIcon;

	[SerializeField]
	private CharacterCosmeticsList defaultCharacterCosmetics;

	public GameObject DefaultCharacterCosmeticsGameObject;

	[field: SerializeField]
	public IconList DefaultIconList { get; private set; }

	public CharacterCosmeticsList DefaultCharacterCosmetics => defaultCharacterCosmetics;

	public Sprite MissingPropDisplayIcon => missingPropDisplayIcon;

	public DefaultPalette DefaultTerrain => defaultTerrain;

	public DefaultPalette DefaultProps => defaultProps;

	public async Task AddAllDefaults(Game game)
	{
		await AddDefaultTerrain(game);
		await AddDefaultProps(game);
	}

	public async Task AddDefaultProps(Game game)
	{
		BulkAssetCacheResult<Prop> bulkResult = await EndlessAssetCache.GetBulkAssetsAsync<Prop>(defaultProps.DefaultEntries.Select((DefaultPaletteEntry entry) => ((SerializableGuid, string))(entry.Id, "")).ToArray());
		foreach (Prop asset in bulkResult.Assets)
		{
			AssetReference propReference = new AssetReference
			{
				AssetID = asset.AssetID,
				AssetType = asset.AssetType,
				AssetVersion = asset.AssetVersion
			};
			game.GameLibrary.AddProp(propReference);
		}
		if (bulkResult.HasErrors)
		{
			int num = defaultProps.DefaultEntries.Count - bulkResult.Assets.Count;
			Debug.LogException(new Exception($"{num} default props fails to load"));
		}
		foreach (DefaultPaletteEntry item in defaultProps.DefaultEntries.Where((DefaultPaletteEntry reference) => !bulkResult.Assets.Any((Prop asset) => asset.AssetID == reference.Id)))
		{
			game.GameLibrary.AddProp(new AssetReference
			{
				AssetID = item.Id,
				AssetVersion = "0.0.0",
				AssetType = "prop"
			});
		}
	}

	public async Task AddDefaultTerrain(Game game)
	{
		BulkAssetCacheResult<TerrainTilesetCosmeticAsset> bulkResult = await EndlessAssetCache.GetBulkAssetsAsync<TerrainTilesetCosmeticAsset>(defaultTerrain.DefaultEntries.Select((DefaultPaletteEntry entry) => ((SerializableGuid, string))(entry.Id, "")).ToArray());
		List<TerrainUsage> list = new List<TerrainUsage>();
		for (int num = 0; num < defaultTerrain.DefaultEntries.Count; num++)
		{
			list.Add(null);
		}
		game.GameLibrary.TerrainEntries.AddRange(list);
		foreach (TerrainTilesetCosmeticAsset asset in bulkResult.Assets)
		{
			int indexFromAssetList = GetIndexFromAssetList(defaultTerrain.DefaultEntries, asset.AssetID);
			game.GameLibrary.InsertTerrainUsage(asset.AssetID, asset.AssetVersion, indexFromAssetList);
		}
		if (bulkResult.HasErrors)
		{
			int num2 = defaultTerrain.DefaultEntries.Count - bulkResult.Assets.Count;
			Debug.LogException(new Exception($"{num2} default terrain fails to load"));
		}
		foreach (DefaultPaletteEntry item in defaultTerrain.DefaultEntries.Where((DefaultPaletteEntry reference) => !bulkResult.Assets.Any((TerrainTilesetCosmeticAsset asset) => asset.AssetID == reference.Id)))
		{
			game.GameLibrary.AddTerrainUsage(item.Id, "0.0.0");
		}
	}

	private int GetIndexFromAssetList(IReadOnlyList<DefaultPaletteEntry> list, SerializableGuid id)
	{
		DefaultPaletteEntry defaultPaletteEntry = list.FirstOrDefault((DefaultPaletteEntry entry) => (SerializableGuid)entry.Id == id);
		if (defaultPaletteEntry == null)
		{
			return -1;
		}
		return list.IndexOf(defaultPaletteEntry);
	}
}
