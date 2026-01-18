using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Gameplay.LevelEditing;
using UnityEngine;

namespace Endless.Creator
{
	// Token: 0x02000070 RID: 112
	[CreateAssetMenu(menuName = "ScriptableObject/Level States/Level State Template", fileName = "Level State Template")]
	public class StaticLevelStateTemplateSource : LevelStateTemplateSourceBase
	{
		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000182 RID: 386 RVA: 0x0000C5E2 File Offset: 0x0000A7E2
		// (set) Token: 0x06000183 RID: 387 RVA: 0x0000C5EA File Offset: 0x0000A7EA
		private string DisplayName { get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000184 RID: 388 RVA: 0x0000C5F3 File Offset: 0x0000A7F3
		// (set) Token: 0x06000185 RID: 389 RVA: 0x0000C5FB File Offset: 0x0000A7FB
		private Sprite DisplaySprite { get; set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000186 RID: 390 RVA: 0x0000C604 File Offset: 0x0000A804
		// (set) Token: 0x06000187 RID: 391 RVA: 0x0000C60C File Offset: 0x0000A80C
		private TextAsset LevelFile { get; set; }

		// Token: 0x06000188 RID: 392 RVA: 0x0000C618 File Offset: 0x0000A818
		public override async Task<string> GetDisplayName()
		{
			return this.DisplayName;
		}

		// Token: 0x06000189 RID: 393 RVA: 0x0000C65C File Offset: 0x0000A85C
		public override Task<string> GetDescription()
		{
			StaticLevelStateTemplateSource.<GetDescription>d__16 <GetDescription>d__;
			<GetDescription>d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
			<GetDescription>d__.<>1__state = -1;
			<GetDescription>d__.<>t__builder.Start<StaticLevelStateTemplateSource.<GetDescription>d__16>(ref <GetDescription>d__);
			return <GetDescription>d__.<>t__builder.Task;
		}

		// Token: 0x0600018A RID: 394 RVA: 0x0000C698 File Offset: 0x0000A898
		public override async Task<Sprite> GetDisplaySprite()
		{
			return this.DisplaySprite;
		}

		// Token: 0x0600018B RID: 395 RVA: 0x0000C6DC File Offset: 0x0000A8DC
		public override async Task<LevelState> GetLevelState(Game game)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			List<TerrainUsage> list = game.GameLibrary.TerrainEntries.Where((TerrainUsage terrain) => terrain.IsActive).ToList<TerrainUsage>();
			foreach (TerrainEntry terrainEntry in this.levelState.TerrainEntries)
			{
				if (!dictionary.ContainsKey(terrainEntry.TilesetId))
				{
					TerrainUsage originalUsage = this.gameLibrary.GetActiveTerrainUsageFromIndex(terrainEntry.TilesetId);
					TerrainUsage terrainUsage = list.FirstOrDefault((TerrainUsage gameTerrain) => gameTerrain.TerrainAssetReference.AssetID == originalUsage.TilesetId);
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
			return this.levelState;
		}

		// Token: 0x0600018C RID: 396 RVA: 0x0000C728 File Offset: 0x0000A928
		public override async Task<List<SerializableGuid>> GetRequiredTerrainAssets(Game game)
		{
			return (from usage in this.gameLibrary.GetTerrainUsagesInLevel(this.levelState.GetUsedTileSetIds(this.gameLibrary))
				select usage.TilesetId).Except(game.GameLibrary.TerrainEntries.Select((TerrainUsage entry) => entry.TilesetId)).ToList<SerializableGuid>();
		}

		// Token: 0x0600018D RID: 397 RVA: 0x0000C774 File Offset: 0x0000A974
		public override async Task<List<SerializableGuid>> GetRequiredPropAssets(Game game)
		{
			IEnumerable<SerializableGuid> enumerable = from reference in game.GameLibrary.PropReferences
				select reference.AssetID into id
				select (id);
			return this.levelState.GetUsedPropIds().Except(enumerable).ToList<SerializableGuid>();
		}

		// Token: 0x0600018E RID: 398 RVA: 0x0000C7C0 File Offset: 0x0000A9C0
		public override async Task Prepare()
		{
			this.staticLevelTemplateAsset = JsonConvert.DeserializeObject<StaticLevelTemplateAsset>(this.LevelFile.text);
			this.levelState = this.staticLevelTemplateAsset.LevelState;
			this.gameLibrary = this.staticLevelTemplateAsset.GameLibrary;
		}

		// Token: 0x040001FD RID: 509
		private StaticLevelTemplateAsset staticLevelTemplateAsset;

		// Token: 0x040001FE RID: 510
		private LevelState levelState;

		// Token: 0x040001FF RID: 511
		private GameLibrary gameLibrary;
	}
}
