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

namespace Endless.Gameplay.LevelEditing
{
	// Token: 0x020004D9 RID: 1241
	public class DefaultContentManager : MonoBehaviourSingleton<DefaultContentManager>
	{
		// Token: 0x170005F1 RID: 1521
		// (get) Token: 0x06001E9B RID: 7835 RVA: 0x00085AF6 File Offset: 0x00083CF6
		// (set) Token: 0x06001E9C RID: 7836 RVA: 0x00085AFE File Offset: 0x00083CFE
		public IconList DefaultIconList { get; private set; }

		// Token: 0x170005F2 RID: 1522
		// (get) Token: 0x06001E9D RID: 7837 RVA: 0x00085B07 File Offset: 0x00083D07
		public CharacterCosmeticsList DefaultCharacterCosmetics
		{
			get
			{
				return this.defaultCharacterCosmetics;
			}
		}

		// Token: 0x170005F3 RID: 1523
		// (get) Token: 0x06001E9E RID: 7838 RVA: 0x00085B0F File Offset: 0x00083D0F
		public Sprite MissingPropDisplayIcon
		{
			get
			{
				return this.missingPropDisplayIcon;
			}
		}

		// Token: 0x170005F4 RID: 1524
		// (get) Token: 0x06001E9F RID: 7839 RVA: 0x00085B17 File Offset: 0x00083D17
		public DefaultPalette DefaultTerrain
		{
			get
			{
				return this.defaultTerrain;
			}
		}

		// Token: 0x170005F5 RID: 1525
		// (get) Token: 0x06001EA0 RID: 7840 RVA: 0x00085B1F File Offset: 0x00083D1F
		public DefaultPalette DefaultProps
		{
			get
			{
				return this.defaultProps;
			}
		}

		// Token: 0x06001EA1 RID: 7841 RVA: 0x00085B28 File Offset: 0x00083D28
		public async Task AddAllDefaults(Game game)
		{
			await this.AddDefaultTerrain(game);
			await this.AddDefaultProps(game);
		}

		// Token: 0x06001EA2 RID: 7842 RVA: 0x00085B74 File Offset: 0x00083D74
		public async Task AddDefaultProps(Game game)
		{
			DefaultContentManager.<>c__DisplayClass18_0 CS$<>8__locals1 = new DefaultContentManager.<>c__DisplayClass18_0();
			BulkAssetCacheResult<Prop> bulkAssetCacheResult = await EndlessAssetCache.GetBulkAssetsAsync<Prop>(this.defaultProps.DefaultEntries.Select((DefaultPaletteEntry entry) => new ValueTuple<SerializableGuid, string>(entry.Id, "")).ToArray<ValueTuple<SerializableGuid, string>>());
			CS$<>8__locals1.bulkResult = bulkAssetCacheResult;
			foreach (Prop prop in CS$<>8__locals1.bulkResult.Assets)
			{
				AssetReference assetReference = new AssetReference();
				assetReference.AssetID = prop.AssetID;
				assetReference.AssetType = prop.AssetType;
				assetReference.AssetVersion = prop.AssetVersion;
				game.GameLibrary.AddProp(assetReference);
			}
			if (CS$<>8__locals1.bulkResult.HasErrors)
			{
				Debug.LogException(new Exception(string.Format("{0} default props fails to load", this.defaultProps.DefaultEntries.Count - CS$<>8__locals1.bulkResult.Assets.Count)));
			}
			IEnumerable<DefaultPaletteEntry> defaultEntries = this.defaultProps.DefaultEntries;
			Func<DefaultPaletteEntry, bool> func;
			if ((func = CS$<>8__locals1.<>9__1) == null)
			{
				DefaultContentManager.<>c__DisplayClass18_0 CS$<>8__locals2 = CS$<>8__locals1;
				Func<DefaultPaletteEntry, bool> func2 = (DefaultPaletteEntry reference) => !CS$<>8__locals1.bulkResult.Assets.Any((Prop asset) => asset.AssetID == reference.Id);
				CS$<>8__locals2.<>9__1 = func2;
				func = func2;
			}
			foreach (DefaultPaletteEntry defaultPaletteEntry in defaultEntries.Where(func))
			{
				game.GameLibrary.AddProp(new AssetReference
				{
					AssetID = defaultPaletteEntry.Id,
					AssetVersion = "0.0.0",
					AssetType = "prop"
				});
			}
		}

		// Token: 0x06001EA3 RID: 7843 RVA: 0x00085BC0 File Offset: 0x00083DC0
		public async Task AddDefaultTerrain(Game game)
		{
			DefaultContentManager.<>c__DisplayClass19_0 CS$<>8__locals1 = new DefaultContentManager.<>c__DisplayClass19_0();
			BulkAssetCacheResult<TerrainTilesetCosmeticAsset> bulkAssetCacheResult = await EndlessAssetCache.GetBulkAssetsAsync<TerrainTilesetCosmeticAsset>(this.defaultTerrain.DefaultEntries.Select((DefaultPaletteEntry entry) => new ValueTuple<SerializableGuid, string>(entry.Id, "")).ToArray<ValueTuple<SerializableGuid, string>>());
			CS$<>8__locals1.bulkResult = bulkAssetCacheResult;
			List<TerrainUsage> list = new List<TerrainUsage>();
			for (int i = 0; i < this.defaultTerrain.DefaultEntries.Count; i++)
			{
				list.Add(null);
			}
			game.GameLibrary.TerrainEntries.AddRange(list);
			foreach (TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset in CS$<>8__locals1.bulkResult.Assets)
			{
				int indexFromAssetList = this.GetIndexFromAssetList(this.defaultTerrain.DefaultEntries, terrainTilesetCosmeticAsset.AssetID);
				game.GameLibrary.InsertTerrainUsage(terrainTilesetCosmeticAsset.AssetID, terrainTilesetCosmeticAsset.AssetVersion, indexFromAssetList);
			}
			if (CS$<>8__locals1.bulkResult.HasErrors)
			{
				Debug.LogException(new Exception(string.Format("{0} default terrain fails to load", this.defaultTerrain.DefaultEntries.Count - CS$<>8__locals1.bulkResult.Assets.Count)));
			}
			IEnumerable<DefaultPaletteEntry> defaultEntries = this.defaultTerrain.DefaultEntries;
			Func<DefaultPaletteEntry, bool> func;
			if ((func = CS$<>8__locals1.<>9__1) == null)
			{
				DefaultContentManager.<>c__DisplayClass19_0 CS$<>8__locals2 = CS$<>8__locals1;
				Func<DefaultPaletteEntry, bool> func2 = (DefaultPaletteEntry reference) => !CS$<>8__locals1.bulkResult.Assets.Any((TerrainTilesetCosmeticAsset asset) => asset.AssetID == reference.Id);
				CS$<>8__locals2.<>9__1 = func2;
				func = func2;
			}
			foreach (DefaultPaletteEntry defaultPaletteEntry in defaultEntries.Where(func))
			{
				game.GameLibrary.AddTerrainUsage(defaultPaletteEntry.Id, "0.0.0");
			}
		}

		// Token: 0x06001EA4 RID: 7844 RVA: 0x00085C0C File Offset: 0x00083E0C
		private int GetIndexFromAssetList(IReadOnlyList<DefaultPaletteEntry> list, SerializableGuid id)
		{
			DefaultPaletteEntry defaultPaletteEntry = list.FirstOrDefault((DefaultPaletteEntry entry) => entry.Id == id);
			if (defaultPaletteEntry == null)
			{
				return -1;
			}
			return list.IndexOf(defaultPaletteEntry);
		}

		// Token: 0x04001796 RID: 6038
		[SerializeField]
		private DefaultPalette defaultTerrain;

		// Token: 0x04001797 RID: 6039
		[SerializeField]
		private DefaultPalette defaultProps;

		// Token: 0x04001798 RID: 6040
		[SerializeField]
		private Sprite missingPropDisplayIcon;

		// Token: 0x04001799 RID: 6041
		[SerializeField]
		private CharacterCosmeticsList defaultCharacterCosmetics;

		// Token: 0x0400179B RID: 6043
		public GameObject DefaultCharacterCosmeticsGameObject;
	}
}
