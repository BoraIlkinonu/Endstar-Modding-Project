using System.Collections;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class RigidbodyWorldFalloffController : NetworkBehaviour
{
	public enum WorldFallOffActionType
	{
		Destroy
	}

	[SerializeField]
	private Rigidbody targetRigidbody;

	[SerializeField]
	private WorldFallOffActionType action;

	public void CheckFallOffStage()
	{
		if (base.IsServer && (bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && targetRigidbody.position.y < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageResetHeight)
		{
			DestroyPropClientRpc();
		}
	}

	[ClientRpc]
	public void DestroyPropClientRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(2053186936u, clientRpcParams, RpcDelivery.Reliable);
				__endSendClientRpc(ref bufferWriter, 2053186936u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				StartCoroutine(DestroyRoutine());
			}
		}
	}

	private IEnumerator DestroyRoutine()
	{
		MonoBehaviourSingleton<StageManager>.Instance.PropDestroyed(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject));
		yield return null;
		yield return new WaitForSeconds(1f);
		if (base.IsServer)
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2053186936u, __rpc_handler_2053186936, "DestroyPropClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2053186936(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((RigidbodyWorldFalloffController)target).DestroyPropClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "RigidbodyWorldFalloffController";
	}
}
