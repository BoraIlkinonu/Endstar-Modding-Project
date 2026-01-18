using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class PlacementManager : EndlessBehaviourSingleton<PlacementManager>
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PlacementComparer : IComparer<Vector3Int>
	{
		public int Compare(Vector3Int a, Vector3Int b)
		{
			int num = math.abs(a.y).CompareTo(math.abs(b.y));
			if (num != 0)
			{
				return num;
			}
			return a.magnitude.CompareTo(b.magnitude);
		}
	}

	[SerializeField]
	private LayerMask overlapMask;

	private const float MAX_SPAWN_DISTANCE = 3f;

	private List<Vector3Int> cellOffsets;

	private List<Vector3Int> claimedCells;

	protected override void Awake()
	{
		base.Awake();
		cellOffsets = GetNearbyCellOffsets(3f);
		claimedCells = new List<Vector3Int>();
		cellOffsets.Remove(Vector3Int.zero);
	}

	private static List<Vector3Int> GetNearbyCellOffsets(float radius)
	{
		List<Vector3Int> list = new List<Vector3Int>();
		int num = Mathf.CeilToInt(radius);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				for (int k = -num; k <= num; k++)
				{
					Vector3Int vector3Int = new Vector3Int(i, j, k);
					if (vector3Int != Vector3Int.zero && vector3Int.magnitude <= radius)
					{
						list.Add(vector3Int);
					}
				}
			}
		}
		list.Sort(default(PlacementComparer));
		return list;
	}

	public static Vector3? GetClosestOpenPosition(Vector3Int origin, bool claimPosition = false, uint claimFrames = 0u)
	{
		if (IsCellClear(origin) && !MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Contains(origin))
		{
			if (claimPosition)
			{
				MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Add(origin);
				MonoBehaviourSingleton<PlacementManager>.Instance.ReleaseClaimedCell(origin, NetClock.FixedDeltaTime * (float)claimFrames);
			}
			NavMeshHit hit;
			return NavMesh.SamplePosition(origin, out hit, 1f, -1) ? (hit.position + Vector3.up * 0.5f) : ((Vector3)origin);
		}
		foreach (Vector3Int cellOffset in MonoBehaviourSingleton<PlacementManager>.Instance.cellOffsets)
		{
			Vector3Int vector3Int = origin + cellOffset;
			if (!MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Contains(vector3Int) && IsCellClear(vector3Int) && IsCellNavigable(vector3Int) && !CrossesWall(origin, vector3Int))
			{
				if (claimPosition)
				{
					MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Add(vector3Int);
					MonoBehaviourSingleton<PlacementManager>.Instance.ReleaseClaimedCell(vector3Int, NetClock.FixedDeltaTime * (float)claimFrames);
				}
				NavMeshHit hit2;
				return NavMesh.SamplePosition(vector3Int, out hit2, 1f, -1) ? (hit2.position + Vector3.up * 0.5f) : ((Vector3)vector3Int);
			}
		}
		return null;
	}

	public static Vector3? GetClosestOpenPosition(Vector3 point, bool claimPosition = false, uint claimFrames = 0u)
	{
		Vector3Int vector3Int = Stage.WorldSpacePointToGridCoordinate(point);
		if (IsPositionClear(point) && !MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Contains(vector3Int))
		{
			if (!claimPosition)
			{
				MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Add(vector3Int);
				MonoBehaviourSingleton<PlacementManager>.Instance.ReleaseClaimedCell(vector3Int, NetClock.FixedDeltaTime * (float)claimFrames);
			}
			NavMeshHit hit;
			return NavMesh.SamplePosition(point, out hit, 1f, -1) ? (hit.position + Vector3.up * 0.5f) : point;
		}
		foreach (Vector3Int cellOffset in MonoBehaviourSingleton<PlacementManager>.Instance.cellOffsets)
		{
			Vector3Int vector3Int2 = vector3Int + cellOffset;
			if (!MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Contains(vector3Int2) && IsCellClear(vector3Int2) && IsCellNavigable(vector3Int2) && !CrossesWall(vector3Int, vector3Int2))
			{
				if (claimPosition)
				{
					MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Add(vector3Int2);
					MonoBehaviourSingleton<PlacementManager>.Instance.ReleaseClaimedCell(vector3Int2, NetClock.FixedDeltaTime * (float)claimFrames);
				}
				NavMeshHit hit2;
				return NavMesh.SamplePosition(vector3Int2, out hit2, 1f, -1) ? (hit2.position + Vector3.up * 0.5f) : ((Vector3)vector3Int2);
			}
		}
		return null;
	}

	private void ReleaseClaimedCell(Vector3Int claimedCell, float releaseTime)
	{
		StartCoroutine(ReleaseCellRoutine(claimedCell, releaseTime));
	}

	private IEnumerator ReleaseCellRoutine(Vector3Int claimedCell, float releaseTime)
	{
		yield return new WaitForSeconds(releaseTime);
		claimedCells.Remove(claimedCell);
	}

	private static bool IsCellClear(Vector3Int cell)
	{
		return !Physics.CheckCapsule(cell, cell + Vector3.up, 0.25f, MonoBehaviourSingleton<PlacementManager>.Instance.overlapMask);
	}

	private static bool IsPositionClear(Vector3 position)
	{
		return !Physics.CheckCapsule(position, position + Vector3.up, 0.25f, MonoBehaviourSingleton<PlacementManager>.Instance.overlapMask);
	}

	private static bool IsCellNavigable(Vector3Int cell)
	{
		return MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(cell);
	}

	private static bool CrossesWall(Vector3Int origin, Vector3Int targetPosition)
	{
		Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(origin);
		SerializableGuid ignoredInstanceId = default(SerializableGuid);
		if (cellFromCoordinate is PropCell propCell)
		{
			ignoredInstanceId = propCell.InstanceId;
		}
		Vector3 vector = targetPosition - origin;
		Vector3 normalized = vector.normalized;
		float magnitude = vector.magnitude;
		Ray ray = new Ray(origin, normalized);
		LineCastHit lineCastHit = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.Linecast(ray, magnitude, 10f, ignoredInstanceId);
		if (!lineCastHit.IntersectionOccured)
		{
			return false;
		}
		Cell cellFromCoordinate2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(lineCastHit.IntersectedObjectPosition);
		return MonoBehaviourSingleton<Pathfinding>.Instance.CheckBlockingAtPoint(cellFromCoordinate2.Coordinate);
	}
}
