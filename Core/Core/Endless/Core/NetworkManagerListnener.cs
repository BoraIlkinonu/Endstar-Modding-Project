using System;
using Endless.Gameplay;
using Endless.Matchmaking;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Core
{
	// Token: 0x0200002D RID: 45
	public class NetworkManagerListnener : MonoBehaviour
	{
		// Token: 0x060000C1 RID: 193 RVA: 0x00006042 File Offset: 0x00004242
		private void Start()
		{
			NetworkManager.Singleton.OnServerStarted += this.ServerStartedCallback;
			MatchSession.OnClientConnected += this.ClientConnectedCallback;
			MatchSession.OnClientLeft += this.ClientDisconnectedCallback;
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x0000607C File Offset: 0x0000427C
		protected void OnDestroy()
		{
			if (NetworkManager.Singleton)
			{
				NetworkManager.Singleton.OnServerStarted -= this.ServerStartedCallback;
			}
			MatchSession.OnClientConnected -= this.ClientConnectedCallback;
			MatchSession.OnClientLeft -= this.ClientDisconnectedCallback;
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x000060CD File Offset: 0x000042CD
		private void ServerStartedCallback()
		{
			global::UnityEngine.Object.Instantiate<GameObject>(this.networkSyncObject).GetComponent<NetworkObject>().Spawn(false);
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x0000229D File Offset: 0x0000049D
		private void ClientConnectedCallback(ulong clientID)
		{
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x000060E5 File Offset: 0x000042E5
		private void ClientDisconnectedCallback(ulong clientID)
		{
			if (NetworkManager.Singleton.IsServer)
			{
				NetworkScopeManager.RemoveConnection(clientID);
			}
		}

		// Token: 0x04000079 RID: 121
		[SerializeField]
		private GameObject networkSyncObject;
	}
}
