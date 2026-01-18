using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Networking.UDP;
using Networking.UDP.Utils;
using Relay;

namespace MatchmakingClientSDK.Udp
{
	// Token: 0x02000063 RID: 99
	public class UdpRelayHost : IDisposable
	{
		// Token: 0x1700006D RID: 109
		// (get) Token: 0x060003C8 RID: 968 RVA: 0x00010AA3 File Offset: 0x0000ECA3
		public bool Disposed
		{
			get
			{
				return this.disposed;
			}
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x060003C9 RID: 969 RVA: 0x00010AAD File Offset: 0x0000ECAD
		// (set) Token: 0x060003CA RID: 970 RVA: 0x00010AB5 File Offset: 0x0000ECB5
		public bool Authenticated { get; private set; }

		// Token: 0x060003CB RID: 971 RVA: 0x00010AC0 File Offset: 0x0000ECC0
		public UdpRelayHost(string ip, int relayPort, int localServerPort, ulong key, string token, Action<string, bool> log)
		{
			this.Ip = ip;
			this.RelayPort = relayPort;
			this.LocalServerPort = localServerPort;
			this.Token = token;
			this.Key = key;
			this.log = log;
			this.RelayEndPoint = new IPEndPoint(IPAddress.Parse(this.Ip), this.RelayPort);
			this.LocalEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), this.LocalServerPort);
			this.relayClient = new SimpleUdp(false, 0, this.log);
			this.relayClient.OnDataReceived += this.OnReceive;
			this.relayClient.OnDataSent += this.OnSent;
			this.relayClient.Client();
			this.Authenticate();
		}

		// Token: 0x060003CC RID: 972 RVA: 0x00010BA0 File Offset: 0x0000EDA0
		private async Task Authenticate()
		{
			this.log(string.Format("Authenticating with relay server [{0}]...", this.Key), false);
			while (!this.disposed && !this.Authenticated)
			{
				RelayHeader relayHeader = new RelayHeader
				{
					MessageType = RelayMessageType.Connect,
					Channel = 0,
					Key = this.Key
				};
				NetDataWriter netDataWriter = NetDataWriterPool.GetNetDataWriter();
				netDataWriter.Reset();
				relayHeader.Serialize(netDataWriter);
				netDataWriter.Put(Encoding.UTF8.GetBytes(this.Token));
				this.Send(netDataWriter, 5);
				await Task.Delay(3000);
			}
		}

		// Token: 0x060003CD RID: 973 RVA: 0x00010BE4 File Offset: 0x0000EDE4
		private void OnReceive(SimpleUdp udpClient, EndPoint endPoint, byte[] data, int count)
		{
			if (data == null || count < 10)
			{
				this.log(string.Format("Relay message too short. Must be at least {0} bytes.", 10), true);
				return;
			}
			RelayHeader relayHeader = RelayHeader.Deserialize(data);
			ArraySegment<byte> arraySegment = new ArraySegment<byte>(data, 10, count - 10);
			if (relayHeader.MessageType == RelayMessageType.Connect)
			{
				if (!this.Authenticated)
				{
					this.log("Authenticated with relay server!", false);
					this.Authenticated = true;
				}
				return;
			}
			if (relayHeader.Channel == 0)
			{
				return;
			}
			SimpleUdp simpleUdp;
			if (!this.localTunnels.TryGetValue(relayHeader.Channel, out simpleUdp))
			{
				simpleUdp = new SimpleUdp(false, relayHeader.Channel, this.log);
				simpleUdp.OnDataReceived += this.OnTunnelReceive;
				simpleUdp.OnDataSent += this.OnTunnelSent;
				this.localTunnels[relayHeader.Channel] = simpleUdp;
				simpleUdp.Client();
			}
			NetDataWriter netDataWriter = NetDataWriterPool.GetNetDataWriter();
			netDataWriter.Put(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
			simpleUdp.Send(netDataWriter, this.LocalEndPoint, 1);
		}

		// Token: 0x060003CE RID: 974 RVA: 0x00010CF3 File Offset: 0x0000EEF3
		private void Send(NetDataWriter buffer, int attempts = 1)
		{
			this.relayClient.Send(buffer, this.RelayEndPoint, attempts);
		}

		// Token: 0x060003CF RID: 975 RVA: 0x00010D08 File Offset: 0x0000EF08
		private async void OnSent(SimpleUdp udp)
		{
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x00010D38 File Offset: 0x0000EF38
		private void OnTunnelReceive(SimpleUdp tunnel, EndPoint endPoint, byte[] data, int count)
		{
			RelayHeader relayHeader = new RelayHeader
			{
				MessageType = RelayMessageType.Data,
				Channel = tunnel.Id,
				Key = this.Key
			};
			NetDataWriter netDataWriter = NetDataWriterPool.GetNetDataWriter();
			relayHeader.Serialize(netDataWriter);
			netDataWriter.Put(data, 0, count);
			this.Send(netDataWriter, 1);
		}

		// Token: 0x060003D1 RID: 977 RVA: 0x00010D91 File Offset: 0x0000EF91
		private void OnTunnelSent(SimpleUdp tunnel)
		{
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x00010D94 File Offset: 0x0000EF94
		public void Dispose()
		{
			object obj = this.localLock;
			lock (obj)
			{
				if (!this.disposed)
				{
					this.disposed = true;
					foreach (SimpleUdp simpleUdp in this.localTunnels.Values)
					{
						simpleUdp.Dispose();
					}
					this.relayClient.Dispose();
				}
			}
		}

		// Token: 0x04000277 RID: 631
		public const string LOCAL_HOST = "127.0.0.1";

		// Token: 0x04000278 RID: 632
		public readonly string Ip;

		// Token: 0x04000279 RID: 633
		public readonly int RelayPort;

		// Token: 0x0400027A RID: 634
		public readonly int LocalServerPort;

		// Token: 0x0400027B RID: 635
		public readonly string Token;

		// Token: 0x0400027C RID: 636
		public readonly ulong Key;

		// Token: 0x0400027D RID: 637
		public readonly EndPoint RelayEndPoint;

		// Token: 0x0400027E RID: 638
		public readonly EndPoint LocalEndPoint;

		// Token: 0x0400027F RID: 639
		private readonly Action<string, bool> log;

		// Token: 0x04000281 RID: 641
		private readonly object localLock = new object();

		// Token: 0x04000282 RID: 642
		private volatile bool disposed;

		// Token: 0x04000283 RID: 643
		private readonly SimpleUdp relayClient;

		// Token: 0x04000284 RID: 644
		private readonly ConcurrentDictionary<byte, SimpleUdp> localTunnels = new ConcurrentDictionary<byte, SimpleUdp>();
	}
}
