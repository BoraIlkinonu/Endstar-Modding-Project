using System;
using System.Collections;
using Endless.Gameplay.Jobs;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Endless.Gameplay
{
	// Token: 0x020001B9 RID: 441
	public static class IslandGenerator
	{
		// Token: 0x060009C5 RID: 2501 RVA: 0x0002D0C4 File Offset: 0x0002B2C4
		public static IEnumerator GenerateIslands(NativeArray<Octant> octants, NativeParallelHashMap<int, SerializableGuid> associatedPropMap, NativeParallelHashMap<int, SlopeNeighbors> slopeMap, NavMeshQuery query, int numWalkableOctants, Action<IslandGenerator.Results> getResults)
		{
			NativeParallelMultiHashMap<int, int> neighborMap = new NativeParallelMultiHashMap<int, int>(numWalkableOctants * 16, AllocatorManager.Persistent);
			IslandGenerator.FindNeighborsJob findNeighborsJob = new IslandGenerator.FindNeighborsJob
			{
				Octants = octants,
				Query = query,
				NeighborMap = neighborMap.AsParallelWriter()
			};
			int num = octants.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			JobHandle jobHandle = findNeighborsJob.Schedule(octants.Length, num, default(JobHandle));
			NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap = new NativeParallelHashMap<int, WalkableOctantData>(numWalkableOctants, AllocatorManager.Persistent);
			NativeParallelMultiHashMap<int, int> islandMap = new NativeParallelMultiHashMap<int, int>(numWalkableOctants, AllocatorManager.Persistent);
			NativeParallelMultiHashMap<int, int> sectionMap = new NativeParallelMultiHashMap<int, int>(numWalkableOctants, AllocatorManager.Persistent);
			NativeList<int> islandKeys = new NativeList<int>(1024, AllocatorManager.Persistent);
			IslandGenerator.BuildIslandsJob buildIslandsJob = new IslandGenerator.BuildIslandsJob
			{
				Octants = octants,
				NeighborMap = neighborMap,
				AssociatedPropMap = associatedPropMap,
				IslandMap = islandMap,
				WalkableOctantDataMap = walkableOctantDataMap
			};
			GetUniqueKeysJob getUniqueKeysJob = new GetUniqueKeysJob
			{
				RawMultiMap = islandMap,
				UniqueKeys = islandKeys
			};
			jobHandle = buildIslandsJob.Schedule(jobHandle);
			jobHandle = getUniqueKeysJob.Schedule(jobHandle);
			yield return JobUtilities.WaitForJobToComplete(jobHandle, false);
			NativeParallelMultiHashMap<int, int> islandKeyToSectionKeyMap = new NativeParallelMultiHashMap<int, int>(numWalkableOctants, AllocatorManager.Persistent);
			NativeList<int> sectionKeys = new NativeList<int>(1024, AllocatorManager.Persistent);
			IslandGenerator.BuildSectionsJob buildSectionsJob = new IslandGenerator.BuildSectionsJob
			{
				IslandMap = islandMap,
				IslandKeys = islandKeys.AsArray(),
				NeighborMap = neighborMap,
				Octants = octants,
				SlopeMap = slopeMap,
				SectionMap = sectionMap.AsParallelWriter(),
				IslandToSectionMap = islandKeyToSectionKeyMap.AsParallelWriter(),
				WalkableOctantDataMap = walkableOctantDataMap
			};
			GetUniqueKeysJob getUniqueKeysJob2 = new GetUniqueKeysJob
			{
				RawMultiMap = sectionMap,
				UniqueKeys = sectionKeys
			};
			num = islandKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			JobHandle jobHandle2 = buildSectionsJob.Schedule(islandKeys.Length, num, default(JobHandle));
			jobHandle2 = islandMap.Dispose(jobHandle2);
			jobHandle2 = getUniqueKeysJob2.Schedule(jobHandle2);
			yield return JobUtilities.WaitForJobToComplete(jobHandle2, false);
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(16, AllocatorManager.Persistent);
			NativeParallelHashMap<int, SectionSurface> surfaceMap = new NativeParallelHashMap<int, SectionSurface>(sectionKeys.Length, AllocatorManager.Persistent);
			FindEdgeOctantsJob findEdgeOctantsJob = new FindEdgeOctantsJob
			{
				SectionKeys = sectionKeys,
				SectionMap = sectionMap,
				NeighborMap = neighborMap,
				Octants = octants,
				WalkableOctantDataMap = walkableOctantDataMap
			};
			IslandGenerator.PruneNeighborsJob pruneNeighborsJob = new IslandGenerator.PruneNeighborsJob
			{
				Octants = octants,
				NeighborMap = neighborMap,
				WalkableOctantDataMap = walkableOctantDataMap,
				WorkingSet = nativeHashSet
			};
			BuildSurfacesJob buildSurfacesJob = new BuildSurfacesJob
			{
				Octants = octants,
				SectionMap = sectionMap,
				SectionKeyArray = sectionKeys,
				Query = query,
				SurfaceMap = surfaceMap.AsParallelWriter()
			};
			num = sectionKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			JobHandle jobHandle3 = findEdgeOctantsJob.Schedule(sectionKeys.Length, num, default(JobHandle));
			jobHandle3 = pruneNeighborsJob.Schedule(jobHandle3);
			nativeHashSet.Dispose(jobHandle3);
			jobHandle3 = buildSurfacesJob.Schedule(sectionKeys.Length, num, jobHandle3);
			yield return JobUtilities.WaitForJobToComplete(jobHandle3, false);
			getResults(new IslandGenerator.Results
			{
				IslandKeys = islandKeys,
				SectionKeys = sectionKeys,
				NeighborMap = neighborMap,
				IslandKeyToSectionKeyMap = islandKeyToSectionKeyMap,
				SectionMap = sectionMap,
				SurfaceMap = surfaceMap,
				WalkableOctantDataMap = walkableOctantDataMap
			});
			yield break;
		}

		// Token: 0x020001BA RID: 442
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct FindNeighborsJob : IJobParallelFor
		{
			// Token: 0x060009C6 RID: 2502 RVA: 0x0002D0F8 File Offset: 0x0002B2F8
			public void Execute(int index)
			{
				Octant octant = this.Octants[index];
				if (!octant.IsWalkable)
				{
					return;
				}
				float x = octant.Size.x;
				if (math.abs(x - 1f) < 0.01f)
				{
					for (int i = 0; i < 4; i++)
					{
						float3 neighborOffset = OctreeHelperMethods.GetNeighborOffset(i, x);
						int num;
						if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant.Center + neighborOffset, this.Octants, x, out num))
						{
							Octant octant2 = this.Octants[num];
							if (math.abs(octant2.Size.x - x) <= 0.01f && octant2.IsWalkable && !octant2.IsSlope)
							{
								this.NeighborMap.Add(index, num);
							}
						}
					}
					for (int j = 0; j < 16; j++)
					{
						float3 bigToSmallNeighborOffset = OctreeHelperMethods.GetBigToSmallNeighborOffset(j);
						int num2;
						if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant.Center + bigToSmallNeighborOffset, this.Octants, x / 4f, out num2))
						{
							Octant octant3 = this.Octants[num2];
							if (octant3.IsWalkable && octant3.Size.x <= 0.25f && OctreeHelperMethods.SweepNeighboringEdge(octant, octant3, this.Query))
							{
								this.NeighborMap.Add(index, num2);
								this.NeighborMap.Add(num2, index);
							}
						}
					}
					for (int k = 0; k < 16; k++)
					{
						float3 terrainToSmallBelowOffset = OctreeHelperMethods.GetTerrainToSmallBelowOffset(k);
						int num3;
						if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant.Center + terrainToSmallBelowOffset, this.Octants, x / 4f, out num3))
						{
							Octant octant4 = this.Octants[num3];
							if (octant4.IsWalkable && octant4.Size.x <= 0.25f && OctreeHelperMethods.SweepNeighboringEdge(octant, octant4, this.Query))
							{
								this.NeighborMap.Add(index, num3);
								this.NeighborMap.Add(num3, index);
							}
						}
					}
					return;
				}
				for (int l = 0; l < 12; l++)
				{
					float3 subCellNeighborOffset = OctreeHelperMethods.GetSubCellNeighborOffset(l, octant.Size.x);
					int num4;
					if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant.Center + subCellNeighborOffset, this.Octants, octant.Size.x, out num4))
					{
						Octant octant5 = this.Octants[num4];
						if (math.abs(octant5.Size.x - octant.Size.x) <= 0.01f && octant5.IsWalkable)
						{
							if (octant.IsSlope && octant5.IsSlope)
							{
								this.NeighborMap.Add(index, num4);
							}
							else if (OctreeHelperMethods.SweepNeighboringEdge(octant, octant5, this.Query))
							{
								this.NeighborMap.Add(index, num4);
							}
						}
					}
				}
			}

			// Token: 0x040007EF RID: 2031
			[ReadOnly]
			public NativeArray<Octant> Octants;

			// Token: 0x040007F0 RID: 2032
			[ReadOnly]
			public NavMeshQuery Query;

			// Token: 0x040007F1 RID: 2033
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int>.ParallelWriter NeighborMap;
		}

		// Token: 0x020001BB RID: 443
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct BuildIslandsJob : IJob
		{
			// Token: 0x060009C7 RID: 2503 RVA: 0x0002D3BC File Offset: 0x0002B5BC
			public void Execute()
			{
				int num = 1;
				NativeQueue<int> nativeQueue = new NativeQueue<int>(AllocatorManager.Temp);
				NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(1024, AllocatorManager.Temp);
				for (int i = 0; i < this.Octants.Length; i++)
				{
					if (!nativeHashSet.Contains(i))
					{
						Octant octant = this.Octants[i];
						if (!octant.IsValid)
						{
							break;
						}
						if (octant.IsWalkable)
						{
							this.IslandMap.Add(num, i);
							this.WalkableOctantDataMap.Add(i, new WalkableOctantData
							{
								IslandKey = num
							});
							nativeHashSet.Add(i);
							using (NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.NeighborMap.GetValuesForKey(i).GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									int num2 = enumerator.Current;
									Octant octant2 = this.Octants[num2];
									if (octant.IsConditionallyNavigable == octant2.IsConditionallyNavigable)
									{
										SerializableGuid serializableGuid;
										if (this.AssociatedPropMap.TryGetValue(i, out serializableGuid))
										{
											SerializableGuid serializableGuid2;
											if (!this.AssociatedPropMap.TryGetValue(num2, out serializableGuid2))
											{
												continue;
											}
											if (serializableGuid != serializableGuid2)
											{
												continue;
											}
										}
										else if (this.AssociatedPropMap.ContainsKey(num2))
										{
											continue;
										}
										nativeQueue.Enqueue(num2);
										nativeHashSet.Add(num2);
									}
								}
								goto IL_024A;
							}
							goto IL_013D;
							IL_024A:
							if (nativeQueue.IsEmpty())
							{
								num++;
								goto IL_025A;
							}
							IL_013D:
							int num3 = nativeQueue.Dequeue();
							Octant octant3 = this.Octants[num3];
							this.IslandMap.Add(num, num3);
							if (!this.WalkableOctantDataMap.TryAdd(num3, new WalkableOctantData
							{
								IslandKey = num
							}))
							{
								Debug.LogError("Malformed octutree.");
							}
							foreach (int num4 in this.NeighborMap.GetValuesForKey(num3))
							{
								Octant octant4 = this.Octants[num4];
								if (octant3.IsConditionallyNavigable == octant4.IsConditionallyNavigable)
								{
									SerializableGuid serializableGuid3;
									if (this.AssociatedPropMap.TryGetValue(i, out serializableGuid3))
									{
										SerializableGuid serializableGuid4;
										if (!this.AssociatedPropMap.TryGetValue(num4, out serializableGuid4))
										{
											continue;
										}
										if (serializableGuid3 != serializableGuid4)
										{
											continue;
										}
									}
									else if (this.AssociatedPropMap.ContainsKey(num4))
									{
										continue;
									}
									if (!nativeHashSet.Contains(num4))
									{
										nativeQueue.Enqueue(num4);
										nativeHashSet.Add(num4);
									}
								}
							}
							goto IL_024A;
						}
					}
					IL_025A:;
				}
			}

			// Token: 0x040007F2 RID: 2034
			[ReadOnly]
			public NativeArray<Octant> Octants;

			// Token: 0x040007F3 RID: 2035
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> NeighborMap;

			// Token: 0x040007F4 RID: 2036
			[ReadOnly]
			public NativeParallelHashMap<int, SerializableGuid> AssociatedPropMap;

			// Token: 0x040007F5 RID: 2037
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int> IslandMap;

			// Token: 0x040007F6 RID: 2038
			[WriteOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;
		}

		// Token: 0x020001BC RID: 444
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct BuildSectionsJob : IJobParallelFor
		{
			// Token: 0x060009C8 RID: 2504 RVA: 0x0002D654 File Offset: 0x0002B854
			public void Execute(int index)
			{
				int num = this.IslandKeys[index];
				NativeList<int> nativeList = new NativeList<int>(1024, AllocatorManager.Temp);
				NativeList<int> nativeList2 = new NativeList<int>(1024, AllocatorManager.Temp);
				NativeList<int> nativeList3 = new NativeList<int>(1024, AllocatorManager.Temp);
				NativeList<int> nativeList4 = new NativeList<int>(1024, AllocatorManager.Temp);
				NativeList<int> nativeList5 = new NativeList<int>(1024, AllocatorManager.Temp);
				NativeList<int> nativeList6 = new NativeList<int>(16, AllocatorManager.Temp);
				NativeList<int> nativeList7 = new NativeList<int>(16, AllocatorManager.Temp);
				int num2 = 1;
				foreach (int num3 in this.IslandMap.GetValuesForKey(num))
				{
					if (this.Octants[num3].Size.x >= 1f)
					{
						nativeList.Add(in num3);
					}
					else
					{
						nativeList2.Add(in num3);
					}
				}
				nativeList.Sort(new OctantComparer(this.Octants));
				nativeList2.Sort(new OctantComparer(this.Octants));
				while (nativeList.Length > 0)
				{
					int num4 = nativeList[0];
					Octant octant = this.Octants[num4];
					nativeList3.Add(in num4);
					for (int i = 1; i < 5; i++)
					{
						for (int j = 0; j <= i; j++)
						{
							for (int k = 0; k <= i; k++)
							{
								if (j == i || k == i)
								{
									int num5;
									if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant.Center + new float3((float)j, 0f, (float)k), this.Octants, 1f, out num5) || !nativeList.Contains(num5))
									{
										goto IL_01FB;
									}
									nativeList4.Add(in num5);
								}
							}
						}
						foreach (int num6 in nativeList4)
						{
							nativeList3.Add(in num6);
						}
						nativeList4.Clear();
					}
					IL_01FB:
					if (nativeList3.Length > 1)
					{
						foreach (int num7 in nativeList3)
						{
							int num8 = nativeList.IndexOf(num7);
							if (num8 == -1)
							{
								throw new Exception("Something Broke");
							}
							nativeList.RemoveAt(num8);
							int sectionKey = BurstPathfindingUtilities.GetSectionKey(num, num2);
							this.SectionMap.Add(sectionKey, num7);
							this.IslandToSectionMap.Add(num, sectionKey);
							WalkableOctantData walkableOctantData = this.WalkableOctantDataMap[num7];
							walkableOctantData.SectionKey = sectionKey;
							this.WalkableOctantDataMap[num7] = walkableOctantData;
						}
						num2++;
					}
					else
					{
						nativeList.RemoveAt(0);
						nativeList5.Add(in num4);
					}
					nativeList4.Clear();
					nativeList3.Clear();
				}
				nativeList.AddRangeNoResize(nativeList5);
				nativeList5.Clear();
				while (nativeList.Length > 0)
				{
					int num9 = nativeList[0];
					Octant octant2 = this.Octants[num9];
					nativeList3.Add(in num9);
					int num10 = 0;
					int num11 = 0;
					int num12 = 1;
					int num13;
					while (num12 < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant2.Center + new float3((float)num12, 0f, 0f), this.Octants, 1f, out num13) && nativeList.Contains(num13))
					{
						num10++;
						num12++;
					}
					if (num10 == 4)
					{
						for (int l = 1; l <= num10; l++)
						{
							int num14;
							OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant2.Center + new float3((float)l, 0f, 0f), this.Octants, 1f, out num14);
							nativeList3.Add(in num14);
						}
					}
					else
					{
						int num15 = 1;
						int num16;
						while (num15 < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant2.Center + new float3(0f, 0f, (float)num15), this.Octants, 1f, out num16) && nativeList.Contains(num16))
						{
							num11++;
							num15++;
						}
						if (num11 > num10)
						{
							for (int m = 1; m <= num11; m++)
							{
								int num17;
								OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant2.Center + new float3(0f, 0f, (float)m), this.Octants, 1f, out num17);
								nativeList3.Add(in num17);
							}
						}
						else
						{
							for (int n = 1; n <= num10; n++)
							{
								int num18;
								OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant2.Center + new float3((float)n, 0f, 0f), this.Octants, 1f, out num18);
								nativeList3.Add(in num18);
							}
						}
					}
					foreach (int num19 in nativeList3)
					{
						int num20 = nativeList.IndexOf(num19);
						if (num20 == -1)
						{
							throw new Exception("Something Broke");
						}
						nativeList.RemoveAt(num20);
						int sectionKey2 = BurstPathfindingUtilities.GetSectionKey(num, num2);
						this.SectionMap.Add(sectionKey2, num19);
						this.IslandToSectionMap.Add(num, sectionKey2);
						WalkableOctantData walkableOctantData2 = this.WalkableOctantDataMap[num19];
						walkableOctantData2.SectionKey = sectionKey2;
						this.WalkableOctantDataMap[num19] = walkableOctantData2;
					}
					nativeList3.Clear();
					num2++;
				}
				while (nativeList2.Length > 0)
				{
					int num21 = nativeList2[0];
					ref Octant @ref = ref this.Octants.GetRef(num21);
					int num22;
					if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(@ref.Center, this.Octants, 1f, out num22))
					{
						throw new Exception("Malformed Octutree");
					}
					ref Octant ref2 = ref this.Octants.GetRef(num22);
					if (@ref.IsSlope)
					{
						OctreeHelperMethods.GetWalkableChildrenIndices(this.Octants, nativeList6, ref ref2);
						foreach (int num23 in nativeList6)
						{
							if (nativeList2.Contains(num23) && !nativeList3.Contains(num23))
							{
								nativeList3.Add(in num23);
							}
						}
						bool flag = true;
						int num24 = 0;
						int num25 = 0;
						int num26 = num22;
						int num27 = 0;
						int num28 = 1;
						int num29;
						while (num28 < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(ref2.Center + new float3((float)num28, 0f, 0f), this.Octants, 1f, out num29) && this.Octants.GetRef(num29).IsSlope && this.SlopeMap[num22] == this.SlopeMap[num29] && OctreeHelperMethods.AreChildrenNeighbors(num26, num29, this.Octants, this.NeighborMap, nativeList6, nativeList7))
						{
							num26 = num29;
							num27++;
							num28++;
						}
						num26 = num22;
						int num30 = 0;
						int num31 = 1;
						int num32;
						while (num31 < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(ref2.Center + new float3(0f, 0f, (float)num31), this.Octants, 1f, out num32) && this.Octants.GetRef(num32).IsSlope && this.SlopeMap[num22] == this.SlopeMap[num32] && OctreeHelperMethods.AreChildrenNeighbors(num26, num32, this.Octants, this.NeighborMap, nativeList6, nativeList7))
						{
							num26 = num32;
							num30++;
							num31++;
						}
						int num33;
						if (num30 > num27)
						{
							flag = false;
							num33 = num30;
						}
						else
						{
							num33 = num27;
						}
						for (int num34 = 0; num34 < 4; num34++)
						{
							num26 = num22;
							int num35 = 0;
							float3 zdirection = IslandGenerator.BuildSectionsJob.GetZDirection(num34);
							int num36 = 1;
							int num37;
							while (num36 < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(ref2.Center + zdirection * (float)num36, this.Octants, 1f, out num37) && this.Octants.GetRef(num37).IsSlope && this.SlopeMap[num22] == this.SlopeMap[num37] && OctreeHelperMethods.AreChildrenNeighbors(num26, num37, this.Octants, this.NeighborMap, nativeList6, nativeList7))
							{
								num26 = num37;
								num35++;
								num36++;
							}
							if (num35 > num24)
							{
								num25 = num34;
								num24 = num35;
							}
						}
						if (num33 >= num24)
						{
							float3 @float = (flag ? new float3(1f, 0f, 0f) : new float3(0f, 0f, 1f));
							for (int num38 = 1; num38 <= num33; num38++)
							{
								int num39;
								OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(ref2.Center + @float * (float)num38, this.Octants, 1f, out num39);
								ref Octant ref3 = ref this.Octants.GetRef(num39);
								OctreeHelperMethods.GetWalkableChildrenIndices(this.Octants, nativeList7, ref ref3);
								foreach (int num40 in nativeList7)
								{
									if (nativeList2.Contains(num40))
									{
										nativeList3.Add(in num40);
									}
								}
							}
						}
						else
						{
							float3 zdirection2 = IslandGenerator.BuildSectionsJob.GetZDirection(num25);
							for (int num41 = 1; num41 < num24; num41++)
							{
								int num42;
								OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(ref2.Center + zdirection2 * (float)num41, this.Octants, 1f, out num42);
								ref Octant ref4 = ref this.Octants.GetRef(num42);
								OctreeHelperMethods.GetWalkableChildrenIndices(this.Octants, nativeList7, ref ref4);
								foreach (int num43 in nativeList7)
								{
									if (nativeList2.Contains(num43))
									{
										nativeList3.Add(in num43);
									}
								}
							}
						}
					}
					else
					{
						nativeList3.Add(in num21);
						nativeList4.Add(in num21);
						while (nativeList4.Length > 0)
						{
							int num44 = nativeList4[0];
							nativeList4.RemoveAt(0);
							foreach (int num45 in this.NeighborMap.GetValuesForKey(num44))
							{
								Octant octant3 = this.Octants[num45];
								if (octant3.Size.x <= 0.25f && !octant3.IsSlope)
								{
									int num46;
									if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant3.Center, this.Octants, 1f, out num46))
									{
										throw new Exception("Malformed Octutree");
									}
									if (num22 == num46 && nativeList2.Contains(num45) && !nativeList4.Contains(num45) && !nativeList3.Contains(num45))
									{
										nativeList3.Add(in num45);
										nativeList4.Add(in num45);
									}
								}
							}
						}
					}
					foreach (int num47 in nativeList3)
					{
						int num48 = nativeList2.IndexOf(num47);
						if (num48 == -1)
						{
							throw new Exception("Something Broke");
						}
						nativeList2.RemoveAt(num48);
						int sectionKey3 = BurstPathfindingUtilities.GetSectionKey(num, num2);
						this.SectionMap.Add(sectionKey3, num47);
						this.IslandToSectionMap.Add(num, sectionKey3);
						WalkableOctantData walkableOctantData3 = this.WalkableOctantDataMap[num47];
						walkableOctantData3.SectionKey = sectionKey3;
						this.WalkableOctantDataMap[num47] = walkableOctantData3;
					}
					nativeList4.Clear();
					nativeList3.Clear();
					num2++;
				}
			}

			// Token: 0x060009C9 RID: 2505 RVA: 0x0002E268 File Offset: 0x0002C468
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

			// Token: 0x040007F7 RID: 2039
			[ReadOnly]
			public NativeArray<int> IslandKeys;

			// Token: 0x040007F8 RID: 2040
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> IslandMap;

			// Token: 0x040007F9 RID: 2041
			[ReadOnly]
			public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

			// Token: 0x040007FA RID: 2042
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> NeighborMap;

			// Token: 0x040007FB RID: 2043
			[NativeDisableParallelForRestriction]
			public NativeArray<Octant> Octants;

			// Token: 0x040007FC RID: 2044
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int>.ParallelWriter SectionMap;

			// Token: 0x040007FD RID: 2045
			[NativeDisableParallelForRestriction]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x040007FE RID: 2046
			[WriteOnly]
			public NativeParallelMultiHashMap<int, int>.ParallelWriter IslandToSectionMap;
		}

		// Token: 0x020001BD RID: 445
		[BurstCompile]
		private struct PruneNeighborsJob : IJob
		{
			// Token: 0x060009CA RID: 2506 RVA: 0x0002E2F0 File Offset: 0x0002C4F0
			public void Execute()
			{
				for (int i = 0; i < this.Octants.Length; i++)
				{
					if (this.WalkableOctantDataMap.ContainsKey(i))
					{
						this.WorkingSet.Clear();
						foreach (int num in this.NeighborMap.GetValuesForKey(i))
						{
							if (this.WalkableOctantDataMap[i].SectionKey == this.WalkableOctantDataMap[num].SectionKey)
							{
								this.WorkingSet.Add(num);
							}
						}
						foreach (int num2 in this.WorkingSet)
						{
							this.NeighborMap.Remove(i, num2);
						}
					}
				}
			}

			// Token: 0x040007FF RID: 2047
			[ReadOnly]
			public NativeArray<Octant> Octants;

			// Token: 0x04000800 RID: 2048
			[ReadOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x04000801 RID: 2049
			public NativeHashSet<int> WorkingSet;

			// Token: 0x04000802 RID: 2050
			public NativeParallelMultiHashMap<int, int> NeighborMap;
		}

		// Token: 0x020001BE RID: 446
		public struct Results
		{
			// Token: 0x04000803 RID: 2051
			public NativeList<int> SectionKeys;

			// Token: 0x04000804 RID: 2052
			public NativeList<int> IslandKeys;

			// Token: 0x04000805 RID: 2053
			public NativeParallelMultiHashMap<int, int> NeighborMap;

			// Token: 0x04000806 RID: 2054
			public NativeParallelMultiHashMap<int, int> IslandKeyToSectionKeyMap;

			// Token: 0x04000807 RID: 2055
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x04000808 RID: 2056
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x04000809 RID: 2057
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;
		}
	}
}
