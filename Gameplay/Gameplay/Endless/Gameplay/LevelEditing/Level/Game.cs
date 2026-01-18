using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level.UpgradeVersions;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Gameplay.LevelEditing;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000541 RID: 1345
	[Serializable]
	public class Game : Asset
	{
		// Token: 0x0600206F RID: 8303 RVA: 0x0009222C File Offset: 0x0009042C
		public Game()
		{
			this.AssetType = "game";
			this.Name = "Game";
			this.InternalVersion = Game.INTERNAL_VERSION.ToString();
		}

		// Token: 0x06002070 RID: 8304 RVA: 0x00092298 File Offset: 0x00090498
		public object GetAnonymousObjectForUpload(LevelState newLevelState)
		{
			object[] array = ((newLevelState == null) ? new object[this.levels.Count] : new object[this.levels.Count + 1]);
			for (int i = 0; i < this.levels.Count; i++)
			{
				array[i] = this.levels[i].GetAnonymousObject();
			}
			if (newLevelState != null)
			{
				array[this.levels.Count] = newLevelState.GetAnonymousObjectForUpload();
			}
			return new
			{
				asset_id = this.AssetID,
				internal_version = this.InternalVersion,
				asset_version = string.Empty,
				asset_type = "game",
				Name = this.Name,
				Description = this.Description,
				GameLibrary = this.GameLibrary,
				levels = array,
				MininumNumberOfPlayers = this.MininumNumberOfPlayers,
				MaximumNumberOfPlayers = this.MaximumNumberOfPlayers,
				revision_meta_data = this.RevisionMetaData,
				screenshots = this.Screenshots
			};
		}

		// Token: 0x06002071 RID: 8305 RVA: 0x00092353 File Offset: 0x00090553
		public Game Clone()
		{
			return JsonConvert.DeserializeObject<Game>(JsonConvert.SerializeObject(this));
		}

		// Token: 0x06002072 RID: 8306 RVA: 0x00092360 File Offset: 0x00090560
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
			foreach (TerrainUsage terrainUsage in game.GameLibrary.TerrainEntries)
			{
				if (terrainUsage.IsActive)
				{
					terrainUsage.TerrainAssetReference.AssetType = "terrain-tileset-cosmetic";
				}
			}
			game.InternalVersion = Game.INTERNAL_VERSION.ToString();
			return game;
		}

		// Token: 0x06002073 RID: 8307 RVA: 0x00092464 File Offset: 0x00090664
		public LevelReference GetLevelReferenceById(SerializableGuid levelId)
		{
			return this.levels.First((LevelReference reference) => reference.AssetID == levelId);
		}

		// Token: 0x06002074 RID: 8308 RVA: 0x00092495 File Offset: 0x00090695
		public List<AssetReference> GatherDependentAssetReferences()
		{
			return this.GameLibrary.PropReferences.AsEnumerable<AssetReference>().Concat(this.GetActiveTerrainReferences()).ToList<AssetReference>();
		}

		// Token: 0x06002075 RID: 8309 RVA: 0x000924B8 File Offset: 0x000906B8
		public List<AssetReference> GetActiveTerrainReferences()
		{
			return (from entry in this.GameLibrary.TerrainEntries
				where entry.IsActive
				select entry.TerrainAssetReference).ToList<AssetReference>();
		}

		// Token: 0x06002076 RID: 8310 RVA: 0x0009251D File Offset: 0x0009071D
		public bool ReorderLevels(List<LevelReference> newOrder)
		{
			if (this.levels.Count != newOrder.Count)
			{
				return false;
			}
			this.levels = newOrder;
			return true;
		}

		// Token: 0x06002077 RID: 8311 RVA: 0x0009253C File Offset: 0x0009073C
		public void AddScreenshots(List<ScreenshotFileInstances> collection)
		{
			this.Screenshots.AddRange(collection);
		}

		// Token: 0x06002078 RID: 8312 RVA: 0x0009254A File Offset: 0x0009074A
		public void RemoveScreenshotAt(int index)
		{
			this.Screenshots.RemoveAt(index);
		}

		// Token: 0x06002079 RID: 8313 RVA: 0x00092558 File Offset: 0x00090758
		public void ReorderScreenshot(List<ScreenshotFileInstances> newOrder)
		{
			this.Screenshots = newOrder;
		}

		// Token: 0x040019EF RID: 6639
		public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(1, 1, 0);

		// Token: 0x040019F0 RID: 6640
		public List<LevelReference> levels = new List<LevelReference>();

		// Token: 0x040019F1 RID: 6641
		[JsonProperty]
		public GameLibrary GameLibrary = new GameLibrary();

		// Token: 0x040019F2 RID: 6642
		[JsonProperty("screenshots")]
		public List<ScreenshotFileInstances> Screenshots = new List<ScreenshotFileInstances>();

		// Token: 0x040019F3 RID: 6643
		public int MininumNumberOfPlayers = 1;

		// Token: 0x040019F4 RID: 6644
		public int MaximumNumberOfPlayers = 10;
	}
}
