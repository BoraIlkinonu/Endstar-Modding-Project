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

namespace Endless.Gameplay;

public class Pathfinding : EndlessBehaviourSingleton<Pathfinding>, IGameEndSubscriber
{
	private class UpdateCollections
	{
		public HashSet<SerializableGuid> UpdatedProps;

		public NativeParallelMultiHashMap<int, Edge> EdgeMap;

		public NativeParallelHashMap<int, GraphNode> NodeMap;

		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		public NativeParallelMultiHashMap<int, int> SectionMap;

		public NativeParallelMultiHashMap<int, int> AreaGraph;

		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		public NativeArray<Octant> Octants;
	}

	public struct Request
	{
		public readonly WorldObject Requester;

		public readonly Vector3 StartPosition;

		public readonly Vector3 EndPosition;

		public readonly PathfindingRange PathfindingRange;

		public readonly Action<Response> NavigationPathCallback;

		public float OnMeshPathDistance;

		public readonly List<ExcludedEdge> ExcludedEdges;

		public bool IsValid { get; private set; }

		public static bool TryCreateNewPathfindingRequest(out Request request, WorldObject requester, Vector3 startPosition, Vector3 endPosition, PathfindingRange pathfindingRange, Action<Response> navigationPathCallback, List<ExcludedEdge> excludedEdges = null)
		{
			if (!MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(startPosition) || !MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(endPosition))
			{
				request = default(Request);
				return false;
			}
			if (excludedEdges == null)
			{
				excludedEdges = new List<ExcludedEdge>();
			}
			request = new Request(requester, startPosition, endPosition, pathfindingRange, navigationPathCallback, excludedEdges);
			return true;
		}

		private Request(WorldObject requester, Vector3 startPosition, Vector3 endPosition, PathfindingRange pathfindingRange, Action<Response> navigationPathCallback, List<ExcludedEdge> excludedEdges)
		{
			Requester = requester;
			StartPosition = startPosition;
			EndPosition = endPosition;
			PathfindingRange = pathfindingRange;
			NavigationPathCallback = navigationPathCallback;
			OnMeshPathDistance = 0f;
			ExcludedEdges = excludedEdges;
			IsValid = true;
		}

		public NativePathfindingRequest GetNativePathfindingRequest()
		{
			bool flag = false;
			Vector3 walkablePosition = StartPosition;
			Vector3 walkablePosition2 = EndPosition;
			if (!MonoBehaviourSingleton<Pathfinding>.Instance.TryGetSectionKeyForPoint(StartPosition, out var sectionKey))
			{
				if (MonoBehaviourSingleton<Pathfinding>.Instance.TryGetNearbyWalkableOctantIndex(StartPosition, out var octantIndex, out walkablePosition))
				{
					sectionKey = MonoBehaviourSingleton<Pathfinding>.Instance.walkableOctantDataMap[octantIndex].SectionKey;
				}
				else
				{
					flag = true;
				}
			}
			if (!MonoBehaviourSingleton<Pathfinding>.Instance.TryGetSectionKeyForPoint(EndPosition, out var sectionKey2))
			{
				if (MonoBehaviourSingleton<Pathfinding>.Instance.TryGetNearbyWalkableOctantIndex(EndPosition, out var octantIndex2, out walkablePosition2))
				{
					sectionKey2 = MonoBehaviourSingleton<Pathfinding>.Instance.walkableOctantDataMap[octantIndex2].SectionKey;
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				sectionKey = -1;
				sectionKey2 = -1;
			}
			return new NativePathfindingRequest
			{
				StartPosition = walkablePosition,
				EndPosition = walkablePosition2,
				StartNodeKey = sectionKey,
				EndNodeKey = sectionKey2,
				PathfindingRange = PathfindingRange,
				OnMeshPathDistance = OnMeshPathDistance
			};
		}
	}

	public struct Response
	{
		public readonly NpcEnum.PathfindingResult PathfindingResult;

		public readonly NavPath Path;

		public Response(NpcEnum.PathfindingResult pathfindingResult, NavPath path)
		{
			PathfindingResult = pathfindingResult;
			Path = path;
		}
	}

	public const int INVALID_KEY = -1;

	private readonly Dictionary<WorldObject, Request> pathfindingRequests = new Dictionary<WorldObject, Request>();

	private readonly List<Request> requestsToProcess = new List<Request>();

	private static readonly Vector3[] pathCorners = new Vector3[100];

	private bool hasReceivedCollections;

	private UpdateCollections updateCollections;

	private int currentPathfindingJobs;

	private NativeParallelHashMap<int, SerializableGuid> associatedPropMap;

	private NativeParallelMultiHashMap<int, Edge> edgeMap;

	private NativeParallelHashMap<int, GraphNode> nodeMap;

	private NativeParallelHashMap<int, SectionSurface> surfaceMap;

	private NativeParallelMultiHashMap<int, int> sectionMap;

	private NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap;

	private NativeParallelMultiHashMap<int, int> areaGraph;

	private NativeArray<Octant> octants;

	[SerializeField]
	private float onMeshShortcutDistance = 8f;

	public int NumSections { get; private set; }

	public event Action<HashSet<SerializableGuid>> OnPathfindingUpdated;

	protected override void Start()
	{
		base.Start();
		UnifiedStateUpdater.OnProcessRequests += ProcessRequests;
	}

	public void InitializePathfindingCollections(NativeParallelMultiHashMap<int, Edge> edges, NativeParallelHashMap<int, GraphNode> nodes, NativeParallelHashMap<int, SectionSurface> surfaces, NativeParallelMultiHashMap<int, int> sections, NativeParallelMultiHashMap<int, int> areaConnectionGraph, NativeParallelHashMap<int, WalkableOctantData> octantData, NativeArray<Octant> octantArray, NativeParallelHashMap<int, SerializableGuid> propMap)
	{
		hasReceivedCollections = true;
		edgeMap = edges;
		nodeMap = nodes;
		surfaceMap = surfaces;
		sectionMap = sections;
		NumSections = surfaceMap.Count();
		areaGraph = areaConnectionGraph;
		walkableOctantDataMap = octantData;
		octants = octantArray;
		associatedPropMap = propMap;
	}

	public void UpdatePathfindingCollections(HashSet<SerializableGuid> updatedProps, NativeParallelMultiHashMap<int, Edge> edges, NativeParallelHashMap<int, GraphNode> nodes, NativeParallelHashMap<int, SectionSurface> surfaces, NativeParallelMultiHashMap<int, int> sections, NativeParallelMultiHashMap<int, int> areaConnectionGraph, NativeParallelHashMap<int, WalkableOctantData> octantData, NativeArray<Octant> octantArray)
	{
		updateCollections = new UpdateCollections
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
		if (currentPathfindingJobs == 0)
		{
			ApplyCollectionsUpdate();
		}
	}

	private bool CanReachPosition(Vector3 startPosition, Vector3 endPosition)
	{
		if (!IsPositionNavigable(startPosition) || !IsPositionNavigable(endPosition))
		{
			return false;
		}
		return CanReachPositionSynchronous(startPosition, endPosition, octants, walkableOctantDataMap, areaGraph);
	}

	public bool IsValidDestination(Vector3 origin, Vector3 destination, PathfindingRange range, bool canDoubleJump)
	{
		switch (range)
		{
		case PathfindingRange.Island:
			return MonoBehaviourSingleton<Pathfinding>.Instance.AreInSameArea(origin, destination);
		case PathfindingRange.Area:
			if (canDoubleJump)
			{
				return MonoBehaviourSingleton<Pathfinding>.Instance.AreInSameZone(origin, destination);
			}
			return MonoBehaviourSingleton<Pathfinding>.Instance.AreInSameArea(origin, destination);
		case PathfindingRange.Global:
			return CanReachGlobalLocation(origin, destination, canDoubleJump);
		default:
			throw new ArgumentOutOfRangeException("range", range, null);
		}
	}

	private static bool CanReachGlobalLocation(float3 startPoint, float3 endPoint, bool canDoubleJump)
	{
		if (MonoBehaviourSingleton<Pathfinding>.Instance.AreInSameArea(startPoint, endPoint))
		{
			return true;
		}
		if (canDoubleJump && MonoBehaviourSingleton<Pathfinding>.Instance.AreInSameZone(startPoint, endPoint))
		{
			return true;
		}
		return MonoBehaviourSingleton<Pathfinding>.Instance.CanReachPosition(startPoint, endPoint);
	}

	public void RequestPath(Request request)
	{
		pathfindingRequests[request.Requester] = request;
		if (!IsValidDestination(request.StartPosition, request.EndPosition, request.PathfindingRange, canDoubleJump: false))
		{
			request.NavigationPathCallback(new Response(NpcEnum.PathfindingResult.Failure, null));
			pathfindingRequests.Remove(request.Requester);
		}
		else
		{
			if (!AreOnSameIsland(request.StartPosition, request.EndPosition))
			{
				return;
			}
			NavMeshPath navMeshPath = new NavMeshPath();
			NavMesh.CalculatePath(request.StartPosition, request.EndPosition, NpcEntity.NavFilter, navMeshPath);
			if (navMeshPath.status == NavMeshPathStatus.PathComplete)
			{
				float pathDistance = GetPathDistance(navMeshPath.corners);
				if (pathDistance < onMeshShortcutDistance)
				{
					NavPath navPath = new NavPath(request.EndPosition, new Queue<NavPath.Segment>());
					NavPath.Segment item = new NavPath.Segment
					{
						StartPosition = request.StartPosition,
						EndPosition = request.EndPosition
					};
					navPath.NavigationSegments.Enqueue(item);
					request.NavigationPathCallback(new Response(NpcEnum.PathfindingResult.Success, navPath));
					pathfindingRequests.Remove(request.Requester);
				}
				request.OnMeshPathDistance = pathDistance;
			}
		}
	}

	public Door GetDoorFromThresholdSection(int sectionKey)
	{
		sectionMap.TryGetFirstValue(sectionKey, out var item, out var _);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(MonoBehaviourSingleton<Pathfinding>.Instance.associatedPropMap[item]).GetComponent<WorldObject>().TryGetUserComponent<Door>(out var component);
		return component;
	}

	private void ProcessRequests()
	{
		if (pathfindingRequests.Count != 0 && hasReceivedCollections && updateCollections == null)
		{
			requestsToProcess.Clear();
			requestsToProcess.AddRange(pathfindingRequests.Values);
			currentPathfindingJobs++;
			StartCoroutine(PathfindingGenerator.GeneratePathfindingResults(new List<Request>(requestsToProcess), nodeMap, edgeMap, surfaceMap, octants, NumSections, ProcessResults));
			pathfindingRequests.Clear();
		}
	}

	private void ProcessResults(PathfindingGenerator.Results results)
	{
		if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			return;
		}
		for (int i = 0; i < results.Requests.Length; i++)
		{
			int num = results.CompletedPathLengths[i];
			Request request = results.Requests[i];
			if (num == 0)
			{
				request.NavigationPathCallback(new Response(NpcEnum.PathfindingResult.Failure, null));
				continue;
			}
			int num2 = i * NumSections * 2;
			Queue<NavPath.Segment> segments = new Queue<NavPath.Segment>(results.CompletedPaths[num2..(num2 + num)]);
			Response obj = new Response(NpcEnum.PathfindingResult.Success, new NavPath(request.EndPosition, segments));
			try
			{
				request.NavigationPathCallback(obj);
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
		}
		currentPathfindingJobs--;
		if (currentPathfindingJobs == 0 && updateCollections != null)
		{
			ApplyCollectionsUpdate();
		}
	}

	private void ApplyCollectionsUpdate()
	{
		DisposeCollections();
		nodeMap = updateCollections.NodeMap;
		edgeMap = updateCollections.EdgeMap;
		surfaceMap = updateCollections.SurfaceMap;
		sectionMap = updateCollections.SectionMap;
		areaGraph = updateCollections.AreaGraph;
		walkableOctantDataMap = updateCollections.WalkableOctantDataMap;
		octants = updateCollections.Octants;
		NumSections = surfaceMap.Count();
		this.OnPathfindingUpdated?.Invoke(updateCollections.UpdatedProps);
		updateCollections = null;
	}

	public void NavMeshUpdated()
	{
		this.OnPathfindingUpdated?.Invoke(new HashSet<SerializableGuid>());
	}

	private void DisposeCollections()
	{
		if (nodeMap.IsCreated)
		{
			nodeMap.Dispose(default(JobHandle));
			edgeMap.Dispose(default(JobHandle));
			areaGraph.Dispose(default(JobHandle));
			surfaceMap.Dispose(default(JobHandle));
			sectionMap.Dispose(default(JobHandle));
			walkableOctantDataMap.Dispose(default(JobHandle));
			octants.Dispose(default(JobHandle));
		}
	}

	private static float GetPathDistance(Vector3[] corners)
	{
		float num = 0f;
		for (int i = 0; i < corners.Length - 1; i++)
		{
			num += Vector3.Distance(corners[i], corners[i + 1]);
		}
		return num;
	}

	public static Vector3 GetPositionOnPath(Vector3[] corners, float distance)
	{
		float pathDistance = GetPathDistance(corners);
		if (Mathf.Approximately(pathDistance, distance) || pathDistance < distance || Mathf.Approximately(pathDistance, 0f))
		{
			return corners[^1];
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

	public static Vector3 GetPositionOnPath(NavMeshPath path, float distance)
	{
		int cornersNonAlloc = path.GetCornersNonAlloc(pathCorners);
		float pathDistance = GetPathDistance(pathCorners);
		if (Mathf.Approximately(pathDistance, distance) || pathDistance < distance || Mathf.Approximately(pathDistance, 0f))
		{
			return pathCorners[cornersNonAlloc - 1];
		}
		for (int i = 0; i < cornersNonAlloc - 1; i++)
		{
			float num = Vector3.Distance(pathCorners[i], pathCorners[i + 1]);
			if (num >= distance)
			{
				return Vector3.Lerp(pathCorners[i], pathCorners[i + 1], distance / num);
			}
			distance -= num;
		}
		throw new Exception("We shouldn't be able to get here");
	}

	public static Vector3? GetWanderPosition(Vector3 wanderPoint, float maxDistance)
	{
		List<Vector3> list = MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(wanderPoint, maxDistance);
		if (list.Count == 0)
		{
			return null;
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		return list[index];
	}

	public static Vector3? GetRovePosition(Vector3 roveCenter, Vector3 currentPosition, float maxDistanceFromCenter, float maxDistancePerMove)
	{
		List<Vector3> list = MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(currentPosition, maxDistancePerMove);
		while (list.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			Vector3 vector = list[index];
			list.RemoveAt(index);
			if (Vector3.Distance(vector, roveCenter) < maxDistanceFromCenter)
			{
				return vector;
			}
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	private static bool CanReachPositionSynchronous(Vector3 startPosition, Vector3 endPosition, NativeArray<Octant> octants, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, NativeParallelMultiHashMap<int, int> areaGraph)
	{
		if (!TryGetAreaForPoint(startPosition, octants, walkableOctantDataMap, out var areaKey) || !TryGetAreaForPoint(endPosition, octants, walkableOctantDataMap, out var areaKey2))
		{
			return false;
		}
		(NativeArray<int>, int) uniqueKeyArray = areaGraph.GetUniqueKeyArray(AllocatorManager.Temp);
		NativeQueue<int> nativeQueue = new NativeQueue<int>(AllocatorManager.Temp);
		NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(uniqueKeyArray.Item2, AllocatorManager.Temp);
		nativeQueue.Enqueue(areaKey);
		while (nativeQueue.Count > 0)
		{
			int num = nativeQueue.Dequeue();
			if (num == areaKey2)
			{
				return true;
			}
			foreach (int item in areaGraph.GetValuesForKey(num))
			{
				if (nativeHashSet.Add(item))
				{
					nativeQueue.Enqueue(item);
				}
			}
		}
		return false;
	}

	public bool IsCellNavigable(int3 cell)
	{
		if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(new float3(cell.x, cell.y, cell.z), octants, 1f, out var index))
		{
			return octants[index].IsWalkable;
		}
		return false;
	}

	public bool IsPositionNavigable(Vector3 position)
	{
		return IsPositionNavigable(position.ToFloat3());
	}

	private bool IsPositionNavigable(float3 position)
	{
		int octantIndex;
		Vector3 walkablePosition;
		return TryGetNearbyWalkableOctantIndex(position, out octantIndex, out walkablePosition);
	}

	private bool TryGetNearbyWalkableOctantIndex(float3 point, out int octantIndex, out Vector3 walkablePosition)
	{
		walkablePosition = Vector3.negativeInfinity;
		octantIndex = -1;
		if (!octants.IsCreated)
		{
			return false;
		}
		if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(point, octants, 0.25f, out octantIndex))
		{
			ref Octant reference = ref octants.GetRef(octantIndex);
			if (octants[octantIndex].IsWalkable)
			{
				if (!NavMesh.SamplePosition(reference.Center, out var hit, reference.Size.x, -1))
				{
					Debug.LogWarning("This shouldn't happen");
				}
				else
				{
					walkablePosition = hit.position;
				}
				return true;
			}
		}
		for (int i = 0; i < 24; i++)
		{
			float3 @float = GetNearbyOctantOffset(i, 0.25f);
			if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(point + @float, octants, 0.25f, out octantIndex))
			{
				continue;
			}
			ref Octant reference2 = ref octants.GetRef(octantIndex);
			if (octants[octantIndex].IsWalkable)
			{
				if (!NavMesh.SamplePosition(reference2.Center, out var hit2, reference2.Size.x, -1))
				{
					Debug.LogWarning("This shouldn't happen");
				}
				else
				{
					walkablePosition = hit2.position;
				}
				return true;
			}
		}
		return false;
	}

	private static Vector3 GetNearbyOctantOffset(int index, float offset)
	{
		float f = MathF.PI / 180f * (float)index * 45f;
		if (index < 16)
		{
			if (index < 8)
			{
				return new Vector3(Mathf.Cos(f) * offset, 0f, Mathf.Sin(f) * offset);
			}
			return new Vector3(Mathf.Cos(f) * offset, 0f - offset, Mathf.Sin(f) * offset);
		}
		if (index < 24)
		{
			return new Vector3(Mathf.Cos(f) * offset, offset, Mathf.Sin(f) * offset);
		}
		throw new ArgumentOutOfRangeException("index", index, null);
	}

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
					Vector3 vector = position + new Vector3(i, k, j);
					if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(vector, octants, 1f, out var index) && octants[index].IsWalkable && NavMesh.SamplePosition(vector, out var hit, 1f, -1))
					{
						list.Add(hit.position);
					}
				}
			}
		}
		list.Sort((Vector3 vectorA, Vector3 vectorB) => (position - vectorA).sqrMagnitude.CompareTo((position - vectorB).sqrMagnitude));
		return list;
	}

	public bool AreOnSameIsland(float3 point1, float3 point2)
	{
		if (!TryGetNearbyWalkableOctantIndex(point1, out var octantIndex, out var walkablePosition))
		{
			return false;
		}
		if (!TryGetNearbyWalkableOctantIndex(point2, out var octantIndex2, out walkablePosition))
		{
			return false;
		}
		return walkableOctantDataMap[octantIndex].IslandKey == walkableOctantDataMap[octantIndex2].IslandKey;
	}

	private bool AreOnSameIsland(Vector3 point1, Vector3 point2)
	{
		return AreOnSameIsland(point1.ToFloat3(), point2.ToFloat3());
	}

	public bool AreInSameArea(float3 point1, float3 point2)
	{
		if (!TryGetNearbyWalkableOctantIndex(point1, out var octantIndex, out var walkablePosition))
		{
			return false;
		}
		if (!TryGetNearbyWalkableOctantIndex(point2, out var octantIndex2, out walkablePosition))
		{
			return false;
		}
		return walkableOctantDataMap[octantIndex].AreaKey == walkableOctantDataMap[octantIndex2].AreaKey;
	}

	public bool AreInSameZone(float3 point1, float3 point2)
	{
		if (!TryGetNearbyWalkableOctantIndex(point1, out var octantIndex, out var walkablePosition))
		{
			return false;
		}
		if (!TryGetNearbyWalkableOctantIndex(point2, out var octantIndex2, out walkablePosition))
		{
			return false;
		}
		return walkableOctantDataMap[octantIndex].ZoneKey == walkableOctantDataMap[octantIndex2].ZoneKey;
	}

	public bool TryGetSectionKeyForPoint(Vector3 position, out int sectionKey)
	{
		return TryGetSectionKeyForPoint(position.ToFloat3(), out sectionKey);
	}

	private bool TryGetSectionKeyForPoint(float3 position, out int sectionKey)
	{
		if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(position, octants, 0.25f, out var index) && walkableOctantDataMap.TryGetValue(index, out var item))
		{
			sectionKey = item.SectionKey;
			return true;
		}
		sectionKey = -1;
		return false;
	}

	private static bool TryGetAreaForPoint(float3 position, NativeArray<Octant> octants, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, out int areaKey)
	{
		if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(position, octants, 0.25f, out var index) && walkableOctantDataMap.TryGetValue(index, out var item))
		{
			areaKey = item.AreaKey;
			return true;
		}
		areaKey = -1;
		return false;
	}

	public void EndlessGameEnd()
	{
		if (associatedPropMap.IsCreated)
		{
			associatedPropMap.Dispose(default(JobHandle));
		}
		DisposeCollections();
		this.OnPathfindingUpdated = null;
	}

	public bool CheckBlockingAtPoint(float3 point)
	{
		if (octants.IsCreated)
		{
			return OctreeHelperMethods.CheckBlockingAtPoint(point, octants);
		}
		return false;
	}

	public bool CheckBlockingAtPoint(Vector3 point)
	{
		return CheckBlockingAtPoint(point.ToFloat3());
	}
}
