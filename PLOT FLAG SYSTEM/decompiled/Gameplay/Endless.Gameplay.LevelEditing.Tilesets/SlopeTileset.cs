using System.Linq;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public class SlopeTileset : Tileset
{
	public SlopeTileset(TilesetCosmeticProfile cosmeticProfile, Asset asset, Sprite displayIcon, int index)
		: base(cosmeticProfile, asset, displayIcon, index)
	{
	}

	public override TileSpawnContext GetValidVisualForCellPosition(Stage stage, Vector3Int coordinates)
	{
		Tile tile = null;
		Cell cellFromCoordinate = stage.GetCellFromCoordinate(coordinates + Vector3Int.up);
		Cell cellFromCoordinate2 = stage.GetCellFromCoordinate(coordinates + Vector3Int.left);
		Cell cellFromCoordinate3 = stage.GetCellFromCoordinate(coordinates + Vector3Int.right);
		Cell cellFromCoordinate4 = stage.GetCellFromCoordinate(coordinates + Vector3Int.forward);
		Cell cellFromCoordinate5 = stage.GetCellFromCoordinate(coordinates + Vector3Int.back);
		bool flag = cellFromCoordinate2 is TerrainCell;
		bool flag2 = cellFromCoordinate3 is TerrainCell;
		bool flag3 = cellFromCoordinate4 is TerrainCell;
		bool flag4 = cellFromCoordinate5 is TerrainCell;
		if (cellFromCoordinate is TerrainCell)
		{
			tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(15));
		}
		else
		{
			Cell cellFromCoordinate6 = stage.GetCellFromCoordinate(coordinates + Vector3Int.left + Vector3Int.forward);
			Cell cellFromCoordinate7 = stage.GetCellFromCoordinate(coordinates + Vector3Int.right + Vector3Int.forward);
			Cell cellFromCoordinate8 = stage.GetCellFromCoordinate(coordinates + Vector3Int.back + Vector3Int.left);
			Cell cellFromCoordinate9 = stage.GetCellFromCoordinate(coordinates + Vector3Int.back + Vector3Int.right);
			Cell[] array = new Cell[4] { cellFromCoordinate4, cellFromCoordinate3, cellFromCoordinate5, cellFromCoordinate2 };
			Cell[] array2 = new Cell[4] { cellFromCoordinate7, cellFromCoordinate9, cellFromCoordinate8, cellFromCoordinate6 };
			bool flag5 = cellFromCoordinate6 is TerrainCell;
			bool flag6 = cellFromCoordinate7 is TerrainCell;
			bool flag7 = cellFromCoordinate8 is TerrainCell;
			bool flag8 = cellFromCoordinate9 is TerrainCell;
			int num = (flag ? 1 : 0) + (flag2 ? 1 : 0) + (flag3 ? 1 : 0) + (flag4 ? 1 : 0);
			int num2 = (flag5 ? 1 : 0) + (flag6 ? 1 : 0) + (flag7 ? 1 : 0) + (flag8 ? 1 : 0);
			switch (num)
			{
			case 4:
				if (num2 == 3 && SlopesArrangedInCornerConfiguration(array, stage))
				{
					for (int num3 = 0; num3 < 4; num3++)
					{
						if (!DiagonalOfCornerIsFilled(num3, array2))
						{
							tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(4 + (num3 + 2) % 4));
							break;
						}
					}
				}
				else if (num2 == 4)
				{
					tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(14));
				}
				else if (SlopeCount(array, stage) == 2)
				{
					int cornerIndexFromCellsToNonSlope = GetCornerIndexFromCellsToNonSlope(array);
					tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(4 + cornerIndexFromCellsToNonSlope));
				}
				else
				{
					tile = (((!(flag5 && flag8) || flag6 || flag7) && (!(flag6 && flag7) || flag5 || flag8)) ? new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(14)) : ((!(flag5 && flag8)) ? new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(12)) : new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(13))));
				}
				break;
			case 3:
				if (SlopeCount(array, stage) == 4)
				{
					if (array2.Where((Cell diagonal) => diagonal != null).Count() == 3)
					{
						tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(14));
						break;
					}
					int straightSlopeIndexFromCardinalCells = GetStraightSlopeIndexFromCardinalCells(array);
					tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(straightSlopeIndexFromCardinalCells));
				}
				else
				{
					int straightSlopeIndexFromCardinalCells2 = GetStraightSlopeIndexFromCardinalCells(array);
					tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(straightSlopeIndexFromCardinalCells2));
				}
				break;
			case 2:
			{
				if ((flag3 && flag4) || (flag2 && flag))
				{
					tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(14));
					break;
				}
				int cornerIndexFromCells = GetCornerIndexFromCells(array);
				if (cornerIndexFromCells >= 0)
				{
					if (CornerCellsAreNotTypes(stage, cornerIndexFromCells, array, TilesetType.Slope, TilesetType.Slope))
					{
						tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(4 + cornerIndexFromCells));
					}
					else if (OnlyOneCornerIsType(stage, cornerIndexFromCells, array, TilesetType.Slope))
					{
						int straightSlopeIndexTowardsFirstNeighborNotOfType2 = GetStraightSlopeIndexTowardsFirstNeighborNotOfType(array, stage, TilesetType.Slope);
						tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(straightSlopeIndexTowardsFirstNeighborNotOfType2));
					}
					else if (CornerCellsAreTypes(stage, cornerIndexFromCells, array, TilesetType.Slope, TilesetType.Slope))
					{
						tile = ((!DiagonalOfCornerIsFilled(cornerIndexFromCells, array2)) ? ((!DiagonalOfCornerIsFilled((cornerIndexFromCells + 1) % 4, array2) || !DiagonalOfCornerIsFilled((cornerIndexFromCells - 1 < 0) ? 3 : (cornerIndexFromCells - 1), array2)) ? new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(8 + cornerIndexFromCells)) : new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(4 + cornerIndexFromCells))) : new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(8 + cornerIndexFromCells)));
					}
				}
				break;
			}
			case 1:
			{
				int singleNeighborIndexForCell = GetSingleNeighborIndexForCell(array);
				Cell cell = array[singleNeighborIndexForCell];
				if (stage.GetTilesetFromCell(cell).TilesetType == TilesetType.Slope)
				{
					if (num2 == 0 || num2 == 2)
					{
						int straightSlopeIndexTowardsFirstNeighborOfType = GetStraightSlopeIndexTowardsFirstNeighborOfType(array, stage, TilesetType.Slope);
						tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(straightSlopeIndexTowardsFirstNeighborOfType));
					}
					else
					{
						int singleNeighborIndexForCell2 = GetSingleNeighborIndexForCell(array2);
						tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(8 + singleNeighborIndexForCell2));
					}
				}
				else
				{
					int straightSlopeIndexTowardsFirstNeighborNotOfType = GetStraightSlopeIndexTowardsFirstNeighborNotOfType(array, stage, TilesetType.Slope);
					tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(straightSlopeIndexTowardsFirstNeighborNotOfType));
				}
				break;
			}
			default:
				tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(0));
				break;
			}
		}
		return new TileSpawnContext
		{
			Tileset = this,
			Tile = tile,
			TopFilled = false,
			BottomFilled = false,
			LeftFilled = flag,
			RightFilled = flag2,
			FrontFilled = flag3,
			BackFilled = flag4
		};
	}

	private bool SlopesArrangedInCornerConfiguration(Cell[] cardinalCells, Stage stage)
	{
		if (SlopeCount(cardinalCells, stage) < 2)
		{
			return false;
		}
		bool result = false;
		for (int i = 0; i < cardinalCells.Length; i++)
		{
			if (stage.GetTilesetFromCell(cardinalCells[i]).TilesetType == TilesetType.Slope && stage.GetTilesetFromCell(cardinalCells[(i + 1) % 4]).TilesetType == TilesetType.Slope)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public int SlopeCount(Cell[] cells, Stage stage)
	{
		return cells.Where(delegate(Cell cell)
		{
			Tileset tilesetFromCell = stage.GetTilesetFromCell(cell);
			return cell is TerrainCell && tilesetFromCell != null && tilesetFromCell.TilesetType == TilesetType.Slope;
		}).Count();
	}

	public int GetCornerIndexFromCellsToNonSlope(Cell[] cells)
	{
		TerrainCell terrainCell = cells[0] as TerrainCell;
		TerrainCell terrainCell2 = cells[1] as TerrainCell;
		TerrainCell terrainCell3 = cells[2] as TerrainCell;
		TerrainCell terrainCell4 = cells[3] as TerrainCell;
		if (terrainCell != null && terrainCell2 != null && !terrainCell.IsSlope() && !terrainCell2.IsSlope())
		{
			return 0;
		}
		if (terrainCell2 != null && terrainCell3 != null && !terrainCell2.IsSlope() && !terrainCell3.IsSlope())
		{
			return 1;
		}
		if (terrainCell3 != null && terrainCell4 != null && !terrainCell3.IsSlope() && !terrainCell4.IsSlope())
		{
			return 2;
		}
		if (terrainCell4 != null && terrainCell != null && !terrainCell4.IsSlope() && !terrainCell.IsSlope())
		{
			return 3;
		}
		return -1;
	}

	public int GetCornerIndexFromCells(Cell[] cells)
	{
		if (cells[0] is TerrainCell && cells[1] is TerrainCell)
		{
			return 0;
		}
		if (cells[1] is TerrainCell && cells[2] is TerrainCell)
		{
			return 1;
		}
		if (cells[2] is TerrainCell && cells[3] is TerrainCell)
		{
			return 2;
		}
		if (cells[3] is TerrainCell && cells[0] is TerrainCell)
		{
			return 3;
		}
		return -1;
	}

	private bool CornerCellsAreTypes(Stage stage, int cornerIndex, Cell[] cells, TilesetType typeOne, TilesetType typeTwo)
	{
		Cell cell = null;
		Cell cell2 = null;
		switch (cornerIndex)
		{
		default:
			cell = cells[0];
			cell2 = cells[1];
			break;
		case 1:
			cell = cells[1];
			cell2 = cells[2];
			break;
		case 2:
			cell = cells[2];
			cell2 = cells[3];
			break;
		case 3:
			cell = cells[3];
			cell2 = cells[0];
			break;
		}
		Tileset tilesetFromCell = stage.GetTilesetFromCell(cell);
		Tileset tilesetFromCell2 = stage.GetTilesetFromCell(cell2);
		if (tilesetFromCell != null && cell is TerrainCell && tilesetFromCell2 != null && cell2 is TerrainCell && ((tilesetFromCell.TilesetType == typeOne && tilesetFromCell2.TilesetType == typeTwo) || (tilesetFromCell.TilesetType == typeTwo && tilesetFromCell2.TilesetType == typeOne)))
		{
			return true;
		}
		return false;
	}

	private bool CornerCellsAreNotTypes(Stage stage, int cornerIndex, Cell[] cells, TilesetType typeOne, TilesetType typeTwo)
	{
		Cell cell = null;
		Cell cell2 = null;
		switch (cornerIndex)
		{
		default:
			cell = cells[0];
			cell2 = cells[1];
			break;
		case 1:
			cell = cells[1];
			cell2 = cells[2];
			break;
		case 2:
			cell = cells[2];
			cell2 = cells[3];
			break;
		case 3:
			cell = cells[3];
			cell2 = cells[0];
			break;
		}
		Tileset tilesetFromCell = stage.GetTilesetFromCell(cell);
		Tileset tilesetFromCell2 = stage.GetTilesetFromCell(cell2);
		if (tilesetFromCell != null && cell is TerrainCell && tilesetFromCell2 != null && cell2 is TerrainCell && ((tilesetFromCell.TilesetType != typeOne && tilesetFromCell2.TilesetType != typeTwo) || (tilesetFromCell.TilesetType != typeTwo && tilesetFromCell2.TilesetType != typeOne)))
		{
			return true;
		}
		return false;
	}

	private bool OnlyOneCornerIsType(Stage stage, int cornerIndex, Cell[] cells, TilesetType type)
	{
		Cell cell = null;
		Cell cell2 = null;
		switch (cornerIndex)
		{
		default:
			cell = cells[0];
			cell2 = cells[1];
			break;
		case 1:
			cell = cells[1];
			cell2 = cells[2];
			break;
		case 2:
			cell = cells[2];
			cell2 = cells[3];
			break;
		case 3:
			cell = cells[3];
			cell2 = cells[0];
			break;
		}
		Tileset tilesetFromCell = stage.GetTilesetFromCell(cell);
		Tileset tilesetFromCell2 = stage.GetTilesetFromCell(cell2);
		if (tilesetFromCell != null && cell is TerrainCell && tilesetFromCell2 != null && cell2 is TerrainCell && ((tilesetFromCell.TilesetType == type && tilesetFromCell2.TilesetType != type) || (tilesetFromCell.TilesetType != type && tilesetFromCell2.TilesetType == type)))
		{
			return true;
		}
		return false;
	}

	private bool DiagonalOfCornerIsFilled(int cornerIndex, Cell[] cells)
	{
		return cells[cornerIndex] is TerrainCell;
	}

	private int GetSingleNeighborIndexForCell(Cell[] cells)
	{
		int i;
		for (i = 0; i < cells.Length && !(cells[i] is TerrainCell); i++)
		{
		}
		if (i != cells.Length)
		{
			return i;
		}
		return 0;
	}

	private int GetStraightSlopeIndexFromCardinalCells(Cell[] cells)
	{
		if (cells[0] is TerrainCell && cells[2] is TerrainCell)
		{
			if (cells[1] is TerrainCell)
			{
				return 1;
			}
			return 3;
		}
		if (cells[1] is TerrainCell { TilesetIndex: not -1 } && cells[3] is TerrainCell { TilesetIndex: not -1 })
		{
			if (cells[0] is TerrainCell)
			{
				return 0;
			}
			return 2;
		}
		return 0;
	}

	private int GetStraightSlopeIndexTowardsFirstNeighborOfType(Cell[] cells, Stage stage, TilesetType type)
	{
		int i;
		for (i = 0; i < cells.Length && (!(cells[i] is TerrainCell) || stage.GetTilesetFromCell(cells[i]).TilesetType != type); i++)
		{
		}
		if (i != cells.Length)
		{
			return i;
		}
		return 0;
	}

	private int GetStraightSlopeIndexTowardsFirstNeighborNotOfType(Cell[] cells, Stage stage, TilesetType type)
	{
		int i;
		for (i = 0; i < cells.Length && (!(cells[i] is TerrainCell) || stage.GetTilesetFromCell(cells[i]).TilesetType == type); i++)
		{
		}
		if (i != cells.Length)
		{
			return i;
		}
		return 0;
	}
}
