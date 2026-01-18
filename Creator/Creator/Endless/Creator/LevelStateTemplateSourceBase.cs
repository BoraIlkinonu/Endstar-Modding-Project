using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Creator
{
	// Token: 0x0200005D RID: 93
	public abstract class LevelStateTemplateSourceBase : ScriptableObject
	{
		// Token: 0x06000145 RID: 325
		public abstract Task<string> GetDisplayName();

		// Token: 0x06000146 RID: 326
		public abstract Task<string> GetDescription();

		// Token: 0x06000147 RID: 327
		public abstract Task<Sprite> GetDisplaySprite();

		// Token: 0x06000148 RID: 328
		public abstract Task<LevelState> GetLevelState(Game game);

		// Token: 0x06000149 RID: 329
		public abstract Task<List<SerializableGuid>> GetRequiredTerrainAssets(Game game);

		// Token: 0x0600014A RID: 330
		public abstract Task<List<SerializableGuid>> GetRequiredPropAssets(Game game);

		// Token: 0x0600014B RID: 331
		public abstract Task Prepare();

		// Token: 0x0600014C RID: 332 RVA: 0x0000AEA8 File Offset: 0x000090A8
		public async Task<LevelState> CreateLevelState(Game targetGame, string levelName, string levelDescription, bool useGameEditor)
		{
			await this.Prepare();
			List<SerializableGuid> neededProps = await this.GetRequiredPropAssets(targetGame);
			List<SerializableGuid> neededTerrain = await this.GetRequiredTerrainAssets(targetGame);
			if (neededTerrain.Count > 0 || neededProps.Count > 0)
			{
				bool continueAdd = await MonoBehaviourSingleton<UIModalManager>.Instance.Confirm(string.Format("You have to add {0} props/terrain to your game in order to use this template. Do you wish to continue?", neededProps.Count + neededTerrain.Count), UIModalManagerStackActions.MaintainStack);
				await Task.Yield();
				if (!continueAdd)
				{
					throw new NotImplementedException("User chose not to add. Can we abort cleaner?");
				}
			}
			if (neededProps.Count > 0)
			{
				Debug.LogWarning(string.Format("The game is missing {0} props that are found within the template", neededProps.Count));
				BulkAssetCacheResult<Prop> bulkAssetCacheResult = await EndlessAssetCache.GetLatestBulkAssetsAsync<Prop>(neededProps.Select((SerializableGuid id) => new ValueTuple<SerializableGuid, string>(id, string.Empty)).ToArray<ValueTuple<SerializableGuid, string>>());
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
				neededProps = await this.GetRequiredPropAssets(targetGame);
				if (neededProps.Count > 0)
				{
					throw new Exception(string.Format("Partial add success (prop) is not enough to move forward (missing {0})", neededProps.Count));
				}
			}
			if (neededTerrain.Count > 0)
			{
				Debug.LogWarning(string.Format("The game is missing {0} terrain that are found within the template", neededTerrain.Count));
				BulkAssetCacheResult<TerrainTilesetCosmeticAsset> bulkAssetCacheResult2 = await EndlessAssetCache.GetLatestBulkAssetsAsync<TerrainTilesetCosmeticAsset>(neededTerrain.Select((SerializableGuid id) => new ValueTuple<SerializableGuid, string>(id, string.Empty)).ToArray<ValueTuple<SerializableGuid, string>>());
				if (bulkAssetCacheResult2.HasErrors)
				{
					throw new Exception("Failed to retrieve required props");
				}
				Debug.LogWarning(string.Format("I found {0} terrains to add", bulkAssetCacheResult2.Assets.Count));
				if (useGameEditor)
				{
					await MonoBehaviourSingleton<GameEditor>.Instance.AddTerrainUsagesToGameLibrary(bulkAssetCacheResult2.Assets);
				}
				else
				{
					targetGame.GameLibrary.AddTerrainUsages(bulkAssetCacheResult2.Assets.Select((TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset) => terrainTilesetCosmeticAsset.ToAssetReference()));
				}
				neededTerrain = await this.GetRequiredTerrainAssets(targetGame);
				if (neededTerrain.Count > 0)
				{
					throw new Exception(string.Format("Partial add success (terrain) is not enough to move forward (missing {0})", neededTerrain.Count));
				}
			}
			LevelState levelState = await this.GetLevelState(targetGame);
			levelState.Name = levelName;
			levelState.Description = levelDescription;
			levelState.Archived = false;
			levelState.AssetType = "level";
			levelState.AssetID = null;
			levelState.AssetVersion = null;
			return levelState;
		}
	}
}
