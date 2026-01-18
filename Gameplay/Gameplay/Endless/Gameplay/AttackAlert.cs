using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200012E RID: 302
	public class AttackAlert : NetworkBehaviour
	{
		// Token: 0x060006CC RID: 1740 RVA: 0x00021514 File Offset: 0x0001F714
		[ClientRpc]
		private void ImminentlyAttackingClientRpc(uint frameDifference, bool forceReplay)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1539829234U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, frameDifference);
				fastBufferWriter.WriteValueSafe<bool>(in forceReplay, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 1539829234U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (!this.attackAlert.isPlaying || forceReplay)
			{
				float num = Mathf.InverseLerp(0f, 20f, frameDifference);
				this.attackAlert.time = 1f - num;
				this.attackAlert.Play();
				base.StartCoroutine(this.StopPlaying(num));
			}
		}

		// Token: 0x060006CD RID: 1741 RVA: 0x0002165F File Offset: 0x0001F85F
		public void ImminentlyAttacking(uint frameDifference, bool forceReplay = false)
		{
			this.ImminentlyAttackingClientRpc(frameDifference, forceReplay);
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x00021669 File Offset: 0x0001F869
		public void ImminentlyAttacking(bool forceReplay = false)
		{
			this.ImminentlyAttacking(0U, forceReplay);
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x00021673 File Offset: 0x0001F873
		private IEnumerator StopPlaying(float time = 1f)
		{
			yield return new WaitForSeconds(time);
			this.attackAlert.Stop();
			yield break;
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x0002168C File Offset: 0x0001F88C
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x000216A2 File Offset: 0x0001F8A2
		protected override void __initializeRpcs()
		{
			base.__registerRpc(1539829234U, new NetworkBehaviour.RpcReceiveHandler(AttackAlert.__rpc_handler_1539829234), "ImminentlyAttackingClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x000216C8 File Offset: 0x0001F8C8
		private static void __rpc_handler_1539829234(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((AttackAlert)target).ImminentlyAttackingClientRpc(num, flag);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x00021749 File Offset: 0x0001F949
		protected internal override string __getTypeName()
		{
			return "AttackAlert";
		}

		// Token: 0x04000591 RID: 1425
		[SerializeField]
		private ParticleSystem attackAlert;
	}
}
