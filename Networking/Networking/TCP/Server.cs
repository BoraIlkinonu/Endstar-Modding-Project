using System;
using System.Collections.Generic;

namespace Endless.Networking.TCP
{
	// Token: 0x02000016 RID: 22
	public abstract class Server<PH> where PH : IServerTcpPacketHandler, new()
	{
		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000097 RID: 151 RVA: 0x00003FDD File Offset: 0x000021DD
		// (set) Token: 0x06000098 RID: 152 RVA: 0x00003FE5 File Offset: 0x000021E5
		public bool Initialized { get; private set; } = false;

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000099 RID: 153
		public abstract string LogFileName { get; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600009A RID: 154
		public abstract int MaxConnections { get; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600009B RID: 155
		public abstract int ConnectionTimeout { get; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600009C RID: 156
		public abstract int Port { get; }

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600009D RID: 157
		public abstract int QueueSize { get; }

		// Token: 0x0600009E RID: 158 RVA: 0x00003FF0 File Offset: 0x000021F0
		public void Initialize(Dictionary<int, Func<DataBuffer, Message>> readCollection)
		{
			bool initialized = this.Initialized;
			if (!initialized)
			{
				this.packetHandler = new PH();
				this.packetHandler.Initialize<PH>(this, readCollection, new Action<int, Message>(this.OnMessageReceived));
				this.messageHandlers = this.InitializeMessageHandlers();
				this.serverTCP = new ServerTCP();
				this.serverTCP.OnConnectionEstablished += this.OnConnectionEstablished;
				this.serverTCP.OnConnectionTerminated += this.OnConnectionTerminated;
				this.serverTCP.OnPacketReceived += this.OnPacketReceived;
				this.serverTCP.LogFile = this.LogFileName;
				this.serverTCP.MaxIncomingConnections = this.MaxConnections;
				this.serverTCP.ConnectionTimeout = this.ConnectionTimeout;
				this.serverTCP.AcceptPort = this.Port;
				this.serverTCP.IncomingQueueSize = this.QueueSize;
				this.serverTCP.Initialize();
				this.Initialized = true;
				this.OnInitialized();
			}
		}

		// Token: 0x0600009F RID: 159 RVA: 0x0000410C File Offset: 0x0000230C
		protected void HandleEvents()
		{
			this.serverTCP.HandleEvents();
			Queue<DataBuffer> queue = this.receivedPackets;
			lock (queue)
			{
				while (this.receivedPackets.Count > 0)
				{
					DataBuffer dataBuffer = this.receivedPackets.Dequeue();
					bool flag2 = dataBuffer != null;
					if (flag2)
					{
						this.packetHandler.HandlePacket(dataBuffer);
					}
				}
			}
		}

		// Token: 0x060000A0 RID: 160
		protected abstract void OnInitialized();

		// Token: 0x060000A1 RID: 161
		protected abstract void OnConnectionEstablished(int connectionId);

		// Token: 0x060000A2 RID: 162
		protected abstract void OnConnectionTerminated(int connectionId);

		// Token: 0x060000A3 RID: 163 RVA: 0x00004198 File Offset: 0x00002398
		public void TerminateConnection(int connectionId)
		{
			this.serverTCP.TerminateConnection(connectionId);
		}

		// Token: 0x060000A4 RID: 164
		protected abstract Dictionary<Type, Action<int, Message>> InitializeMessageHandlers();

		// Token: 0x060000A5 RID: 165 RVA: 0x000041A8 File Offset: 0x000023A8
		private void OnPacketReceived(int connectionId, DataBuffer packetBuffer)
		{
			DataBuffer dataBuffer = new DataBuffer(1000);
			dataBuffer.WriteInteger(connectionId);
			dataBuffer.WriteBytes(packetBuffer.ToArray());
			this.QueuePacket(dataBuffer);
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x000041E0 File Offset: 0x000023E0
		private void QueuePacket(DataBuffer packetBuffer)
		{
			Queue<DataBuffer> queue = this.receivedPackets;
			lock (queue)
			{
				this.receivedPackets.Enqueue(packetBuffer);
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060000A7 RID: 167 RVA: 0x0000422C File Offset: 0x0000242C
		// (set) Token: 0x060000A8 RID: 168 RVA: 0x00004234 File Offset: 0x00002434
		private protected int MessagesReceived { protected get; private set; } = 0;

		// Token: 0x060000A9 RID: 169 RVA: 0x00004240 File Offset: 0x00002440
		private void OnMessageReceived(int connectionId, Message message)
		{
			Action<int, Message> action;
			bool flag = !this.messageHandlers.TryGetValue(message.GetType(), out action);
			if (!flag)
			{
				int messagesReceived = this.MessagesReceived;
				this.MessagesReceived = messagesReceived + 1;
				if (action != null)
				{
					action(connectionId, message);
				}
			}
		}

		// Token: 0x060000AA RID: 170 RVA: 0x0000428C File Offset: 0x0000248C
		public void SendNetworkMessage(int connectionId, int messageId, DataBuffer messageBuffer)
		{
			DataBuffer dataBuffer = new DataBuffer(1000);
			dataBuffer.WriteInteger(messageBuffer.Length() + 4);
			dataBuffer.WriteInteger(messageId);
			dataBuffer.WriteBytes(messageBuffer.ToArray());
			this.serverTCP.SendNetworkMessage(connectionId, dataBuffer);
		}

		// Token: 0x04000048 RID: 72
		private readonly Queue<DataBuffer> receivedPackets = new Queue<DataBuffer>(100);

		// Token: 0x04000049 RID: 73
		private ServerTCP serverTCP;

		// Token: 0x0400004A RID: 74
		private PH packetHandler;

		// Token: 0x0400004B RID: 75
		private Dictionary<Type, Action<int, Message>> messageHandlers;
	}
}
