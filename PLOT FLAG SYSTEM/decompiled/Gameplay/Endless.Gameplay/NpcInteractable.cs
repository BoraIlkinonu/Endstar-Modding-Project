using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class NpcInteractable : InteractableBase, IStartSubscriber
{
	[SerializeField]
	private NpcEntity npcEntity;

	[SerializeField]
	private List<Collider> colliders;

	private bool isInteractable;

	public List<InteractorBase> ActiveInteractors { get; } = new List<InteractorBase>();

	private IInteractionBehavior InteractionBehavior => npcEntity.InteractionBehavior;

	private bool IsInteractable
	{
		get
		{
			return isInteractable;
		}
		set
		{
			isInteractable = value;
			UpdateInteractableState();
		}
	}

	public override void InteractionStopped(InteractorBase interactor)
	{
		if (ActiveInteractors.Remove(interactor))
		{
			InteractionBehavior?.InteractionStopped(interactor.ContextObject, npcEntity.Context);
		}
	}

	public override void SetAllInteractablesEnabled(bool interactable)
	{
		base.SetAllInteractablesEnabled(interactable);
		IsInteractable = interactable;
	}

	private void UpdateInteractableState()
	{
		bool colliderEnabledState_ClientRpc = IsInteractable && npcEntity.CombatState == NpcEnum.CombatState.None && !npcEntity.IsDowned;
		SetColliderEnabledState_ClientRpc(colliderEnabledState_ClientRpc);
	}

	[ClientRpc]
	private void SetColliderEnabledState_ClientRpc(bool newState)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1453106355u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in newState, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 1453106355u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		foreach (Collider collider in colliders)
		{
			collider.enabled = newState;
		}
	}

	protected override bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
	{
		if (npcEntity.CombatState != NpcEnum.CombatState.None)
		{
			return false;
		}
		return InteractionBehavior?.AttemptInteractServerLogic(interactor.ContextObject, npcEntity.Context, colliderIndex) ?? false;
	}

	protected override void InteractServerLogic(InteractorBase interactor, int colliderIndex)
	{
		if (!ActiveInteractors.Contains(interactor))
		{
			ActiveInteractors.Add(interactor);
		}
		InteractionBehavior?.OnInteracted(interactor.ContextObject, npcEntity.Context, colliderIndex);
	}

	[ServerRpc(RequireOwnership = false)]
	private void RequestInteractableState_ServerRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			ServerRpcParams serverRpcParams = default(ServerRpcParams);
			FastBufferWriter bufferWriter = __beginSendServerRpc(2240966889u, serverRpcParams, RpcDelivery.Reliable);
			__endSendServerRpc(ref bufferWriter, 2240966889u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (interactableColliders.Count > 0)
			{
				SetColliderEnabledState_ClientRpc(colliders[0].enabled);
			}
		}
	}

	private void OnDisable()
	{
		npcEntity.OnCombatStateChanged -= NpcEntityOnOnCombatStateChanged;
	}

	public void EndlessStart()
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			RequestInteractableState_ServerRpc();
		}
		npcEntity.OnCombatStateChanged += NpcEntityOnOnCombatStateChanged;
	}

	private void NpcEntityOnOnCombatStateChanged()
	{
		UpdateInteractableState();
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1453106355u, __rpc_handler_1453106355, "SetColliderEnabledState_ClientRpc");
		__registerRpc(2240966889u, __rpc_handler_2240966889, "RequestInteractableState_ServerRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1453106355(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((NpcInteractable)target).SetColliderEnabledState_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2240966889(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((NpcInteractable)target).RequestInteractableState_ServerRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "NpcInteractable";
	}
}
