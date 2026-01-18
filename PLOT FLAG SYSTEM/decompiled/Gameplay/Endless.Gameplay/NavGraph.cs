using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.AI.Navigation;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class NavGraph : EndlessBehaviourSingleton<NavGraph>, NetClock.ISimulateFrameLateSubscriber, IGameEndSubscriber, IStartSubscriber
{
	public struct ChangedCell
	{
		public int3 Cell;

		public WorldObject WorldObject;

		public bool IsBlocking;
	}

	[BurstCompile]
	private struct BuildPathfindingGraphJob : IJob
	{
		[ReadOnly]
		public NativeParallelHashMap<int, SectionSurface> SectionToSurfaceMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> SectionMap;

		[ReadOnly]
		public NativeParallelHashSet<BidirectionalConnection> ThresholdConnections;

		[ReadOnly]
		public NativeParallelHashSet<BidirectionalConnection> WalkConnections;

		[ReadOnly]
		public NativeParallelHashSet<BidirectionalConnection> JumpConnections;

		[ReadOnly]
		public NativeParallelHashSet<Connection> DropConnections;

		[ReadOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantData;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, Edge> EdgeMap;

		[WriteOnly]
		public NativeParallelHashMap<int, GraphNode> NodeMap;

		public void Execute()
		{
			foreach (KeyValue<int, SectionSurface> item2 in SectionToSurfaceMap)
			{
				if (!SectionMap.TryGetFirstValue(item2.Key, out var item, out var _))
				{
					throw new Exception("Section without any octants found. This shouldn't be possible");
				}
				WalkableOctantData walkableOctantData = WalkableOctantData[item];
				NodeMap.Add(item2.Key, new GraphNode
				{
					IslandKey = walkableOctantData.IslandKey,
					AreaKey = walkableOctantData.AreaKey,
					ZoneKey = walkableOctantData.ZoneKey,
					Key = item2.Key,
					Center = item2.Value.Center
				});
			}
			foreach (BidirectionalConnection thresholdConnection in ThresholdConnections)
			{
				AddConnection(thresholdConnection.SectionIndexA, thresholdConnection.SectionIndexB, ConnectionKind.Threshold);
				AddConnection(thresholdConnection.SectionIndexB, thresholdConnection.SectionIndexA, ConnectionKind.Threshold);
			}
			foreach (BidirectionalConnection walkConnection in WalkConnections)
			{
				AddConnection(walkConnection.SectionIndexA, walkConnection.SectionIndexB, ConnectionKind.Walk);
				AddConnection(walkConnection.SectionIndexB, walkConnection.SectionIndexA, ConnectionKind.Walk);
			}
			foreach (BidirectionalConnection jumpConnection in JumpConnections)
			{
				AddConnection(jumpConnection.SectionIndexA, jumpConnection.SectionIndexB, ConnectionKind.Jump, NpcMovementValues.JumpCostScalar);
				AddConnection(jumpConnection.SectionIndexB, jumpConnection.SectionIndexA, ConnectionKind.Jump, NpcMovementValues.JumpCostScalar);
			}
			foreach (Connection dropConnection in DropConnections)
			{
				AddConnection(dropConnection.StartSectionKey, dropConnection.EndSectionKey, ConnectionKind.Dropdown, NpcMovementValues.DropCostScalar);
			}
		}

		private void AddConnection(int startSectionIndex, int endSectionIndex, ConnectionKind connectionKind, float costScalar = 1f)
		{
			float3 center = SectionToSurfaceMap[startSectionIndex].Center;
			float3 center2 = SectionToSurfaceMap[endSectionIndex].Center;
			float cost = math.distance(center, center2) * costScalar;
			Edge item = new Edge
			{
				Connection = connectionKind,
				ConnectedNodeKey = endSectionIndex,
				Cost = cost
			};
			EdgeMap.Add(startSectionIndex, item);
		}
	}

	[BurstCompile]
	private struct CopyNativeCollectionsJob : IJob
	{
		[ReadOnly]
		public NativeParallelMultiHashMap<int, Edge> EdgeMap;

		[ReadOnly]
		public NativeParallelHashMap<int, GraphNode> NodeMap;

		[ReadOnly]
		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		[ReadOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> SectionMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> AreaGraph;

		[ReadOnly]
		public NativeParallelHashMap<int, SerializableGuid> PropMap;

		[ReadOnly]
		public NativeArray<Octant> Octants;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, Edge> NewEdgeMap;

		[WriteOnly]
		public NativeParallelHashMap<int, GraphNode> NewNodeMap;

		[WriteOnly]
		public NativeParallelHashMap<int, SectionSurface> NewSurfaceMap;

		[WriteOnly]
		public NativeParallelHashMap<int, WalkableOctantData> NewWalkableOctantDataMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int> NewSectionMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int> NewAreaGraph;

		[WriteOnly]
		public NativeParallelHashMap<int, SerializableGuid> NewPropMap;

		[WriteOnly]
		public NativeArray<Octant> NewOctants;

		public void Execute()
		{
			foreach (KeyValue<int, Edge> item in EdgeMap)
			{
				NewEdgeMap.Add(item.Key, item.Value);
			}
			foreach (KeyValue<int, GraphNode> item2 in NodeMap)
			{
				NewNodeMap.Add(item2.Key, item2.Value);
			}
			foreach (KeyValue<int, SectionSurface> item3 in SurfaceMap)
			{
				NewSurfaceMap.Add(item3.Key, item3.Value);
			}
			foreach (KeyValue<int, WalkableOctantData> item4 in WalkableOctantDataMap)
			{
				NewWalkableOctantDataMap.Add(item4.Key, item4.Value);
			}
			foreach (KeyValue<int, int> item5 in SectionMap)
			{
				NewSectionMap.Add(item5.Key, item5.Value);
			}
			foreach (KeyValue<int, int> item6 in AreaGraph)
			{
				NewAreaGraph.Add(item6.Key, item6.Value);
			}
			foreach (KeyValue<int, SerializableGuid> item7 in PropMap)
			{
				NewPropMap.Add(item7.Key, item7.Value);
			}
			for (int i = 0; i < Octants.Length; i++)
			{
				NewOctants[i] = Octants[i];
			}
		}
	}

	[BurstCompile]
	private struct CopyNativeCollectionsGameplayJob : IJob
	{
		[ReadOnly]
		public NativeParallelMultiHashMap<int, Edge> EdgeMap;

		[ReadOnly]
		public NativeParallelHashMap<int, GraphNode> NodeMap;

		[ReadOnly]
		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		[ReadOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> SectionMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> AreaGraph;

		[ReadOnly]
		public NativeArray<Octant> Octants;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, Edge> NewEdgeMap;

		[WriteOnly]
		public NativeParallelHashMap<int, GraphNode> NewNodeMap;

		[WriteOnly]
		public NativeParallelHashMap<int, SectionSurface> NewSurfaceMap;

		[WriteOnly]
		public NativeParallelHashMap<int, WalkableOctantData> NewWalkableOctantDataMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int> NewSectionMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int> NewAreaGraph;

		[WriteOnly]
		public NativeArray<Octant> NewOctants;

		public void Execute()
		{
			foreach (KeyValue<int, Edge> item in EdgeMap)
			{
				NewEdgeMap.Add(item.Key, item.Value);
			}
			foreach (KeyValue<int, GraphNode> item2 in NodeMap)
			{
				NewNodeMap.Add(item2.Key, item2.Value);
			}
			foreach (KeyValue<int, SectionSurface> item3 in SurfaceMap)
			{
				NewSurfaceMap.Add(item3.Key, item3.Value);
			}
			foreach (KeyValue<int, WalkableOctantData> item4 in WalkableOctantDataMap)
			{
				NewWalkableOctantDataMap.Add(item4.Key, item4.Value);
			}
			foreach (KeyValue<int, int> item5 in SectionMap)
			{
				NewSectionMap.Add(item5.Key, item5.Value);
			}
			foreach (KeyValue<int, int> item6 in AreaGraph)
			{
				NewAreaGraph.Add(item6.Key, item6.Value);
			}
			for (int i = 0; i < Octants.Length; i++)
			{
				NewOctants[i] = Octants[i];
			}
		}
	}

	[SerializeField]
	private int splitCellDivisions;

	[SerializeField]
	private float minSplitCellAreaPercentage;

	[SerializeField]
	private LayerMask jumpSweepMask;

	[SerializeField]
	private NavMeshModifierVolume volume;

	[SerializeField]
	private NavGraphDebugger debugger;

	private NativeArray<Octant> octants;

	private NativeParallelHashMap<int, SerializableGuid> associatedPropMap;

	private NativeParallelHashMap<int, SlopeNeighbors> slopeMap;

	private Dictionary<SerializableGuid, HashSet<int3>> dynamicCellMap;

	private NativeParallelMultiHashMap<int, Edge> edgeMap;

	private NativeParallelHashMap<int, GraphNode> nodeMap;

	private NativeParallelHashMap<int, SectionSurface> surfaceMap;

	private NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap;

	private NativeParallelHashSet<BidirectionalConnection> thresholdConnections;

	private NativeParallelHashSet<BidirectionalConnection> walkConnections;

	private NativeParallelHashSet<BidirectionalConnection> jumpConnections;

	private NativeParallelHashSet<Connection> dropConnections;

	private NativeParallelMultiHashMap<int, int> sectionMap;

	private NativeParallelMultiHashMap<int, int> islandToSectionMap;

	private NativeParallelMultiHashMap<int, int> areaMap;

	private NativeParallelMultiHashMap<int, int> areaGraph;

	private NativeParallelMultiHashMap<int, int> neighborMap;

	private float falloffHeight;

	private bool ignoreUpdates;

	private bool shouldUpdateGraph;

	private bool discardUpdateResults;

	private bool isUpdatingGraph;

	private Coroutine navMeshRoutine;

	private readonly List<ChangedCell> cellsToUpdate = new List<ChangedCell>();

	private readonly List<SerializableGuid> propsToUpdate = new List<SerializableGuid>();

	public int MaxNumBatches;

	public NavMeshBaker NavMeshBaker { get; set; }

	public NavMeshModifierVolume FalloffVolume => volume;

	public static int SplitCellDivisions => MonoBehaviourSingleton<NavGraph>.Instance.splitCellDivisions;

	public static float MinSplitCellAreaPercentage => MonoBehaviourSingleton<NavGraph>.Instance.minSplitCellAreaPercentage;

	protected override void Start()
	{
		base.Start();
		NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
		NetClock.Register(this);
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayLoadingStateReadyToEnd.AddListener(BuildNavigationGraph);
		MaxNumBatches = Math.Max(1, Mathf.CeilToInt((float)JobsUtility.JobWorkerCount * 0.75f));
	}

	private void LateUpdate()
	{
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && math.abs(falloffHeight - MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight) > 1f)
		{
			AdjustFalloff();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DisposeCollections();
	}

	public void EndlessStart()
	{
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.DepthPlane)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.DepthPlane.OnWaterLevelReached.AddListener(HandleWaterLevelReached);
		}
	}

	public void SimulateFrameLate(uint frame)
	{
		if (base.IsServer && shouldUpdateGraph && !isUpdatingGraph)
		{
			shouldUpdateGraph = false;
			isUpdatingGraph = true;
			List<ChangedCell> updateCells = new List<ChangedCell>(cellsToUpdate);
			HashSet<SerializableGuid> changedProps = new HashSet<SerializableGuid>(propsToUpdate);
			cellsToUpdate.Clear();
			propsToUpdate.Clear();
			NavMeshBaker.UpdateNavMesh(delegate
			{
				UpdateConditionalOctants(changedProps, updateCells);
			});
		}
	}

	public void EndlessGameEnd()
	{
		ignoreUpdates = true;
		shouldUpdateGraph = false;
		cellsToUpdate.Clear();
		propsToUpdate.Clear();
		if (!isUpdatingGraph)
		{
			DisposeCollections();
			NavMesh.RemoveAllNavMeshData();
		}
		else
		{
			discardUpdateResults = true;
		}
	}

	public void PropStateChanged(WorldObject worldObject, bool isBlocking)
	{
		if (ignoreUpdates)
		{
			return;
		}
		shouldUpdateGraph = true;
		propsToUpdate.Add(worldObject.InstanceId);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.BuildSourceTracker.AddPropToSources(worldObject.gameObject);
		foreach (int3 item in dynamicCellMap[worldObject.InstanceId])
		{
			cellsToUpdate.Add(new ChangedCell
			{
				Cell = item,
				WorldObject = worldObject,
				IsBlocking = isBlocking
			});
		}
	}

	private void BuildNavigationGraph(BlockTokenCollection blockTokenCollection)
	{
		StartCoroutine(BuildNavigationGraphRoutine(blockTokenCollection));
	}

	internal IEnumerator BuildNavigationGraphRoutine(BlockTokenCollection blockTokenCollection)
	{
		if (!base.IsServer)
		{
			yield break;
		}
		MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("BuildNavigationGraphRoutine");
		BlockToken blockToken = blockTokenCollection.RequestBlockToken();
		if (navMeshRoutine != null)
		{
			yield return navMeshRoutine;
		}
		discardUpdateResults = false;
		ignoreUpdates = false;
		NavMeshObstacle[] obstacles = UnityEngine.Object.FindObjectsByType<NavMeshObstacle>(FindObjectsSortMode.None);
		NavMeshObstacle[] array = obstacles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].carving = false;
		}
		ModifyVolume();
		falloffHeight = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight;
		MonoBehaviourSingleton<CameraController>.Instance.GameplayCameraBrain.enabled = false;
		yield return NavMeshBaker.BuildInitialNavMesh();
		yield return NavGraphGenerator.GenerateNavGraph(OnNavGraphGenerated);
		nodeMap = new NativeParallelHashMap<int, GraphNode>(surfaceMap.Count(), AllocatorManager.Persistent);
		int num = walkConnections.Count() * 2;
		int num2 = jumpConnections.Count() * 2;
		int num3 = dropConnections.Count();
		edgeMap = new NativeParallelMultiHashMap<int, Edge>(num + num2 + num3, AllocatorManager.Persistent);
		BuildPathfindingGraphJob jobData = new BuildPathfindingGraphJob
		{
			SectionToSurfaceMap = surfaceMap,
			SectionMap = sectionMap,
			WalkableOctantData = walkableOctantDataMap,
			ThresholdConnections = thresholdConnections,
			WalkConnections = walkConnections,
			JumpConnections = jumpConnections,
			DropConnections = dropConnections,
			EdgeMap = edgeMap,
			NodeMap = nodeMap
		};
		CleanJumpConnections jobData2 = new CleanJumpConnections
		{
			EdgeMap = edgeMap,
			NodeMap = nodeMap
		};
		JobHandle dependsOn = jobData.Schedule();
		dependsOn = jobData2.Schedule(dependsOn);
		dependsOn = walkConnections.Dispose(dependsOn);
		dependsOn = jumpConnections.Dispose(dependsOn);
		dependsOn = dropConnections.Dispose(dependsOn);
		dependsOn = thresholdConnections.Dispose(dependsOn);
		yield return JobUtilities.WaitForJobToComplete(dependsOn);
		HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
		List<ChangedCell> list = new List<ChangedCell>();
		if (!MonoBehaviourSingleton<StageManager>.Instance || !MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
		{
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("BuildNavigationGraphRoutine");
			DisposeCollections();
			yield break;
		}
		foreach (Cell cell in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.Cells)
		{
			if (!(cell is PropCell propCell) || !hashSet.Add(propCell.InstanceId))
			{
				continue;
			}
			WorldObject componentInChildren = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propCell.InstanceId).GetComponentInChildren<WorldObject>();
			if (componentInChildren.TryGetUserComponent(typeof(Door), out var _))
			{
				foreach (int3 item in dynamicCellMap[propCell.InstanceId])
				{
					list.Add(new ChangedCell
					{
						Cell = item,
						IsBlocking = true,
						WorldObject = componentInChildren
					});
				}
			}
			else
			{
				if (!componentInChildren.TryGetUserComponent(typeof(DynamicNavigationComponent), out var component2))
				{
					continue;
				}
				DynamicNavigationComponent dynamicNavigationComponent = (DynamicNavigationComponent)component2;
				foreach (int3 item2 in dynamicCellMap[propCell.InstanceId])
				{
					list.Add(new ChangedCell
					{
						Cell = item2,
						IsBlocking = dynamicNavigationComponent.StartsBlocking,
						WorldObject = componentInChildren
					});
				}
			}
		}
		yield return NavGraphUpdateGenerator.UpdateNavGraph(list, octants, slopeMap, associatedPropMap, walkableOctantDataMap, nodeMap, edgeMap, areaMap, surfaceMap, sectionMap, islandToSectionMap, areaGraph);
		NativeParallelMultiHashMap<int, Edge> edgeMapCopy = new NativeParallelMultiHashMap<int, Edge>(edgeMap.Count(), AllocatorManager.Persistent);
		NativeParallelHashMap<int, GraphNode> nodeMapCopy = new NativeParallelHashMap<int, GraphNode>(nodeMap.Count(), AllocatorManager.Persistent);
		NativeParallelHashMap<int, SectionSurface> surfaceMapCopy = new NativeParallelHashMap<int, SectionSurface>(surfaceMap.Count(), AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> areaGraphCopy = new NativeParallelMultiHashMap<int, int>(areaGraph.Count(), AllocatorManager.Persistent);
		NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMapCopy = new NativeParallelHashMap<int, WalkableOctantData>(walkableOctantDataMap.Count(), AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> sectionMapCopy = new NativeParallelMultiHashMap<int, int>(sectionMap.Count(), AllocatorManager.Persistent);
		NativeArray<Octant> octantsCopy = new NativeArray<Octant>(octants.Length, Allocator.Persistent);
		NativeParallelHashMap<int, SerializableGuid> propMapCopy = new NativeParallelHashMap<int, SerializableGuid>(associatedPropMap.Count(), AllocatorManager.Persistent);
		JobHandle handle = new CopyNativeCollectionsJob
		{
			EdgeMap = edgeMap,
			NodeMap = nodeMap,
			SurfaceMap = surfaceMap,
			WalkableOctantDataMap = walkableOctantDataMap,
			SectionMap = sectionMap,
			AreaGraph = areaGraph,
			PropMap = associatedPropMap,
			Octants = octants,
			NewEdgeMap = edgeMapCopy,
			NewNodeMap = nodeMapCopy,
			NewSurfaceMap = surfaceMapCopy,
			NewWalkableOctantDataMap = walkableOctantDataMapCopy,
			NewSectionMap = sectionMapCopy,
			NewAreaGraph = areaGraphCopy,
			NewOctants = octantsCopy,
			NewPropMap = propMapCopy
		}.Schedule();
		yield return JobUtilities.WaitForJobToComplete(handle);
		MonoBehaviourSingleton<Pathfinding>.Instance.InitializePathfindingCollections(edgeMapCopy, nodeMapCopy, surfaceMapCopy, sectionMapCopy, areaGraphCopy, walkableOctantDataMapCopy, octantsCopy, propMapCopy);
		debugger.SetCollections(octants, sectionMap, walkableOctantDataMap, surfaceMap, islandToSectionMap, areaMap, edgeMap);
		array = obstacles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].carving = true;
		}
		blockToken.Release();
		MonoBehaviourSingleton<CameraController>.Instance.GameplayCameraBrain.enabled = true;
		MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("BuildNavigationGraphRoutine");
	}

	private void OnNavGraphGenerated(NavGraphGenerator.Results results)
	{
		octants = results.Octants;
		associatedPropMap = results.AssociatedPropMap;
		slopeMap = results.SlopeMap;
		dynamicCellMap = results.DynamicCellMap;
		sectionMap = results.SectionMap;
		surfaceMap = results.SurfaceMap;
		walkConnections = results.NativeWalkConnections;
		jumpConnections = results.NativeJumpConnections;
		dropConnections = results.NativeDropConnections;
		walkableOctantDataMap = results.WalkableOctantDataMap;
		islandToSectionMap = results.IslandToSectionMap;
		neighborMap = results.NeighborMap;
		areaMap = results.AreaMap;
		areaGraph = results.AreaGraph;
		thresholdConnections = results.ThresholdConnections;
	}

	private void DisposeCollections()
	{
		if (octants.IsCreated)
		{
			octants.Dispose();
			associatedPropMap.Dispose();
			slopeMap.Dispose();
			nodeMap.Dispose();
			edgeMap.Dispose();
			sectionMap.Dispose();
			surfaceMap.Dispose();
			walkableOctantDataMap.Dispose();
			islandToSectionMap.Dispose();
			neighborMap.Dispose();
			areaMap.Dispose();
			areaGraph.Dispose();
			if (walkConnections.IsCreated)
			{
				thresholdConnections.Dispose();
				walkConnections.Dispose();
				jumpConnections.Dispose();
				dropConnections.Dispose();
			}
		}
	}

	private void UpdateConditionalOctants(HashSet<SerializableGuid> changedProps, List<ChangedCell> cells)
	{
		navMeshRoutine = StartCoroutine(UpdateConditionalOctantsRoutine(changedProps, cells));
	}

	private IEnumerator UpdateConditionalOctantsRoutine(HashSet<SerializableGuid> changedProps, List<ChangedCell> cells)
	{
		yield return NavGraphUpdateGenerator.UpdateNavGraph(cells, octants, slopeMap, associatedPropMap, walkableOctantDataMap, nodeMap, edgeMap, areaMap, surfaceMap, sectionMap, islandToSectionMap, areaGraph);
		NativeParallelMultiHashMap<int, Edge> edgeMapCopy = new NativeParallelMultiHashMap<int, Edge>(edgeMap.Count(), AllocatorManager.Persistent);
		NativeParallelHashMap<int, GraphNode> nodeMapCopy = new NativeParallelHashMap<int, GraphNode>(nodeMap.Count(), AllocatorManager.Persistent);
		NativeParallelHashMap<int, SectionSurface> surfaceMapCopy = new NativeParallelHashMap<int, SectionSurface>(surfaceMap.Count(), AllocatorManager.Persistent);
		NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMapCopy = new NativeParallelHashMap<int, WalkableOctantData>(walkableOctantDataMap.Count(), AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> sectionMapCopy = new NativeParallelMultiHashMap<int, int>(sectionMap.Count(), AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> areaGraphCopy = new NativeParallelMultiHashMap<int, int>(areaGraph.Count(), AllocatorManager.Persistent);
		NativeArray<Octant> octantsCopy = new NativeArray<Octant>(octants.Length, Allocator.Persistent);
		JobHandle handle = new CopyNativeCollectionsGameplayJob
		{
			EdgeMap = edgeMap,
			NodeMap = nodeMap,
			SurfaceMap = surfaceMap,
			WalkableOctantDataMap = walkableOctantDataMap,
			SectionMap = sectionMap,
			AreaGraph = areaGraph,
			Octants = octants,
			NewEdgeMap = edgeMapCopy,
			NewNodeMap = nodeMapCopy,
			NewSurfaceMap = surfaceMapCopy,
			NewAreaGraph = areaGraphCopy,
			NewWalkableOctantDataMap = walkableOctantDataMapCopy,
			NewSectionMap = sectionMapCopy,
			NewOctants = octantsCopy
		}.Schedule();
		yield return JobUtilities.WaitForJobToComplete(handle);
		if (discardUpdateResults)
		{
			DisposeCollections();
			edgeMapCopy.Dispose();
			nodeMapCopy.Dispose();
			surfaceMapCopy.Dispose();
			areaGraphCopy.Dispose();
			walkableOctantDataMapCopy.Dispose();
			sectionMapCopy.Dispose();
			octantsCopy.Dispose();
			NavMesh.RemoveAllNavMeshData();
			isUpdatingGraph = false;
			navMeshRoutine = null;
		}
		else
		{
			MonoBehaviourSingleton<Pathfinding>.Instance.UpdatePathfindingCollections(changedProps, edgeMapCopy, nodeMapCopy, surfaceMapCopy, sectionMapCopy, areaGraphCopy, walkableOctantDataMapCopy, octantsCopy);
			debugger.SetCollections(octants, sectionMap, walkableOctantDataMap, surfaceMap, islandToSectionMap, areaMap, edgeMap);
			isUpdatingGraph = false;
			navMeshRoutine = null;
		}
	}

	private void HandleWaterLevelReached(Context _)
	{
		AdjustFalloff();
	}

	private void AdjustFalloff()
	{
		if (base.IsServer)
		{
			ModifyVolume();
			falloffHeight = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight;
			Vector3Int minimumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents;
			Vector3Int maximumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents;
			UnityEngine.Vector3 center = (UnityEngine.Vector3)(minimumExtents + maximumExtents) / 2f;
			UnityEngine.Vector3 size = maximumExtents - minimumExtents;
			size.y = 0f;
			center.y = falloffHeight;
			GridUtilities.DrawDebugCube(center, size, UnityEngine.Color.red, 15f, topOnly: true);
			NavMeshBaker.UpdateNavMesh(delegate
			{
				MonoBehaviourSingleton<Pathfinding>.Instance.NavMeshUpdated();
			});
		}
	}

	private void ModifyVolume()
	{
		Vector3Int minimumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents;
		Vector3Int maximumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents;
		float num = minimumExtents.y;
		float stageFallOffHeight = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight;
		UnityEngine.Vector3 center = new UnityEngine.Vector3((float)(maximumExtents.x + minimumExtents.x) / 2f, (stageFallOffHeight + num) / 2f, (float)(maximumExtents.z + minimumExtents.z) / 2f);
		UnityEngine.Vector3 size = new UnityEngine.Vector3(maximumExtents.x - minimumExtents.x + 1, stageFallOffHeight - num, maximumExtents.z - minimumExtents.z + 1);
		volume.center = center;
		volume.size = size;
	}
}
