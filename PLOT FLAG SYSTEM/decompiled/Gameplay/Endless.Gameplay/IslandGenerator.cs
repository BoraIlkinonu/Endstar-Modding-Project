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

namespace Endless.Gameplay;

public static class IslandGenerator
{
	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct FindNeighborsJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<Octant> Octants;

		[ReadOnly]
		public NavMeshQuery Query;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int>.ParallelWriter NeighborMap;

		public void Execute(int index)
		{
			Octant octant = Octants[index];
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
					if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant.Center + neighborOffset, Octants, x, out var index2))
					{
						Octant octant2 = Octants[index2];
						if (!(math.abs(octant2.Size.x - x) > 0.01f) && octant2.IsWalkable && !octant2.IsSlope)
						{
							NeighborMap.Add(index, index2);
						}
					}
				}
				for (int j = 0; j < 16; j++)
				{
					float3 bigToSmallNeighborOffset = OctreeHelperMethods.GetBigToSmallNeighborOffset(j);
					if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant.Center + bigToSmallNeighborOffset, Octants, x / 4f, out var index3))
					{
						Octant octant3 = Octants[index3];
						if (octant3.IsWalkable && !(octant3.Size.x > 0.25f) && OctreeHelperMethods.SweepNeighboringEdge(octant, octant3, Query))
						{
							NeighborMap.Add(index, index3);
							NeighborMap.Add(index3, index);
						}
					}
				}
				for (int k = 0; k < 16; k++)
				{
					float3 terrainToSmallBelowOffset = OctreeHelperMethods.GetTerrainToSmallBelowOffset(k);
					if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant.Center + terrainToSmallBelowOffset, Octants, x / 4f, out var index4))
					{
						Octant octant4 = Octants[index4];
						if (octant4.IsWalkable && !(octant4.Size.x > 0.25f) && OctreeHelperMethods.SweepNeighboringEdge(octant, octant4, Query))
						{
							NeighborMap.Add(index, index4);
							NeighborMap.Add(index4, index);
						}
					}
				}
				return;
			}
			for (int l = 0; l < 12; l++)
			{
				float3 subCellNeighborOffset = OctreeHelperMethods.GetSubCellNeighborOffset(l, octant.Size.x);
				if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant.Center + subCellNeighborOffset, Octants, octant.Size.x, out var index5))
				{
					continue;
				}
				Octant octant5 = Octants[index5];
				if (!(math.abs(octant5.Size.x - octant.Size.x) > 0.01f) && octant5.IsWalkable)
				{
					if (octant.IsSlope && octant5.IsSlope)
					{
						NeighborMap.Add(index, index5);
					}
					else if (OctreeHelperMethods.SweepNeighboringEdge(octant, octant5, Query))
					{
						NeighborMap.Add(index, index5);
					}
				}
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct BuildIslandsJob : IJob
	{
		[ReadOnly]
		public NativeArray<Octant> Octants;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> NeighborMap;

		[ReadOnly]
		public NativeParallelHashMap<int, SerializableGuid> AssociatedPropMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int> IslandMap;

		[WriteOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		public void Execute()
		{
			int num = 1;
			NativeQueue<int> nativeQueue = new NativeQueue<int>(AllocatorManager.Temp);
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(1024, AllocatorManager.Temp);
			for (int i = 0; i < Octants.Length; i++)
			{
				if (nativeHashSet.Contains(i))
				{
					continue;
				}
				Octant octant = Octants[i];
				if (!octant.IsValid)
				{
					break;
				}
				if (!octant.IsWalkable)
				{
					continue;
				}
				IslandMap.Add(num, i);
				WalkableOctantDataMap.Add(i, new WalkableOctantData
				{
					IslandKey = num
				});
				nativeHashSet.Add(i);
				foreach (int item5 in NeighborMap.GetValuesForKey(i))
				{
					Octant octant2 = Octants[item5];
					if (octant.IsConditionallyNavigable != octant2.IsConditionallyNavigable)
					{
						continue;
					}
					if (AssociatedPropMap.TryGetValue(i, out var item))
					{
						if (!AssociatedPropMap.TryGetValue(item5, out var item2) || item != item2)
						{
							continue;
						}
					}
					else if (AssociatedPropMap.ContainsKey(item5))
					{
						continue;
					}
					nativeQueue.Enqueue(item5);
					nativeHashSet.Add(item5);
				}
				while (!nativeQueue.IsEmpty())
				{
					int num2 = nativeQueue.Dequeue();
					Octant octant3 = Octants[num2];
					IslandMap.Add(num, num2);
					if (!WalkableOctantDataMap.TryAdd(num2, new WalkableOctantData
					{
						IslandKey = num
					}))
					{
						Debug.LogError("Malformed octutree.");
					}
					foreach (int item6 in NeighborMap.GetValuesForKey(num2))
					{
						Octant octant4 = Octants[item6];
						if (octant3.IsConditionallyNavigable != octant4.IsConditionallyNavigable)
						{
							continue;
						}
						if (AssociatedPropMap.TryGetValue(i, out var item3))
						{
							if (!AssociatedPropMap.TryGetValue(item6, out var item4) || item3 != item4)
							{
								continue;
							}
						}
						else if (AssociatedPropMap.ContainsKey(item6))
						{
							continue;
						}
						if (!nativeHashSet.Contains(item6))
						{
							nativeQueue.Enqueue(item6);
							nativeHashSet.Add(item6);
						}
					}
				}
				num++;
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct BuildSectionsJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<int> IslandKeys;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> IslandMap;

		[ReadOnly]
		public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> NeighborMap;

		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int>.ParallelWriter SectionMap;

		[NativeDisableParallelForRestriction]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int>.ParallelWriter IslandToSectionMap;

		public void Execute(int index)
		{
			int num = IslandKeys[index];
			NativeList<int> list = new NativeList<int>(1024, AllocatorManager.Temp);
			NativeList<int> list2 = new NativeList<int>(1024, AllocatorManager.Temp);
			NativeList<int> list3 = new NativeList<int>(1024, AllocatorManager.Temp);
			NativeList<int> list4 = new NativeList<int>(1024, AllocatorManager.Temp);
			NativeList<int> list5 = new NativeList<int>(1024, AllocatorManager.Temp);
			NativeList<int> nativeList = new NativeList<int>(16, AllocatorManager.Temp);
			NativeList<int> nativeList2 = new NativeList<int>(16, AllocatorManager.Temp);
			int num2 = 1;
			foreach (int item in IslandMap.GetValuesForKey(num))
			{
				int value = item;
				if (Octants[value].Size.x >= 1f)
				{
					list.Add(in value);
				}
				else
				{
					list2.Add(in value);
				}
			}
			list.Sort(new OctantComparer(Octants));
			list2.Sort(new OctantComparer(Octants));
			while (list.Length > 0)
			{
				int value2 = list[0];
				Octant octant = Octants[value2];
				list3.Add(in value2);
				for (int i = 1; i < 5; i++)
				{
					for (int j = 0; j <= i; j++)
					{
						for (int k = 0; k <= i; k++)
						{
							if (j == i || k == i)
							{
								if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant.Center + new float3(j, 0f, k), Octants, 1f, out var index2) || !list.Contains(index2))
								{
									goto end_IL_01f3;
								}
								list4.Add(in index2);
							}
						}
					}
					foreach (int item2 in list4)
					{
						list3.Add(item2);
					}
					list4.Clear();
					continue;
					end_IL_01f3:
					break;
				}
				if (list3.Length > 1)
				{
					foreach (int item3 in list3)
					{
						int num3 = list.IndexOf(item3);
						if (num3 == -1)
						{
							throw new Exception("Something Broke");
						}
						list.RemoveAt(num3);
						int sectionKey = BurstPathfindingUtilities.GetSectionKey(num, num2);
						SectionMap.Add(sectionKey, item3);
						IslandToSectionMap.Add(num, sectionKey);
						WalkableOctantData value3 = WalkableOctantDataMap[item3];
						value3.SectionKey = sectionKey;
						WalkableOctantDataMap[item3] = value3;
					}
					num2++;
				}
				else
				{
					list.RemoveAt(0);
					list5.Add(in value2);
				}
				list4.Clear();
				list3.Clear();
			}
			list.AddRangeNoResize(list5);
			list5.Clear();
			while (list.Length > 0)
			{
				int value4 = list[0];
				Octant octant2 = Octants[value4];
				list3.Add(in value4);
				int num4 = 0;
				int num5 = 0;
				int index3;
				for (int l = 1; l < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant2.Center + new float3(l, 0f, 0f), Octants, 1f, out index3); l++)
				{
					if (!list.Contains(index3))
					{
						break;
					}
					num4++;
				}
				if (num4 == 4)
				{
					for (int m = 1; m <= num4; m++)
					{
						OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant2.Center + new float3(m, 0f, 0f), Octants, 1f, out var index4);
						list3.Add(in index4);
					}
				}
				else
				{
					int index5;
					for (int n = 1; n < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant2.Center + new float3(0f, 0f, n), Octants, 1f, out index5); n++)
					{
						if (!list.Contains(index5))
						{
							break;
						}
						num5++;
					}
					if (num5 > num4)
					{
						for (int num6 = 1; num6 <= num5; num6++)
						{
							OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant2.Center + new float3(0f, 0f, num6), Octants, 1f, out var index6);
							list3.Add(in index6);
						}
					}
					else
					{
						for (int num7 = 1; num7 <= num4; num7++)
						{
							OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant2.Center + new float3(num7, 0f, 0f), Octants, 1f, out var index7);
							list3.Add(in index7);
						}
					}
				}
				foreach (int item4 in list3)
				{
					int num8 = list.IndexOf(item4);
					if (num8 == -1)
					{
						throw new Exception("Something Broke");
					}
					list.RemoveAt(num8);
					int sectionKey2 = BurstPathfindingUtilities.GetSectionKey(num, num2);
					SectionMap.Add(sectionKey2, item4);
					IslandToSectionMap.Add(num, sectionKey2);
					WalkableOctantData value5 = WalkableOctantDataMap[item4];
					value5.SectionKey = sectionKey2;
					WalkableOctantDataMap[item4] = value5;
				}
				list3.Clear();
				num2++;
			}
			while (list2.Length > 0)
			{
				int value6 = list2[0];
				ref Octant reference = ref Octants.GetRef(value6);
				if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference.Center, Octants, 1f, out var index8))
				{
					throw new Exception("Malformed Octutree");
				}
				ref Octant reference2 = ref Octants.GetRef(index8);
				if (reference.IsSlope)
				{
					OctreeHelperMethods.GetWalkableChildrenIndices(Octants, nativeList, ref reference2);
					foreach (int item5 in nativeList)
					{
						int value7 = item5;
						if (list2.Contains(value7) && !list3.Contains(value7))
						{
							list3.Add(in value7);
						}
					}
					bool flag = true;
					int num9 = 0;
					int i2 = 0;
					int octantIndex = index8;
					int num10 = 0;
					int index9;
					for (int num11 = 1; num11 < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference2.Center + new float3(num11, 0f, 0f), Octants, 1f, out index9); num11++)
					{
						if (!Octants.GetRef(index9).IsSlope)
						{
							break;
						}
						if (SlopeMap[index8] != SlopeMap[index9])
						{
							break;
						}
						if (!OctreeHelperMethods.AreChildrenNeighbors(octantIndex, index9, Octants, NeighborMap, nativeList, nativeList2))
						{
							break;
						}
						octantIndex = index9;
						num10++;
					}
					octantIndex = index8;
					int num12 = 0;
					int index10;
					for (int num13 = 1; num13 < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference2.Center + new float3(0f, 0f, num13), Octants, 1f, out index10); num13++)
					{
						if (!Octants.GetRef(index10).IsSlope)
						{
							break;
						}
						if (SlopeMap[index8] != SlopeMap[index10])
						{
							break;
						}
						if (!OctreeHelperMethods.AreChildrenNeighbors(octantIndex, index10, Octants, NeighborMap, nativeList, nativeList2))
						{
							break;
						}
						octantIndex = index10;
						num12++;
					}
					int num14;
					if (num12 > num10)
					{
						flag = false;
						num14 = num12;
					}
					else
					{
						num14 = num10;
					}
					for (int num15 = 0; num15 < 4; num15++)
					{
						octantIndex = index8;
						int num16 = 0;
						float3 zDirection = GetZDirection(num15);
						int index11;
						for (int num17 = 1; num17 < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference2.Center + zDirection * num17, Octants, 1f, out index11); num17++)
						{
							if (!Octants.GetRef(index11).IsSlope)
							{
								break;
							}
							if (SlopeMap[index8] != SlopeMap[index11])
							{
								break;
							}
							if (!OctreeHelperMethods.AreChildrenNeighbors(octantIndex, index11, Octants, NeighborMap, nativeList, nativeList2))
							{
								break;
							}
							octantIndex = index11;
							num16++;
						}
						if (num16 > num9)
						{
							i2 = num15;
							num9 = num16;
						}
					}
					if (num14 >= num9)
					{
						float3 @float = (flag ? new float3(1f, 0f, 0f) : new float3(0f, 0f, 1f));
						for (int num18 = 1; num18 <= num14; num18++)
						{
							OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference2.Center + @float * num18, Octants, 1f, out var index12);
							ref Octant octant3 = ref Octants.GetRef(index12);
							OctreeHelperMethods.GetWalkableChildrenIndices(Octants, nativeList2, ref octant3);
							foreach (int item6 in nativeList2)
							{
								int value8 = item6;
								if (list2.Contains(value8))
								{
									list3.Add(in value8);
								}
							}
						}
					}
					else
					{
						float3 zDirection2 = GetZDirection(i2);
						for (int num19 = 1; num19 < num9; num19++)
						{
							OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference2.Center + zDirection2 * num19, Octants, 1f, out var index13);
							ref Octant octant4 = ref Octants.GetRef(index13);
							OctreeHelperMethods.GetWalkableChildrenIndices(Octants, nativeList2, ref octant4);
							foreach (int item7 in nativeList2)
							{
								int value9 = item7;
								if (list2.Contains(value9))
								{
									list3.Add(in value9);
								}
							}
						}
					}
				}
				else
				{
					list3.Add(in value6);
					list4.Add(in value6);
					while (list4.Length > 0)
					{
						int key = list4[0];
						list4.RemoveAt(0);
						foreach (int item8 in NeighborMap.GetValuesForKey(key))
						{
							int value10 = item8;
							Octant octant5 = Octants[value10];
							if (!(octant5.Size.x > 0.25f) && !octant5.IsSlope)
							{
								if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant5.Center, Octants, 1f, out var index14))
								{
									throw new Exception("Malformed Octutree");
								}
								if (index8 == index14 && list2.Contains(value10) && !list4.Contains(value10) && !list3.Contains(value10))
								{
									list3.Add(in value10);
									list4.Add(in value10);
								}
							}
						}
					}
				}
				foreach (int item9 in list3)
				{
					int num20 = list2.IndexOf(item9);
					if (num20 == -1)
					{
						throw new Exception("Something Broke");
					}
					list2.RemoveAt(num20);
					int sectionKey3 = BurstPathfindingUtilities.GetSectionKey(num, num2);
					SectionMap.Add(sectionKey3, item9);
					IslandToSectionMap.Add(num, sectionKey3);
					WalkableOctantData value11 = WalkableOctantDataMap[item9];
					value11.SectionKey = sectionKey3;
					WalkableOctantDataMap[item9] = value11;
				}
				list4.Clear();
				list3.Clear();
				num2++;
			}
		}

		private static float3 GetZDirection(int i)
		{
			return i switch
			{
				0 => new float3(1f, 1f, 0f), 
				1 => new float3(0f, 1f, 1f), 
				2 => new float3(-1f, 1f, 0f), 
				3 => new float3(0f, 1f, -1f), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}

	[BurstCompile]
	private struct PruneNeighborsJob : IJob
	{
		[ReadOnly]
		public NativeArray<Octant> Octants;

		[ReadOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		public NativeHashSet<int> WorkingSet;

		public NativeParallelMultiHashMap<int, int> NeighborMap;

		public void Execute()
		{
			for (int i = 0; i < Octants.Length; i++)
			{
				if (!WalkableOctantDataMap.ContainsKey(i))
				{
					continue;
				}
				WorkingSet.Clear();
				foreach (int item in NeighborMap.GetValuesForKey(i))
				{
					if (WalkableOctantDataMap[i].SectionKey == WalkableOctantDataMap[item].SectionKey)
					{
						WorkingSet.Add(item);
					}
				}
				foreach (int item2 in WorkingSet)
				{
					NeighborMap.Remove(i, item2);
				}
			}
		}
	}

	public struct Results
	{
		public NativeList<int> SectionKeys;

		public NativeList<int> IslandKeys;

		public NativeParallelMultiHashMap<int, int> NeighborMap;

		public NativeParallelMultiHashMap<int, int> IslandKeyToSectionKeyMap;

		public NativeParallelMultiHashMap<int, int> SectionMap;

		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;
	}

	public static IEnumerator GenerateIslands(NativeArray<Octant> octants, NativeParallelHashMap<int, SerializableGuid> associatedPropMap, NativeParallelHashMap<int, SlopeNeighbors> slopeMap, NavMeshQuery query, int numWalkableOctants, Action<Results> getResults)
	{
		NativeParallelMultiHashMap<int, int> neighborMap = new NativeParallelMultiHashMap<int, int>(numWalkableOctants * 16, AllocatorManager.Persistent);
		JobHandle dependsOn = new FindNeighborsJob
		{
			Octants = octants,
			Query = query,
			NeighborMap = neighborMap.AsParallelWriter()
		}.Schedule(innerloopBatchCount: octants.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches, arrayLength: octants.Length);
		NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap = new NativeParallelHashMap<int, WalkableOctantData>(numWalkableOctants, AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> islandMap = new NativeParallelMultiHashMap<int, int>(numWalkableOctants, AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> sectionMap = new NativeParallelMultiHashMap<int, int>(numWalkableOctants, AllocatorManager.Persistent);
		NativeList<int> islandKeys = new NativeList<int>(1024, AllocatorManager.Persistent);
		BuildIslandsJob jobData = new BuildIslandsJob
		{
			Octants = octants,
			NeighborMap = neighborMap,
			AssociatedPropMap = associatedPropMap,
			IslandMap = islandMap,
			WalkableOctantDataMap = walkableOctantDataMap
		};
		GetUniqueKeysJob jobData2 = new GetUniqueKeysJob
		{
			RawMultiMap = islandMap,
			UniqueKeys = islandKeys
		};
		dependsOn = jobData.Schedule(dependsOn);
		dependsOn = jobData2.Schedule(dependsOn);
		yield return JobUtilities.WaitForJobToComplete(dependsOn);
		NativeParallelMultiHashMap<int, int> islandKeyToSectionKeyMap = new NativeParallelMultiHashMap<int, int>(numWalkableOctants, AllocatorManager.Persistent);
		NativeList<int> sectionKeys = new NativeList<int>(1024, AllocatorManager.Persistent);
		BuildSectionsJob jobData3 = new BuildSectionsJob
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
		GetUniqueKeysJob jobData4 = new GetUniqueKeysJob
		{
			RawMultiMap = sectionMap,
			UniqueKeys = sectionKeys
		};
		JobHandle inputDeps = IJobParallelForExtensions.Schedule(innerloopBatchCount: islandKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches, jobData: jobData3, arrayLength: islandKeys.Length);
		inputDeps = islandMap.Dispose(inputDeps);
		inputDeps = jobData4.Schedule(inputDeps);
		yield return JobUtilities.WaitForJobToComplete(inputDeps);
		NativeHashSet<int> workingSet = new NativeHashSet<int>(16, AllocatorManager.Persistent);
		NativeParallelHashMap<int, SectionSurface> surfaceMap = new NativeParallelHashMap<int, SectionSurface>(sectionKeys.Length, AllocatorManager.Persistent);
		FindEdgeOctantsJob jobData5 = new FindEdgeOctantsJob
		{
			SectionKeys = sectionKeys,
			SectionMap = sectionMap,
			NeighborMap = neighborMap,
			Octants = octants,
			WalkableOctantDataMap = walkableOctantDataMap
		};
		PruneNeighborsJob jobData6 = new PruneNeighborsJob
		{
			Octants = octants,
			NeighborMap = neighborMap,
			WalkableOctantDataMap = walkableOctantDataMap,
			WorkingSet = workingSet
		};
		BuildSurfacesJob jobData7 = new BuildSurfacesJob
		{
			Octants = octants,
			SectionMap = sectionMap,
			SectionKeyArray = sectionKeys,
			Query = query,
			SurfaceMap = surfaceMap.AsParallelWriter()
		};
		int innerloopBatchCount = sectionKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
		JobHandle dependsOn2 = IJobParallelForExtensions.Schedule(jobData5, sectionKeys.Length, innerloopBatchCount);
		dependsOn2 = jobData6.Schedule(dependsOn2);
		workingSet.Dispose(dependsOn2);
		dependsOn2 = IJobParallelForExtensions.Schedule(jobData7, sectionKeys.Length, innerloopBatchCount, dependsOn2);
		yield return JobUtilities.WaitForJobToComplete(dependsOn2);
		getResults(new Results
		{
			IslandKeys = islandKeys,
			SectionKeys = sectionKeys,
			NeighborMap = neighborMap,
			IslandKeyToSectionKeyMap = islandKeyToSectionKeyMap,
			SectionMap = sectionMap,
			SurfaceMap = surfaceMap,
			WalkableOctantDataMap = walkableOctantDataMap
		});
	}
}
