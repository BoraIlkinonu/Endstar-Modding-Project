using System;
using System.Threading;

namespace Networking.UDP
{
	// Token: 0x0200002D RID: 45
	public sealed class NetStatistics
	{
		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000148 RID: 328 RVA: 0x000082AA File Offset: 0x000064AA
		public long PacketsSent
		{
			get
			{
				return Interlocked.Read(ref this._packetsSent);
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000149 RID: 329 RVA: 0x000082B7 File Offset: 0x000064B7
		public long PacketsReceived
		{
			get
			{
				return Interlocked.Read(ref this._packetsReceived);
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600014A RID: 330 RVA: 0x000082C4 File Offset: 0x000064C4
		public long BytesSent
		{
			get
			{
				return Interlocked.Read(ref this._bytesSent);
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600014B RID: 331 RVA: 0x000082D1 File Offset: 0x000064D1
		public long BytesReceived
		{
			get
			{
				return Interlocked.Read(ref this._bytesReceived);
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600014C RID: 332 RVA: 0x000082DE File Offset: 0x000064DE
		public long PacketLoss
		{
			get
			{
				return Interlocked.Read(ref this._packetLoss);
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600014D RID: 333 RVA: 0x000082EC File Offset: 0x000064EC
		public long PacketLossPercent
		{
			get
			{
				long packetsSent = this.PacketsSent;
				long packetLoss = this.PacketLoss;
				if (packetsSent != 0L)
				{
					return packetLoss * 100L / packetsSent;
				}
				return 0L;
			}
		}

		// Token: 0x0600014E RID: 334 RVA: 0x00008314 File Offset: 0x00006514
		public void Reset()
		{
			Interlocked.Exchange(ref this._packetsSent, 0L);
			Interlocked.Exchange(ref this._packetsReceived, 0L);
			Interlocked.Exchange(ref this._bytesSent, 0L);
			Interlocked.Exchange(ref this._bytesReceived, 0L);
			Interlocked.Exchange(ref this._packetLoss, 0L);
		}

		// Token: 0x0600014F RID: 335 RVA: 0x00008367 File Offset: 0x00006567
		public void IncrementPacketsSent()
		{
			Interlocked.Increment(ref this._packetsSent);
		}

		// Token: 0x06000150 RID: 336 RVA: 0x00008375 File Offset: 0x00006575
		public void IncrementPacketsReceived()
		{
			Interlocked.Increment(ref this._packetsReceived);
		}

		// Token: 0x06000151 RID: 337 RVA: 0x00008383 File Offset: 0x00006583
		public void AddBytesSent(long bytesSent)
		{
			Interlocked.Add(ref this._bytesSent, bytesSent);
		}

		// Token: 0x06000152 RID: 338 RVA: 0x00008392 File Offset: 0x00006592
		public void AddBytesReceived(long bytesReceived)
		{
			Interlocked.Add(ref this._bytesReceived, bytesReceived);
		}

		// Token: 0x06000153 RID: 339 RVA: 0x000083A1 File Offset: 0x000065A1
		public void IncrementPacketLoss()
		{
			Interlocked.Increment(ref this._packetLoss);
		}

		// Token: 0x06000154 RID: 340 RVA: 0x000083AF File Offset: 0x000065AF
		public void AddPacketLoss(long packetLoss)
		{
			Interlocked.Add(ref this._packetLoss, packetLoss);
		}

		// Token: 0x06000155 RID: 341 RVA: 0x000083C0 File Offset: 0x000065C0
		public override string ToString()
		{
			return string.Format("BytesReceived: {0}\nPacketsReceived: {1}\nBytesSent: {2}\nPacketsSent: {3}\nPacketLoss: {4}\nPacketLossPercent: {5}\n", new object[] { this.BytesReceived, this.PacketsReceived, this.BytesSent, this.PacketsSent, this.PacketLoss, this.PacketLossPercent });
		}

		// Token: 0x04000131 RID: 305
		private long _packetsSent;

		// Token: 0x04000132 RID: 306
		private long _packetsReceived;

		// Token: 0x04000133 RID: 307
		private long _bytesSent;

		// Token: 0x04000134 RID: 308
		private long _bytesReceived;

		// Token: 0x04000135 RID: 309
		private long _packetLoss;
	}
}
