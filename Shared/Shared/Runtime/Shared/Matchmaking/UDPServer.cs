using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000026 RID: 38
	public class UDPServer
	{
		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000118 RID: 280 RVA: 0x00007B3E File Offset: 0x00005D3E
		// (set) Token: 0x06000119 RID: 281 RVA: 0x00007B46 File Offset: 0x00005D46
		public UnityTransport Transport { get; private set; }

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x0600011A RID: 282 RVA: 0x00007B4F File Offset: 0x00005D4F
		// (set) Token: 0x0600011B RID: 283 RVA: 0x00007B57 File Offset: 0x00005D57
		public string LastMessage { get; private set; } = string.Empty;

		// Token: 0x0600011C RID: 284 RVA: 0x00007B60 File Offset: 0x00005D60
		public UDPServer(int port, string key)
		{
			this.InitializeTransport();
			this.StartServer(port, key);
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00007B8C File Offset: 0x00005D8C
		private void InitializeTransport()
		{
			this.Transport = new GameObject("Transport").AddComponent<UnityTransport>();
			this.Transport.MaxPacketQueueSize = 1280;
			this.Transport.MaxPayloadSize = 61440;
			this.Transport.HeartbeatTimeoutMS = 500;
			this.Transport.ConnectTimeoutMS = 1000;
			this.Transport.MaxConnectAttempts = 60;
			this.Transport.DisconnectTimeoutMS = 30000;
			this.Transport.OnTransportEvent += this.OnTransportEvent;
			this.Transport.Initialize(null);
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00007C2E File Offset: 0x00005E2E
		private void StartServer(int port, string key)
		{
			this.Transport.SetConnectionData(IPAddress.Any.ToString(), (ushort)port, IPAddress.Any.ToString());
			this.Transport.StartServer();
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00007C60 File Offset: 0x00005E60
		public void Send(string message, params ulong[] excludedClients)
		{
			foreach (ulong num in this.clients)
			{
				if (!excludedClients.Contains(num))
				{
					this.Transport.Send(num, Encoding.UTF8.GetBytes(message), NetworkDelivery.ReliableSequenced);
				}
			}
		}

		// Token: 0x06000120 RID: 288 RVA: 0x00007CD4 File Offset: 0x00005ED4
		private void OnTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
		{
			switch (eventType)
			{
			case NetworkEvent.Data:
				Debug.Log("Received a message");
				try
				{
					this.LastMessage = Encoding.UTF8.GetString(payload.Array, payload.Offset, payload.Count);
					this.Send(this.LastMessage, new ulong[] { clientId });
					return;
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to parse incoming message {0}", ex));
					return;
				}
				break;
			case NetworkEvent.Connect:
				if (this.clients.Add(clientId))
				{
					Debug.Log(string.Format("Client {0} connected!", clientId));
					return;
				}
				return;
			case NetworkEvent.Disconnect:
				if (this.clients.Remove(clientId))
				{
					Debug.Log(string.Format("Client {0} disconnected!", clientId));
					return;
				}
				return;
			case NetworkEvent.TransportFailure:
				break;
			case NetworkEvent.Nothing:
				return;
			default:
				return;
			}
			Debug.LogError("Transport failure");
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00007DBC File Offset: 0x00005FBC
		public void Dispose()
		{
			if (this.Transport != null)
			{
				global::UnityEngine.Object.Destroy(this.Transport.gameObject);
			}
		}

		// Token: 0x04000092 RID: 146
		private readonly HashSet<ulong> clients = new HashSet<ulong>();
	}
}
