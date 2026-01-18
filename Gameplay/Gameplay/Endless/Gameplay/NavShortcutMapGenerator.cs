using System;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay
{
	// Token: 0x020001DD RID: 477
	public static class NavShortcutMapGenerator
	{
		// Token: 0x06000A03 RID: 2563 RVA: 0x00033C0C File Offset: 0x00031E0C
		public static IEnumerator GenerateShortcutMaps(NativeParallelHashSet<BidirectionalConnection> walkConnections, NativeParallelHashSet<BidirectionalConnection> jumpConnections, NativeParallelHashSet<Connection> dropConnections, NativeList<int> islandKeys, NativeParallelMultiHashMap<int, int> sectionMap, NativeParallelMultiHashMap<int, int> islandKeyToSectionKeyMap, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, NativeList<int> sectionKeys, Action<NavShortcutMapGenerator.Results> getResults)
		{
			NativeParallelMultiHashMap<int, int> nativeParallelMultiHashMap = new NativeParallelMultiHashMap<int, int>(walkConnections.Count() * 2, AllocatorManager.Persistent);
			NativeParallelMultiHashMap<int, int> nativeParallelMultiHashMap2 = new NativeParallelMultiHashMap<int, int>(jumpConnections.Count() * 2, AllocatorManager.Persistent);
			NativeParallelMultiHashMap<int, int> nativeParallelMultiHashMap3 = new NativeParallelMultiHashMap<int, int>(dropConnections.Count(), AllocatorManager.Persistent);
			NativeParallelMultiHashMap<int, int> areaMap = new NativeParallelMultiHashMap<int, int>(islandKeys.Length, AllocatorManager.Persistent);
			NativeParallelMultiHashMap<int, int> areaGraph = new NativeParallelMultiHashMap<int, int>(islandKeys.Length * islandKeys.Length, AllocatorManager.Persistent);
			NavShortcutMapGenerator.ConvertConnectionsToMapsJob convertConnectionsToMapsJob = new NavShortcutMapGenerator.ConvertConnectionsToMapsJob
			{
				walkConnections = walkConnections,
				verifiedJumpConnections = jumpConnections,
				verifiedDropdownConnections = dropConnections,
				walkConnectionMap = nativeParallelMultiHashMap,
				jumpConnectionMap = nativeParallelMultiHashMap2,
				dropConnectionMap = nativeParallelMultiHashMap3
			};
			NavShortcutMapGenerator.ConstructAreaJobs constructAreaJobs = new NavShortcutMapGenerator.ConstructAreaJobs
			{
				IslandKeyArray = islandKeys.AsArray(),
				NumUniqueKeys = islandKeys.Length,
				SectionMap = sectionMap,
				WalkConnectionMap = nativeParallelMultiHashMap,
				JumpConnectionMap = nativeParallelMultiHashMap2,
				DropConnectionMap = nativeParallelMultiHashMap3,
				IslandToSectionMap = islandKeyToSectionKeyMap,
				AreaMap = areaMap,
				WalkableOctantDataMap = walkableOctantDataMap,
				AreaGraph = areaGraph
			};
			JobHandle jobHandle = convertConnectionsToMapsJob.Schedule(default(JobHandle));
			jobHandle = constructAreaJobs.Schedule(jobHandle);
			jobHandle = islandKeys.Dispose(jobHandle);
			jobHandle = sectionKeys.Dispose(jobHandle);
			jobHandle = nativeParallelMultiHashMap.Dispose(jobHandle);
			jobHandle = nativeParallelMultiHashMap2.Dispose(jobHandle);
			jobHandle = nativeParallelMultiHashMap3.Dispose(jobHandle);
			yield return JobUtilities.WaitForJobToComplete(jobHandle, false);
			getResults(new NavShortcutMapGenerator.Results
			{
				AreaMap = areaMap,
				AreaGraph = areaGraph
			});
			yield break;
		}

		// Token: 0x020001DE RID: 478
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct ConvertConnectionsToMapsJob : IJob
		{
			// Token: 0x06000A04 RID: 2564 RVA: 0x00033C64 File Offset: 0x00031E64
			public void Execute()
			{
				foreach (BidirectionalConnection bidirectionalConnection in this.walkConnections)
				{
					this.walkConnectionMap.Add(bidirectionalConnection.SectionIndexA, bidirectionalConnection.SectionIndexB);
					this.walkConnectionMap.Add(bidirectionalConnection.SectionIndexB, bidirectionalConnection.SectionIndexA);
				}
				foreach (BidirectionalConnection bidirectionalConnection2 in this.verifiedJumpConnections)
				{
					this.jumpConnectionMap.Add(bidirectionalConnection2.SectionIndexA, bidirectionalConnection2.SectionIndexB);
					this.jumpConnectionMap.Add(bidirectionalConnection2.SectionIndexB, bidirectionalConnection2.SectionIndexA);
				}
				foreach (Connection connection in this.verifiedDropdownConnections)
				{
					this.dropConnectionMap.Add(connection.StartSectionKey, connection.EndSectionKey);
				}
			}

			// Token: 0x040008E4 RID: 2276
			[ReadOnly]
			public NativeParallelHashSet<BidirectionalConnection> walkConnections;

			// Token: 0x040008E5 RID: 2277
			[ReadOnly]
			public NativeParallelHashSet<BidirectionalConnection> verifiedJumpConnections;

			// Token: 0x040008E6 RID: 2278
			[ReadOnly]
			public NativeParallelHashSet<Connection> verifiedDropdownConnections;

			// Token: 0x040008E7 RID: 2279
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int> walkConnectionMap;

			// Token: 0x040008E8 RID: 2280
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int> jumpConnectionMap;

			// Token: 0x040008E9 RID: 2281
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int> dropConnectionMap;
		}

		// Token: 0x020001DF RID: 479
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct ConstructAreaJobs : IJob
		{
			// Token: 0x06000A05 RID: 2565 RVA: 0x00033D9C File Offset: 0x00031F9C
			public void Execute()
			{
				int num = 0;
				NativeList<int> nativeList = new NativeList<int>(64, AllocatorManager.Persistent);
				NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Persistent);
				NativeHashSet<int> nativeHashSet2 = new NativeHashSet<int>(64, AllocatorManager.Persistent);
				NativeParallelHashMap<int, int> nativeParallelHashMap = new NativeParallelHashMap<int, int>(64, AllocatorManager.Temp);
				for (int i = 0; i < this.NumUniqueKeys; i++)
				{
					int num2 = this.IslandKeyArray[i];
					nativeList.Clear();
					nativeHashSet2.Clear();
					if (!nativeHashSet.Contains(num2))
					{
						nativeList.Add(in num2);
						while (nativeList.Length > 0)
						{
							int num3 = nativeList[0];
							nativeList.RemoveAtSwapBack(0);
							nativeHashSet.Add(num3);
							nativeHashSet2.Add(num3);
							NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.IslandToSectionMap.GetValuesForKey(num3);
							foreach (int num4 in enumerator)
							{
								NativeParallelMultiHashMap<int, int>.Enumerator enumerator3 = this.JumpConnectionMap.GetValuesForKey(num4);
								foreach (int num5 in enumerator3)
								{
									int islandFromSectionKey = BurstPathfindingUtilities.GetIslandFromSectionKey(num5);
									if (!nativeList.Contains(islandFromSectionKey) && !nativeHashSet.Contains(islandFromSectionKey))
									{
										nativeList.Add(in islandFromSectionKey);
									}
								}
								enumerator3 = this.WalkConnectionMap.GetValuesForKey(num4);
								foreach (int num6 in enumerator3)
								{
									int islandFromSectionKey2 = BurstPathfindingUtilities.GetIslandFromSectionKey(num6);
									if (!nativeList.Contains(islandFromSectionKey2) && !nativeHashSet.Contains(islandFromSectionKey2))
									{
										nativeList.Add(in islandFromSectionKey2);
									}
								}
							}
						}
						foreach (int num7 in nativeHashSet2)
						{
							this.AreaMap.Add(num, num7);
							nativeParallelHashMap.Add(num7, num);
							NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.IslandToSectionMap.GetValuesForKey(num7);
							foreach (int num8 in enumerator)
							{
								NativeParallelMultiHashMap<int, int>.Enumerator enumerator3 = this.SectionMap.GetValuesForKey(num8);
								foreach (int num9 in enumerator3)
								{
									WalkableOctantData walkableOctantData = this.WalkableOctantDataMap[num9];
									walkableOctantData.AreaKey = num;
									this.WalkableOctantDataMap[num9] = walkableOctantData;
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
					foreach (int num10 in enumerator)
					{
						NativeParallelMultiHashMap<int, int>.Enumerator enumerator3 = this.IslandToSectionMap.GetValuesForKey(num10);
						foreach (int num11 in enumerator3)
						{
							foreach (int num12 in this.DropConnectionMap.GetValuesForKey(num11))
							{
								int islandFromSectionKey3 = BurstPathfindingUtilities.GetIslandFromSectionKey(num12);
								int num13 = nativeParallelHashMap[islandFromSectionKey3];
								if (num13 != j && nativeHashSet3.Add(num13))
								{
									this.AreaGraph.Add(j, num13);
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
					IL_041D:
					foreach (int num14 in nativeHashSet5)
					{
						nativeHashSet4.Remove(num14);
					}
					nativeHashSet5.Clear();
					foreach (int num15 in nativeHashSet4)
					{
						NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.AreaGraph.GetValuesForKey(num15);
						foreach (int num16 in enumerator)
						{
							BurstPathfindingUtilities.FindAreaPath(num15, num16, this.AreaGraph, nativeList2, nativeQueue, nativeArray, nativeArray2);
							foreach (int num17 in nativeList2)
							{
								nativeHashSet5.Add(num17);
							}
						}
						if (nativeHashSet5.Count > 0)
						{
							BurstPathfindingUtilities.ConsolidateAreas(num15, nativeHashSet5, this.AreaGraph, this.AreaMap, nativeParallelHashMap, this.IslandToSectionMap, this.SectionMap, this.WalkableOctantDataMap);
							goto IL_041D;
						}
					}
					break;
				}
			}

			// Token: 0x040008EA RID: 2282
			[ReadOnly]
			public NativeArray<int> IslandKeyArray;

			// Token: 0x040008EB RID: 2283
			[ReadOnly]
			public int NumUniqueKeys;

			// Token: 0x040008EC RID: 2284
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x040008ED RID: 2285
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> WalkConnectionMap;

			// Token: 0x040008EE RID: 2286
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> JumpConnectionMap;

			// Token: 0x040008EF RID: 2287
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> DropConnectionMap;

			// Token: 0x040008F0 RID: 2288
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> IslandToSectionMap;

			// Token: 0x040008F1 RID: 2289
			public NativeParallelMultiHashMap<int, int> AreaMap;

			// Token: 0x040008F2 RID: 2290
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x040008F3 RID: 2291
			public NativeParallelMultiHashMap<int, int> AreaGraph;
		}

		// Token: 0x020001E0 RID: 480
		public struct Results
		{
			// Token: 0x040008F4 RID: 2292
			public NativeParallelMultiHashMap<int, int> AreaMap;

			// Token: 0x040008F5 RID: 2293
			public NativeParallelMultiHashMap<int, int> AreaGraph;
		}
	}
}
