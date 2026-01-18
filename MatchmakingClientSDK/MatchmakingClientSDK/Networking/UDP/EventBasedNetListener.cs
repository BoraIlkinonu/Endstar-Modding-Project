using System;
using System.Net;
using System.Net.Sockets;
using Networking.UDP.Utils;

namespace Networking.UDP
{
	// Token: 0x02000014 RID: 20
	public class EventBasedNetListener : INetEventListener, IDeliveryEventListener, INtpEventListener, IPeerAddressChangedListener
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x0600002C RID: 44 RVA: 0x000023A0 File Offset: 0x000005A0
		// (remove) Token: 0x0600002D RID: 45 RVA: 0x000023D8 File Offset: 0x000005D8
		public event EventBasedNetListener.OnPeerConnected PeerConnectedEvent;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x0600002E RID: 46 RVA: 0x00002410 File Offset: 0x00000610
		// (remove) Token: 0x0600002F RID: 47 RVA: 0x00002448 File Offset: 0x00000648
		public event EventBasedNetListener.OnPeerDisconnected PeerDisconnectedEvent;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000030 RID: 48 RVA: 0x00002480 File Offset: 0x00000680
		// (remove) Token: 0x06000031 RID: 49 RVA: 0x000024B8 File Offset: 0x000006B8
		public event EventBasedNetListener.OnNetworkError NetworkErrorEvent;

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000032 RID: 50 RVA: 0x000024F0 File Offset: 0x000006F0
		// (remove) Token: 0x06000033 RID: 51 RVA: 0x00002528 File Offset: 0x00000728
		public event EventBasedNetListener.OnNetworkReceive NetworkReceiveEvent;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000034 RID: 52 RVA: 0x00002560 File Offset: 0x00000760
		// (remove) Token: 0x06000035 RID: 53 RVA: 0x00002598 File Offset: 0x00000798
		public event EventBasedNetListener.OnNetworkReceiveUnconnected NetworkReceiveUnconnectedEvent;

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x06000036 RID: 54 RVA: 0x000025D0 File Offset: 0x000007D0
		// (remove) Token: 0x06000037 RID: 55 RVA: 0x00002608 File Offset: 0x00000808
		public event EventBasedNetListener.OnNetworkLatencyUpdate NetworkLatencyUpdateEvent;

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x06000038 RID: 56 RVA: 0x00002640 File Offset: 0x00000840
		// (remove) Token: 0x06000039 RID: 57 RVA: 0x00002678 File Offset: 0x00000878
		public event EventBasedNetListener.OnConnectionRequest ConnectionRequestEvent;

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x0600003A RID: 58 RVA: 0x000026B0 File Offset: 0x000008B0
		// (remove) Token: 0x0600003B RID: 59 RVA: 0x000026E8 File Offset: 0x000008E8
		public event EventBasedNetListener.OnDeliveryEvent DeliveryEvent;

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x0600003C RID: 60 RVA: 0x00002720 File Offset: 0x00000920
		// (remove) Token: 0x0600003D RID: 61 RVA: 0x00002758 File Offset: 0x00000958
		public event EventBasedNetListener.OnNtpResponseEvent NtpResponseEvent;

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x0600003E RID: 62 RVA: 0x00002790 File Offset: 0x00000990
		// (remove) Token: 0x0600003F RID: 63 RVA: 0x000027C8 File Offset: 0x000009C8
		public event EventBasedNetListener.OnPeerAddressChangedEvent PeerAddressChangedEvent;

		// Token: 0x06000040 RID: 64 RVA: 0x000027FD File Offset: 0x000009FD
		public void ClearPeerConnectedEvent()
		{
			this.PeerConnectedEvent = null;
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00002806 File Offset: 0x00000A06
		public void ClearPeerDisconnectedEvent()
		{
			this.PeerDisconnectedEvent = null;
		}

		// Token: 0x06000042 RID: 66 RVA: 0x0000280F File Offset: 0x00000A0F
		public void ClearNetworkErrorEvent()
		{
			this.NetworkErrorEvent = null;
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00002818 File Offset: 0x00000A18
		public void ClearNetworkReceiveEvent()
		{
			this.NetworkReceiveEvent = null;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00002821 File Offset: 0x00000A21
		public void ClearNetworkReceiveUnconnectedEvent()
		{
			this.NetworkReceiveUnconnectedEvent = null;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x0000282A File Offset: 0x00000A2A
		public void ClearNetworkLatencyUpdateEvent()
		{
			this.NetworkLatencyUpdateEvent = null;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00002833 File Offset: 0x00000A33
		public void ClearConnectionRequestEvent()
		{
			this.ConnectionRequestEvent = null;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x0000283C File Offset: 0x00000A3C
		public void ClearDeliveryEvent()
		{
			this.DeliveryEvent = null;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00002845 File Offset: 0x00000A45
		public void ClearNtpResponseEvent()
		{
			this.NtpResponseEvent = null;
		}

		// Token: 0x06000049 RID: 73 RVA: 0x0000284E File Offset: 0x00000A4E
		public void ClearPeerAddressChangedEvent()
		{
			this.PeerAddressChangedEvent = null;
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00002857 File Offset: 0x00000A57
		void INetEventListener.OnPeerConnected(NetPeer peer)
		{
			if (this.PeerConnectedEvent != null)
			{
				this.PeerConnectedEvent(peer);
			}
		}

		// Token: 0x0600004B RID: 75 RVA: 0x0000286D File Offset: 0x00000A6D
		void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			if (this.PeerDisconnectedEvent != null)
			{
				this.PeerDisconnectedEvent(peer, disconnectInfo);
			}
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00002884 File Offset: 0x00000A84
		void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
		{
			if (this.NetworkErrorEvent != null)
			{
				this.NetworkErrorEvent(endPoint, socketErrorCode);
			}
		}

		// Token: 0x0600004D RID: 77 RVA: 0x0000289B File Offset: 0x00000A9B
		void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
		{
			if (this.NetworkReceiveEvent != null)
			{
				this.NetworkReceiveEvent(peer, reader, channelNumber, deliveryMethod);
			}
		}

		// Token: 0x0600004E RID: 78 RVA: 0x000028B5 File Offset: 0x00000AB5
		void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
		{
			if (this.NetworkReceiveUnconnectedEvent != null)
			{
				this.NetworkReceiveUnconnectedEvent(remoteEndPoint, reader, messageType);
			}
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000028CD File Offset: 0x00000ACD
		void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
		{
			if (this.NetworkLatencyUpdateEvent != null)
			{
				this.NetworkLatencyUpdateEvent(peer, latency);
			}
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000028E4 File Offset: 0x00000AE4
		void INetEventListener.OnConnectionRequest(ConnectionRequest request)
		{
			if (this.ConnectionRequestEvent != null)
			{
				this.ConnectionRequestEvent(request);
			}
		}

		// Token: 0x06000051 RID: 81 RVA: 0x000028FA File Offset: 0x00000AFA
		void IDeliveryEventListener.OnMessageDelivered(NetPeer peer, object userData)
		{
			if (this.DeliveryEvent != null)
			{
				this.DeliveryEvent(peer, userData);
			}
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00002911 File Offset: 0x00000B11
		void INtpEventListener.OnNtpResponse(NtpPacket packet)
		{
			if (this.NtpResponseEvent != null)
			{
				this.NtpResponseEvent(packet);
			}
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00002927 File Offset: 0x00000B27
		void IPeerAddressChangedListener.OnPeerAddressChanged(NetPeer peer, IPEndPoint previousAddress)
		{
			if (this.PeerAddressChangedEvent != null)
			{
				this.PeerAddressChangedEvent(peer, previousAddress);
			}
		}

		// Token: 0x02000072 RID: 114
		// (Invoke) Token: 0x06000434 RID: 1076
		public delegate void OnPeerConnected(NetPeer peer);

		// Token: 0x02000073 RID: 115
		// (Invoke) Token: 0x06000438 RID: 1080
		public delegate void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);

		// Token: 0x02000074 RID: 116
		// (Invoke) Token: 0x0600043C RID: 1084
		public delegate void OnNetworkError(IPEndPoint endPoint, SocketError socketError);

		// Token: 0x02000075 RID: 117
		// (Invoke) Token: 0x06000440 RID: 1088
		public delegate void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod);

		// Token: 0x02000076 RID: 118
		// (Invoke) Token: 0x06000444 RID: 1092
		public delegate void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);

		// Token: 0x02000077 RID: 119
		// (Invoke) Token: 0x06000448 RID: 1096
		public delegate void OnNetworkLatencyUpdate(NetPeer peer, int latency);

		// Token: 0x02000078 RID: 120
		// (Invoke) Token: 0x0600044C RID: 1100
		public delegate void OnConnectionRequest(ConnectionRequest request);

		// Token: 0x02000079 RID: 121
		// (Invoke) Token: 0x06000450 RID: 1104
		public delegate void OnDeliveryEvent(NetPeer peer, object userData);

		// Token: 0x0200007A RID: 122
		// (Invoke) Token: 0x06000454 RID: 1108
		public delegate void OnNtpResponseEvent(NtpPacket packet);

		// Token: 0x0200007B RID: 123
		// (Invoke) Token: 0x06000458 RID: 1112
		public delegate void OnPeerAddressChangedEvent(NetPeer peer, IPEndPoint previousAddress);
	}
}
