using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Analytics;

namespace MatchmakingClientSDK
{
	// Token: 0x02000057 RID: 87
	public class MatchmakingWebsocketClient
	{
		// Token: 0x14000024 RID: 36
		// (add) Token: 0x06000342 RID: 834 RVA: 0x0000EB04 File Offset: 0x0000CD04
		// (remove) Token: 0x06000343 RID: 835 RVA: 0x0000EB3C File Offset: 0x0000CD3C
		public event Action OnConnectedToServer;

		// Token: 0x14000025 RID: 37
		// (add) Token: 0x06000344 RID: 836 RVA: 0x0000EB74 File Offset: 0x0000CD74
		// (remove) Token: 0x06000345 RID: 837 RVA: 0x0000EBAC File Offset: 0x0000CDAC
		public event Action OnConnectionToServerFailed;

		// Token: 0x14000026 RID: 38
		// (add) Token: 0x06000346 RID: 838 RVA: 0x0000EBE4 File Offset: 0x0000CDE4
		// (remove) Token: 0x06000347 RID: 839 RVA: 0x0000EC1C File Offset: 0x0000CE1C
		public event Action OnDisconnectedFromServer;

		// Token: 0x14000027 RID: 39
		// (add) Token: 0x06000348 RID: 840 RVA: 0x0000EC54 File Offset: 0x0000CE54
		// (remove) Token: 0x06000349 RID: 841 RVA: 0x0000EC8C File Offset: 0x0000CE8C
		internal event Action<Document> OnAuthentication;

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x0600034A RID: 842 RVA: 0x0000ECC1 File Offset: 0x0000CEC1
		public bool IsConnected
		{
			get
			{
				return this.clientWebSocket != null;
			}
		}

		// Token: 0x0600034B RID: 843 RVA: 0x0000ECCC File Offset: 0x0000CECC
		public MatchmakingWebsocketClient(string stage, Action<string, bool> log, Func<float> time, Dictionary<string, string> endpoints)
		{
			this.Stage = stage;
			this.Log = log;
			this.Time = time;
			this.Endpoints = new ReadOnlyDictionary<string, string>(endpoints);
		}

		// Token: 0x0600034C RID: 844 RVA: 0x0000ED4D File Offset: 0x0000CF4D
		public void AddServiceHandler(string service, Action<Document> handler)
		{
			this.serviceHandlers[service] = handler;
		}

		// Token: 0x0600034D RID: 845 RVA: 0x0000ED5C File Offset: 0x0000CF5C
		public void RemoveServiceHandler(string service)
		{
			this.serviceHandlers.Remove(service);
		}

		// Token: 0x0600034E RID: 846 RVA: 0x0000ED6C File Offset: 0x0000CF6C
		private async Task<string> GetPublicIp()
		{
			string text;
			try
			{
				using (HttpClient client = new HttpClient())
				{
					text = await (await client.GetAsync("https://api.ipify.org")).Content.ReadAsStringAsync();
				}
			}
			catch (Exception ex)
			{
				this.Log(string.Format("Failed to get public ip! {0}", ex), true);
				text = null;
			}
			return text;
		}

		// Token: 0x0600034F RID: 847 RVA: 0x0000EDB0 File Offset: 0x0000CFB0
		public async void Connect(string authToken, string platform)
		{
			this.Disconnect();
			int reconnectionAttempts = 0;
			CancellationTokenSource cancellationSource = new CancellationTokenSource();
			this.connectionCancellationTokenSource = cancellationSource;
			object obj;
			Exception ex;
			for (;;)
			{
				ClientWebSocket webSocket = new ClientWebSocket();
				int num = 0;
				try
				{
					webSocket.Options.SetRequestHeader("Authorization", authToken);
					webSocket.Options.SetRequestHeader("Platform", platform);
					webSocket.Options.SetRequestHeader("Project", "Endstar");
					string text = (string.IsNullOrWhiteSpace(MatchmakingVersion.Sha) ? "0" : MatchmakingVersion.Sha);
					webSocket.Options.SetRequestHeader("Version", text);
					string text2 = await this.GetPublicIp();
					if (text2 == null)
					{
						text2 = "0.0.0.0";
					}
					webSocket.Options.SetRequestHeader("Endpoint", text2);
					await webSocket.ConnectAsync(new Uri(this.Endpoints[this.Stage]), cancellationSource.Token);
					this.clientWebSocket = webSocket;
					this.Log("Connected to server!", false);
					this.lastMessageTime = this.Time();
					Action onConnectedToServer = this.OnConnectedToServer;
					if (onConnectedToServer != null)
					{
						onConnectedToServer();
					}
					this.StartReceiving();
					this.StartSending();
				}
				catch (Exception obj)
				{
					num = 1;
				}
				if (num != 1)
				{
					goto IL_035B;
				}
				ex = (Exception)obj;
				if (cancellationSource.IsCancellationRequested)
				{
					break;
				}
				reconnectionAttempts++;
				if (reconnectionAttempts >= 3)
				{
					goto Block_4;
				}
				this.Log(string.Format("Connection to server failed! Retrying [{0}]. time... {1}", reconnectionAttempts, ex), true);
				try
				{
					await Task.Delay(3000, cancellationSource.Token);
					continue;
				}
				catch
				{
					break;
				}
				goto IL_035B;
			}
			return;
			Block_4:
			this.Log(string.Format("Connection to server failed after [{0}] tries! {1}", reconnectionAttempts, ex), true);
			Action onConnectionToServerFailed = this.OnConnectionToServerFailed;
			if (onConnectionToServerFailed != null)
			{
				onConnectionToServerFailed();
			}
			return;
			IL_035B:
			obj = null;
		}

		// Token: 0x06000350 RID: 848 RVA: 0x0000EDF8 File Offset: 0x0000CFF8
		public void Update()
		{
			while (this.receiveQueue.Count > 0)
			{
				Document document = this.receiveQueue.Dequeue();
				this.receivedMessagesCount++;
				this.Log("Message received: " + document.ToJsonPretty(), false);
				DynamoDBEntry dynamoDBEntry;
				DynamoDBEntry dynamoDBEntry2;
				if (this.receivedMessagesCount == 1)
				{
					Action<Document> onAuthentication = this.OnAuthentication;
					if (onAuthentication != null)
					{
						onAuthentication(document);
					}
				}
				else if (document.TryGetValue("requestId", out dynamoDBEntry))
				{
					string text = dynamoDBEntry.AsString();
					Action<Document> action;
					if (this.SendingCallbacks.TryGetValue(text, out action))
					{
						this.SendingCallbacks.Remove(text);
						if (action != null)
						{
							action(document);
						}
					}
					else
					{
						this.Log("Received response for unknown request! " + document.ToJsonPretty(), true);
					}
				}
				else if (document.TryGetValue("action", out dynamoDBEntry2))
				{
					string text2 = dynamoDBEntry2.AsString();
					Action<Document> action2;
					if (this.serviceHandlers.TryGetValue(text2, out action2))
					{
						if (action2 != null)
						{
							action2(document);
						}
					}
					else
					{
						this.Log("Received unknown message! " + document.ToJsonPretty(), true);
					}
				}
				else
				{
					this.Log("Received message without service label! " + document.ToJsonPretty(), true);
				}
			}
			if (this.clientWebSocket != null && this.Time() - this.lastMessageTime > 60f)
			{
				this.SendQueue.Enqueue(null);
				this.lastMessageTime = this.Time();
			}
		}

		// Token: 0x06000351 RID: 849 RVA: 0x0000EF88 File Offset: 0x0000D188
		private async void StartReceiving()
		{
			if (this.receiveCancellationTokenSource != null)
			{
				throw new InvalidOperationException("Previous receive operation has not been disposed of!");
			}
			CancellationTokenSource cancellationSource = new CancellationTokenSource();
			this.receiveCancellationTokenSource = cancellationSource;
			int receiveOffset = 0;
			for (;;)
			{
				try
				{
					WebSocketReceiveResult webSocketReceiveResult = await this.clientWebSocket.ReceiveAsync(new ArraySegment<byte>(this.receiveBuffer, receiveOffset, this.receiveBuffer.Length - receiveOffset), cancellationSource.Token);
					if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
					{
						this.Disconnect();
						break;
					}
					receiveOffset += webSocketReceiveResult.Count;
					if (webSocketReceiveResult.EndOfMessage)
					{
						this.OnMessageReceived(Encoding.UTF8.GetString(this.receiveBuffer, 0, receiveOffset));
						receiveOffset = 0;
					}
					else if (receiveOffset >= this.receiveBuffer.Length)
					{
						this.Log(string.Format("Received message is too large! Receive buffer only accepts messages up to [{0}KB].", this.receiveBuffer.Length / 1024), true);
						this.OnMessageReceived(Encoding.UTF8.GetString(this.receiveBuffer, 0, receiveOffset));
						receiveOffset = 0;
					}
				}
				catch (Exception ex)
				{
					if (cancellationSource.IsCancellationRequested)
					{
						break;
					}
					this.Log(string.Format("Receive failed! {0}", ex), true);
					this.Disconnect();
					break;
				}
				await Task.Yield();
			}
		}

		// Token: 0x06000352 RID: 850 RVA: 0x0000EFC0 File Offset: 0x0000D1C0
		internal void OnMessageReceived(string json)
		{
			Document document = Document.FromJson(json);
			this.receiveQueue.Enqueue(document);
		}

		// Token: 0x06000353 RID: 851 RVA: 0x0000EFE0 File Offset: 0x0000D1E0
		private void SendNetworkMessage(Document document)
		{
			if (document.Count == 0)
			{
				throw new Exception("Cannot send empty message!");
			}
			this.SendQueue.Enqueue(document.ToJsonPretty());
		}

		// Token: 0x06000354 RID: 852 RVA: 0x0000F008 File Offset: 0x0000D208
		internal void SendRequest(Document document, Action<Document> responseCallback)
		{
			string text = Guid.NewGuid().ToString();
			this.SendingCallbacks[text] = responseCallback;
			document["requestId"] = text;
			this.SendNetworkMessage(document);
		}

		// Token: 0x06000355 RID: 853 RVA: 0x0000F050 File Offset: 0x0000D250
		private async void StartSending()
		{
			if (this.sendCancellationTokenSource != null)
			{
				throw new InvalidOperationException("Previous send operation has not been disposed of!");
			}
			CancellationTokenSource cancellationSource = new CancellationTokenSource();
			this.sendCancellationTokenSource = cancellationSource;
			for (;;)
			{
				try
				{
					if (cancellationSource.IsCancellationRequested)
					{
						break;
					}
					if (this.SendQueue.Count > 0)
					{
						string jsonPacket = this.SendQueue.Dequeue();
						if (jsonPacket != null && jsonPacket.Length > this.sendBuffer.Length)
						{
							this.Log(string.Format("Packet is too large and will be truncated! Send buffer only accepts messages up to [{0}KB].", this.sendBuffer.Length / 1024), true);
						}
						this.lastMessageTime = this.Time();
						int num = ((jsonPacket != null) ? Encoding.UTF8.GetBytes(jsonPacket, 0, MatchmakingWebsocketClient.<StartSending>g__Clamp|42_0(jsonPacket.Length, 0, this.sendBuffer.Length), this.sendBuffer, 0) : 0);
						await this.clientWebSocket.SendAsync(new ArraySegment<byte>(this.sendBuffer, 0, num), WebSocketMessageType.Text, true, cancellationSource.Token);
						if (jsonPacket != null)
						{
							this.Log("Sent a packet: " + jsonPacket, false);
						}
						jsonPacket = null;
					}
				}
				catch (Exception ex)
				{
					if (cancellationSource.IsCancellationRequested)
					{
						break;
					}
					this.Log(string.Format("Send failed! {0}", ex), true);
					this.Disconnect();
					break;
				}
				await Task.Yield();
			}
		}

		// Token: 0x06000356 RID: 854 RVA: 0x0000F087 File Offset: 0x0000D287
		public void Disconnect()
		{
			this.Dispose();
		}

		// Token: 0x06000357 RID: 855 RVA: 0x0000F090 File Offset: 0x0000D290
		private void Dispose()
		{
			if (this.clientWebSocket != null)
			{
				try
				{
					ClientWebSocket clientWebSocket = this.clientWebSocket;
					if (clientWebSocket != null)
					{
						clientWebSocket.Dispose();
					}
				}
				catch
				{
				}
				this.clientWebSocket = null;
				Action onDisconnectedFromServer = this.OnDisconnectedFromServer;
				if (onDisconnectedFromServer != null)
				{
					onDisconnectedFromServer();
				}
			}
			this.SendingCallbacks.Clear();
			try
			{
				CancellationTokenSource cancellationTokenSource = this.connectionCancellationTokenSource;
				if (cancellationTokenSource != null)
				{
					cancellationTokenSource.Cancel();
				}
			}
			catch
			{
			}
			this.connectionCancellationTokenSource = null;
			try
			{
				CancellationTokenSource cancellationTokenSource2 = this.receiveCancellationTokenSource;
				if (cancellationTokenSource2 != null)
				{
					cancellationTokenSource2.Cancel();
				}
			}
			catch
			{
			}
			this.receiveCancellationTokenSource = null;
			try
			{
				CancellationTokenSource cancellationTokenSource3 = this.sendCancellationTokenSource;
				if (cancellationTokenSource3 != null)
				{
					cancellationTokenSource3.Cancel();
				}
			}
			catch
			{
			}
			this.sendCancellationTokenSource = null;
			this.SendQueue.Clear();
			this.receiveQueue.Clear();
			this.receivedMessagesCount = 0;
		}

		// Token: 0x06000358 RID: 856 RVA: 0x0000F188 File Offset: 0x0000D388
		[CompilerGenerated]
		internal static int <StartSending>g__Clamp|42_0(int value, int min, int max)
		{
			if (value < min)
			{
				value = min;
			}
			else if (value > max)
			{
				value = max;
			}
			return value;
		}

		// Token: 0x040001DA RID: 474
		private const float HEARTBEAT_TRIGGER_TIME = 60f;

		// Token: 0x040001DB RID: 475
		private const int MAX_MESSAGE_SIZE_IN_KB = 64;

		// Token: 0x040001DC RID: 476
		public readonly string Stage;

		// Token: 0x040001DD RID: 477
		public readonly Action<string, bool> Log;

		// Token: 0x040001DE RID: 478
		public readonly Func<float> Time;

		// Token: 0x040001DF RID: 479
		public readonly ReadOnlyDictionary<string, string> Endpoints;

		// Token: 0x040001E4 RID: 484
		private readonly Dictionary<string, Action<Document>> serviceHandlers = new Dictionary<string, Action<Document>>();

		// Token: 0x040001E5 RID: 485
		private readonly Queue<Document> receiveQueue = new Queue<Document>();

		// Token: 0x040001E6 RID: 486
		private ClientWebSocket clientWebSocket;

		// Token: 0x040001E7 RID: 487
		private CancellationTokenSource connectionCancellationTokenSource;

		// Token: 0x040001E8 RID: 488
		private CancellationTokenSource receiveCancellationTokenSource;

		// Token: 0x040001E9 RID: 489
		private byte[] receiveBuffer = new byte[65536];

		// Token: 0x040001EA RID: 490
		private int receivedMessagesCount;

		// Token: 0x040001EB RID: 491
		internal readonly Dictionary<string, Action<Document>> SendingCallbacks = new Dictionary<string, Action<Document>>();

		// Token: 0x040001EC RID: 492
		internal readonly Queue<string> SendQueue = new Queue<string>();

		// Token: 0x040001ED RID: 493
		private byte[] sendBuffer = new byte[65536];

		// Token: 0x040001EE RID: 494
		private CancellationTokenSource sendCancellationTokenSource;

		// Token: 0x040001EF RID: 495
		private float lastMessageTime;
	}
}
