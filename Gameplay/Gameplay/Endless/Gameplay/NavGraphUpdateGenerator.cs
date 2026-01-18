using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.Jobs;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

namespace Endless.Gameplay
{
	// Token: 0x020001C9 RID: 457
	public static class NavGraphUpdateGenerator
	{
		// Token: 0x060009E8 RID: 2536 RVA: 0x0002F898 File Offset: 0x0002DA98
		public static IEnumerator UpdateNavGraph(List<NavGraph.ChangedCell> updateCells, NativeArray<Octant> octants, NativeParallelHashMap<int, SlopeNeighbors> slopeMap, NativeParallelHashMap<int, SerializableGuid> associatedPropMap, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, NativeParallelHashMap<int, GraphNode> nodeGraph, NativeParallelMultiHashMap<int, Edge> edgeGraph, NativeParallelMultiHashMap<int, int> areaMap, NativeParallelHashMap<int, SectionSurface> surfaceMap, NativeParallelMultiHashMap<int, int> sectionMap, NativeParallelMultiHashMap<int, int> islandToSectionMap, NativeParallelMultiHashMap<int, int> areaGraph)
		{
			NavMeshObstacle[] obstacles = global::UnityEngine.Object.FindObjectsByType<NavMeshObstacle>(FindObjectsSortMode.None);
			NavMeshObstacle[] array = obstacles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].carving = false;
			}
			NativeArray<int3> cellsToUpdate = new NativeArray<int3>(updateCells.Count, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			for (int j = 0; j < updateCells.Count; j++)
			{
				cellsToUpdate[j] = updateCells[j].Cell;
			}
			NativeReference<int> largestKey = new NativeReference<int>(AllocatorManager.Persistent, NativeArrayOptions.ClearMemory);
			NavMeshQuery query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, 0);
			NativeQueue<int> updatedOctantIndexes = new NativeQueue<int>(AllocatorManager.Persistent);
			NativeQueue<int> releasedIslandKeys = new NativeQueue<int>(AllocatorManager.Persistent);
			int numCells = cellsToUpdate.Length;
			GetLargestKey getLargestKey = new GetLargestKey
			{
				MultiHashMap = islandToSectionMap,
				LargestKey = largestKey
			};
			NavGraphUpdateGenerator.ClearAssociatedCellDataJob clearAssociatedCellDataJob = new NavGraphUpdateGenerator.ClearAssociatedCellDataJob
			{
				CellsToClear = cellsToUpdate,
				Octants = octants,
				WalkableOctantDataMap = walkableOctantDataMap,
				SectionMap = sectionMap,
				SurfaceMap = surfaceMap,
				IslandToSectionMap = islandToSectionMap,
				NodeGraph = nodeGraph,
				EdgeGraph = edgeGraph,
				UpdateOctantIndexes = updatedOctantIndexes,
				ReleasedIslandKeys = releasedIslandKeys
			};
			JobHandle jobHandle = getLargestKey.Schedule(default(JobHandle));
			jobHandle = clearAssociatedCellDataJob.Schedule(jobHandle);
			yield return JobUtilities.WaitForJobToComplete(jobHandle, false);
			int nextIslandKey = largestKey.Value + 1;
			NativeArray<int> octantsToUpdate = updatedOctantIndexes.ToArray(AllocatorManager.Persistent);
			updatedOctantIndexes.Dispose(default(JobHandle));
			NativeArray<OverlapBoxCommand> nativeArray = new NativeArray<OverlapBoxCommand>(octantsToUpdate.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			NativeArray<OverlapBoxCommand> nativeArray2 = new NativeArray<OverlapBoxCommand>(octantsToUpdate.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			NativeArray<ColliderHit> nativeArray3 = new NativeArray<ColliderHit>(octantsToUpdate.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			NativeArray<ColliderHit> nativeArray4 = new NativeArray<ColliderHit>(octantsToUpdate.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			NavGraphUpdateGenerator.ConstructBoxcastCommandsJob constructBoxcastCommandsJob = new NavGraphUpdateGenerator.ConstructBoxcastCommandsJob
			{
				OctantsToUpdate = octantsToUpdate,
				Octants = octants,
				BlockingOverlapBoxCommands = nativeArray,
				WalkableOverlapBoxCommands = nativeArray2,
				QueryParameters = new QueryParameters(NpcMovementValues.JumpSweepMask, false, QueryTriggerInteraction.UseGlobal, false)
			};
			NavGraphUpdateGenerator.UpdateOctantData updateOctantData = new NavGraphUpdateGenerator.UpdateOctantData
			{
				OctantsToUpdate = octantsToUpdate,
				BlockingResults = nativeArray3,
				WalkableResults = nativeArray4,
				MinSplitCellAreaPercentage = NavGraph.MinSplitCellAreaPercentage,
				octantDivisions = NavGraph.SplitCellDivisions,
				Octants = octants,
				Query = query
			};
			JobHandle jobHandle2 = constructBoxcastCommandsJob.Schedule(default(JobHandle));
			jobHandle2 = OverlapBoxCommand.ScheduleBatch(nativeArray, nativeArray3, 30, 1, jobHandle2);
			jobHandle2 = OverlapBoxCommand.ScheduleBatch(nativeArray2, nativeArray4, 30, 1, jobHandle2);
			int num = octantsToUpdate.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			jobHandle2 = updateOctantData.Schedule(octantsToUpdate.Length, num, jobHandle2);
			nativeArray.Dispose(jobHandle2);
			nativeArray3.Dispose(jobHandle2);
			nativeArray2.Dispose(jobHandle2);
			nativeArray4.Dispose(jobHandle2);
			yield return JobUtilities.WaitForJobToComplete(jobHandle2, false);
			NativeParallelMultiHashMap<int, int> nativeParallelMultiHashMap = new NativeParallelMultiHashMap<int, int>(numCells * 64 * 16, AllocatorManager.Persistent);
			NativeParallelMultiHashMap<int, int> neighborMap = new NativeParallelMultiHashMap<int, int>(numCells * 64 * 16, AllocatorManager.Persistent);
			NavGraphUpdateGenerator.FindNeighborsJob findNeighborsJob = new NavGraphUpdateGenerator.FindNeighborsJob
			{
				OctantsToUpdate = octantsToUpdate,
				NeighborMap = nativeParallelMultiHashMap.AsParallelWriter(),
				Octants = octants,
				Query = query
			};
			NavGraphUpdateGenerator.CleanNeighborsJob cleanNeighborsJob = new NavGraphUpdateGenerator.CleanNeighborsJob
			{
				RawNeighborMap = nativeParallelMultiHashMap,
				NeighborMap = neighborMap
			};
			num = octantsToUpdate.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			JobHandle jobHandle3 = findNeighborsJob.Schedule(octantsToUpdate.Length, num, default(JobHandle));
			jobHandle3 = cleanNeighborsJob.Schedule(jobHandle3);
			nativeParallelMultiHashMap.Dispose(jobHandle3);
			cellsToUpdate.Dispose(jobHandle3);
			yield return JobUtilities.WaitForJobToComplete(jobHandle3, true);
			NativeReference<int> numOctantsOfLargestIslandReference = new NativeReference<int>(AllocatorManager.Persistent, NativeArrayOptions.ClearMemory);
			NativeParallelMultiHashMap<int, int> newIslandMap = new NativeParallelMultiHashMap<int, int>(octantsToUpdate.Length, AllocatorManager.Persistent);
			JobHandle jobHandle4 = new NavGraphUpdateGenerator.BuildIslandsJob
			{
				OctantsToUpdate = octantsToUpdate,
				NeighborMap = neighborMap,
				AssociatedPropMap = associatedPropMap,
				ReleasedIslandKeys = releasedIslandKeys,
				NextIslandKey = nextIslandKey,
				Octants = octants,
				IslandMap = newIslandMap,
				WalkableOctantDataMap = walkableOctantDataMap,
				NumOctantsOfLargestIsland = numOctantsOfLargestIslandReference
			}.Schedule(default(JobHandle));
			releasedIslandKeys.Dispose(jobHandle4);
			yield return JobUtilities.WaitForJobToComplete(jobHandle4, true);
			int numOctantsOfLargestIsland = numOctantsOfLargestIslandReference.Value;
			numOctantsOfLargestIslandReference.Dispose(default(JobHandle));
			NativeList<int> newIslandKeys = new NativeList<int>(1000, AllocatorManager.Persistent);
			JobHandle jobHandle5 = new GetUniqueKeysJob
			{
				RawMultiMap = newIslandMap,
				UniqueKeys = newIslandKeys
			}.Schedule(default(JobHandle));
			yield return JobUtilities.WaitForJobToComplete(jobHandle5, false);
			NativeParallelMultiHashMap<int, int> newSectionMap = new NativeParallelMultiHashMap<int, int>(octantsToUpdate.Length, AllocatorManager.Persistent);
			int num2 = islandToSectionMap.Capacity - islandToSectionMap.Count();
			int num3 = newIslandKeys.Length * numOctantsOfLargestIsland;
			if (num3 > num2)
			{
				islandToSectionMap.Capacity = math.ceilpow2(islandToSectionMap.Capacity + num3);
			}
			num2 = sectionMap.Capacity - sectionMap.Count();
			if (num3 > num2)
			{
				sectionMap.Capacity = math.ceilpow2(sectionMap.Capacity + num3);
			}
			NavGraphUpdateGenerator.BuildSectionsJob buildSectionsJob = new NavGraphUpdateGenerator.BuildSectionsJob
			{
				IslandMap = newIslandMap,
				IslandKeys = newIslandKeys.AsArray(),
				SlopeMap = slopeMap,
				NeighborMap = neighborMap,
				Octants = octants,
				SectionMap = sectionMap.AsParallelWriter(),
				NewSectionMap = newSectionMap.AsParallelWriter(),
				IslandToSectionMap = islandToSectionMap.AsParallelWriter(),
				WalkableOctantDataMap = walkableOctantDataMap
			};
			num = newIslandKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			JobHandle jobHandle6 = buildSectionsJob.Schedule(newIslandKeys.Length, num, default(JobHandle));
			yield return JobUtilities.WaitForJobToComplete(jobHandle6, false);
			newIslandMap.Dispose(default(JobHandle));
			newIslandKeys.Dispose(default(JobHandle));
			NativeList<int> sectionKeys = new NativeList<int>(1024, AllocatorManager.Persistent);
			NativeList<int> allSectionKeys = new NativeList<int>(1024, AllocatorManager.Persistent);
			GetUniqueKeysJob getUniqueKeysJob = new GetUniqueKeysJob
			{
				RawMultiMap = newSectionMap,
				UniqueKeys = sectionKeys
			};
			GetUniqueKeysJob getUniqueKeysJob2 = new GetUniqueKeysJob
			{
				RawMultiMap = sectionMap,
				UniqueKeys = allSectionKeys
			};
			JobHandle jobHandle7 = getUniqueKeysJob.Schedule(default(JobHandle));
			jobHandle7 = getUniqueKeysJob2.Schedule(jobHandle7);
			newSectionMap.Dispose(jobHandle7);
			yield return JobUtilities.WaitForJobToComplete(jobHandle7, false);
			NativeQueue<BidirectionalConnection> nativeQueue = new NativeQueue<BidirectionalConnection>(AllocatorManager.Persistent);
			NativeQueue<Connection> dropdownConnections = new NativeQueue<Connection>(AllocatorManager.Persistent);
			NativeHashSet<Connection> dropConnections = new NativeHashSet<Connection>(1024, AllocatorManager.Persistent);
			NativeParallelHashSet<BidirectionalConnection> walkConnections = new NativeParallelHashSet<BidirectionalConnection>(octantsToUpdate.Length * 5, AllocatorManager.Persistent);
			NativeHashSet<BidirectionalConnection> jumpConnections = new NativeHashSet<BidirectionalConnection>(octantsToUpdate.Length, AllocatorManager.Persistent);
			num2 = surfaceMap.Capacity - surfaceMap.Count();
			if (num2 < sectionKeys.Length)
			{
				surfaceMap.Capacity = math.ceilpow2(sectionKeys.Length + surfaceMap.Count());
			}
			FindEdgeOctantsJob findEdgeOctantsJob = new FindEdgeOctantsJob
			{
				SectionKeys = sectionKeys.AsDeferredJobArray(),
				SectionMap = sectionMap,
				NeighborMap = neighborMap,
				Octants = octants,
				WalkableOctantDataMap = walkableOctantDataMap
			};
			NavGraphUpdateGenerator.PruneNeighborsJob pruneNeighborsJob = new NavGraphUpdateGenerator.PruneNeighborsJob
			{
				OctantsToUpdate = octantsToUpdate,
				NeighborMap = neighborMap,
				WalkableOctantDataMap = walkableOctantDataMap
			};
			BuildSurfacesJob buildSurfacesJob = new BuildSurfacesJob
			{
				SectionKeyArray = sectionKeys.AsArray(),
				SectionMap = sectionMap,
				Octants = octants,
				Query = query,
				SurfaceMap = surfaceMap.AsParallelWriter()
			};
			NavGraphUpdateGenerator.FindWalkConnectionsJob findWalkConnectionsJob = new NavGraphUpdateGenerator.FindWalkConnectionsJob
			{
				SectionKeyArray = sectionKeys.AsArray(),
				NeighborMap = neighborMap,
				WalkableOctantDataMap = walkableOctantDataMap,
				SectionMap = sectionMap,
				WalkConnections = walkConnections.AsParallelWriter()
			};
			NavGraphUpdateGenerator.FindJumpConnectionsJob findJumpConnectionsJob = new NavGraphUpdateGenerator.FindJumpConnectionsJob
			{
				SectionKeyArray = sectionKeys.AsArray(),
				Octants = octants,
				SectionMap = sectionMap,
				SurfaceMap = surfaceMap,
				WalkableOctantDataMap = walkableOctantDataMap,
				WalkConnections = walkConnections,
				JumpConnections = nativeQueue.AsParallelWriter()
			};
			ConvertQueueToHashset convertQueueToHashset = new ConvertQueueToHashset
			{
				ConnectionsQueue = nativeQueue,
				ConnectionsHashSet = jumpConnections
			};
			NavGraphUpdateGenerator.FindDropdownConnectionsJob findDropdownConnectionsJob = new NavGraphUpdateGenerator.FindDropdownConnectionsJob
			{
				NewSectionsKeyArray = sectionKeys,
				Octants = octants,
				AllSectionKeys = allSectionKeys,
				SurfaceMap = surfaceMap,
				VerifiedJumpConnections = jumpConnections,
				PotentialDropdownConnections = dropdownConnections.AsParallelWriter()
			};
			NavGraphUpdateGenerator.ConvertConnectionQueueToHashset convertConnectionQueueToHashset = new NavGraphUpdateGenerator.ConvertConnectionQueueToHashset
			{
				Connections = dropdownConnections,
				DropConnections = dropConnections
			};
			num = sectionKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			JobHandle jobHandle8 = findEdgeOctantsJob.Schedule(sectionKeys.Length, num, default(JobHandle));
			jobHandle8 = pruneNeighborsJob.Schedule(jobHandle8);
			jobHandle8 = buildSurfacesJob.Schedule(sectionKeys.Length, num, jobHandle8);
			jobHandle8 = findWalkConnectionsJob.Schedule(sectionKeys.Length, num, jobHandle8);
			jobHandle8 = findJumpConnectionsJob.Schedule(sectionKeys.Length, num, jobHandle8);
			jobHandle8 = convertQueueToHashset.Schedule(jobHandle8);
			jobHandle8 = findDropdownConnectionsJob.Schedule(sectionKeys.Length, num, jobHandle8);
			jobHandle8 = convertConnectionQueueToHashset.Schedule(jobHandle8);
			nativeQueue.Dispose(jobHandle8);
			octantsToUpdate.Dispose(jobHandle8);
			neighborMap.Dispose(jobHandle8);
			allSectionKeys.Dispose(jobHandle8);
			yield return JobUtilities.WaitForJobToComplete(jobHandle8, false);
			NativeArray<Connection> potentialDropConnections = dropdownConnections.ToArray(AllocatorManager.Persistent);
			dropdownConnections.Dispose(default(JobHandle));
			NativeQueue<Connection> verifiedDropConnectionQueue = new NativeQueue<Connection>(AllocatorManager.Persistent);
			NativeList<int> uniqueKeys = new NativeList<int>(128, AllocatorManager.Persistent);
			JobHandle jobHandle9 = new GetUniqueKeysJob
			{
				RawMultiMap = islandToSectionMap,
				UniqueKeys = uniqueKeys
			}.Schedule(default(JobHandle));
			yield return JobUtilities.WaitForJobToComplete(jobHandle9, false);
			NativeParallelHashMap<int, int> reverseAreaMap = new NativeParallelHashMap<int, int>(uniqueKeys.Length, AllocatorManager.Persistent);
			int num4 = math.ceilpow2(uniqueKeys.Length * uniqueKeys.Length);
			if (areaGraph.Capacity < num4)
			{
				areaGraph.Capacity = num4;
			}
			NavGraphUpdateGenerator.AddUpdateDataToGraph addUpdateDataToGraph = new NavGraphUpdateGenerator.AddUpdateDataToGraph
			{
				SectionKeys = sectionKeys,
				SectionMap = sectionMap,
				SurfaceMap = surfaceMap,
				WalkableOctantData = walkableOctantDataMap,
				WalkConnections = walkConnections,
				JumpConnections = jumpConnections,
				DropConnections = dropConnections,
				NodeGraph = nodeGraph,
				EdgeGraph = edgeGraph
			};
			CleanJumpConnections cleanJumpConnections = new CleanJumpConnections
			{
				EdgeMap = edgeGraph,
				NodeMap = nodeGraph
			};
			NavGraphUpdateGenerator.ConstructAreaJobs constructAreaJobs = new NavGraphUpdateGenerator.ConstructAreaJobs
			{
				IslandKeys = uniqueKeys,
				SectionMap = sectionMap,
				IslandToSectionMap = islandToSectionMap,
				EdgeGraph = edgeGraph,
				AreaMap = areaMap,
				ReverseAreaMap = reverseAreaMap,
				AreaGraph = areaGraph,
				WalkableOctantDataMap = walkableOctantDataMap
			};
			JobHandle jobHandle10 = addUpdateDataToGraph.Schedule(default(JobHandle));
			potentialDropConnections.Dispose(jobHandle10);
			verifiedDropConnectionQueue.Dispose(jobHandle10);
			walkConnections.Dispose(jobHandle10);
			jumpConnections.Dispose(jobHandle10);
			dropConnections.Dispose(jobHandle10);
			jobHandle10 = cleanJumpConnections.Schedule(jobHandle10);
			jobHandle10 = constructAreaJobs.Schedule(jobHandle10);
			uniqueKeys.Dispose(jobHandle10);
			sectionKeys.Dispose(jobHandle10);
			yield return JobUtilities.WaitForJobToComplete(jobHandle10, false);
			NativeParallelHashMap<SerializableGuid, UnsafeHashSet<int3>> thresholdMap = new NativeParallelHashMap<SerializableGuid, UnsafeHashSet<int3>>(updateCells.Count, AllocatorManager.Persistent);
			NativeQueue<BidirectionalConnection> thresholdConnections = new NativeQueue<BidirectionalConnection>(AllocatorManager.Persistent);
			foreach (NavGraph.ChangedCell changedCell in updateCells)
			{
				if (changedCell.IsBlocking)
				{
					SerializableGuid instanceId = changedCell.WorldObject.InstanceId;
					object obj;
					if (changedCell.WorldObject.TryGetUserComponent(typeof(Door), out obj) && ((Door)obj).CurrentNpcDoorInteraction != Door.NpcDoorInteraction.NotOpenable)
					{
						UnsafeHashSet<int3> unsafeHashSet;
						if (thresholdMap.TryGetValue(instanceId, out unsafeHashSet))
						{
							unsafeHashSet.Add(changedCell.Cell);
							thresholdMap[instanceId] = unsafeHashSet;
						}
						else
						{
							thresholdMap.Add(instanceId, new UnsafeHashSet<int3>(64, AllocatorManager.Persistent) { changedCell.Cell });
						}
					}
				}
			}
			int num5 = 0;
			foreach (KeyValue<SerializableGuid, UnsafeHashSet<int3>> keyValue in thresholdMap)
			{
				if (keyValue.Value.Count > num5)
				{
					num5 = keyValue.Value.Count;
				}
			}
			NavGraphUpdateGenerator.BuildThresholdConnections buildThresholdConnections = new NavGraphUpdateGenerator.BuildThresholdConnections
			{
				thresholdMap = thresholdMap,
				Octants = octants,
				WalkableOctantDataMap = walkableOctantDataMap,
				SurfaceMap = surfaceMap,
				AreaGraph = areaGraph.AsParallelWriter(),
				ReverseAreaMap = reverseAreaMap,
				IslandToSectionMap = islandToSectionMap,
				ThresholdConnections = thresholdConnections.AsParallelWriter()
			};
			int length = thresholdMap.GetKeyArray(AllocatorManager.Temp).Length;
			num = length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			JobHandle jobHandle11 = buildThresholdConnections.Schedule(length, num, default(JobHandle));
			yield return JobUtilities.WaitForJobToComplete(jobHandle11, false);
			foreach (KeyValue<SerializableGuid, UnsafeHashSet<int3>> keyValue2 in thresholdMap)
			{
				keyValue2.Value.Dispose(default(JobHandle));
			}
			thresholdMap.Dispose(default(JobHandle));
			foreach (BidirectionalConnection bidirectionalConnection in thresholdConnections.ToArray(AllocatorManager.Temp))
			{
				float num6 = math.distance(surfaceMap[bidirectionalConnection.SectionIndexA].Center, surfaceMap[bidirectionalConnection.SectionIndexB].Center);
				edgeGraph.Add(bidirectionalConnection.SectionIndexA, new Edge
				{
					Connection = ConnectionKind.Threshold,
					ConnectedNodeKey = bidirectionalConnection.SectionIndexB,
					Cost = num6
				});
				edgeGraph.Add(bidirectionalConnection.SectionIndexB, new Edge
				{
					Connection = ConnectionKind.Threshold,
					ConnectedNodeKey = bidirectionalConnection.SectionIndexA,
					Cost = num6
				});
			}
			thresholdConnections.Dispose(default(JobHandle));
			reverseAreaMap.Dispose(default(JobHandle));
			query.Dispose();
			array = obstacles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].carving = true;
			}
			yield break;
		}

		// Token: 0x020001CA RID: 458
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct ClearAssociatedCellDataJob : IJob
		{
			// Token: 0x060009E9 RID: 2537 RVA: 0x0002F908 File Offset: 0x0002DB08
			public unsafe void Execute()
			{
				NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(this.SurfaceMap.Capacity, AllocatorManager.Temp);
				foreach (int3 @int in this.CellsToClear)
				{
					ref Octant smallestOctantForPoint = ref OctreeHelperMethods.GetSmallestOctantForPoint(@int, this.Octants, 1f);
					if (smallestOctantForPoint.HasChildren)
					{
						for (int i = 0; i < 8; i++)
						{
							int num;
							smallestOctantForPoint.TryGetChildIndex(i, out num);
							ref Octant @ref = ref this.Octants.GetRef(num);
							for (int j = 0; j < 8; j++)
							{
								int num2;
								@ref.TryGetChildIndex(j, out num2);
								this.Octants.GetRef(num2).IsBlocking = false;
								this.UpdateOctantIndexes.Enqueue(num2);
								if (this.WalkableOctantDataMap.ContainsKey(num2))
								{
									nativeHashSet.Add(this.WalkableOctantDataMap[num2].IslandKey);
								}
							}
						}
					}
				}
				foreach (int num3 in nativeHashSet)
				{
					NativeParallelMultiHashMap<int, int>.Enumerator enumerator3 = this.IslandToSectionMap.GetValuesForKey(num3);
					foreach (int num4 in enumerator3)
					{
						foreach (int num5 in this.SectionMap.GetValuesForKey(num4))
						{
							this.UpdateOctantIndexes.Enqueue(num5);
							this.WalkableOctantDataMap.Remove(num5);
							ref Octant ref2 = ref this.Octants.GetRef(num5);
							ref Octant smallestOctantForPoint2 = ref OctreeHelperMethods.GetSmallestOctantForPoint(ref2.Center, this.Octants, 1f);
							ref Octant smallestOctantForPoint3 = ref OctreeHelperMethods.GetSmallestOctantForPoint(ref2.Center, this.Octants, 0.5f);
							bool flag = false;
							for (int k = 0; k < 8; k++)
							{
								int num6;
								if (smallestOctantForPoint3.TryGetChildIndex(k, out num6) && this.Octants[num6].IsWalkable)
								{
									flag = true;
									break;
								}
							}
							smallestOctantForPoint3.HasWalkableChildren = flag;
							flag = false;
							for (int l = 0; l < 8; l++)
							{
								int num7;
								if (smallestOctantForPoint2.TryGetChildIndex(l, out num7) && this.Octants[num7].IsWalkable)
								{
									flag = true;
									break;
								}
							}
							smallestOctantForPoint2.HasWalkableChildren = flag;
							ref2.IsWalkable = false;
							ref2.IsBlocking = false;
						}
						this.SectionMap.Remove(num4);
						this.SurfaceMap.Remove(num4);
						NativeList<ValueTuple<int, Edge>> nativeList = new NativeList<ValueTuple<int, Edge>>(64, AllocatorManager.Temp);
						foreach (KeyValue<int, Edge> keyValue in this.EdgeGraph)
						{
							if (keyValue.Value.ConnectedNodeKey == num4)
							{
								ValueTuple<int, Edge> valueTuple = new ValueTuple<int, Edge>(keyValue.Key, *keyValue.Value);
								nativeList.Add(in valueTuple);
							}
						}
						this.EdgeGraph.Remove(num4);
						foreach (ValueTuple<int, Edge> valueTuple2 in nativeList)
						{
							this.EdgeGraph.Remove(valueTuple2.Item1, valueTuple2.Item2);
						}
						this.NodeGraph.Remove(num4);
					}
					this.IslandToSectionMap.Remove(num3);
					this.ReleasedIslandKeys.Enqueue(num3);
				}
			}

			// Token: 0x04000857 RID: 2135
			[ReadOnly]
			public NativeArray<int3> CellsToClear;

			// Token: 0x04000858 RID: 2136
			public NativeArray<Octant> Octants;

			// Token: 0x04000859 RID: 2137
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x0400085A RID: 2138
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x0400085B RID: 2139
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x0400085C RID: 2140
			public NativeParallelMultiHashMap<int, int> IslandToSectionMap;

			// Token: 0x0400085D RID: 2141
			public NativeParallelHashMap<int, GraphNode> NodeGraph;

			// Token: 0x0400085E RID: 2142
			public NativeParallelMultiHashMap<int, Edge> EdgeGraph;

			// Token: 0x0400085F RID: 2143
			[WriteOnly]
			public NativeQueue<int> UpdateOctantIndexes;

			// Token: 0x04000860 RID: 2144
			[WriteOnly]
			public NativeQueue<int> ReleasedIslandKeys;
		}

		// Token: 0x020001CB RID: 459
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct ConstructBoxcastCommandsJob : IJob
		{
			// Token: 0x060009EA RID: 2538 RVA: 0x0002FD60 File Offset: 0x0002DF60
			public void Execute()
			{
				for (int i = 0; i < this.OctantsToUpdate.Length; i++)
				{
					int num = this.OctantsToUpdate[i];
					ref Octant @ref = ref this.Octants.GetRef(num);
					Vector3 vector = new Vector3(0.1f, 0.1f, 0.1f);
					Vector3 vector2 = new Vector3(0.001f, 0.001f, 0.001f);
					this.BlockingOverlapBoxCommands[i] = new OverlapBoxCommand(@ref.Center, vector, Quaternion.identity, this.QueryParameters);
					this.WalkableOverlapBoxCommands[i] = new OverlapBoxCommand(@ref.Center.ToVector3() + Vector3.up, vector2, Quaternion.identity, this.QueryParameters);
				}
			}

			// Token: 0x04000861 RID: 2145
			[ReadOnly]
			public NativeArray<int> OctantsToUpdate;

			// Token: 0x04000862 RID: 2146
			public NativeArray<Octant> Octants;

			// Token: 0x04000863 RID: 2147
			[WriteOnly]
			public NativeArray<OverlapBoxCommand> BlockingOverlapBoxCommands;

			// Token: 0x04000864 RID: 2148
			[WriteOnly]
			public NativeArray<OverlapBoxCommand> WalkableOverlapBoxCommands;

			// Token: 0x04000865 RID: 2149
			public QueryParameters QueryParameters;
		}

		// Token: 0x020001CC RID: 460
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct UpdateOctantData : IJobParallelFor
		{
			// Token: 0x060009EB RID: 2539 RVA: 0x0002FE2C File Offset: 0x0002E02C
			public void Execute(int index)
			{
				int num = this.OctantsToUpdate[index];
				ref Octant @ref = ref this.Octants.GetRef(num);
				Vector3 vector = new Vector3(0f, -0.001f, 0f);
				Vector3 vector2 = new Vector3(0.001f, 0.125f, 0.001f);
				int num2 = this.octantDivisions * this.octantDivisions;
				int num3 = 0;
				NativeList<Vector3> nativeList = new NativeList<Vector3>(num2, AllocatorManager.Temp);
				OctreeHelperMethods.GetSamplePositions(nativeList, @ref.Min, @ref.Max, this.octantDivisions);
				foreach (Vector3 vector3 in nativeList)
				{
					NavMeshLocation navMeshLocation = this.Query.MapLocation(vector3 + vector, vector2, 0, -1);
					if (this.Query.IsValid(navMeshLocation))
					{
						num3++;
					}
				}
				if ((float)num3 / (float)num2 > this.MinSplitCellAreaPercentage && this.WalkableResults[index].instanceID == 0)
				{
					@ref.IsWalkable = true;
					OctreeHelperMethods.GetSmallestOctantForPoint(@ref.Center, this.Octants, 1f).HasWalkableChildren = true;
					return;
				}
				if (this.BlockingResults[index].instanceID != 0)
				{
					@ref.IsBlocking = true;
				}
			}

			// Token: 0x04000866 RID: 2150
			[ReadOnly]
			public NativeArray<int> OctantsToUpdate;

			// Token: 0x04000867 RID: 2151
			[ReadOnly]
			public NativeArray<ColliderHit> BlockingResults;

			// Token: 0x04000868 RID: 2152
			[ReadOnly]
			public NativeArray<ColliderHit> WalkableResults;

			// Token: 0x04000869 RID: 2153
			[NativeDisableParallelForRestriction]
			public NativeArray<Octant> Octants;

			// Token: 0x0400086A RID: 2154
			[NativeDisableParallelForRestriction]
			public NavMeshQuery Query;

			// Token: 0x0400086B RID: 2155
			public int octantDivisions;

			// Token: 0x0400086C RID: 2156
			public float MinSplitCellAreaPercentage;
		}

		// Token: 0x020001CD RID: 461
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct FindNeighborsJob : IJobParallelFor
		{
			// Token: 0x060009EC RID: 2540 RVA: 0x0002FF8C File Offset: 0x0002E18C
			public void Execute(int index)
			{
				int num = this.OctantsToUpdate[index];
				ref Octant @ref = ref this.Octants.GetRef(num);
				if (!@ref.IsWalkable)
				{
					return;
				}
				for (int i = 0; i < 12; i++)
				{
					float3 subCellNeighborOffset = OctreeHelperMethods.GetSubCellNeighborOffset(i, @ref.Size.x);
					int num2;
					if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(@ref.Center + subCellNeighborOffset, this.Octants, @ref.Size.x, out num2))
					{
						ref Octant ref2 = ref this.Octants.GetRef(num2);
						if (ref2.IsWalkable)
						{
							if (@ref.IsSlope && ref2.IsSlope)
							{
								this.NeighborMap.Add(num, num2);
								this.NeighborMap.Add(num2, num);
							}
							else if (OctreeHelperMethods.SweepNeighboringEdge(@ref, ref2, this.Query))
							{
								this.NeighborMap.Add(num, num2);
								this.NeighborMap.Add(num2, num);
							}
						}
					}
				}
			}

			// Token: 0x0400086D RID: 2157
			[ReadOnly]
			public NativeArray<int> OctantsToUpdate;

			// Token: 0x0400086E RID: 2158
			[NativeDisableParallelForRestriction]
			public NativeArray<Octant> Octants;

			// Token: 0x0400086F RID: 2159
			[ReadOnly]
			public NavMeshQuery Query;

			// Token: 0x04000870 RID: 2160
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int>.ParallelWriter NeighborMap;
		}

		// Token: 0x020001CE RID: 462
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct CleanNeighborsJob : IJob
		{
			// Token: 0x060009ED RID: 2541 RVA: 0x00030084 File Offset: 0x0002E284
			public void Execute()
			{
				ValueTuple<NativeArray<int>, int> uniqueKeyArray = this.RawNeighborMap.GetUniqueKeyArray(AllocatorManager.Temp);
				for (int i = 0; i < uniqueKeyArray.Item2; i++)
				{
					int num = uniqueKeyArray.Item1[i];
					NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(this.RawNeighborMap.CountValuesForKey(num), AllocatorManager.Temp);
					foreach (int num2 in this.RawNeighborMap.GetValuesForKey(num))
					{
						nativeHashSet.Add(num2);
					}
					foreach (int num3 in nativeHashSet)
					{
						this.NeighborMap.Add(num, num3);
					}
				}
			}

			// Token: 0x04000871 RID: 2161
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> RawNeighborMap;

			// Token: 0x04000872 RID: 2162
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int> NeighborMap;
		}

		// Token: 0x020001CF RID: 463
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct BuildIslandsJob : IJob
		{
			// Token: 0x060009EE RID: 2542 RVA: 0x0003017C File Offset: 0x0002E37C
			public void Execute()
			{
				NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Temp);
				NativeQueue<int> nativeQueue = new NativeQueue<int>(AllocatorManager.Temp);
				for (int i = 0; i < this.OctantsToUpdate.Length; i++)
				{
					int num = this.OctantsToUpdate[i];
					if (!nativeHashSet.Contains(num) && this.Octants.GetRef(num).IsWalkable)
					{
						int nextIslandKey = this.GetNextIslandKey();
						int num2 = 1;
						this.IslandMap.Add(nextIslandKey, num);
						this.AddWalkableOctantData(num, nextIslandKey);
						nativeHashSet.Add(num);
						using (NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.NeighborMap.GetValuesForKey(num).GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								int num3 = enumerator.Current;
								SerializableGuid serializableGuid;
								if (this.AssociatedPropMap.TryGetValue(num, out serializableGuid))
								{
									SerializableGuid serializableGuid2;
									if (!this.AssociatedPropMap.TryGetValue(num3, out serializableGuid2))
									{
										continue;
									}
									if (serializableGuid != serializableGuid2)
									{
										continue;
									}
								}
								else if (this.AssociatedPropMap.ContainsKey(num3))
								{
									continue;
								}
								nativeQueue.Enqueue(num3);
								nativeHashSet.Add(num3);
							}
							goto IL_01C9;
						}
						goto IL_010A;
						IL_01C9:
						if (nativeQueue.IsEmpty())
						{
							if (this.NumOctantsOfLargestIsland.Value < num2)
							{
								this.NumOctantsOfLargestIsland.Value = num2;
								goto IL_01F1;
							}
							goto IL_01F1;
						}
						IL_010A:
						int num4 = nativeQueue.Dequeue();
						this.IslandMap.Add(nextIslandKey, num4);
						num2++;
						this.AddWalkableOctantData(num4, nextIslandKey);
						foreach (int num5 in this.NeighborMap.GetValuesForKey(num4))
						{
							SerializableGuid serializableGuid3;
							if (this.AssociatedPropMap.TryGetValue(num, out serializableGuid3))
							{
								SerializableGuid serializableGuid4;
								if (!this.AssociatedPropMap.TryGetValue(num5, out serializableGuid4))
								{
									continue;
								}
								if (serializableGuid3 != serializableGuid4)
								{
									continue;
								}
							}
							else if (this.AssociatedPropMap.ContainsKey(num5))
							{
								continue;
							}
							if (!nativeHashSet.Contains(num5))
							{
								nativeQueue.Enqueue(num5);
								nativeHashSet.Add(num5);
							}
						}
						goto IL_01C9;
					}
					IL_01F1:;
				}
			}

			// Token: 0x060009EF RID: 2543 RVA: 0x000303AC File Offset: 0x0002E5AC
			private void AddWalkableOctantData(int octantIndex, int currentIsland)
			{
				if (!this.WalkableOctantDataMap.TryAdd(octantIndex, new WalkableOctantData
				{
					IslandKey = currentIsland
				}))
				{
					this.WalkableOctantDataMap[octantIndex] = new WalkableOctantData
					{
						IslandKey = currentIsland
					};
				}
			}

			// Token: 0x060009F0 RID: 2544 RVA: 0x000303F5 File Offset: 0x0002E5F5
			private int GetNextIslandKey()
			{
				if (this.ReleasedIslandKeys.Count > 0)
				{
					return this.ReleasedIslandKeys.Dequeue();
				}
				int nextIslandKey = this.NextIslandKey;
				this.NextIslandKey++;
				return nextIslandKey;
			}

			// Token: 0x04000873 RID: 2163
			[ReadOnly]
			public NativeArray<int> OctantsToUpdate;

			// Token: 0x04000874 RID: 2164
			[ReadOnly]
			public NativeParallelHashMap<int, SerializableGuid> AssociatedPropMap;

			// Token: 0x04000875 RID: 2165
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> NeighborMap;

			// Token: 0x04000876 RID: 2166
			public NativeQueue<int> ReleasedIslandKeys;

			// Token: 0x04000877 RID: 2167
			public int NextIslandKey;

			// Token: 0x04000878 RID: 2168
			public NativeArray<Octant> Octants;

			// Token: 0x04000879 RID: 2169
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int> IslandMap;

			// Token: 0x0400087A RID: 2170
			[WriteOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x0400087B RID: 2171
			public NativeReference<int> NumOctantsOfLargestIsland;
		}

		// Token: 0x020001D0 RID: 464
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct BuildSectionsJob : IJobParallelFor
		{
			// Token: 0x060009F1 RID: 2545 RVA: 0x00030428 File Offset: 0x0002E628
			public void Execute(int index)
			{
				int num = this.IslandKeys[index];
				NativeList<int> nativeList = new NativeList<int>(1024, AllocatorManager.Temp);
				NativeList<int> nativeList2 = new NativeList<int>(1024, AllocatorManager.Temp);
				NativeList<int> nativeList3 = new NativeList<int>(64, AllocatorManager.Temp);
				NativeList<int> nativeList4 = new NativeList<int>(16, AllocatorManager.Temp);
				NativeList<int> nativeList5 = new NativeList<int>(16, AllocatorManager.Temp);
				int num2 = 1;
				foreach (int num3 in this.IslandMap.GetValuesForKey(num))
				{
					nativeList.Add(in num3);
				}
				nativeList.Sort(new OctantComparer(this.Octants));
				while (nativeList.Length > 0)
				{
					int num4 = nativeList[0];
					ref Octant @ref = ref this.Octants.GetRef(num4);
					int num5;
					if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(@ref.Center, this.Octants, 1f, out num5))
					{
						throw new Exception("Malformed Octutree");
					}
					ref Octant ref2 = ref this.Octants.GetRef(num5);
					if (@ref.IsSlope)
					{
						OctreeHelperMethods.GetWalkableChildrenIndices(this.Octants, nativeList4, ref ref2);
						foreach (int num6 in nativeList4)
						{
							if (!nativeList2.Contains(num6) && nativeList.Contains(num6))
							{
								nativeList2.Add(in num6);
							}
						}
						bool flag = true;
						int num7 = 0;
						int num8 = 0;
						int num9 = num5;
						int num10 = 0;
						int num11 = 1;
						int num12;
						while (num11 < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(ref2.Center + new float3((float)num11, 0f, 0f), this.Octants, 1f, out num12) && this.Octants.GetRef(num12).IsSlope && this.SlopeMap[num5] == this.SlopeMap[num12] && OctreeHelperMethods.AreChildrenNeighbors(num9, num12, this.Octants, this.NeighborMap, nativeList4, nativeList5))
						{
							num9 = num12;
							num10++;
							num11++;
						}
						num9 = num5;
						int num13 = 0;
						int num14 = 1;
						int num15;
						while (num14 < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(ref2.Center + new float3(0f, 0f, (float)num14), this.Octants, 1f, out num15) && this.Octants.GetRef(num15).IsSlope && this.SlopeMap[num5] == this.SlopeMap[num15] && OctreeHelperMethods.AreChildrenNeighbors(num9, num15, this.Octants, this.NeighborMap, nativeList4, nativeList5))
						{
							num9 = num15;
							num13++;
							num14++;
						}
						int num16;
						if (num13 > num10)
						{
							flag = false;
							num16 = num13;
						}
						else
						{
							num16 = num10;
						}
						for (int i = 0; i < 4; i++)
						{
							num9 = num5;
							int num17 = 0;
							float3 zdirection = NavGraphUpdateGenerator.BuildSectionsJob.GetZDirection(i);
							int num18 = 1;
							int num19;
							while (num18 < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(ref2.Center + zdirection * (float)num18, this.Octants, 1f, out num19) && this.Octants.GetRef(num19).IsSlope && this.SlopeMap[num5] == this.SlopeMap[num19] && OctreeHelperMethods.AreChildrenNeighbors(num9, num19, this.Octants, this.NeighborMap, nativeList4, nativeList5))
							{
								num9 = num19;
								num17++;
								num18++;
							}
							if (num17 > num7)
							{
								num8 = i;
								num7 = num17;
							}
						}
						if (num16 >= num7)
						{
							float3 @float = (flag ? new float3(1f, 0f, 0f) : new float3(0f, 0f, 1f));
							for (int j = 1; j <= num16; j++)
							{
								int num20;
								OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(ref2.Center + @float * (float)j, this.Octants, 1f, out num20);
								ref Octant ref3 = ref this.Octants.GetRef(num20);
								OctreeHelperMethods.GetWalkableChildrenIndices(this.Octants, nativeList5, ref ref3);
								foreach (int num21 in nativeList5)
								{
									if (nativeList.Contains(num21))
									{
										nativeList2.Add(in num21);
									}
								}
							}
						}
						else
						{
							float3 zdirection2 = NavGraphUpdateGenerator.BuildSectionsJob.GetZDirection(num8);
							for (int k = 1; k < num7; k++)
							{
								int num22;
								OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(ref2.Center + zdirection2 * (float)k, this.Octants, 1f, out num22);
								ref Octant ref4 = ref this.Octants.GetRef(num22);
								OctreeHelperMethods.GetWalkableChildrenIndices(this.Octants, nativeList5, ref ref4);
								foreach (int num23 in nativeList5)
								{
									if (nativeList.Contains(num23))
									{
										nativeList2.Add(in num23);
									}
								}
							}
						}
					}
					else
					{
						nativeList2.Add(in num4);
						nativeList3.Add(in num4);
						while (nativeList3.Length > 0)
						{
							int num24 = nativeList3[0];
							nativeList3.RemoveAt(0);
							foreach (int num25 in this.NeighborMap.GetValuesForKey(num24))
							{
								Octant octant = this.Octants[num25];
								if (octant.Size.x <= 0.25f && !octant.IsSlope)
								{
									int num26;
									if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant.Center, this.Octants, 1f, out num26))
									{
										throw new Exception("Malformed Octutree");
									}
									if (num5 == num26 && nativeList.Contains(num25) && !nativeList3.Contains(num25) && !nativeList2.Contains(num25))
									{
										nativeList2.AddNoResize(num25);
										nativeList3.AddNoResize(num25);
									}
								}
							}
						}
					}
					foreach (int num27 in nativeList2)
					{
						int num28 = nativeList.IndexOf(num27);
						if (num28 == -1)
						{
							throw new Exception("Malformed Sections");
						}
						nativeList.RemoveAt(num28);
						int sectionKey = BurstPathfindingUtilities.GetSectionKey(num, num2);
						this.SectionMap.Add(sectionKey, num27);
						this.NewSectionMap.Add(sectionKey, num27);
						this.IslandToSectionMap.Add(num, sectionKey);
						WalkableOctantData walkableOctantData = this.WalkableOctantDataMap[num27];
						walkableOctantData.SectionKey = sectionKey;
						this.WalkableOctantDataMap[num27] = walkableOctantData;
					}
					nativeList3.Clear();
					nativeList2.Clear();
					num2++;
				}
			}

			// Token: 0x060009F2 RID: 2546 RVA: 0x00030B70 File Offset: 0x0002ED70
			private static float3 GetZDirection(int i)
			{
				float3 @float;
				switch (i)
				{
				case 0:
					@float = new float3(1f, 1f, 0f);
					break;
				case 1:
					@float = new float3(0f, 1f, 1f);
					break;
				case 2:
					@float = new float3(-1f, 1f, 0f);
					break;
				case 3:
					@float = new float3(0f, 1f, -1f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				return @float;
			}

			// Token: 0x0400087C RID: 2172
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> IslandMap;

			// Token: 0x0400087D RID: 2173
			[ReadOnly]
			public NativeArray<int> IslandKeys;

			// Token: 0x0400087E RID: 2174
			[ReadOnly]
			public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

			// Token: 0x0400087F RID: 2175
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> NeighborMap;

			// Token: 0x04000880 RID: 2176
			[NativeDisableParallelForRestriction]
			public NativeArray<Octant> Octants;

			// Token: 0x04000881 RID: 2177
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int>.ParallelWriter SectionMap;

			// Token: 0x04000882 RID: 2178
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int>.ParallelWriter NewSectionMap;

			// Token: 0x04000883 RID: 2179
			[NativeDisableParallelForRestriction]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x04000884 RID: 2180
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int>.ParallelWriter IslandToSectionMap;
		}

		// Token: 0x020001D1 RID: 465
		[BurstCompile]
		private struct PruneNeighborsJob : IJob
		{
			// Token: 0x060009F3 RID: 2547 RVA: 0x00030BF8 File Offset: 0x0002EDF8
			public void Execute()
			{
				NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(16, AllocatorManager.Temp);
				foreach (int num in this.OctantsToUpdate)
				{
					if (this.WalkableOctantDataMap.ContainsKey(num))
					{
						nativeHashSet.Clear();
						foreach (int num2 in this.NeighborMap.GetValuesForKey(num))
						{
							if (this.WalkableOctantDataMap[num].SectionKey == this.WalkableOctantDataMap[num2].SectionKey)
							{
								nativeHashSet.Add(num2);
							}
						}
						foreach (int num3 in nativeHashSet)
						{
							this.NeighborMap.Remove(num, num3);
						}
					}
				}
			}

			// Token: 0x04000885 RID: 2181
			[ReadOnly]
			public NativeArray<int> OctantsToUpdate;

			// Token: 0x04000886 RID: 2182
			[ReadOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x04000887 RID: 2183
			public NativeParallelMultiHashMap<int, int> NeighborMap;
		}

		// Token: 0x020001D2 RID: 466
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct FindWalkConnectionsJob : IJobParallelFor
		{
			// Token: 0x060009F4 RID: 2548 RVA: 0x00030D2C File Offset: 0x0002EF2C
			public void Execute(int index)
			{
				int num = this.SectionKeyArray[index];
				NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.SectionMap.GetValuesForKey(num);
				foreach (int num2 in enumerator)
				{
					foreach (int num3 in this.NeighborMap.GetValuesForKey(num2))
					{
						this.WalkConnections.Add(new BidirectionalConnection(this.WalkableOctantDataMap[num2].SectionKey, this.WalkableOctantDataMap[num3].SectionKey));
					}
				}
			}

			// Token: 0x04000888 RID: 2184
			[ReadOnly]
			public NativeArray<int> SectionKeyArray;

			// Token: 0x04000889 RID: 2185
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> NeighborMap;

			// Token: 0x0400088A RID: 2186
			[ReadOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x0400088B RID: 2187
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x0400088C RID: 2188
			[WriteOnly]
			public NativeParallelHashSet<BidirectionalConnection>.ParallelWriter WalkConnections;
		}

		// Token: 0x020001D3 RID: 467
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct FindDropdownConnectionsJob : IJobParallelFor
		{
			// Token: 0x060009F5 RID: 2549 RVA: 0x00030E08 File Offset: 0x0002F008
			public void Execute(int index)
			{
				int num = this.NewSectionsKeyArray[index];
				SectionSurface sectionSurface = this.SurfaceMap[num];
				foreach (int num2 in this.AllSectionKeys)
				{
					if (num != num2)
					{
						SectionSurface sectionSurface2 = this.SurfaceMap[num2];
						if (sectionSurface2.Center.y - sectionSurface.Center.y >= 1f)
						{
							float3 @float = sectionSurface.Center - sectionSurface2.Center;
							@float.y = 0f;
							if (math.length(@float) > 24f)
							{
								continue;
							}
							SectionEdge closestEdgeToPoint = sectionSurface.GetClosestEdgeToPoint(sectionSurface2.Center);
							SectionEdge closestEdgeToPoint2 = sectionSurface2.GetClosestEdgeToPoint(sectionSurface.Center);
							float3 float2 = closestEdgeToPoint.GetClosestPointOnEdge(sectionSurface2.Center, 0f);
							float3 float3 = closestEdgeToPoint2.GetClosestPointOnEdge(float2, 0f);
							float2 = closestEdgeToPoint.GetClosestPointOnEdge(float3, 0f);
							float3 = closestEdgeToPoint2.GetClosestPointOnEdge(float2, 0f);
							if (!BurstPathfindingUtilities.CanReachJumpPosition(in float3, in float2, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity) || this.VerifiedJumpConnections.Contains(new BidirectionalConnection(num, num2)))
							{
								continue;
							}
							using (NativeArray<BurstPathfindingUtilities.SurfacePair>.Enumerator enumerator2 = BurstPathfindingUtilities.GetSortedDropPairs(sectionSurface, sectionSurface2).GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									BurstPathfindingUtilities.SurfacePair surfacePair = enumerator2.Current;
									if (!BurstPathfindingUtilities.CanReachJumpPosition(in surfacePair.JumpPoint, in surfacePair.LandPoint, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
									{
										return;
									}
									float num3 = BurstPathfindingUtilities.EstimateLaunchAngle(in surfacePair.JumpPoint, in surfacePair.LandPoint);
									float3 float4;
									float num4;
									if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in surfacePair.JumpPoint, in surfacePair.LandPoint, num3, NpcMovementValues.Gravity, out float4, out num4) && !float.IsNaN(num4) && num4 > 0f && !BurstPathfindingUtilities.IsPathBlocked(surfacePair.JumpPoint, surfacePair.LandPoint, num4, float4, this.Octants))
									{
										this.PotentialDropdownConnections.Enqueue(new Connection(num2, num));
										break;
									}
								}
								continue;
							}
						}
						if (sectionSurface.Center.y - sectionSurface2.Center.y >= 1f)
						{
							float3 float5 = sectionSurface.Center - sectionSurface2.Center;
							float5.y = 0f;
							if (math.length(float5) <= 24f)
							{
								SectionEdge closestEdgeToPoint3 = sectionSurface.GetClosestEdgeToPoint(sectionSurface2.Center);
								SectionEdge closestEdgeToPoint4 = sectionSurface2.GetClosestEdgeToPoint(sectionSurface.Center);
								float3 float6 = closestEdgeToPoint3.GetClosestPointOnEdge(sectionSurface2.Center, 0f);
								float3 float7 = closestEdgeToPoint4.GetClosestPointOnEdge(float6, 0f);
								float6 = closestEdgeToPoint3.GetClosestPointOnEdge(float7, 0f);
								float7 = closestEdgeToPoint4.GetClosestPointOnEdge(float6, 0f);
								if (BurstPathfindingUtilities.CanReachJumpPosition(in float6, in float7, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity) && !this.VerifiedJumpConnections.Contains(new BidirectionalConnection(num, num2)))
								{
									foreach (BurstPathfindingUtilities.SurfacePair surfacePair2 in BurstPathfindingUtilities.GetSortedDropPairs(sectionSurface, sectionSurface2))
									{
										if (!BurstPathfindingUtilities.CanReachJumpPosition(in surfacePair2.JumpPoint, in surfacePair2.LandPoint, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
										{
											return;
										}
										float num5 = BurstPathfindingUtilities.EstimateLaunchAngle(in surfacePair2.JumpPoint, in surfacePair2.LandPoint);
										float3 float8;
										float num6;
										if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in surfacePair2.JumpPoint, in surfacePair2.LandPoint, num5, NpcMovementValues.Gravity, out float8, out num6) && !float.IsNaN(num6) && num6 > 0f && !BurstPathfindingUtilities.IsPathBlocked(surfacePair2.JumpPoint, surfacePair2.LandPoint, num6, float8, this.Octants))
										{
											this.PotentialDropdownConnections.Enqueue(new Connection(num, num2));
											break;
										}
									}
								}
							}
						}
					}
				}
			}

			// Token: 0x0400088D RID: 2189
			[ReadOnly]
			public NativeList<int> NewSectionsKeyArray;

			// Token: 0x0400088E RID: 2190
			[ReadOnly]
			public NativeList<int> AllSectionKeys;

			// Token: 0x0400088F RID: 2191
			[ReadOnly]
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x04000890 RID: 2192
			[ReadOnly]
			public NativeHashSet<BidirectionalConnection> VerifiedJumpConnections;

			// Token: 0x04000891 RID: 2193
			[NativeDisableParallelForRestriction]
			public NativeArray<Octant> Octants;

			// Token: 0x04000892 RID: 2194
			[WriteOnly]
			public NativeQueue<Connection>.ParallelWriter PotentialDropdownConnections;
		}

		// Token: 0x020001D4 RID: 468
		[BurstCompile]
		private struct ConvertConnectionQueueToHashset : IJob
		{
			// Token: 0x060009F6 RID: 2550 RVA: 0x00031268 File Offset: 0x0002F468
			public void Execute()
			{
				while (this.Connections.Count > 0)
				{
					this.DropConnections.Add(this.Connections.Dequeue());
				}
			}

			// Token: 0x04000893 RID: 2195
			public NativeQueue<Connection> Connections;

			// Token: 0x04000894 RID: 2196
			[WriteOnly]
			public NativeHashSet<Connection> DropConnections;
		}

		// Token: 0x020001D5 RID: 469
		[BurstCompile]
		private struct AddUpdateDataToGraph : IJob
		{
			// Token: 0x060009F7 RID: 2551 RVA: 0x00031294 File Offset: 0x0002F494
			public void Execute()
			{
				foreach (int num in this.SectionKeys)
				{
					int num2;
					NativeParallelMultiHashMapIterator<int> nativeParallelMultiHashMapIterator;
					this.SectionMap.TryGetFirstValue(num, out num2, out nativeParallelMultiHashMapIterator);
					WalkableOctantData walkableOctantData = this.WalkableOctantData[num2];
					GraphNode graphNode = new GraphNode
					{
						IslandKey = walkableOctantData.IslandKey,
						AreaKey = walkableOctantData.AreaKey,
						ZoneKey = walkableOctantData.ZoneKey,
						Key = num,
						Center = this.SurfaceMap[num].Center
					};
					this.NodeGraph.Add(num, graphNode);
				}
				foreach (BidirectionalConnection bidirectionalConnection in this.WalkConnections)
				{
					float num3 = math.distance(this.SurfaceMap[bidirectionalConnection.SectionIndexA].Center, this.SurfaceMap[bidirectionalConnection.SectionIndexB].Center);
					this.EdgeGraph.Add(bidirectionalConnection.SectionIndexA, new Edge
					{
						Cost = num3,
						Connection = ConnectionKind.Walk,
						ConnectedNodeKey = bidirectionalConnection.SectionIndexB
					});
					this.EdgeGraph.Add(bidirectionalConnection.SectionIndexB, new Edge
					{
						Cost = num3,
						Connection = ConnectionKind.Walk,
						ConnectedNodeKey = bidirectionalConnection.SectionIndexA
					});
				}
				foreach (BidirectionalConnection bidirectionalConnection2 in this.JumpConnections)
				{
					float num4 = math.distance(this.SurfaceMap[bidirectionalConnection2.SectionIndexA].Center, this.SurfaceMap[bidirectionalConnection2.SectionIndexB].Center) * 2f;
					this.EdgeGraph.Add(bidirectionalConnection2.SectionIndexA, new Edge
					{
						Cost = num4,
						Connection = ConnectionKind.Jump,
						ConnectedNodeKey = bidirectionalConnection2.SectionIndexB
					});
					this.EdgeGraph.Add(bidirectionalConnection2.SectionIndexB, new Edge
					{
						Cost = num4,
						Connection = ConnectionKind.Jump,
						ConnectedNodeKey = bidirectionalConnection2.SectionIndexA
					});
				}
				foreach (Connection connection in this.DropConnections)
				{
					float num5 = math.distance(this.SurfaceMap[connection.StartSectionKey].Center, this.SurfaceMap[connection.EndSectionKey].Center) * 2.5f;
					this.EdgeGraph.Add(connection.StartSectionKey, new Edge
					{
						Connection = ConnectionKind.Dropdown,
						ConnectedNodeKey = connection.EndSectionKey,
						Cost = num5
					});
				}
			}

			// Token: 0x04000895 RID: 2197
			[ReadOnly]
			public NativeList<int> SectionKeys;

			// Token: 0x04000896 RID: 2198
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x04000897 RID: 2199
			[ReadOnly]
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x04000898 RID: 2200
			[ReadOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantData;

			// Token: 0x04000899 RID: 2201
			[ReadOnly]
			public NativeParallelHashSet<BidirectionalConnection> WalkConnections;

			// Token: 0x0400089A RID: 2202
			[ReadOnly]
			public NativeHashSet<BidirectionalConnection> JumpConnections;

			// Token: 0x0400089B RID: 2203
			[ReadOnly]
			public NativeHashSet<Connection> DropConnections;

			// Token: 0x0400089C RID: 2204
			[WriteOnly]
			public NativeParallelHashMap<int, GraphNode> NodeGraph;

			// Token: 0x0400089D RID: 2205
			[WriteOnly]
			public NativeParallelMultiHashMap<int, Edge> EdgeGraph;
		}

		// Token: 0x020001D6 RID: 470
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct ConstructAreaJobs : IJob
		{
			// Token: 0x060009F8 RID: 2552 RVA: 0x0003161C File Offset: 0x0002F81C
			public void Execute()
			{
				int num = 0;
				NativeList<int> nativeList = new NativeList<int>(64, AllocatorManager.Persistent);
				NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Persistent);
				NativeHashSet<int> nativeHashSet2 = new NativeHashSet<int>(64, AllocatorManager.Persistent);
				this.AreaMap.Clear();
				this.AreaGraph.Clear();
				for (int i = 0; i < this.IslandKeys.Length; i++)
				{
					int num2 = this.IslandKeys[i];
					nativeList.Clear();
					nativeHashSet2.Clear();
					if (!nativeHashSet.Contains(num2))
					{
						nativeHashSet.Add(num2);
						nativeList.Add(in num2);
						while (nativeList.Length > 0)
						{
							int num3 = nativeList[0];
							nativeList.RemoveAt(0);
							nativeHashSet.Add(num3);
							nativeHashSet2.Add(num3);
							NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.IslandToSectionMap.GetValuesForKey(num3);
							foreach (int num4 in enumerator)
							{
								foreach (Edge edge in this.EdgeGraph.GetValuesForKey(num4))
								{
									ConnectionKind connection = edge.Connection;
									if (connection != ConnectionKind.Dropdown && connection != ConnectionKind.Threshold)
									{
										int islandFromSectionKey = BurstPathfindingUtilities.GetIslandFromSectionKey(edge.ConnectedNodeKey);
										if (!nativeList.Contains(islandFromSectionKey) && !nativeHashSet.Contains(islandFromSectionKey))
										{
											nativeList.Add(in islandFromSectionKey);
										}
									}
								}
							}
						}
						foreach (int num5 in nativeHashSet2)
						{
							this.AreaMap.Add(num, num5);
							this.ReverseAreaMap.Add(num5, num);
							NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.IslandToSectionMap.GetValuesForKey(num5);
							foreach (int num6 in enumerator)
							{
								foreach (int num7 in this.SectionMap.GetValuesForKey(num6))
								{
									WalkableOctantData walkableOctantData = this.WalkableOctantDataMap[num7];
									walkableOctantData.AreaKey = num;
									this.WalkableOctantDataMap[num7] = walkableOctantData;
								}
							}
						}
						num++;
					}
				}
				for (int j = 0; j < num; j++)
				{
					NativeHashSet<int> nativeHashSet3 = new NativeHashSet<int>(num, AllocatorManager.Temp);
					NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.AreaMap.GetValuesForKey(j);
					foreach (int num8 in enumerator)
					{
						foreach (int num9 in this.IslandToSectionMap.GetValuesForKey(num8))
						{
							foreach (Edge edge2 in this.EdgeGraph.GetValuesForKey(num9))
							{
								if (edge2.Connection == ConnectionKind.Dropdown)
								{
									int islandFromSectionKey2 = BurstPathfindingUtilities.GetIslandFromSectionKey(edge2.ConnectedNodeKey);
									int num10 = this.ReverseAreaMap[islandFromSectionKey2];
									if (num10 != j && nativeHashSet3.Add(num10))
									{
										this.AreaGraph.Add(j, num10);
									}
								}
							}
						}
					}
				}
				NativeHashSet<int> nativeHashSet4 = new NativeHashSet<int>(num, AllocatorManager.Temp);
				NativeHashSet<int> nativeHashSet5 = new NativeHashSet<int>(64, AllocatorManager.Temp);
				NativeList<int> nativeList2 = new NativeList<int>(num, AllocatorManager.Temp);
				NativeQueue<int> nativeQueue = new NativeQueue<int>(AllocatorManager.Temp);
				NativeArray<int> nativeArray = new NativeArray<int>(num, Allocator.Temp, NativeArrayOptions.ClearMemory);
				NativeArray<int> nativeArray2 = new NativeArray<int>(num, Allocator.Temp, NativeArrayOptions.ClearMemory);
				for (int k = 0; k < num; k++)
				{
					nativeHashSet4.Add(k);
				}
				for (;;)
				{
					IL_0412:
					foreach (int num11 in nativeHashSet5)
					{
						nativeHashSet4.Remove(num11);
					}
					nativeHashSet5.Clear();
					foreach (int num12 in nativeHashSet4)
					{
						NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.AreaGraph.GetValuesForKey(num12);
						foreach (int num13 in enumerator)
						{
							BurstPathfindingUtilities.FindAreaPath(num12, num13, this.AreaGraph, nativeList2, nativeQueue, nativeArray, nativeArray2);
							foreach (int num14 in nativeList2)
							{
								nativeHashSet5.Add(num14);
							}
						}
						if (nativeHashSet5.Count > 0)
						{
							BurstPathfindingUtilities.ConsolidateAreas(num12, nativeHashSet5, this.AreaGraph, this.AreaMap, this.ReverseAreaMap, this.IslandToSectionMap, this.SectionMap, this.WalkableOctantDataMap);
							goto IL_0412;
						}
					}
					break;
				}
				foreach (KeyValue<int, Edge> keyValue in this.EdgeGraph)
				{
					if (keyValue.Value.Connection == ConnectionKind.Threshold)
					{
						int num15;
						NativeParallelMultiHashMapIterator<int> nativeParallelMultiHashMapIterator;
						this.SectionMap.TryGetFirstValue(keyValue.Key, out num15, out nativeParallelMultiHashMapIterator);
						int num16;
						this.SectionMap.TryGetFirstValue(keyValue.Value.ConnectedNodeKey, out num16, out nativeParallelMultiHashMapIterator);
						int areaKey = this.WalkableOctantDataMap[num15].AreaKey;
						int areaKey2 = this.WalkableOctantDataMap[num16].AreaKey;
						if (areaKey != areaKey2)
						{
							this.AreaGraph.Add(areaKey, areaKey2);
						}
					}
				}
			}

			// Token: 0x0400089E RID: 2206
			[ReadOnly]
			public NativeList<int> IslandKeys;

			// Token: 0x0400089F RID: 2207
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x040008A0 RID: 2208
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> IslandToSectionMap;

			// Token: 0x040008A1 RID: 2209
			[ReadOnly]
			public NativeParallelMultiHashMap<int, Edge> EdgeGraph;

			// Token: 0x040008A2 RID: 2210
			public NativeParallelMultiHashMap<int, int> AreaMap;

			// Token: 0x040008A3 RID: 2211
			public NativeParallelHashMap<int, int> ReverseAreaMap;

			// Token: 0x040008A4 RID: 2212
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x040008A5 RID: 2213
			public NativeParallelMultiHashMap<int, int> AreaGraph;
		}

		// Token: 0x020001D7 RID: 471
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct FindJumpConnectionsJob : IJobParallelFor
		{
			// Token: 0x060009F9 RID: 2553 RVA: 0x00031CC8 File Offset: 0x0002FEC8
			public void Execute(int index)
			{
				int num = this.SectionKeyArray[index];
				NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Temp);
				UnsafeHashSet<int> unsafeHashSet = new UnsafeHashSet<int>(64, AllocatorManager.Temp);
				foreach (int num2 in this.SectionMap.GetValuesForKey(num))
				{
					NpcEnum.Edge edge = this.WalkableOctantDataMap[num2].Edge;
					int num3;
					if (Hint.Unlikely(!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(this.Octants.GetRef(num2).Center, this.Octants, 1f, out num3)))
					{
						throw new Exception("This shouldn't be possible");
					}
					if (nativeHashSet.Add(num3))
					{
						float3 center = this.Octants.GetRef(num3).Center;
						if ((edge & NpcEnum.Edge.North) != NpcEnum.Edge.None)
						{
							for (int i = 0; i < 275; i++)
							{
								float3 jumpUpdateDirectionalOffset = BurstPathfindingUtilities.GetJumpUpdateDirectionalOffset(i, math.forward());
								int num4;
								if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpUpdateDirectionalOffset, this.Octants, 1f, out num4))
								{
									Octant octant = this.Octants[num4];
									if (octant.IsWalkable || octant.HasWalkableChildren)
									{
										WalkableOctantData walkableOctantData;
										if (octant.HasWalkableChildren)
										{
											for (int j = 0; j < 8; j++)
											{
												int num5;
												if (octant.TryGetChildIndex(j, out num5))
												{
													ref Octant @ref = ref this.Octants.GetRef(num5);
													for (int k = 0; k < 8; k++)
													{
														int num6;
														if (@ref.TryGetChildIndex(k, out num6) && this.Octants.GetRef(num6).IsWalkable)
														{
															unsafeHashSet.Add(this.WalkableOctantDataMap[num6].SectionKey);
														}
													}
												}
											}
										}
										else if (this.WalkableOctantDataMap.TryGetValue(num4, out walkableOctantData))
										{
											unsafeHashSet.Add(walkableOctantData.SectionKey);
										}
									}
								}
							}
						}
						if ((edge & NpcEnum.Edge.East) != NpcEnum.Edge.None)
						{
							for (int l = 0; l < 275; l++)
							{
								float3 jumpUpdateDirectionalOffset2 = BurstPathfindingUtilities.GetJumpUpdateDirectionalOffset(l, math.right());
								int num7;
								if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpUpdateDirectionalOffset2, this.Octants, 1f, out num7))
								{
									Octant octant2 = this.Octants[num7];
									if (octant2.IsWalkable || octant2.HasWalkableChildren)
									{
										WalkableOctantData walkableOctantData2;
										if (octant2.HasWalkableChildren)
										{
											for (int m = 0; m < 8; m++)
											{
												int num8;
												if (octant2.TryGetChildIndex(m, out num8))
												{
													ref Octant ref2 = ref this.Octants.GetRef(num8);
													for (int n = 0; n < 8; n++)
													{
														int num9;
														if (ref2.TryGetChildIndex(n, out num9) && this.Octants.GetRef(num9).IsWalkable)
														{
															unsafeHashSet.Add(this.WalkableOctantDataMap[num9].SectionKey);
														}
													}
												}
											}
										}
										else if (this.WalkableOctantDataMap.TryGetValue(num7, out walkableOctantData2))
										{
											unsafeHashSet.Add(walkableOctantData2.SectionKey);
										}
									}
								}
							}
						}
						if ((edge & NpcEnum.Edge.South) != NpcEnum.Edge.None)
						{
							for (int num10 = 0; num10 < 275; num10++)
							{
								float3 jumpUpdateDirectionalOffset3 = BurstPathfindingUtilities.GetJumpUpdateDirectionalOffset(num10, math.back());
								int num11;
								if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpUpdateDirectionalOffset3, this.Octants, 1f, out num11))
								{
									Octant octant3 = this.Octants[num11];
									if (octant3.IsWalkable || octant3.HasWalkableChildren)
									{
										WalkableOctantData walkableOctantData3;
										if (octant3.HasWalkableChildren)
										{
											for (int num12 = 0; num12 < 8; num12++)
											{
												int num13;
												if (octant3.TryGetChildIndex(num12, out num13))
												{
													ref Octant ref3 = ref this.Octants.GetRef(num13);
													for (int num14 = 0; num14 < 8; num14++)
													{
														int num15;
														if (ref3.TryGetChildIndex(num14, out num15) && this.Octants.GetRef(num15).IsWalkable)
														{
															unsafeHashSet.Add(this.WalkableOctantDataMap[num15].SectionKey);
														}
													}
												}
											}
										}
										else if (this.WalkableOctantDataMap.TryGetValue(num11, out walkableOctantData3))
										{
											unsafeHashSet.Add(walkableOctantData3.SectionKey);
										}
									}
								}
							}
						}
						if ((edge & NpcEnum.Edge.West) != NpcEnum.Edge.None)
						{
							for (int num16 = 0; num16 < 275; num16++)
							{
								float3 jumpUpdateDirectionalOffset4 = BurstPathfindingUtilities.GetJumpUpdateDirectionalOffset(num16, math.left());
								int num17;
								if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpUpdateDirectionalOffset4, this.Octants, 1f, out num17))
								{
									Octant octant4 = this.Octants[num17];
									if (octant4.IsWalkable || octant4.HasWalkableChildren)
									{
										WalkableOctantData walkableOctantData4;
										if (octant4.HasWalkableChildren)
										{
											for (int num18 = 0; num18 < 8; num18++)
											{
												int num19;
												if (octant4.TryGetChildIndex(num18, out num19))
												{
													ref Octant ref4 = ref this.Octants.GetRef(num19);
													for (int num20 = 0; num20 < 8; num20++)
													{
														int num21;
														if (ref4.TryGetChildIndex(num20, out num21) && this.Octants.GetRef(num21).IsWalkable)
														{
															unsafeHashSet.Add(this.WalkableOctantDataMap[num21].SectionKey);
														}
													}
												}
											}
										}
										else if (this.WalkableOctantDataMap.TryGetValue(num17, out walkableOctantData4))
										{
											unsafeHashSet.Add(walkableOctantData4.SectionKey);
										}
									}
								}
							}
						}
						foreach (int num22 in unsafeHashSet)
						{
							if (num != num22)
							{
								BidirectionalConnection bidirectionalConnection = new BidirectionalConnection(num, num22);
								if (!this.WalkConnections.Contains(bidirectionalConnection))
								{
									SectionSurface sectionSurface = this.SurfaceMap[num];
									SectionSurface sectionSurface2 = this.SurfaceMap[num22];
									if (sectionSurface.Center.y > sectionSurface2.Center.y)
									{
										SectionSurface sectionSurface3 = sectionSurface2;
										SectionSurface sectionSurface4 = sectionSurface;
										sectionSurface = sectionSurface3;
										sectionSurface2 = sectionSurface4;
									}
									foreach (BurstPathfindingUtilities.SurfacePair surfacePair in BurstPathfindingUtilities.GetSortedJumpPairs(sectionSurface, sectionSurface2))
									{
										if (BurstPathfindingUtilities.CanReachJumpPosition(in surfacePair.JumpPoint, in surfacePair.LandPoint, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
										{
											float num23 = BurstPathfindingUtilities.EstimateLaunchAngle(in surfacePair.JumpPoint, in surfacePair.LandPoint);
											float3 @float;
											float num24;
											if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in surfacePair.JumpPoint, in surfacePair.LandPoint, num23, NpcMovementValues.Gravity, out @float, out num24) && !float.IsNaN(num24) && num24 > 0f && !BurstPathfindingUtilities.IsPathBlocked(surfacePair.JumpPoint, surfacePair.LandPoint, num24, @float, this.Octants))
											{
												this.JumpConnections.Enqueue(bidirectionalConnection);
												break;
											}
										}
									}
								}
							}
						}
						nativeHashSet.Clear();
						unsafeHashSet.Clear();
					}
				}
			}

			// Token: 0x040008A6 RID: 2214
			[NativeDisableParallelForRestriction]
			public NativeArray<Octant> Octants;

			// Token: 0x040008A7 RID: 2215
			[ReadOnly]
			public NativeArray<int> SectionKeyArray;

			// Token: 0x040008A8 RID: 2216
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x040008A9 RID: 2217
			[ReadOnly]
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x040008AA RID: 2218
			[ReadOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x040008AB RID: 2219
			[ReadOnly]
			public NativeParallelHashSet<BidirectionalConnection> WalkConnections;

			// Token: 0x040008AC RID: 2220
			[WriteOnly]
			public NativeQueue<BidirectionalConnection>.ParallelWriter JumpConnections;
		}

		// Token: 0x020001D8 RID: 472
		[BurstCompile]
		private struct BuildThresholdConnections : IJobParallelFor
		{
			// Token: 0x060009FA RID: 2554 RVA: 0x000323D4 File Offset: 0x000305D4
			public void Execute(int index)
			{
				SerializableGuid serializableGuid = this.thresholdMap.GetKeyArray(AllocatorManager.Temp)[index];
				NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Temp);
				foreach (int3 @int in this.thresholdMap[serializableGuid])
				{
					ref Octant smallestOctantForPoint = ref OctreeHelperMethods.GetSmallestOctantForPoint(@int, this.Octants, 1f);
					if (smallestOctantForPoint.HasWalkableChildren)
					{
						for (int i = 0; i < 8; i++)
						{
							int num;
							smallestOctantForPoint.TryGetChildIndex(i, out num);
							ref Octant @ref = ref this.Octants.GetRef(num);
							for (int j = 0; j < 8; j++)
							{
								int num2;
								@ref.TryGetChildIndex(j, out num2);
								if (this.Octants.GetRef(num2).IsWalkable)
								{
									nativeHashSet.Add(this.WalkableOctantDataMap[num2].IslandKey);
								}
							}
						}
					}
				}
				NativeHashSet<int> nativeHashSet2 = new NativeHashSet<int>(64, AllocatorManager.Temp);
				foreach (int num3 in nativeHashSet)
				{
					nativeHashSet2.Add(this.ReverseAreaMap[num3]);
				}
				foreach (int num4 in nativeHashSet)
				{
					foreach (int num5 in nativeHashSet)
					{
						if (num4 != num5)
						{
							NavGraphUpdateGenerator.BuildThresholdConnections.SectionPair sectionPair = new NavGraphUpdateGenerator.BuildThresholdConnections.SectionPair
							{
								SectionA = -1,
								SectionB = -1
							};
							float num6 = float.MaxValue;
							NativeParallelMultiHashMap<int, int>.Enumerator enumerator4 = this.IslandToSectionMap.GetValuesForKey(num4);
							foreach (int num7 in enumerator4)
							{
								SectionSurface sectionSurface = this.SurfaceMap[num7];
								foreach (int num8 in this.IslandToSectionMap.GetValuesForKey(num5))
								{
									SectionSurface sectionSurface2 = this.SurfaceMap[num8];
									if (math.abs(sectionSurface.Center.y - sectionSurface2.Center.y) <= 1f)
									{
										float num9 = math.distancesq(sectionSurface.Center, sectionSurface2.Center);
										if (num9 < num6)
										{
											num6 = num9;
											sectionPair = new NavGraphUpdateGenerator.BuildThresholdConnections.SectionPair
											{
												SectionA = num7,
												SectionB = num8
											};
										}
									}
								}
							}
							if (sectionPair.SectionA != -1)
							{
								int num10 = this.ReverseAreaMap[num4];
								int num11 = this.ReverseAreaMap[num5];
								if (nativeHashSet2.Count > 1)
								{
									if (num10 != num11)
									{
										this.ThresholdConnections.Enqueue(new BidirectionalConnection(sectionPair.SectionA, sectionPair.SectionB));
										this.AreaGraph.Add(num10, num11);
									}
								}
								else
								{
									this.ThresholdConnections.Enqueue(new BidirectionalConnection(sectionPair.SectionA, sectionPair.SectionB));
								}
							}
						}
					}
				}
			}

			// Token: 0x040008AD RID: 2221
			[NativeDisableParallelForRestriction]
			public NativeArray<Octant> Octants;

			// Token: 0x040008AE RID: 2222
			[ReadOnly]
			public NativeParallelHashMap<SerializableGuid, UnsafeHashSet<int3>> thresholdMap;

			// Token: 0x040008AF RID: 2223
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> IslandToSectionMap;

			// Token: 0x040008B0 RID: 2224
			[ReadOnly]
			public NativeParallelHashMap<int, int> ReverseAreaMap;

			// Token: 0x040008B1 RID: 2225
			[ReadOnly]
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x040008B2 RID: 2226
			[ReadOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x040008B3 RID: 2227
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int>.ParallelWriter AreaGraph;

			// Token: 0x040008B4 RID: 2228
			[WriteOnly]
			public NativeQueue<BidirectionalConnection>.ParallelWriter ThresholdConnections;

			// Token: 0x020001D9 RID: 473
			private struct SectionPair
			{
				// Token: 0x040008B5 RID: 2229
				public int SectionA;

				// Token: 0x040008B6 RID: 2230
				public int SectionB;
			}
		}
	}
}
