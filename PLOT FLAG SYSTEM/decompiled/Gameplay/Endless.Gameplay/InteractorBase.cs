using Endless.Gameplay.Scripting;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public abstract class InteractorBase : NetworkBehaviour
{
	[SerializeField]
	private float interactOffset = 1f;

	[SerializeField]
	private float interactRadius = 1f;

	[SerializeField]
	private float interactHeightOffset = 0.5f;

	[SerializeField]
	private LayerMask layerMask;

	private readonly Collider[] collisionResults = new Collider[4];

	private InteractableBase currentInteractable;

	private float interactionStartTime;

	private InteractableCollider currentInteractableTarget;

	public UnityEvent<InteractableBase, InteractableBase> OnInteractionChanged = new UnityEvent<InteractableBase, InteractableBase>();

	public float InteractOffset => interactOffset;

	public float InteractRadius => interactRadius;

	protected virtual bool IsActive => base.IsServer;

	protected virtual UnityEngine.Vector3 Position => base.transform.position;

	protected virtual UnityEngine.Vector3 Forward => base.transform.forward;

	public abstract Context ContextObject { get; }

	public InteractableBase CurrentInteractable
	{
		get
		{
			return currentInteractable;
		}
		protected set
		{
			if (currentInteractable != value)
			{
				interactionStartTime = Time.time;
				InteractableBase arg = currentInteractable;
				currentInteractable = value;
				OnInteractionChanged.Invoke(arg, value);
			}
		}
	}

	public InteractableCollider CurrentInteractableTarget
	{
		get
		{
			return currentInteractableTarget;
		}
		protected set
		{
			if (value != currentInteractableTarget)
			{
				if (currentInteractableTarget != null)
				{
					currentInteractableTarget.UnselectLocally();
				}
				currentInteractableTarget = value;
				if (currentInteractableTarget != null)
				{
					currentInteractableTarget.SelectLocally();
				}
			}
		}
	}

	public float InteractionTime => Time.time - interactionStartTime;

	protected virtual void Update()
	{
		if (IsActive)
		{
			UnityEngine.Vector3 position = Position + Forward * interactOffset + UnityEngine.Vector3.up * interactHeightOffset;
			InteractableCollider interactableCollider = null;
			float num = 0f;
			int num2 = Physics.OverlapSphereNonAlloc(position, interactRadius, collisionResults, layerMask, QueryTriggerInteraction.Collide);
			for (int i = 0; i < collisionResults.Length && i < num2; i++)
			{
				if (!collisionResults[i])
				{
					continue;
				}
				InteractableCollider component = collisionResults[i].GetComponent<InteractableCollider>();
				if (!component || (!component.IsInteractable && !(component.InteractableBase == CurrentInteractable)))
				{
					continue;
				}
				if (!interactableCollider)
				{
					interactableCollider = component;
					num = GetRelevancyScore(component);
					continue;
				}
				float relevancyScore = GetRelevancyScore(component);
				if (relevancyScore > num)
				{
					interactableCollider = component;
					num = relevancyScore;
				}
			}
			CurrentInteractableTarget = interactableCollider;
		}
		else
		{
			CurrentInteractableTarget = null;
		}
	}

	public override void OnNetworkDespawn()
	{
		if (base.IsServer)
		{
			ServerProcessAbandonInteraction();
		}
	}

	public virtual void HandleFailedInteraction(InteractableBase interactable)
	{
		CurrentInteractable = null;
	}

	public virtual void HandleInteractionSucceeded(InteractableBase interactableBase)
	{
		CurrentInteractable = interactableBase;
	}

	private float GetRelevancyScore(InteractableCollider interactableCollider)
	{
		if (interactableCollider.InteractableBase == CurrentInteractable)
		{
			return 1000f;
		}
		UnityEngine.Vector3 position = interactableCollider.transform.position;
		float num = Mathf.Min(1f, UnityEngine.Vector3.Angle(position - Position, Forward));
		float num2 = UnityEngine.Vector3.Distance(position, Position);
		return 1f - num / 90f + (1f - num2 / (interactOffset + interactRadius * 2f));
	}

	[ServerRpc(RequireOwnership = false)]
	protected void AbandonInteraction_ServerRPC()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams = default(ServerRpcParams);
				FastBufferWriter bufferWriter = __beginSendServerRpc(3173210109u, serverRpcParams, RpcDelivery.Reliable);
				__endSendServerRpc(ref bufferWriter, 3173210109u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				ServerProcessAbandonInteraction();
			}
		}
	}

	private void ServerProcessAbandonInteraction()
	{
		if ((bool)CurrentInteractable)
		{
			CurrentInteractable.InteractionStopped(this);
		}
		CurrentInteractable = null;
		StopInteracting_ClientRPC();
	}

	[ClientRpc]
	protected void StopInteracting_ClientRPC()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(1954377598u, clientRpcParams, RpcDelivery.Reliable);
				__endSendClientRpc(ref bufferWriter, 1954377598u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				CurrentInteractable = null;
			}
		}
	}

	public void HandleHealthZeroed()
	{
		if ((bool)CurrentInteractable)
		{
			AbandonInteraction_ServerRPC();
		}
	}

	public void InteractableStoppedInteraction()
	{
		CurrentInteractable = null;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(3173210109u, __rpc_handler_3173210109, "AbandonInteraction_ServerRPC");
		__registerRpc(1954377598u, __rpc_handler_1954377598, "StopInteracting_ClientRPC");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_3173210109(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((InteractorBase)target).AbandonInteraction_ServerRPC();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1954377598(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((InteractorBase)target).StopInteracting_ClientRPC();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "InteractorBase";
	}
}
