using System;
using System.Collections.Generic;

namespace Endless.Networking.TCP.Examples
{
	// Token: 0x0200001A RID: 26
	public class ClientExample : Client<ClientTcpPacketHandler>
	{
		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060000EF RID: 239 RVA: 0x00005628 File Offset: 0x00003828
		public override string LogFileName
		{
			get
			{
				return ExampleConfig.LogFileName;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000F0 RID: 240 RVA: 0x0000562F File Offset: 0x0000382F
		public override string ServerIp
		{
			get
			{
				return ExampleConfig.ServerIp;
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000F1 RID: 241 RVA: 0x00005636 File Offset: 0x00003836
		public override int ServerPort
		{
			get
			{
				return ExampleConfig.ServerPort;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000F2 RID: 242 RVA: 0x0000563D File Offset: 0x0000383D
		public override int ConnectionTimeout
		{
			get
			{
				return ExampleConfig.ConnectionTimeout;
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000F3 RID: 243 RVA: 0x00005644 File Offset: 0x00003844
		// (set) Token: 0x060000F4 RID: 244 RVA: 0x0000564C File Offset: 0x0000384C
		public bool Connected { get; private set; } = false;

		// Token: 0x060000F5 RID: 245 RVA: 0x00005655 File Offset: 0x00003855
		protected override void OnInitialized()
		{
			Logger.Log(this.LogFileName, "Client initialized.", true);
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x0000566A File Offset: 0x0000386A
		protected override void OnConnectionEstablished()
		{
			this.Connected = true;
			Logger.Log(this.LogFileName, "Connected to server.", true);
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x00005687 File Offset: 0x00003887
		protected override void OnConnectionFailed()
		{
			Logger.Log(this.LogFileName, "Connection to server failed!", true);
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x0000569C File Offset: 0x0000389C
		protected override void OnConnectionTerminated()
		{
			this.Connected = false;
			Logger.Log(this.LogFileName, "Disconnected from server.", true);
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x000056B9 File Offset: 0x000038B9
		private void OnChatMessageReceived(ChatMessage msg)
		{
			Logger.Log(this.LogFileName, "Chat message received: " + msg.Content, true);
		}

		// Token: 0x060000FA RID: 250 RVA: 0x000056DC File Offset: 0x000038DC
		public void SendChatMessage(string msg)
		{
			DataBuffer dataBuffer = new DataBuffer(1000);
			dataBuffer.WriteString(msg);
			base.SendNetworkMessage(1, dataBuffer);
			Logger.Log(this.LogFileName, "Chat message send: " + msg, true);
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00005720 File Offset: 0x00003920
		protected override Dictionary<Type, Action<Message>> InitializeMessageHandlers()
		{
			return new Dictionary<Type, Action<Message>> { 
			{
				typeof(ChatMessage),
				delegate(Message msg)
				{
					this.OnChatMessageReceived((ChatMessage)msg);
				}
			} };
		}
	}
}
