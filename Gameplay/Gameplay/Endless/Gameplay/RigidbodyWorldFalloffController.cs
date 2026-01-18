using System;
using System.Collections;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200011B RID: 283
	public class RigidbodyWorldFalloffController : NetworkBehaviour
	{
		// Token: 0x06000662 RID: 1634 RVA: 0x0001F558 File Offset: 0x0001D758
		public void CheckFallOffStage()
		{
			if (!base.IsServer)
			{
				return;
			}
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && this.targetRigidbody.position.y < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageResetHeight)
			{
				this.DestroyPropClientRpc();
			}
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x0001F5A8 File Offset: 0x0001D7A8
		[ClientRpc]
		public void DestroyPropClientRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2053186936U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 2053186936U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			base.StartCoroutine(this.DestroyRoutine());
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x0001F688 File Offset: 0x0001D888
		private IEnumerator DestroyRoutine()
		{
			MonoBehaviourSingleton<StageManager>.Instance.PropDestroyed(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject));
			yield return null;
			yield return new WaitForSeconds(1f);
			if (base.IsServer)
			{
				global::UnityEngine.Object.Destroy(base.gameObject);
			}
			yield break;
		}

		// Token: 0x06000666 RID: 1638 RVA: 0x0001F698 File Offset: 0x0001D898
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x0001F6AE File Offset: 0x0001D8AE
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2053186936U, new NetworkBehaviour.RpcReceiveHandler(RigidbodyWorldFalloffController.__rpc_handler_2053186936), "DestroyPropClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06000668 RID: 1640 RVA: 0x0001F6D4 File Offset: 0x0001D8D4
		private static void __rpc_handler_2053186936(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((RigidbodyWorldFalloffController)target).DestroyPropClientRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000669 RID: 1641 RVA: 0x0001F725 File Offset: 0x0001D925
		protected internal override string __getTypeName()
		{
			return "RigidbodyWorldFalloffController";
		}

		// Token: 0x040004D3 RID: 1235
		[SerializeField]
		private Rigidbody targetRigidbody;

		// Token: 0x040004D4 RID: 1236
		[SerializeField]
		private RigidbodyWorldFalloffController.WorldFallOffActionType action;

		// Token: 0x0200011C RID: 284
		public enum WorldFallOffActionType
		{
			// Token: 0x040004D6 RID: 1238
			Destroy
		}
	}
}
