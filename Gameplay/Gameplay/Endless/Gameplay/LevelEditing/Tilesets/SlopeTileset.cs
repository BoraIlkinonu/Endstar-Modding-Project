using System;
using System.Linq;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x02000530 RID: 1328
	public class SlopeTileset : Tileset
	{
		// Token: 0x06001FD8 RID: 8152 RVA: 0x0008B0DC File Offset: 0x000892DC
		public SlopeTileset(TilesetCosmeticProfile cosmeticProfile, Asset asset, Sprite displayIcon, int index)
			: base(cosmeticProfile, asset, displayIcon, index)
		{
		}

		// Token: 0x06001FD9 RID: 8153 RVA: 0x0008FB08 File Offset: 0x0008DD08
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
				Cell[] array = new Cell[] { cellFromCoordinate4, cellFromCoordinate3, cellFromCoordinate5, cellFromCoordinate2 };
				Cell[] array2 = new Cell[] { cellFromCoordinate7, cellFromCoordinate9, cellFromCoordinate8, cellFromCoordinate6 };
				bool flag5 = cellFromCoordinate6 is TerrainCell;
				bool flag6 = cellFromCoordinate7 is TerrainCell;
				bool flag7 = cellFromCoordinate8 is TerrainCell;
				bool flag8 = cellFromCoordinate9 is TerrainCell;
				int num = (flag ? 1 : 0) + (flag2 ? 1 : 0) + (flag3 ? 1 : 0) + (flag4 ? 1 : 0);
				int num2 = (flag5 ? 1 : 0) + (flag6 ? 1 : 0) + (flag7 ? 1 : 0) + (flag8 ? 1 : 0);
				if (num == 4)
				{
					if (num2 == 3 && this.SlopesArrangedInCornerConfiguration(array, stage))
					{
						for (int i = 0; i < 4; i++)
						{
							if (!this.DiagonalOfCornerIsFilled(i, array2))
							{
								tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(4 + (i + 2) % 4));
								break;
							}
						}
					}
					else if (num2 == 4)
					{
						tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(14));
					}
					else if (this.SlopeCount(array, stage) == 2)
					{
						int cornerIndexFromCellsToNonSlope = this.GetCornerIndexFromCellsToNonSlope(array);
						tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(4 + cornerIndexFromCellsToNonSlope));
					}
					else if ((flag5 && flag8 && !flag6 && !flag7) || (flag6 && flag7 && !flag5 && !flag8))
					{
						if (flag5 && flag8)
						{
							tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(13));
						}
						else
						{
							tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(12));
						}
					}
					else
					{
						tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(14));
					}
				}
				else if (num == 3)
				{
					if (this.SlopeCount(array, stage) == 4)
					{
						if (array2.Where((Cell diagonal) => diagonal != null).Count<Cell>() == 3)
						{
							tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(14));
						}
						else
						{
							int straightSlopeIndexFromCardinalCells = this.GetStraightSlopeIndexFromCardinalCells(array);
							tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(straightSlopeIndexFromCardinalCells));
						}
					}
					else
					{
						int straightSlopeIndexFromCardinalCells2 = this.GetStraightSlopeIndexFromCardinalCells(array);
						tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(straightSlopeIndexFromCardinalCells2));
					}
				}
				else if (num == 2)
				{
					if ((flag3 && flag4) || (flag2 && flag))
					{
						tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(14));
					}
					else
					{
						int cornerIndexFromCells = this.GetCornerIndexFromCells(array);
						if (cornerIndexFromCells >= 0)
						{
							if (this.CornerCellsAreNotTypes(stage, cornerIndexFromCells, array, TilesetType.Slope, TilesetType.Slope))
							{
								tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(4 + cornerIndexFromCells));
							}
							else if (this.OnlyOneCornerIsType(stage, cornerIndexFromCells, array, TilesetType.Slope))
							{
								int straightSlopeIndexTowardsFirstNeighborNotOfType = this.GetStraightSlopeIndexTowardsFirstNeighborNotOfType(array, stage, TilesetType.Slope);
								tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(straightSlopeIndexTowardsFirstNeighborNotOfType));
							}
							else if (this.CornerCellsAreTypes(stage, cornerIndexFromCells, array, TilesetType.Slope, TilesetType.Slope))
							{
								if (this.DiagonalOfCornerIsFilled(cornerIndexFromCells, array2))
								{
									tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(8 + cornerIndexFromCells));
								}
								else if (this.DiagonalOfCornerIsFilled((cornerIndexFromCells + 1) % 4, array2) && this.DiagonalOfCornerIsFilled((cornerIndexFromCells - 1 < 0) ? 3 : (cornerIndexFromCells - 1), array2))
								{
									tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(4 + cornerIndexFromCells));
								}
								else
								{
									tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(8 + cornerIndexFromCells));
								}
							}
						}
					}
				}
				else if (num == 1)
				{
					int singleNeighborIndexForCell = this.GetSingleNeighborIndexForCell(array);
					Cell cell = array[singleNeighborIndexForCell];
					if (stage.GetTilesetFromCell(cell).TilesetType == TilesetType.Slope)
					{
						if (num2 == 0 || num2 == 2)
						{
							int straightSlopeIndexTowardsFirstNeighborOfType = this.GetStraightSlopeIndexTowardsFirstNeighborOfType(array, stage, TilesetType.Slope);
							tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(straightSlopeIndexTowardsFirstNeighborOfType));
						}
						else
						{
							int singleNeighborIndexForCell2 = this.GetSingleNeighborIndexForCell(array2);
							tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(8 + singleNeighborIndexForCell2));
						}
					}
					else
					{
						int straightSlopeIndexTowardsFirstNeighborNotOfType2 = this.GetStraightSlopeIndexTowardsFirstNeighborNotOfType(array, stage, TilesetType.Slope);
						tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(straightSlopeIndexTowardsFirstNeighborNotOfType2));
					}
				}
				else
				{
					tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(0));
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

		// Token: 0x06001FDA RID: 8154 RVA: 0x000900A8 File Offset: 0x0008E2A8
		private bool SlopesArrangedInCornerConfiguration(Cell[] cardinalCells, Stage stage)
		{
			if (this.SlopeCount(cardinalCells, stage) < 2)
			{
				return false;
			}
			bool flag = false;
			for (int i = 0; i < cardinalCells.Length; i++)
			{
				if (stage.GetTilesetFromCell(cardinalCells[i]).TilesetType == TilesetType.Slope && stage.GetTilesetFromCell(cardinalCells[(i + 1) % 4]).TilesetType == TilesetType.Slope)
				{
					flag = true;
					break;
				}
			}
			return flag;
		}

		// Token: 0x06001FDB RID: 8155 RVA: 0x00090100 File Offset: 0x0008E300
		public int SlopeCount(Cell[] cells, Stage stage)
		{
			return cells.Where(delegate(Cell cell)
			{
				Tileset tilesetFromCell = stage.GetTilesetFromCell(cell);
				return cell is TerrainCell && tilesetFromCell != null && tilesetFromCell.TilesetType == TilesetType.Slope;
			}).Count<Cell>();
		}

		// Token: 0x06001FDC RID: 8156 RVA: 0x00090134 File Offset: 0x0008E334
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

		// Token: 0x06001FDD RID: 8157 RVA: 0x000901C8 File Offset: 0x0008E3C8
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

		// Token: 0x06001FDE RID: 8158 RVA: 0x00090230 File Offset: 0x0008E430
		private bool CornerCellsAreTypes(Stage stage, int cornerIndex, Cell[] cells, TilesetType typeOne, TilesetType typeTwo)
		{
			Cell cell;
			Cell cell2;
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
			return tilesetFromCell != null && cell is TerrainCell && tilesetFromCell2 != null && cell2 is TerrainCell && ((tilesetFromCell.TilesetType == typeOne && tilesetFromCell2.TilesetType == typeTwo) || (tilesetFromCell.TilesetType == typeTwo && tilesetFromCell2.TilesetType == typeOne));
		}

		// Token: 0x06001FDF RID: 8159 RVA: 0x000902D0 File Offset: 0x0008E4D0
		private bool CornerCellsAreNotTypes(Stage stage, int cornerIndex, Cell[] cells, TilesetType typeOne, TilesetType typeTwo)
		{
			Cell cell;
			Cell cell2;
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
			return tilesetFromCell != null && cell is TerrainCell && tilesetFromCell2 != null && cell2 is TerrainCell && ((tilesetFromCell.TilesetType != typeOne && tilesetFromCell2.TilesetType != typeTwo) || (tilesetFromCell.TilesetType != typeTwo && tilesetFromCell2.TilesetType != typeOne));
		}

		// Token: 0x06001FE0 RID: 8160 RVA: 0x00090370 File Offset: 0x0008E570
		private bool OnlyOneCornerIsType(Stage stage, int cornerIndex, Cell[] cells, TilesetType type)
		{
			Cell cell;
			Cell cell2;
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
			return tilesetFromCell != null && cell is TerrainCell && tilesetFromCell2 != null && cell2 is TerrainCell && ((tilesetFromCell.TilesetType == type && tilesetFromCell2.TilesetType != type) || (tilesetFromCell.TilesetType != type && tilesetFromCell2.TilesetType == type));
		}

		// Token: 0x06001FE1 RID: 8161 RVA: 0x0009040E File Offset: 0x0008E60E
		private bool DiagonalOfCornerIsFilled(int cornerIndex, Cell[] cells)
		{
			return cells[cornerIndex] is TerrainCell;
		}

		// Token: 0x06001FE2 RID: 8162 RVA: 0x0009041C File Offset: 0x0008E61C
		private int GetSingleNeighborIndexForCell(Cell[] cells)
		{
			int num = 0;
			while (num < cells.Length && !(cells[num] is TerrainCell))
			{
				num++;
			}
			if (num != cells.Length)
			{
				return num;
			}
			return 0;
		}

		// Token: 0x06001FE3 RID: 8163 RVA: 0x0009044C File Offset: 0x0008E64C
		private int GetStraightSlopeIndexFromCardinalCells(Cell[] cells)
		{
			if (!(cells[0] is TerrainCell) || !(cells[2] is TerrainCell))
			{
				TerrainCell terrainCell = cells[1] as TerrainCell;
				if (terrainCell != null && terrainCell.TilesetIndex != -1)
				{
					TerrainCell terrainCell2 = cells[3] as TerrainCell;
					if (terrainCell2 != null && terrainCell2.TilesetIndex != -1)
					{
						if (cells[0] is TerrainCell)
						{
							return 0;
						}
						return 2;
					}
				}
				return 0;
			}
			if (cells[1] is TerrainCell)
			{
				return 1;
			}
			return 3;
		}

		// Token: 0x06001FE4 RID: 8164 RVA: 0x000904B4 File Offset: 0x0008E6B4
		private int GetStraightSlopeIndexTowardsFirstNeighborOfType(Cell[] cells, Stage stage, TilesetType type)
		{
			int num = 0;
			while (num < cells.Length && (!(cells[num] is TerrainCell) || stage.GetTilesetFromCell(cells[num]).TilesetType != type))
			{
				num++;
			}
			if (num != cells.Length)
			{
				return num;
			}
			return 0;
		}

		// Token: 0x06001FE5 RID: 8165 RVA: 0x000904F4 File Offset: 0x0008E6F4
		private int GetStraightSlopeIndexTowardsFirstNeighborNotOfType(Cell[] cells, Stage stage, TilesetType type)
		{
			int num = 0;
			while (num < cells.Length && (!(cells[num] is TerrainCell) || stage.GetTilesetFromCell(cells[num]).TilesetType == type))
			{
				num++;
			}
			if (num != cells.Length)
			{
				return num;
			}
			return 0;
		}
	}
}
