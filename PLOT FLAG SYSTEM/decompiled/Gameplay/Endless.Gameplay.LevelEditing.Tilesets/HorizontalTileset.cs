using System;
using System.Linq;
using System.Text;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public class HorizontalTileset : Tileset
{
	public HorizontalTileset(TilesetCosmeticProfile cosmeticProfile, Asset asset, Sprite displayIcon, int index)
		: base(cosmeticProfile, asset, displayIcon, index)
	{
	}

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
		bool flag = cellFromCoordinate2 is TerrainCell terrainCell2 && (consideredTilesetIds.Contains<SerializableGuid>(stage.GetTilesetIdFromCell(cellFromCoordinate2)) || (cellFromCoordinate6 != null && terrainCell.TilesetIndex == terrainCell2.TilesetIndex));
		bool flag2 = cellFromCoordinate3 is TerrainCell terrainCell3 && (consideredTilesetIds.Contains<SerializableGuid>(stage.GetTilesetIdFromCell(cellFromCoordinate3)) || (cellFromCoordinate6 != null && terrainCell.TilesetIndex == terrainCell3.TilesetIndex));
		bool flag3 = cellFromCoordinate4 is TerrainCell terrainCell4 && (consideredTilesetIds.Contains<SerializableGuid>(stage.GetTilesetIdFromCell(cellFromCoordinate4)) || (cellFromCoordinate6 != null && terrainCell.TilesetIndex == terrainCell4.TilesetIndex));
		bool flag4 = cellFromCoordinate5 is TerrainCell terrainCell5 && (consideredTilesetIds.Contains<SerializableGuid>(stage.GetTilesetIdFromCell(cellFromCoordinate5)) || (cellFromCoordinate6 != null && terrainCell.TilesetIndex == terrainCell5.TilesetIndex));
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
