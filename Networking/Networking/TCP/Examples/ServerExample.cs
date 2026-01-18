using System;
using System.Collections.Generic;

namespace Endless.Networking.TCP.Examples
{
	// Token: 0x0200001E RID: 30
	public class ServerExample : Server<ServerTcpPacketHandler>
	{
		// Token: 0x1700002D RID: 45
		// (get) Token: 0x0600010E RID: 270 RVA: 0x00005857 File Offset: 0x00003A57
		public override string LogFileName
		{
			get
			{
				return ExampleConfig.LogFileName;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600010F RID: 271 RVA: 0x0000585E File Offset: 0x00003A5E
		public override int MaxConnections
		{
			get
			{
				return ExampleConfig.MaxConnections;
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000110 RID: 272 RVA: 0x00005865 File Offset: 0x00003A65
		public override int ConnectionTimeout
		{
			get
			{
				return ExampleConfig.ConnectionTimeout;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x06000111 RID: 273 RVA: 0x0000586C File Offset: 0x00003A6C
		public override int Port
		{
			get
			{
				return ExampleConfig.ServerPort;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x06000112 RID: 274 RVA: 0x00005873 File Offset: 0x00003A73
		public override int QueueSize
		{
			get
			{
				return ExampleConfig.QueueSize;
			}
		}

		// Token: 0x06000113 RID: 275 RVA: 0x0000587A File Offset: 0x00003A7A
		protected override void OnInitialized()
		{
			Logger.Log(this.LogFileName, "Server initialized", true);
		}

		// Token: 0x06000114 RID: 276 RVA: 0x0000588F File Offset: 0x00003A8F
		protected override void OnConnectionEstablished(int connectionId)
		{
			Logger.Log(this.LogFileName, string.Format("Client {0} connected", connectionId), true);
		}

		// Token: 0x06000115 RID: 277 RVA: 0x000058AF File Offset: 0x00003AAF
		protected override void OnConnectionTerminated(int connectionId)
		{
			Logger.Log(this.LogFileName, string.Format("Client {0} disconnected", connectionId), true);
		}

		// Token: 0x06000116 RID: 278 RVA: 0x000058CF File Offset: 0x00003ACF
		private void OnChatMessageReceived(int connectionId, ChatMessage msg)
		{
			Logger.Log(this.LogFileName, string.Format("Client {0} sends a chat message: {1}", connectionId, msg.Content), true);
		}

		// Token: 0x06000117 RID: 279 RVA: 0x000058F8 File Offset: 0x00003AF8
		public void SendChatMessage(int connectionId, string msg)
		{
			DataBuffer dataBuffer = new DataBuffer(1000);
			dataBuffer.WriteString(msg);
			base.SendNetworkMessage(connectionId, 1, dataBuffer);
			Logger.Log(this.LogFileName, "Chat message send: " + msg, true);
		}

		// Token: 0x06000118 RID: 280 RVA: 0x0000593C File Offset: 0x00003B3C
		protected override Dictionary<Type, Action<int, Message>> InitializeMessageHandlers()
		{
			return new Dictionary<Type, Action<int, Message>> { 
			{
				typeof(ChatMessage),
				delegate(int connectionId, Message message)
				{
					this.OnChatMessageReceived(connectionId, (ChatMessage)message);
				}
			} };
		}
	}
}
