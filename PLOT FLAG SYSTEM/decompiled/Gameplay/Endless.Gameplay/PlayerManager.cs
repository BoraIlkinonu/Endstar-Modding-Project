using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
{
	public UnityEvent<ulong, PlayerReferenceManager> OnNewPlayerRegistered = new UnityEvent<ulong, PlayerReferenceManager>();

	public UnityEvent<ulong, PlayerReferenceManager> OnOwnerRegistered = new UnityEvent<ulong, PlayerReferenceManager>();

	public UnityEvent<ulong, PlayerReferenceManager> PlayerUnregistered = new UnityEvent<ulong, PlayerReferenceManager>();

	private readonly Dictionary<ulong, PlayerReferenceManager> playerObjectMap = new Dictionary<ulong, PlayerReferenceManager>();

	public int CurrentPlayerCount => playerObjectMap.Keys.Count;

	public List<ulong> CurrentPlayerGuids => new List<ulong>(playerObjectMap.Keys);

	public void RegisterPlayer(ulong clientId, PlayerReferenceManager playerReferenceManager)
	{
		Debug.Log($"Registering player for clientId {clientId}");
		if (playerObjectMap.ContainsKey(clientId))
		{
			if (NetworkManager.Singleton.IsServer)
			{
				UnityEngine.Object.Destroy(playerReferenceManager.gameObject);
			}
			Debug.LogException(new Exception($"Player Manager already has a player registered for ID {clientId}"));
			playerObjectMap[clientId] = playerReferenceManager;
		}
		else
		{
			playerObjectMap.Add(clientId, playerReferenceManager);
		}
		OnNewPlayerRegistered.Invoke(clientId, playerReferenceManager);
		if (playerReferenceManager.IsOwner)
		{
			OnOwnerRegistered.Invoke(clientId, playerReferenceManager);
		}
	}

	public void UnregisterPlayer(ulong clientId, PlayerReferenceManager playerReferenceManager)
	{
		Debug.Log($"Unregistering player for clientId {clientId}");
		if (playerObjectMap.ContainsKey(clientId))
		{
			if (playerReferenceManager == playerObjectMap[clientId])
			{
				playerObjectMap.Remove(clientId);
				PlayerUnregistered.Invoke(clientId, playerReferenceManager);
			}
		}
		else
		{
			Debug.LogException(new Exception($"Player Manager does not have a player registered for ID {clientId}"));
		}
	}

	public PlayerReferenceManager GetPlayerObject(ulong clientId)
	{
		if (playerObjectMap.ContainsKey(clientId))
		{
			return playerObjectMap[clientId];
		}
		return null;
	}

	public List<ulong> GetPlayerIds()
	{
		return new List<ulong>(playerObjectMap.Keys);
	}

	public PlayerReferenceManager GetLocalPlayerObject()
	{
		ulong localClientId = NetworkManager.Singleton.LocalClientId;
		return GetPlayerObject(localClientId);
	}

	public bool IsPlayerInitialized(ulong clientId)
	{
		if (playerObjectMap.ContainsKey(clientId))
		{
			return playerObjectMap[clientId] != null;
		}
		return false;
	}

	public PlayerReferenceManager[] GetPlayerObjects()
	{
		return playerObjectMap.Values.ToArray();
	}
}
