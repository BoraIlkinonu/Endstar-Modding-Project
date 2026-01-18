using System;
using System.Linq;
using System.Text;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x02000515 RID: 1301
	public class HorizontalTileset : Tileset
	{
		// Token: 0x06001F79 RID: 8057 RVA: 0x0008B0DC File Offset: 0x000892DC
		public HorizontalTileset(TilesetCosmeticProfile cosmeticProfile, Asset asset, Sprite displayIcon, int index)
			: base(cosmeticProfile, asset, displayIcon, index)
		{
		}

		// Token: 0x06001F7A RID: 8058 RVA: 0x0008B6D8 File Offset: 0x000898D8
		public override TileSpawnContext GetValidVisualForCellPosition(Stage stage, Vector3Int coordinates)
		{
			SerializableGuid[] consideredTilesetIds = base.CosmeticProfile.ConsideredTilesetIds;
			Cell cellFromCoordinate = stage.GetCellFromCoordinate(coordinates + Vector3Int.up);
			Cell cellFromCoordinate2 = stage.GetCellFromCoordinate(coordinates + Vector3Int.left);
			Cell cellFromCoordinate3 = stage.GetCellFromCoordinate(coordinates + Vector3Int.right);
			Cell cellFromCoordinate4 = stage.GetCellFromCoordinate(coordinates + Vector3Int.forward);
			Cell cellFromCoordinate5 = stage.GetCellFromCoordinate(coordinates + Vector3Int.back);
			Cell cellFromCoordinate6 = stage.GetCellFromCoordinate(coordinates);
			TerrainCell terrainCell = cellFromCoordinate6 as TerrainCell;
			TerrainCell terrainCell2 = cellFromCoordinate2 as TerrainCell;
			bool flag = terrainCell2 != null && (consideredTilesetIds.Contains(stage.GetTilesetIdFromCell(cellFromCoordinate2)) || (cellFromCoordinate6 != null && terrainCell.TilesetIndex == terrainCell2.TilesetIndex));
			TerrainCell terrainCell3 = cellFromCoordinate3 as TerrainCell;
			bool flag2 = terrainCell3 != null && (consideredTilesetIds.Contains(stage.GetTilesetIdFromCell(cellFromCoordinate3)) || (cellFromCoordinate6 != null && terrainCell.TilesetIndex == terrainCell3.TilesetIndex));
			TerrainCell terrainCell4 = cellFromCoordinate4 as TerrainCell;
			bool flag3 = terrainCell4 != null && (consideredTilesetIds.Contains(stage.GetTilesetIdFromCell(cellFromCoordinate4)) || (cellFromCoordinate6 != null && terrainCell.TilesetIndex == terrainCell4.TilesetIndex));
			TerrainCell terrainCell5 = cellFromCoordinate5 as TerrainCell;
			bool flag4 = terrainCell5 != null && (consideredTilesetIds.Contains(stage.GetTilesetIdFromCell(cellFromCoordinate5)) || (cellFromCoordinate6 != null && terrainCell.TilesetIndex == terrainCell5.TilesetIndex));
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(flag ? 1 : 0);
			stringBuilder.Append(flag2 ? 1 : 0);
			stringBuilder.Append(flag4 ? 1 : 0);
			stringBuilder.Append(flag3 ? 1 : 0);
			int num = Convert.ToInt32(stringBuilder.ToString(), 2);
			if (num < 0 || num > 64)
			{
				return null;
			}
			Tile tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(num));
			return new TileSpawnContext
			{
				Tileset = this,
				Tile = tile,
				LeftFilled = flag,
				RightFilled = flag2,
				FrontFilled = flag3,
				BackFilled = flag4,
				AllowTopDecoration = (cellFromCoordinate == null)
			};
		}
	}
}
