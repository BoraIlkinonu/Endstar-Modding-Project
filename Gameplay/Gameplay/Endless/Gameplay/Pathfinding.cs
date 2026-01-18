using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000209 RID: 521
	public class Pathfinding : EndlessBehaviourSingleton<Pathfinding>, IGameEndSubscriber
	{
		// Token: 0x14000014 RID: 20
		// (add) Token: 0x06000ACE RID: 2766 RVA: 0x0003AAC8 File Offset: 0x00038CC8
		// (remove) Token: 0x06000ACF RID: 2767 RVA: 0x0003AB00 File Offset: 0x00038D00
		public event Action<HashSet<SerializableGuid>> OnPathfindingUpdated;

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x06000AD0 RID: 2768 RVA: 0x0003AB35 File Offset: 0x00038D35
		// (set) Token: 0x06000AD1 RID: 2769 RVA: 0x0003AB3D File Offset: 0x00038D3D
		public int NumSections { get; private set; }

		// Token: 0x06000AD2 RID: 2770 RVA: 0x0003AB46 File Offset: 0x00038D46
		protected override void Start()
		{
			base.Start();
			UnifiedStateUpdater.OnProcessRequests += this.ProcessRequests;
		}

		// Token: 0x06000AD3 RID: 2771 RVA: 0x0003AB60 File Offset: 0x00038D60
		public void InitializePathfindingCollections(NativeParallelMultiHashMap<int, Edge> edges, NativeParallelHashMap<int, GraphNode> nodes, NativeParallelHashMap<int, SectionSurface> surfaces, NativeParallelMultiHashMap<int, int> sections, NativeParallelMultiHashMap<int, int> areaConnectionGraph, NativeParallelHashMap<int, WalkableOctantData> octantData, NativeArray<Octant> octantArray, NativeParallelHashMap<int, SerializableGuid> propMap)
		{
			this.hasReceivedCollections = true;
			this.edgeMap = edges;
			this.nodeMap = nodes;
			this.surfaceMap = surfaces;
			this.sectionMap = sections;
			this.NumSections = this.surfaceMap.Count();
			this.areaGraph = areaConnectionGraph;
			this.walkableOctantDataMap = octantData;
			this.octants = octantArray;
			this.associatedPropMap = propMap;
		}

		// Token: 0x06000AD4 RID: 2772 RVA: 0x0003ABC4 File Offset: 0x00038DC4
		public void UpdatePathfindingCollections(HashSet<SerializableGuid> updatedProps, NativeParallelMultiHashMap<int, Edge> edges, NativeParallelHashMap<int, GraphNode> nodes, NativeParallelHashMap<int, SectionSurface> surfaces, NativeParallelMultiHashMap<int, int> sections, NativeParallelMultiHashMap<int, int> areaConnectionGraph, NativeParallelHashMap<int, WalkableOctantData> octantData, NativeArray<Octant> octantArray)
		{
			this.updateCollections = new Pathfinding.UpdateCollections
			{
				UpdatedProps = updatedProps,
				EdgeMap = edges,
				NodeMap = nodes,
				SurfaceMap = surfaces,
				SectionMap = sections,
				AreaGraph = areaConnectionGraph,
				WalkableOctantDataMap = octantData,
				Octants = octantArray
			};
			if (this.currentPathfindingJobs == 0)
			{
				this.ApplyCollectionsUpdate();
			}
		}

		// Token: 0x06000AD5 RID: 2773 RVA: 0x0003AC27 File Offset: 0x00038E27
		private bool CanReachPosition(Vector3 startPosition, Vector3 endPosition)
		{
			return this.IsPositionNavigable(startPosition) && this.IsPositionNavigable(endPosition) && Pathfinding.CanReachPositionSynchronous(startPosition, endPosition, this.octants, this.walkableOctantDataMap, this.areaGraph);
		}

		// Token: 0x06000AD6 RID: 2774 RVA: 0x0003AC58 File Offset: 0x00038E58
		public bool IsValidDestination(Vector3 origin, Vector3 destination, PathfindingRange range, bool canDoubleJump)
		{
			bool flag;
			switch (range)
			{
			case PathfindingRange.Island:
				flag = MonoBehaviourSingleton<Pathfinding>.Instance.AreInSameArea(origin, destination);
				break;
			case PathfindingRange.Area:
				if (canDoubleJump)
				{
					flag = MonoBehaviourSingleton<Pathfinding>.Instance.AreInSameZone(origin, destination);
				}
				else
				{
					flag = MonoBehaviourSingleton<Pathfinding>.Instance.AreInSameArea(origin, destination);
				}
				break;
			case PathfindingRange.Global:
				flag = Pathfinding.CanReachGlobalLocation(origin, destination, canDoubleJump);
				break;
			default:
				throw new ArgumentOutOfRangeException("range", range, null);
			}
			return flag;
		}

		// Token: 0x06000AD7 RID: 2775 RVA: 0x0003ACF3 File Offset: 0x00038EF3
		private static bool CanReachGlobalLocation(float3 startPoint, float3 endPoint, bool canDoubleJump)
		{
			return MonoBehaviourSingleton<Pathfinding>.Instance.AreInSameArea(startPoint, endPoint) || (canDoubleJump && MonoBehaviourSingleton<Pathfinding>.Instance.AreInSameZone(startPoint, endPoint)) || MonoBehaviourSingleton<Pathfinding>.Instance.CanReachPosition(startPoint, endPoint);
		}

		// Token: 0x06000AD8 RID: 2776 RVA: 0x0003AD30 File Offset: 0x00038F30
		public void RequestPath(Pathfinding.Request request)
		{
			this.pathfindingRequests[request.Requester] = request;
			if (!this.IsValidDestination(request.StartPosition, request.EndPosition, request.PathfindingRange, false))
			{
				request.NavigationPathCallback(new Pathfinding.Response(NpcEnum.PathfindingResult.Failure, null));
				this.pathfindingRequests.Remove(request.Requester);
				return;
			}
			if (!this.AreOnSameIsland(request.StartPosition, request.EndPosition))
			{
				return;
			}
			NavMeshPath navMeshPath = new NavMeshPath();
			NavMesh.CalculatePath(request.StartPosition, request.EndPosition, NpcEntity.NavFilter, navMeshPath);
			if (navMeshPath.status != NavMeshPathStatus.PathComplete)
			{
				return;
			}
			float pathDistance = Pathfinding.GetPathDistance(navMeshPath.corners);
			if (pathDistance < this.onMeshShortcutDistance)
			{
				NavPath navPath = new NavPath(request.EndPosition, new Queue<NavPath.Segment>());
				NavPath.Segment segment = new NavPath.Segment
				{
					StartPosition = request.StartPosition,
					EndPosition = request.EndPosition
				};
				navPath.NavigationSegments.Enqueue(segment);
				request.NavigationPathCallback(new Pathfinding.Response(NpcEnum.PathfindingResult.Success, navPath));
				this.pathfindingRequests.Remove(request.Requester);
			}
			request.OnMeshPathDistance = pathDistance;
		}

		// Token: 0x06000AD9 RID: 2777 RVA: 0x0003AE58 File Offset: 0x00039058
		public Door GetDoorFromThresholdSection(int sectionKey)
		{
			int num;
			NativeParallelMultiHashMapIterator<int> nativeParallelMultiHashMapIterator;
			this.sectionMap.TryGetFirstValue(sectionKey, out num, out nativeParallelMultiHashMapIterator);
			Door door;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(MonoBehaviourSingleton<Pathfinding>.Instance.associatedPropMap[num]).GetComponent<WorldObject>().TryGetUserComponent<Door>(out door);
			return door;
		}

		// Token: 0x06000ADA RID: 2778 RVA: 0x0003AEA4 File Offset: 0x000390A4
		private void ProcessRequests()
		{
			if (this.pathfindingRequests.Count == 0 || !this.hasReceivedCollections || this.updateCollections != null)
			{
				return;
			}
			this.requestsToProcess.Clear();
			this.requestsToProcess.AddRange(this.pathfindingRequests.Values);
			this.currentPathfindingJobs++;
			base.StartCoroutine(PathfindingGenerator.GeneratePathfindingResults(new List<Pathfinding.Request>(this.requestsToProcess), this.nodeMap, this.edgeMap, this.surfaceMap, this.octants, this.NumSections, new Action<PathfindingGenerator.Results>(this.ProcessResults)));
			this.pathfindingRequests.Clear();
		}

		// Token: 0x06000ADB RID: 2779 RVA: 0x0003AF4C File Offset: 0x0003914C
		private void ProcessResults(PathfindingGenerator.Results results)
		{
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				return;
			}
			for (int i = 0; i < results.Requests.Length; i++)
			{
				int num = results.CompletedPathLengths[i];
				Pathfinding.Request request = results.Requests[i];
				if (num == 0)
				{
					request.NavigationPathCallback(new Pathfinding.Response(NpcEnum.PathfindingResult.Failure, null));
				}
				else
				{
					int num2 = i * this.NumSections * 2;
					Queue<NavPath.Segment> queue = new Queue<NavPath.Segment>(RuntimeHelpers.GetSubArray<NavPath.Segment>(results.CompletedPaths, new Range(num2, num2 + num)));
					Pathfinding.Response response = new Pathfinding.Response(NpcEnum.PathfindingResult.Success, new NavPath(request.EndPosition, queue));
					try
					{
						request.NavigationPathCallback(response);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				}
			}
			this.currentPathfindingJobs--;
			if (this.currentPathfindingJobs == 0 && this.updateCollections != null)
			{
				this.ApplyCollectionsUpdate();
			}
		}

		// Token: 0x06000ADC RID: 2780 RVA: 0x0003B03C File Offset: 0x0003923C
		private void ApplyCollectionsUpdate()
		{
			this.DisposeCollections();
			this.nodeMap = this.updateCollections.NodeMap;
			this.edgeMap = this.updateCollections.EdgeMap;
			this.surfaceMap = this.updateCollections.SurfaceMap;
			this.sectionMap = this.updateCollections.SectionMap;
			this.areaGraph = this.updateCollections.AreaGraph;
			this.walkableOctantDataMap = this.updateCollections.WalkableOctantDataMap;
			this.octants = this.updateCollections.Octants;
			this.NumSections = this.surfaceMap.Count();
			Action<HashSet<SerializableGuid>> onPathfindingUpdated = this.OnPathfindingUpdated;
			if (onPathfindingUpdated != null)
			{
				onPathfindingUpdated(this.updateCollections.UpdatedProps);
			}
			this.updateCollections = null;
		}

		// Token: 0x06000ADD RID: 2781 RVA: 0x0003B0FA File Offset: 0x000392FA
		public void NavMeshUpdated()
		{
			Action<HashSet<SerializableGuid>> onPathfindingUpdated = this.OnPathfindingUpdated;
			if (onPathfindingUpdated == null)
			{
				return;
			}
			onPathfindingUpdated(new HashSet<SerializableGuid>());
		}

		// Token: 0x06000ADE RID: 2782 RVA: 0x0003B114 File Offset: 0x00039314
		private void DisposeCollections()
		{
			if (!this.nodeMap.IsCreated)
			{
				return;
			}
			this.nodeMap.Dispose(default(JobHandle));
			this.edgeMap.Dispose(default(JobHandle));
			this.areaGraph.Dispose(default(JobHandle));
			this.surfaceMap.Dispose(default(JobHandle));
			this.sectionMap.Dispose(default(JobHandle));
			this.walkableOctantDataMap.Dispose(default(JobHandle));
			this.octants.Dispose(default(JobHandle));
		}

		// Token: 0x06000ADF RID: 2783 RVA: 0x0003B1C4 File Offset: 0x000393C4
		private static float GetPathDistance(Vector3[] corners)
		{
			float num = 0f;
			for (int i = 0; i < corners.Length - 1; i++)
			{
				num += Vector3.Distance(corners[i], corners[i + 1]);
			}
			return num;
		}

		// Token: 0x06000AE0 RID: 2784 RVA: 0x0003B200 File Offset: 0x00039400
		public static Vector3 GetPositionOnPath(Vector3[] corners, float distance)
		{
			float pathDistance = Pathfinding.GetPathDistance(corners);
			if (Mathf.Approximately(pathDistance, distance) || pathDistance < distance || Mathf.Approximately(pathDistance, 0f))
			{
				return corners[corners.Length - 1];
			}
			for (int i = 0; i < corners.Length - 1; i++)
			{
				float num = Vector3.Distance(corners[i], corners[i + 1]);
				if (num >= distance)
				{
					return Vector3.Lerp(corners[i], corners[i + 1], distance / num);
				}
				distance -= num;
			}
			throw new Exception("We shouldn't be able to get here");
		}

		// Token: 0x06000AE1 RID: 2785 RVA: 0x0003B28C File Offset: 0x0003948C
		public static Vector3 GetPositionOnPath(NavMeshPath path, float distance)
		{
			int cornersNonAlloc = path.GetCornersNonAlloc(Pathfinding.pathCorners);
			float pathDistance = Pathfinding.GetPathDistance(Pathfinding.pathCorners);
			if (Mathf.Approximately(pathDistance, distance) || pathDistance < distance || Mathf.Approximately(pathDistance, 0f))
			{
				return Pathfinding.pathCorners[cornersNonAlloc - 1];
			}
			for (int i = 0; i < cornersNonAlloc - 1; i++)
			{
				float num = Vector3.Distance(Pathfinding.pathCorners[i], Pathfinding.pathCorners[i + 1]);
				if (num >= distance)
				{
					return Vector3.Lerp(Pathfinding.pathCorners[i], Pathfinding.pathCorners[i + 1], distance / num);
				}
				distance -= num;
			}
			throw new Exception("We shouldn't be able to get here");
		}

		// Token: 0x06000AE2 RID: 2786 RVA: 0x0003B338 File Offset: 0x00039538
		public static Vector3? GetWanderPosition(Vector3 wanderPoint, float maxDistance)
		{
			List<Vector3> list = MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(wanderPoint, maxDistance);
			if (list.Count == 0)
			{
				return null;
			}
			int num = global::UnityEngine.Random.Range(0, list.Count);
			return new Vector3?(list[num]);
		}

		// Token: 0x06000AE3 RID: 2787 RVA: 0x0003B380 File Offset: 0x00039580
		public static Vector3? GetRovePosition(Vector3 roveCenter, Vector3 currentPosition, float maxDistanceFromCenter, float maxDistancePerMove)
		{
			List<Vector3> list = MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(currentPosition, maxDistancePerMove);
			while (list.Count > 0)
			{
				int num = global::UnityEngine.Random.Range(0, list.Count);
				Vector3 vector = list[num];
				list.RemoveAt(num);
				if (Vector3.Distance(vector, roveCenter) < maxDistanceFromCenter)
				{
					return new Vector3?(vector);
				}
			}
			return null;
		}

		// Token: 0x06000AE4 RID: 2788 RVA: 0x0003B3DC File Offset: 0x000395DC
		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CanReachPositionSynchronous(Vector3 startPosition, Vector3 endPosition, NativeArray<Octant> octants, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, NativeParallelMultiHashMap<int, int> areaGraph)
		{
			int num;
			int num2;
			if (!Pathfinding.TryGetAreaForPoint(startPosition, octants, walkableOctantDataMap, out num) || !Pathfinding.TryGetAreaForPoint(endPosition, octants, walkableOctantDataMap, out num2))
			{
				return false;
			}
			ValueTuple<NativeArray<int>, int> uniqueKeyArray = areaGraph.GetUniqueKeyArray(AllocatorManager.Temp);
			NativeQueue<int> nativeQueue = new NativeQueue<int>(AllocatorManager.Temp);
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(uniqueKeyArray.Item2, AllocatorManager.Temp);
			nativeQueue.Enqueue(num);
			while (nativeQueue.Count > 0)
			{
				int num3 = nativeQueue.Dequeue();
				if (num3 == num2)
				{
					return true;
				}
				foreach (int num4 in areaGraph.GetValuesForKey(num3))
				{
					if (nativeHashSet.Add(num4))
					{
						nativeQueue.Enqueue(num4);
					}
				}
			}
			return false;
		}

		// Token: 0x06000AE5 RID: 2789 RVA: 0x0003B4BC File Offset: 0x000396BC
		public bool IsCellNavigable(int3 cell)
		{
			int num;
			return OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(new float3((float)cell.x, (float)cell.y, (float)cell.z), this.octants, 1f, out num) && this.octants[num].IsWalkable;
		}

		// Token: 0x06000AE6 RID: 2790 RVA: 0x0003B50D File Offset: 0x0003970D
		public bool IsPositionNavigable(Vector3 position)
		{
			return this.IsPositionNavigable(position.ToFloat3());
		}

		// Token: 0x06000AE7 RID: 2791 RVA: 0x0003B51C File Offset: 0x0003971C
		private bool IsPositionNavigable(float3 position)
		{
			int num;
			Vector3 vector;
			return this.TryGetNearbyWalkableOctantIndex(position, out num, out vector);
		}

		// Token: 0x06000AE8 RID: 2792 RVA: 0x0003B534 File Offset: 0x00039734
		private bool TryGetNearbyWalkableOctantIndex(float3 point, out int octantIndex, out Vector3 walkablePosition)
		{
			walkablePosition = Vector3.negativeInfinity;
			octantIndex = -1;
			if (!this.octants.IsCreated)
			{
				return false;
			}
			if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(point, this.octants, 0.25f, out octantIndex))
			{
				ref Octant @ref = ref this.octants.GetRef(octantIndex);
				if (this.octants[octantIndex].IsWalkable)
				{
					NavMeshHit navMeshHit;
					if (!NavMesh.SamplePosition(@ref.Center, out navMeshHit, @ref.Size.x, -1))
					{
						Debug.LogWarning("This shouldn't happen");
					}
					else
					{
						walkablePosition = navMeshHit.position;
					}
					return true;
				}
			}
			for (int i = 0; i < 24; i++)
			{
				float3 @float = Pathfinding.GetNearbyOctantOffset(i, 0.25f);
				if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(point + @float, this.octants, 0.25f, out octantIndex))
				{
					ref Octant ref2 = ref this.octants.GetRef(octantIndex);
					if (this.octants[octantIndex].IsWalkable)
					{
						NavMeshHit navMeshHit2;
						if (!NavMesh.SamplePosition(ref2.Center, out navMeshHit2, ref2.Size.x, -1))
						{
							Debug.LogWarning("This shouldn't happen");
						}
						else
						{
							walkablePosition = navMeshHit2.position;
						}
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000AE9 RID: 2793 RVA: 0x0003B678 File Offset: 0x00039878
		private static Vector3 GetNearbyOctantOffset(int index, float offset)
		{
			float num = 0.017453292f * (float)index * 45f;
			Vector3 vector;
			if (index < 16)
			{
				if (index >= 8)
				{
					vector = new Vector3(Mathf.Cos(num) * offset, -offset, Mathf.Sin(num) * offset);
				}
				else
				{
					vector = new Vector3(Mathf.Cos(num) * offset, 0f, Mathf.Sin(num) * offset);
				}
			}
			else
			{
				if (index >= 24)
				{
					throw new ArgumentOutOfRangeException("index", index, null);
				}
				vector = new Vector3(Mathf.Cos(num) * offset, offset, Mathf.Sin(num) * offset);
			}
			return vector;
		}

		// Token: 0x06000AEA RID: 2794 RVA: 0x0003B70C File Offset: 0x0003990C
		public List<Vector3> FindNavigationPositionsInRange(Vector3 position, float maxDistance)
		{
			List<Vector3> list = new List<Vector3>();
			int num = Mathf.RoundToInt(maxDistance);
			for (int i = -num; i < num; i++)
			{
				for (int j = -num; j < num; j++)
				{
					for (int k = -num; k < num; k++)
					{
						Vector3 vector = position + new Vector3((float)i, (float)k, (float)j);
						int num2;
						NavMeshHit navMeshHit;
						if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(vector, this.octants, 1f, out num2) && this.octants[num2].IsWalkable && NavMesh.SamplePosition(vector, out navMeshHit, 1f, -1))
						{
							list.Add(navMeshHit.position);
						}
					}
				}
			}
			list.Sort((Vector3 vectorA, Vector3 vectorB) => (position - vectorA).sqrMagnitude.CompareTo((position - vectorB).sqrMagnitude));
			return list;
		}

		// Token: 0x06000AEB RID: 2795 RVA: 0x0003B7EC File Offset: 0x000399EC
		public bool AreOnSameIsland(float3 point1, float3 point2)
		{
			int num;
			Vector3 vector;
			int num2;
			return this.TryGetNearbyWalkableOctantIndex(point1, out num, out vector) && this.TryGetNearbyWalkableOctantIndex(point2, out num2, out vector) && this.walkableOctantDataMap[num].IslandKey == this.walkableOctantDataMap[num2].IslandKey;
		}

		// Token: 0x06000AEC RID: 2796 RVA: 0x0003B83B File Offset: 0x00039A3B
		private bool AreOnSameIsland(Vector3 point1, Vector3 point2)
		{
			return this.AreOnSameIsland(point1.ToFloat3(), point2.ToFloat3());
		}

		// Token: 0x06000AED RID: 2797 RVA: 0x0003B850 File Offset: 0x00039A50
		public bool AreInSameArea(float3 point1, float3 point2)
		{
			int num;
			Vector3 vector;
			int num2;
			return this.TryGetNearbyWalkableOctantIndex(point1, out num, out vector) && this.TryGetNearbyWalkableOctantIndex(point2, out num2, out vector) && this.walkableOctantDataMap[num].AreaKey == this.walkableOctantDataMap[num2].AreaKey;
		}

		// Token: 0x06000AEE RID: 2798 RVA: 0x0003B8A0 File Offset: 0x00039AA0
		public bool AreInSameZone(float3 point1, float3 point2)
		{
			int num;
			Vector3 vector;
			int num2;
			return this.TryGetNearbyWalkableOctantIndex(point1, out num, out vector) && this.TryGetNearbyWalkableOctantIndex(point2, out num2, out vector) && this.walkableOctantDataMap[num].ZoneKey == this.walkableOctantDataMap[num2].ZoneKey;
		}

		// Token: 0x06000AEF RID: 2799 RVA: 0x0003B8EF File Offset: 0x00039AEF
		public bool TryGetSectionKeyForPoint(Vector3 position, out int sectionKey)
		{
			return this.TryGetSectionKeyForPoint(position.ToFloat3(), out sectionKey);
		}

		// Token: 0x06000AF0 RID: 2800 RVA: 0x0003B900 File Offset: 0x00039B00
		private bool TryGetSectionKeyForPoint(float3 position, out int sectionKey)
		{
			int num;
			WalkableOctantData walkableOctantData;
			if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(position, this.octants, 0.25f, out num) && this.walkableOctantDataMap.TryGetValue(num, out walkableOctantData))
			{
				sectionKey = walkableOctantData.SectionKey;
				return true;
			}
			sectionKey = -1;
			return false;
		}

		// Token: 0x06000AF1 RID: 2801 RVA: 0x0003B940 File Offset: 0x00039B40
		private static bool TryGetAreaForPoint(float3 position, NativeArray<Octant> octants, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, out int areaKey)
		{
			int num;
			WalkableOctantData walkableOctantData;
			if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(position, octants, 0.25f, out num) && walkableOctantDataMap.TryGetValue(num, out walkableOctantData))
			{
				areaKey = walkableOctantData.AreaKey;
				return true;
			}
			areaKey = -1;
			return false;
		}

		// Token: 0x06000AF2 RID: 2802 RVA: 0x0003B978 File Offset: 0x00039B78
		public void EndlessGameEnd()
		{
			if (this.associatedPropMap.IsCreated)
			{
				this.associatedPropMap.Dispose(default(JobHandle));
			}
			this.DisposeCollections();
			this.OnPathfindingUpdated = null;
		}

		// Token: 0x06000AF3 RID: 2803 RVA: 0x0003B9B4 File Offset: 0x00039BB4
		public bool CheckBlockingAtPoint(float3 point)
		{
			return this.octants.IsCreated && OctreeHelperMethods.CheckBlockingAtPoint(point, this.octants);
		}

		// Token: 0x06000AF4 RID: 2804 RVA: 0x0003B9D1 File Offset: 0x00039BD1
		public bool CheckBlockingAtPoint(Vector3 point)
		{
			return this.CheckBlockingAtPoint(point.ToFloat3());
		}

		// Token: 0x04000A27 RID: 2599
		public const int INVALID_KEY = -1;

		// Token: 0x04000A28 RID: 2600
		private readonly Dictionary<WorldObject, Pathfinding.Request> pathfindingRequests = new Dictionary<WorldObject, Pathfinding.Request>();

		// Token: 0x04000A29 RID: 2601
		private readonly List<Pathfinding.Request> requestsToProcess = new List<Pathfinding.Request>();

		// Token: 0x04000A2A RID: 2602
		private static readonly Vector3[] pathCorners = new Vector3[100];

		// Token: 0x04000A2C RID: 2604
		private bool hasReceivedCollections;

		// Token: 0x04000A2D RID: 2605
		private Pathfinding.UpdateCollections updateCollections;

		// Token: 0x04000A2E RID: 2606
		private int currentPathfindingJobs;

		// Token: 0x04000A2F RID: 2607
		private NativeParallelHashMap<int, SerializableGuid> associatedPropMap;

		// Token: 0x04000A30 RID: 2608
		private NativeParallelMultiHashMap<int, Edge> edgeMap;

		// Token: 0x04000A31 RID: 2609
		private NativeParallelHashMap<int, GraphNode> nodeMap;

		// Token: 0x04000A32 RID: 2610
		private NativeParallelHashMap<int, SectionSurface> surfaceMap;

		// Token: 0x04000A33 RID: 2611
		private NativeParallelMultiHashMap<int, int> sectionMap;

		// Token: 0x04000A34 RID: 2612
		private NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap;

		// Token: 0x04000A35 RID: 2613
		private NativeParallelMultiHashMap<int, int> areaGraph;

		// Token: 0x04000A36 RID: 2614
		private NativeArray<Octant> octants;

		// Token: 0x04000A38 RID: 2616
		[SerializeField]
		private float onMeshShortcutDistance = 8f;

		// Token: 0x0200020A RID: 522
		private class UpdateCollections
		{
			// Token: 0x04000A39 RID: 2617
			public HashSet<SerializableGuid> UpdatedProps;

			// Token: 0x04000A3A RID: 2618
			public NativeParallelMultiHashMap<int, Edge> EdgeMap;

			// Token: 0x04000A3B RID: 2619
			public NativeParallelHashMap<int, GraphNode> NodeMap;

			// Token: 0x04000A3C RID: 2620
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x04000A3D RID: 2621
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x04000A3E RID: 2622
			public NativeParallelMultiHashMap<int, int> AreaGraph;

			// Token: 0x04000A3F RID: 2623
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x04000A40 RID: 2624
			public NativeArray<Octant> Octants;
		}

		// Token: 0x0200020B RID: 523
		public struct Request
		{
			// Token: 0x1700020D RID: 525
			// (get) Token: 0x06000AF8 RID: 2808 RVA: 0x0003BA16 File Offset: 0x00039C16
			// (set) Token: 0x06000AF9 RID: 2809 RVA: 0x0003BA1E File Offset: 0x00039C1E
			public bool IsValid { readonly get; private set; }

			// Token: 0x06000AFA RID: 2810 RVA: 0x0003BA28 File Offset: 0x00039C28
			public static bool TryCreateNewPathfindingRequest(out Pathfinding.Request request, WorldObject requester, Vector3 startPosition, Vector3 endPosition, PathfindingRange pathfindingRange, Action<Pathfinding.Response> navigationPathCallback, List<ExcludedEdge> excludedEdges = null)
			{
				if (!MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(startPosition) || !MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(endPosition))
				{
					request = default(Pathfinding.Request);
					return false;
				}
				if (excludedEdges == null)
				{
					excludedEdges = new List<ExcludedEdge>();
				}
				request = new Pathfinding.Request(requester, startPosition, endPosition, pathfindingRange, navigationPathCallback, excludedEdges);
				return true;
			}

			// Token: 0x06000AFB RID: 2811 RVA: 0x0003BA78 File Offset: 0x00039C78
			private Request(WorldObject requester, Vector3 startPosition, Vector3 endPosition, PathfindingRange pathfindingRange, Action<Pathfinding.Response> navigationPathCallback, List<ExcludedEdge> excludedEdges)
			{
				this.Requester = requester;
				this.StartPosition = startPosition;
				this.EndPosition = endPosition;
				this.PathfindingRange = pathfindingRange;
				this.NavigationPathCallback = navigationPathCallback;
				this.OnMeshPathDistance = 0f;
				this.ExcludedEdges = excludedEdges;
				this.IsValid = true;
			}

			// Token: 0x06000AFC RID: 2812 RVA: 0x0003BAC4 File Offset: 0x00039CC4
			public NativePathfindingRequest GetNativePathfindingRequest()
			{
				bool flag = false;
				Vector3 startPosition = this.StartPosition;
				Vector3 endPosition = this.EndPosition;
				int num;
				if (!MonoBehaviourSingleton<Pathfinding>.Instance.TryGetSectionKeyForPoint(this.StartPosition, out num))
				{
					int num2;
					if (MonoBehaviourSingleton<Pathfinding>.Instance.TryGetNearbyWalkableOctantIndex(this.StartPosition, out num2, out startPosition))
					{
						num = MonoBehaviourSingleton<Pathfinding>.Instance.walkableOctantDataMap[num2].SectionKey;
					}
					else
					{
						flag = true;
					}
				}
				int num3;
				if (!MonoBehaviourSingleton<Pathfinding>.Instance.TryGetSectionKeyForPoint(this.EndPosition, out num3))
				{
					int num4;
					if (MonoBehaviourSingleton<Pathfinding>.Instance.TryGetNearbyWalkableOctantIndex(this.EndPosition, out num4, out endPosition))
					{
						num3 = MonoBehaviourSingleton<Pathfinding>.Instance.walkableOctantDataMap[num4].SectionKey;
					}
					else
					{
						flag = true;
					}
				}
				if (flag)
				{
					num = -1;
					num3 = -1;
				}
				return new NativePathfindingRequest
				{
					StartPosition = startPosition,
					EndPosition = endPosition,
					StartNodeKey = num,
					EndNodeKey = num3,
					PathfindingRange = this.PathfindingRange,
					OnMeshPathDistance = this.OnMeshPathDistance
				};
			}

			// Token: 0x04000A42 RID: 2626
			public readonly WorldObject Requester;

			// Token: 0x04000A43 RID: 2627
			public readonly Vector3 StartPosition;

			// Token: 0x04000A44 RID: 2628
			public readonly Vector3 EndPosition;

			// Token: 0x04000A45 RID: 2629
			public readonly PathfindingRange PathfindingRange;

			// Token: 0x04000A46 RID: 2630
			public readonly Action<Pathfinding.Response> NavigationPathCallback;

			// Token: 0x04000A47 RID: 2631
			public float OnMeshPathDistance;

			// Token: 0x04000A48 RID: 2632
			public readonly List<ExcludedEdge> ExcludedEdges;
		}

		// Token: 0x0200020C RID: 524
		public struct Response
		{
			// Token: 0x06000AFD RID: 2813 RVA: 0x0003BBC3 File Offset: 0x00039DC3
			public Response(NpcEnum.PathfindingResult pathfindingResult, NavPath path)
			{
				this.PathfindingResult = pathfindingResult;
				this.Path = path;
			}

			// Token: 0x04000A49 RID: 2633
			public readonly NpcEnum.PathfindingResult PathfindingResult;

			// Token: 0x04000A4A RID: 2634
			public readonly NavPath Path;
		}
	}
}
