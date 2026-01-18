using System;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000027 RID: 39
	public class UnityServerMatchSession : MatchSession
	{
		// Token: 0x06000122 RID: 290 RVA: 0x00007DDC File Offset: 0x00005FDC
		public override void Initialize(MatchData matchData, SerializableGuid gameId)
		{
			MatchSession.Instance = this;
			base.Initialize(matchData, gameId);
			Debug.Log(string.Format("Starting server: Ip: {0}, Port: {1}", matchData.ServerInstance.InstanceIp, matchData.ServerInstance.InstancePort));
			EndlessServices.New(base.transform);
			EndlessServices.Instance.Initialize(null, TargetPlatforms.Test);
			this.StartServer(matchData.ServerInstance, matchData.MatchAuthKey);
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00007E4C File Offset: 0x0000604C
		public void StartServer(ServerInstance serverInstance, string authKey)
		{
			this.sessionNetworkManager.NetworkConfig.ConnectionApproval = true;
			UnityTransport component = this.sessionNetworkManager.gameObject.GetComponent<UnityTransport>();
			this.sessionNetworkManager.NetworkConfig.NetworkTransport = component;
			component.SetConnectionData(serverInstance.InstanceIp, (ushort)serverInstance.InstancePort, "0.0.0.0");
			NetworkManager sessionNetworkManager = this.sessionNetworkManager;
			sessionNetworkManager.ConnectionApprovalCallback = (Action<NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse>)Delegate.Combine(sessionNetworkManager.ConnectionApprovalCallback, new Action<NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse>(this.ProcessConnectionApproval));
			this.sessionNetworkManager.StartServer();
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00007ED8 File Offset: 0x000060D8
		private void ProcessConnectionApproval(NetworkManager.ConnectionApprovalRequest approvalRequest, NetworkManager.ConnectionApprovalResponse approvalResponse)
		{
			DataBuffer dataBuffer = DataBuffer.FromBytes(approvalRequest.Payload);
			try
			{
				string text = dataBuffer.ReadString(true);
				approvalResponse.Approved = text == base.MatchData.MatchAuthKey;
				approvalResponse.Pending = false;
				approvalResponse.CreatePlayerObject = true;
				Debug.Log("Connection approved with correct auth key.");
			}
			catch (Exception ex)
			{
				approvalResponse.Approved = false;
				Debug.LogError(ex);
			}
		}

		// Token: 0x06000125 RID: 293 RVA: 0x00007F48 File Offset: 0x00006148
		protected override void OnDestroy()
		{
			base.OnDestroy();
			EndlessServices.Remove();
		}
	}
}
