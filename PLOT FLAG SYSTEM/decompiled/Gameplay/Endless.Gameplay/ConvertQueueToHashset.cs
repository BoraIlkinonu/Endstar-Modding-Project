using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay;

[BurstCompile]
public struct ConvertQueueToHashset : IJob
{
	public NativeQueue<BidirectionalConnection> ConnectionsQueue;

	[WriteOnly]
	public NativeHashSet<BidirectionalConnection> ConnectionsHashSet;

	public void Execute()
	{
		while (ConnectionsQueue.Count > 0)
		{
			ConnectionsHashSet.Add(ConnectionsQueue.Dequeue());
		}
	}
}
