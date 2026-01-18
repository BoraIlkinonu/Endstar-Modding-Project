using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public abstract class PlaneCellCollectionMode : CellCollectionMode
{
	private Vector3Int startingPoint;

	private bool isFinished = true;

	private Vector3 planePosition;

	protected Vector3Int StartingPoint => startingPoint;

	protected Vector3 PlanePosition => planePosition;

	public override void InputDown(Vector3Int point)
	{
		startingPoint = point;
		planePosition = GetPlanePosition();
	}

	public override void InputReleased()
	{
		isFinished = true;
	}

	public override bool IsComplete()
	{
		return isFinished;
	}

	public override void Reset()
	{
		isFinished = false;
	}

	protected List<Vector3Int> CollectPoints(Plane plane, Ray ray, int maximumPoints)
	{
		List<Vector3Int> list = new List<Vector3Int>();
		if (plane.Raycast(ray, out var enter))
		{
			Vector3Int vector3Int = (base.CurrentEndCoordinate = Stage.WorldSpacePointToGridCoordinate(ray.GetPoint(enter) + Vector3.up * 0.1f));
			Vector3Int vector3Int3 = CalculateMinimumCoordinate(StartingPoint, vector3Int);
			Vector3Int vector3Int4 = CalculateAbsoluteDifference(StartingPoint, vector3Int);
			if (vector3Int4.x * vector3Int4.y * vector3Int4.z <= maximumPoints)
			{
				for (int i = 0; i < vector3Int4.x; i++)
				{
					for (int j = 0; j < vector3Int4.y; j++)
					{
						for (int k = 0; k < vector3Int4.z; k++)
						{
							list.Add(new Vector3Int(i + vector3Int3.x, j + vector3Int3.y, k + vector3Int3.z));
						}
					}
				}
			}
			else
			{
				Vector3Int vector3Int5 = vector3Int - StartingPoint;
				if (StartingPoint.x != vector3Int.x)
				{
					vector3Int5.x = ((vector3Int.x >= StartingPoint.x) ? 1 : (-1));
				}
				if (StartingPoint.y != vector3Int.y)
				{
					vector3Int5.y = ((vector3Int.y >= StartingPoint.y) ? 1 : (-1));
				}
				if (StartingPoint.z != vector3Int.z)
				{
					vector3Int5.z = ((vector3Int.z >= StartingPoint.z) ? 1 : (-1));
				}
				Vector3Int vector3Int6 = StartingPoint;
				int num = 0;
				while (vector3Int6 != vector3Int && num < 10000)
				{
					Vector3Int vector3Int7 = vector3Int6;
					num++;
					bool flag = false;
					if (vector3Int6.x != vector3Int.x && vector3Int6.x + vector3Int5.x != vector3Int.x)
					{
						vector3Int7.x += vector3Int5.x;
						flag = true;
					}
					if (vector3Int6.y != vector3Int.y && vector3Int6.y + vector3Int5.y != vector3Int.y)
					{
						vector3Int7.y += vector3Int5.y;
						flag = true;
					}
					if (vector3Int6.z != vector3Int.z && vector3Int6.z + vector3Int5.z != vector3Int.z)
					{
						vector3Int7.z += vector3Int5.z;
						flag = true;
					}
					if (!flag)
					{
						break;
					}
					Vector3Int vector3Int8 = CalculateAbsoluteDifference(StartingPoint, vector3Int7);
					if (vector3Int8.x * vector3Int8.y * vector3Int8.z >= 100)
					{
						break;
					}
					vector3Int6 = vector3Int7;
					if (num >= 10000)
					{
						break;
					}
				}
				vector3Int3 = CalculateMinimumCoordinate(StartingPoint, vector3Int6);
				vector3Int4 = CalculateAbsoluteDifference(StartingPoint, vector3Int6);
				for (int l = 0; l < vector3Int4.x; l++)
				{
					for (int m = 0; m < vector3Int4.y; m++)
					{
						for (int n = 0; n < vector3Int4.z; n++)
						{
							list.Add(new Vector3Int(l + vector3Int3.x, m + vector3Int3.y, n + vector3Int3.z));
						}
					}
				}
			}
		}
		return list;
	}

	protected abstract Vector3 GetPlanePosition();
}
