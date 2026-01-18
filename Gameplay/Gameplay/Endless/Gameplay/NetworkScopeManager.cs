using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200011E RID: 286
	public class NetworkScopeManager : MonoBehaviourSingleton<NetworkScopeManager>
	{
		// Token: 0x06000670 RID: 1648 RVA: 0x0001F7EE File Offset: 0x0001D9EE
		private void Start()
		{
			NetworkManager.Singleton.OnServerStopped += this.HandleNetworkReset;
		}

		// Token: 0x06000671 RID: 1649 RVA: 0x0001F806 File Offset: 0x0001DA06
		public void HandleNetworkedPropBuiltByClient(ulong clientId, ulong networkPrefabId)
		{
			if (!this.clientReadyPropsDictionary.ContainsKey(clientId))
			{
				this.clientReadyPropsDictionary.Add(clientId, new HashSet<ulong>());
			}
			this.clientReadyPropsDictionary[clientId].Add(networkPrefabId);
		}

		// Token: 0x06000672 RID: 1650 RVA: 0x0001F83C File Offset: 0x0001DA3C
		public void ClientSendScopeRequest(SerializableGuid mapID)
		{
			FastBufferWriter fastBufferWriter = new FastBufferWriter(200, Allocator.Temp, -1);
			using (new FastBufferWriter(200, Allocator.Temp, -1))
			{
				fastBufferWriter.WriteValue<SerializableGuid>(in mapID, default(FastBufferWriter.ForNetworkSerializable));
				NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ReadyToScope", 0UL, fastBufferWriter, NetworkDelivery.ReliableSequenced);
			}
		}

		// Token: 0x06000673 RID: 1651 RVA: 0x0001F8B0 File Offset: 0x0001DAB0
		private void HandleNetworkReset(bool back)
		{
			this.ClearAll();
		}

		// Token: 0x06000674 RID: 1652 RVA: 0x0001F8B8 File Offset: 0x0001DAB8
		public static bool CheckIfInScope(ulong clientId, NetworkObject networkObject)
		{
			if (clientId == 0UL)
			{
				return true;
			}
			if (!MonoBehaviourSingleton<NetworkScopeManager>.Instance.fullyLoadedClients.Contains(clientId))
			{
				HashSet<ulong> hashSet;
				return MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientObjectInScopeSet.TryGetValue(clientId, out hashSet) && hashSet.Contains(networkObject.NetworkObjectId);
			}
			if (MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientReadyPropsDictionary[clientId].Contains((ulong)networkObject.PrefabIdHash))
			{
				return true;
			}
			if (!MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientBuildingPropsBuffer.ContainsKey(clientId))
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientBuildingPropsBuffer.Add(clientId, new List<NetworkObject>());
			}
			if (!MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientBuildingPropsBuffer[clientId].Contains(networkObject))
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientBuildingPropsBuffer[clientId].Add(networkObject);
			}
			return false;
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x0001F975 File Offset: 0x0001DB75
		public static void AddNetworkObject(NetworkObject networkObject)
		{
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.networkObjectScopeList.Add(networkObject);
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x0001F987 File Offset: 0x0001DB87
		public static void RemoveNetworkObject(NetworkObject networkObject)
		{
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.networkObjectScopeList.Remove(networkObject);
		}

		// Token: 0x06000677 RID: 1655 RVA: 0x0001F99C File Offset: 0x0001DB9C
		public static void AddConnection(ulong clientId)
		{
			if (clientId == 0UL)
			{
				return;
			}
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientList.Add(clientId);
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientDictionary.Add(clientId, 0);
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientObjectInScopeSet.Add(clientId, new HashSet<ulong>());
			if (!MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientBuildingPropsBuffer.ContainsKey(clientId))
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientBuildingPropsBuffer.Add(clientId, new List<NetworkObject>());
			}
			if (!MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientReadyPropsDictionary.ContainsKey(clientId))
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientReadyPropsDictionary.Add(clientId, new HashSet<ulong>());
			}
		}

		// Token: 0x06000678 RID: 1656 RVA: 0x0001FA34 File Offset: 0x0001DC34
		public static void RemoveConnection(ulong clientId)
		{
			if (MonoBehaviourSingleton<NetworkScopeManager>.Instance.fullyLoadedClients.Contains(clientId))
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.fullyLoadedClients.Remove(clientId);
			}
			if (MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientDictionary.ContainsKey(clientId))
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientDictionary.Remove(clientId);
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientList.Remove(clientId);
			}
			if (MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientObjectInScopeSet.ContainsKey(clientId))
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientObjectInScopeSet.Remove(clientId);
			}
			if (MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientBuildingPropsBuffer.ContainsKey(clientId))
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientBuildingPropsBuffer.Remove(clientId);
			}
			if (MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientReadyPropsDictionary.ContainsKey(clientId))
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientReadyPropsDictionary.Remove(clientId);
			}
		}

		// Token: 0x06000679 RID: 1657 RVA: 0x0001FB01 File Offset: 0x0001DD01
		public static void BeginScopeChecking()
		{
			if (!NetworkManager.Singleton.IsServer)
			{
				return;
			}
			if (MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopeCoroutine == null)
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopeCoroutine = MonoBehaviourSingleton<NetworkScopeManager>.Instance.StartCoroutine(MonoBehaviourSingleton<NetworkScopeManager>.Instance.Loop());
			}
		}

		// Token: 0x0600067A RID: 1658 RVA: 0x0001FB3C File Offset: 0x0001DD3C
		private void ClearAll()
		{
			if (MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopeCoroutine != null)
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.StopCoroutine(MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopeCoroutine);
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopeCoroutine = null;
			}
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientDictionary.Clear();
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientList.Clear();
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.finishedScopingClientList.Clear();
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.fullyLoadedClients.Clear();
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientObjectInScopeSet.Clear();
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientReadyPropsDictionary.Clear();
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientBuildingPropsBuffer.Clear();
		}

		// Token: 0x0600067B RID: 1659 RVA: 0x0001FBDD File Offset: 0x0001DDDD
		public static void PrepareToLoadNewLevel()
		{
			if (!NetworkManager.Singleton.IsServer)
			{
				return;
			}
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.ClearAll();
		}

		// Token: 0x0600067C RID: 1660 RVA: 0x0001FBF6 File Offset: 0x0001DDF6
		public IEnumerator Loop()
		{
			for (;;)
			{
				if (this.scopingClientList.Count > 0)
				{
					foreach (ulong num in this.finishedScopingClientList)
					{
						base.StartCoroutine(this.FinishClientLoad(num));
					}
					this.finishedScopingClientList.Clear();
					for (int i = 0; i < this.scopingClientList.Count; i++)
					{
						this.ScopeInLoop(this.scopingClientList[i]);
					}
				}
				foreach (ulong num2 in this.clientBuildingPropsBuffer.Keys)
				{
					if (this.clientBuildingPropsBuffer[num2].Count >= 1)
					{
						foreach (NetworkObject networkObject in new List<NetworkObject>(this.clientBuildingPropsBuffer[num2]))
						{
							try
							{
								if (this.clientReadyPropsDictionary[num2].Contains((ulong)networkObject.PrefabIdHash))
								{
									if (networkObject.IsSpawned && !networkObject.IsNetworkVisibleTo(num2))
									{
										networkObject.NetworkShow(num2);
									}
									this.clientBuildingPropsBuffer[num2].Remove(networkObject);
								}
							}
							catch (Exception ex)
							{
								Debug.LogException(ex);
							}
						}
					}
				}
				if (!NetworkManager.Singleton.IsServer)
				{
					break;
				}
				yield return new NetworkTickUtility.WaitForNetworkTicks(1);
			}
			this.scopeCoroutine = null;
			yield break;
		}

		// Token: 0x0600067D RID: 1661 RVA: 0x0001FC05 File Offset: 0x0001DE05
		private IEnumerator FinishClientLoad(ulong clientId)
		{
			this.scopingClientList.Remove(clientId);
			this.fullyLoadedClients.Add(clientId);
			this.scopingClientObjectInScopeSet.Remove(clientId);
			this.scopingClientDictionary.Remove(clientId);
			yield return new NetworkTickUtility.WaitForNetworkTicks(1);
			foreach (NetworkObject networkObject in this.networkObjectScopeList)
			{
				if (networkObject != null && networkObject.IsSpawned && !networkObject.IsNetworkVisibleTo(clientId))
				{
					if (this.clientReadyPropsDictionary[clientId].Contains((ulong)networkObject.PrefabIdHash))
					{
						networkObject.NetworkShow(clientId);
					}
					else if (!this.clientBuildingPropsBuffer[clientId].Contains(networkObject))
					{
						this.clientBuildingPropsBuffer[clientId].Add(networkObject);
					}
				}
			}
			yield return new NetworkTickUtility.WaitForNetworkTicks(2);
			FastBufferWriter fastBufferWriter = new FastBufferWriter(200, Allocator.Temp, -1);
			NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ScopeFinished", clientId, fastBufferWriter, NetworkDelivery.ReliableSequenced);
			yield break;
		}

		// Token: 0x0600067E RID: 1662 RVA: 0x0001FC1C File Offset: 0x0001DE1C
		private void ScopeInLoop(ulong clientId)
		{
			for (int i = 0; i < 2; i++)
			{
				int num = this.scopingClientDictionary[clientId];
				if (num >= this.networkObjectScopeList.Count)
				{
					break;
				}
				if (this.networkObjectScopeList[num] != null && this.networkObjectScopeList[num].IsSpawned && !this.networkObjectScopeList[num].IsNetworkVisibleTo(clientId))
				{
					HashSet<ulong> hashSet;
					if (this.scopingClientObjectInScopeSet.TryGetValue(clientId, out hashSet))
					{
						hashSet.Add(this.networkObjectScopeList[num].NetworkObjectId);
					}
					else
					{
						Debug.LogWarning("Client not found...");
					}
					NetworkObject networkObject = this.networkObjectScopeList[num];
					if (this.clientReadyPropsDictionary[clientId].Contains((ulong)networkObject.PrefabIdHash))
					{
						networkObject.NetworkShow(clientId);
					}
					else if (!this.clientBuildingPropsBuffer[clientId].Contains(networkObject))
					{
						this.clientBuildingPropsBuffer[clientId].Add(networkObject);
					}
				}
				Dictionary<ulong, int> dictionary = this.scopingClientDictionary;
				int num2 = dictionary[clientId];
				dictionary[clientId] = num2 + 1;
			}
			if (this.scopingClientDictionary[clientId] >= this.networkObjectScopeList.Count)
			{
				Debug.Log(string.Format("NetworkScope: Finished scoping client {0}", clientId));
				this.finishedScopingClientList.Add(clientId);
			}
		}

		// Token: 0x040004DA RID: 1242
		private const int ACTIVATED_PER_FRAME = 2;

		// Token: 0x040004DB RID: 1243
		private Coroutine scopeCoroutine;

		// Token: 0x040004DC RID: 1244
		protected HashSet<ulong> fullyLoadedClients = new HashSet<ulong>();

		// Token: 0x040004DD RID: 1245
		protected Dictionary<ulong, int> scopingClientDictionary = new Dictionary<ulong, int>();

		// Token: 0x040004DE RID: 1246
		protected List<ulong> scopingClientList = new List<ulong>();

		// Token: 0x040004DF RID: 1247
		protected HashSet<ulong> finishedScopingClientList = new HashSet<ulong>();

		// Token: 0x040004E0 RID: 1248
		protected List<NetworkObject> networkObjectScopeList = new List<NetworkObject>();

		// Token: 0x040004E1 RID: 1249
		private Dictionary<ulong, HashSet<ulong>> scopingClientObjectInScopeSet = new Dictionary<ulong, HashSet<ulong>>();

		// Token: 0x040004E2 RID: 1250
		private Dictionary<ulong, HashSet<ulong>> clientReadyPropsDictionary = new Dictionary<ulong, HashSet<ulong>>();

		// Token: 0x040004E3 RID: 1251
		private Dictionary<ulong, List<NetworkObject>> clientBuildingPropsBuffer = new Dictionary<ulong, List<NetworkObject>>();
	}
}
