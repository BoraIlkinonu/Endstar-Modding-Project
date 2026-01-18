using System;
using Networking.UDP.Utils;

namespace Networking.UDP
{
	// Token: 0x02000016 RID: 22
	internal sealed class NetConnectAcceptPacket
	{
		// Token: 0x06000059 RID: 89 RVA: 0x00002AC3 File Offset: 0x00000CC3
		private NetConnectAcceptPacket(long connectionTime, byte connectionNumber, int peerId, bool peerNetworkChanged)
		{
			this.ConnectionTime = connectionTime;
			this.ConnectionNumber = connectionNumber;
			this.PeerId = peerId;
			this.PeerNetworkChanged = peerNetworkChanged;
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00002AE8 File Offset: 0x00000CE8
		public static NetConnectAcceptPacket FromData(NetPacket packet)
		{
			if (packet.Size != 15)
			{
				return null;
			}
			long num = BitConverter.ToInt64(packet.RawData, 1);
			byte b = packet.RawData[9];
			if (b >= 64)
			{
				return null;
			}
			byte b2 = packet.RawData[10];
			if (b2 > 1)
			{
				return null;
			}
			int num2 = BitConverter.ToInt32(packet.RawData, 11);
			if (num2 < 0)
			{
				return null;
			}
			return new NetConnectAcceptPacket(num, b, num2, b2 == 1);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00002B4F File Offset: 0x00000D4F
		public static NetPacket Make(long connectTime, byte connectNum, int localPeerId)
		{
			NetPacket netPacket = new NetPacket(PacketProperty.ConnectAccept, 0);
			FastBitConverter.GetBytes(netPacket.RawData, 1, connectTime);
			netPacket.RawData[9] = connectNum;
			FastBitConverter.GetBytes(netPacket.RawData, 11, localPeerId);
			return netPacket;
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00002B80 File Offset: 0x00000D80
		public static NetPacket MakeNetworkChanged(NetPeer peer)
		{
			NetPacket netPacket = new NetPacket(PacketProperty.PeerNotFound, 14);
			FastBitConverter.GetBytes(netPacket.RawData, 1, peer.ConnectTime);
			netPacket.RawData[9] = peer.ConnectionNum;
			netPacket.RawData[10] = 1;
			FastBitConverter.GetBytes(netPacket.RawData, 11, peer.RemoteId);
			return netPacket;
		}

		// Token: 0x0400003A RID: 58
		public const int Size = 15;

		// Token: 0x0400003B RID: 59
		public readonly long ConnectionTime;

		// Token: 0x0400003C RID: 60
		public readonly byte ConnectionNumber;

		// Token: 0x0400003D RID: 61
		public readonly int PeerId;

		// Token: 0x0400003E RID: 62
		public readonly bool PeerNetworkChanged;
	}
}
