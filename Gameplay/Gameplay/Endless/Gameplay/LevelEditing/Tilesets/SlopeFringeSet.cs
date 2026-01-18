using System;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x0200052D RID: 1325
	public class SlopeFringeSet : FringeSet
	{
		// Token: 0x06001FC9 RID: 8137 RVA: 0x0008EC22 File Offset: 0x0008CE22
		public SlopeFringeSet(SlopeFringeCosmeticProfile slopeFringeCosmeticSet)
		{
			this.cosmeticSet = slopeFringeCosmeticSet;
		}

		// Token: 0x06001FCA RID: 8138 RVA: 0x0008EC34 File Offset: 0x0008CE34
		public override void AddFringe(Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage)
		{
			if (stage.GetCellFromCoordinate(coordinate + Vector3Int.up) is TerrainCell)
			{
				return;
			}
			bool[] array = this.CalculateValidFringeDirections(context, coordinate, stage);
			int num = array.Where((bool valid) => valid).Count<bool>();
			bool[] array2 = array.Select((bool valid) => !valid).ToArray<bool>();
			int num2 = array2.Where((bool valid) => valid).Count<bool>();
			if (num > 0)
			{
				switch (context.Tile.Index)
				{
				case 0:
				case 1:
				case 2:
				case 3:
					this.AddFlatSlopeFringe(array, num, parent, coordinate, context, stage);
					break;
				case 4:
				case 5:
				case 6:
				case 7:
					this.AddInnerCornerSlopeFringe(this.cosmeticSet.InnerCornerFringe, this.cosmeticSet.InnerCornerFringePeninsulas, array, num, parent, context);
					break;
				case 8:
				case 9:
				case 10:
				case 11:
					this.AddOuterCornerSlopeFringe(this.cosmeticSet.OuterCornerFringe, this.cosmeticSet.OuterCornerFringePeninsulas, array, num, parent, context);
					break;
				}
			}
			if (num2 > 0)
			{
				switch (context.Tile.Index)
				{
				case 0:
				case 1:
				case 2:
				case 3:
					this.AddFlatVerticalFringe(array2, parent, coordinate, context, stage);
					return;
				case 4:
				case 5:
				case 6:
				case 7:
					this.AddInnerCornerVerticalFringe(parent, coordinate, context, stage);
					break;
				case 8:
				case 9:
				case 10:
				case 11:
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x06001FCB RID: 8139 RVA: 0x0008EDE8 File Offset: 0x0008CFE8
		private bool[] CalculateValidFringeDirections(TileSpawnContext context, Vector3Int coordinate, Stage stage)
		{
			bool[] array = new bool[]
			{
				!context.FrontFilled,
				!context.RightFilled,
				!context.BackFilled,
				!context.LeftFilled
			};
			int index = context.Tile.Index;
			if (index > 3)
			{
				if (index - 8 <= 3)
				{
					array = this.CalculateValidFringeForOuterCorner(array, stage, context, coordinate);
				}
			}
			else
			{
				array = this.CalculateValidFringeForSingleSlope(array, stage, context, coordinate);
			}
			return array;
		}

		// Token: 0x06001FCC RID: 8140 RVA: 0x0008EE60 File Offset: 0x0008D060
		private bool[] CalculateValidFringeForSingleSlope(bool[] originalValidDirections, Stage stage, TileSpawnContext context, Vector3Int coordinate)
		{
			Vector3Int vector3Int = Vector3Int.zero;
			int num = 0;
			switch (context.Tile.Index)
			{
			case 0:
				num = 2;
				vector3Int = Vector3Int.down + Vector3Int.back;
				break;
			case 1:
				num = 3;
				vector3Int = Vector3Int.down + Vector3Int.left;
				break;
			case 2:
				num = 0;
				vector3Int = Vector3Int.down + Vector3Int.forward;
				break;
			case 3:
				num = 1;
				vector3Int = Vector3Int.down + Vector3Int.right;
				break;
			}
			if (stage.GetCellFromCoordinate(coordinate + vector3Int) is TerrainCell)
			{
				originalValidDirections[num] = false;
			}
			return originalValidDirections;
		}

		// Token: 0x06001FCD RID: 8141 RVA: 0x0008EF04 File Offset: 0x0008D104
		private bool[] CalculateValidFringeForOuterCorner(bool[] originalValidDirections, Stage stage, TileSpawnContext context, Vector3Int coordinate)
		{
			Vector3Int vector3Int = Vector3Int.zero;
			Vector3Int vector3Int2 = Vector3Int.zero;
			int num = 0;
			int num2 = 0;
			switch (context.Tile.Index)
			{
			case 8:
				num = 2;
				num2 = 3;
				vector3Int = Vector3Int.down + Vector3Int.back;
				vector3Int2 = Vector3Int.down + Vector3Int.left;
				break;
			case 9:
				num = 0;
				num2 = 3;
				vector3Int = Vector3Int.down + Vector3Int.forward;
				vector3Int2 = Vector3Int.down + Vector3Int.left;
				break;
			case 10:
				num = 1;
				num2 = 0;
				vector3Int = Vector3Int.down + Vector3Int.right;
				vector3Int2 = Vector3Int.down + Vector3Int.forward;
				break;
			case 11:
				num = 1;
				num2 = 2;
				vector3Int = Vector3Int.down + Vector3Int.right;
				vector3Int2 = Vector3Int.down + Vector3Int.back;
				break;
			}
			if (stage.GetCellFromCoordinate(coordinate + vector3Int) is TerrainCell)
			{
				originalValidDirections[num] = false;
			}
			if (stage.GetCellFromCoordinate(coordinate + vector3Int2) is TerrainCell)
			{
				originalValidDirections[num2] = false;
			}
			return originalValidDirections;
		}

		// Token: 0x06001FCE RID: 8142 RVA: 0x0008F018 File Offset: 0x0008D218
		private void AddFlatSlopeFringe(bool[] validFringeDirections, int validDirectionCount, Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage)
		{
			switch (validDirectionCount)
			{
			case 1:
			{
				for (int i = 0; i < validFringeDirections.Length; i++)
				{
					if (validFringeDirections[(i + context.Tile.Index) % 4])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[i], (context.Tile.Index + 2) % 4, parent);
						return;
					}
				}
				return;
			}
			case 2:
			{
				if ((validFringeDirections[0] && validFringeDirections[1]) || (validFringeDirections[0] && validFringeDirections[1]))
				{
					for (int j = 0; j < validFringeDirections.Length; j++)
					{
						if (validFringeDirections[(j + context.Tile.Index) % 4] && validFringeDirections[(j + context.Tile.Index + 1) % 4])
						{
							base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeCorners[j], (context.Tile.Index + 2) % 4, parent);
							return;
						}
					}
					return;
				}
				for (int k = 0; k < validFringeDirections.Length; k++)
				{
					if (validFringeDirections[k])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[(k + context.Tile.Index) % 4], (context.Tile.Index + 2) % 4, parent);
					}
				}
				return;
			}
			case 3:
			{
				for (int l = 0; l < validFringeDirections.Length; l++)
				{
					if (validFringeDirections[(l + context.Tile.Index) % 4] && validFringeDirections[(l + context.Tile.Index + 1) % 4])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringePeninsulas[l], (context.Tile.Index + 2) % 4, parent);
						return;
					}
				}
				return;
			}
			case 4:
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeIsland, (context.Tile.Index + 2) % 4, parent);
				return;
			default:
				return;
			}
		}

		// Token: 0x06001FCF RID: 8143 RVA: 0x0008F1C0 File Offset: 0x0008D3C0
		private void AddFlatVerticalFringe(bool[] validVerticalFringeDirections, Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage)
		{
			Vector3Int vector3Int = Vector3Int.up;
			Vector3Int[] array = new Vector3Int[]
			{
				Vector3Int.forward,
				Vector3Int.right,
				Vector3Int.back,
				Vector3Int.left
			};
			int num;
			switch (context.Tile.Index)
			{
			default:
				num = 0;
				vector3Int += Vector3Int.forward;
				break;
			case 1:
				num = 1;
				vector3Int += Vector3Int.right;
				break;
			case 2:
				num = 2;
				vector3Int += Vector3Int.back;
				break;
			case 3:
				num = 3;
				vector3Int += Vector3Int.left;
				break;
			}
			int num2 = (num + 1) % 4;
			int num3 = (num + 2) % 4;
			int num4 = (num + 3) % 4;
			int num5 = (num + 2) % 4;
			Cell cellFromCoordinate = stage.GetCellFromCoordinate(coordinate + vector3Int);
			Tileset tilesetFromCell = stage.GetTilesetFromCell(cellFromCoordinate);
			if (cellFromCoordinate is TerrainCell && tilesetFromCell.TilesetType == TilesetType.Base)
			{
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.VerticalFringes[0], num5, parent);
			}
			Cell cellFromCoordinate2 = stage.GetCellFromCoordinate(coordinate + array[num2]);
			Tileset tilesetFromCell2 = stage.GetTilesetFromCell(cellFromCoordinate2);
			if (cellFromCoordinate2 is TerrainCell && tilesetFromCell2.TilesetType == TilesetType.Base)
			{
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.VerticalFringes[1], num5, parent);
			}
			Cell cellFromCoordinate3 = stage.GetCellFromCoordinate(coordinate + array[num4]);
			Tileset tilesetFromCell3 = stage.GetTilesetFromCell(cellFromCoordinate3);
			if (cellFromCoordinate3 is TerrainCell && tilesetFromCell3.TilesetType == TilesetType.Base)
			{
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.VerticalFringes[3], num5, parent);
			}
		}

		// Token: 0x06001FD0 RID: 8144 RVA: 0x0008F360 File Offset: 0x0008D560
		private void AddOuterCornerSlopeFringe(GameObject cornerFringe, GameObject[] peninsulaFringes, bool[] validFringeDirections, int validDirectionCount, Transform parent, TileSpawnContext context)
		{
			switch (validDirectionCount)
			{
			case 1:
				switch (context.Tile.Index)
				{
				case 8:
					if (validFringeDirections[0])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[3], 3, parent);
						return;
					}
					if (validFringeDirections[1])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[1], 2, parent);
						return;
					}
					if (validFringeDirections[2])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[2], 2, parent);
						return;
					}
					if (validFringeDirections[3])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[2], 3, parent);
						return;
					}
					break;
				case 9:
					if (validFringeDirections[2])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[1], 3, parent);
						return;
					}
					if (validFringeDirections[1])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[3], 0, parent);
						return;
					}
					if (validFringeDirections[0])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[2], 0, parent);
						return;
					}
					if (validFringeDirections[3])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[2], 3, parent);
						return;
					}
					break;
				case 10:
					if (validFringeDirections[2])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[3], 1, parent);
						return;
					}
					if (validFringeDirections[3])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[1], 0, parent);
						return;
					}
					if (validFringeDirections[0])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[2], 0, parent);
						return;
					}
					if (validFringeDirections[1])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[2], 1, parent);
						return;
					}
					break;
				case 11:
					if (validFringeDirections[0])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[1], 1, parent);
						return;
					}
					if (validFringeDirections[3])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[3], 2, parent);
						return;
					}
					if (validFringeDirections[2])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[2], 2, parent);
						return;
					}
					if (validFringeDirections[1])
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[2], 1, parent);
						return;
					}
					break;
				default:
					return;
				}
				break;
			case 2:
				if ((!validFringeDirections[0] || !validFringeDirections[2]) && (!validFringeDirections[1] || !validFringeDirections[3]))
				{
					base.SpawnFringeWithRotationIndex(cornerFringe, (context.Tile.Index + 2) % 4, parent);
					return;
				}
				if (validFringeDirections[0] && validFringeDirections[2])
				{
					if (context.Tile.Index % 4 == 0 || context.Tile.Index % 4 == 3)
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[(context.Tile.Index + 2) % 4], (context.Tile.Index + 2) % 4, parent);
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[(context.Tile.Index + 3) % 4], (context.Tile.Index + 3) % 4, parent);
						return;
					}
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[context.Tile.Index % 4], (context.Tile.Index + 2) % 4, parent);
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[(context.Tile.Index + 1) % 4], (context.Tile.Index + 3) % 4, parent);
					return;
				}
				else
				{
					if (context.Tile.Index % 4 == 0 || context.Tile.Index % 4 == 1)
					{
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[(context.Tile.Index + 1) % 4], (context.Tile.Index + 2) % 4, parent);
						base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[(context.Tile.Index + 2) % 4], (context.Tile.Index + 3) % 4, parent);
						return;
					}
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[context.Tile.Index % 4], (context.Tile.Index + 3) % 4, parent);
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeSingles[(context.Tile.Index + 3) % 4], (context.Tile.Index + 2) % 4, parent);
					return;
				}
				break;
			case 3:
			{
				for (int i = 0; i < validFringeDirections.Length; i++)
				{
					if (context.Tile.Index % 2 == 0)
					{
						if (!validFringeDirections[(i + context.Tile.Index + 2) % 4])
						{
							base.SpawnFringeWithRotationIndex(peninsulaFringes[i], (context.Tile.Index + 2) % 4, parent);
							return;
						}
					}
					else if (!validFringeDirections[(i + context.Tile.Index + 3) % 4])
					{
						base.SpawnFringeWithRotationIndex(peninsulaFringes[i], (context.Tile.Index + 2) % 4, parent);
						return;
					}
				}
				return;
			}
			case 4:
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeIsland, (context.Tile.Index + 2) % 4, parent);
				break;
			default:
				return;
			}
		}

		// Token: 0x06001FD1 RID: 8145 RVA: 0x0008F848 File Offset: 0x0008DA48
		private void AddInnerCornerSlopeFringe(GameObject cornerFringe, GameObject[] peninsulaFringes, bool[] validFringeDirections, int validDirectionCount, Transform parent, TileSpawnContext context)
		{
			switch (validDirectionCount)
			{
			case 2:
				base.SpawnFringeWithRotationIndex(cornerFringe, (context.Tile.Index + 2) % 4, parent);
				return;
			case 3:
			{
				for (int i = 0; i < validFringeDirections.Length; i++)
				{
					if (!validFringeDirections[i])
					{
						base.SpawnFringeWithRotationIndex(peninsulaFringes[(i + 2) % 4], (context.Tile.Index + 2) % 4, parent);
						return;
					}
				}
				return;
			}
			case 4:
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.FlatFringeIsland, (context.Tile.Index + 2) % 4, parent);
				return;
			default:
				return;
			}
		}

		// Token: 0x06001FD2 RID: 8146 RVA: 0x0008F8DC File Offset: 0x0008DADC
		private void AddInnerCornerVerticalFringe(Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage)
		{
			Vector3Int vector3Int = Vector3Int.up;
			Vector3Int vector3Int2 = Vector3Int.up;
			int num;
			switch (context.Tile.Index)
			{
			default:
				num = 0;
				vector3Int += Vector3Int.forward;
				vector3Int2 += Vector3Int.right;
				break;
			case 5:
				num = 1;
				vector3Int += Vector3Int.right;
				vector3Int2 += Vector3Int.back;
				break;
			case 6:
				num = 2;
				vector3Int += Vector3Int.back;
				vector3Int2 += Vector3Int.left;
				break;
			case 7:
				num = 3;
				vector3Int += Vector3Int.left;
				vector3Int2 += Vector3Int.forward;
				break;
			}
			Cell cell = stage.GetCellFromCoordinate(coordinate + vector3Int);
			Tileset tileset = stage.GetTilesetFromCell(cell);
			if (cell is TerrainCell && tileset.TilesetType == TilesetType.Base)
			{
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.VerticalFringes[0], (num + 2) % 4, parent);
			}
			cell = stage.GetCellFromCoordinate(coordinate + vector3Int2);
			tileset = stage.GetTilesetFromCell(cell);
			if (cell is TerrainCell && tileset.TilesetType == TilesetType.Base)
			{
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.VerticalFringes[0], (num + 3) % 4, parent);
			}
			Vector3Int[] array = new Vector3Int[]
			{
				Vector3Int.forward,
				Vector3Int.right,
				Vector3Int.back,
				Vector3Int.left
			};
			int num2 = (num + 2) % 4;
			int num3 = (num + 3) % 4;
			Cell cellFromCoordinate = stage.GetCellFromCoordinate(coordinate + array[num2]);
			Tileset tilesetFromCell = stage.GetTilesetFromCell(cellFromCoordinate);
			if (cellFromCoordinate is TerrainCell && tilesetFromCell.TilesetType == TilesetType.Base)
			{
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.VerticalFringes[1], (num2 + 1) % 4, parent);
			}
			Cell cellFromCoordinate2 = stage.GetCellFromCoordinate(coordinate + array[num3]);
			Tileset tilesetFromCell2 = stage.GetTilesetFromCell(cellFromCoordinate2);
			if (cellFromCoordinate2 is TerrainCell && tilesetFromCell2.TilesetType == TilesetType.Base)
			{
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.VerticalFringes[3], (num3 + 3) % 4, parent);
			}
		}

		// Token: 0x0400197F RID: 6527
		private SlopeFringeCosmeticProfile cosmeticSet;
	}
}
