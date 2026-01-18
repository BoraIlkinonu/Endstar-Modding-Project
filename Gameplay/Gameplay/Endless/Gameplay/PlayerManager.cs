using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000277 RID: 631
	public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
	{
		// Token: 0x17000269 RID: 617
		// (get) Token: 0x06000D50 RID: 3408 RVA: 0x0004860A File Offset: 0x0004680A
		public int CurrentPlayerCount
		{
			get
			{
				return this.playerObjectMap.Keys.Count;
			}
		}

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x06000D51 RID: 3409 RVA: 0x0004861C File Offset: 0x0004681C
		public List<ulong> CurrentPlayerGuids
		{
			get
			{
				return new List<ulong>(this.playerObjectMap.Keys);
			}
		}

		// Token: 0x06000D52 RID: 3410 RVA: 0x00048630 File Offset: 0x00046830
		public void RegisterPlayer(ulong clientId, PlayerReferenceManager playerReferenceManager)
		{
			Debug.Log(string.Format("Registering player for clientId {0}", clientId));
			if (this.playerObjectMap.ContainsKey(clientId))
			{
				if (NetworkManager.Singleton.IsServer)
				{
					global::UnityEngine.Object.Destroy(playerReferenceManager.gameObject);
				}
				Debug.LogException(new Exception(string.Format("Player Manager already has a player registered for ID {0}", clientId)));
				this.playerObjectMap[clientId] = playerReferenceManager;
			}
			else
			{
				this.playerObjectMap.Add(clientId, playerReferenceManager);
			}
			this.OnNewPlayerRegistered.Invoke(clientId, playerReferenceManager);
			if (playerReferenceManager.IsOwner)
			{
				this.OnOwnerRegistered.Invoke(clientId, playerReferenceManager);
			}
		}

		// Token: 0x06000D53 RID: 3411 RVA: 0x000486D0 File Offset: 0x000468D0
		public void UnregisterPlayer(ulong clientId, PlayerReferenceManager playerReferenceManager)
		{
			Debug.Log(string.Format("Unregistering player for clientId {0}", clientId));
			if (this.playerObjectMap.ContainsKey(clientId))
			{
				if (playerReferenceManager == this.playerObjectMap[clientId])
				{
					this.playerObjectMap.Remove(clientId);
					this.PlayerUnregistered.Invoke(clientId, playerReferenceManager);
					return;
				}
			}
			else
			{
				Debug.LogException(new Exception(string.Format("Player Manager does not have a player registered for ID {0}", clientId)));
			}
		}

		// Token: 0x06000D54 RID: 3412 RVA: 0x00048749 File Offset: 0x00046949
		public PlayerReferenceManager GetPlayerObject(ulong clientId)
		{
			if (this.playerObjectMap.ContainsKey(clientId))
			{
				return this.playerObjectMap[clientId];
			}
			return null;
		}

		// Token: 0x06000D55 RID: 3413 RVA: 0x0004861C File Offset: 0x0004681C
		public List<ulong> GetPlayerIds()
		{
			return new List<ulong>(this.playerObjectMap.Keys);
		}

		// Token: 0x06000D56 RID: 3414 RVA: 0x00048768 File Offset: 0x00046968
		public PlayerReferenceManager GetLocalPlayerObject()
		{
			ulong localClientId = NetworkManager.Singleton.LocalClientId;
			return this.GetPlayerObject(localClientId);
		}

		// Token: 0x06000D57 RID: 3415 RVA: 0x00048787 File Offset: 0x00046987
		public bool IsPlayerInitialized(ulong clientId)
		{
			return this.playerObjectMap.ContainsKey(clientId) && this.playerObjectMap[clientId] != null;
		}

		// Token: 0x06000D58 RID: 3416 RVA: 0x000487AB File Offset: 0x000469AB
		public PlayerReferenceManager[] GetPlayerObjects()
		{
			return this.playerObjectMap.Values.ToArray<PlayerReferenceManager>();
		}

		// Token: 0x04000C41 RID: 3137
		public UnityEvent<ulong, PlayerReferenceManager> OnNewPlayerRegistered = new UnityEvent<ulong, PlayerReferenceManager>();

		// Token: 0x04000C42 RID: 3138
		public UnityEvent<ulong, PlayerReferenceManager> OnOwnerRegistered = new UnityEvent<ulong, PlayerReferenceManager>();

		// Token: 0x04000C43 RID: 3139
		public UnityEvent<ulong, PlayerReferenceManager> PlayerUnregistered = new UnityEvent<ulong, PlayerReferenceManager>();

		// Token: 0x04000C44 RID: 3140
		private readonly Dictionary<ulong, PlayerReferenceManager> playerObjectMap = new Dictionary<ulong, PlayerReferenceManager>();
	}
}
