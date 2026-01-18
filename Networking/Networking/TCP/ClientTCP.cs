using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Timers;

namespace Endless.Networking.TCP
{
	// Token: 0x02000011 RID: 17
	public class ClientTCP
	{
		// Token: 0x0600006E RID: 110 RVA: 0x00003550 File Offset: 0x00001750
		public void HandleEvents()
		{
			Queue<ClientTCP.ConnEvent> queue = this.connectionEvents;
			lock (queue)
			{
				while (this.connectionEvents.Count > 0)
				{
					ClientTCP.ConnEvent connEvent = this.connectionEvents.Dequeue();
					bool flag2 = connEvent.ConnectionType == 1;
					if (flag2)
					{
						this.OnConnectionEstablished();
					}
					else
					{
						bool flag3 = connEvent.ConnectionType == 0;
						if (flag3)
						{
							this.OnConnectionTerminated();
						}
						else
						{
							bool flag4 = connEvent.ConnectionType == 2;
							if (flag4)
							{
								this.OnConnectionFailed();
							}
						}
					}
				}
			}
			while (this.pendingPackets.Count > 0)
			{
				ClientTCP.PacketData packetData = null;
				this.pendingPackets.TryDequeue(out packetData);
				bool flag5 = packetData != null;
				if (flag5)
				{
					ClientTCP.PacketEvent onPacketReceived = this.OnPacketReceived;
					if (onPacketReceived != null)
					{
						onPacketReceived(packetData.PacketBuffer);
					}
				}
			}
		}

		// Token: 0x0600006F RID: 111 RVA: 0x0000365C File Offset: 0x0000185C
		private void onConnectedEvent(TCPConnection _connection)
		{
			Queue<ClientTCP.ConnEvent> queue = this.connectionEvents;
			lock (queue)
			{
				this.connectionEvents.Enqueue(new ClientTCP.ConnEvent
				{
					Connection = _connection,
					ConnectionType = 1
				});
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x000036BC File Offset: 0x000018BC
		private void onConnectionFailedEvent(TCPConnection _connection)
		{
			Queue<ClientTCP.ConnEvent> queue = this.connectionEvents;
			lock (queue)
			{
				this.connectionEvents.Enqueue(new ClientTCP.ConnEvent
				{
					Connection = _connection,
					ConnectionType = 2
				});
			}
		}

		// Token: 0x06000071 RID: 113 RVA: 0x0000371C File Offset: 0x0000191C
		private void onDisconnectedEvent(TCPConnection _connection)
		{
			Queue<ClientTCP.ConnEvent> queue = this.connectionEvents;
			lock (queue)
			{
				this.connectionEvents.Enqueue(new ClientTCP.ConnEvent
				{
					Connection = _connection,
					ConnectionType = 0
				});
			}
		}

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000072 RID: 114 RVA: 0x0000377C File Offset: 0x0000197C
		// (remove) Token: 0x06000073 RID: 115 RVA: 0x000037B4 File Offset: 0x000019B4
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ClientTCP.ConnectionEvent OnConnectionEstablished;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000074 RID: 116 RVA: 0x000037EC File Offset: 0x000019EC
		// (remove) Token: 0x06000075 RID: 117 RVA: 0x00003824 File Offset: 0x00001A24
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ClientTCP.ConnectionEvent OnConnectionFailed;

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x06000076 RID: 118 RVA: 0x0000385C File Offset: 0x00001A5C
		// (remove) Token: 0x06000077 RID: 119 RVA: 0x00003894 File Offset: 0x00001A94
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ClientTCP.ConnectionEvent OnConnectionTerminated;

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x06000078 RID: 120 RVA: 0x000038CC File Offset: 0x00001ACC
		// (remove) Token: 0x06000079 RID: 121 RVA: 0x00003904 File Offset: 0x00001B04
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ClientTCP.PacketEvent OnPacketReceived;

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600007A RID: 122 RVA: 0x00003939 File Offset: 0x00001B39
		// (set) Token: 0x0600007B RID: 123 RVA: 0x00003941 File Offset: 0x00001B41
		public bool Busy { get; private set; } = false;

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600007C RID: 124 RVA: 0x0000394A File Offset: 0x00001B4A
		// (set) Token: 0x0600007D RID: 125 RVA: 0x00003952 File Offset: 0x00001B52
		public bool Connected { get; private set; } = false;

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600007E RID: 126 RVA: 0x0000395B File Offset: 0x00001B5B
		// (set) Token: 0x0600007F RID: 127 RVA: 0x00003963 File Offset: 0x00001B63
		public bool AutoReconnect { get; set; } = false;

		// Token: 0x06000080 RID: 128 RVA: 0x0000396C File Offset: 0x00001B6C
		public ClientTCP()
		{
			this.Connected = false;
			this.Busy = false;
			this.connectingTimeoutTimer = new Timer(5000.0);
			this.connectingTimeoutTimer.Elapsed += new ElapsedEventHandler(this.connectingTimedOut);
			this.connectingTimeoutTimer.AutoReset = false;
			this.connectionTimeoutTimer = new Timer(250.0);
			this.connectionTimeoutTimer.Elapsed += this.checkConnectionTimeout;
			this.connectionTimeoutTimer.AutoReset = true;
			this.connectionTimeoutTimer.Start();
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00003A56 File Offset: 0x00001C56
		public void StartConnection(string serverIp, int serverPort)
		{
			this.Connected = false;
			this.Busy = true;
			this.connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.serverIp = serverIp;
			this.serverPort = serverPort;
			this.TryConnect();
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00003A8C File Offset: 0x00001C8C
		private void TryConnect()
		{
			try
			{
				IAsyncResult asyncResult = this.connectionSocket.BeginConnect(this.serverIp, this.serverPort, new AsyncCallback(this.ConnectCallback), this.connectionSocket);
				this.connectingTimeoutTimer.Start();
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), true);
				this.onConnectionFailed();
			}
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00003B04 File Offset: 0x00001D04
		private void connectingTimedOut(object _sender, EventArgs _args)
		{
			try
			{
				this.connectingTimeoutTimer.Stop();
				this.connectionSocket.Close();
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), true);
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00003B58 File Offset: 0x00001D58
		private void ConnectCallback(IAsyncResult _ar)
		{
			try
			{
				this.connectionSocket.EndConnect(_ar);
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), true);
				this.onConnectionFailed();
				return;
			}
			this.connectingTimeoutTimer.Stop();
			this.tcpConnection = new TCPConnection();
			this.tcpConnection.OnConnectionEstablished += this.onConnectionEstablished;
			this.tcpConnection.OnConnectionTerminated += this.onConnectionTerminated;
			this.tcpConnection.OnPacketReceived += this.onPacketReceived;
			this.tcpConnection.LogFile = this.LogFile;
			this.tcpConnection.StartConnection(this.connectionSocket);
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00003C2C File Offset: 0x00001E2C
		private void onConnectionEstablished(TCPConnection _connection)
		{
			this.Connected = true;
			this.onConnectedEvent(_connection);
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00003C40 File Offset: 0x00001E40
		private void onConnectionFailed()
		{
			bool enabled = this.connectingTimeoutTimer.Enabled;
			if (enabled)
			{
				this.connectingTimedOut(null, null);
			}
			this.Busy = false;
			this.onConnectionFailedEvent(null);
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00003C78 File Offset: 0x00001E78
		private void onConnectionTerminated(TCPConnection _connection)
		{
			this.Connected = false;
			this.Busy = false;
			this.onDisconnectedEvent(_connection);
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00003C94 File Offset: 0x00001E94
		private void onPacketReceived(TCPConnection _connection, DataBuffer _packet)
		{
			ClientTCP.PacketData packetData = new ClientTCP.PacketData
			{
				Connection = _connection,
				PacketBuffer = _packet
			};
			this.pendingPackets.Enqueue(packetData);
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00003CC4 File Offset: 0x00001EC4
		private void checkConnectionTimeout(object _sender, ElapsedEventArgs _args)
		{
			bool flag = !this.Connected || this.tcpConnection == null;
			if (!flag)
			{
				float num = this.tcpConnection.ConnectionTime - this.tcpConnection.LastMessageSentTime;
				bool flag2 = num > (float)this.ConnectionTimeout;
				if (flag2)
				{
					this.CloseConnection();
				}
				else
				{
					bool flag3 = num > 5f && !this.tcpConnection.PacketSendInProgress;
					if (flag3)
					{
						DataBuffer dataBuffer = new DataBuffer(1000);
						dataBuffer.WriteInteger(0);
						this.SendNetworkMessage(dataBuffer.ToArray());
					}
				}
			}
		}

		// Token: 0x0600008A RID: 138 RVA: 0x00003D60 File Offset: 0x00001F60
		public void SendNetworkMessage(byte[] _message)
		{
			bool flag = !this.Connected;
			if (!flag)
			{
				DataBuffer dataBuffer = new DataBuffer(1000);
				dataBuffer.WriteInteger(_message.Length);
				dataBuffer.WriteBytes(_message);
				this.tcpConnection.QueuePacket(dataBuffer);
			}
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00003DA8 File Offset: 0x00001FA8
		public void ShutdownConnection()
		{
			bool flag = this.tcpConnection == null;
			if (!flag)
			{
				this.tcpConnection.ShutdownConnection();
			}
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00003DD4 File Offset: 0x00001FD4
		public void CloseConnection()
		{
			bool flag = this.tcpConnection == null;
			if (!flag)
			{
				this.tcpConnection.CloseConnection();
			}
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00003E00 File Offset: 0x00002000
		public void TryReconnect()
		{
			bool busy = this.Busy;
			if (!busy)
			{
				bool autoReconnect = this.AutoReconnect;
				if (autoReconnect)
				{
					this.RestartConnection();
				}
			}
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00003E30 File Offset: 0x00002030
		public void RestartConnectionEvent(object _sender, ElapsedEventArgs e, Timer _eventTimer)
		{
			bool flag = !this.Busy;
			if (flag)
			{
				this.StartConnection(this.serverIp, this.serverPort);
			}
			_eventTimer.Enabled = false;
			_eventTimer.Stop();
			_eventTimer.Dispose();
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00003E74 File Offset: 0x00002074
		public void RestartConnection()
		{
			bool busy = this.Busy;
			if (!busy)
			{
				Timer _reconnectionTimer = new Timer((double)(this.ReconnectionInterval * 1000));
				_reconnectionTimer.Elapsed += delegate(object _sender, ElapsedEventArgs e)
				{
					this.RestartConnectionEvent(_sender, e, _reconnectionTimer);
				};
				_reconnectionTimer.AutoReset = false;
				_reconnectionTimer.Enabled = true;
				GC.KeepAlive(_reconnectionTimer);
				_reconnectionTimer.Start();
			}
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00003EFD File Offset: 0x000020FD
		public void OnDestroy()
		{
			this.Busy = false;
			this.CloseConnection();
		}

		// Token: 0x04000031 RID: 49
		public string LogFile;

		// Token: 0x04000032 RID: 50
		public int ConnectionTimeout = 10;

		// Token: 0x04000033 RID: 51
		public int ReconnectionInterval = 3;

		// Token: 0x04000034 RID: 52
		private Queue<ClientTCP.ConnEvent> connectionEvents = new Queue<ClientTCP.ConnEvent>(100);

		// Token: 0x04000038 RID: 56
		private ConcurrentQueue<ClientTCP.PacketData> pendingPackets = new ConcurrentQueue<ClientTCP.PacketData>();

		// Token: 0x0400003D RID: 61
		private TCPConnection tcpConnection;

		// Token: 0x0400003E RID: 62
		private string serverIp;

		// Token: 0x0400003F RID: 63
		private int serverPort;

		// Token: 0x04000040 RID: 64
		private Socket connectionSocket;

		// Token: 0x04000041 RID: 65
		private Stopwatch connectionStopwatch = new Stopwatch();

		// Token: 0x04000042 RID: 66
		private Timer connectingTimeoutTimer;

		// Token: 0x04000043 RID: 67
		private Timer connectionTimeoutTimer;

		// Token: 0x0200002D RID: 45
		// (Invoke) Token: 0x06000150 RID: 336
		public delegate void ConnectionEvent();

		// Token: 0x0200002E RID: 46
		private class ConnEvent
		{
			// Token: 0x040000BA RID: 186
			public TCPConnection Connection;

			// Token: 0x040000BB RID: 187
			public int ConnectionType;
		}

		// Token: 0x0200002F RID: 47
		// (Invoke) Token: 0x06000155 RID: 341
		public delegate void PacketEvent(DataBuffer _packet);

		// Token: 0x02000030 RID: 48
		private class PacketData
		{
			// Token: 0x040000BC RID: 188
			public TCPConnection Connection;

			// Token: 0x040000BD RID: 189
			public DataBuffer PacketBuffer;
		}
	}
}
