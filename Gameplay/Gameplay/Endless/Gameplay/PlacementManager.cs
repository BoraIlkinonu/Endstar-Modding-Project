using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000236 RID: 566
	public class PlacementManager : EndlessBehaviourSingleton<PlacementManager>
	{
		// Token: 0x06000BB3 RID: 2995 RVA: 0x00040461 File Offset: 0x0003E661
		protected override void Awake()
		{
			base.Awake();
			this.cellOffsets = PlacementManager.GetNearbyCellOffsets(3f);
			this.claimedCells = new List<Vector3Int>();
			this.cellOffsets.Remove(Vector3Int.zero);
		}

		// Token: 0x06000BB4 RID: 2996 RVA: 0x00040498 File Offset: 0x0003E698
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
			list.Sort(default(PlacementManager.PlacementComparer));
			return list;
		}

		// Token: 0x06000BB5 RID: 2997 RVA: 0x00040520 File Offset: 0x0003E720
		public static Vector3? GetClosestOpenPosition(Vector3Int origin, bool claimPosition = false, uint claimFrames = 0U)
		{
			if (PlacementManager.IsCellClear(origin) && !MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Contains(origin))
			{
				if (claimPosition)
				{
					MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Add(origin);
					MonoBehaviourSingleton<PlacementManager>.Instance.ReleaseClaimedCell(origin, NetClock.FixedDeltaTime * claimFrames);
				}
				NavMeshHit navMeshHit;
				return new Vector3?(NavMesh.SamplePosition(origin, out navMeshHit, 1f, -1) ? (navMeshHit.position + Vector3.up * 0.5f) : origin);
			}
			foreach (Vector3Int vector3Int in MonoBehaviourSingleton<PlacementManager>.Instance.cellOffsets)
			{
				Vector3Int vector3Int2 = origin + vector3Int;
				if (!MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Contains(vector3Int2) && PlacementManager.IsCellClear(vector3Int2) && PlacementManager.IsCellNavigable(vector3Int2) && !PlacementManager.CrossesWall(origin, vector3Int2))
				{
					if (claimPosition)
					{
						MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Add(vector3Int2);
						MonoBehaviourSingleton<PlacementManager>.Instance.ReleaseClaimedCell(vector3Int2, NetClock.FixedDeltaTime * claimFrames);
					}
					NavMeshHit navMeshHit2;
					return new Vector3?(NavMesh.SamplePosition(vector3Int2, out navMeshHit2, 1f, -1) ? (navMeshHit2.position + Vector3.up * 0.5f) : vector3Int2);
				}
			}
			return null;
		}

		// Token: 0x06000BB6 RID: 2998 RVA: 0x000406A0 File Offset: 0x0003E8A0
		public static Vector3? GetClosestOpenPosition(Vector3 point, bool claimPosition = false, uint claimFrames = 0U)
		{
			Vector3Int vector3Int = Stage.WorldSpacePointToGridCoordinate(point);
			if (PlacementManager.IsPositionClear(point) && !MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Contains(vector3Int))
			{
				if (!claimPosition)
				{
					MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Add(vector3Int);
					MonoBehaviourSingleton<PlacementManager>.Instance.ReleaseClaimedCell(vector3Int, NetClock.FixedDeltaTime * claimFrames);
				}
				NavMeshHit navMeshHit;
				return new Vector3?(NavMesh.SamplePosition(point, out navMeshHit, 1f, -1) ? (navMeshHit.position + Vector3.up * 0.5f) : point);
			}
			foreach (Vector3Int vector3Int2 in MonoBehaviourSingleton<PlacementManager>.Instance.cellOffsets)
			{
				Vector3Int vector3Int3 = vector3Int + vector3Int2;
				if (!MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Contains(vector3Int3) && PlacementManager.IsCellClear(vector3Int3) && PlacementManager.IsCellNavigable(vector3Int3) && !PlacementManager.CrossesWall(vector3Int, vector3Int3))
				{
					if (claimPosition)
					{
						MonoBehaviourSingleton<PlacementManager>.Instance.claimedCells.Add(vector3Int3);
						MonoBehaviourSingleton<PlacementManager>.Instance.ReleaseClaimedCell(vector3Int3, NetClock.FixedDeltaTime * claimFrames);
					}
					NavMeshHit navMeshHit2;
					return new Vector3?(NavMesh.SamplePosition(vector3Int3, out navMeshHit2, 1f, -1) ? (navMeshHit2.position + Vector3.up * 0.5f) : vector3Int3);
				}
			}
			return null;
		}

		// Token: 0x06000BB7 RID: 2999 RVA: 0x00040828 File Offset: 0x0003EA28
		private void ReleaseClaimedCell(Vector3Int claimedCell, float releaseTime)
		{
			base.StartCoroutine(this.ReleaseCellRoutine(claimedCell, releaseTime));
		}

		// Token: 0x06000BB8 RID: 3000 RVA: 0x00040839 File Offset: 0x0003EA39
		private IEnumerator ReleaseCellRoutine(Vector3Int claimedCell, float releaseTime)
		{
			yield return new WaitForSeconds(releaseTime);
			this.claimedCells.Remove(claimedCell);
			yield break;
		}

		// Token: 0x06000BB9 RID: 3001 RVA: 0x00040856 File Offset: 0x0003EA56
		private static bool IsCellClear(Vector3Int cell)
		{
			return !Physics.CheckCapsule(cell, cell + Vector3.up, 0.25f, MonoBehaviourSingleton<PlacementManager>.Instance.overlapMask);
		}

		// Token: 0x06000BBA RID: 3002 RVA: 0x0004088A File Offset: 0x0003EA8A
		private static bool IsPositionClear(Vector3 position)
		{
			return !Physics.CheckCapsule(position, position + Vector3.up, 0.25f, MonoBehaviourSingleton<PlacementManager>.Instance.overlapMask);
		}

		// Token: 0x06000BBB RID: 3003 RVA: 0x000408B4 File Offset: 0x0003EAB4
		private static bool IsCellNavigable(Vector3Int cell)
		{
			return MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(cell);
		}

		// Token: 0x06000BBC RID: 3004 RVA: 0x000408C8 File Offset: 0x0003EAC8
		private static bool CrossesWall(Vector3Int origin, Vector3Int targetPosition)
		{
			Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(origin);
			SerializableGuid serializableGuid = default(SerializableGuid);
			PropCell propCell = cellFromCoordinate as PropCell;
			if (propCell != null)
			{
				serializableGuid = propCell.InstanceId;
			}
			Vector3 vector = targetPosition - origin;
			Vector3 normalized = vector.normalized;
			float magnitude = vector.magnitude;
			Ray ray = new Ray(origin, normalized);
			LineCastHit lineCastHit = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.Linecast(ray, magnitude, 10f, serializableGuid);
			if (!lineCastHit.IntersectionOccured)
			{
				return false;
			}
			Cell cellFromCoordinate2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(lineCastHit.IntersectedObjectPosition);
			return MonoBehaviourSingleton<Pathfinding>.Instance.CheckBlockingAtPoint(cellFromCoordinate2.Coordinate);
		}

		// Token: 0x04000AF3 RID: 2803
		[SerializeField]
		private LayerMask overlapMask;

		// Token: 0x04000AF4 RID: 2804
		private const float MAX_SPAWN_DISTANCE = 3f;

		// Token: 0x04000AF5 RID: 2805
		private List<Vector3Int> cellOffsets;

		// Token: 0x04000AF6 RID: 2806
		private List<Vector3Int> claimedCells;

		// Token: 0x02000237 RID: 567
		public struct PlacementComparer : IComparer<Vector3Int>
		{
			// Token: 0x06000BBE RID: 3006 RVA: 0x00040988 File Offset: 0x0003EB88
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
	}
}
