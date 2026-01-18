using System;
using Networking.UDP;

namespace MatchmakingClientSDK
{
	// Token: 0x0200005D RID: 93
	public readonly struct UdpClientData
	{
		// Token: 0x06000394 RID: 916 RVA: 0x0000FCDE File Offset: 0x0000DEDE
		public UdpClientData(NetPeer peer, string userId)
		{
			this.Peer = peer;
			this.UserId = userId;
			this.Channel = 0;
			this.ClientId = ulong.Parse(userId);
		}

		// Token: 0x06000395 RID: 917 RVA: 0x0000FD01 File Offset: 0x0000DF01
		public UdpClientData(NetPeer peer, string userId, byte channel)
		{
			this.Peer = peer;
			this.UserId = userId;
			this.Channel = channel;
			this.ClientId = ulong.Parse(userId);
		}

		// Token: 0x04000259 RID: 601
		public readonly NetPeer Peer;

		// Token: 0x0400025A RID: 602
		public readonly string UserId;

		// Token: 0x0400025B RID: 603
		public readonly byte Channel;

		// Token: 0x0400025C RID: 604
		public readonly ulong ClientId;
	}
}
