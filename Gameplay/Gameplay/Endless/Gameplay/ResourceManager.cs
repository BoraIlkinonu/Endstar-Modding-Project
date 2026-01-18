using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000C6 RID: 198
	public class ResourceManager : NetworkBehaviourSingleton<ResourceManager>
	{
		// Token: 0x060003B4 RID: 948 RVA: 0x000142E4 File Offset: 0x000124E4
		public ResourceCollectionRule GetCollectionRule(ResourceLibraryReference resource)
		{
			ResourceCollectionRule resourceCollectionRule;
			if (this.collectionRules.TryGetValue(resource, out resourceCollectionRule))
			{
				return resourceCollectionRule;
			}
			return ResourceCollectionRule.Duplicated;
		}

		// Token: 0x060003B5 RID: 949 RVA: 0x00014304 File Offset: 0x00012504
		public bool SetCollectionRule(ResourceLibraryReference resource, ResourceCollectionRule newRule)
		{
			return this.collectionRules.TryAdd(resource, newRule);
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x00014314 File Offset: 0x00012514
		public void ResourceCollected(ResourceLibraryReference resource, int quantity, ulong clientId)
		{
			if (!this.currentGameResources.Contains(resource))
			{
				this.currentGameResources.Add(resource);
			}
			if (quantity <= 0)
			{
				return;
			}
			switch (this.GetCollectionRule(resource))
			{
			case ResourceCollectionRule.Solo:
				this.ResourceCollectedForSingleUser(resource, clientId, quantity);
				return;
			case ResourceCollectionRule.Duplicated:
			{
				if (!this.sharedResourceAmount.TryAdd(resource, quantity))
				{
					Dictionary<ResourceLibraryReference, int> dictionary = this.sharedResourceAmount;
					dictionary[resource] += quantity;
				}
				using (List<ulong>.Enumerator enumerator = MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerGuids.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ulong num = enumerator.Current;
						this.ResourceCollectedForSingleUser(resource, num, quantity);
					}
					return;
				}
				break;
			}
			case ResourceCollectionRule.SharedPool:
				break;
			default:
				return;
			}
			if (!this.sharedResourceAmount.TryAdd(resource, quantity))
			{
				Dictionary<ResourceLibraryReference, int> dictionary = this.sharedResourceAmount;
				dictionary[resource] += quantity;
			}
			foreach (ulong num2 in MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerGuids)
			{
				this.NotifyPlayerOfCountChanged(resource, num2, true);
			}
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x00014450 File Offset: 0x00012650
		public bool AttemptSpendResource(ResourceLibraryReference resource, int quantity, ulong clientId)
		{
			if (quantity < 1)
			{
				return true;
			}
			ResourceCollectionRule collectionRule = this.GetCollectionRule(resource);
			if (collectionRule > ResourceCollectionRule.Duplicated)
			{
				if (collectionRule != ResourceCollectionRule.SharedPool)
				{
					throw new NotImplementedException();
				}
				if (!this.sharedResourceAmount.ContainsKey(resource))
				{
					return false;
				}
				if (this.sharedResourceAmount[resource] >= quantity)
				{
					Dictionary<ResourceLibraryReference, int> dictionary = this.sharedResourceAmount;
					dictionary[resource] -= quantity;
					foreach (ulong num in MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerGuids)
					{
						this.NotifyPlayerOfCountChanged(resource, num, true);
					}
					return true;
				}
				return false;
			}
			else
			{
				this.resourceCountPerPlayer.TryAdd(clientId, new Dictionary<ResourceLibraryReference, int>());
				if (!this.resourceCountPerPlayer[clientId].ContainsKey(resource))
				{
					return false;
				}
				if (this.resourceCountPerPlayer[clientId][resource] >= quantity)
				{
					Dictionary<ResourceLibraryReference, int> dictionary = this.resourceCountPerPlayer[clientId];
					dictionary[resource] -= quantity;
					this.NotifyPlayerOfCountChanged(resource, clientId, false);
					return true;
				}
				return false;
			}
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x00014570 File Offset: 0x00012770
		public void ClearAllResourcesForAllPlayers()
		{
			foreach (ResourceLibraryReference resourceLibraryReference in this.currentGameResources)
			{
				this.ClearResourceForAllPlayers(resourceLibraryReference);
			}
			this.currentGameResources.Clear();
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x000145D0 File Offset: 0x000127D0
		public void ClearAllResourcesForPlayer(ulong playerId)
		{
			foreach (ResourceLibraryReference resourceLibraryReference in this.currentGameResources)
			{
				this.ClearResourceForPlayer(resourceLibraryReference, playerId);
			}
		}

		// Token: 0x060003BA RID: 954 RVA: 0x00014624 File Offset: 0x00012824
		public void ClearResourceForPlayer(ResourceLibraryReference resource, ulong clientId)
		{
			ResourceCollectionRule collectionRule = this.GetCollectionRule(resource);
			Dictionary<ResourceLibraryReference, int> dictionary;
			if (this.resourceCountPerPlayer.TryGetValue(clientId, out dictionary) && dictionary.ContainsKey(resource) && dictionary[resource] != 0)
			{
				dictionary[resource] = 0;
				if (collectionRule != ResourceCollectionRule.SharedPool)
				{
					this.NotifyPlayerOfCountChanged(resource, clientId, false);
				}
			}
			if (collectionRule == ResourceCollectionRule.SharedPool)
			{
				this.ClearSharedPoolOfResource(resource);
			}
		}

		// Token: 0x060003BB RID: 955 RVA: 0x0001467C File Offset: 0x0001287C
		public void ClearResourceForAllPlayers(ResourceLibraryReference resource)
		{
			ResourceCollectionRule collectionRule = this.GetCollectionRule(resource);
			foreach (KeyValuePair<ulong, Dictionary<ResourceLibraryReference, int>> keyValuePair in this.resourceCountPerPlayer)
			{
				if (keyValuePair.Value != null && keyValuePair.Value.ContainsKey(resource))
				{
					keyValuePair.Value[resource] = 0;
					if (collectionRule != ResourceCollectionRule.SharedPool)
					{
						this.NotifyPlayerOfCountChanged(resource, keyValuePair.Key, false);
					}
				}
			}
			this.ClearSharedPoolOfResource(resource);
		}

		// Token: 0x060003BC RID: 956 RVA: 0x00014710 File Offset: 0x00012910
		private void ClearSharedPoolOfResource(ResourceLibraryReference resource)
		{
			if (this.sharedResourceAmount.ContainsKey(resource))
			{
				this.sharedResourceAmount[resource] = 0;
				if (this.GetCollectionRule(resource) == ResourceCollectionRule.SharedPool)
				{
					foreach (ulong num in MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerGuids)
					{
						this.NotifyPlayerOfCountChanged(resource, num, true);
					}
				}
			}
		}

		// Token: 0x060003BD RID: 957 RVA: 0x00014790 File Offset: 0x00012990
		private void ResourceCollectedForSingleUser(ResourceLibraryReference resource, ulong clientId, int quantity)
		{
			this.resourceCountPerPlayer.TryAdd(clientId, new Dictionary<ResourceLibraryReference, int>());
			if (!this.resourceCountPerPlayer[clientId].TryAdd(resource, quantity))
			{
				Dictionary<ResourceLibraryReference, int> dictionary = this.resourceCountPerPlayer[clientId];
				dictionary[resource] += quantity;
			}
			this.NotifyPlayerOfCountChanged(resource, clientId, false);
		}

		// Token: 0x060003BE RID: 958 RVA: 0x000147EC File Offset: 0x000129EC
		private void NotifyPlayerOfCountChanged(ResourceLibraryReference resource, ulong clientId, bool useSharedPool = false)
		{
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new ulong[] { clientId }
				}
			};
			if (useSharedPool)
			{
				this.HandleCountChanged_ClientRpc(resource, clientId, this.sharedResourceAmount[resource], clientRpcParams);
				return;
			}
			this.HandleCountChanged_ClientRpc(resource, clientId, this.resourceCountPerPlayer[clientId][resource], clientRpcParams);
		}

		// Token: 0x060003BF RID: 959 RVA: 0x00014858 File Offset: 0x00012A58
		public bool HasResourceCheck(ResourceLibraryReference resource, ulong playerId, int quantity)
		{
			if (quantity < 1)
			{
				return true;
			}
			ResourceCollectionRule collectionRule = this.GetCollectionRule(resource);
			if (collectionRule <= ResourceCollectionRule.Duplicated)
			{
				this.resourceCountPerPlayer.TryAdd(playerId, new Dictionary<ResourceLibraryReference, int>());
				return this.resourceCountPerPlayer[playerId].ContainsKey(resource) && this.resourceCountPerPlayer[playerId][resource] >= quantity;
			}
			if (collectionRule != ResourceCollectionRule.SharedPool)
			{
				throw new NotImplementedException();
			}
			return this.sharedResourceAmount.ContainsKey(resource) && this.sharedResourceAmount[resource] >= quantity;
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x000148E8 File Offset: 0x00012AE8
		[ClientRpc]
		private void HandleCountChanged_ClientRpc(ResourceLibraryReference resource, ulong clientId, int amount, ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(895863218U, rpcParams, RpcDelivery.Reliable);
				bool flag = resource != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<ResourceLibraryReference>(in resource, default(FastBufferWriter.ForNetworkSerializable));
				}
				BytePacker.WriteValueBitPacked(fastBufferWriter, clientId);
				BytePacker.WriteValueBitPacked(fastBufferWriter, amount);
				base.__endSendClientRpc(ref fastBufferWriter, 895863218U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.OnLocalCoinAmountUpdated.Invoke(amount);
		}

		// Token: 0x060003C1 RID: 961 RVA: 0x00014A2C File Offset: 0x00012C2C
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (base.IsServer)
			{
				MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.HandleNewPlayerJoinedNew));
				MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.ResetResourceManager));
			}
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x00014A80 File Offset: 0x00012C80
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			if (MonoBehaviourSingleton<PlayerManager>.Instance)
			{
				MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.RemoveListener(new UnityAction<ulong, PlayerReferenceManager>(this.HandleNewPlayerJoinedNew));
			}
			if (MonoBehaviourSingleton<GameplayManager>.Instance)
			{
				MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.RemoveListener(new UnityAction(this.ResetResourceManager));
			}
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x00014AE1 File Offset: 0x00012CE1
		private void ResetResourceManager()
		{
			this.resourceCountPerPlayer.Clear();
			this.sharedResourceAmount.Clear();
			this.collectionRules.Clear();
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x00014B04 File Offset: 0x00012D04
		private void HandleNewPlayerJoinedNew(ulong playerId, PlayerReferenceManager arg1)
		{
			foreach (ResourceLibraryReference resourceLibraryReference in this.sharedResourceAmount.Keys)
			{
				ResourceCollectionRule collectionRule = this.GetCollectionRule(resourceLibraryReference);
				if (collectionRule == ResourceCollectionRule.Duplicated)
				{
					this.ResourceCollectedForSingleUser(resourceLibraryReference, playerId, this.sharedResourceAmount[resourceLibraryReference]);
				}
				else if (collectionRule == ResourceCollectionRule.SharedPool)
				{
					this.NotifyPlayerOfCountChanged(resourceLibraryReference, playerId, true);
				}
			}
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x00014BE4 File Offset: 0x00012DE4
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x00014BFA File Offset: 0x00012DFA
		protected override void __initializeRpcs()
		{
			base.__registerRpc(895863218U, new NetworkBehaviour.RpcReceiveHandler(ResourceManager.__rpc_handler_895863218), "HandleCountChanged_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x00014C20 File Offset: 0x00012E20
		private static void __rpc_handler_895863218(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ResourceLibraryReference resourceLibraryReference = null;
			if (flag)
			{
				reader.ReadValueSafe<ResourceLibraryReference>(out resourceLibraryReference, default(FastBufferWriter.ForNetworkSerializable));
			}
			ulong num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			int num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ResourceManager)target).HandleCountChanged_ClientRpc(resourceLibraryReference, num, num2, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003C9 RID: 969 RVA: 0x00014CEA File Offset: 0x00012EEA
		protected internal override string __getTypeName()
		{
			return "ResourceManager";
		}

		// Token: 0x04000370 RID: 880
		private Dictionary<ulong, Dictionary<ResourceLibraryReference, int>> resourceCountPerPlayer = new Dictionary<ulong, Dictionary<ResourceLibraryReference, int>>();

		// Token: 0x04000371 RID: 881
		private Dictionary<ResourceLibraryReference, int> sharedResourceAmount = new Dictionary<ResourceLibraryReference, int>();

		// Token: 0x04000372 RID: 882
		private Dictionary<ResourceLibraryReference, ResourceCollectionRule> collectionRules = new Dictionary<ResourceLibraryReference, ResourceCollectionRule>();

		// Token: 0x04000373 RID: 883
		private List<SerializableGuid> itemsPerPlayer = new List<SerializableGuid>();

		// Token: 0x04000374 RID: 884
		private Dictionary<SerializableGuid, List<Item>> inWorldItems = new Dictionary<SerializableGuid, List<Item>>();

		// Token: 0x04000375 RID: 885
		public UnityEvent<int> OnLocalCoinAmountUpdated = new UnityEvent<int>();

		// Token: 0x04000376 RID: 886
		private HashSet<ResourceLibraryReference> currentGameResources = new HashSet<ResourceLibraryReference>();
	}
}
