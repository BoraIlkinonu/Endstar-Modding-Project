using System;
using System.Collections;
using Endless.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay;

public static class WalkGenerator
{
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
		public NativeQueue<BidirectionalConnection>.ParallelWriter WalkConnections;

		public void Execute(int index)
		{
			int key = SectionKeyArray[index];
			foreach (int item in SectionMap.GetValuesForKey(key))
			{
				foreach (int item2 in NeighborMap.GetValuesForKey(item))
				{
					WalkConnections.Enqueue(new BidirectionalConnection(WalkableOctantDataMap[item].SectionKey, WalkableOctantDataMap[item2].SectionKey));
				}
			}
		}
	}

	public struct Results
	{
		public NativeParallelHashSet<BidirectionalConnection> WalkConnections;
	}

	public static IEnumerator GenerateWalkConnections(NativeList<int> sectionKeys, NativeParallelMultiHashMap<int, int> neighborMap, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, NativeParallelMultiHashMap<int, int> sectionMap, Action<Results> getResults)
	{
		int innerloopBatchCount = sectionKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
		NativeQueue<BidirectionalConnection> connectionsQueue = new NativeQueue<BidirectionalConnection>(AllocatorManager.Persistent);
		NativeParallelHashSet<BidirectionalConnection> walkConnections = new NativeParallelHashSet<BidirectionalConnection>(1024, AllocatorManager.Persistent);
		FindWalkConnectionsJob jobData = new FindWalkConnectionsJob
		{
			SectionKeyArray = sectionKeys.AsArray(),
			NeighborMap = neighborMap,
			WalkableOctantDataMap = walkableOctantDataMap,
			SectionMap = sectionMap,
			WalkConnections = connectionsQueue.AsParallelWriter()
		};
		ConvertQueueToParallelHashset jobData2 = new ConvertQueueToParallelHashset
		{
			ConnectionsQueue = connectionsQueue,
			ConnectionsHashSet = walkConnections
		};
		JobHandle dependsOn = IJobParallelForExtensions.Schedule(jobData, sectionKeys.Length, innerloopBatchCount);
		dependsOn = jobData2.Schedule(dependsOn);
		dependsOn = connectionsQueue.Dispose(dependsOn);
		yield return JobUtilities.WaitForJobToComplete(dependsOn);
		getResults(new Results
		{
			WalkConnections = walkConnections
		});
	}
}
