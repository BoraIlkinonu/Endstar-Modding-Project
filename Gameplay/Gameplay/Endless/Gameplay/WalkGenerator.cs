using System;
using System.Collections;
using Endless.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay
{
	// Token: 0x020001EA RID: 490
	public static class WalkGenerator
	{
		// Token: 0x06000A2B RID: 2603 RVA: 0x00036AB3 File Offset: 0x00034CB3
		public static IEnumerator GenerateWalkConnections(NativeList<int> sectionKeys, NativeParallelMultiHashMap<int, int> neighborMap, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, NativeParallelMultiHashMap<int, int> sectionMap, Action<WalkGenerator.Results> getResults)
		{
			int num = sectionKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			NativeQueue<BidirectionalConnection> nativeQueue = new NativeQueue<BidirectionalConnection>(AllocatorManager.Persistent);
			NativeParallelHashSet<BidirectionalConnection> walkConnections = new NativeParallelHashSet<BidirectionalConnection>(1024, AllocatorManager.Persistent);
			WalkGenerator.FindWalkConnectionsJob findWalkConnectionsJob = new WalkGenerator.FindWalkConnectionsJob
			{
				SectionKeyArray = sectionKeys.AsArray(),
				NeighborMap = neighborMap,
				WalkableOctantDataMap = walkableOctantDataMap,
				SectionMap = sectionMap,
				WalkConnections = nativeQueue.AsParallelWriter()
			};
			ConvertQueueToParallelHashset convertQueueToParallelHashset = new ConvertQueueToParallelHashset
			{
				ConnectionsQueue = nativeQueue,
				ConnectionsHashSet = walkConnections
			};
			JobHandle jobHandle = findWalkConnectionsJob.Schedule(sectionKeys.Length, num, default(JobHandle));
			jobHandle = convertQueueToParallelHashset.Schedule(jobHandle);
			jobHandle = nativeQueue.Dispose(jobHandle);
			yield return JobUtilities.WaitForJobToComplete(jobHandle, false);
			getResults(new WalkGenerator.Results
			{
				WalkConnections = walkConnections
			});
			yield break;
		}

		// Token: 0x020001EB RID: 491
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct FindWalkConnectionsJob : IJobParallelFor
		{
			// Token: 0x06000A2C RID: 2604 RVA: 0x00036AE0 File Offset: 0x00034CE0
			public void Execute(int index)
			{
				int num = this.SectionKeyArray[index];
				NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.SectionMap.GetValuesForKey(num);
				foreach (int num2 in enumerator)
				{
					foreach (int num3 in this.NeighborMap.GetValuesForKey(num2))
					{
						this.WalkConnections.Enqueue(new BidirectionalConnection(this.WalkableOctantDataMap[num2].SectionKey, this.WalkableOctantDataMap[num3].SectionKey));
					}
				}
			}

			// Token: 0x04000961 RID: 2401
			[ReadOnly]
			public NativeArray<int> SectionKeyArray;

			// Token: 0x04000962 RID: 2402
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> NeighborMap;

			// Token: 0x04000963 RID: 2403
			[ReadOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x04000964 RID: 2404
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x04000965 RID: 2405
			[WriteOnly]
			public NativeQueue<BidirectionalConnection>.ParallelWriter WalkConnections;
		}

		// Token: 0x020001EC RID: 492
		public struct Results
		{
			// Token: 0x04000966 RID: 2406
			public NativeParallelHashSet<BidirectionalConnection> WalkConnections;
		}
	}
}
