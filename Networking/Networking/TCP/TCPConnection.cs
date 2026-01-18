using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;

namespace Endless.Networking.TCP
{
	// Token: 0x02000019 RID: 25
	public class TCPConnection
	{
		// Token: 0x060000C7 RID: 199 RVA: 0x00004CC0 File Offset: 0x00002EC0
		public TCPConnection()
		{
			this.ID = TCPConnection.NewID();
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060000C8 RID: 200 RVA: 0x00004D20 File Offset: 0x00002F20
		// (set) Token: 0x060000C9 RID: 201 RVA: 0x00004D28 File Offset: 0x00002F28
		public int ID { get; protected set; }

		// Token: 0x060000CA RID: 202 RVA: 0x00004D34 File Offset: 0x00002F34
		protected static int NewID()
		{
			return TCPConnection.IDs++;
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000CB RID: 203 RVA: 0x00004D53 File Offset: 0x00002F53
		public string Address
		{
			get
			{
				return this.connectionSocket.RemoteEndPoint.ToString();
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000CC RID: 204 RVA: 0x00004D65 File Offset: 0x00002F65
		// (set) Token: 0x060000CD RID: 205 RVA: 0x00004D6D File Offset: 0x00002F6D
		public bool Busy { get; protected set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000CE RID: 206 RVA: 0x00004D76 File Offset: 0x00002F76
		// (set) Token: 0x060000CF RID: 207 RVA: 0x00004D7E File Offset: 0x00002F7E
		public bool Connected { get; protected set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000D0 RID: 208 RVA: 0x00004D87 File Offset: 0x00002F87
		// (set) Token: 0x060000D1 RID: 209 RVA: 0x00004D8F File Offset: 0x00002F8F
		public float LastMessageReceivedTime { get; protected set; } = 0f;

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060000D2 RID: 210 RVA: 0x00004D98 File Offset: 0x00002F98
		// (set) Token: 0x060000D3 RID: 211 RVA: 0x00004DA0 File Offset: 0x00002FA0
		public float LastMessageSentTime { get; protected set; } = 0f;

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060000D4 RID: 212 RVA: 0x00004DAC File Offset: 0x00002FAC
		public float ConnectionTime
		{
			get
			{
				return (float)this.connectionStopwatch.Elapsed.TotalSeconds;
			}
		}

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x060000D5 RID: 213 RVA: 0x00004DD4 File Offset: 0x00002FD4
		// (remove) Token: 0x060000D6 RID: 214 RVA: 0x00004E0C File Offset: 0x0000300C
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event TCPConnection.ConnectionEvent OnConnectionEstablished;

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x060000D7 RID: 215 RVA: 0x00004E44 File Offset: 0x00003044
		// (remove) Token: 0x060000D8 RID: 216 RVA: 0x00004E7C File Offset: 0x0000307C
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event TCPConnection.ConnectionEvent OnConnectionTerminated;

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x060000D9 RID: 217 RVA: 0x00004EB4 File Offset: 0x000030B4
		// (remove) Token: 0x060000DA RID: 218 RVA: 0x00004EEC File Offset: 0x000030EC
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event TCPConnection.PacketEvent OnPacketReceived;

		// Token: 0x060000DB RID: 219 RVA: 0x00004F21 File Offset: 0x00003121
		public void StartConnection(Socket _connectionSocket)
		{
			this.Busy = true;
			this.connectionSocket = _connectionSocket;
			this.onConnectionEstablished();
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00004F3C File Offset: 0x0000313C
		protected void onConnectionEstablished()
		{
			this.Connected = true;
			this.LastMessageReceivedTime = 0f;
			this.LastMessageSentTime = 0f;
			this.connectionStopwatch.Reset();
			this.connectionStopwatch.Start();
			TCPConnection.ConnectionEvent onConnectionEstablished = this.OnConnectionEstablished;
			if (onConnectionEstablished != null)
			{
				onConnectionEstablished(this);
			}
			this.TryReceiveSize();
		}

		// Token: 0x060000DD RID: 221 RVA: 0x00004F9C File Offset: 0x0000319C
		protected void onConnectionTerminated()
		{
			this.Connected = false;
			bool flag = !this.Busy;
			if (!flag)
			{
				this.Busy = false;
				this.connectionStopwatch.Stop();
				TCPConnection.ConnectionEvent onConnectionTerminated = this.OnConnectionTerminated;
				if (onConnectionTerminated != null)
				{
					onConnectionTerminated(this);
				}
			}
		}

		// Token: 0x060000DE RID: 222 RVA: 0x00004FE8 File Offset: 0x000031E8
		protected void TryReceiveSize()
		{
			TCPConnection.ReceiveContainer _receiveContainer = new TCPConnection.ReceiveContainer
			{
				WorkSocket = this.connectionSocket,
				ReceiveBuffer = new byte[4],
				ReadPosition = 0
			};
			_receiveContainer.OnSuccess = delegate
			{
				this.TryReceiveMessage(_receiveContainer);
			};
			this.TryReceive(_receiveContainer);
		}

		// Token: 0x060000DF RID: 223 RVA: 0x00005054 File Offset: 0x00003254
		protected void TryReceiveMessage(TCPConnection.ReceiveContainer _sizeContainer)
		{
			int num = 0;
			try
			{
				num = DataBuffer.GetInt32(_sizeContainer.ReceiveBuffer, 0);
			}
			catch
			{
				this.ShutdownConnection();
				return;
			}
			bool flag = num <= 0 || num > this.MaxMessageSize;
			if (flag)
			{
				Logger.Log(this.LogFile, "Invalid message size! Shutting down connection...", true);
				this.ShutdownConnection();
			}
			else
			{
				TCPConnection.ReceiveContainer _receiveContainer = new TCPConnection.ReceiveContainer
				{
					WorkSocket = _sizeContainer.WorkSocket,
					ReceiveBuffer = new byte[num],
					ReadPosition = 0
				};
				_receiveContainer.OnSuccess = delegate
				{
					this.LastMessageReceivedTime = this.ConnectionTime;
					this.OnPacketReceived(this, DataBuffer.FromBytes(_receiveContainer.ReceiveBuffer.ToArray<byte>()));
					this.TryReceiveSize();
				};
				this.TryReceive(_receiveContainer);
			}
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x00005120 File Offset: 0x00003320
		protected void TryReceive(TCPConnection.ReceiveContainer _receiveContainer)
		{
			try
			{
				this.connectionSocket.BeginReceive(_receiveContainer.ReceiveBuffer, _receiveContainer.ReadPosition, _receiveContainer.ReceiveBuffer.Length - _receiveContainer.ReadPosition, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), _receiveContainer);
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), true);
				this.ShutdownConnection();
				this.CloseConnection();
			}
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x0000519C File Offset: 0x0000339C
		protected void ReceiveCallback(IAsyncResult _a)
		{
			int num = 0;
			TCPConnection.ReceiveContainer receiveContainer = (TCPConnection.ReceiveContainer)_a.AsyncState;
			try
			{
				num = receiveContainer.WorkSocket.EndReceive(_a);
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), false);
				this.ShutdownConnection();
				this.CloseConnection();
				return;
			}
			_a.AsyncWaitHandle.Close();
			bool flag = num <= 0;
			if (flag)
			{
				this.ShutdownConnection();
				this.CloseConnection();
			}
			else
			{
				receiveContainer.ReadPosition += num;
				bool full = receiveContainer.Full;
				if (full)
				{
					receiveContainer.OnSuccess();
				}
				else
				{
					this.TryReceive(receiveContainer);
				}
			}
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x0000525C File Offset: 0x0000345C
		public void QueuePacket(DataBuffer _packet)
		{
			bool flag = !this.Busy;
			if (!flag)
			{
				Queue<DataBuffer> queue = this.pendingPackets;
				lock (queue)
				{
					bool flag3 = this.pendingPackets.Count >= 5;
					if (flag3)
					{
						this.ShutdownConnection();
						return;
					}
					this.pendingPackets.Enqueue(_packet);
				}
				bool flag4 = !this.PacketSendInProgress;
				if (flag4)
				{
					this.SendNextPacket();
				}
			}
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x000052F0 File Offset: 0x000034F0
		public int GetPendingPackets()
		{
			return this.PacketSendInProgress ? (1 + this.pendingPackets.Count) : this.pendingPackets.Count;
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x00005324 File Offset: 0x00003524
		// (set) Token: 0x060000E5 RID: 229 RVA: 0x0000532C File Offset: 0x0000352C
		public bool PacketSendInProgress { get; protected set; } = false;

		// Token: 0x060000E6 RID: 230 RVA: 0x00005338 File Offset: 0x00003538
		protected void SendNextPacket()
		{
			Queue<DataBuffer> queue = this.pendingPackets;
			DataBuffer dataBuffer;
			lock (queue)
			{
				bool flag2 = this.pendingPackets.Count <= 0;
				if (flag2)
				{
					this.PacketSendInProgress = false;
					return;
				}
				this.PacketSendInProgress = true;
				try
				{
					dataBuffer = this.pendingPackets.Dequeue();
				}
				catch (Exception ex)
				{
					Logger.Log(this.LogFile, ex.ToString(), false);
					this.PacketSendInProgress = false;
					return;
				}
			}
			dataBuffer.Sent = 0;
			this.trySend(dataBuffer);
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x000053F0 File Offset: 0x000035F0
		protected void trySend(DataBuffer _packet)
		{
			try
			{
				this.connectionSocket.BeginSend(_packet.ToArray(), _packet.GetReadPosition() + _packet.Sent, _packet.Length() - _packet.Sent, SocketFlags.None, new AsyncCallback(this.sendCallback), _packet);
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), true);
				this.ShutdownConnection();
			}
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x0000546C File Offset: 0x0000366C
		protected void sendCallback(IAsyncResult _asyncResult)
		{
			int num = 0;
			try
			{
				num = this.connectionSocket.EndSend(_asyncResult);
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), true);
				this.ShutdownConnection();
				return;
			}
			_asyncResult.AsyncWaitHandle.Close();
			DataBuffer dataBuffer = (DataBuffer)_asyncResult.AsyncState;
			bool flag = num + dataBuffer.Sent >= dataBuffer.Length();
			if (flag)
			{
				this.LastMessageSentTime = this.ConnectionTime;
				dataBuffer.Dispose();
				this.SendNextPacket();
			}
			else
			{
				dataBuffer.Sent += num;
				this.trySend(dataBuffer);
			}
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x00005524 File Offset: 0x00003724
		public void ShutdownConnection()
		{
			this.stopReceiving();
			this.stopSending();
		}

		// Token: 0x060000EA RID: 234 RVA: 0x00005535 File Offset: 0x00003735
		public void CloseConnection()
		{
			this.closeSocket();
			this.onConnectionTerminated();
		}

		// Token: 0x060000EB RID: 235 RVA: 0x00005548 File Offset: 0x00003748
		protected void stopReceiving()
		{
			try
			{
				this.connectionSocket.Shutdown(SocketShutdown.Receive);
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), false);
			}
		}

		// Token: 0x060000EC RID: 236 RVA: 0x00005590 File Offset: 0x00003790
		protected void stopSending()
		{
			try
			{
				this.connectionSocket.Shutdown(SocketShutdown.Send);
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), false);
			}
		}

		// Token: 0x060000ED RID: 237 RVA: 0x000055D8 File Offset: 0x000037D8
		protected void closeSocket()
		{
			try
			{
				this.connectionSocket.Close();
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFile, ex.ToString(), false);
			}
		}

		// Token: 0x04000061 RID: 97
		protected static int IDs = 1;

		// Token: 0x04000062 RID: 98
		public string LogFile;

		// Token: 0x04000063 RID: 99
		public int MaxMessageSize = 10000;

		// Token: 0x04000068 RID: 104
		protected Stopwatch connectionStopwatch = new Stopwatch();

		// Token: 0x04000069 RID: 105
		protected Socket connectionSocket;

		// Token: 0x0400006D RID: 109
		protected Queue<DataBuffer> pendingPackets = new Queue<DataBuffer>(5);

		// Token: 0x02000036 RID: 54
		protected class ReceiveContainer
		{
			// Token: 0x1700003C RID: 60
			// (get) Token: 0x06000165 RID: 357 RVA: 0x00006EF8 File Offset: 0x000050F8
			public bool Full
			{
				get
				{
					return this.ReadPosition >= this.ReceiveBuffer.Length;
				}
			}

			// Token: 0x040000C4 RID: 196
			public Socket WorkSocket;

			// Token: 0x040000C5 RID: 197
			public byte[] ReceiveBuffer;

			// Token: 0x040000C6 RID: 198
			public int ReadPosition;

			// Token: 0x040000C7 RID: 199
			public Action OnSuccess;
		}

		// Token: 0x02000037 RID: 55
		// (Invoke) Token: 0x06000168 RID: 360
		public delegate void ConnectionEvent(TCPConnection _connection);

		// Token: 0x02000038 RID: 56
		// (Invoke) Token: 0x0600016C RID: 364
		public delegate void PacketEvent(TCPConnection _connection, DataBuffer _packet);
	}
}
