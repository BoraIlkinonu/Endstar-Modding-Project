using System;
using System.Collections.Generic;
using System.Threading;

namespace Networking.UDP
{
	// Token: 0x0200000A RID: 10
	internal abstract class BaseChannel
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600000A RID: 10 RVA: 0x0000210E File Offset: 0x0000030E
		public int PacketsInQueue
		{
			get
			{
				return this.OutgoingQueue.Count;
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000211B File Offset: 0x0000031B
		protected BaseChannel(NetPeer peer)
		{
			this.Peer = peer;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002138 File Offset: 0x00000338
		public void AddToQueue(NetPacket packet)
		{
			Queue<NetPacket> outgoingQueue = this.OutgoingQueue;
			lock (outgoingQueue)
			{
				this.OutgoingQueue.Enqueue(packet);
			}
			this.AddToPeerChannelSendQueue();
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002184 File Offset: 0x00000384
		protected void AddToPeerChannelSendQueue()
		{
			if (Interlocked.CompareExchange(ref this._isAddedToPeerChannelSendQueue, 1, 0) == 0)
			{
				this.Peer.AddToReliableChannelSendQueue(this);
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000021A1 File Offset: 0x000003A1
		public bool SendAndCheckQueue()
		{
			bool flag = this.SendNextPackets();
			if (!flag)
			{
				Interlocked.Exchange(ref this._isAddedToPeerChannelSendQueue, 0);
			}
			return flag;
		}

		// Token: 0x0600000F RID: 15
		protected abstract bool SendNextPackets();

		// Token: 0x06000010 RID: 16
		public abstract bool ProcessPacket(NetPacket packet);

		// Token: 0x0400000A RID: 10
		protected readonly NetPeer Peer;

		// Token: 0x0400000B RID: 11
		protected readonly Queue<NetPacket> OutgoingQueue = new Queue<NetPacket>(64);

		// Token: 0x0400000C RID: 12
		private int _isAddedToPeerChannelSendQueue;
	}
}
