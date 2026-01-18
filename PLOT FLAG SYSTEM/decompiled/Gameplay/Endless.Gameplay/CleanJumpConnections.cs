using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Endless.Gameplay;

[BurstCompile]
public struct CleanJumpConnections : IJob
{
	[ReadOnly]
	public NativeParallelHashMap<int, GraphNode> NodeMap;

	public NativeParallelMultiHashMap<int, Edge> EdgeMap;

	public void Execute()
	{
		NativeHashSet<Edge> nativeHashSet = new NativeHashSet<Edge>(64, AllocatorManager.Temp);
		NativeHashSet<Edge> nativeHashSet2 = new NativeHashSet<Edge>(64, AllocatorManager.Temp);
		NativeHashSet<Edge> nativeHashSet3 = new NativeHashSet<Edge>(64, AllocatorManager.Temp);
		foreach (KeyValue<int, GraphNode> item in NodeMap)
		{
			foreach (Edge item2 in EdgeMap.GetValuesForKey(item.Key))
			{
				switch (item2.Connection)
				{
				case ConnectionKind.Walk:
					nativeHashSet.Add(item2);
					break;
				case ConnectionKind.Jump:
					nativeHashSet2.Add(item2);
					break;
				}
			}
			foreach (Edge item3 in nativeHashSet2)
			{
				bool flag = false;
				foreach (Edge item4 in nativeHashSet)
				{
					if (item4.ConnectedNodeKey == item3.ConnectedNodeKey)
					{
						nativeHashSet3.Add(item3);
						flag = true;
						continue;
					}
					foreach (Edge item5 in EdgeMap.GetValuesForKey(item4.ConnectedNodeKey))
					{
						if (item5.Connection == ConnectionKind.Walk && item5.ConnectedNodeKey == item3.ConnectedNodeKey)
						{
							nativeHashSet3.Add(item3);
							flag = true;
						}
					}
					if (!flag)
					{
						continue;
					}
					break;
				}
			}
			foreach (Edge item6 in nativeHashSet3)
			{
				EdgeMap.Remove(item.Key, item6);
			}
			nativeHashSet.Clear();
			nativeHashSet2.Clear();
			nativeHashSet3.Clear();
		}
	}
}
