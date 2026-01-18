using System.Collections.Generic;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public abstract class CellCollectionMode
{
	public Vector3Int CurrentEndCoordinate { get; protected set; }

	public abstract string DisplayName { get; }

	public abstract void InputDown(Vector3Int point);

	public abstract bool IsComplete();

	public abstract void InputReleased();

	public abstract void Reset();

	public abstract List<Vector3Int> CollectPaintedPositions(Ray ray, int maximumPositions);

	protected Vector3Int CalculateMinimumCoordinate(Vector3Int initial, Vector3Int end)
	{
		Vector3Int zero = Vector3Int.zero;
		zero.x = Mathf.Min(initial.x, end.x);
		zero.y = Mathf.Min(initial.y, end.y);
		zero.z = Mathf.Min(initial.z, end.z);
		return zero;
	}

	protected Vector3Int CalculateAbsoluteDifference(Vector3Int initial, Vector3Int end)
	{
		Vector3Int zero = Vector3Int.zero;
		zero.x = Mathf.Abs(initial.x - end.x) + 1;
		zero.y = Mathf.Abs(initial.y - end.y) + 1;
		zero.z = Mathf.Abs(initial.z - end.z) + 1;
		return zero;
	}
}
