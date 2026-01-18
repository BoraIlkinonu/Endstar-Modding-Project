using System;
using System.Diagnostics;
using System.Linq;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public class BaseTileset : Tileset
{
	public BaseTileset(TilesetCosmeticProfile cosmeticProfile, Asset asset, Sprite displayIcon, int index)
		: base(cosmeticProfile, asset, displayIcon, index)
	{
	}

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
		bool flag = cellFromCoordinate is TerrainCell terrainCell2 && ((stage.GetTilesetFromCell(cellFromCoordinate) != null && stage.GetTilesetFromCell(cellFromCoordinate).TilesetType == TilesetType.Slope) || (stage.GetTilesetFromCell(cellFromCoordinate) != null && consideredTilesetIds.Contains<SerializableGuid>(stage.GetTilesetIdFromCell(cellFromCoordinate))) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell2.TilesetIndex));
		bool flag2 = cellFromCoordinate2 is TerrainCell terrainCell3 && (consideredTilesetIds.Contains<SerializableGuid>(stage.GetTilesetIdFromCell(cellFromCoordinate2)) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell3.TilesetIndex));
		bool flag3 = cellFromCoordinate3 is TerrainCell terrainCell4 && (consideredTilesetIds.Contains<SerializableGuid>(stage.GetTilesetIdFromCell(cellFromCoordinate3)) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell4.TilesetIndex));
		bool flag4 = cellFromCoordinate4 is TerrainCell terrainCell5 && (consideredTilesetIds.Contains<SerializableGuid>(stage.GetTilesetIdFromCell(cellFromCoordinate4)) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell5.TilesetIndex));
		bool flag5 = cellFromCoordinate5 is TerrainCell terrainCell6 && (consideredTilesetIds.Contains<SerializableGuid>(stage.GetTilesetIdFromCell(cellFromCoordinate5)) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell6.TilesetIndex));
		bool flag6 = cellFromCoordinate6 is TerrainCell terrainCell7 && (consideredTilesetIds.Contains<SerializableGuid>(stage.GetTilesetIdFromCell(cellFromCoordinate6)) || (cellFromCoordinate7 != null && terrainCell.TilesetIndex == terrainCell7.TilesetIndex));
		int num = Convert.ToInt32($"{(flag ? 1 : 0)}{(flag2 ? 1 : 0)}{(flag3 ? 1 : 0)}{(flag4 ? 1 : 0)}{(flag6 ? 1 : 0)}{(flag5 ? 1 : 0)}", 2);
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
