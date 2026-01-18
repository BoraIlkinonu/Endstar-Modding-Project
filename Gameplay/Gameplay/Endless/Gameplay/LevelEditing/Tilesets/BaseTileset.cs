using System;
using System.Diagnostics;
using System.Linq;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x0200050D RID: 1293
	public class BaseTileset : Tileset
	{
		// Token: 0x06001F63 RID: 8035 RVA: 0x0008B0DC File Offset: 0x000892DC
		public BaseTileset(TilesetCosmeticProfile cosmeticProfile, Asset asset, Sprite displayIcon, int index)
			: base(cosmeticProfile, asset, displayIcon, index)
		{
		}

		// Token: 0x06001F64 RID: 8036 RVA: 0x0008B0EC File Offset: 0x000892EC
		public override TileSpawnContext GetValidVisualForCellPosition(Stage stage, Vector3Int coordinates)
		{
			SerializableGuid[] consideredTilesetIds = base.CosmeticProfile.ConsideredTilesetIds;
			Stopwatch.StartNew();
			Cell cellFromCoordinate = stage.GetCellFromCoordinate(coordinates + Vector3Int.up);
			Cell cellFromCoordinate2 = stage.GetCellFromCoordinate(coordinates + Vector3Int.down);
			Cell cellFromCoordinate3 = stage.GetCellFromCoordinate(coordinates + Vector3Int.left);
			Cell cellFromCoordinate4 = stage.GetCellFromCoordinate(coordinates + Vector3Int.right);
			Cell cellFromCoordinate5 = stage.GetCellFromCoordinate(coordinates + Vector3Int.forward);
			Cell cellFromCoordinate6 = stage.GetCellFromCoordinate(coordinates + Vector3Int.back);
			Cell cellFromCoordinate7 = stage.GetCellFromCoordinate(coordinates);
			TerrainCell terrainCell = cellFromCoordinate7 as TerrainCell;
			TerrainCell terrainCell2 = cellFromCoordinate as TerrainCell;
			bool flag = terrainCell2 != null && ((stage.GetTilesetFromCell(cellFromCoordinate) != null && stage.GetTilesetFromCell(cellFromCoordinate).TilesetType == TilesetType.Slope) || (stage.GetTilesetFromCell(cellFromCoordinate) != null && consideredTilesetIds.Contains(stage.GetTilesetIdFromCell(cellFromCoordinate))) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell2.TilesetIndex));
			TerrainCell terrainCell3 = cellFromCoordinate2 as TerrainCell;
			bool flag2 = terrainCell3 != null && (consideredTilesetIds.Contains(stage.GetTilesetIdFromCell(cellFromCoordinate2)) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell3.TilesetIndex));
			TerrainCell terrainCell4 = cellFromCoordinate3 as TerrainCell;
			bool flag3 = terrainCell4 != null && (consideredTilesetIds.Contains(stage.GetTilesetIdFromCell(cellFromCoordinate3)) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell4.TilesetIndex));
			TerrainCell terrainCell5 = cellFromCoordinate4 as TerrainCell;
			bool flag4 = terrainCell5 != null && (consideredTilesetIds.Contains(stage.GetTilesetIdFromCell(cellFromCoordinate4)) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell5.TilesetIndex));
			TerrainCell terrainCell6 = cellFromCoordinate5 as TerrainCell;
			bool flag5 = terrainCell6 != null && (consideredTilesetIds.Contains(stage.GetTilesetIdFromCell(cellFromCoordinate5)) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell6.TilesetIndex));
			TerrainCell terrainCell7 = cellFromCoordinate6 as TerrainCell;
			bool flag6 = terrainCell7 != null && (consideredTilesetIds.Contains(stage.GetTilesetIdFromCell(cellFromCoordinate6)) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell7.TilesetIndex));
			int num = Convert.ToInt32(string.Format("{0}{1}{2}{3}{4}{5}", new object[]
			{
				flag ? 1 : 0,
				flag2 ? 1 : 0,
				flag3 ? 1 : 0,
				flag4 ? 1 : 0,
				flag6 ? 1 : 0,
				flag5 ? 1 : 0
			}), 2);
			if (num < 0 || num > 64)
			{
				return null;
			}
			Tile tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(num));
			return new TileSpawnContext
			{
				Tileset = this,
				Tile = tile,
				AllowTopDecoration = (!(cellFromCoordinate is TerrainCell) && !flag),
				TopFilled = flag,
				BottomFilled = flag2,
				LeftFilled = flag3,
				RightFilled = flag4,
				FrontFilled = flag5,
				BackFilled = flag6
			};
		}
	}
}
