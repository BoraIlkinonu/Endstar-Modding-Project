using System;
using Endless.Networking;
using Endless.Shared.DataTypes;
using MatchmakingAPI.Matches;
using Runtime.Shared.Matchmaking;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Endless.Matchmaking
{
	// Token: 0x02000044 RID: 68
	public class UnityClientMatchSession : MatchSession
	{
		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000276 RID: 630 RVA: 0x0000CB62 File Offset: 0x0000AD62
		// (set) Token: 0x06000277 RID: 631 RVA: 0x0000CB6A File Offset: 0x0000AD6A
		public ClientData LocalClientData { get; private set; }

		// Token: 0x06000278 RID: 632 RVA: 0x0000CB74 File Offset: 0x0000AD74
		public void Initialize(ClientData localClientData, MatchData matchData, SerializableGuid gameId, Allocation unityRelayAllocation)
		{
			MatchSession.Instance = this;
			this.LocalClientData = localClientData;
			base.Initialize(matchData, gameId);
			base.IsClient = true;
			base.IsServer = unityRelayAllocation != null;
			if (matchData.MatchServerType == MatchServerTypes.USER)
			{
				if (base.IsServer)
				{
					if (unityRelayAllocation != null)
					{
						this.StartHost(unityRelayAllocation);
						return;
					}
					Debug.LogException(new Exception("Allocation is null!"));
					return;
				}
				else if (base.IsClient)
				{
					if (!string.IsNullOrWhiteSpace(matchData.MatchAuthKey))
					{
						this.StartClient(matchData.MatchAuthKey);
						return;
					}
					Debug.LogException(new Exception("Match authentication key is invalid!"));
					return;
				}
			}
			else if (matchData.MatchServerType == MatchServerTypes.DEDICATED)
			{
				Debug.Log(string.Format("Starting client: Ip: {0}, Port: {1}", matchData.ServerInstance.InstanceIp, matchData.ServerInstance.InstancePort));
				this.StartClient(matchData.ServerInstance, matchData.MatchAuthKey);
			}
		}

		// Token: 0x06000279 RID: 633 RVA: 0x0000CC4C File Offset: 0x0000AE4C
		private async void StartClient(string authKey)
		{
			JoinAllocation joinAllocation;
			try
			{
				joinAllocation = await Relay.Instance.JoinAllocationAsync(authKey);
			}
			catch (Exception ex)
			{
				Debug.LogException(new Exception("Relay create join code request failed " + ex.Message));
				if (this.sessionNetworkManager != null && !this.sessionNetworkManager.ShutdownInProgress)
				{
					this.sessionNetworkManager.Shutdown(false);
				}
				return;
			}
			UnityTransport component = this.sessionNetworkManager.gameObject.GetComponent<UnityTransport>();
			this.sessionNetworkManager.NetworkConfig.NetworkTransport = component;
			component.SetRelayServerData(joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port, joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData, false);
			this.sessionNetworkManager.StartClient();
		}

		// Token: 0x0600027A RID: 634 RVA: 0x0000CC8C File Offset: 0x0000AE8C
		private void StartClient(ServerInstance serverInstance, string authKey)
		{
			this.sessionNetworkManager.NetworkConfig.ConnectionApproval = true;
			UnityTransport component = this.sessionNetworkManager.gameObject.GetComponent<UnityTransport>();
			this.sessionNetworkManager.NetworkConfig.NetworkTransport = component;
			component.SetConnectionData(serverInstance.InstanceIp, (ushort)serverInstance.InstancePort, null);
			DataBuffer dataBuffer = new DataBuffer(1000);
			dataBuffer.WriteString(authKey);
			this.sessionNetworkManager.NetworkConfig.ConnectionData = dataBuffer.ToArray();
			this.sessionNetworkManager.StartClient();
		}

		// Token: 0x0600027B RID: 635 RVA: 0x0000CD14 File Offset: 0x0000AF14
		private void StartHost(Allocation allocation)
		{
			UnityTransport component = this.sessionNetworkManager.gameObject.GetComponent<UnityTransport>();
			this.sessionNetworkManager.NetworkConfig.NetworkTransport = component;
			component.SetRelayServerData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, null, false);
			this.sessionNetworkManager.StartHost();
		}
	}
}
