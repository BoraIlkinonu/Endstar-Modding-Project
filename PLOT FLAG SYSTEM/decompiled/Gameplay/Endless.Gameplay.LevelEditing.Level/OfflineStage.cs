using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

public class OfflineStage : MonoBehaviour
{
	private readonly Dictionary<Vector3Int, Cell> cellLookup = new Dictionary<Vector3Int, Cell>();

	[field: SerializeField]
	public Transform TileRoot { get; private set; }

	[field: SerializeField]
	public ChunkManager ChunkManager { get; private set; }

	public bool TryGetCellFromCoordinate(Vector3Int coordinate, out Cell result)
	{
		return cellLookup.TryGetValue(coordinate, out result);
	}

	public void AddCell(Vector3Int coordinate, Cell cell)
	{
		cellLookup.Add(coordinate, cell);
	}

	public bool RemoveCell(Vector3Int coordinate)
	{
		return cellLookup.Remove(coordinate);
	}

	public IEnumerable<Cell> GetCells()
	{
		return cellLookup.Values;
	}

	public IEnumerable<Vector3Int> GetCellCoordinates()
	{
		return cellLookup.Keys;
	}
}
