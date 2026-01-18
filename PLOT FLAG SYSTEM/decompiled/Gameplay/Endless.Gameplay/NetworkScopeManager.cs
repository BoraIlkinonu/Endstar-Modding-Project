using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class NetworkScopeManager : MonoBehaviourSingleton<NetworkScopeManager>
{
	private const int ACTIVATED_PER_FRAME = 2;

	private Coroutine scopeCoroutine;

	protected HashSet<ulong> fullyLoadedClients = new HashSet<ulong>();

	protected Dictionary<ulong, int> scopingClientDictionary = new Dictionary<ulong, int>();

	protected List<ulong> scopingClientList = new List<ulong>();

	protected HashSet<ulong> finishedScopingClientList = new HashSet<ulong>();

	protected List<NetworkObject> networkObjectScopeList = new List<NetworkObject>();

	private Dictionary<ulong, HashSet<ulong>> scopingClientObjectInScopeSet = new Dictionary<ulong, HashSet<ulong>>();

	private Dictionary<ulong, HashSet<ulong>> clientReadyPropsDictionary = new Dictionary<ulong, HashSet<ulong>>();

	private Dictionary<ulong, List<NetworkObject>> clientBuildingPropsBuffer = new Dictionary<ulong, List<NetworkObject>>();

	private void Start()
	{
		NetworkManager.Singleton.OnServerStopped += HandleNetworkReset;
	}

	public void HandleNetworkedPropBuiltByClient(ulong clientId, ulong networkPrefabId)
	{
		if (!clientReadyPropsDictionary.ContainsKey(clientId))
		{
			clientReadyPropsDictionary.Add(clientId, new HashSet<ulong>());
		}
		clientReadyPropsDictionary[clientId].Add(networkPrefabId);
	}

	public void ClientSendScopeRequest(SerializableGuid mapID)
	{
		FastBufferWriter messageStream = new FastBufferWriter(200, Allocator.Temp);
		using (new FastBufferWriter(200, Allocator.Temp))
		{
			messageStream.WriteValue(in mapID, default(FastBufferWriter.ForNetworkSerializable));
			NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ReadyToScope", 0uL, messageStream);
		}
	}

	private void HandleNetworkReset(bool back)
	{
		ClearAll();
	}

	public static bool CheckIfInScope(ulong clientId, NetworkObject networkObject)
	{
		if (clientId == 0L)
		{
			return true;
		}
		if (MonoBehaviourSingleton<NetworkScopeManager>.Instance.fullyLoadedClients.Contains(clientId))
		{
			if (MonoBehaviourSingleton<NetworkScopeManager>.Instance.clientReadyPropsDictionary[clientId].Contains(networkObject.PrefabIdHash))
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
		if (MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopingClientObjectInScopeSet.TryGetValue(clientId, out var value))
		{
			return value.Contains(networkObject.NetworkObjectId);
		}
		return false;
	}

	public static void AddNetworkObject(NetworkObject networkObject)
	{
		MonoBehaviourSingleton<NetworkScopeManager>.Instance.networkObjectScopeList.Add(networkObject);
	}

	public static void RemoveNetworkObject(NetworkObject networkObject)
	{
		MonoBehaviourSingleton<NetworkScopeManager>.Instance.networkObjectScopeList.Remove(networkObject);
	}

	public static void AddConnection(ulong clientId)
	{
		if (clientId != 0L)
		{
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
	}

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

	public static void BeginScopeChecking()
	{
		if (NetworkManager.Singleton.IsServer && MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopeCoroutine == null)
		{
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.scopeCoroutine = MonoBehaviourSingleton<NetworkScopeManager>.Instance.StartCoroutine(MonoBehaviourSingleton<NetworkScopeManager>.Instance.Loop());
		}
	}

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

	public static void PrepareToLoadNewLevel()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.ClearAll();
		}
	}

	public IEnumerator Loop()
	{
		while (true)
		{
			if (scopingClientList.Count > 0)
			{
				foreach (ulong finishedScopingClient in finishedScopingClientList)
				{
					StartCoroutine(FinishClientLoad(finishedScopingClient));
				}
				finishedScopingClientList.Clear();
				for (int i = 0; i < scopingClientList.Count; i++)
				{
					ScopeInLoop(scopingClientList[i]);
				}
			}
			foreach (ulong key in clientBuildingPropsBuffer.Keys)
			{
				if (clientBuildingPropsBuffer[key].Count < 1)
				{
					continue;
				}
				foreach (NetworkObject item in new List<NetworkObject>(clientBuildingPropsBuffer[key]))
				{
					try
					{
						if (clientReadyPropsDictionary[key].Contains(item.PrefabIdHash))
						{
							if (item.IsSpawned && !item.IsNetworkVisibleTo(key))
							{
								item.NetworkShow(key);
							}
							clientBuildingPropsBuffer[key].Remove(item);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			}
			if (!NetworkManager.Singleton.IsServer)
			{
				break;
			}
			yield return new NetworkTickUtility.WaitForNetworkTicks(1);
		}
		scopeCoroutine = null;
	}

	private IEnumerator FinishClientLoad(ulong clientId)
	{
		scopingClientList.Remove(clientId);
		fullyLoadedClients.Add(clientId);
		scopingClientObjectInScopeSet.Remove(clientId);
		scopingClientDictionary.Remove(clientId);
		yield return new NetworkTickUtility.WaitForNetworkTicks(1);
		foreach (NetworkObject networkObjectScope in networkObjectScopeList)
		{
			if (networkObjectScope != null && networkObjectScope.IsSpawned && !networkObjectScope.IsNetworkVisibleTo(clientId))
			{
				if (clientReadyPropsDictionary[clientId].Contains(networkObjectScope.PrefabIdHash))
				{
					networkObjectScope.NetworkShow(clientId);
				}
				else if (!clientBuildingPropsBuffer[clientId].Contains(networkObjectScope))
				{
					clientBuildingPropsBuffer[clientId].Add(networkObjectScope);
				}
			}
		}
		yield return new NetworkTickUtility.WaitForNetworkTicks(2);
		FastBufferWriter messageStream = new FastBufferWriter(200, Allocator.Temp);
		NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ScopeFinished", clientId, messageStream);
	}

	private void ScopeInLoop(ulong clientId)
	{
		for (int i = 0; i < 2; i++)
		{
			int num = scopingClientDictionary[clientId];
			if (num >= networkObjectScopeList.Count)
			{
				break;
			}
			if (networkObjectScopeList[num] != null && networkObjectScopeList[num].IsSpawned && !networkObjectScopeList[num].IsNetworkVisibleTo(clientId))
			{
				if (scopingClientObjectInScopeSet.TryGetValue(clientId, out var value))
				{
					value.Add(networkObjectScopeList[num].NetworkObjectId);
				}
				else
				{
					Debug.LogWarning("Client not found...");
				}
				NetworkObject networkObject = networkObjectScopeList[num];
				if (clientReadyPropsDictionary[clientId].Contains(networkObject.PrefabIdHash))
				{
					networkObject.NetworkShow(clientId);
				}
				else if (!clientBuildingPropsBuffer[clientId].Contains(networkObject))
				{
					clientBuildingPropsBuffer[clientId].Add(networkObject);
				}
			}
			scopingClientDictionary[clientId]++;
		}
		if (scopingClientDictionary[clientId] >= networkObjectScopeList.Count)
		{
			Debug.Log($"NetworkScope: Finished scoping client {clientId}");
			finishedScopingClientList.Add(clientId);
		}
	}
}
