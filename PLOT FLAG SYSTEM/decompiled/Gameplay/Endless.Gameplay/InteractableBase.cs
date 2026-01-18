using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.SoVariables;
using Endless.Props;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class InteractableBase : EndlessNetworkBehaviour
{
	[SerializeField]
	private UnityEngine.Vector3 interactionPromptOffset = UnityEngine.Vector3.up;

	protected readonly NetworkVariable<InteractionAnimation> interactionAnimation = new NetworkVariable<InteractionAnimation>(InteractionAnimation.Default);

	protected readonly NetworkVariable<float> interactionDuration = new NetworkVariable<float>(0f);

	protected readonly NetworkVariable<bool> isHeldInteraction = new NetworkVariable<bool>(value: false);

	protected readonly NetworkVariable<bool> hidePromptDuringInteraction = new NetworkVariable<bool>(value: false);

	private readonly NetworkList<NetworkedNullableVector3> overridePositions = new NetworkList<NetworkedNullableVector3>();

	private readonly NetworkList<bool> usePlayerAnchor = new NetworkList<bool>();

	private readonly NetworkList<bool> isInteractable = new NetworkList<bool>();

	[SerializeField]
	protected List<InteractableCollider> interactableColliders;

	[field: SerializeField]
	public UIInteractionPromptVariable InteractionPrompt { get; protected set; }

	public UnityEngine.Vector3 InteractionPromptOffset => interactionPromptOffset;

	public bool IsHeldInteraction
	{
		get
		{
			return isHeldInteraction.Value;
		}
		set
		{
			if (base.IsServer)
			{
				isHeldInteraction.Value = value;
			}
		}
	}

	public bool HidePromptDuringInteraction
	{
		get
		{
			return hidePromptDuringInteraction.Value;
		}
		set
		{
			if (base.IsServer)
			{
				hidePromptDuringInteraction.Value = value;
			}
		}
	}

	public float InteractionDuration
	{
		get
		{
			if (!IsHeldInteraction)
			{
				return 0f;
			}
			return interactionDuration.Value;
		}
		set
		{
			if (base.IsServer)
			{
				interactionDuration.Value = value;
			}
		}
	}

	public InteractionAnimation InteractionAnimation
	{
		get
		{
			return interactionAnimation.Value;
		}
		set
		{
			if (base.IsServer)
			{
				interactionAnimation.Value = value;
			}
		}
	}

	private void OnValidate()
	{
		if (InteractionPrompt == null)
		{
			Debug.LogError("An InteractionPrompt is required!", this);
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		overridePositions.OnListChanged += OnOverridePositionsListChanged;
		usePlayerAnchor.OnListChanged += OnUsePlayerAnchorChanged;
		isInteractable.OnListChanged += OnIsInteractableListChanged;
	}

	private void OnIsInteractableListChanged(NetworkListEvent<bool> changeEvent)
	{
		switch (changeEvent.Type)
		{
		case NetworkListEvent<bool>.EventType.Add:
		case NetworkListEvent<bool>.EventType.Insert:
		case NetworkListEvent<bool>.EventType.Value:
			interactableColliders[changeEvent.Index].IsInteractable = changeEvent.Value;
			break;
		case NetworkListEvent<bool>.EventType.Full:
		{
			for (int i = 0; i < isInteractable.Count; i++)
			{
				interactableColliders[i].IsInteractable = isInteractable[i];
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void OnUsePlayerAnchorChanged(NetworkListEvent<bool> changeEvent)
	{
		switch (changeEvent.Type)
		{
		case NetworkListEvent<bool>.EventType.Add:
		case NetworkListEvent<bool>.EventType.Insert:
		case NetworkListEvent<bool>.EventType.Value:
			interactableColliders[changeEvent.Index].UsePlayerForAnchor = changeEvent.Value;
			break;
		case NetworkListEvent<bool>.EventType.Full:
		{
			for (int i = 0; i < usePlayerAnchor.Count; i++)
			{
				interactableColliders[i].UsePlayerForAnchor = usePlayerAnchor[i];
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void OnOverridePositionsListChanged(NetworkListEvent<NetworkedNullableVector3> changeEvent)
	{
		switch (changeEvent.Type)
		{
		case NetworkListEvent<NetworkedNullableVector3>.EventType.Add:
		case NetworkListEvent<NetworkedNullableVector3>.EventType.Insert:
		case NetworkListEvent<NetworkedNullableVector3>.EventType.Value:
			interactableColliders[changeEvent.Index].OverrideAnchorPosition = changeEvent.Value;
			break;
		case NetworkListEvent<NetworkedNullableVector3>.EventType.Full:
		{
			for (int i = 0; i < overridePositions.Count; i++)
			{
				interactableColliders[i].OverrideAnchorPosition = overridePositions[i];
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	protected void SetupColliders(IReadOnlyList<ColliderInfo> colliderInfos)
	{
		interactableColliders = new List<InteractableCollider>();
		foreach (ColliderInfo colliderInfo in colliderInfos)
		{
			Collider[] cachedColliders = colliderInfo.CachedColliders;
			for (int i = 0; i < cachedColliders.Length; i++)
			{
				InteractableCollider interactableCollider = cachedColliders[i].gameObject.AddComponent<InteractableCollider>();
				interactableCollider.InteractableBase = this;
				interactableCollider.ColliderInfo = colliderInfo;
				interactableColliders.Add(interactableCollider);
			}
		}
	}

	internal void SetInteractableEnabled(Context _, int index, bool newValue)
	{
		if (index >= interactableColliders.Count)
		{
			throw new ArgumentOutOfRangeException("index", "Provided index to Anchor Override is greater than the number of available colliders");
		}
		while (isInteractable.Count <= index)
		{
			isInteractable.Add(item: true);
		}
		isInteractable[index] = newValue;
	}

	public virtual void SetAllInteractablesEnabled(bool interactable)
	{
		while (isInteractable.Count < interactableColliders.Count)
		{
			isInteractable.Add(item: true);
		}
		for (int i = 0; i < isInteractable.Count; i++)
		{
			isInteractable[i] = interactable;
		}
	}

	[ClientRpc]
	protected void AttemptInteractResult_ClientRPC(NetworkObjectReference interactorNetworkObject, bool result, ClientRpcParams rpcParams)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(3862296847u, rpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in interactorNetworkObject, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in result, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 3862296847u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (!interactorNetworkObject.TryGet(out var networkObject))
		{
			return;
		}
		InteractorBase component = networkObject.GetComponent<InteractorBase>();
		if ((bool)component)
		{
			if (result)
			{
				component.HandleInteractionSucceeded(this);
			}
			else
			{
				component.HandleFailedInteraction(this);
			}
		}
	}

	public void SetInteractionResultSprite(Sprite newInteractionResultSprite)
	{
		foreach (InteractableCollider interactableCollider in interactableColliders)
		{
			interactableCollider.SetInteractionResultSprite(newInteractionResultSprite);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	protected virtual void AttemptInteract_ServerRPC(NetworkObjectReference interactorNetworkObject, int colliderIndex, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(3318549560u, serverRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in interactorNetworkObject, default(FastBufferWriter.ForNetworkSerializable));
			BytePacker.WriteValueBitPacked(bufferWriter, colliderIndex);
			__endSendServerRpc(ref bufferWriter, 3318549560u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (colliderIndex < 0 || interactableColliders.Count <= colliderIndex || !interactorNetworkObject.TryGet(out var networkObject))
		{
			return;
		}
		InteractorBase component = networkObject.GetComponent<InteractorBase>();
		if (component != null)
		{
			bool flag = interactableColliders[colliderIndex].CheckInteractionDistance(component) && AttemptInteract_ServerLogic(component, colliderIndex);
			ClientRpcParams rpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new ulong[2]
					{
						base.OwnerClientId,
						serverRpcParams.Receive.SenderClientId
					}
				}
			};
			AttemptInteractResult_ClientRPC(interactorNetworkObject, flag, rpcParams);
			if (flag)
			{
				InteractServerLogic(component, colliderIndex);
			}
		}
	}

	protected virtual bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
	{
		return true;
	}

	protected virtual void InteractServerLogic(InteractorBase interactor, int colliderIndex)
	{
	}

	public void AttemptInteract(InteractorBase interactor, InteractableCollider interactableCollider)
	{
		int num = interactableColliders.IndexOf(interactableCollider);
		if (num >= 0)
		{
			AttemptInteract_ServerRPC(interactor.NetworkObject, num);
		}
	}

	public virtual void InteractionStopped(InteractorBase interactor)
	{
	}

	public void SetAnchorPosition(UnityEngine.Vector3? anchorPosition, int index)
	{
		if (index >= interactableColliders.Count)
		{
			throw new ArgumentOutOfRangeException("index", "Provided index to Anchor Override is greater than the number of available colliders");
		}
		while (overridePositions.Count <= index)
		{
			overridePositions.Add(null);
		}
		overridePositions[index] = anchorPosition;
	}

	public void SetUsePlayerAnchor(bool newValue, int index)
	{
		if (index >= interactableColliders.Count)
		{
			throw new ArgumentOutOfRangeException("index", "Provided index to Anchor Override is greater than the number of available colliders");
		}
		while (usePlayerAnchor.Count <= index)
		{
			usePlayerAnchor.Add(item: false);
		}
		usePlayerAnchor[index] = newValue;
	}

	protected override void __initializeVariables()
	{
		if (interactionAnimation == null)
		{
			throw new Exception("InteractableBase.interactionAnimation cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		interactionAnimation.Initialize(this);
		__nameNetworkVariable(interactionAnimation, "interactionAnimation");
		NetworkVariableFields.Add(interactionAnimation);
		if (interactionDuration == null)
		{
			throw new Exception("InteractableBase.interactionDuration cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		interactionDuration.Initialize(this);
		__nameNetworkVariable(interactionDuration, "interactionDuration");
		NetworkVariableFields.Add(interactionDuration);
		if (isHeldInteraction == null)
		{
			throw new Exception("InteractableBase.isHeldInteraction cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		isHeldInteraction.Initialize(this);
		__nameNetworkVariable(isHeldInteraction, "isHeldInteraction");
		NetworkVariableFields.Add(isHeldInteraction);
		if (hidePromptDuringInteraction == null)
		{
			throw new Exception("InteractableBase.hidePromptDuringInteraction cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		hidePromptDuringInteraction.Initialize(this);
		__nameNetworkVariable(hidePromptDuringInteraction, "hidePromptDuringInteraction");
		NetworkVariableFields.Add(hidePromptDuringInteraction);
		if (overridePositions == null)
		{
			throw new Exception("InteractableBase.overridePositions cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		overridePositions.Initialize(this);
		__nameNetworkVariable(overridePositions, "overridePositions");
		NetworkVariableFields.Add(overridePositions);
		if (usePlayerAnchor == null)
		{
			throw new Exception("InteractableBase.usePlayerAnchor cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		usePlayerAnchor.Initialize(this);
		__nameNetworkVariable(usePlayerAnchor, "usePlayerAnchor");
		NetworkVariableFields.Add(usePlayerAnchor);
		if (isInteractable == null)
		{
			throw new Exception("InteractableBase.isInteractable cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		isInteractable.Initialize(this);
		__nameNetworkVariable(isInteractable, "isInteractable");
		NetworkVariableFields.Add(isInteractable);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(3862296847u, __rpc_handler_3862296847, "AttemptInteractResult_ClientRPC");
		__registerRpc(3318549560u, __rpc_handler_3318549560, "AttemptInteract_ServerRPC");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_3862296847(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out NetworkObjectReference value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value2, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((InteractableBase)target).AttemptInteractResult_ClientRPC(value, value2, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3318549560(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out NetworkObjectReference value, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out int value2);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((InteractableBase)target).AttemptInteract_ServerRPC(value, value2, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "InteractableBase";
	}
}
