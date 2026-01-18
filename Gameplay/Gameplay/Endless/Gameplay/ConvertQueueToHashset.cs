using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay
{
	// Token: 0x020001DB RID: 475
	[BurstCompile]
	public struct ConvertQueueToHashset : IJob
	{
		// Token: 0x06000A01 RID: 2561 RVA: 0x00033BB8 File Offset: 0x00031DB8
		public void Execute()
		{
			while (this.ConnectionsQueue.Count > 0)
			{
				this.ConnectionsHashSet.Add(this.ConnectionsQueue.Dequeue());
			}
		}

		// Token: 0x040008E0 RID: 2272
		public NativeQueue<BidirectionalConnection> ConnectionsQueue;

		// Token: 0x040008E1 RID: 2273
		[WriteOnly]
		public NativeHashSet<BidirectionalConnection> ConnectionsHashSet;
	}
}
