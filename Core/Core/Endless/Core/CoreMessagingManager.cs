using System;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Core
{
	// Token: 0x02000026 RID: 38
	public class CoreMessagingManager : NetworkBehaviourSingleton<CoreMessagingManager>
	{
		// Token: 0x06000081 RID: 129 RVA: 0x00004728 File Offset: 0x00002928
		[ServerRpc(RequireOwnership = false)]
		public void RequestGoToGameplay_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1860339214U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 1860339214U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (Application.isBatchMode)
			{
				NetworkBehaviourSingleton<GameStateManager>.Instance.FlipGameState();
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x0000480C File Offset: 0x00002A0C
		[ServerRpc(RequireOwnership = false)]
		public void RequestGoToCreator_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2014686332U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 2014686332U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (Application.isBatchMode)
			{
				NetworkBehaviourSingleton<GameStateManager>.Instance.FlipGameState();
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x000048F8 File Offset: 0x00002AF8
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00004910 File Offset: 0x00002B10
		protected override void __initializeRpcs()
		{
			base.__registerRpc(1860339214U, new NetworkBehaviour.RpcReceiveHandler(CoreMessagingManager.__rpc_handler_1860339214), "RequestGoToGameplay_ServerRpc");
			base.__registerRpc(2014686332U, new NetworkBehaviour.RpcReceiveHandler(CoreMessagingManager.__rpc_handler_2014686332), "RequestGoToCreator_ServerRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00004960 File Offset: 0x00002B60
		private static void __rpc_handler_1860339214(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CoreMessagingManager)target).RequestGoToGameplay_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x000049C0 File Offset: 0x00002BC0
		private static void __rpc_handler_2014686332(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CoreMessagingManager)target).RequestGoToCreator_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00004A1F File Offset: 0x00002C1F
		protected internal override string __getTypeName()
		{
			return "CoreMessagingManager";
		}
	}
}
