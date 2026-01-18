using System;
using System.Collections.Generic;

namespace Endless.Networking.TCP
{
	// Token: 0x02000010 RID: 16
	public abstract class Client<PH> : IDisposable where PH : IClientTcpPacketHandler, new()
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000055 RID: 85
		public abstract string LogFileName { get; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000056 RID: 86
		public abstract string ServerIp { get; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000057 RID: 87
		public abstract int ServerPort { get; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000058 RID: 88
		public abstract int ConnectionTimeout { get; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000059 RID: 89 RVA: 0x000031EA File Offset: 0x000013EA
		// (set) Token: 0x0600005A RID: 90 RVA: 0x000031F2 File Offset: 0x000013F2
		public bool Initialized { get; private set; } = false;

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600005B RID: 91 RVA: 0x000031FB File Offset: 0x000013FB
		public bool IsConnected
		{
			get
			{
				return this.clientTCP.Connected;
			}
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00003208 File Offset: 0x00001408
		protected void Initialize(Dictionary<int, Func<DataBuffer, Message>> readCollection)
		{
			bool initialized = this.Initialized;
			if (!initialized)
			{
				this.messageHandlers = this.InitializeMessageHandlers();
				this.packetHandler = new PH();
				this.packetHandler.Initialize<PH>(this, readCollection, new Action<Message>(this.OnMessageReceived));
				this.clientTCP = new ClientTCP();
				this.clientTCP.OnConnectionEstablished += this.OnConnectionEstablished;
				this.clientTCP.OnConnectionTerminated += this.OnConnectionTerminated;
				this.clientTCP.OnConnectionFailed += this.OnConnectionFailed;
				this.clientTCP.OnPacketReceived += this.OnPacketReceived;
				this.clientTCP.LogFile = this.LogFileName;
				this.clientTCP.ConnectionTimeout = this.ConnectionTimeout;
				this.clientTCP.ReconnectionInterval = 3;
				this.Initialized = true;
				this.OnInitialized();
			}
		}

		// Token: 0x0600005D RID: 93
		protected abstract void OnInitialized();

		// Token: 0x0600005E RID: 94 RVA: 0x00003308 File Offset: 0x00001508
		public void HandleEvents()
		{
			this.clientTCP.HandleEvents();
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

		// Token: 0x0600005F RID: 95 RVA: 0x00003394 File Offset: 0x00001594
		public virtual void Connect()
		{
			bool busy = this.clientTCP.Busy;
			if (!busy)
			{
				this.clientTCP.AutoReconnect = true;
				this.clientTCP.StartConnection(this.ServerIp, this.ServerPort);
			}
		}

		// Token: 0x06000060 RID: 96 RVA: 0x000033D8 File Offset: 0x000015D8
		public virtual void Disconnect()
		{
			bool flag = !this.clientTCP.Busy;
			if (!flag)
			{
				this.clientTCP.AutoReconnect = false;
				this.clientTCP.ShutdownConnection();
			}
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003413 File Offset: 0x00001613
		public virtual void Reconnect()
		{
			this.clientTCP.TryReconnect();
		}

		// Token: 0x06000062 RID: 98
		protected abstract void OnConnectionEstablished();

		// Token: 0x06000063 RID: 99
		protected abstract void OnConnectionFailed();

		// Token: 0x06000064 RID: 100
		protected abstract void OnConnectionTerminated();

		// Token: 0x06000065 RID: 101
		protected abstract Dictionary<Type, Action<Message>> InitializeMessageHandlers();

		// Token: 0x06000066 RID: 102 RVA: 0x00003422 File Offset: 0x00001622
		private void OnPacketReceived(DataBuffer packetBuffer)
		{
			this.QueuePacket(packetBuffer);
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00003430 File Offset: 0x00001630
		private void QueuePacket(DataBuffer packetBuffer)
		{
			Queue<DataBuffer> queue = this.receivedPackets;
			lock (queue)
			{
				this.receivedPackets.Enqueue(packetBuffer);
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000068 RID: 104 RVA: 0x0000347C File Offset: 0x0000167C
		// (set) Token: 0x06000069 RID: 105 RVA: 0x00003484 File Offset: 0x00001684
		public int MessagesReceived { get; private set; } = 0;

		// Token: 0x0600006A RID: 106 RVA: 0x00003490 File Offset: 0x00001690
		private void OnMessageReceived(Message message)
		{
			Action<Message> action;
			bool flag = !this.messageHandlers.TryGetValue(message.GetType(), out action);
			if (!flag)
			{
				int messagesReceived = this.MessagesReceived;
				this.MessagesReceived = messagesReceived + 1;
				if (action != null)
				{
					action(message);
				}
			}
		}

		// Token: 0x0600006B RID: 107 RVA: 0x000034D8 File Offset: 0x000016D8
		public void SendNetworkMessage(int messageId, DataBuffer messageBuffer)
		{
			DataBuffer dataBuffer = new DataBuffer(1000);
			dataBuffer.WriteInteger(messageId);
			dataBuffer.WriteBytes(messageBuffer.ToArray());
			this.clientTCP.SendNetworkMessage(dataBuffer.ToArray());
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00003518 File Offset: 0x00001718
		public void Dispose()
		{
			ClientTCP clientTCP = this.clientTCP;
			if (clientTCP != null)
			{
				clientTCP.OnDestroy();
			}
		}

		// Token: 0x0400002B RID: 43
		private ClientTCP clientTCP;

		// Token: 0x0400002C RID: 44
		private readonly Queue<DataBuffer> receivedPackets = new Queue<DataBuffer>(5);

		// Token: 0x0400002D RID: 45
		private PH packetHandler;

		// Token: 0x0400002F RID: 47
		private Dictionary<Type, Action<Message>> messageHandlers;
	}
}
