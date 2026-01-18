using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Creator;

public abstract class LevelStateTemplateSourceBase : ScriptableObject
{
	public abstract Task<string> GetDisplayName();

	public abstract Task<string> GetDescription();

	public abstract Task<Sprite> GetDisplaySprite();

	public abstract Task<LevelState> GetLevelState(Game game);

	public abstract Task<List<SerializableGuid>> GetRequiredTerrainAssets(Game game);

	public abstract Task<List<SerializableGuid>> GetRequiredPropAssets(Game game);

	public abstract Task Prepare();

	public async Task<LevelState> CreateLevelState(Game targetGame, string levelName, string levelDescription, bool useGameEditor)
	{
		await Prepare();
		List<SerializableGuid> neededProps = await GetRequiredPropAssets(targetGame);
		List<SerializableGuid> neededTerrain = await GetRequiredTerrainAssets(targetGame);
		if (neededTerrain.Count > 0 || neededProps.Count > 0)
		{
			bool continueAdd = await MonoBehaviourSingleton<UIModalManager>.Instance.Confirm($"You have to add {neededProps.Count + neededTerrain.Count} props/terrain to your game in order to use this template. Do you wish to continue?");
			await Task.Yield();
			if (!continueAdd)
			{
				throw new NotImplementedException("User chose not to add. Can we abort cleaner?");
			}
		}
		if (neededProps.Count > 0)
		{
			Debug.LogWarning($"The game is missing {neededProps.Count} props that are found within the template");
			BulkAssetCacheResult<Prop> bulkAssetCacheResult = await EndlessAssetCache.GetLatestBulkAssetsAsync<Prop>(neededProps.Select((SerializableGuid id) => (id: id, Empty: string.Empty)).ToArray());
			if (bulkAssetCacheResult.HasErrors)
			{
				throw new Exception("Failed to retrieve required props");
			}
			if (useGameEditor)
			{
				await MonoBehaviourSingleton<GameEditor>.Instance.AddPropsToGameLibrary(bulkAssetCacheResult.Assets);
			}
			else
			{
				targetGame.GameLibrary.AddProps(bulkAssetCacheResult.Assets.Select((Prop prop) => prop.ToAssetReference()));
			}
			neededProps = await GetRequiredPropAssets(targetGame);
			if (neededProps.Count > 0)
			{
				throw new Exception($"Partial add success (prop) is not enough to move forward (missing {neededProps.Count})");
			}
		}
		if (neededTerrain.Count > 0)
		{
			Debug.LogWarning($"The game is missing {neededTerrain.Count} terrain that are found within the template");
			BulkAssetCacheResult<TerrainTilesetCosmeticAsset> bulkAssetCacheResult2 = await EndlessAssetCache.GetLatestBulkAssetsAsync<TerrainTilesetCosmeticAsset>(neededTerrain.Select((SerializableGuid id) => (id: id, Empty: string.Empty)).ToArray());
			if (bulkAssetCacheResult2.HasErrors)
			{
				throw new Exception("Failed to retrieve required props");
			}
			Debug.LogWarning($"I found {bulkAssetCacheResult2.Assets.Count} terrains to add");
			if (useGameEditor)
			{
				await MonoBehaviourSingleton<GameEditor>.Instance.AddTerrainUsagesToGameLibrary(bulkAssetCacheResult2.Assets);
			}
			else
			{
				targetGame.GameLibrary.AddTerrainUsages(bulkAssetCacheResult2.Assets.Select((TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset) => terrainTilesetCosmeticAsset.ToAssetReference()));
			}
			neededTerrain = await GetRequiredTerrainAssets(targetGame);
			if (neededTerrain.Count > 0)
			{
				throw new Exception($"Partial add success (terrain) is not enough to move forward (missing {neededTerrain.Count})");
			}
		}
		LevelState obj = await GetLevelState(targetGame);
		obj.Name = levelName;
		obj.Description = levelDescription;
		obj.Archived = false;
		obj.AssetType = "level";
		obj.AssetID = null;
		obj.AssetVersion = null;
		return obj;
	}
}
