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

namespace Endless.Gameplay;

public static class NavGraphUpdateGenerator
{
	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct ClearAssociatedCellDataJob : IJob
	{
		[ReadOnly]
		public NativeArray<int3> CellsToClear;

		public NativeArray<Octant> Octants;

		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		public NativeParallelMultiHashMap<int, int> SectionMap;

		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		public NativeParallelMultiHashMap<int, int> IslandToSectionMap;

		public NativeParallelHashMap<int, GraphNode> NodeGraph;

		public NativeParallelMultiHashMap<int, Edge> EdgeGraph;

		[WriteOnly]
		public NativeQueue<int> UpdateOctantIndexes;

		[WriteOnly]
		public NativeQueue<int> ReleasedIslandKeys;

		public void Execute()
		{
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(SurfaceMap.Capacity, AllocatorManager.Temp);
			foreach (int3 item in CellsToClear)
			{
				ref Octant smallestOctantForPoint = ref OctreeHelperMethods.GetSmallestOctantForPoint(item, Octants, 1f);
				if (!smallestOctantForPoint.HasChildren)
				{
					continue;
				}
				for (int i = 0; i < 8; i++)
				{
					smallestOctantForPoint.TryGetChildIndex(i, out var key);
					ref Octant reference = ref Octants.GetRef(key);
					for (int j = 0; j < 8; j++)
					{
						reference.TryGetChildIndex(j, out var key2);
						Octants.GetRef(key2).IsBlocking = false;
						UpdateOctantIndexes.Enqueue(key2);
						if (WalkableOctantDataMap.ContainsKey(key2))
						{
							nativeHashSet.Add(WalkableOctantDataMap[key2].IslandKey);
						}
					}
				}
			}
			foreach (int item2 in nativeHashSet)
			{
				foreach (int item3 in IslandToSectionMap.GetValuesForKey(item2))
				{
					foreach (int item4 in SectionMap.GetValuesForKey(item3))
					{
						UpdateOctantIndexes.Enqueue(item4);
						WalkableOctantDataMap.Remove(item4);
						ref Octant reference2 = ref Octants.GetRef(item4);
						ref Octant smallestOctantForPoint2 = ref OctreeHelperMethods.GetSmallestOctantForPoint(reference2.Center, Octants, 1f);
						ref Octant smallestOctantForPoint3 = ref OctreeHelperMethods.GetSmallestOctantForPoint(reference2.Center, Octants, 0.5f);
						bool hasWalkableChildren = false;
						for (int k = 0; k < 8; k++)
						{
							if (smallestOctantForPoint3.TryGetChildIndex(k, out var key3) && Octants[key3].IsWalkable)
							{
								hasWalkableChildren = true;
								break;
							}
						}
						smallestOctantForPoint3.HasWalkableChildren = hasWalkableChildren;
						hasWalkableChildren = false;
						for (int l = 0; l < 8; l++)
						{
							if (smallestOctantForPoint2.TryGetChildIndex(l, out var key4) && Octants[key4].IsWalkable)
							{
								hasWalkableChildren = true;
								break;
							}
						}
						smallestOctantForPoint2.HasWalkableChildren = hasWalkableChildren;
						reference2.IsWalkable = false;
						reference2.IsBlocking = false;
					}
					SectionMap.Remove(item3);
					SurfaceMap.Remove(item3);
					NativeList<(int, Edge)> nativeList = new NativeList<(int, Edge)>(64, AllocatorManager.Temp);
					foreach (KeyValue<int, Edge> item5 in EdgeGraph)
					{
						if (item5.Value.ConnectedNodeKey == item3)
						{
							nativeList.Add((item5.Key, item5.Value));
						}
					}
					EdgeGraph.Remove(item3);
					foreach (var item6 in nativeList)
					{
						EdgeGraph.Remove(item6.Item1, item6.Item2);
					}
					NodeGraph.Remove(item3);
				}
				IslandToSectionMap.Remove(item2);
				ReleasedIslandKeys.Enqueue(item2);
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct ConstructBoxcastCommandsJob : IJob
	{
		[ReadOnly]
		public NativeArray<int> OctantsToUpdate;

		public NativeArray<Octant> Octants;

		[WriteOnly]
		public NativeArray<OverlapBoxCommand> BlockingOverlapBoxCommands;

		[WriteOnly]
		public NativeArray<OverlapBoxCommand> WalkableOverlapBoxCommands;

		public QueryParameters QueryParameters;

		public void Execute()
		{
			for (int i = 0; i < OctantsToUpdate.Length; i++)
			{
				int index = OctantsToUpdate[i];
				ref Octant reference = ref Octants.GetRef(index);
				Vector3 halfExtents = new Vector3(0.1f, 0.1f, 0.1f);
				Vector3 halfExtents2 = new Vector3(0.001f, 0.001f, 0.001f);
				BlockingOverlapBoxCommands[i] = new OverlapBoxCommand(reference.Center, halfExtents, Quaternion.identity, QueryParameters);
				WalkableOverlapBoxCommands[i] = new OverlapBoxCommand(reference.Center.ToVector3() + Vector3.up, halfExtents2, Quaternion.identity, QueryParameters);
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct UpdateOctantData : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<int> OctantsToUpdate;

		[ReadOnly]
		public NativeArray<ColliderHit> BlockingResults;

		[ReadOnly]
		public NativeArray<ColliderHit> WalkableResults;

		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		[NativeDisableParallelForRestriction]
		public NavMeshQuery Query;

		public int octantDivisions;

		public float MinSplitCellAreaPercentage;

		public void Execute(int index)
		{
			int index2 = OctantsToUpdate[index];
			ref Octant reference = ref Octants.GetRef(index2);
			Vector3 vector = new Vector3(0f, -0.001f, 0f);
			Vector3 extents = new Vector3(0.001f, 0.125f, 0.001f);
			int num = octantDivisions * octantDivisions;
			int num2 = 0;
			NativeList<Vector3> samplePositions = new NativeList<Vector3>(num, AllocatorManager.Temp);
			OctreeHelperMethods.GetSamplePositions(samplePositions, reference.Min, reference.Max, octantDivisions);
			foreach (Vector3 item in samplePositions)
			{
				NavMeshLocation location = Query.MapLocation(item + vector, extents, 0);
				if (Query.IsValid(location))
				{
					num2++;
				}
			}
			if ((float)num2 / (float)num > MinSplitCellAreaPercentage && WalkableResults[index].instanceID == 0)
			{
				reference.IsWalkable = true;
				OctreeHelperMethods.GetSmallestOctantForPoint(reference.Center, Octants, 1f).HasWalkableChildren = true;
			}
			else if (BlockingResults[index].instanceID != 0)
			{
				reference.IsBlocking = true;
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct FindNeighborsJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<int> OctantsToUpdate;

		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		[ReadOnly]
		public NavMeshQuery Query;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int>.ParallelWriter NeighborMap;

		public void Execute(int index)
		{
			int num = OctantsToUpdate[index];
			ref Octant reference = ref Octants.GetRef(num);
			if (!reference.IsWalkable)
			{
				return;
			}
			for (int i = 0; i < 12; i++)
			{
				float3 subCellNeighborOffset = OctreeHelperMethods.GetSubCellNeighborOffset(i, reference.Size.x);
				if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference.Center + subCellNeighborOffset, Octants, reference.Size.x, out var index2))
				{
					continue;
				}
				ref Octant reference2 = ref Octants.GetRef(index2);
				if (reference2.IsWalkable)
				{
					if (reference.IsSlope && reference2.IsSlope)
					{
						NeighborMap.Add(num, index2);
						NeighborMap.Add(index2, num);
					}
					else if (OctreeHelperMethods.SweepNeighboringEdge(reference, reference2, Query))
					{
						NeighborMap.Add(num, index2);
						NeighborMap.Add(index2, num);
					}
				}
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct CleanNeighborsJob : IJob
	{
		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> RawNeighborMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int> NeighborMap;

		public void Execute()
		{
			(NativeArray<int>, int) uniqueKeyArray = RawNeighborMap.GetUniqueKeyArray(AllocatorManager.Temp);
			for (int i = 0; i < uniqueKeyArray.Item2; i++)
			{
				int key = uniqueKeyArray.Item1[i];
				NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(RawNeighborMap.CountValuesForKey(key), AllocatorManager.Temp);
				foreach (int item in RawNeighborMap.GetValuesForKey(key))
				{
					nativeHashSet.Add(item);
				}
				foreach (int item2 in nativeHashSet)
				{
					NeighborMap.Add(key, item2);
				}
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct BuildIslandsJob : IJob
	{
		[ReadOnly]
		public NativeArray<int> OctantsToUpdate;

		[ReadOnly]
		public NativeParallelHashMap<int, SerializableGuid> AssociatedPropMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> NeighborMap;

		public NativeQueue<int> ReleasedIslandKeys;

		public int NextIslandKey;

		public NativeArray<Octant> Octants;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int> IslandMap;

		[WriteOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		public NativeReference<int> NumOctantsOfLargestIsland;

		public void Execute()
		{
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Temp);
			NativeQueue<int> nativeQueue = new NativeQueue<int>(AllocatorManager.Temp);
			for (int i = 0; i < OctantsToUpdate.Length; i++)
			{
				int num = OctantsToUpdate[i];
				if (nativeHashSet.Contains(num) || !Octants.GetRef(num).IsWalkable)
				{
					continue;
				}
				int nextIslandKey = GetNextIslandKey();
				int num2 = 1;
				IslandMap.Add(nextIslandKey, num);
				AddWalkableOctantData(num, nextIslandKey);
				nativeHashSet.Add(num);
				foreach (int item5 in NeighborMap.GetValuesForKey(num))
				{
					if (AssociatedPropMap.TryGetValue(num, out var item))
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
					int num3 = nativeQueue.Dequeue();
					IslandMap.Add(nextIslandKey, num3);
					num2++;
					AddWalkableOctantData(num3, nextIslandKey);
					foreach (int item6 in NeighborMap.GetValuesForKey(num3))
					{
						if (AssociatedPropMap.TryGetValue(num, out var item3))
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
				if (NumOctantsOfLargestIsland.Value < num2)
				{
					NumOctantsOfLargestIsland.Value = num2;
				}
			}
		}

		private void AddWalkableOctantData(int octantIndex, int currentIsland)
		{
			if (!WalkableOctantDataMap.TryAdd(octantIndex, new WalkableOctantData
			{
				IslandKey = currentIsland
			}))
			{
				WalkableOctantDataMap[octantIndex] = new WalkableOctantData
				{
					IslandKey = currentIsland
				};
			}
		}

		private int GetNextIslandKey()
		{
			if (ReleasedIslandKeys.Count > 0)
			{
				return ReleasedIslandKeys.Dequeue();
			}
			int nextIslandKey = NextIslandKey;
			NextIslandKey++;
			return nextIslandKey;
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct BuildSectionsJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> IslandMap;

		[ReadOnly]
		public NativeArray<int> IslandKeys;

		[ReadOnly]
		public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> NeighborMap;

		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int>.ParallelWriter SectionMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int>.ParallelWriter NewSectionMap;

		[NativeDisableParallelForRestriction]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int>.ParallelWriter IslandToSectionMap;

		public void Execute(int index)
		{
			int num = IslandKeys[index];
			NativeList<int> list = new NativeList<int>(1024, AllocatorManager.Temp);
			NativeList<int> list2 = new NativeList<int>(1024, AllocatorManager.Temp);
			NativeList<int> list3 = new NativeList<int>(64, AllocatorManager.Temp);
			NativeList<int> nativeList = new NativeList<int>(16, AllocatorManager.Temp);
			NativeList<int> nativeList2 = new NativeList<int>(16, AllocatorManager.Temp);
			int num2 = 1;
			foreach (int item in IslandMap.GetValuesForKey(num))
			{
				list.Add(item);
			}
			list.Sort(new OctantComparer(Octants));
			while (list.Length > 0)
			{
				int value = list[0];
				ref Octant reference = ref Octants.GetRef(value);
				if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference.Center, Octants, 1f, out var index2))
				{
					throw new Exception("Malformed Octutree");
				}
				ref Octant reference2 = ref Octants.GetRef(index2);
				if (reference.IsSlope)
				{
					OctreeHelperMethods.GetWalkableChildrenIndices(Octants, nativeList, ref reference2);
					foreach (int item2 in nativeList)
					{
						int value2 = item2;
						if (!list2.Contains(value2) && list.Contains(value2))
						{
							list2.Add(in value2);
						}
					}
					bool flag = true;
					int num3 = 0;
					int i = 0;
					int octantIndex = index2;
					int num4 = 0;
					int index3;
					for (int j = 1; j < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference2.Center + new float3(j, 0f, 0f), Octants, 1f, out index3); j++)
					{
						if (!Octants.GetRef(index3).IsSlope)
						{
							break;
						}
						if (SlopeMap[index2] != SlopeMap[index3])
						{
							break;
						}
						if (!OctreeHelperMethods.AreChildrenNeighbors(octantIndex, index3, Octants, NeighborMap, nativeList, nativeList2))
						{
							break;
						}
						octantIndex = index3;
						num4++;
					}
					octantIndex = index2;
					int num5 = 0;
					int index4;
					for (int k = 1; k < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference2.Center + new float3(0f, 0f, k), Octants, 1f, out index4); k++)
					{
						if (!Octants.GetRef(index4).IsSlope)
						{
							break;
						}
						if (SlopeMap[index2] != SlopeMap[index4])
						{
							break;
						}
						if (!OctreeHelperMethods.AreChildrenNeighbors(octantIndex, index4, Octants, NeighborMap, nativeList, nativeList2))
						{
							break;
						}
						octantIndex = index4;
						num5++;
					}
					int num6;
					if (num5 > num4)
					{
						flag = false;
						num6 = num5;
					}
					else
					{
						num6 = num4;
					}
					for (int l = 0; l < 4; l++)
					{
						octantIndex = index2;
						int num7 = 0;
						float3 zDirection = GetZDirection(l);
						int index5;
						for (int m = 1; m < 5 && OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference2.Center + zDirection * m, Octants, 1f, out index5); m++)
						{
							if (!Octants.GetRef(index5).IsSlope)
							{
								break;
							}
							if (SlopeMap[index2] != SlopeMap[index5])
							{
								break;
							}
							if (!OctreeHelperMethods.AreChildrenNeighbors(octantIndex, index5, Octants, NeighborMap, nativeList, nativeList2))
							{
								break;
							}
							octantIndex = index5;
							num7++;
						}
						if (num7 > num3)
						{
							i = l;
							num3 = num7;
						}
					}
					if (num6 >= num3)
					{
						float3 @float = (flag ? new float3(1f, 0f, 0f) : new float3(0f, 0f, 1f));
						for (int n = 1; n <= num6; n++)
						{
							OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference2.Center + @float * n, Octants, 1f, out var index6);
							ref Octant octant = ref Octants.GetRef(index6);
							OctreeHelperMethods.GetWalkableChildrenIndices(Octants, nativeList2, ref octant);
							foreach (int item3 in nativeList2)
							{
								int value3 = item3;
								if (list.Contains(value3))
								{
									list2.Add(in value3);
								}
							}
						}
					}
					else
					{
						float3 zDirection2 = GetZDirection(i);
						for (int num8 = 1; num8 < num3; num8++)
						{
							OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(reference2.Center + zDirection2 * num8, Octants, 1f, out var index7);
							ref Octant octant2 = ref Octants.GetRef(index7);
							OctreeHelperMethods.GetWalkableChildrenIndices(Octants, nativeList2, ref octant2);
							foreach (int item4 in nativeList2)
							{
								int value4 = item4;
								if (list.Contains(value4))
								{
									list2.Add(in value4);
								}
							}
						}
					}
				}
				else
				{
					list2.Add(in value);
					list3.Add(in value);
					while (list3.Length > 0)
					{
						int key = list3[0];
						list3.RemoveAt(0);
						foreach (int item5 in NeighborMap.GetValuesForKey(key))
						{
							Octant octant3 = Octants[item5];
							if (!(octant3.Size.x > 0.25f) && !octant3.IsSlope)
							{
								if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(octant3.Center, Octants, 1f, out var index8))
								{
									throw new Exception("Malformed Octutree");
								}
								if (index2 == index8 && list.Contains(item5) && !list3.Contains(item5) && !list2.Contains(item5))
								{
									list2.AddNoResize(item5);
									list3.AddNoResize(item5);
								}
							}
						}
					}
				}
				foreach (int item6 in list2)
				{
					int num9 = list.IndexOf(item6);
					if (num9 == -1)
					{
						throw new Exception("Malformed Sections");
					}
					list.RemoveAt(num9);
					int sectionKey = BurstPathfindingUtilities.GetSectionKey(num, num2);
					SectionMap.Add(sectionKey, item6);
					NewSectionMap.Add(sectionKey, item6);
					IslandToSectionMap.Add(num, sectionKey);
					WalkableOctantData value5 = WalkableOctantDataMap[item6];
					value5.SectionKey = sectionKey;
					WalkableOctantDataMap[item6] = value5;
				}
				list3.Clear();
				list2.Clear();
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
		public NativeArray<int> OctantsToUpdate;

		[ReadOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		public NativeParallelMultiHashMap<int, int> NeighborMap;

		public void Execute()
		{
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(16, AllocatorManager.Temp);
			foreach (int item in OctantsToUpdate)
			{
				if (!WalkableOctantDataMap.ContainsKey(item))
				{
					continue;
				}
				nativeHashSet.Clear();
				foreach (int item2 in NeighborMap.GetValuesForKey(item))
				{
					if (WalkableOctantDataMap[item].SectionKey == WalkableOctantDataMap[item2].SectionKey)
					{
						nativeHashSet.Add(item2);
					}
				}
				foreach (int item3 in nativeHashSet)
				{
					NeighborMap.Remove(item, item3);
				}
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct FindWalkConnectionsJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<int> SectionKeyArray;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> NeighborMap;

		[ReadOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> SectionMap;

		[WriteOnly]
		public NativeParallelHashSet<BidirectionalConnection>.ParallelWriter WalkConnections;

		public void Execute(int index)
		{
			int key = SectionKeyArray[index];
			foreach (int item in SectionMap.GetValuesForKey(key))
			{
				foreach (int item2 in NeighborMap.GetValuesForKey(item))
				{
					WalkConnections.Add(new BidirectionalConnection(WalkableOctantDataMap[item].SectionKey, WalkableOctantDataMap[item2].SectionKey));
				}
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct FindDropdownConnectionsJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeList<int> NewSectionsKeyArray;

		[ReadOnly]
		public NativeList<int> AllSectionKeys;

		[ReadOnly]
		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		[ReadOnly]
		public NativeHashSet<BidirectionalConnection> VerifiedJumpConnections;

		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		[WriteOnly]
		public NativeQueue<Connection>.ParallelWriter PotentialDropdownConnections;

		public void Execute(int index)
		{
			int num = NewSectionsKeyArray[index];
			SectionSurface jumpSurface = SurfaceMap[num];
			foreach (int allSectionKey in AllSectionKeys)
			{
				if (num == allSectionKey)
				{
					continue;
				}
				SectionSurface landSurface = SurfaceMap[allSectionKey];
				if (landSurface.Center.y - jumpSurface.Center.y >= 1f)
				{
					float3 x = jumpSurface.Center - landSurface.Center;
					x.y = 0f;
					if (math.length(x) > 24f)
					{
						continue;
					}
					SectionEdge closestEdgeToPoint = jumpSurface.GetClosestEdgeToPoint(landSurface.Center);
					SectionEdge closestEdgeToPoint2 = landSurface.GetClosestEdgeToPoint(jumpSurface.Center);
					float3 closestPointOnEdge = closestEdgeToPoint.GetClosestPointOnEdge(landSurface.Center);
					float3 closestPointOnEdge2 = closestEdgeToPoint2.GetClosestPointOnEdge(closestPointOnEdge);
					closestPointOnEdge = closestEdgeToPoint.GetClosestPointOnEdge(closestPointOnEdge2);
					if (!BurstPathfindingUtilities.CanReachJumpPosition(closestEdgeToPoint2.GetClosestPointOnEdge(closestPointOnEdge), in closestPointOnEdge, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity) || VerifiedJumpConnections.Contains(new BidirectionalConnection(num, allSectionKey)))
					{
						continue;
					}
					foreach (BurstPathfindingUtilities.SurfacePair sortedDropPair in BurstPathfindingUtilities.GetSortedDropPairs(jumpSurface, landSurface))
					{
						BurstPathfindingUtilities.SurfacePair current2 = sortedDropPair;
						if (!BurstPathfindingUtilities.CanReachJumpPosition(in current2.JumpPoint, in current2.LandPoint, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
						{
							return;
						}
						float launchAngleDegrees = BurstPathfindingUtilities.EstimateLaunchAngle(in current2.JumpPoint, in current2.LandPoint);
						if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in current2.JumpPoint, in current2.LandPoint, launchAngleDegrees, NpcMovementValues.Gravity, out var initialVelocity, out var timeOfFlight) && !float.IsNaN(timeOfFlight) && !(timeOfFlight <= 0f) && !BurstPathfindingUtilities.IsPathBlocked(current2.JumpPoint, current2.LandPoint, timeOfFlight, initialVelocity, Octants))
						{
							PotentialDropdownConnections.Enqueue(new Connection(allSectionKey, num));
							break;
						}
					}
				}
				else
				{
					if (!(jumpSurface.Center.y - landSurface.Center.y >= 1f))
					{
						continue;
					}
					float3 x2 = jumpSurface.Center - landSurface.Center;
					x2.y = 0f;
					if (math.length(x2) > 24f)
					{
						continue;
					}
					SectionEdge closestEdgeToPoint3 = jumpSurface.GetClosestEdgeToPoint(landSurface.Center);
					SectionEdge closestEdgeToPoint4 = landSurface.GetClosestEdgeToPoint(jumpSurface.Center);
					float3 closestPointOnEdge3 = closestEdgeToPoint3.GetClosestPointOnEdge(landSurface.Center);
					float3 closestPointOnEdge4 = closestEdgeToPoint4.GetClosestPointOnEdge(closestPointOnEdge3);
					closestPointOnEdge3 = closestEdgeToPoint3.GetClosestPointOnEdge(closestPointOnEdge4);
					if (!BurstPathfindingUtilities.CanReachJumpPosition(in closestPointOnEdge3, closestEdgeToPoint4.GetClosestPointOnEdge(closestPointOnEdge3), NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity) || VerifiedJumpConnections.Contains(new BidirectionalConnection(num, allSectionKey)))
					{
						continue;
					}
					foreach (BurstPathfindingUtilities.SurfacePair sortedDropPair2 in BurstPathfindingUtilities.GetSortedDropPairs(jumpSurface, landSurface))
					{
						BurstPathfindingUtilities.SurfacePair current3 = sortedDropPair2;
						if (!BurstPathfindingUtilities.CanReachJumpPosition(in current3.JumpPoint, in current3.LandPoint, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
						{
							return;
						}
						float launchAngleDegrees2 = BurstPathfindingUtilities.EstimateLaunchAngle(in current3.JumpPoint, in current3.LandPoint);
						if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in current3.JumpPoint, in current3.LandPoint, launchAngleDegrees2, NpcMovementValues.Gravity, out var initialVelocity2, out var timeOfFlight2) && !float.IsNaN(timeOfFlight2) && !(timeOfFlight2 <= 0f) && !BurstPathfindingUtilities.IsPathBlocked(current3.JumpPoint, current3.LandPoint, timeOfFlight2, initialVelocity2, Octants))
						{
							PotentialDropdownConnections.Enqueue(new Connection(num, allSectionKey));
							break;
						}
					}
				}
			}
		}
	}

	[BurstCompile]
	private struct ConvertConnectionQueueToHashset : IJob
	{
		public NativeQueue<Connection> Connections;

		[WriteOnly]
		public NativeHashSet<Connection> DropConnections;

		public void Execute()
		{
			while (Connections.Count > 0)
			{
				DropConnections.Add(Connections.Dequeue());
			}
		}
	}

	[BurstCompile]
	private struct AddUpdateDataToGraph : IJob
	{
		[ReadOnly]
		public NativeList<int> SectionKeys;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> SectionMap;

		[ReadOnly]
		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		[ReadOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantData;

		[ReadOnly]
		public NativeParallelHashSet<BidirectionalConnection> WalkConnections;

		[ReadOnly]
		public NativeHashSet<BidirectionalConnection> JumpConnections;

		[ReadOnly]
		public NativeHashSet<Connection> DropConnections;

		[WriteOnly]
		public NativeParallelHashMap<int, GraphNode> NodeGraph;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, Edge> EdgeGraph;

		public void Execute()
		{
			foreach (int sectionKey in SectionKeys)
			{
				SectionMap.TryGetFirstValue(sectionKey, out var item, out var _);
				WalkableOctantData walkableOctantData = WalkableOctantData[item];
				GraphNode item2 = new GraphNode
				{
					IslandKey = walkableOctantData.IslandKey,
					AreaKey = walkableOctantData.AreaKey,
					ZoneKey = walkableOctantData.ZoneKey,
					Key = sectionKey,
					Center = SurfaceMap[sectionKey].Center
				};
				NodeGraph.Add(sectionKey, item2);
			}
			foreach (BidirectionalConnection walkConnection in WalkConnections)
			{
				float cost = math.distance(SurfaceMap[walkConnection.SectionIndexA].Center, SurfaceMap[walkConnection.SectionIndexB].Center);
				EdgeGraph.Add(walkConnection.SectionIndexA, new Edge
				{
					Cost = cost,
					Connection = ConnectionKind.Walk,
					ConnectedNodeKey = walkConnection.SectionIndexB
				});
				EdgeGraph.Add(walkConnection.SectionIndexB, new Edge
				{
					Cost = cost,
					Connection = ConnectionKind.Walk,
					ConnectedNodeKey = walkConnection.SectionIndexA
				});
			}
			foreach (BidirectionalConnection jumpConnection in JumpConnections)
			{
				float cost2 = math.distance(SurfaceMap[jumpConnection.SectionIndexA].Center, SurfaceMap[jumpConnection.SectionIndexB].Center) * 2f;
				EdgeGraph.Add(jumpConnection.SectionIndexA, new Edge
				{
					Cost = cost2,
					Connection = ConnectionKind.Jump,
					ConnectedNodeKey = jumpConnection.SectionIndexB
				});
				EdgeGraph.Add(jumpConnection.SectionIndexB, new Edge
				{
					Cost = cost2,
					Connection = ConnectionKind.Jump,
					ConnectedNodeKey = jumpConnection.SectionIndexA
				});
			}
			foreach (Connection dropConnection in DropConnections)
			{
				float cost3 = math.distance(SurfaceMap[dropConnection.StartSectionKey].Center, SurfaceMap[dropConnection.EndSectionKey].Center) * 2.5f;
				EdgeGraph.Add(dropConnection.StartSectionKey, new Edge
				{
					Connection = ConnectionKind.Dropdown,
					ConnectedNodeKey = dropConnection.EndSectionKey,
					Cost = cost3
				});
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct ConstructAreaJobs : IJob
	{
		[ReadOnly]
		public NativeList<int> IslandKeys;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> SectionMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> IslandToSectionMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, Edge> EdgeGraph;

		public NativeParallelMultiHashMap<int, int> AreaMap;

		public NativeParallelHashMap<int, int> ReverseAreaMap;

		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		public NativeParallelMultiHashMap<int, int> AreaGraph;

		public void Execute()
		{
			int num = 0;
			NativeList<int> list = new NativeList<int>(64, AllocatorManager.Persistent);
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Persistent);
			NativeHashSet<int> nativeHashSet2 = new NativeHashSet<int>(64, AllocatorManager.Persistent);
			AreaMap.Clear();
			AreaGraph.Clear();
			for (int i = 0; i < IslandKeys.Length; i++)
			{
				int value = IslandKeys[i];
				list.Clear();
				nativeHashSet2.Clear();
				if (nativeHashSet.Contains(value))
				{
					continue;
				}
				nativeHashSet.Add(value);
				list.Add(in value);
				while (list.Length > 0)
				{
					int num2 = list[0];
					list.RemoveAt(0);
					nativeHashSet.Add(num2);
					nativeHashSet2.Add(num2);
					foreach (int item3 in IslandToSectionMap.GetValuesForKey(num2))
					{
						foreach (Edge item4 in EdgeGraph.GetValuesForKey(item3))
						{
							ConnectionKind connection = item4.Connection;
							if (connection != ConnectionKind.Dropdown && connection != ConnectionKind.Threshold)
							{
								int value2 = BurstPathfindingUtilities.GetIslandFromSectionKey(item4.ConnectedNodeKey);
								if (!list.Contains(value2) && !nativeHashSet.Contains(value2))
								{
									list.Add(in value2);
								}
							}
						}
					}
				}
				foreach (int item5 in nativeHashSet2)
				{
					AreaMap.Add(num, item5);
					ReverseAreaMap.Add(item5, num);
					foreach (int item6 in IslandToSectionMap.GetValuesForKey(item5))
					{
						foreach (int item7 in SectionMap.GetValuesForKey(item6))
						{
							WalkableOctantData value3 = WalkableOctantDataMap[item7];
							value3.AreaKey = num;
							WalkableOctantDataMap[item7] = value3;
						}
					}
				}
				num++;
			}
			for (int j = 0; j < num; j++)
			{
				NativeHashSet<int> nativeHashSet3 = new NativeHashSet<int>(num, AllocatorManager.Temp);
				foreach (int item8 in AreaMap.GetValuesForKey(j))
				{
					foreach (int item9 in IslandToSectionMap.GetValuesForKey(item8))
					{
						foreach (Edge item10 in EdgeGraph.GetValuesForKey(item9))
						{
							if (item10.Connection == ConnectionKind.Dropdown)
							{
								int islandFromSectionKey = BurstPathfindingUtilities.GetIslandFromSectionKey(item10.ConnectedNodeKey);
								int num3 = ReverseAreaMap[islandFromSectionKey];
								if (num3 != j && nativeHashSet3.Add(num3))
								{
									AreaGraph.Add(j, num3);
								}
							}
						}
					}
				}
			}
			NativeHashSet<int> nativeHashSet4 = new NativeHashSet<int>(num, AllocatorManager.Temp);
			NativeHashSet<int> areasToConsolidate = new NativeHashSet<int>(64, AllocatorManager.Temp);
			NativeList<int> result = new NativeList<int>(num, AllocatorManager.Temp);
			NativeQueue<int> queue = new NativeQueue<int>(AllocatorManager.Temp);
			NativeArray<int> distance = new NativeArray<int>(num, Allocator.Temp);
			NativeArray<int> path = new NativeArray<int>(num, Allocator.Temp);
			for (int k = 0; k < num; k++)
			{
				nativeHashSet4.Add(k);
			}
			while (true)
			{
				IL_0412:
				foreach (int item11 in areasToConsolidate)
				{
					nativeHashSet4.Remove(item11);
				}
				areasToConsolidate.Clear();
				foreach (int item12 in nativeHashSet4)
				{
					foreach (int item13 in AreaGraph.GetValuesForKey(item12))
					{
						BurstPathfindingUtilities.FindAreaPath(item12, item13, AreaGraph, result, queue, distance, path);
						foreach (int item14 in result)
						{
							areasToConsolidate.Add(item14);
						}
					}
					if (areasToConsolidate.Count > 0)
					{
						BurstPathfindingUtilities.ConsolidateAreas(item12, areasToConsolidate, AreaGraph, AreaMap, ReverseAreaMap, IslandToSectionMap, SectionMap, WalkableOctantDataMap);
						goto IL_0412;
					}
				}
				break;
			}
			foreach (KeyValue<int, Edge> item15 in EdgeGraph)
			{
				if (item15.Value.Connection == ConnectionKind.Threshold)
				{
					SectionMap.TryGetFirstValue(item15.Key, out var item, out var it);
					SectionMap.TryGetFirstValue(item15.Value.ConnectedNodeKey, out var item2, out it);
					int areaKey = WalkableOctantDataMap[item].AreaKey;
					int areaKey2 = WalkableOctantDataMap[item2].AreaKey;
					if (areaKey != areaKey2)
					{
						AreaGraph.Add(areaKey, areaKey2);
					}
				}
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct FindJumpConnectionsJob : IJobParallelFor
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		[ReadOnly]
		public NativeArray<int> SectionKeyArray;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> SectionMap;

		[ReadOnly]
		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		[ReadOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		[ReadOnly]
		public NativeParallelHashSet<BidirectionalConnection> WalkConnections;

		[WriteOnly]
		public NativeQueue<BidirectionalConnection>.ParallelWriter JumpConnections;

		public void Execute(int index)
		{
			int num = SectionKeyArray[index];
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Temp);
			UnsafeHashSet<int> unsafeHashSet = new UnsafeHashSet<int>(64, AllocatorManager.Temp);
			foreach (int item5 in SectionMap.GetValuesForKey(num))
			{
				NpcEnum.Edge edge = WalkableOctantDataMap[item5].Edge;
				if (Hint.Unlikely(!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(Octants.GetRef(item5).Center, Octants, 1f, out var index2)))
				{
					throw new Exception("This shouldn't be possible");
				}
				if (!nativeHashSet.Add(index2))
				{
					continue;
				}
				float3 center = Octants.GetRef(index2).Center;
				if ((edge & NpcEnum.Edge.North) != NpcEnum.Edge.None)
				{
					for (int i = 0; i < 275; i++)
					{
						float3 jumpUpdateDirectionalOffset = BurstPathfindingUtilities.GetJumpUpdateDirectionalOffset(i, math.forward());
						if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpUpdateDirectionalOffset, Octants, 1f, out var index3))
						{
							continue;
						}
						Octant octant = Octants[index3];
						if (!octant.IsWalkable && !octant.HasWalkableChildren)
						{
							continue;
						}
						WalkableOctantData item;
						if (octant.HasWalkableChildren)
						{
							for (int j = 0; j < 8; j++)
							{
								if (!octant.TryGetChildIndex(j, out var key))
								{
									continue;
								}
								ref Octant reference = ref Octants.GetRef(key);
								for (int k = 0; k < 8; k++)
								{
									if (reference.TryGetChildIndex(k, out var key2) && Octants.GetRef(key2).IsWalkable)
									{
										unsafeHashSet.Add(WalkableOctantDataMap[key2].SectionKey);
									}
								}
							}
						}
						else if (WalkableOctantDataMap.TryGetValue(index3, out item))
						{
							unsafeHashSet.Add(item.SectionKey);
						}
					}
				}
				if ((edge & NpcEnum.Edge.East) != NpcEnum.Edge.None)
				{
					for (int l = 0; l < 275; l++)
					{
						float3 jumpUpdateDirectionalOffset2 = BurstPathfindingUtilities.GetJumpUpdateDirectionalOffset(l, math.right());
						if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpUpdateDirectionalOffset2, Octants, 1f, out var index4))
						{
							continue;
						}
						Octant octant2 = Octants[index4];
						if (!octant2.IsWalkable && !octant2.HasWalkableChildren)
						{
							continue;
						}
						WalkableOctantData item2;
						if (octant2.HasWalkableChildren)
						{
							for (int m = 0; m < 8; m++)
							{
								if (!octant2.TryGetChildIndex(m, out var key3))
								{
									continue;
								}
								ref Octant reference2 = ref Octants.GetRef(key3);
								for (int n = 0; n < 8; n++)
								{
									if (reference2.TryGetChildIndex(n, out var key4) && Octants.GetRef(key4).IsWalkable)
									{
										unsafeHashSet.Add(WalkableOctantDataMap[key4].SectionKey);
									}
								}
							}
						}
						else if (WalkableOctantDataMap.TryGetValue(index4, out item2))
						{
							unsafeHashSet.Add(item2.SectionKey);
						}
					}
				}
				if ((edge & NpcEnum.Edge.South) != NpcEnum.Edge.None)
				{
					for (int num2 = 0; num2 < 275; num2++)
					{
						float3 jumpUpdateDirectionalOffset3 = BurstPathfindingUtilities.GetJumpUpdateDirectionalOffset(num2, math.back());
						if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpUpdateDirectionalOffset3, Octants, 1f, out var index5))
						{
							continue;
						}
						Octant octant3 = Octants[index5];
						if (!octant3.IsWalkable && !octant3.HasWalkableChildren)
						{
							continue;
						}
						WalkableOctantData item3;
						if (octant3.HasWalkableChildren)
						{
							for (int num3 = 0; num3 < 8; num3++)
							{
								if (!octant3.TryGetChildIndex(num3, out var key5))
								{
									continue;
								}
								ref Octant reference3 = ref Octants.GetRef(key5);
								for (int num4 = 0; num4 < 8; num4++)
								{
									if (reference3.TryGetChildIndex(num4, out var key6) && Octants.GetRef(key6).IsWalkable)
									{
										unsafeHashSet.Add(WalkableOctantDataMap[key6].SectionKey);
									}
								}
							}
						}
						else if (WalkableOctantDataMap.TryGetValue(index5, out item3))
						{
							unsafeHashSet.Add(item3.SectionKey);
						}
					}
				}
				if ((edge & NpcEnum.Edge.West) != NpcEnum.Edge.None)
				{
					for (int num5 = 0; num5 < 275; num5++)
					{
						float3 jumpUpdateDirectionalOffset4 = BurstPathfindingUtilities.GetJumpUpdateDirectionalOffset(num5, math.left());
						if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpUpdateDirectionalOffset4, Octants, 1f, out var index6))
						{
							continue;
						}
						Octant octant4 = Octants[index6];
						if (!octant4.IsWalkable && !octant4.HasWalkableChildren)
						{
							continue;
						}
						WalkableOctantData item4;
						if (octant4.HasWalkableChildren)
						{
							for (int num6 = 0; num6 < 8; num6++)
							{
								if (!octant4.TryGetChildIndex(num6, out var key7))
								{
									continue;
								}
								ref Octant reference4 = ref Octants.GetRef(key7);
								for (int num7 = 0; num7 < 8; num7++)
								{
									if (reference4.TryGetChildIndex(num7, out var key8) && Octants.GetRef(key8).IsWalkable)
									{
										unsafeHashSet.Add(WalkableOctantDataMap[key8].SectionKey);
									}
								}
							}
						}
						else if (WalkableOctantDataMap.TryGetValue(index6, out item4))
						{
							unsafeHashSet.Add(item4.SectionKey);
						}
					}
				}
				foreach (int item6 in unsafeHashSet)
				{
					if (num == item6)
					{
						continue;
					}
					BidirectionalConnection bidirectionalConnection = new BidirectionalConnection(num, item6);
					if (WalkConnections.Contains(bidirectionalConnection))
					{
						continue;
					}
					SectionSurface sectionSurface = SurfaceMap[num];
					SectionSurface sectionSurface2 = SurfaceMap[item6];
					if (sectionSurface.Center.y > sectionSurface2.Center.y)
					{
						SectionSurface sectionSurface3 = sectionSurface2;
						SectionSurface sectionSurface4 = sectionSurface;
						sectionSurface = sectionSurface3;
						sectionSurface2 = sectionSurface4;
					}
					foreach (BurstPathfindingUtilities.SurfacePair sortedJumpPair in BurstPathfindingUtilities.GetSortedJumpPairs(sectionSurface, sectionSurface2))
					{
						BurstPathfindingUtilities.SurfacePair current3 = sortedJumpPair;
						if (BurstPathfindingUtilities.CanReachJumpPosition(in current3.JumpPoint, in current3.LandPoint, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
						{
							float launchAngleDegrees = BurstPathfindingUtilities.EstimateLaunchAngle(in current3.JumpPoint, in current3.LandPoint);
							if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in current3.JumpPoint, in current3.LandPoint, launchAngleDegrees, NpcMovementValues.Gravity, out var initialVelocity, out var timeOfFlight) && !float.IsNaN(timeOfFlight) && !(timeOfFlight <= 0f) && !BurstPathfindingUtilities.IsPathBlocked(current3.JumpPoint, current3.LandPoint, timeOfFlight, initialVelocity, Octants))
							{
								JumpConnections.Enqueue(bidirectionalConnection);
								break;
							}
						}
					}
				}
				nativeHashSet.Clear();
				unsafeHashSet.Clear();
			}
		}
	}

	[BurstCompile]
	private struct BuildThresholdConnections : IJobParallelFor
	{
		private struct SectionPair
		{
			public int SectionA;

			public int SectionB;
		}

		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		[ReadOnly]
		public NativeParallelHashMap<SerializableGuid, UnsafeHashSet<int3>> thresholdMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> IslandToSectionMap;

		[ReadOnly]
		public NativeParallelHashMap<int, int> ReverseAreaMap;

		[ReadOnly]
		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		[ReadOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int>.ParallelWriter AreaGraph;

		[WriteOnly]
		public NativeQueue<BidirectionalConnection>.ParallelWriter ThresholdConnections;

		public void Execute(int index)
		{
			SerializableGuid key = thresholdMap.GetKeyArray(AllocatorManager.Temp)[index];
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Temp);
			foreach (int3 item in thresholdMap[key])
			{
				ref Octant smallestOctantForPoint = ref OctreeHelperMethods.GetSmallestOctantForPoint(item, Octants, 1f);
				if (!smallestOctantForPoint.HasWalkableChildren)
				{
					continue;
				}
				for (int i = 0; i < 8; i++)
				{
					smallestOctantForPoint.TryGetChildIndex(i, out var key2);
					ref Octant reference = ref Octants.GetRef(key2);
					for (int j = 0; j < 8; j++)
					{
						reference.TryGetChildIndex(j, out var key3);
						if (Octants.GetRef(key3).IsWalkable)
						{
							nativeHashSet.Add(WalkableOctantDataMap[key3].IslandKey);
						}
					}
				}
			}
			NativeHashSet<int> nativeHashSet2 = new NativeHashSet<int>(64, AllocatorManager.Temp);
			foreach (int item2 in nativeHashSet)
			{
				nativeHashSet2.Add(ReverseAreaMap[item2]);
			}
			foreach (int item3 in nativeHashSet)
			{
				foreach (int item4 in nativeHashSet)
				{
					if (item3 == item4)
					{
						continue;
					}
					SectionPair sectionPair = new SectionPair
					{
						SectionA = -1,
						SectionB = -1
					};
					float num = float.MaxValue;
					foreach (int item5 in IslandToSectionMap.GetValuesForKey(item3))
					{
						SectionSurface sectionSurface = SurfaceMap[item5];
						foreach (int item6 in IslandToSectionMap.GetValuesForKey(item4))
						{
							SectionSurface sectionSurface2 = SurfaceMap[item6];
							if (!(math.abs(sectionSurface.Center.y - sectionSurface2.Center.y) > 1f))
							{
								float num2 = math.distancesq(sectionSurface.Center, sectionSurface2.Center);
								if (num2 < num)
								{
									num = num2;
									sectionPair = new SectionPair
									{
										SectionA = item5,
										SectionB = item6
									};
								}
							}
						}
					}
					if (sectionPair.SectionA == -1)
					{
						continue;
					}
					int num3 = ReverseAreaMap[item3];
					int num4 = ReverseAreaMap[item4];
					if (nativeHashSet2.Count > 1)
					{
						if (num3 != num4)
						{
							ThresholdConnections.Enqueue(new BidirectionalConnection(sectionPair.SectionA, sectionPair.SectionB));
							AreaGraph.Add(num3, num4);
						}
					}
					else
					{
						ThresholdConnections.Enqueue(new BidirectionalConnection(sectionPair.SectionA, sectionPair.SectionB));
					}
				}
			}
		}
	}

	public static IEnumerator UpdateNavGraph(List<NavGraph.ChangedCell> updateCells, NativeArray<Octant> octants, NativeParallelHashMap<int, SlopeNeighbors> slopeMap, NativeParallelHashMap<int, SerializableGuid> associatedPropMap, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, NativeParallelHashMap<int, GraphNode> nodeGraph, NativeParallelMultiHashMap<int, Edge> edgeGraph, NativeParallelMultiHashMap<int, int> areaMap, NativeParallelHashMap<int, SectionSurface> surfaceMap, NativeParallelMultiHashMap<int, int> sectionMap, NativeParallelMultiHashMap<int, int> islandToSectionMap, NativeParallelMultiHashMap<int, int> areaGraph)
	{
		NavMeshObstacle[] obstacles = UnityEngine.Object.FindObjectsByType<NavMeshObstacle>(FindObjectsSortMode.None);
		NavMeshObstacle[] array = obstacles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].carving = false;
		}
		NativeArray<int3> cellsToUpdate = new NativeArray<int3>(updateCells.Count, Allocator.Persistent);
		for (int j = 0; j < updateCells.Count; j++)
		{
			cellsToUpdate[j] = updateCells[j].Cell;
		}
		NativeReference<int> largestKey = new NativeReference<int>(AllocatorManager.Persistent);
		NavMeshQuery query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent);
		NativeQueue<int> updatedOctantIndexes = new NativeQueue<int>(AllocatorManager.Persistent);
		NativeQueue<int> releasedIslandKeys = new NativeQueue<int>(AllocatorManager.Persistent);
		int numCells = cellsToUpdate.Length;
		GetLargestKey jobData = new GetLargestKey
		{
			MultiHashMap = islandToSectionMap,
			LargestKey = largestKey
		};
		ClearAssociatedCellDataJob jobData2 = new ClearAssociatedCellDataJob
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
		JobHandle dependsOn = jobData.Schedule();
		dependsOn = jobData2.Schedule(dependsOn);
		yield return JobUtilities.WaitForJobToComplete(dependsOn);
		int nextIslandKey = largestKey.Value + 1;
		NativeArray<int> octantsToUpdate = updatedOctantIndexes.ToArray(AllocatorManager.Persistent);
		updatedOctantIndexes.Dispose(default(JobHandle));
		NativeArray<OverlapBoxCommand> nativeArray = new NativeArray<OverlapBoxCommand>(octantsToUpdate.Length, Allocator.Persistent);
		NativeArray<OverlapBoxCommand> nativeArray2 = new NativeArray<OverlapBoxCommand>(octantsToUpdate.Length, Allocator.Persistent);
		NativeArray<ColliderHit> nativeArray3 = new NativeArray<ColliderHit>(octantsToUpdate.Length, Allocator.Persistent);
		NativeArray<ColliderHit> nativeArray4 = new NativeArray<ColliderHit>(octantsToUpdate.Length, Allocator.Persistent);
		ConstructBoxcastCommandsJob jobData3 = new ConstructBoxcastCommandsJob
		{
			OctantsToUpdate = octantsToUpdate,
			Octants = octants,
			BlockingOverlapBoxCommands = nativeArray,
			WalkableOverlapBoxCommands = nativeArray2,
			QueryParameters = new QueryParameters(NpcMovementValues.JumpSweepMask)
		};
		JobHandle jobHandle = new UpdateOctantData
		{
			OctantsToUpdate = octantsToUpdate,
			BlockingResults = nativeArray3,
			WalkableResults = nativeArray4,
			MinSplitCellAreaPercentage = NavGraph.MinSplitCellAreaPercentage,
			octantDivisions = NavGraph.SplitCellDivisions,
			Octants = octants,
			Query = query
		}.Schedule(dependsOn: OverlapBoxCommand.ScheduleBatch(dependsOn: OverlapBoxCommand.ScheduleBatch(dependsOn: jobData3.Schedule(), commands: nativeArray, results: nativeArray3, minCommandsPerJob: 30, maxHits: 1), commands: nativeArray2, results: nativeArray4, minCommandsPerJob: 30, maxHits: 1), innerloopBatchCount: octantsToUpdate.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches, arrayLength: octantsToUpdate.Length);
		nativeArray.Dispose(jobHandle);
		nativeArray3.Dispose(jobHandle);
		nativeArray2.Dispose(jobHandle);
		nativeArray4.Dispose(jobHandle);
		yield return JobUtilities.WaitForJobToComplete(jobHandle);
		NativeParallelMultiHashMap<int, int> rawNeighborMap = new NativeParallelMultiHashMap<int, int>(numCells * 64 * 16, AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> neighborMap = new NativeParallelMultiHashMap<int, int>(numCells * 64 * 16, AllocatorManager.Persistent);
		FindNeighborsJob jobData4 = new FindNeighborsJob
		{
			OctantsToUpdate = octantsToUpdate,
			NeighborMap = rawNeighborMap.AsParallelWriter(),
			Octants = octants,
			Query = query
		};
		CleanNeighborsJob jobData5 = new CleanNeighborsJob
		{
			RawNeighborMap = rawNeighborMap,
			NeighborMap = neighborMap
		};
		JobHandle dependsOn2 = IJobParallelForExtensions.Schedule(innerloopBatchCount: octantsToUpdate.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches, jobData: jobData4, arrayLength: octantsToUpdate.Length);
		dependsOn2 = jobData5.Schedule(dependsOn2);
		rawNeighborMap.Dispose(dependsOn2);
		cellsToUpdate.Dispose(dependsOn2);
		yield return JobUtilities.WaitForJobToComplete(dependsOn2, enforceTempCompletion: true);
		NativeReference<int> numOctantsOfLargestIslandReference = new NativeReference<int>(AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> newIslandMap = new NativeParallelMultiHashMap<int, int>(octantsToUpdate.Length, AllocatorManager.Persistent);
		JobHandle jobHandle2 = new BuildIslandsJob
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
		}.Schedule();
		releasedIslandKeys.Dispose(jobHandle2);
		yield return JobUtilities.WaitForJobToComplete(jobHandle2, enforceTempCompletion: true);
		int numOctantsOfLargestIsland = numOctantsOfLargestIslandReference.Value;
		numOctantsOfLargestIslandReference.Dispose(default(JobHandle));
		NativeList<int> newIslandKeys = new NativeList<int>(1000, AllocatorManager.Persistent);
		JobHandle handle = new GetUniqueKeysJob
		{
			RawMultiMap = newIslandMap,
			UniqueKeys = newIslandKeys
		}.Schedule();
		yield return JobUtilities.WaitForJobToComplete(handle);
		NativeParallelMultiHashMap<int, int> newSectionMap = new NativeParallelMultiHashMap<int, int>(octantsToUpdate.Length, AllocatorManager.Persistent);
		int num = islandToSectionMap.Capacity - islandToSectionMap.Count();
		int num2 = newIslandKeys.Length * numOctantsOfLargestIsland;
		if (num2 > num)
		{
			islandToSectionMap.Capacity = math.ceilpow2(islandToSectionMap.Capacity + num2);
		}
		num = sectionMap.Capacity - sectionMap.Count();
		if (num2 > num)
		{
			sectionMap.Capacity = math.ceilpow2(sectionMap.Capacity + num2);
		}
		JobHandle handle2 = new BuildSectionsJob
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
		}.Schedule(innerloopBatchCount: newIslandKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches, arrayLength: newIslandKeys.Length);
		yield return JobUtilities.WaitForJobToComplete(handle2);
		newIslandMap.Dispose(default(JobHandle));
		newIslandKeys.Dispose(default(JobHandle));
		NativeList<int> sectionKeys = new NativeList<int>(1024, AllocatorManager.Persistent);
		NativeList<int> allSectionKeys = new NativeList<int>(1024, AllocatorManager.Persistent);
		GetUniqueKeysJob jobData6 = new GetUniqueKeysJob
		{
			RawMultiMap = newSectionMap,
			UniqueKeys = sectionKeys
		};
		GetUniqueKeysJob jobData7 = new GetUniqueKeysJob
		{
			RawMultiMap = sectionMap,
			UniqueKeys = allSectionKeys
		};
		JobHandle dependsOn3 = jobData6.Schedule();
		dependsOn3 = jobData7.Schedule(dependsOn3);
		newSectionMap.Dispose(dependsOn3);
		yield return JobUtilities.WaitForJobToComplete(dependsOn3);
		NativeQueue<BidirectionalConnection> connectionsQueue = new NativeQueue<BidirectionalConnection>(AllocatorManager.Persistent);
		NativeQueue<Connection> dropdownConnections = new NativeQueue<Connection>(AllocatorManager.Persistent);
		NativeHashSet<Connection> dropConnections = new NativeHashSet<Connection>(1024, AllocatorManager.Persistent);
		NativeParallelHashSet<BidirectionalConnection> walkConnections = new NativeParallelHashSet<BidirectionalConnection>(octantsToUpdate.Length * 5, AllocatorManager.Persistent);
		NativeHashSet<BidirectionalConnection> jumpConnections = new NativeHashSet<BidirectionalConnection>(octantsToUpdate.Length, AllocatorManager.Persistent);
		num = surfaceMap.Capacity - surfaceMap.Count();
		if (num < sectionKeys.Length)
		{
			surfaceMap.Capacity = math.ceilpow2(sectionKeys.Length + surfaceMap.Count());
		}
		FindEdgeOctantsJob jobData8 = new FindEdgeOctantsJob
		{
			SectionKeys = sectionKeys.AsDeferredJobArray(),
			SectionMap = sectionMap,
			NeighborMap = neighborMap,
			Octants = octants,
			WalkableOctantDataMap = walkableOctantDataMap
		};
		PruneNeighborsJob jobData9 = new PruneNeighborsJob
		{
			OctantsToUpdate = octantsToUpdate,
			NeighborMap = neighborMap,
			WalkableOctantDataMap = walkableOctantDataMap
		};
		BuildSurfacesJob jobData10 = new BuildSurfacesJob
		{
			SectionKeyArray = sectionKeys.AsArray(),
			SectionMap = sectionMap,
			Octants = octants,
			Query = query,
			SurfaceMap = surfaceMap.AsParallelWriter()
		};
		FindWalkConnectionsJob jobData11 = new FindWalkConnectionsJob
		{
			SectionKeyArray = sectionKeys.AsArray(),
			NeighborMap = neighborMap,
			WalkableOctantDataMap = walkableOctantDataMap,
			SectionMap = sectionMap,
			WalkConnections = walkConnections.AsParallelWriter()
		};
		FindJumpConnectionsJob jobData12 = new FindJumpConnectionsJob
		{
			SectionKeyArray = sectionKeys.AsArray(),
			Octants = octants,
			SectionMap = sectionMap,
			SurfaceMap = surfaceMap,
			WalkableOctantDataMap = walkableOctantDataMap,
			WalkConnections = walkConnections,
			JumpConnections = connectionsQueue.AsParallelWriter()
		};
		ConvertQueueToHashset jobData13 = new ConvertQueueToHashset
		{
			ConnectionsQueue = connectionsQueue,
			ConnectionsHashSet = jumpConnections
		};
		FindDropdownConnectionsJob jobData14 = new FindDropdownConnectionsJob
		{
			NewSectionsKeyArray = sectionKeys,
			Octants = octants,
			AllSectionKeys = allSectionKeys,
			SurfaceMap = surfaceMap,
			VerifiedJumpConnections = jumpConnections,
			PotentialDropdownConnections = dropdownConnections.AsParallelWriter()
		};
		ConvertConnectionQueueToHashset jobData15 = new ConvertConnectionQueueToHashset
		{
			Connections = dropdownConnections,
			DropConnections = dropConnections
		};
		int innerloopBatchCount = sectionKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
		JobHandle dependsOn4 = IJobParallelForExtensions.Schedule(jobData8, sectionKeys.Length, innerloopBatchCount);
		dependsOn4 = IJobParallelForExtensions.Schedule(dependsOn: IJobParallelForExtensions.Schedule(dependsOn: IJobParallelForExtensions.Schedule(dependsOn: jobData9.Schedule(dependsOn4), jobData: jobData10, arrayLength: sectionKeys.Length, innerloopBatchCount: innerloopBatchCount), jobData: jobData11, arrayLength: sectionKeys.Length, innerloopBatchCount: innerloopBatchCount), jobData: jobData12, arrayLength: sectionKeys.Length, innerloopBatchCount: innerloopBatchCount);
		dependsOn4 = IJobParallelForExtensions.Schedule(dependsOn: jobData13.Schedule(dependsOn4), jobData: jobData14, arrayLength: sectionKeys.Length, innerloopBatchCount: innerloopBatchCount);
		dependsOn4 = jobData15.Schedule(dependsOn4);
		connectionsQueue.Dispose(dependsOn4);
		octantsToUpdate.Dispose(dependsOn4);
		neighborMap.Dispose(dependsOn4);
		allSectionKeys.Dispose(dependsOn4);
		yield return JobUtilities.WaitForJobToComplete(dependsOn4);
		NativeArray<Connection> potentialDropConnections = dropdownConnections.ToArray(AllocatorManager.Persistent);
		dropdownConnections.Dispose(default(JobHandle));
		NativeQueue<Connection> verifiedDropConnectionQueue = new NativeQueue<Connection>(AllocatorManager.Persistent);
		NativeList<int> uniqueKeys = new NativeList<int>(128, AllocatorManager.Persistent);
		JobHandle handle3 = new GetUniqueKeysJob
		{
			RawMultiMap = islandToSectionMap,
			UniqueKeys = uniqueKeys
		}.Schedule();
		yield return JobUtilities.WaitForJobToComplete(handle3);
		NativeParallelHashMap<int, int> reverseAreaMap = new NativeParallelHashMap<int, int>(uniqueKeys.Length, AllocatorManager.Persistent);
		int num3 = math.ceilpow2(uniqueKeys.Length * uniqueKeys.Length);
		if (areaGraph.Capacity < num3)
		{
			areaGraph.Capacity = num3;
		}
		AddUpdateDataToGraph jobData16 = new AddUpdateDataToGraph
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
		CleanJumpConnections jobData17 = new CleanJumpConnections
		{
			EdgeMap = edgeGraph,
			NodeMap = nodeGraph
		};
		ConstructAreaJobs jobData18 = new ConstructAreaJobs
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
		JobHandle jobHandle3 = jobData16.Schedule();
		potentialDropConnections.Dispose(jobHandle3);
		verifiedDropConnectionQueue.Dispose(jobHandle3);
		walkConnections.Dispose(jobHandle3);
		jumpConnections.Dispose(jobHandle3);
		dropConnections.Dispose(jobHandle3);
		jobHandle3 = jobData17.Schedule(jobHandle3);
		jobHandle3 = jobData18.Schedule(jobHandle3);
		uniqueKeys.Dispose(jobHandle3);
		sectionKeys.Dispose(jobHandle3);
		yield return JobUtilities.WaitForJobToComplete(jobHandle3);
		NativeParallelHashMap<SerializableGuid, UnsafeHashSet<int3>> thresholdMap = new NativeParallelHashMap<SerializableGuid, UnsafeHashSet<int3>>(updateCells.Count, AllocatorManager.Persistent);
		NativeQueue<BidirectionalConnection> thresholdConnections = new NativeQueue<BidirectionalConnection>(AllocatorManager.Persistent);
		foreach (NavGraph.ChangedCell updateCell in updateCells)
		{
			if (!updateCell.IsBlocking)
			{
				continue;
			}
			SerializableGuid instanceId = updateCell.WorldObject.InstanceId;
			if (updateCell.WorldObject.TryGetUserComponent(typeof(Door), out var component) && ((Door)component).CurrentNpcDoorInteraction != Door.NpcDoorInteraction.NotOpenable)
			{
				if (thresholdMap.TryGetValue(instanceId, out var item))
				{
					item.Add(updateCell.Cell);
					thresholdMap[instanceId] = item;
				}
				else
				{
					UnsafeHashSet<int3> item2 = new UnsafeHashSet<int3>(64, AllocatorManager.Persistent);
					item2.Add(updateCell.Cell);
					thresholdMap.Add(instanceId, item2);
				}
			}
		}
		int num4 = 0;
		foreach (KeyValue<SerializableGuid, UnsafeHashSet<int3>> item3 in thresholdMap)
		{
			if (item3.Value.Count > num4)
			{
				num4 = item3.Value.Count;
			}
		}
		BuildThresholdConnections jobData19 = new BuildThresholdConnections
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
		innerloopBatchCount = length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
		JobHandle handle4 = IJobParallelForExtensions.Schedule(jobData19, length, innerloopBatchCount);
		yield return JobUtilities.WaitForJobToComplete(handle4);
		foreach (KeyValue<SerializableGuid, UnsafeHashSet<int3>> item4 in thresholdMap)
		{
			item4.Value.Dispose(default(JobHandle));
		}
		thresholdMap.Dispose(default(JobHandle));
		foreach (BidirectionalConnection item5 in thresholdConnections.ToArray(AllocatorManager.Temp))
		{
			float cost = math.distance(surfaceMap[item5.SectionIndexA].Center, surfaceMap[item5.SectionIndexB].Center);
			edgeGraph.Add(item5.SectionIndexA, new Edge
			{
				Connection = ConnectionKind.Threshold,
				ConnectedNodeKey = item5.SectionIndexB,
				Cost = cost
			});
			edgeGraph.Add(item5.SectionIndexB, new Edge
			{
				Connection = ConnectionKind.Threshold,
				ConnectedNodeKey = item5.SectionIndexA,
				Cost = cost
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
	}
}
