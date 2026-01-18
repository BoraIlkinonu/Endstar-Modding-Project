using System;
using System.Net;
using Networking.UDP;
using Networking.UDP.Utils;
using Relay;

namespace MatchmakingClientSDK.Udp
{
	// Token: 0x02000062 RID: 98
	public class UdpRelayClient : IDisposable
	{
		// Token: 0x060003C2 RID: 962 RVA: 0x00010898 File Offset: 0x0000EA98
		public UdpRelayClient(IPAddress relayIp, int port, int localListenerPort, ulong key, Action<string, bool> log = null)
		{
			this.RelayEndPoint = new IPEndPoint(relayIp, port);
			this.Port = port;
			this.Key = key;
			this.Log = log;
			this.listener = new SimpleUdp(false, 0, log);
			this.listener.OnDataReceived += this.OnListenReceive;
			this.listener.OnDataSent += this.OnListenSent;
			this.relayClient = new SimpleUdp(false, 0, log);
			this.relayClient.OnDataReceived += this.OnRelayClientReceive;
			this.relayClient.OnDataSent += this.OnRelayClientSent;
			this.listener.Server(localListenerPort);
			this.relayClient.Client();
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x00010960 File Offset: 0x0000EB60
		private void OnListenReceive(SimpleUdp udp, EndPoint sendingEndPoint, byte[] data, int count)
		{
			this.lastListenEndPoint = sendingEndPoint;
			RelayHeader relayHeader = new RelayHeader
			{
				MessageType = RelayMessageType.Data,
				Key = this.Key
			};
			NetDataWriter netDataWriter = NetDataWriterPool.GetNetDataWriter();
			netDataWriter.Reset();
			relayHeader.Serialize(netDataWriter);
			netDataWriter.Put(data, 0, count);
			this.relayClient.Send(netDataWriter, this.RelayEndPoint, 1);
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x000109C4 File Offset: 0x0000EBC4
		private void OnListenSent(SimpleUdp udp)
		{
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x000109C8 File Offset: 0x0000EBC8
		private void OnRelayClientReceive(SimpleUdp udp, EndPoint endPoint, byte[] data, int count)
		{
			if (this.lastListenEndPoint == null)
			{
				return;
			}
			if (count < 10)
			{
				return;
			}
			if (RelayHeader.Deserialize(data).MessageType != RelayMessageType.Data)
			{
				return;
			}
			NetDataWriter netDataWriter = NetDataWriterPool.GetNetDataWriter();
			netDataWriter.Reset();
			netDataWriter.Put(data, 10, count - 10);
			this.listener.Send(netDataWriter, this.lastListenEndPoint, 1);
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x00010A21 File Offset: 0x0000EC21
		private void OnRelayClientSent(SimpleUdp udp)
		{
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x00010A24 File Offset: 0x0000EC24
		public void Dispose()
		{
			this.listener.OnDataReceived -= this.OnListenReceive;
			this.listener.OnDataSent -= this.OnListenSent;
			this.listener.Dispose();
			this.relayClient.OnDataReceived -= this.OnRelayClientReceive;
			this.relayClient.OnDataSent -= this.OnRelayClientSent;
			this.relayClient.Dispose();
		}

		// Token: 0x04000270 RID: 624
		public readonly EndPoint RelayEndPoint;

		// Token: 0x04000271 RID: 625
		public readonly int Port;

		// Token: 0x04000272 RID: 626
		public readonly ulong Key;

		// Token: 0x04000273 RID: 627
		public readonly Action<string, bool> Log;

		// Token: 0x04000274 RID: 628
		private readonly SimpleUdp listener;

		// Token: 0x04000275 RID: 629
		private readonly SimpleUdp relayClient;

		// Token: 0x04000276 RID: 630
		private EndPoint lastListenEndPoint;
	}
}
