using System;
using System.Net;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared.DataTypes;
using MatchmakingAPI.Matches;
using MatchmakingClientSDK;
using MatchmakingClientSDK.Udp;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x0200000E RID: 14
	public class EndlessClientMatchSession : MatchSession
	{
		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000069 RID: 105 RVA: 0x00004CBE File Offset: 0x00002EBE
		// (set) Token: 0x0600006A RID: 106 RVA: 0x00004CC6 File Offset: 0x00002EC6
		public ClientData LocalClientData { get; private set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600006B RID: 107 RVA: 0x00004CCF File Offset: 0x00002ECF
		// (set) Token: 0x0600006C RID: 108 RVA: 0x00004CD7 File Offset: 0x00002ED7
		public string Token { get; private set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600006D RID: 109 RVA: 0x00004CE0 File Offset: 0x00002EE0
		// (set) Token: 0x0600006E RID: 110 RVA: 0x00004CE8 File Offset: 0x00002EE8
		public string ServerType { get; private set; }

		// Token: 0x0600006F RID: 111 RVA: 0x00004CF4 File Offset: 0x00002EF4
		public void Initialize(ClientData localClientData, string token, MatchData matchData, SerializableGuid gameId, bool server, AllocationData allocationData, string serverType)
		{
			MatchSession.Instance = this;
			this.LocalClientData = localClientData;
			this.Token = token;
			this.ServerType = serverType;
			base.Initialize(matchData, gameId);
			base.IsClient = true;
			base.IsServer = server;
			if (matchData.MatchServerType == MatchServerTypes.USER)
			{
				if (base.IsServer)
				{
					this.StartHost(allocationData);
					return;
				}
				if (base.IsClient)
				{
					this.StartClient(allocationData);
				}
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00004D60 File Offset: 0x00002F60
		private void StartClient(AllocationData allocationData)
		{
			this.sessionNetworkManager.NetworkConfig.ConnectionApproval = false;
			UnityTransport component = this.sessionNetworkManager.gameObject.GetComponent<UnityTransport>();
			this.sessionNetworkManager.NetworkConfig.NetworkTransport = component;
			string serverType = this.ServerType;
			if (!(serverType == "EndlessRelay"))
			{
				if (!(serverType == "LAN"))
				{
					Debug.LogError("Unknown server type: " + this.ServerType);
				}
				else
				{
					component.SetConnectionData(allocationData.localIp, (ushort)allocationData.port, null);
				}
			}
			else
			{
				ulong num;
				if (!ulong.TryParse(allocationData.key, out num))
				{
					Debug.LogError("Failed to parse allocation key.");
				}
				int num2 = global::UnityEngine.Random.Range(40000, 60000);
				this.relayClient = new UdpRelayClient(IPAddress.Parse(allocationData.publicIp), allocationData.port, num2, num, new Action<string, bool>(this.Log));
				component.SetConnectionData("127.0.0.1", (ushort)num2, null);
			}
			this.sessionNetworkManager.StartClient();
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00004E60 File Offset: 0x00003060
		private void StartHost(AllocationData allocationData)
		{
			UnityTransport component = this.sessionNetworkManager.gameObject.GetComponent<UnityTransport>();
			this.sessionNetworkManager.NetworkConfig.NetworkTransport = component;
			component.SetConnectionData("127.0.0.1", (ushort)allocationData.port, "0.0.0.0");
			if (this.ServerType == "EndlessRelay")
			{
				ulong num;
				if (!ulong.TryParse(allocationData.key, out num))
				{
					Debug.LogError("Failed to parse allocation key.");
				}
				this.relayHost = new UdpRelayHost(allocationData.publicIp, allocationData.port, allocationData.port, num, this.Token, new Action<string, bool>(this.Log));
			}
			this.sessionNetworkManager.StartHost();
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00004F0D File Offset: 0x0000310D
		private void Log(string message, bool isError)
		{
			if (isError)
			{
				Debug.LogError(message);
				return;
			}
			Debug.Log(message);
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00004F1F File Offset: 0x0000311F
		protected override void OnDestroy()
		{
			UdpRelayHost udpRelayHost = this.relayHost;
			if (udpRelayHost != null)
			{
				udpRelayHost.Dispose();
			}
			this.relayHost = null;
			UdpRelayClient udpRelayClient = this.relayClient;
			if (udpRelayClient != null)
			{
				udpRelayClient.Dispose();
			}
			this.relayClient = null;
			base.OnDestroy();
		}

		// Token: 0x0400001D RID: 29
		private UdpRelayHost relayHost;

		// Token: 0x0400001E RID: 30
		private UdpRelayClient relayClient;
	}
}
