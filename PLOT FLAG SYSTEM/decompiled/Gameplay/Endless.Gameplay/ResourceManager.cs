using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class ResourceManager : NetworkBehaviourSingleton<ResourceManager>
{
	private Dictionary<ulong, Dictionary<ResourceLibraryReference, int>> resourceCountPerPlayer = new Dictionary<ulong, Dictionary<ResourceLibraryReference, int>>();

	private Dictionary<ResourceLibraryReference, int> sharedResourceAmount = new Dictionary<ResourceLibraryReference, int>();

	private Dictionary<ResourceLibraryReference, ResourceCollectionRule> collectionRules = new Dictionary<ResourceLibraryReference, ResourceCollectionRule>();

	private List<SerializableGuid> itemsPerPlayer = new List<SerializableGuid>();

	private Dictionary<SerializableGuid, List<Item>> inWorldItems = new Dictionary<SerializableGuid, List<Item>>();

	public UnityEvent<int> OnLocalCoinAmountUpdated = new UnityEvent<int>();

	private HashSet<ResourceLibraryReference> currentGameResources = new HashSet<ResourceLibraryReference>();

	public ResourceCollectionRule GetCollectionRule(ResourceLibraryReference resource)
	{
		if (collectionRules.TryGetValue(resource, out var value))
		{
			return value;
		}
		return ResourceCollectionRule.Duplicated;
	}

	public bool SetCollectionRule(ResourceLibraryReference resource, ResourceCollectionRule newRule)
	{
		return collectionRules.TryAdd(resource, newRule);
	}

	public void ResourceCollected(ResourceLibraryReference resource, int quantity, ulong clientId)
	{
		if (!currentGameResources.Contains(resource))
		{
			currentGameResources.Add(resource);
		}
		if (quantity <= 0)
		{
			return;
		}
		switch (GetCollectionRule(resource))
		{
		case ResourceCollectionRule.Solo:
			ResourceCollectedForSingleUser(resource, clientId, quantity);
			break;
		case ResourceCollectionRule.Duplicated:
			if (!sharedResourceAmount.TryAdd(resource, quantity))
			{
				sharedResourceAmount[resource] += quantity;
			}
			{
				foreach (ulong currentPlayerGuid in MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerGuids)
				{
					ResourceCollectedForSingleUser(resource, currentPlayerGuid, quantity);
				}
				break;
			}
		case ResourceCollectionRule.SharedPool:
			if (!sharedResourceAmount.TryAdd(resource, quantity))
			{
				sharedResourceAmount[resource] += quantity;
			}
			{
				foreach (ulong currentPlayerGuid2 in MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerGuids)
				{
					NotifyPlayerOfCountChanged(resource, currentPlayerGuid2, useSharedPool: true);
				}
				break;
			}
		}
	}

	public bool AttemptSpendResource(ResourceLibraryReference resource, int quantity, ulong clientId)
	{
		if (quantity < 1)
		{
			return true;
		}
		switch (GetCollectionRule(resource))
		{
		case ResourceCollectionRule.Solo:
		case ResourceCollectionRule.Duplicated:
			resourceCountPerPlayer.TryAdd(clientId, new Dictionary<ResourceLibraryReference, int>());
			if (!resourceCountPerPlayer[clientId].ContainsKey(resource))
			{
				return false;
			}
			if (resourceCountPerPlayer[clientId][resource] >= quantity)
			{
				resourceCountPerPlayer[clientId][resource] -= quantity;
				NotifyPlayerOfCountChanged(resource, clientId);
				return true;
			}
			return false;
		case ResourceCollectionRule.SharedPool:
			if (!sharedResourceAmount.ContainsKey(resource))
			{
				return false;
			}
			if (sharedResourceAmount[resource] >= quantity)
			{
				sharedResourceAmount[resource] -= quantity;
				foreach (ulong currentPlayerGuid in MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerGuids)
				{
					NotifyPlayerOfCountChanged(resource, currentPlayerGuid, useSharedPool: true);
				}
				return true;
			}
			return false;
		default:
			throw new NotImplementedException();
		}
	}

	public void ClearAllResourcesForAllPlayers()
	{
		foreach (ResourceLibraryReference currentGameResource in currentGameResources)
		{
			ClearResourceForAllPlayers(currentGameResource);
		}
		currentGameResources.Clear();
	}

	public void ClearAllResourcesForPlayer(ulong playerId)
	{
		foreach (ResourceLibraryReference currentGameResource in currentGameResources)
		{
			ClearResourceForPlayer(currentGameResource, playerId);
		}
	}

	public void ClearResourceForPlayer(ResourceLibraryReference resource, ulong clientId)
	{
		ResourceCollectionRule collectionRule = GetCollectionRule(resource);
		if (resourceCountPerPlayer.TryGetValue(clientId, out var value) && value.ContainsKey(resource) && value[resource] != 0)
		{
			value[resource] = 0;
			if (collectionRule != ResourceCollectionRule.SharedPool)
			{
				NotifyPlayerOfCountChanged(resource, clientId);
			}
		}
		if (collectionRule == ResourceCollectionRule.SharedPool)
		{
			ClearSharedPoolOfResource(resource);
		}
	}

	public void ClearResourceForAllPlayers(ResourceLibraryReference resource)
	{
		ResourceCollectionRule collectionRule = GetCollectionRule(resource);
		foreach (KeyValuePair<ulong, Dictionary<ResourceLibraryReference, int>> item in resourceCountPerPlayer)
		{
			if (item.Value != null && item.Value.ContainsKey(resource))
			{
				item.Value[resource] = 0;
				if (collectionRule != ResourceCollectionRule.SharedPool)
				{
					NotifyPlayerOfCountChanged(resource, item.Key);
				}
			}
		}
		ClearSharedPoolOfResource(resource);
	}

	private void ClearSharedPoolOfResource(ResourceLibraryReference resource)
	{
		if (!sharedResourceAmount.ContainsKey(resource))
		{
			return;
		}
		sharedResourceAmount[resource] = 0;
		if (GetCollectionRule(resource) != ResourceCollectionRule.SharedPool)
		{
			return;
		}
		foreach (ulong currentPlayerGuid in MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerGuids)
		{
			NotifyPlayerOfCountChanged(resource, currentPlayerGuid, useSharedPool: true);
		}
	}

	private void ResourceCollectedForSingleUser(ResourceLibraryReference resource, ulong clientId, int quantity)
	{
		resourceCountPerPlayer.TryAdd(clientId, new Dictionary<ResourceLibraryReference, int>());
		if (!resourceCountPerPlayer[clientId].TryAdd(resource, quantity))
		{
			resourceCountPerPlayer[clientId][resource] += quantity;
		}
		NotifyPlayerOfCountChanged(resource, clientId);
	}

	private void NotifyPlayerOfCountChanged(ResourceLibraryReference resource, ulong clientId, bool useSharedPool = false)
	{
		ClientRpcParams rpcParams = new ClientRpcParams
		{
			Send = new ClientRpcSendParams
			{
				TargetClientIds = new ulong[1] { clientId }
			}
		};
		if (useSharedPool)
		{
			HandleCountChanged_ClientRpc(resource, clientId, sharedResourceAmount[resource], rpcParams);
		}
		else
		{
			HandleCountChanged_ClientRpc(resource, clientId, resourceCountPerPlayer[clientId][resource], rpcParams);
		}
	}

	public bool HasResourceCheck(ResourceLibraryReference resource, ulong playerId, int quantity)
	{
		if (quantity < 1)
		{
			return true;
		}
		switch (GetCollectionRule(resource))
		{
		case ResourceCollectionRule.Solo:
		case ResourceCollectionRule.Duplicated:
			resourceCountPerPlayer.TryAdd(playerId, new Dictionary<ResourceLibraryReference, int>());
			if (!resourceCountPerPlayer[playerId].ContainsKey(resource))
			{
				return false;
			}
			return resourceCountPerPlayer[playerId][resource] >= quantity;
		case ResourceCollectionRule.SharedPool:
			if (!sharedResourceAmount.ContainsKey(resource))
			{
				return false;
			}
			return sharedResourceAmount[resource] >= quantity;
		default:
			throw new NotImplementedException();
		}
	}

	[ClientRpc]
	private void HandleCountChanged_ClientRpc(ResourceLibraryReference resource, ulong clientId, int amount, ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(895863218u, rpcParams, RpcDelivery.Reliable);
			bool value = (object)resource != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(in resource, default(FastBufferWriter.ForNetworkSerializable));
			}
			BytePacker.WriteValueBitPacked(bufferWriter, clientId);
			BytePacker.WriteValueBitPacked(bufferWriter, amount);
			__endSendClientRpc(ref bufferWriter, 895863218u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			OnLocalCoinAmountUpdated.Invoke(amount);
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (base.IsServer)
		{
			MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(HandleNewPlayerJoinedNew);
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(ResetResourceManager);
		}
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		if ((bool)MonoBehaviourSingleton<PlayerManager>.Instance)
		{
			MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.RemoveListener(HandleNewPlayerJoinedNew);
		}
		if ((bool)MonoBehaviourSingleton<GameplayManager>.Instance)
		{
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.RemoveListener(ResetResourceManager);
		}
	}

	private void ResetResourceManager()
	{
		resourceCountPerPlayer.Clear();
		sharedResourceAmount.Clear();
		collectionRules.Clear();
	}

	private void HandleNewPlayerJoinedNew(ulong playerId, PlayerReferenceManager arg1)
	{
		foreach (ResourceLibraryReference key in sharedResourceAmount.Keys)
		{
			switch (GetCollectionRule(key))
			{
			case ResourceCollectionRule.Duplicated:
				ResourceCollectedForSingleUser(key, playerId, sharedResourceAmount[key]);
				break;
			case ResourceCollectionRule.SharedPool:
				NotifyPlayerOfCountChanged(key, playerId, useSharedPool: true);
				break;
			}
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(895863218u, __rpc_handler_895863218, "HandleCountChanged_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_895863218(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			ResourceLibraryReference value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value3);
			ByteUnpacker.ReadValueBitPacked(reader, out int value4);
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ResourceManager)target).HandleCountChanged_ClientRpc(value2, value3, value4, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "ResourceManager";
	}
}
