using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class AttackAlert : NetworkBehaviour
{
	[SerializeField]
	private ParticleSystem attackAlert;

	[ClientRpc]
	private void ImminentlyAttackingClientRpc(uint frameDifference, bool forceReplay)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1539829234u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, frameDifference);
			bufferWriter.WriteValueSafe(in forceReplay, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 1539829234u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!attackAlert.isPlaying || forceReplay)
			{
				float num = Mathf.InverseLerp(0f, 20f, frameDifference);
				attackAlert.time = 1f - num;
				attackAlert.Play();
				StartCoroutine(StopPlaying(num));
			}
		}
	}

	public void ImminentlyAttacking(uint frameDifference, bool forceReplay = false)
	{
		ImminentlyAttackingClientRpc(frameDifference, forceReplay);
	}

	public void ImminentlyAttacking(bool forceReplay = false)
	{
		ImminentlyAttacking(0u, forceReplay);
	}

	private IEnumerator StopPlaying(float time = 1f)
	{
		yield return new WaitForSeconds(time);
		attackAlert.Stop();
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1539829234u, __rpc_handler_1539829234, "ImminentlyAttackingClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1539829234(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out uint value);
			reader.ReadValueSafe(out bool value2, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((AttackAlert)target).ImminentlyAttackingClientRpc(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "AttackAlert";
	}
}
