using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000024 RID: 36
	public class UDPClient : IDisposable
	{
		// Token: 0x17000031 RID: 49
		// (get) Token: 0x06000103 RID: 259 RVA: 0x0000755E File Offset: 0x0000575E
		// (set) Token: 0x06000104 RID: 260 RVA: 0x00007566 File Offset: 0x00005766
		public UnityTransport Transport { get; private set; }

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000105 RID: 261 RVA: 0x0000756F File Offset: 0x0000576F
		// (set) Token: 0x06000106 RID: 262 RVA: 0x00007577 File Offset: 0x00005777
		public ulong? ClientId { get; private set; }

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000107 RID: 263 RVA: 0x00007580 File Offset: 0x00005780
		public bool Connected
		{
			get
			{
				return this.ClientId != null;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000108 RID: 264 RVA: 0x0000759B File Offset: 0x0000579B
		// (set) Token: 0x06000109 RID: 265 RVA: 0x000075A3 File Offset: 0x000057A3
		public string LastMessage { get; private set; } = string.Empty;

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x0600010A RID: 266 RVA: 0x000075AC File Offset: 0x000057AC
		// (set) Token: 0x0600010B RID: 267 RVA: 0x000075B4 File Offset: 0x000057B4
		public bool IsHost { get; private set; }

		// Token: 0x0600010C RID: 268 RVA: 0x000075C0 File Offset: 0x000057C0
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

		// Token: 0x0600010D RID: 269 RVA: 0x00007662 File Offset: 0x00005862
		public UDPClient(string joinCode)
		{
			this.InitializeTransport();
			this.JoinRelay(joinCode);
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00007690 File Offset: 0x00005890
		private async void JoinRelay(string joinCode)
		{
			try
			{
				JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
				this.Transport.SetClientRelayData(joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port, joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData, false);
				this.Transport.StartClient();
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to join relay " + ex.Message);
			}
		}

		// Token: 0x0600010F RID: 271 RVA: 0x000076CF File Offset: 0x000058CF
		public UDPClient(Allocation relayAllocation)
		{
			this.InitializeTransport();
			this.HostRelay(relayAllocation);
		}

		// Token: 0x06000110 RID: 272 RVA: 0x000076FC File Offset: 0x000058FC
		private void HostRelay(Allocation allocation)
		{
			this.Transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, false);
			this.Transport.StartServer();
			this.IsHost = true;
			this.OnTransportEvent(NetworkEvent.Connect, this.Transport.ServerClientId, default(ArraySegment<byte>), Time.time);
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00007771 File Offset: 0x00005971
		public UDPClient(string publicIp, string localIp, string name, int port, string key)
		{
			this.InitializeTransport();
			this.ConnectToServer(publicIp, localIp, name, port, key);
		}

		// Token: 0x06000112 RID: 274 RVA: 0x000077A2 File Offset: 0x000059A2
		private void ConnectToServer(string publicIp, string localIp, string name, int port, string key)
		{
			this.Transport.SetConnectionData(publicIp, (ushort)port, null);
			this.Transport.StartClient();
		}

		// Token: 0x06000113 RID: 275 RVA: 0x000077C0 File Offset: 0x000059C0
		public void Send(string message, params ulong[] excludedClients)
		{
			if (this.Connected)
			{
				if (!this.IsHost)
				{
					this.Transport.Send(this.Transport.ServerClientId, Encoding.UTF8.GetBytes(message), NetworkDelivery.ReliableSequenced);
					return;
				}
				foreach (ulong num in this.otherClients)
				{
					if (!excludedClients.Contains(num))
					{
						this.Transport.Send(num, Encoding.UTF8.GetBytes(message), NetworkDelivery.ReliableSequenced);
					}
				}
			}
		}

		// Token: 0x06000114 RID: 276 RVA: 0x00007870 File Offset: 0x00005A70
		private void OnTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
		{
			switch (eventType)
			{
			case NetworkEvent.Data:
				Debug.Log("Received a message");
				try
				{
					this.LastMessage = Encoding.UTF8.GetString(payload.Array, payload.Offset, payload.Count);
					if (this.IsHost)
					{
						this.Send(this.LastMessage, new ulong[] { clientId });
					}
					return;
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to parse incoming message {0}", ex));
					return;
				}
				break;
			case NetworkEvent.Connect:
				if (this.ClientId == null)
				{
					this.ClientId = new ulong?(clientId);
					Debug.Log("Connected to match server!");
					return;
				}
				if (this.otherClients.Add(clientId))
				{
					Debug.Log(string.Format("Client {0} connected!", clientId));
					return;
				}
				return;
			case NetworkEvent.Disconnect:
			{
				ulong? clientId2 = this.ClientId;
				if ((clientId2.GetValueOrDefault() == clientId) & (clientId2 != null))
				{
					this.ClientId = null;
					this.otherClients.Clear();
					Debug.Log("Disconnected from match server!");
					return;
				}
				if (this.otherClients.Remove(clientId))
				{
					Debug.Log(string.Format("Client {0} disconnected!", clientId));
					return;
				}
				return;
			}
			case NetworkEvent.TransportFailure:
				break;
			case NetworkEvent.Nothing:
				return;
			default:
				return;
			}
			Debug.LogError("Transport failure");
		}

		// Token: 0x06000115 RID: 277 RVA: 0x000079CC File Offset: 0x00005BCC
		public void Dispose()
		{
			this.ClientId = new ulong?(0UL);
			if (this.Transport != null)
			{
				global::UnityEngine.Object.Destroy(this.Transport.gameObject);
			}
		}

		// Token: 0x0400008A RID: 138
		private readonly HashSet<ulong> otherClients = new HashSet<ulong>();
	}
}
