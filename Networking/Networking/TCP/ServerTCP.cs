using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace Endless.Networking.TCP
{
	// Token: 0x02000017 RID: 23
	public class ServerTCP
	{
		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060000AC RID: 172 RVA: 0x000042FB File Offset: 0x000024FB
		// (set) Token: 0x060000AD RID: 173 RVA: 0x00004303 File Offset: 0x00002503
		public bool Initialized { get; protected set; } = false;

		// Token: 0x060000AE RID: 174 RVA: 0x0000430C File Offset: 0x0000250C
		public void Initialize()
		{
			bool initialized = this.Initialized;
			if (!initialized)
			{
				this.Initialized = true;
				this.initializeConnections();
			}
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00004338 File Offset: 0x00002538
		public void HandleEvents()
		{
			Queue<ServerTCP.ConnEvent> queue = this.connectionEvents;
			lock (queue)
			{
				while (this.connectionEvents.Count > 0)
				{
					ServerTCP.ConnEvent connEvent = this.connectionEvents.Dequeue();
					bool connected = connEvent.Connected;
					if (connected)
					{
						this.OnConnectionEstablished(connEvent.Connection.ID);
					}
					else
					{
						this.OnConnectionTerminated(connEvent.Connection.ID);
					}
				}
			}
			while (this.pendingPackets.Count > 0)
			{
				ServerTCP.PacketData packetData = null;
				this.pendingPackets.TryDequeue(out packetData);
				bool flag2 = packetData != null;
				if (flag2)
				{
					ServerTCP.PacketEvent onPacketReceived = this.OnPacketReceived;
					if (onPacketReceived != null)
					{
						onPacketReceived(packetData.Connection.ID, packetData.PacketBuffer);
					}
				}
			}
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00004434 File Offset: 0x00002634
		private void onConnectedEvent(TCPConnection _connection)
		{
			Queue<ServerTCP.ConnEvent> queue = this.connectionEvents;
			lock (queue)
			{
				this.connectionEvents.Enqueue(new ServerTCP.ConnEvent
				{
					Connection = _connection,
					Connected = true
				});
			}
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00004494 File Offset: 0x00002694
		private void onDisconnectedEvent(TCPConnection _connection)
		{
			Queue<ServerTCP.ConnEvent> queue = this.connectionEvents;
			lock (queue)
			{
				this.connectionEvents.Enqueue(new ServerTCP.ConnEvent
				{
					Connection = _connection,
					Connected = false
				});
			}
		}

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x060000B2 RID: 178 RVA: 0x000044F4 File Offset: 0x000026F4
		// (remove) Token: 0x060000B3 RID: 179 RVA: 0x0000452C File Offset: 0x0000272C
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ServerTCP.ConnectionEvent OnConnectionEstablished;

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x060000B4 RID: 180 RVA: 0x00004564 File Offset: 0x00002764
		// (remove) Token: 0x060000B5 RID: 181 RVA: 0x0000459C File Offset: 0x0000279C
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ServerTCP.ConnectionEvent OnConnectionTerminated;

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x060000B6 RID: 182 RVA: 0x000045D4 File Offset: 0x000027D4
		// (remove) Token: 0x060000B7 RID: 183 RVA: 0x0000460C File Offset: 0x0000280C
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ServerTCP.PacketEvent OnPacketReceived;

		// Token: 0x060000B8 RID: 184 RVA: 0x00004644 File Offset: 0x00002844
		private TCPConnection getConnectionFromID(int _id)
		{
			TCPConnection[] array = this.incomingConnections;
			TCPConnection tcpconnection2;
			lock (array)
			{
				foreach (TCPConnection tcpconnection in this.incomingConnections)
				{
					bool flag2 = tcpconnection.ID == _id;
					if (flag2)
					{
						return tcpconnection;
					}
				}
				tcpconnection2 = null;
			}
			return tcpconnection2;
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x000046BC File Offset: 0x000028BC
		private void initializeConnections()
		{
			this.incomingConnections = new TCPConnection[this.MaxIncomingConnections];
			for (int i = 0; i < this.incomingConnections.Length; i++)
			{
				this.incomingConnections[i] = new TCPConnection();
				this.incomingConnections[i].OnConnectionEstablished += this.onConnectionEstablished;
				this.incomingConnections[i].OnConnectionTerminated += this.onConnectionTerminated;
				this.incomingConnections[i].OnPacketReceived += this.onPacketReceived;
				this.incomingConnections[i].LogFile = this.LogFile;
			}
			this.ConnectionTimeoutTimer = new Timer(1000.0);
			this.ConnectionTimeoutTimer.Elapsed += this.connectionTimeoutChecker;
			this.ConnectionTimeoutTimer.AutoReset = true;
			this.ConnectionTimeoutTimer.Start();
			this.reacceptTimer = new Timer(3000.0);
			this.reacceptTimer.Elapsed += this.startConnectionListener;
			this.reacceptTimer.AutoReset = false;
			this.reacceptTimer.Enabled = false;
			this.startConnectionListener(null, null);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x000047FC File Offset: 0x000029FC
		private void connectionTimeoutChecker(object _sender, ElapsedEventArgs _args)
		{
			for (int i = 0; i < this.incomingConnections.Length; i++)
			{
				bool flag = this.incomingConnections[i].Connected && this.incomingConnections[i].ConnectionTime - this.incomingConnections[i].LastMessageReceivedTime > (float)this.ConnectionTimeout;
				if (flag)
				{
					this.incomingConnections[i].ShutdownConnection();
				}
			}
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00004870 File Offset: 0x00002A70
		private void startConnectionListener(object _sender = null, ElapsedEventArgs _args = null)
		{
			this.reacceptTimer.Stop();
			this.acceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				this.acceptSocket.Bind(new IPEndPoint(IPAddress.Any, this.AcceptPort));
				this.acceptSocket.Listen(this.IncomingQueueSize);
				this.startListeningForConnections();
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), true);
				this.reacceptTimer.Start();
			}
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00004904 File Offset: 0x00002B04
		private void startListeningForConnections()
		{
			try
			{
				this.acceptSocket.BeginAccept(new AsyncCallback(this.acceptClientConnectionCallback), null);
			}
			catch (Exception ex)
			{
				try
				{
					this.acceptSocket.Close();
				}
				catch
				{
				}
				Logger.Log(this.LogFile, ex.ToString(), true);
				this.reacceptTimer.Start();
			}
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00004984 File Offset: 0x00002B84
		private void acceptClientConnectionCallback(IAsyncResult ar)
		{
			Socket socket;
			try
			{
				socket = this.acceptSocket.EndAccept(ar);
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), true);
				try
				{
					this.acceptSocket.Close();
				}
				catch
				{
				}
				this.reacceptTimer.Start();
				return;
			}
			bool flag = false;
			bool flag2 = this.acceptConnections;
			if (flag2)
			{
				TCPConnection[] array = this.incomingConnections;
				lock (array)
				{
					for (int i = 0; i < this.incomingConnections.Length; i++)
					{
						bool flag4 = !this.incomingConnections[i].Busy;
						if (flag4)
						{
							this.incomingConnections[i].StartConnection(socket);
							flag = true;
							break;
						}
					}
				}
			}
			bool flag5 = !flag;
			if (flag5)
			{
				try
				{
					socket.Shutdown(SocketShutdown.Both);
					socket.Close();
				}
				catch (Exception ex2)
				{
					Logger.Log(this.LogFile, ex2.ToString(), false);
				}
				Logger.Log(this.LogFile, "Incoming connection rejected!", true);
			}
			this.startListeningForConnections();
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00004AE4 File Offset: 0x00002CE4
		private void onConnectionEstablished(TCPConnection _connection)
		{
			this.onConnectedEvent(_connection);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00004AEF File Offset: 0x00002CEF
		private void onConnectionTerminated(TCPConnection _connection)
		{
			this.onDisconnectedEvent(_connection);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00004AFA File Offset: 0x00002CFA
		private void onPacketReceived(TCPConnection _connection, DataBuffer _packet)
		{
			this.pendingPackets.Enqueue(new ServerTCP.PacketData
			{
				Connection = _connection,
				PacketBuffer = _packet
			});
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00004B1C File Offset: 0x00002D1C
		public void SendNetworkMessage(int _connectionID, DataBuffer _message)
		{
			TCPConnection connectionFromID = this.getConnectionFromID(_connectionID);
			bool flag = connectionFromID == null || !connectionFromID.Connected;
			if (!flag)
			{
				connectionFromID.QueuePacket(_message);
			}
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00004B50 File Offset: 0x00002D50
		public void TerminateConnection(int _connectionID)
		{
			bool flag = !this.Initialized;
			if (flag)
			{
				InvalidOperationException ex = new InvalidOperationException("ServerTCP needs to be initialized before calling this method.");
				Logger.Log(this.LogFile, ex.ToString(), true);
			}
			TCPConnection connectionFromID = this.getConnectionFromID(_connectionID);
			bool flag2 = connectionFromID != null;
			if (flag2)
			{
				connectionFromID.ShutdownConnection();
			}
		}

		// Token: 0x0400004D RID: 77
		public string LogFile;

		// Token: 0x0400004E RID: 78
		public int MaxIncomingConnections;

		// Token: 0x0400004F RID: 79
		public int ConnectionTimeout;

		// Token: 0x04000050 RID: 80
		public int AcceptPort;

		// Token: 0x04000051 RID: 81
		public int IncomingQueueSize;

		// Token: 0x04000053 RID: 83
		private Queue<ServerTCP.ConnEvent> connectionEvents = new Queue<ServerTCP.ConnEvent>(100);

		// Token: 0x04000056 RID: 86
		private ConcurrentQueue<ServerTCP.PacketData> pendingPackets = new ConcurrentQueue<ServerTCP.PacketData>();

		// Token: 0x04000058 RID: 88
		private Socket acceptSocket;

		// Token: 0x04000059 RID: 89
		private Timer reacceptTimer;

		// Token: 0x0400005A RID: 90
		private TCPConnection[] incomingConnections;

		// Token: 0x0400005B RID: 91
		private Timer ConnectionTimeoutTimer;

		// Token: 0x0400005C RID: 92
		private bool acceptConnections = true;

		// Token: 0x02000032 RID: 50
		// (Invoke) Token: 0x0600015C RID: 348
		public delegate void ConnectionEvent(int _connectionID);

		// Token: 0x02000033 RID: 51
		private class ConnEvent
		{
			// Token: 0x040000C0 RID: 192
			public TCPConnection Connection;

			// Token: 0x040000C1 RID: 193
			public bool Connected;
		}

		// Token: 0x02000034 RID: 52
		// (Invoke) Token: 0x06000161 RID: 353
		public delegate void PacketEvent(int _connectionID, DataBuffer _packet);

		// Token: 0x02000035 RID: 53
		private class PacketData
		{
			// Token: 0x040000C2 RID: 194
			public TCPConnection Connection;

			// Token: 0x040000C3 RID: 195
			public DataBuffer PacketBuffer;
		}
	}
}
