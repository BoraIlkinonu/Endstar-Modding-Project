using System;
using Networking.UDP.Utils;

namespace Networking.UDP
{
	// Token: 0x02000023 RID: 35
	public sealed class NetPacketReader : NetDataReader
	{
		// Token: 0x06000082 RID: 130 RVA: 0x00003544 File Offset: 0x00001744
		internal NetPacketReader(NetManager manager, NetEvent evt)
		{
			this._manager = manager;
			this._evt = evt;
		}

		// Token: 0x06000083 RID: 131 RVA: 0x0000355A File Offset: 0x0000175A
		internal void SetSource(NetPacket packet, int headerSize)
		{
			if (packet == null)
			{
				return;
			}
			this._packet = packet;
			base.SetSource(packet.RawData, headerSize, packet.Size);
		}

		// Token: 0x06000084 RID: 132 RVA: 0x0000357A File Offset: 0x0000177A
		internal void RecycleInternal()
		{
			base.Clear();
			if (this._packet != null)
			{
				this._manager.PoolRecycle(this._packet);
			}
			this._packet = null;
			this._manager.RecycleEvent(this._evt);
		}

		// Token: 0x06000085 RID: 133 RVA: 0x000035B3 File Offset: 0x000017B3
		public void Recycle()
		{
			if (this._manager.AutoRecycle)
			{
				return;
			}
			this.RecycleInternal();
		}

		// Token: 0x04000072 RID: 114
		private NetPacket _packet;

		// Token: 0x04000073 RID: 115
		private readonly NetManager _manager;

		// Token: 0x04000074 RID: 116
		private readonly NetEvent _evt;
	}
}
