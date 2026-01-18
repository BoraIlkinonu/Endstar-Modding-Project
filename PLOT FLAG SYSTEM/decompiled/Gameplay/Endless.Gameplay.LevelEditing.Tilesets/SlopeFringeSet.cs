using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public class SlopeFringeSet : FringeSet
{
	private SlopeFringeCosmeticProfile cosmeticSet;

	public SlopeFringeSet(SlopeFringeCosmeticProfile slopeFringeCosmeticSet)
	{
		cosmeticSet = slopeFringeCosmeticSet;
	}

	public override void AddFringe(Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage)
	{
		if (stage.GetCellFromCoordinate(coordinate + Vector3Int.up) is TerrainCell)
		{
			return;
		}
		bool[] array = CalculateValidFringeDirections(context, coordinate, stage);
		int num = array.Where((bool valid) => valid).Count();
		bool[] array2 = array.Select((bool valid) => !valid).ToArray();
		int num2 = array2.Where((bool valid) => valid).Count();
		if (num > 0)
		{
			switch (context.Tile.Index)
			{
			case 0:
			case 1:
			case 2:
			case 3:
				AddFlatSlopeFringe(array, num, parent, coordinate, context, stage);
				break;
			case 4:
			case 5:
			case 6:
			case 7:
				AddInnerCornerSlopeFringe(cosmeticSet.InnerCornerFringe, cosmeticSet.InnerCornerFringePeninsulas, array, num, parent, context);
				break;
			case 8:
			case 9:
			case 10:
			case 11:
				AddOuterCornerSlopeFringe(cosmeticSet.OuterCornerFringe, cosmeticSet.OuterCornerFringePeninsulas, array, num, parent, context);
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
				AddFlatVerticalFringe(array2, parent, coordinate, context, stage);
				break;
			case 4:
			case 5:
			case 6:
			case 7:
				AddInnerCornerVerticalFringe(parent, coordinate, context, stage);
				break;
			case 8:
			case 9:
			case 10:
			case 11:
				break;
			}
		}
	}

	private bool[] CalculateValidFringeDirections(TileSpawnContext context, Vector3Int coordinate, Stage stage)
	{
		bool[] array = new bool[4]
		{
			!context.FrontFilled,
			!context.RightFilled,
			!context.BackFilled,
			!context.LeftFilled
		};
		switch (context.Tile.Index)
		{
		case 0:
		case 1:
		case 2:
		case 3:
			array = CalculateValidFringeForSingleSlope(array, stage, context, coordinate);
			break;
		case 8:
		case 9:
		case 10:
		case 11:
			array = CalculateValidFringeForOuterCorner(array, stage, context, coordinate);
			break;
		}
		return array;
	}

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
		case 2:
			num = 0;
			vector3Int = Vector3Int.down + Vector3Int.forward;
			break;
		case 1:
			num = 3;
			vector3Int = Vector3Int.down + Vector3Int.left;
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

	private void AddFlatSlopeFringe(bool[] validFringeDirections, int validDirectionCount, Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage)
	{
		switch (validDirectionCount)
		{
		case 1:
		{
			for (int j = 0; j < validFringeDirections.Length; j++)
			{
				if (validFringeDirections[(j + context.Tile.Index) % 4])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[j], (context.Tile.Index + 2) % 4, parent);
					break;
				}
			}
			break;
		}
		case 2:
		{
			if ((validFringeDirections[0] && validFringeDirections[1]) || (validFringeDirections[0] && validFringeDirections[1]))
			{
				for (int k = 0; k < validFringeDirections.Length; k++)
				{
					if (validFringeDirections[(k + context.Tile.Index) % 4] && validFringeDirections[(k + context.Tile.Index + 1) % 4])
					{
						SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeCorners[k], (context.Tile.Index + 2) % 4, parent);
						break;
					}
				}
				break;
			}
			for (int l = 0; l < validFringeDirections.Length; l++)
			{
				if (validFringeDirections[l])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[(l + context.Tile.Index) % 4], (context.Tile.Index + 2) % 4, parent);
				}
			}
			break;
		}
		case 3:
		{
			for (int i = 0; i < validFringeDirections.Length; i++)
			{
				if (validFringeDirections[(i + context.Tile.Index) % 4] && validFringeDirections[(i + context.Tile.Index + 1) % 4])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringePeninsulas[i], (context.Tile.Index + 2) % 4, parent);
					break;
				}
			}
			break;
		}
		case 4:
			SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeIsland, (context.Tile.Index + 2) % 4, parent);
			break;
		}
	}

	private void AddFlatVerticalFringe(bool[] validVerticalFringeDirections, Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage)
	{
		int num = 0;
		Vector3Int up = Vector3Int.up;
		Vector3Int[] array = new Vector3Int[4]
		{
			Vector3Int.forward,
			Vector3Int.right,
			Vector3Int.back,
			Vector3Int.left
		};
		switch (context.Tile.Index)
		{
		default:
			num = 0;
			up += Vector3Int.forward;
			break;
		case 1:
			num = 1;
			up += Vector3Int.right;
			break;
		case 2:
			num = 2;
			up += Vector3Int.back;
			break;
		case 3:
			num = 3;
			up += Vector3Int.left;
			break;
		}
		int num2 = (num + 1) % 4;
		_ = (num + 2) % 4;
		int num3 = (num + 3) % 4;
		int index = (num + 2) % 4;
		Cell cellFromCoordinate = stage.GetCellFromCoordinate(coordinate + up);
		Tileset tilesetFromCell = stage.GetTilesetFromCell(cellFromCoordinate);
		if (cellFromCoordinate is TerrainCell && tilesetFromCell.TilesetType == TilesetType.Base)
		{
			SpawnFringeWithRotationIndex(cosmeticSet.VerticalFringes[0], index, parent);
		}
		Cell cellFromCoordinate2 = stage.GetCellFromCoordinate(coordinate + array[num2]);
		Tileset tilesetFromCell2 = stage.GetTilesetFromCell(cellFromCoordinate2);
		if (cellFromCoordinate2 is TerrainCell && tilesetFromCell2.TilesetType == TilesetType.Base)
		{
			SpawnFringeWithRotationIndex(cosmeticSet.VerticalFringes[1], index, parent);
		}
		Cell cellFromCoordinate3 = stage.GetCellFromCoordinate(coordinate + array[num3]);
		Tileset tilesetFromCell3 = stage.GetTilesetFromCell(cellFromCoordinate3);
		if (cellFromCoordinate3 is TerrainCell && tilesetFromCell3.TilesetType == TilesetType.Base)
		{
			SpawnFringeWithRotationIndex(cosmeticSet.VerticalFringes[3], index, parent);
		}
	}

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
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[3], 3, parent);
				}
				else if (validFringeDirections[1])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[1], 2, parent);
				}
				else if (validFringeDirections[2])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[2], 2, parent);
				}
				else if (validFringeDirections[3])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[2], 3, parent);
				}
				break;
			case 9:
				if (validFringeDirections[2])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[1], 3, parent);
				}
				else if (validFringeDirections[1])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[3], 0, parent);
				}
				else if (validFringeDirections[0])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[2], 0, parent);
				}
				else if (validFringeDirections[3])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[2], 3, parent);
				}
				break;
			case 10:
				if (validFringeDirections[2])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[3], 1, parent);
				}
				else if (validFringeDirections[3])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[1], 0, parent);
				}
				else if (validFringeDirections[0])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[2], 0, parent);
				}
				else if (validFringeDirections[1])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[2], 1, parent);
				}
				break;
			case 11:
				if (validFringeDirections[0])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[1], 1, parent);
				}
				else if (validFringeDirections[3])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[3], 2, parent);
				}
				else if (validFringeDirections[2])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[2], 2, parent);
				}
				else if (validFringeDirections[1])
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[2], 1, parent);
				}
				break;
			}
			break;
		case 2:
			if ((validFringeDirections[0] && validFringeDirections[2]) || (validFringeDirections[1] && validFringeDirections[3]))
			{
				if (validFringeDirections[0] && validFringeDirections[2])
				{
					if (context.Tile.Index % 4 == 0 || context.Tile.Index % 4 == 3)
					{
						SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[(context.Tile.Index + 2) % 4], (context.Tile.Index + 2) % 4, parent);
						SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[(context.Tile.Index + 3) % 4], (context.Tile.Index + 3) % 4, parent);
					}
					else
					{
						SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[context.Tile.Index % 4], (context.Tile.Index + 2) % 4, parent);
						SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[(context.Tile.Index + 1) % 4], (context.Tile.Index + 3) % 4, parent);
					}
				}
				else if (context.Tile.Index % 4 == 0 || context.Tile.Index % 4 == 1)
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[(context.Tile.Index + 1) % 4], (context.Tile.Index + 2) % 4, parent);
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[(context.Tile.Index + 2) % 4], (context.Tile.Index + 3) % 4, parent);
				}
				else
				{
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[context.Tile.Index % 4], (context.Tile.Index + 3) % 4, parent);
					SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeSingles[(context.Tile.Index + 3) % 4], (context.Tile.Index + 2) % 4, parent);
				}
			}
			else
			{
				SpawnFringeWithRotationIndex(cornerFringe, (context.Tile.Index + 2) % 4, parent);
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
						SpawnFringeWithRotationIndex(peninsulaFringes[i], (context.Tile.Index + 2) % 4, parent);
						break;
					}
				}
				else if (!validFringeDirections[(i + context.Tile.Index + 3) % 4])
				{
					SpawnFringeWithRotationIndex(peninsulaFringes[i], (context.Tile.Index + 2) % 4, parent);
					break;
				}
			}
			break;
		}
		case 4:
			SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeIsland, (context.Tile.Index + 2) % 4, parent);
			break;
		}
	}

	private void AddInnerCornerSlopeFringe(GameObject cornerFringe, GameObject[] peninsulaFringes, bool[] validFringeDirections, int validDirectionCount, Transform parent, TileSpawnContext context)
	{
		switch (validDirectionCount)
		{
		case 2:
			SpawnFringeWithRotationIndex(cornerFringe, (context.Tile.Index + 2) % 4, parent);
			break;
		case 3:
		{
			for (int i = 0; i < validFringeDirections.Length; i++)
			{
				if (!validFringeDirections[i])
				{
					SpawnFringeWithRotationIndex(peninsulaFringes[(i + 2) % 4], (context.Tile.Index + 2) % 4, parent);
					break;
				}
			}
			break;
		}
		case 4:
			SpawnFringeWithRotationIndex(cosmeticSet.FlatFringeIsland, (context.Tile.Index + 2) % 4, parent);
			break;
		}
	}

	private void AddInnerCornerVerticalFringe(Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage)
	{
		int num = 0;
		Vector3Int up = Vector3Int.up;
		Vector3Int up2 = Vector3Int.up;
		switch (context.Tile.Index)
		{
		default:
			num = 0;
			up += Vector3Int.forward;
			up2 += Vector3Int.right;
			break;
		case 5:
			num = 1;
			up += Vector3Int.right;
			up2 += Vector3Int.back;
			break;
		case 6:
			num = 2;
			up += Vector3Int.back;
			up2 += Vector3Int.left;
			break;
		case 7:
			num = 3;
			up += Vector3Int.left;
			up2 += Vector3Int.forward;
			break;
		}
		Cell cellFromCoordinate = stage.GetCellFromCoordinate(coordinate + up);
		Tileset tilesetFromCell = stage.GetTilesetFromCell(cellFromCoordinate);
		if (cellFromCoordinate is TerrainCell && tilesetFromCell.TilesetType == TilesetType.Base)
		{
			SpawnFringeWithRotationIndex(cosmeticSet.VerticalFringes[0], (num + 2) % 4, parent);
		}
		cellFromCoordinate = stage.GetCellFromCoordinate(coordinate + up2);
		tilesetFromCell = stage.GetTilesetFromCell(cellFromCoordinate);
		if (cellFromCoordinate is TerrainCell && tilesetFromCell.TilesetType == TilesetType.Base)
		{
			SpawnFringeWithRotationIndex(cosmeticSet.VerticalFringes[0], (num + 3) % 4, parent);
		}
		Vector3Int[] array = new Vector3Int[4]
		{
			Vector3Int.forward,
			Vector3Int.right,
			Vector3Int.back,
			Vector3Int.left
		};
		int num2 = (num + 2) % 4;
		int num3 = (num + 3) % 4;
		Cell cellFromCoordinate2 = stage.GetCellFromCoordinate(coordinate + array[num2]);
		Tileset tilesetFromCell2 = stage.GetTilesetFromCell(cellFromCoordinate2);
		if (cellFromCoordinate2 is TerrainCell && tilesetFromCell2.TilesetType == TilesetType.Base)
		{
			SpawnFringeWithRotationIndex(cosmeticSet.VerticalFringes[1], (num2 + 1) % 4, parent);
		}
		Cell cellFromCoordinate3 = stage.GetCellFromCoordinate(coordinate + array[num3]);
		Tileset tilesetFromCell3 = stage.GetTilesetFromCell(cellFromCoordinate3);
		if (cellFromCoordinate3 is TerrainCell && tilesetFromCell3.TilesetType == TilesetType.Base)
		{
			SpawnFringeWithRotationIndex(cosmeticSet.VerticalFringes[3], (num3 + 3) % 4, parent);
		}
	}
}
