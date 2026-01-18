using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Networking.UDP;
using Networking.UDP.Utils;

namespace MatchmakingClientSDK
{
	// Token: 0x0200005C RID: 92
	public class UdpClient : IDisposable
	{
		// Token: 0x14000029 RID: 41
		// (add) Token: 0x06000374 RID: 884 RVA: 0x0000F59C File Offset: 0x0000D79C
		// (remove) Token: 0x06000375 RID: 885 RVA: 0x0000F5D4 File Offset: 0x0000D7D4
		public event Action<ulong> Connected;

		// Token: 0x1400002A RID: 42
		// (add) Token: 0x06000376 RID: 886 RVA: 0x0000F60C File Offset: 0x0000D80C
		// (remove) Token: 0x06000377 RID: 887 RVA: 0x0000F644 File Offset: 0x0000D844
		public event Action<ulong, DisconnectInfo> ConnectionFailed;

		// Token: 0x1400002B RID: 43
		// (add) Token: 0x06000378 RID: 888 RVA: 0x0000F67C File Offset: 0x0000D87C
		// (remove) Token: 0x06000379 RID: 889 RVA: 0x0000F6B4 File Offset: 0x0000D8B4
		public event Action<ulong, ArraySegment<byte>> Received;

		// Token: 0x1400002C RID: 44
		// (add) Token: 0x0600037A RID: 890 RVA: 0x0000F6EC File Offset: 0x0000D8EC
		// (remove) Token: 0x0600037B RID: 891 RVA: 0x0000F724 File Offset: 0x0000D924
		public event Action<NetPeer, NetPacketReader, byte, DeliveryMethod> ReceivedRaw;

		// Token: 0x1400002D RID: 45
		// (add) Token: 0x0600037C RID: 892 RVA: 0x0000F75C File Offset: 0x0000D95C
		// (remove) Token: 0x0600037D RID: 893 RVA: 0x0000F794 File Offset: 0x0000D994
		public event Action<ulong, DisconnectInfo> Disconnected;

		// Token: 0x1400002E RID: 46
		// (add) Token: 0x0600037E RID: 894 RVA: 0x0000F7CC File Offset: 0x0000D9CC
		// (remove) Token: 0x0600037F RID: 895 RVA: 0x0000F804 File Offset: 0x0000DA04
		public event Action<string> NetworkError;

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000380 RID: 896 RVA: 0x0000F839 File Offset: 0x0000DA39
		// (set) Token: 0x06000381 RID: 897 RVA: 0x0000F841 File Offset: 0x0000DA41
		public bool IsConnecting { get; private set; }

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000382 RID: 898 RVA: 0x0000F84A File Offset: 0x0000DA4A
		// (set) Token: 0x06000383 RID: 899 RVA: 0x0000F852 File Offset: 0x0000DA52
		public bool IsConnected { get; private set; }

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x06000384 RID: 900 RVA: 0x0000F85B File Offset: 0x0000DA5B
		// (set) Token: 0x06000385 RID: 901 RVA: 0x0000F863 File Offset: 0x0000DA63
		public bool Disposed { get; private set; }

		// Token: 0x06000386 RID: 902 RVA: 0x0000F86C File Offset: 0x0000DA6C
		public UdpClient(ulong clientId, bool autoReconnect = false)
		{
			this.ClientId = clientId;
			this.AutoReconnect = autoReconnect;
			this.listener = new EventBasedNetListener();
			this.client = new NetManager(this.listener, null)
			{
				MaxConnectAttempts = 10,
				DisconnectTimeout = 30000,
				AutoRecycle = true
			};
			this.client.Start();
			this.listener.PeerConnectedEvent += this.OnConnected;
			this.listener.PeerDisconnectedEvent += this.OnDisconnected;
			this.listener.NetworkReceiveEvent += this.OnNetworkReceive;
			this.listener.NetworkLatencyUpdateEvent += this.OnLatencyUpdate;
			this.listener.NetworkErrorEvent += this.OnNetworkError;
		}

		// Token: 0x06000387 RID: 903 RVA: 0x0000F94E File Offset: 0x0000DB4E
		private void OnLatencyUpdate(NetPeer netPeer, int latency)
		{
			this.latency = latency;
		}

		// Token: 0x06000388 RID: 904 RVA: 0x0000F958 File Offset: 0x0000DB58
		public void Connect(params ConnectionData[] connections)
		{
			if (this.Disposed)
			{
				throw new ObjectDisposedException("UdpClient");
			}
			if (this.IsConnecting)
			{
				throw new InvalidOperationException("Already connecting to server.");
			}
			if (this.IsConnected)
			{
				throw new InvalidOperationException("Already connected to server.");
			}
			if (connections == null || connections.Length == 0)
			{
				throw new ArgumentException("No connection data provided.");
			}
			this.endpoints = connections;
			this.endPointIndex = 0;
			ConnectionData connectionData = this.endpoints[this.endPointIndex];
			this.IsConnecting = true;
			this.IsConnected = false;
			this.clientPeer = this.client.Connect(connectionData.Ip, connectionData.Port, connectionData.Key);
		}

		// Token: 0x06000389 RID: 905 RVA: 0x0000FA01 File Offset: 0x0000DC01
		public void Reconnect()
		{
			this.Connect(this.endpoints);
		}

		// Token: 0x0600038A RID: 906 RVA: 0x0000FA0F File Offset: 0x0000DC0F
		public void Disconnect()
		{
			if (this.Disposed)
			{
				throw new ObjectDisposedException("UdpClient");
			}
			NetPeer netPeer = this.clientPeer;
			if (netPeer == null)
			{
				return;
			}
			netPeer.Disconnect();
		}

		// Token: 0x0600038B RID: 907 RVA: 0x0000FA34 File Offset: 0x0000DC34
		public void Update()
		{
			if (this.Disposed)
			{
				throw new ObjectDisposedException("UdpClient");
			}
			this.client.PollEvents(0);
		}

		// Token: 0x0600038C RID: 908 RVA: 0x0000FA55 File Offset: 0x0000DC55
		private void OnConnected(NetPeer connectedPeer)
		{
			this.clientPeer = connectedPeer;
			this.IsConnected = true;
			this.IsConnecting = false;
			Action<ulong> connected = this.Connected;
			if (connected == null)
			{
				return;
			}
			connected(this.ClientId);
		}

		// Token: 0x0600038D RID: 909 RVA: 0x0000FA84 File Offset: 0x0000DC84
		private void OnNetworkReceive(NetPeer sendPeer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
		{
			Action<NetPeer, NetPacketReader, byte, DeliveryMethod> receivedRaw = this.ReceivedRaw;
			if (receivedRaw != null)
			{
				receivedRaw(sendPeer, reader, channel, deliveryMethod);
			}
			ArraySegment<byte> arraySegment = new ArraySegment<byte>(reader.RawData, reader.UserDataOffset, reader.UserDataSize);
			Action<ulong, ArraySegment<byte>> received = this.Received;
			if (received == null)
			{
				return;
			}
			received(0UL, arraySegment);
		}

		// Token: 0x0600038E RID: 910 RVA: 0x0000FAD4 File Offset: 0x0000DCD4
		public void Send(ArraySegment<byte> data, DeliveryMethod deliveryMethod)
		{
			if (this.Disposed)
			{
				throw new ObjectDisposedException("UdpClient");
			}
			if (!this.IsConnected)
			{
				return;
			}
			NetPeer netPeer = this.clientPeer;
			if (netPeer == null)
			{
				return;
			}
			netPeer.Send(data.Array, data.Offset, data.Count, deliveryMethod);
		}

		// Token: 0x0600038F RID: 911 RVA: 0x0000FB23 File Offset: 0x0000DD23
		public void Send(NetDataWriter writer, DeliveryMethod deliveryMethod)
		{
			if (this.Disposed)
			{
				throw new ObjectDisposedException("UdpClient");
			}
			if (!this.IsConnected)
			{
				return;
			}
			NetPeer netPeer = this.clientPeer;
			if (netPeer == null)
			{
				return;
			}
			netPeer.Send(writer, deliveryMethod);
		}

		// Token: 0x06000390 RID: 912 RVA: 0x0000FB54 File Offset: 0x0000DD54
		private void OnDisconnected(NetPeer disconnectedPeer, DisconnectInfo info)
		{
			this.clientPeer = null;
			if (this.IsConnecting)
			{
				this.endPointIndex = (this.endPointIndex + 1) % this.endpoints.Length;
				if (this.endPointIndex != 0)
				{
					ConnectionData connectionData = this.endpoints[this.endPointIndex];
					this.IsConnecting = true;
					this.IsConnected = false;
					this.clientPeer = this.client.Connect(connectionData.Ip, connectionData.Port, connectionData.Key);
					return;
				}
				this.IsConnecting = false;
				this.IsConnected = false;
				Action<ulong, DisconnectInfo> connectionFailed = this.ConnectionFailed;
				if (connectionFailed != null)
				{
					connectionFailed(this.ClientId, info);
				}
			}
			else
			{
				this.IsConnected = false;
				this.IsConnecting = false;
				Action<ulong, DisconnectInfo> disconnected = this.Disconnected;
				if (disconnected != null)
				{
					disconnected(this.ClientId, info);
				}
			}
			if (this.AutoReconnect)
			{
				this.Reconnect();
			}
		}

		// Token: 0x06000391 RID: 913 RVA: 0x0000FC32 File Offset: 0x0000DE32
		public int GetLatency()
		{
			return this.latency;
		}

		// Token: 0x06000392 RID: 914 RVA: 0x0000FC3A File Offset: 0x0000DE3A
		private void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
		{
			Action<string> networkError = this.NetworkError;
			if (networkError == null)
			{
				return;
			}
			networkError(string.Format("Network error: {0}", socketError));
		}

		// Token: 0x06000393 RID: 915 RVA: 0x0000FC5C File Offset: 0x0000DE5C
		public void Dispose()
		{
			if (this.Disposed)
			{
				throw new ObjectDisposedException("UdpClient");
			}
			bool isConnected = this.IsConnected;
			this.Disposed = true;
			this.client.Stop();
			this.clientPeer = null;
			this.IsConnected = false;
			this.IsConnecting = false;
			if (isConnected)
			{
				Action<ulong, DisconnectInfo> disconnected = this.Disconnected;
				if (disconnected == null)
				{
					return;
				}
				disconnected(this.ClientId, new DisconnectInfo
				{
					Reason = DisconnectReason.DisconnectPeerCalled,
					SocketErrorCode = SocketError.ConnectionAborted
				});
			}
		}

		// Token: 0x04000247 RID: 583
		public readonly ulong ClientId;

		// Token: 0x04000248 RID: 584
		public readonly bool AutoReconnect;

		// Token: 0x04000252 RID: 594
		private readonly EventBasedNetListener listener;

		// Token: 0x04000253 RID: 595
		private readonly NetManager client;

		// Token: 0x04000254 RID: 596
		private readonly NetDataWriter writer = new NetDataWriter();

		// Token: 0x04000255 RID: 597
		[Nullable(2)]
		private NetPeer clientPeer;

		// Token: 0x04000256 RID: 598
		private int latency;

		// Token: 0x04000257 RID: 599
		private ConnectionData[] endpoints;

		// Token: 0x04000258 RID: 600
		private int endPointIndex;
	}
}
