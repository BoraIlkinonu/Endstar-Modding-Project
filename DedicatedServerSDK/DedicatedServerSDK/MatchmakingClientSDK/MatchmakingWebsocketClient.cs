using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace MatchmakingClientSDK
{
	// Token: 0x02000006 RID: 6
	[NullableContext(1)]
	[Nullable(0)]
	public class MatchmakingWebsocketClient
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000005 RID: 5 RVA: 0x00002094 File Offset: 0x00000294
		// (remove) Token: 0x06000006 RID: 6 RVA: 0x000020CC File Offset: 0x000002CC
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnConnectedToServer;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000007 RID: 7 RVA: 0x00002104 File Offset: 0x00000304
		// (remove) Token: 0x06000008 RID: 8 RVA: 0x0000213C File Offset: 0x0000033C
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnConnectionToServerFailed;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000009 RID: 9 RVA: 0x00002174 File Offset: 0x00000374
		// (remove) Token: 0x0600000A RID: 10 RVA: 0x000021AC File Offset: 0x000003AC
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnDisconnectedFromServer;

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x0600000B RID: 11 RVA: 0x000021E4 File Offset: 0x000003E4
		// (remove) Token: 0x0600000C RID: 12 RVA: 0x0000221C File Offset: 0x0000041C
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal event Action<Document> OnAuthentication;

		// Token: 0x0600000D RID: 13 RVA: 0x00002254 File Offset: 0x00000454
		public MatchmakingWebsocketClient(string stage, Action<string, bool> log, Func<float> time)
		{
			this.Stage = stage;
			this.Log = log;
			this.Time = time;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000022BF File Offset: 0x000004BF
		public void AddServiceHandler(string service, Action<Document> handler)
		{
			this.serviceHandlers[service] = handler;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000022D0 File Offset: 0x000004D0
		public void RemoveServiceHandler(string service)
		{
			this.serviceHandlers.Remove(service);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000022E0 File Offset: 0x000004E0
		public async void Connect(string authToken, string platform)
		{
			this.Disconnect();
			int reconnectionAttempts = 0;
			CancellationTokenSource cancellationSource = new CancellationTokenSource();
			this.connectionCancellationTokenSource = cancellationSource;
			object obj;
			Exception e;
			for (;;)
			{
				ClientWebSocket webSocket = new ClientWebSocket();
				int num = 0;
				try
				{
					webSocket.Options.SetRequestHeader("Authorization", authToken);
					webSocket.Options.SetRequestHeader("Platform", platform);
					webSocket.Options.SetRequestHeader("Project", "Endstar");
					webSocket.Options.SetRequestHeader("Version", "eb70f606cec138900c77f613f9574bb637decfc4");
					await webSocket.ConnectAsync(new Uri(MatchmakingWebsocketClient.endpoints[this.Stage]), cancellationSource.Token);
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
					break;
				}
				e = (Exception)obj;
				if (cancellationSource.IsCancellationRequested)
				{
					goto Block_3;
				}
				reconnectionAttempts++;
				if (reconnectionAttempts >= 3)
				{
					goto Block_4;
				}
				this.Log(string.Format("Connection to server failed! Retrying [{0}]. time... {1}", reconnectionAttempts, e), true);
				try
				{
					await Task.Delay(3000, cancellationSource.Token);
				}
				catch
				{
					return;
				}
			}
			obj = null;
			Block_3:
			return;
			Block_4:
			Action onConnectionToServerFailed = this.OnConnectionToServerFailed;
			if (onConnectionToServerFailed != null)
			{
				onConnectionToServerFailed();
			}
			this.Log(string.Format("Connection to server failed after [{0}] tries! {1}", reconnectionAttempts, e), true);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002328 File Offset: 0x00000528
		public void Update()
		{
			bool flag = this.clientWebSocket != null && this.Time() - this.lastMessageTime > 60f;
			if (flag)
			{
				this.SendQueue.Enqueue(null);
				this.lastMessageTime = this.Time();
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002380 File Offset: 0x00000580
		private async void StartReceiving()
		{
			bool flag = this.receiveCancellationTokenSource != null;
			if (flag)
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
					WebSocketReceiveResult receiveResult = webSocketReceiveResult;
					webSocketReceiveResult = null;
					if (receiveResult.MessageType == WebSocketMessageType.Close)
					{
						this.Disconnect();
						break;
					}
					receiveOffset += receiveResult.Count;
					if (receiveResult.EndOfMessage)
					{
						string json = Encoding.UTF8.GetString(this.receiveBuffer, 0, receiveOffset);
						this.OnMessageReceived(json);
						receiveOffset = 0;
						json = null;
					}
					else if (receiveOffset == this.receiveBuffer.Length)
					{
						this.Log(string.Format("Received message is too large and will be truncated! Receive buffer only accepts messages up to [{0}KB].", this.receiveBuffer.Length / 1024), true);
						string json2 = Encoding.UTF8.GetString(this.receiveBuffer, 0, receiveOffset);
						this.OnMessageReceived(json2);
						receiveOffset = 0;
						json2 = null;
					}
					receiveResult = null;
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

		// Token: 0x06000013 RID: 19 RVA: 0x000023BC File Offset: 0x000005BC
		private void OnMessageReceived(string json)
		{
			this.receivedMessagesCount++;
			bool flag = this.receivedMessagesCount == 1;
			if (flag)
			{
				Document document = Document.FromJson(json);
				Action<Document> onAuthentication = this.OnAuthentication;
				if (onAuthentication != null)
				{
					onAuthentication(document);
				}
			}
			else
			{
				this.Log("Message received: " + json, false);
				Document document2 = Document.FromJson(json);
				DynamoDBEntry dynamoDBEntry;
				bool flag2 = document2.TryGetValue("requestId", out dynamoDBEntry);
				if (flag2)
				{
					string text = dynamoDBEntry.AsString();
					Action<Document> action;
					bool flag3 = this.SendingCallbacks.TryGetValue(text, out action);
					if (flag3)
					{
						this.SendingCallbacks.Remove(text);
						if (action != null)
						{
							action(document2);
						}
					}
					else
					{
						this.Log("Received response for unknown request! " + json, true);
					}
				}
				else
				{
					DynamoDBEntry dynamoDBEntry2;
					bool flag4 = document2.TryGetValue("action", out dynamoDBEntry2);
					if (flag4)
					{
						string text2 = dynamoDBEntry2.AsString();
						Action<Document> action2;
						bool flag5 = this.serviceHandlers.TryGetValue(text2, out action2);
						if (flag5)
						{
							if (action2 != null)
							{
								action2(document2);
							}
						}
						else
						{
							this.Log("Received unknown message! " + json, true);
						}
					}
					else
					{
						this.Log("Received message without service label! " + json, true);
					}
				}
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002514 File Offset: 0x00000714
		internal void SendNetworkMessage(Document document)
		{
			bool flag = this.clientWebSocket == null;
			if (flag)
			{
				throw new Exception("Cannot send message when not connected!");
			}
			bool flag2 = document.Count == 0;
			if (flag2)
			{
				throw new Exception("Cannot send empty message!");
			}
			this.SendQueue.Enqueue(document.ToJsonPretty());
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002568 File Offset: 0x00000768
		internal void SendRequest(Document document, Action<Document> responseCallback)
		{
			string text = Guid.NewGuid().ToString();
			this.SendingCallbacks[text] = responseCallback;
			document["requestId"] = text;
			this.SendNetworkMessage(document);
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000025B4 File Offset: 0x000007B4
		private async void StartSending()
		{
			bool flag = this.sendCancellationTokenSource != null;
			if (flag)
			{
				throw new InvalidOperationException("Previous send operation has not been disposed of!");
			}
			CancellationTokenSource cancellationSource = new CancellationTokenSource();
			this.sendCancellationTokenSource = cancellationSource;
			for (;;)
			{
				try
				{
					bool isCancellationRequested = cancellationSource.IsCancellationRequested;
					if (isCancellationRequested)
					{
						break;
					}
					bool flag2 = this.SendQueue.Count > 0;
					if (flag2)
					{
						string jsonPacket = this.SendQueue.Dequeue();
						bool flag3 = jsonPacket != null && jsonPacket.Length > this.sendBuffer.Length;
						if (flag3)
						{
							this.Log(string.Format("Packet is too large and will be truncated! Send buffer only accepts messages up to [{0}KB].", this.sendBuffer.Length / 1024), true);
						}
						this.lastMessageTime = this.Time();
						int sendBytes = ((jsonPacket != null) ? Encoding.UTF8.GetBytes(jsonPacket, 0, MatchmakingWebsocketClient.<StartSending>g__Clamp|38_0(jsonPacket.Length, 0, this.sendBuffer.Length), this.sendBuffer, 0) : 0);
						await this.clientWebSocket.SendAsync(new ArraySegment<byte>(this.sendBuffer, 0, sendBytes), WebSocketMessageType.Text, true, cancellationSource.Token);
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
					this.Log(string.Format("Receive failed! {0}", ex), true);
					this.Disconnect();
					break;
				}
				await Task.Yield();
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000025ED File Offset: 0x000007ED
		public void Disconnect()
		{
			this.Dispose();
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000025F8 File Offset: 0x000007F8
		private void Dispose()
		{
			bool flag = this.clientWebSocket != null;
			if (flag)
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
		}

		// Token: 0x0600001A RID: 26 RVA: 0x0000273C File Offset: 0x0000093C
		[CompilerGenerated]
		internal static int <StartSending>g__Clamp|38_0(int value, int min, int max)
		{
			bool flag = value < min;
			if (flag)
			{
				value = min;
			}
			else
			{
				bool flag2 = value > max;
				if (flag2)
				{
					value = max;
				}
			}
			return value;
		}

		// Token: 0x04000024 RID: 36
		private const float HEARTBEAT_TRIGGER_TIME = 60f;

		// Token: 0x04000025 RID: 37
		private const int MAX_MESSAGE_SIZE_IN_KB = 64;

		// Token: 0x04000026 RID: 38
		private static Dictionary<string, string> endpoints = new Dictionary<string, string>
		{
			{ "DEV", "ws://endstar-matchmaking-dev.endlessstudios.com:8080" },
			{ "STAGING", "ws://endstar-matchmaking-stage.endlessstudios.com:8080" },
			{ "PROD", "ws://endstar-matchmaking.endlessstudios.com:8080" }
		};

		// Token: 0x04000027 RID: 39
		public readonly string Stage;

		// Token: 0x04000028 RID: 40
		public readonly Action<string, bool> Log;

		// Token: 0x04000029 RID: 41
		public readonly Func<float> Time;

		// Token: 0x0400002E RID: 46
		private Dictionary<string, Action<Document>> serviceHandlers = new Dictionary<string, Action<Document>>();

		// Token: 0x0400002F RID: 47
		private ClientWebSocket clientWebSocket;

		// Token: 0x04000030 RID: 48
		private CancellationTokenSource connectionCancellationTokenSource;

		// Token: 0x04000031 RID: 49
		private CancellationTokenSource receiveCancellationTokenSource;

		// Token: 0x04000032 RID: 50
		private byte[] receiveBuffer = new byte[65536];

		// Token: 0x04000033 RID: 51
		private int receivedMessagesCount;

		// Token: 0x04000034 RID: 52
		internal readonly Dictionary<string, Action<Document>> SendingCallbacks = new Dictionary<string, Action<Document>>();

		// Token: 0x04000035 RID: 53
		internal readonly Queue<string> SendQueue = new Queue<string>();

		// Token: 0x04000036 RID: 54
		private byte[] sendBuffer = new byte[65536];

		// Token: 0x04000037 RID: 55
		private CancellationTokenSource sendCancellationTokenSource;

		// Token: 0x04000038 RID: 56
		private float lastMessageTime;
	}
}
