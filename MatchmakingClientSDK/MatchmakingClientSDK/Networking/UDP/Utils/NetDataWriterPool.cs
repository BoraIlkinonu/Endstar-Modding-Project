using System;
using System.Collections.Concurrent;

namespace Networking.UDP.Utils
{
	// Token: 0x0200003C RID: 60
	public static class NetDataWriterPool
	{
		// Token: 0x06000231 RID: 561 RVA: 0x0000AE3C File Offset: 0x0000903C
		public static NetDataWriter GetNetDataWriter()
		{
			NetDataWriter netDataWriter;
			if (NetDataWriterPool.pool.TryDequeue(out netDataWriter))
			{
				return netDataWriter;
			}
			return new NetDataWriter();
		}

		// Token: 0x06000232 RID: 562 RVA: 0x0000AE5E File Offset: 0x0000905E
		public static void RecycleBuffer(NetDataWriter buffer)
		{
			if (buffer == null)
			{
				return;
			}
			buffer.Reset();
			NetDataWriterPool.pool.Enqueue(buffer);
		}

		// Token: 0x04000174 RID: 372
		private static readonly ConcurrentQueue<NetDataWriter> pool = new ConcurrentQueue<NetDataWriter>();
	}
}
