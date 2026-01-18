using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay
{
	// Token: 0x020001DC RID: 476
	[BurstCompile]
	public struct ConvertQueueToParallelHashset : IJob
	{
		// Token: 0x06000A02 RID: 2562 RVA: 0x00033BE1 File Offset: 0x00031DE1
		public void Execute()
		{
			while (this.ConnectionsQueue.Count > 0)
			{
				this.ConnectionsHashSet.Add(this.ConnectionsQueue.Dequeue());
			}
		}

		// Token: 0x040008E2 RID: 2274
		public NativeQueue<BidirectionalConnection> ConnectionsQueue;

		// Token: 0x040008E3 RID: 2275
		[WriteOnly]
		public NativeParallelHashSet<BidirectionalConnection> ConnectionsHashSet;
	}
}
