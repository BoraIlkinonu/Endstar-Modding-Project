using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class GameplayMessagingManager : NetworkBehaviourSingleton<GameplayMessagingManager>
{
	public bool LocalClientDoneLoadingScope { get; private set; }

	private void Start()
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("NetState", HandleNetStateReceived);
			NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("AiState", HandleAiStateReceived);
			NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("RigidBodyState", HandleRigidbodyStateReceived);
		}
		else
		{
			NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("NetInput", HandleNetInputReceived);
		}
		NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ScopeFinished", HandleScopeFinishedReceived);
		NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ReadyToScope", HandleClientReadyToScope);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ClientBuiltNetworkProp_ServerRpc(ulong networkPrefabId, SerializableGuid level, bool gameplay, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2506153504u, serverRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, networkPrefabId);
			bufferWriter.WriteValueSafe(in level, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in gameplay, default(FastBufferWriter.ForPrimitives));
			__endSendServerRpc(ref bufferWriter, 2506153504u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!(level != MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid))
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.HandleNetworkedPropBuiltByClient(serverRpcParams.Receive.SenderClientId, networkPrefabId);
			}
		}
	}

	private void HandleClientReadyToScope(ulong senderClientId, FastBufferReader reader)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			reader.ReadValue(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			if (value != MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MapId)
			{
				Debug.LogWarning("Client sent a ready to scope for a level that doesn't match server active level..");
			}
			NetworkScopeManager.AddConnection(senderClientId);
			NetworkScopeManager.BeginScopeChecking();
		}
	}

	public void SendNetState(PlayerReferenceManager playerReference, NetState netState)
	{
		if (NetworkManager.Singleton.IsServer && NetworkBehaviourSingleton<NetClock>.Instance.GameplayStreamActive.Value)
		{
			NetState.Send(playerReference, netState);
		}
	}

	public void SendNetInput(NetInput input)
	{
		if (!NetworkManager.Singleton.IsServer && NetworkBehaviourSingleton<NetClock>.Instance.GameplayStreamActive.Value)
		{
			NetInput.Send(input);
		}
	}

	public void SendRigidBodyState(NetworkRigidbodyController controller, RigidbodyState state)
	{
		if (NetworkManager.Singleton.IsServer && NetworkBehaviourSingleton<NetClock>.Instance.GameplayStreamActive.Value)
		{
			RigidbodyState.Send(controller, state);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void RequestLevelSave_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(3526146761u, serverRpcParams, RpcDelivery.Reliable);
				__endSendServerRpc(ref bufferWriter, 3526146761u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				_ = Application.isBatchMode;
			}
		}
	}

	private void HandleScopeFinishedReceived(ulong senderClientId, FastBufferReader reader)
	{
		Debug.Log($"Network scoping finished. Timestamp: {DateTime.Now.ToLocalTime()}");
		LocalClientDoneLoadingScope = true;
	}

	public void LocalRestartLevel()
	{
		LocalClientDoneLoadingScope = false;
	}

	public static void SendAiState(NpcState npcState, uint key)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			NpcState.Send(npcState, key);
		}
	}

	private static void HandleAiStateReceived(ulong senderClientId, FastBufferReader reader)
	{
		NpcState.Receive(reader);
	}

	private void HandleNetStateReceived(ulong senderClientId, FastBufferReader reader)
	{
		NetState.Receive(reader);
	}

	private void HandleNetInputReceived(ulong senderClientId, FastBufferReader reader)
	{
		NetInput.Receive(senderClientId, reader);
	}

	private void HandleRigidbodyStateReceived(ulong senderClientId, FastBufferReader reader)
	{
		RigidbodyState.Receive(reader);
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2506153504u, __rpc_handler_2506153504, "ClientBuiltNetworkProp_ServerRpc");
		__registerRpc(3526146761u, __rpc_handler_3526146761, "RequestLevelSave_ServerRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2506153504(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value);
			reader.ReadValueSafe(out SerializableGuid value2, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value3, default(FastBufferWriter.ForPrimitives));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((GameplayMessagingManager)target).ClientBuiltNetworkProp_ServerRpc(value, value2, value3, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3526146761(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((GameplayMessagingManager)target).RequestLevelSave_ServerRpc(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "GameplayMessagingManager";
	}
}
