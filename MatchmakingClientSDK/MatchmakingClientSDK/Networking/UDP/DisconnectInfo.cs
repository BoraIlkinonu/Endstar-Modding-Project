using System;
using System.Net.Sockets;

namespace Networking.UDP
{
	// Token: 0x0200000F RID: 15
	public struct DisconnectInfo
	{
		// Token: 0x04000027 RID: 39
		public DisconnectReason Reason;

		// Token: 0x04000028 RID: 40
		public SocketError SocketErrorCode;

		// Token: 0x04000029 RID: 41
		public NetPacketReader AdditionalData;
	}
}
