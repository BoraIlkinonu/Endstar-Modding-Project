using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Endless.Gameplay
{
	// Token: 0x020001EF RID: 495
	[BurstCompile]
	public struct CleanJumpConnections : IJob
	{
		// Token: 0x06000A35 RID: 2613 RVA: 0x00037048 File Offset: 0x00035248
		public void Execute()
		{
			NativeHashSet<Edge> nativeHashSet = new NativeHashSet<Edge>(64, AllocatorManager.Temp);
			NativeHashSet<Edge> nativeHashSet2 = new NativeHashSet<Edge>(64, AllocatorManager.Temp);
			NativeHashSet<Edge> nativeHashSet3 = new NativeHashSet<Edge>(64, AllocatorManager.Temp);
			foreach (KeyValue<int, GraphNode> keyValue in this.NodeMap)
			{
				foreach (Edge edge in this.EdgeMap.GetValuesForKey(keyValue.Key))
				{
					ConnectionKind connection = edge.Connection;
					if (connection != ConnectionKind.Walk)
					{
						if (connection == ConnectionKind.Jump)
						{
							nativeHashSet2.Add(edge);
						}
					}
					else
					{
						nativeHashSet.Add(edge);
					}
				}
				foreach (Edge edge2 in nativeHashSet2)
				{
					bool flag = false;
					foreach (Edge edge3 in nativeHashSet)
					{
						if (edge3.ConnectedNodeKey == edge2.ConnectedNodeKey)
						{
							nativeHashSet3.Add(edge2);
							flag = true;
						}
						else
						{
							foreach (Edge edge4 in this.EdgeMap.GetValuesForKey(edge3.ConnectedNodeKey))
							{
								if (edge4.Connection == ConnectionKind.Walk && edge4.ConnectedNodeKey == edge2.ConnectedNodeKey)
								{
									nativeHashSet3.Add(edge2);
									flag = true;
								}
							}
							if (flag)
							{
								break;
							}
						}
					}
				}
				foreach (Edge edge5 in nativeHashSet3)
				{
					this.EdgeMap.Remove(keyValue.Key, edge5);
				}
				nativeHashSet.Clear();
				nativeHashSet2.Clear();
				nativeHashSet3.Clear();
			}
		}

		// Token: 0x04000974 RID: 2420
		[ReadOnly]
		public NativeParallelHashMap<int, GraphNode> NodeMap;

		// Token: 0x04000975 RID: 2421
		public NativeParallelMultiHashMap<int, Edge> EdgeMap;
	}
}
