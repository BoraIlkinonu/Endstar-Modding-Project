using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay;

[BurstCompile]
public struct ConvertQueueToParallelHashset : IJob
{
	public NativeQueue<BidirectionalConnection> ConnectionsQueue;

	[WriteOnly]
	public NativeParallelHashSet<BidirectionalConnection> ConnectionsHashSet;

	public void Execute()
	{
		while (ConnectionsQueue.Count > 0)
		{
			ConnectionsHashSet.Add(ConnectionsQueue.Dequeue());
		}
	}
}
