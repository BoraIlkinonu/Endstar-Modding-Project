using System;
using System.Net;
using System.Net.Sockets;

namespace Networking.UDP
{
	// Token: 0x02000010 RID: 16
	public interface INetEventListener
	{
		// Token: 0x06000022 RID: 34
		void OnPeerConnected(NetPeer peer);

		// Token: 0x06000023 RID: 35
		void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);

		// Token: 0x06000024 RID: 36
		void OnNetworkError(IPEndPoint endPoint, SocketError socketError);

		// Token: 0x06000025 RID: 37
		void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod);

		// Token: 0x06000026 RID: 38
		void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);

		// Token: 0x06000027 RID: 39
		void OnNetworkLatencyUpdate(NetPeer peer, int latency);

		// Token: 0x06000028 RID: 40
		void OnConnectionRequest(ConnectionRequest request);
	}
}
