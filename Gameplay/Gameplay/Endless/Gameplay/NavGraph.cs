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
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020001F1 RID: 497
	public class NavGraph : EndlessBehaviourSingleton<NavGraph>, NetClock.ISimulateFrameLateSubscriber, IGameEndSubscriber, IStartSubscriber
	{
		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x06000A38 RID: 2616 RVA: 0x000375E8 File Offset: 0x000357E8
		// (set) Token: 0x06000A39 RID: 2617 RVA: 0x000375F0 File Offset: 0x000357F0
		public NavMeshBaker NavMeshBaker { get; set; }

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x06000A3A RID: 2618 RVA: 0x000375F9 File Offset: 0x000357F9
		public NavMeshModifierVolume FalloffVolume
		{
			get
			{
				return this.volume;
			}
		}

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x06000A3B RID: 2619 RVA: 0x00037601 File Offset: 0x00035801
		public static int SplitCellDivisions
		{
			get
			{
				return MonoBehaviourSingleton<NavGraph>.Instance.splitCellDivisions;
			}
		}

		// Token: 0x170001EB RID: 491
		// (get) Token: 0x06000A3C RID: 2620 RVA: 0x0003760D File Offset: 0x0003580D
		public static float MinSplitCellAreaPercentage
		{
			get
			{
				return MonoBehaviourSingleton<NavGraph>.Instance.minSplitCellAreaPercentage;
			}
		}

		// Token: 0x06000A3D RID: 2621 RVA: 0x0003761C File Offset: 0x0003581C
		protected override void Start()
		{
			base.Start();
			NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
			NetClock.Register(this);
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayLoadingStateReadyToEnd.AddListener(new UnityAction<BlockTokenCollection>(this.BuildNavigationGraph));
			this.MaxNumBatches = Math.Max(1, Mathf.CeilToInt((float)JobsUtility.JobWorkerCount * 0.75f));
		}

		// Token: 0x06000A3E RID: 2622 RVA: 0x00037674 File Offset: 0x00035874
		private void LateUpdate()
		{
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage || !MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				return;
			}
			if (math.abs(this.falloffHeight - MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight) > 1f)
			{
				this.AdjustFalloff();
			}
		}

		// Token: 0x06000A3F RID: 2623 RVA: 0x000376C7 File Offset: 0x000358C7
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.DisposeCollections();
		}

		// Token: 0x06000A40 RID: 2624 RVA: 0x000376D5 File Offset: 0x000358D5
		public void EndlessStart()
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.DepthPlane)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.DepthPlane.OnWaterLevelReached.AddListener(new UnityAction<Context>(this.HandleWaterLevelReached));
			}
		}

		// Token: 0x06000A41 RID: 2625 RVA: 0x00037714 File Offset: 0x00035914
		public void SimulateFrameLate(uint frame)
		{
			if (!base.IsServer)
			{
				return;
			}
			if (this.shouldUpdateGraph && !this.isUpdatingGraph)
			{
				this.shouldUpdateGraph = false;
				this.isUpdatingGraph = true;
				List<NavGraph.ChangedCell> updateCells = new List<NavGraph.ChangedCell>(this.cellsToUpdate);
				HashSet<SerializableGuid> changedProps = new HashSet<SerializableGuid>(this.propsToUpdate);
				this.cellsToUpdate.Clear();
				this.propsToUpdate.Clear();
				this.NavMeshBaker.UpdateNavMesh(delegate(AsyncOperation _)
				{
					this.UpdateConditionalOctants(changedProps, updateCells);
				});
			}
		}

		// Token: 0x06000A42 RID: 2626 RVA: 0x000377A4 File Offset: 0x000359A4
		public void EndlessGameEnd()
		{
			this.ignoreUpdates = true;
			this.shouldUpdateGraph = false;
			this.cellsToUpdate.Clear();
			this.propsToUpdate.Clear();
			if (!this.isUpdatingGraph)
			{
				this.DisposeCollections();
				NavMesh.RemoveAllNavMeshData();
				return;
			}
			this.discardUpdateResults = true;
		}

		// Token: 0x06000A43 RID: 2627 RVA: 0x000377F0 File Offset: 0x000359F0
		public void PropStateChanged(WorldObject worldObject, bool isBlocking)
		{
			if (this.ignoreUpdates)
			{
				return;
			}
			this.shouldUpdateGraph = true;
			this.propsToUpdate.Add(worldObject.InstanceId);
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.BuildSourceTracker.AddPropToSources(worldObject.gameObject);
			foreach (int3 @int in this.dynamicCellMap[worldObject.InstanceId])
			{
				this.cellsToUpdate.Add(new NavGraph.ChangedCell
				{
					Cell = @int,
					WorldObject = worldObject,
					IsBlocking = isBlocking
				});
			}
		}

		// Token: 0x06000A44 RID: 2628 RVA: 0x000378B0 File Offset: 0x00035AB0
		private void BuildNavigationGraph(BlockTokenCollection blockTokenCollection)
		{
			base.StartCoroutine(this.BuildNavigationGraphRoutine(blockTokenCollection));
		}

		// Token: 0x06000A45 RID: 2629 RVA: 0x000378C0 File Offset: 0x00035AC0
		internal IEnumerator BuildNavigationGraphRoutine(BlockTokenCollection blockTokenCollection)
		{
			if (!base.IsServer)
			{
				yield break;
			}
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("BuildNavigationGraphRoutine");
			BlockToken blockToken = blockTokenCollection.RequestBlockToken();
			if (this.navMeshRoutine != null)
			{
				yield return this.navMeshRoutine;
			}
			this.discardUpdateResults = false;
			this.ignoreUpdates = false;
			NavMeshObstacle[] obstacles = global::UnityEngine.Object.FindObjectsByType<NavMeshObstacle>(FindObjectsSortMode.None);
			NavMeshObstacle[] array = obstacles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].carving = false;
			}
			this.ModifyVolume();
			this.falloffHeight = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight;
			MonoBehaviourSingleton<CameraController>.Instance.GameplayCameraBrain.enabled = false;
			AsyncOperation asyncOperation = this.NavMeshBaker.BuildInitialNavMesh();
			yield return asyncOperation;
			yield return NavGraphGenerator.GenerateNavGraph(new Action<NavGraphGenerator.Results>(this.OnNavGraphGenerated));
			this.nodeMap = new NativeParallelHashMap<int, GraphNode>(this.surfaceMap.Count(), AllocatorManager.Persistent);
			int num = this.walkConnections.Count() * 2;
			int num2 = this.jumpConnections.Count() * 2;
			int num3 = this.dropConnections.Count();
			this.edgeMap = new NativeParallelMultiHashMap<int, Edge>(num + num2 + num3, AllocatorManager.Persistent);
			NavGraph.BuildPathfindingGraphJob buildPathfindingGraphJob = new NavGraph.BuildPathfindingGraphJob
			{
				SectionToSurfaceMap = this.surfaceMap,
				SectionMap = this.sectionMap,
				WalkableOctantData = this.walkableOctantDataMap,
				ThresholdConnections = this.thresholdConnections,
				WalkConnections = this.walkConnections,
				JumpConnections = this.jumpConnections,
				DropConnections = this.dropConnections,
				EdgeMap = this.edgeMap,
				NodeMap = this.nodeMap
			};
			CleanJumpConnections cleanJumpConnections = new CleanJumpConnections
			{
				EdgeMap = this.edgeMap,
				NodeMap = this.nodeMap
			};
			JobHandle jobHandle = buildPathfindingGraphJob.Schedule(default(JobHandle));
			jobHandle = cleanJumpConnections.Schedule(jobHandle);
			jobHandle = this.walkConnections.Dispose(jobHandle);
			jobHandle = this.jumpConnections.Dispose(jobHandle);
			jobHandle = this.dropConnections.Dispose(jobHandle);
			jobHandle = this.thresholdConnections.Dispose(jobHandle);
			yield return JobUtilities.WaitForJobToComplete(jobHandle, false);
			HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
			List<NavGraph.ChangedCell> list = new List<NavGraph.ChangedCell>();
			if (!MonoBehaviourSingleton<StageManager>.Instance || !MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
			{
				MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("BuildNavigationGraphRoutine");
				this.DisposeCollections();
				yield break;
			}
			foreach (Cell cell in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.Cells)
			{
				PropCell propCell = cell as PropCell;
				if (propCell != null && hashSet.Add(propCell.InstanceId))
				{
					WorldObject componentInChildren = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propCell.InstanceId).GetComponentInChildren<WorldObject>();
					object obj;
					if (componentInChildren.TryGetUserComponent(typeof(Door), out obj))
					{
						using (HashSet<int3>.Enumerator enumerator2 = this.dynamicCellMap[propCell.InstanceId].GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								int3 @int = enumerator2.Current;
								list.Add(new NavGraph.ChangedCell
								{
									Cell = @int,
									IsBlocking = true,
									WorldObject = componentInChildren
								});
							}
							continue;
						}
					}
					object obj2;
					if (componentInChildren.TryGetUserComponent(typeof(DynamicNavigationComponent), out obj2))
					{
						DynamicNavigationComponent dynamicNavigationComponent = (DynamicNavigationComponent)obj2;
						foreach (int3 int2 in this.dynamicCellMap[propCell.InstanceId])
						{
							list.Add(new NavGraph.ChangedCell
							{
								Cell = int2,
								IsBlocking = dynamicNavigationComponent.StartsBlocking,
								WorldObject = componentInChildren
							});
						}
					}
				}
			}
			yield return NavGraphUpdateGenerator.UpdateNavGraph(list, this.octants, this.slopeMap, this.associatedPropMap, this.walkableOctantDataMap, this.nodeMap, this.edgeMap, this.areaMap, this.surfaceMap, this.sectionMap, this.islandToSectionMap, this.areaGraph);
			NativeParallelMultiHashMap<int, Edge> edgeMapCopy = new NativeParallelMultiHashMap<int, Edge>(this.edgeMap.Count(), AllocatorManager.Persistent);
			NativeParallelHashMap<int, GraphNode> nodeMapCopy = new NativeParallelHashMap<int, GraphNode>(this.nodeMap.Count(), AllocatorManager.Persistent);
			NativeParallelHashMap<int, SectionSurface> surfaceMapCopy = new NativeParallelHashMap<int, SectionSurface>(this.surfaceMap.Count(), AllocatorManager.Persistent);
			NativeParallelMultiHashMap<int, int> areaGraphCopy = new NativeParallelMultiHashMap<int, int>(this.areaGraph.Count(), AllocatorManager.Persistent);
			NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMapCopy = new NativeParallelHashMap<int, WalkableOctantData>(this.walkableOctantDataMap.Count(), AllocatorManager.Persistent);
			NativeParallelMultiHashMap<int, int> sectionMapCopy = new NativeParallelMultiHashMap<int, int>(this.sectionMap.Count(), AllocatorManager.Persistent);
			NativeArray<Octant> octantsCopy = new NativeArray<Octant>(this.octants.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			NativeParallelHashMap<int, SerializableGuid> propMapCopy = new NativeParallelHashMap<int, SerializableGuid>(this.associatedPropMap.Count(), AllocatorManager.Persistent);
			JobHandle jobHandle2 = new NavGraph.CopyNativeCollectionsJob
			{
				EdgeMap = this.edgeMap,
				NodeMap = this.nodeMap,
				SurfaceMap = this.surfaceMap,
				WalkableOctantDataMap = this.walkableOctantDataMap,
				SectionMap = this.sectionMap,
				AreaGraph = this.areaGraph,
				PropMap = this.associatedPropMap,
				Octants = this.octants,
				NewEdgeMap = edgeMapCopy,
				NewNodeMap = nodeMapCopy,
				NewSurfaceMap = surfaceMapCopy,
				NewWalkableOctantDataMap = walkableOctantDataMapCopy,
				NewSectionMap = sectionMapCopy,
				NewAreaGraph = areaGraphCopy,
				NewOctants = octantsCopy,
				NewPropMap = propMapCopy
			}.Schedule(default(JobHandle));
			yield return JobUtilities.WaitForJobToComplete(jobHandle2, false);
			MonoBehaviourSingleton<Pathfinding>.Instance.InitializePathfindingCollections(edgeMapCopy, nodeMapCopy, surfaceMapCopy, sectionMapCopy, areaGraphCopy, walkableOctantDataMapCopy, octantsCopy, propMapCopy);
			this.debugger.SetCollections(this.octants, this.sectionMap, this.walkableOctantDataMap, this.surfaceMap, this.islandToSectionMap, this.areaMap, this.edgeMap);
			array = obstacles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].carving = true;
			}
			blockToken.Release();
			MonoBehaviourSingleton<CameraController>.Instance.GameplayCameraBrain.enabled = true;
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("BuildNavigationGraphRoutine");
			yield break;
		}

		// Token: 0x06000A46 RID: 2630 RVA: 0x000378D8 File Offset: 0x00035AD8
		private void OnNavGraphGenerated(NavGraphGenerator.Results results)
		{
			this.octants = results.Octants;
			this.associatedPropMap = results.AssociatedPropMap;
			this.slopeMap = results.SlopeMap;
			this.dynamicCellMap = results.DynamicCellMap;
			this.sectionMap = results.SectionMap;
			this.surfaceMap = results.SurfaceMap;
			this.walkConnections = results.NativeWalkConnections;
			this.jumpConnections = results.NativeJumpConnections;
			this.dropConnections = results.NativeDropConnections;
			this.walkableOctantDataMap = results.WalkableOctantDataMap;
			this.islandToSectionMap = results.IslandToSectionMap;
			this.neighborMap = results.NeighborMap;
			this.areaMap = results.AreaMap;
			this.areaGraph = results.AreaGraph;
			this.thresholdConnections = results.ThresholdConnections;
		}

		// Token: 0x06000A47 RID: 2631 RVA: 0x0003799C File Offset: 0x00035B9C
		private void DisposeCollections()
		{
			if (!this.octants.IsCreated)
			{
				return;
			}
			this.octants.Dispose();
			this.associatedPropMap.Dispose();
			this.slopeMap.Dispose();
			this.nodeMap.Dispose();
			this.edgeMap.Dispose();
			this.sectionMap.Dispose();
			this.surfaceMap.Dispose();
			this.walkableOctantDataMap.Dispose();
			this.islandToSectionMap.Dispose();
			this.neighborMap.Dispose();
			this.areaMap.Dispose();
			this.areaGraph.Dispose();
			if (!this.walkConnections.IsCreated)
			{
				return;
			}
			this.thresholdConnections.Dispose();
			this.walkConnections.Dispose();
			this.jumpConnections.Dispose();
			this.dropConnections.Dispose();
		}

		// Token: 0x06000A48 RID: 2632 RVA: 0x00037A75 File Offset: 0x00035C75
		private void UpdateConditionalOctants(HashSet<SerializableGuid> changedProps, List<NavGraph.ChangedCell> cells)
		{
			this.navMeshRoutine = base.StartCoroutine(this.UpdateConditionalOctantsRoutine(changedProps, cells));
		}

		// Token: 0x06000A49 RID: 2633 RVA: 0x00037A8B File Offset: 0x00035C8B
		private IEnumerator UpdateConditionalOctantsRoutine(HashSet<SerializableGuid> changedProps, List<NavGraph.ChangedCell> cells)
		{
			yield return NavGraphUpdateGenerator.UpdateNavGraph(cells, this.octants, this.slopeMap, this.associatedPropMap, this.walkableOctantDataMap, this.nodeMap, this.edgeMap, this.areaMap, this.surfaceMap, this.sectionMap, this.islandToSectionMap, this.areaGraph);
			NativeParallelMultiHashMap<int, Edge> edgeMapCopy = new NativeParallelMultiHashMap<int, Edge>(this.edgeMap.Count(), AllocatorManager.Persistent);
			NativeParallelHashMap<int, GraphNode> nodeMapCopy = new NativeParallelHashMap<int, GraphNode>(this.nodeMap.Count(), AllocatorManager.Persistent);
			NativeParallelHashMap<int, SectionSurface> surfaceMapCopy = new NativeParallelHashMap<int, SectionSurface>(this.surfaceMap.Count(), AllocatorManager.Persistent);
			NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMapCopy = new NativeParallelHashMap<int, WalkableOctantData>(this.walkableOctantDataMap.Count(), AllocatorManager.Persistent);
			NativeParallelMultiHashMap<int, int> sectionMapCopy = new NativeParallelMultiHashMap<int, int>(this.sectionMap.Count(), AllocatorManager.Persistent);
			NativeParallelMultiHashMap<int, int> areaGraphCopy = new NativeParallelMultiHashMap<int, int>(this.areaGraph.Count(), AllocatorManager.Persistent);
			NativeArray<Octant> octantsCopy = new NativeArray<Octant>(this.octants.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			JobHandle jobHandle = new NavGraph.CopyNativeCollectionsGameplayJob
			{
				EdgeMap = this.edgeMap,
				NodeMap = this.nodeMap,
				SurfaceMap = this.surfaceMap,
				WalkableOctantDataMap = this.walkableOctantDataMap,
				SectionMap = this.sectionMap,
				AreaGraph = this.areaGraph,
				Octants = this.octants,
				NewEdgeMap = edgeMapCopy,
				NewNodeMap = nodeMapCopy,
				NewSurfaceMap = surfaceMapCopy,
				NewAreaGraph = areaGraphCopy,
				NewWalkableOctantDataMap = walkableOctantDataMapCopy,
				NewSectionMap = sectionMapCopy,
				NewOctants = octantsCopy
			}.Schedule(default(JobHandle));
			yield return JobUtilities.WaitForJobToComplete(jobHandle, false);
			if (this.discardUpdateResults)
			{
				this.DisposeCollections();
				edgeMapCopy.Dispose();
				nodeMapCopy.Dispose();
				surfaceMapCopy.Dispose();
				areaGraphCopy.Dispose();
				walkableOctantDataMapCopy.Dispose();
				sectionMapCopy.Dispose();
				octantsCopy.Dispose();
				NavMesh.RemoveAllNavMeshData();
				this.isUpdatingGraph = false;
				this.navMeshRoutine = null;
				yield break;
			}
			MonoBehaviourSingleton<Pathfinding>.Instance.UpdatePathfindingCollections(changedProps, edgeMapCopy, nodeMapCopy, surfaceMapCopy, sectionMapCopy, areaGraphCopy, walkableOctantDataMapCopy, octantsCopy);
			this.debugger.SetCollections(this.octants, this.sectionMap, this.walkableOctantDataMap, this.surfaceMap, this.islandToSectionMap, this.areaMap, this.edgeMap);
			this.isUpdatingGraph = false;
			this.navMeshRoutine = null;
			yield break;
		}

		// Token: 0x06000A4A RID: 2634 RVA: 0x00037AA8 File Offset: 0x00035CA8
		private void HandleWaterLevelReached(Context _)
		{
			this.AdjustFalloff();
		}

		// Token: 0x06000A4B RID: 2635 RVA: 0x00037AB0 File Offset: 0x00035CB0
		private void AdjustFalloff()
		{
			if (!base.IsServer)
			{
				return;
			}
			this.ModifyVolume();
			this.falloffHeight = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight;
			Vector3Int minimumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents;
			Vector3Int maximumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents;
			global::UnityEngine.Vector3 vector = (minimumExtents + maximumExtents) / 2f;
			global::UnityEngine.Vector3 vector2 = maximumExtents - minimumExtents;
			vector2.y = 0f;
			vector.y = this.falloffHeight;
			GridUtilities.DrawDebugCube(vector, vector2, global::UnityEngine.Color.red, 15f, true, false);
			this.NavMeshBaker.UpdateNavMesh(delegate(AsyncOperation _)
			{
				MonoBehaviourSingleton<Pathfinding>.Instance.NavMeshUpdated();
			});
		}

		// Token: 0x06000A4C RID: 2636 RVA: 0x00037B7C File Offset: 0x00035D7C
		private void ModifyVolume()
		{
			Vector3Int minimumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents;
			Vector3Int maximumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents;
			float num = (float)minimumExtents.y;
			float stageFallOffHeight = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight;
			global::UnityEngine.Vector3 vector = new global::UnityEngine.Vector3((float)(maximumExtents.x + minimumExtents.x) / 2f, (stageFallOffHeight + num) / 2f, (float)(maximumExtents.z + minimumExtents.z) / 2f);
			global::UnityEngine.Vector3 vector2 = new global::UnityEngine.Vector3((float)(maximumExtents.x - minimumExtents.x + 1), stageFallOffHeight - num, (float)(maximumExtents.z - minimumExtents.z + 1));
			this.volume.center = vector;
			this.volume.size = vector2;
		}

		// Token: 0x0400097B RID: 2427
		[SerializeField]
		private int splitCellDivisions;

		// Token: 0x0400097C RID: 2428
		[SerializeField]
		private float minSplitCellAreaPercentage;

		// Token: 0x0400097D RID: 2429
		[SerializeField]
		private LayerMask jumpSweepMask;

		// Token: 0x0400097E RID: 2430
		[SerializeField]
		private NavMeshModifierVolume volume;

		// Token: 0x0400097F RID: 2431
		[SerializeField]
		private NavGraphDebugger debugger;

		// Token: 0x04000980 RID: 2432
		private NativeArray<Octant> octants;

		// Token: 0x04000981 RID: 2433
		private NativeParallelHashMap<int, SerializableGuid> associatedPropMap;

		// Token: 0x04000982 RID: 2434
		private NativeParallelHashMap<int, SlopeNeighbors> slopeMap;

		// Token: 0x04000983 RID: 2435
		private Dictionary<SerializableGuid, HashSet<int3>> dynamicCellMap;

		// Token: 0x04000984 RID: 2436
		private NativeParallelMultiHashMap<int, Edge> edgeMap;

		// Token: 0x04000985 RID: 2437
		private NativeParallelHashMap<int, GraphNode> nodeMap;

		// Token: 0x04000986 RID: 2438
		private NativeParallelHashMap<int, SectionSurface> surfaceMap;

		// Token: 0x04000987 RID: 2439
		private NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap;

		// Token: 0x04000988 RID: 2440
		private NativeParallelHashSet<BidirectionalConnection> thresholdConnections;

		// Token: 0x04000989 RID: 2441
		private NativeParallelHashSet<BidirectionalConnection> walkConnections;

		// Token: 0x0400098A RID: 2442
		private NativeParallelHashSet<BidirectionalConnection> jumpConnections;

		// Token: 0x0400098B RID: 2443
		private NativeParallelHashSet<Connection> dropConnections;

		// Token: 0x0400098C RID: 2444
		private NativeParallelMultiHashMap<int, int> sectionMap;

		// Token: 0x0400098D RID: 2445
		private NativeParallelMultiHashMap<int, int> islandToSectionMap;

		// Token: 0x0400098E RID: 2446
		private NativeParallelMultiHashMap<int, int> areaMap;

		// Token: 0x0400098F RID: 2447
		private NativeParallelMultiHashMap<int, int> areaGraph;

		// Token: 0x04000990 RID: 2448
		private NativeParallelMultiHashMap<int, int> neighborMap;

		// Token: 0x04000992 RID: 2450
		private float falloffHeight;

		// Token: 0x04000993 RID: 2451
		private bool ignoreUpdates;

		// Token: 0x04000994 RID: 2452
		private bool shouldUpdateGraph;

		// Token: 0x04000995 RID: 2453
		private bool discardUpdateResults;

		// Token: 0x04000996 RID: 2454
		private bool isUpdatingGraph;

		// Token: 0x04000997 RID: 2455
		private Coroutine navMeshRoutine;

		// Token: 0x04000998 RID: 2456
		private readonly List<NavGraph.ChangedCell> cellsToUpdate = new List<NavGraph.ChangedCell>();

		// Token: 0x04000999 RID: 2457
		private readonly List<SerializableGuid> propsToUpdate = new List<SerializableGuid>();

		// Token: 0x0400099A RID: 2458
		public int MaxNumBatches;

		// Token: 0x020001F2 RID: 498
		public struct ChangedCell
		{
			// Token: 0x0400099B RID: 2459
			public int3 Cell;

			// Token: 0x0400099C RID: 2460
			public WorldObject WorldObject;

			// Token: 0x0400099D RID: 2461
			public bool IsBlocking;
		}

		// Token: 0x020001F3 RID: 499
		[BurstCompile]
		private struct BuildPathfindingGraphJob : IJob
		{
			// Token: 0x06000A4E RID: 2638 RVA: 0x00037C64 File Offset: 0x00035E64
			public void Execute()
			{
				foreach (KeyValue<int, SectionSurface> keyValue in this.SectionToSurfaceMap)
				{
					int num;
					NativeParallelMultiHashMapIterator<int> nativeParallelMultiHashMapIterator;
					if (!this.SectionMap.TryGetFirstValue(keyValue.Key, out num, out nativeParallelMultiHashMapIterator))
					{
						throw new Exception("Section without any octants found. This shouldn't be possible");
					}
					WalkableOctantData walkableOctantData = this.WalkableOctantData[num];
					this.NodeMap.Add(keyValue.Key, new GraphNode
					{
						IslandKey = walkableOctantData.IslandKey,
						AreaKey = walkableOctantData.AreaKey,
						ZoneKey = walkableOctantData.ZoneKey,
						Key = keyValue.Key,
						Center = keyValue.Value.Center
					});
				}
				foreach (BidirectionalConnection bidirectionalConnection in this.ThresholdConnections)
				{
					this.AddConnection(bidirectionalConnection.SectionIndexA, bidirectionalConnection.SectionIndexB, ConnectionKind.Threshold, 1f);
					this.AddConnection(bidirectionalConnection.SectionIndexB, bidirectionalConnection.SectionIndexA, ConnectionKind.Threshold, 1f);
				}
				foreach (BidirectionalConnection bidirectionalConnection2 in this.WalkConnections)
				{
					this.AddConnection(bidirectionalConnection2.SectionIndexA, bidirectionalConnection2.SectionIndexB, ConnectionKind.Walk, 1f);
					this.AddConnection(bidirectionalConnection2.SectionIndexB, bidirectionalConnection2.SectionIndexA, ConnectionKind.Walk, 1f);
				}
				foreach (BidirectionalConnection bidirectionalConnection3 in this.JumpConnections)
				{
					this.AddConnection(bidirectionalConnection3.SectionIndexA, bidirectionalConnection3.SectionIndexB, ConnectionKind.Jump, NpcMovementValues.JumpCostScalar);
					this.AddConnection(bidirectionalConnection3.SectionIndexB, bidirectionalConnection3.SectionIndexA, ConnectionKind.Jump, NpcMovementValues.JumpCostScalar);
				}
				foreach (Connection connection in this.DropConnections)
				{
					this.AddConnection(connection.StartSectionKey, connection.EndSectionKey, ConnectionKind.Dropdown, NpcMovementValues.DropCostScalar);
				}
			}

			// Token: 0x06000A4F RID: 2639 RVA: 0x00037EF4 File Offset: 0x000360F4
			private void AddConnection(int startSectionIndex, int endSectionIndex, ConnectionKind connectionKind, float costScalar = 1f)
			{
				float3 center = this.SectionToSurfaceMap[startSectionIndex].Center;
				float3 center2 = this.SectionToSurfaceMap[endSectionIndex].Center;
				float num = math.distance(center, center2) * costScalar;
				Edge edge = new Edge
				{
					Connection = connectionKind,
					ConnectedNodeKey = endSectionIndex,
					Cost = num
				};
				this.EdgeMap.Add(startSectionIndex, edge);
			}

			// Token: 0x0400099E RID: 2462
			[ReadOnly]
			public NativeParallelHashMap<int, SectionSurface> SectionToSurfaceMap;

			// Token: 0x0400099F RID: 2463
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x040009A0 RID: 2464
			[ReadOnly]
			public NativeParallelHashSet<BidirectionalConnection> ThresholdConnections;

			// Token: 0x040009A1 RID: 2465
			[ReadOnly]
			public NativeParallelHashSet<BidirectionalConnection> WalkConnections;

			// Token: 0x040009A2 RID: 2466
			[ReadOnly]
			public NativeParallelHashSet<BidirectionalConnection> JumpConnections;

			// Token: 0x040009A3 RID: 2467
			[ReadOnly]
			public NativeParallelHashSet<Connection> DropConnections;

			// Token: 0x040009A4 RID: 2468
			[ReadOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantData;

			// Token: 0x040009A5 RID: 2469
			[WriteOnly]
			public NativeParallelMultiHashMap<int, Edge> EdgeMap;

			// Token: 0x040009A6 RID: 2470
			[WriteOnly]
			public NativeParallelHashMap<int, GraphNode> NodeMap;
		}

		// Token: 0x020001F4 RID: 500
		[BurstCompile]
		private struct CopyNativeCollectionsJob : IJob
		{
			// Token: 0x06000A50 RID: 2640 RVA: 0x00037F64 File Offset: 0x00036164
			public unsafe void Execute()
			{
				foreach (KeyValue<int, Edge> keyValue in this.EdgeMap)
				{
					this.NewEdgeMap.Add(keyValue.Key, *keyValue.Value);
				}
				foreach (KeyValue<int, GraphNode> keyValue2 in this.NodeMap)
				{
					this.NewNodeMap.Add(keyValue2.Key, *keyValue2.Value);
				}
				foreach (KeyValue<int, SectionSurface> keyValue3 in this.SurfaceMap)
				{
					this.NewSurfaceMap.Add(keyValue3.Key, *keyValue3.Value);
				}
				foreach (KeyValue<int, WalkableOctantData> keyValue4 in this.WalkableOctantDataMap)
				{
					this.NewWalkableOctantDataMap.Add(keyValue4.Key, *keyValue4.Value);
				}
				foreach (KeyValue<int, int> keyValue5 in this.SectionMap)
				{
					this.NewSectionMap.Add(keyValue5.Key, *keyValue5.Value);
				}
				foreach (KeyValue<int, int> keyValue6 in this.AreaGraph)
				{
					this.NewAreaGraph.Add(keyValue6.Key, *keyValue6.Value);
				}
				foreach (KeyValue<int, SerializableGuid> keyValue7 in this.PropMap)
				{
					this.NewPropMap.Add(keyValue7.Key, *keyValue7.Value);
				}
				for (int i = 0; i < this.Octants.Length; i++)
				{
					this.NewOctants[i] = this.Octants[i];
				}
			}

			// Token: 0x040009A7 RID: 2471
			[ReadOnly]
			public NativeParallelMultiHashMap<int, Edge> EdgeMap;

			// Token: 0x040009A8 RID: 2472
			[ReadOnly]
			public NativeParallelHashMap<int, GraphNode> NodeMap;

			// Token: 0x040009A9 RID: 2473
			[ReadOnly]
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x040009AA RID: 2474
			[ReadOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x040009AB RID: 2475
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x040009AC RID: 2476
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> AreaGraph;

			// Token: 0x040009AD RID: 2477
			[ReadOnly]
			public NativeParallelHashMap<int, SerializableGuid> PropMap;

			// Token: 0x040009AE RID: 2478
			[ReadOnly]
			public NativeArray<Octant> Octants;

			// Token: 0x040009AF RID: 2479
			[WriteOnly]
			public NativeParallelMultiHashMap<int, Edge> NewEdgeMap;

			// Token: 0x040009B0 RID: 2480
			[WriteOnly]
			public NativeParallelHashMap<int, GraphNode> NewNodeMap;

			// Token: 0x040009B1 RID: 2481
			[WriteOnly]
			public NativeParallelHashMap<int, SectionSurface> NewSurfaceMap;

			// Token: 0x040009B2 RID: 2482
			[WriteOnly]
			public NativeParallelHashMap<int, WalkableOctantData> NewWalkableOctantDataMap;

			// Token: 0x040009B3 RID: 2483
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int> NewSectionMap;

			// Token: 0x040009B4 RID: 2484
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int> NewAreaGraph;

			// Token: 0x040009B5 RID: 2485
			[WriteOnly]
			public NativeParallelHashMap<int, SerializableGuid> NewPropMap;

			// Token: 0x040009B6 RID: 2486
			[WriteOnly]
			public NativeArray<Octant> NewOctants;
		}

		// Token: 0x020001F5 RID: 501
		[BurstCompile]
		private struct CopyNativeCollectionsGameplayJob : IJob
		{
			// Token: 0x06000A51 RID: 2641 RVA: 0x0003821C File Offset: 0x0003641C
			public unsafe void Execute()
			{
				foreach (KeyValue<int, Edge> keyValue in this.EdgeMap)
				{
					this.NewEdgeMap.Add(keyValue.Key, *keyValue.Value);
				}
				foreach (KeyValue<int, GraphNode> keyValue2 in this.NodeMap)
				{
					this.NewNodeMap.Add(keyValue2.Key, *keyValue2.Value);
				}
				foreach (KeyValue<int, SectionSurface> keyValue3 in this.SurfaceMap)
				{
					this.NewSurfaceMap.Add(keyValue3.Key, *keyValue3.Value);
				}
				foreach (KeyValue<int, WalkableOctantData> keyValue4 in this.WalkableOctantDataMap)
				{
					this.NewWalkableOctantDataMap.Add(keyValue4.Key, *keyValue4.Value);
				}
				foreach (KeyValue<int, int> keyValue5 in this.SectionMap)
				{
					this.NewSectionMap.Add(keyValue5.Key, *keyValue5.Value);
				}
				foreach (KeyValue<int, int> keyValue6 in this.AreaGraph)
				{
					this.NewAreaGraph.Add(keyValue6.Key, *keyValue6.Value);
				}
				for (int i = 0; i < this.Octants.Length; i++)
				{
					this.NewOctants[i] = this.Octants[i];
				}
			}

			// Token: 0x040009B7 RID: 2487
			[ReadOnly]
			public NativeParallelMultiHashMap<int, Edge> EdgeMap;

			// Token: 0x040009B8 RID: 2488
			[ReadOnly]
			public NativeParallelHashMap<int, GraphNode> NodeMap;

			// Token: 0x040009B9 RID: 2489
			[ReadOnly]
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x040009BA RID: 2490
			[ReadOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x040009BB RID: 2491
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x040009BC RID: 2492
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> AreaGraph;

			// Token: 0x040009BD RID: 2493
			[ReadOnly]
			public NativeArray<Octant> Octants;

			// Token: 0x040009BE RID: 2494
			[WriteOnly]
			public NativeParallelMultiHashMap<int, Edge> NewEdgeMap;

			// Token: 0x040009BF RID: 2495
			[WriteOnly]
			public NativeParallelHashMap<int, GraphNode> NewNodeMap;

			// Token: 0x040009C0 RID: 2496
			[WriteOnly]
			public NativeParallelHashMap<int, SectionSurface> NewSurfaceMap;

			// Token: 0x040009C1 RID: 2497
			[WriteOnly]
			public NativeParallelHashMap<int, WalkableOctantData> NewWalkableOctantDataMap;

			// Token: 0x040009C2 RID: 2498
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int> NewSectionMap;

			// Token: 0x040009C3 RID: 2499
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int> NewAreaGraph;

			// Token: 0x040009C4 RID: 2500
			[WriteOnly]
			public NativeArray<Octant> NewOctants;
		}
	}
}
