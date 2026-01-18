using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000CC RID: 204
	public class GameplayMessagingManager : NetworkBehaviourSingleton<GameplayMessagingManager>
	{
		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x06000405 RID: 1029 RVA: 0x00016276 File Offset: 0x00014476
		// (set) Token: 0x06000406 RID: 1030 RVA: 0x0001627E File Offset: 0x0001447E
		public bool LocalClientDoneLoadingScope { get; private set; }

		// Token: 0x06000407 RID: 1031 RVA: 0x00016288 File Offset: 0x00014488
		private void Start()
		{
			if (!NetworkManager.Singleton.IsServer)
			{
				NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("NetState", new CustomMessagingManager.HandleNamedMessageDelegate(this.HandleNetStateReceived));
				NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("AiState", new CustomMessagingManager.HandleNamedMessageDelegate(GameplayMessagingManager.HandleAiStateReceived));
				NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("RigidBodyState", new CustomMessagingManager.HandleNamedMessageDelegate(this.HandleRigidbodyStateReceived));
			}
			else
			{
				NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("NetInput", new CustomMessagingManager.HandleNamedMessageDelegate(this.HandleNetInputReceived));
			}
			NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ScopeFinished", new CustomMessagingManager.HandleNamedMessageDelegate(this.HandleScopeFinishedReceived));
			NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ReadyToScope", new CustomMessagingManager.HandleNamedMessageDelegate(this.HandleClientReadyToScope));
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x00016364 File Offset: 0x00014564
		[ServerRpc(RequireOwnership = false)]
		public void ClientBuiltNetworkProp_ServerRpc(ulong networkPrefabId, SerializableGuid level, bool gameplay, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2506153504U, serverRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, networkPrefabId);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in level, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<bool>(in gameplay, default(FastBufferWriter.ForPrimitives));
				base.__endSendServerRpc(ref fastBufferWriter, 2506153504U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (level != MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid)
			{
				return;
			}
			MonoBehaviourSingleton<NetworkScopeManager>.Instance.HandleNetworkedPropBuiltByClient(serverRpcParams.Receive.SenderClientId, networkPrefabId);
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x000164A4 File Offset: 0x000146A4
		private void HandleClientReadyToScope(ulong senderClientId, FastBufferReader reader)
		{
			if (NetworkManager.Singleton.IsServer)
			{
				SerializableGuid serializableGuid;
				reader.ReadValue<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
				if (serializableGuid != MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MapId)
				{
					Debug.LogWarning("Client sent a ready to scope for a level that doesn't match server active level..");
				}
				NetworkScopeManager.AddConnection(senderClientId);
				NetworkScopeManager.BeginScopeChecking();
			}
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x000164FB File Offset: 0x000146FB
		public void SendNetState(PlayerReferenceManager playerReference, NetState netState)
		{
			if (NetworkManager.Singleton.IsServer && NetworkBehaviourSingleton<NetClock>.Instance.GameplayStreamActive.Value)
			{
				NetState.Send(playerReference, netState);
			}
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x00016521 File Offset: 0x00014721
		public void SendNetInput(NetInput input)
		{
			if (!NetworkManager.Singleton.IsServer && NetworkBehaviourSingleton<NetClock>.Instance.GameplayStreamActive.Value)
			{
				NetInput.Send(input);
			}
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x00016546 File Offset: 0x00014746
		public void SendRigidBodyState(NetworkRigidbodyController controller, RigidbodyState state)
		{
			if (NetworkManager.Singleton.IsServer && NetworkBehaviourSingleton<NetClock>.Instance.GameplayStreamActive.Value)
			{
				RigidbodyState.Send(controller, state);
			}
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x0001656C File Offset: 0x0001476C
		[ServerRpc(RequireOwnership = false)]
		public void RequestLevelSave_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3526146761U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 3526146761U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			bool isBatchMode = Application.isBatchMode;
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x00016648 File Offset: 0x00014848
		private void HandleScopeFinishedReceived(ulong senderClientId, FastBufferReader reader)
		{
			Debug.Log(string.Format("Network scoping finished. Timestamp: {0}", DateTime.Now.ToLocalTime()));
			this.LocalClientDoneLoadingScope = true;
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x0001667D File Offset: 0x0001487D
		public void LocalRestartLevel()
		{
			this.LocalClientDoneLoadingScope = false;
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x00016686 File Offset: 0x00014886
		public static void SendAiState(NpcState npcState, uint key)
		{
			if (NetworkManager.Singleton.IsServer)
			{
				NpcState.Send(npcState, key);
			}
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x0001669B File Offset: 0x0001489B
		private static void HandleAiStateReceived(ulong senderClientId, FastBufferReader reader)
		{
			NpcState.Receive(reader);
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x000166A3 File Offset: 0x000148A3
		private void HandleNetStateReceived(ulong senderClientId, FastBufferReader reader)
		{
			NetState.Receive(reader);
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x000166AB File Offset: 0x000148AB
		private void HandleNetInputReceived(ulong senderClientId, FastBufferReader reader)
		{
			NetInput.Receive(senderClientId, reader);
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x000166B4 File Offset: 0x000148B4
		private void HandleRigidbodyStateReceived(ulong senderClientId, FastBufferReader reader)
		{
			RigidbodyState.Receive(reader);
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x000166C4 File Offset: 0x000148C4
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x000166DC File Offset: 0x000148DC
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2506153504U, new NetworkBehaviour.RpcReceiveHandler(GameplayMessagingManager.__rpc_handler_2506153504), "ClientBuiltNetworkProp_ServerRpc");
			base.__registerRpc(3526146761U, new NetworkBehaviour.RpcReceiveHandler(GameplayMessagingManager.__rpc_handler_3526146761), "RequestLevelSave_ServerRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x0001672C File Offset: 0x0001492C
		private static void __rpc_handler_2506153504(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ulong num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameplayMessagingManager)target).ClientBuiltNetworkProp_ServerRpc(num, serializableGuid, flag, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x000167DC File Offset: 0x000149DC
		private static void __rpc_handler_3526146761(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameplayMessagingManager)target).RequestLevelSave_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x0001683B File Offset: 0x00014A3B
		protected internal override string __getTypeName()
		{
			return "GameplayMessagingManager";
		}
	}
}
