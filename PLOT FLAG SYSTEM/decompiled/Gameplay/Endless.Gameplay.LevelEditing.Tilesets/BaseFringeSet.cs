using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public class BaseFringeSet : FringeSet
{
	private const int VALID_FRINGE_DIRECTION_COUNT = 4;

	private readonly BaseFringeCosmeticProfile cosmeticSet;

	public BaseFringeSet(BaseFringeCosmeticProfile cosmeticSet)
	{
		this.cosmeticSet = cosmeticSet;
	}

	public override void AddFringe(Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage)
	{
		Cell cellFromCoordinate = stage.GetCellFromCoordinate(coordinate + Vector3Int.up);
		if (cellFromCoordinate != null && cellFromCoordinate.BlocksBaseFringe())
		{
			return;
		}
		List<bool> list = (cosmeticSet.IsVerticalFringe ? GetValidFringeDirections(coordinate + new Vector3Int(0, 1, 0), stage, context) : new List<bool>
		{
			!context.FrontFilled,
			!context.RightFilled,
			!context.BackFilled,
			!context.LeftFilled
		});
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if (list[i])
			{
				num++;
			}
		}
		switch (num)
		{
		case 1:
		{
			int index = list.IndexOf(item: true);
			SpawnFringeWithRotationIndex(cosmeticSet.SingleFringe, index, parent);
			break;
		}
		case 2:
			if (list[0] && list[2])
			{
				SpawnFringeWithRotationIndex(cosmeticSet.SingleFringe, 0, parent);
				SpawnFringeWithRotationIndex(cosmeticSet.SingleFringe, 2, parent);
			}
			else if (list[1] && list[3])
			{
				SpawnFringeWithRotationIndex(cosmeticSet.SingleFringe, 1, parent);
				SpawnFringeWithRotationIndex(cosmeticSet.SingleFringe, 3, parent);
			}
			else if (list[0] && list[1])
			{
				SpawnFringeWithRotationIndex(cosmeticSet.CornerFringe, 1, parent);
			}
			else if (list[1] && list[2])
			{
				SpawnFringeWithRotationIndex(cosmeticSet.CornerFringe, 2, parent);
			}
			else if (list[2] && list[3])
			{
				SpawnFringeWithRotationIndex(cosmeticSet.CornerFringe, 3, parent);
			}
			else if (list[3] && list[0])
			{
				SpawnFringeWithRotationIndex(cosmeticSet.CornerFringe, 0, parent);
			}
			break;
		case 3:
			if (!list[0])
			{
				SpawnFringeWithRotationIndex(cosmeticSet.PeninsulaFringe, 2, parent);
			}
			else if (!list[1])
			{
				SpawnFringeWithRotationIndex(cosmeticSet.PeninsulaFringe, 3, parent);
			}
			else if (!list[2])
			{
				SpawnFringeWithRotationIndex(cosmeticSet.PeninsulaFringe, 0, parent);
			}
			else if (!list[3])
			{
				SpawnFringeWithRotationIndex(cosmeticSet.PeninsulaFringe, 1, parent);
			}
			break;
		case 4:
			SpawnFringeWithRotationIndex(cosmeticSet.IslandFringe, 0, parent);
			break;
		}
		Vector3Int[] array = new Vector3Int[4]
		{
			Vector3Int.forward + Vector3Int.right,
			Vector3Int.right + Vector3Int.back,
			Vector3Int.back + Vector3Int.left,
			Vector3Int.left + Vector3Int.forward
		};
		if (!cosmeticSet.CornerCapFringe)
		{
			return;
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (!list[j] && !list[(j + 1) % list.Count] && CanPlaceCornerCap(coordinate + array[j]))
			{
				SpawnFringeWithRotationIndex(cosmeticSet.CornerCapFringe, j, parent);
			}
		}
		bool CanPlaceCornerCap(Vector3Int targetCellCoordinate)
		{
			Cell cellFromCoordinate2 = stage.GetCellFromCoordinate(targetCellCoordinate);
			if (cellFromCoordinate2 == null || !(cellFromCoordinate2 is TerrainCell))
			{
				return true;
			}
			return MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.GetTileset(cellFromCoordinate2.TilesetIndex)?.Index != context.Tileset.Index;
		}
	}

	private List<bool> GetValidFringeDirections(Vector3Int coordinate, Stage stage, TileSpawnContext contex)
	{
		TerrainCell cellFromCoordinateAs = stage.GetCellFromCoordinateAs<TerrainCell>(coordinate + Vector3Int.forward);
		TerrainCell cellFromCoordinateAs2 = stage.GetCellFromCoordinateAs<TerrainCell>(coordinate + Vector3Int.right);
		TerrainCell cellFromCoordinateAs3 = stage.GetCellFromCoordinateAs<TerrainCell>(coordinate + Vector3Int.back);
		TerrainCell cellFromCoordinateAs4 = stage.GetCellFromCoordinateAs<TerrainCell>(coordinate + Vector3Int.left);
		return new List<bool>
		{
			contex.FrontFilled && cellFromCoordinateAs != null && !(stage.TilesetAtIndex(cellFromCoordinateAs.TilesetIndex) is SlopeTileset),
			contex.RightFilled && cellFromCoordinateAs2 != null && !(stage.TilesetAtIndex(cellFromCoordinateAs2.TilesetIndex) is SlopeTileset),
			contex.BackFilled && cellFromCoordinateAs3 != null && !(stage.TilesetAtIndex(cellFromCoordinateAs3.TilesetIndex) is SlopeTileset),
			contex.LeftFilled && cellFromCoordinateAs4 != null && !(stage.TilesetAtIndex(cellFromCoordinateAs4.TilesetIndex) is SlopeTileset)
		};
	}
}
