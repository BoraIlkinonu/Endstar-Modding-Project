using System;
using System.Net;
using System.Net.Sockets;

namespace Networking.UDP
{
	// Token: 0x02000024 RID: 36
	internal sealed class NetEvent
	{
		// Token: 0x06000086 RID: 134 RVA: 0x000035C9 File Offset: 0x000017C9
		public NetEvent(NetManager manager)
		{
			this.DataReader = new NetPacketReader(manager, this);
		}

		// Token: 0x04000075 RID: 117
		public NetEvent Next;

		// Token: 0x04000076 RID: 118
		public NetEvent.EType Type;

		// Token: 0x04000077 RID: 119
		public NetPeer Peer;

		// Token: 0x04000078 RID: 120
		public IPEndPoint RemoteEndPoint;

		// Token: 0x04000079 RID: 121
		public object UserData;

		// Token: 0x0400007A RID: 122
		public int Latency;

		// Token: 0x0400007B RID: 123
		public SocketError ErrorCode;

		// Token: 0x0400007C RID: 124
		public DisconnectReason DisconnectReason;

		// Token: 0x0400007D RID: 125
		public ConnectionRequest ConnectionRequest;

		// Token: 0x0400007E RID: 126
		public DeliveryMethod DeliveryMethod;

		// Token: 0x0400007F RID: 127
		public byte ChannelNumber;

		// Token: 0x04000080 RID: 128
		public readonly NetPacketReader DataReader;

		// Token: 0x02000085 RID: 133
		public enum EType
		{
			// Token: 0x040002C6 RID: 710
			Connect,
			// Token: 0x040002C7 RID: 711
			Disconnect,
			// Token: 0x040002C8 RID: 712
			Receive,
			// Token: 0x040002C9 RID: 713
			ReceiveUnconnected,
			// Token: 0x040002CA RID: 714
			Error,
			// Token: 0x040002CB RID: 715
			ConnectionLatencyUpdated,
			// Token: 0x040002CC RID: 716
			Broadcast,
			// Token: 0x040002CD RID: 717
			ConnectionRequest,
			// Token: 0x040002CE RID: 718
			MessageDelivered,
			// Token: 0x040002CF RID: 719
			PeerAddressChanged
		}
	}
}
