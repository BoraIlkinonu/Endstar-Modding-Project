using System;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay;

public static class NavShortcutMapGenerator
{
	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct ConvertConnectionsToMapsJob : IJob
	{
		[ReadOnly]
		public NativeParallelHashSet<BidirectionalConnection> walkConnections;

		[ReadOnly]
		public NativeParallelHashSet<BidirectionalConnection> verifiedJumpConnections;

		[ReadOnly]
		public NativeParallelHashSet<Connection> verifiedDropdownConnections;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int> walkConnectionMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int> jumpConnectionMap;

		[WriteOnly]
		public NativeParallelMultiHashMap<int, int> dropConnectionMap;

		public void Execute()
		{
			foreach (BidirectionalConnection walkConnection in walkConnections)
			{
				walkConnectionMap.Add(walkConnection.SectionIndexA, walkConnection.SectionIndexB);
				walkConnectionMap.Add(walkConnection.SectionIndexB, walkConnection.SectionIndexA);
			}
			foreach (BidirectionalConnection verifiedJumpConnection in verifiedJumpConnections)
			{
				jumpConnectionMap.Add(verifiedJumpConnection.SectionIndexA, verifiedJumpConnection.SectionIndexB);
				jumpConnectionMap.Add(verifiedJumpConnection.SectionIndexB, verifiedJumpConnection.SectionIndexA);
			}
			foreach (Connection verifiedDropdownConnection in verifiedDropdownConnections)
			{
				dropConnectionMap.Add(verifiedDropdownConnection.StartSectionKey, verifiedDropdownConnection.EndSectionKey);
			}
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct ConstructAreaJobs : IJob
	{
		[ReadOnly]
		public NativeArray<int> IslandKeyArray;

		[ReadOnly]
		public int NumUniqueKeys;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> SectionMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> WalkConnectionMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> JumpConnectionMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> DropConnectionMap;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> IslandToSectionMap;

		public NativeParallelMultiHashMap<int, int> AreaMap;

		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		public NativeParallelMultiHashMap<int, int> AreaGraph;

		public void Execute()
		{
			int num = 0;
			NativeList<int> list = new NativeList<int>(64, AllocatorManager.Persistent);
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Persistent);
			NativeHashSet<int> nativeHashSet2 = new NativeHashSet<int>(64, AllocatorManager.Persistent);
			NativeParallelHashMap<int, int> reverseAreaMap = new NativeParallelHashMap<int, int>(64, AllocatorManager.Temp);
			for (int i = 0; i < NumUniqueKeys; i++)
			{
				int value = IslandKeyArray[i];
				list.Clear();
				nativeHashSet2.Clear();
				if (nativeHashSet.Contains(value))
				{
					continue;
				}
				list.Add(in value);
				while (list.Length > 0)
				{
					int num2 = list[0];
					list.RemoveAtSwapBack(0);
					nativeHashSet.Add(num2);
					nativeHashSet2.Add(num2);
					foreach (int item in IslandToSectionMap.GetValuesForKey(num2))
					{
						foreach (int item2 in JumpConnectionMap.GetValuesForKey(item))
						{
							int value2 = BurstPathfindingUtilities.GetIslandFromSectionKey(item2);
							if (!list.Contains(value2) && !nativeHashSet.Contains(value2))
							{
								list.Add(in value2);
							}
						}
						foreach (int item3 in WalkConnectionMap.GetValuesForKey(item))
						{
							int value3 = BurstPathfindingUtilities.GetIslandFromSectionKey(item3);
							if (!list.Contains(value3) && !nativeHashSet.Contains(value3))
							{
								list.Add(in value3);
							}
						}
					}
				}
				foreach (int item4 in nativeHashSet2)
				{
					AreaMap.Add(num, item4);
					reverseAreaMap.Add(item4, num);
					foreach (int item5 in IslandToSectionMap.GetValuesForKey(item4))
					{
						foreach (int item6 in SectionMap.GetValuesForKey(item5))
						{
							WalkableOctantData value4 = WalkableOctantDataMap[item6];
							value4.AreaKey = num;
							WalkableOctantDataMap[item6] = value4;
						}
					}
				}
				num++;
			}
			for (int j = 0; j < num; j++)
			{
				NativeHashSet<int> nativeHashSet3 = new NativeHashSet<int>(num, AllocatorManager.Temp);
				foreach (int item7 in AreaMap.GetValuesForKey(j))
				{
					foreach (int item8 in IslandToSectionMap.GetValuesForKey(item7))
					{
						foreach (int item9 in DropConnectionMap.GetValuesForKey(item8))
						{
							int islandFromSectionKey = BurstPathfindingUtilities.GetIslandFromSectionKey(item9);
							int num3 = reverseAreaMap[islandFromSectionKey];
							if (num3 != j && nativeHashSet3.Add(num3))
							{
								AreaGraph.Add(j, num3);
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
				foreach (int item10 in areasToConsolidate)
				{
					nativeHashSet4.Remove(item10);
				}
				areasToConsolidate.Clear();
				using NativeHashSet<int>.Enumerator enumerator3 = nativeHashSet4.GetEnumerator();
				int current8;
				do
				{
					if (!enumerator3.MoveNext())
					{
						return;
					}
					current8 = enumerator3.Current;
					foreach (int item11 in AreaGraph.GetValuesForKey(current8))
					{
						BurstPathfindingUtilities.FindAreaPath(current8, item11, AreaGraph, result, queue, distance, path);
						foreach (int item12 in result)
						{
							areasToConsolidate.Add(item12);
						}
					}
				}
				while (areasToConsolidate.Count <= 0);
				BurstPathfindingUtilities.ConsolidateAreas(current8, areasToConsolidate, AreaGraph, AreaMap, reverseAreaMap, IslandToSectionMap, SectionMap, WalkableOctantDataMap);
			}
		}
	}

	public struct Results
	{
		public NativeParallelMultiHashMap<int, int> AreaMap;

		public NativeParallelMultiHashMap<int, int> AreaGraph;
	}

	public static IEnumerator GenerateShortcutMaps(NativeParallelHashSet<BidirectionalConnection> walkConnections, NativeParallelHashSet<BidirectionalConnection> jumpConnections, NativeParallelHashSet<Connection> dropConnections, NativeList<int> islandKeys, NativeParallelMultiHashMap<int, int> sectionMap, NativeParallelMultiHashMap<int, int> islandKeyToSectionKeyMap, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, NativeList<int> sectionKeys, Action<Results> getResults)
	{
		NativeParallelMultiHashMap<int, int> walkConnectionMap = new NativeParallelMultiHashMap<int, int>(walkConnections.Count() * 2, AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> jumpConnectionMap = new NativeParallelMultiHashMap<int, int>(jumpConnections.Count() * 2, AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> dropConnectionMap = new NativeParallelMultiHashMap<int, int>(dropConnections.Count(), AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> areaMap = new NativeParallelMultiHashMap<int, int>(islandKeys.Length, AllocatorManager.Persistent);
		NativeParallelMultiHashMap<int, int> areaGraph = new NativeParallelMultiHashMap<int, int>(islandKeys.Length * islandKeys.Length, AllocatorManager.Persistent);
		ConvertConnectionsToMapsJob jobData = new ConvertConnectionsToMapsJob
		{
			walkConnections = walkConnections,
			verifiedJumpConnections = jumpConnections,
			verifiedDropdownConnections = dropConnections,
			walkConnectionMap = walkConnectionMap,
			jumpConnectionMap = jumpConnectionMap,
			dropConnectionMap = dropConnectionMap
		};
		ConstructAreaJobs jobData2 = new ConstructAreaJobs
		{
			IslandKeyArray = islandKeys.AsArray(),
			NumUniqueKeys = islandKeys.Length,
			SectionMap = sectionMap,
			WalkConnectionMap = walkConnectionMap,
			JumpConnectionMap = jumpConnectionMap,
			DropConnectionMap = dropConnectionMap,
			IslandToSectionMap = islandKeyToSectionKeyMap,
			AreaMap = areaMap,
			WalkableOctantDataMap = walkableOctantDataMap,
			AreaGraph = areaGraph
		};
		JobHandle dependsOn = jobData.Schedule();
		dependsOn = jobData2.Schedule(dependsOn);
		dependsOn = islandKeys.Dispose(dependsOn);
		dependsOn = sectionKeys.Dispose(dependsOn);
		dependsOn = walkConnectionMap.Dispose(dependsOn);
		dependsOn = jumpConnectionMap.Dispose(dependsOn);
		dependsOn = dropConnectionMap.Dispose(dependsOn);
		yield return JobUtilities.WaitForJobToComplete(dependsOn);
		getResults(new Results
		{
			AreaMap = areaMap,
			AreaGraph = areaGraph
		});
	}
}
