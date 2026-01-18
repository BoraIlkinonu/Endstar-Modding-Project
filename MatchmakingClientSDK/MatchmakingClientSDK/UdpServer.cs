using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Networking.UDP;

namespace MatchmakingClientSDK
{
	// Token: 0x0200005E RID: 94
	public class UdpServer : IDisposable
	{
		// Token: 0x1400002F RID: 47
		// (add) Token: 0x06000396 RID: 918 RVA: 0x0000FD24 File Offset: 0x0000DF24
		// (remove) Token: 0x06000397 RID: 919 RVA: 0x0000FD5C File Offset: 0x0000DF5C
		public event Action<ulong> ClientConnected;

		// Token: 0x14000030 RID: 48
		// (add) Token: 0x06000398 RID: 920 RVA: 0x0000FD94 File Offset: 0x0000DF94
		// (remove) Token: 0x06000399 RID: 921 RVA: 0x0000FDCC File Offset: 0x0000DFCC
		public event Action<ulong, DisconnectInfo> ClientDisconnected;

		// Token: 0x14000031 RID: 49
		// (add) Token: 0x0600039A RID: 922 RVA: 0x0000FE04 File Offset: 0x0000E004
		// (remove) Token: 0x0600039B RID: 923 RVA: 0x0000FE3C File Offset: 0x0000E03C
		public event Action<ulong, ArraySegment<byte>> DataReceived;

		// Token: 0x14000032 RID: 50
		// (add) Token: 0x0600039C RID: 924 RVA: 0x0000FE74 File Offset: 0x0000E074
		// (remove) Token: 0x0600039D RID: 925 RVA: 0x0000FEAC File Offset: 0x0000E0AC
		public event Action<string> NetworkError;

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x0600039E RID: 926 RVA: 0x0000FEE1 File Offset: 0x0000E0E1
		// (set) Token: 0x0600039F RID: 927 RVA: 0x0000FEE9 File Offset: 0x0000E0E9
		public bool Disposed { get; private set; }

		// Token: 0x060003A0 RID: 928 RVA: 0x0000FEF4 File Offset: 0x0000E0F4
		public UdpServer(int port, Func<string, Task<string>> authenticator)
		{
			this.Port = port;
			this.authenticator = authenticator;
			this.listener = new EventBasedNetListener();
			this.server = new NetManager(this.listener, null)
			{
				AutoRecycle = true,
				AllowPeerAddressChange = true
			};
			this.server.Start(this.Port);
			this.listener.ConnectionRequestEvent += this.OnConnectionRequest;
			this.listener.PeerDisconnectedEvent += this.OnClientDisconnected;
			this.listener.NetworkReceiveEvent += this.OnReceivedData;
			this.listener.NetworkErrorEvent += this.OnNetworkError;
		}

		// Token: 0x060003A1 RID: 929 RVA: 0x0000FFC4 File Offset: 0x0000E1C4
		public void Update()
		{
			if (this.Disposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			this.server.PollEvents(0);
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x0000FFE8 File Offset: 0x0000E1E8
		private async void OnConnectionRequest(ConnectionRequest request)
		{
			try
			{
				string @string = request.Data.GetString();
				string text = await this.authenticator(@string);
				if (string.IsNullOrWhiteSpace(text))
				{
					request.Reject();
				}
				else
				{
					NetPeer netPeer = request.Accept();
					UdpClientData udpClientData = new UdpClientData(netPeer, text);
					this.connectedClients.Add(netPeer.Id, udpClientData);
					this.clientIdToPeerId.Add(udpClientData.ClientId, netPeer.Id);
					Action<ulong> clientConnected = this.ClientConnected;
					if (clientConnected != null)
					{
						clientConnected(udpClientData.ClientId);
					}
				}
			}
			catch (Exception)
			{
				request.Reject();
			}
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x00010028 File Offset: 0x0000E228
		private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			UdpClientData udpClientData;
			if (this.connectedClients.TryGetValue(peer.Id, out udpClientData))
			{
				this.connectedClients.Remove(peer.Id);
				this.clientIdToPeerId.Remove(udpClientData.ClientId);
				Action<ulong, DisconnectInfo> clientDisconnected = this.ClientDisconnected;
				if (clientDisconnected == null)
				{
					return;
				}
				clientDisconnected(udpClientData.ClientId, disconnectInfo);
			}
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x00010088 File Offset: 0x0000E288
		private void OnReceivedData(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
		{
			UdpClientData udpClientData;
			if (this.connectedClients.TryGetValue(peer.Id, out udpClientData))
			{
				ArraySegment<byte> arraySegment = new ArraySegment<byte>(reader.RawData, reader.UserDataOffset, reader.UserDataSize);
				Action<ulong, ArraySegment<byte>> dataReceived = this.DataReceived;
				if (dataReceived == null)
				{
					return;
				}
				dataReceived(udpClientData.ClientId, arraySegment);
			}
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x000100DC File Offset: 0x0000E2DC
		public void Send(ulong clientId, ArraySegment<byte> data, DeliveryMethod deliveryMethod)
		{
			int num;
			UdpClientData udpClientData;
			if (this.clientIdToPeerId.TryGetValue(clientId, out num) && this.connectedClients.TryGetValue(num, out udpClientData))
			{
				udpClientData.Peer.Send(data.Array, data.Offset, data.Count, deliveryMethod);
			}
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x0001012C File Offset: 0x0000E32C
		public void Broadcast(ArraySegment<byte> data, DeliveryMethod deliveryMethod)
		{
			foreach (UdpClientData udpClientData in this.connectedClients.Values)
			{
				udpClientData.Peer.Send(data.Array, data.Offset, data.Count, deliveryMethod);
			}
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x0001019C File Offset: 0x0000E39C
		public void DisconnectClient(ulong clientId)
		{
			int num;
			UdpClientData udpClientData;
			if (this.clientIdToPeerId.TryGetValue(clientId, out num) && this.connectedClients.TryGetValue(num, out udpClientData))
			{
				udpClientData.Peer.Disconnect();
			}
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x000101D4 File Offset: 0x0000E3D4
		public int GetLatency(ulong clientId)
		{
			if (clientId == 0UL)
			{
				return 0;
			}
			int num;
			if (this.clientIdToPeerId.TryGetValue(clientId, out num))
			{
				return this.connectedClients[num].Peer.RoundTripTime;
			}
			return 0;
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x0001020E File Offset: 0x0000E40E
		private void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
		{
			Action<string> networkError = this.NetworkError;
			if (networkError == null)
			{
				return;
			}
			networkError(string.Format("Network error: {0}", socketErrorCode));
		}

		// Token: 0x060003AA RID: 938 RVA: 0x00010230 File Offset: 0x0000E430
		public void Dispose()
		{
			if (this.Disposed)
			{
				throw new ObjectDisposedException("UdpServer");
			}
			this.Disposed = true;
			this.server.Stop();
			this.connectedClients.Clear();
			this.clientIdToPeerId.Clear();
		}

		// Token: 0x0400025D RID: 605
		public readonly int Port;

		// Token: 0x04000263 RID: 611
		private readonly EventBasedNetListener listener;

		// Token: 0x04000264 RID: 612
		private readonly NetManager server;

		// Token: 0x04000265 RID: 613
		private readonly Func<string, Task<string>> authenticator;

		// Token: 0x04000266 RID: 614
		private readonly Dictionary<int, UdpClientData> connectedClients = new Dictionary<int, UdpClientData>();

		// Token: 0x04000267 RID: 615
		private readonly Dictionary<ulong, int> clientIdToPeerId = new Dictionary<ulong, int>();
	}
}
