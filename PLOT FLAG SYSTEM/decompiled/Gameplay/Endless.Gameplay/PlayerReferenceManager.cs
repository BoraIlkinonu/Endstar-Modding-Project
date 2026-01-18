using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.PlayerInventory;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public abstract class PlayerReferenceManager : EndlessNetworkBehaviour
{
	[SerializeField]
	private CharacterController characterController;

	[SerializeField]
	private PlayerController playerController;

	[SerializeField]
	private TeleportComponent teleportComponent;

	[SerializeField]
	private PlayerInteractor playerInteractor;

	[SerializeField]
	private PlayerNetworkController playerNetworkController;

	[SerializeField]
	private SafeGroundComponent safeGroundComponent;

	[SerializeField]
	private PlayerPhysicsTaker physicsTaker;

	[SerializeField]
	private PlayerStunComponent playerStunComponent;

	[SerializeField]
	public AppearanceAnimator ApperanceBasePrefab;

	[SerializeField]
	public GameObject InteractableGameObject;

	[SerializeField]
	public EndlessVisuals EndlessVisuals;

	[SerializeField]
	private CharacterCosmeticsDefinition fallbackCharacterCosmeticsDefinition;

	public static PlayerReferenceManager LocalInstance;

	private NetworkVariable<SerializableGuid> characterVisualId = new NetworkVariable<SerializableGuid>(SerializableGuid.Empty);

	public UnityEvent<CharacterCosmeticsDefinition> OnCharacterCosmeticsChanged = new UnityEvent<CharacterCosmeticsDefinition>();

	private NetworkVariable<int> userSlot = new NetworkVariable<int>(-1);

	[field: SerializeField]
	public WorldObject WorldObject { get; private set; }

	[field: SerializeField]
	public HittableComponent HittableComponent { get; private set; }

	[field: SerializeField]
	public TeamComponent TeamComponent { get; private set; }

	public CharacterController CharacterController => characterController;

	public PlayerController PlayerController => playerController;

	public TeleportComponent TeleportComponent => teleportComponent;

	public PlayerInteractor PlayerInteractor => playerInteractor;

	public PlayerNetworkController PlayerNetworkController => playerNetworkController;

	public SafeGroundComponent SafeGroundComponent => safeGroundComponent;

	public PlayerPhysicsTaker PhysicsTaker => physicsTaker;

	public PlayerStunComponent PlayerStunComponent => playerStunComponent;

	public AppearanceController ApperanceController => PlayerNetworkController.AppearanceController;

	public virtual PlayerEquipmentManager PlayerEquipmentManager { get; }

	public virtual Inventory Inventory { get; }

	public virtual HealthComponent HealthComponent { get; }

	public virtual PlayerDownedComponent PlayerDownedComponent { get; }

	public virtual WorldCollidable WorldCollidable { get; }

	public virtual Transform LosProbe { get; }

	public virtual DamageReaction DamageReaction { get; }

	public virtual PlayerGhostController PlayerGhostController { get; }

	public SerializableGuid CharacterVisualId => characterVisualId.Value;

	public CharacterCosmeticsDefinition CharacterCosmetics
	{
		get
		{
			if (MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultCharacterCosmetics.TryGetDefinition(CharacterVisualId, out var definition))
			{
				return definition;
			}
			return fallbackCharacterCosmeticsDefinition;
		}
	}

	public int UserSlot
	{
		get
		{
			return userSlot.Value + 1;
		}
		set
		{
			if (NetworkManager.Singleton.IsServer)
			{
				userSlot = new NetworkVariable<int>(value);
			}
		}
	}

	public Team Team => TeamComponent.Team;

	protected virtual void OnValidate()
	{
		if (playerController == null)
		{
			playerController = GetComponent<PlayerController>();
		}
		if (playerNetworkController == null)
		{
			playerNetworkController = GetComponent<PlayerNetworkController>();
		}
		if (playerInteractor == null)
		{
			playerInteractor = GetComponent<PlayerInteractor>();
		}
		if (characterController == null)
		{
			characterController = GetComponent<CharacterController>();
		}
		if (teleportComponent == null)
		{
			teleportComponent = GetComponent<TeleportComponent>();
		}
		if (safeGroundComponent == null)
		{
			safeGroundComponent = GetComponent<SafeGroundComponent>();
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		MonoBehaviourSingleton<PlayerManager>.Instance.RegisterPlayer(base.OwnerClientId, this);
		NetworkVariable<SerializableGuid> networkVariable = characterVisualId;
		networkVariable.OnValueChanged = (NetworkVariable<SerializableGuid>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<SerializableGuid>.OnValueChangedDelegate(HandleCharacterVisualChanged));
		if (base.IsClient && base.IsOwner)
		{
			LocalInstance = this;
			SerializableGuid clientCharacterVisualId = CharacterCosmeticsDefinitionUtility.GetClientCharacterVisualId();
			UpdateCharacterVisualId_ServerRpc(clientCharacterVisualId);
			CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction = (Action<SerializableGuid>)Delegate.Combine(CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction, new Action<SerializableGuid>(UpdateCharacterVisualId_ServerRpc));
			Track();
		}
	}

	protected virtual void Track()
	{
	}

	protected virtual void Untrack()
	{
	}

	private void HandleDisplayNameChanged(NetworkString32 previousValue, NetworkString32 newValue)
	{
	}

	private void HandleCharacterVisualChanged(SerializableGuid previousValue, SerializableGuid newValue)
	{
		OnCharacterCosmeticsChanged.Invoke(CharacterCosmetics);
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		if ((bool)MonoBehaviourSingleton<PlayerManager>.Instance)
		{
			MonoBehaviourSingleton<PlayerManager>.Instance.UnregisterPlayer(base.OwnerClientId, this);
		}
		if (base.IsClient && base.IsOwner)
		{
			CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction = (Action<SerializableGuid>)Delegate.Remove(CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction, new Action<SerializableGuid>(UpdateCharacterVisualId_ServerRpc));
			Untrack();
		}
	}

	[ServerRpc]
	public void UpdateCharacterVisualId_ServerRpc(SerializableGuid newValue)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			if (base.OwnerClientId != networkManager.LocalClientId)
			{
				if (networkManager.LogLevel <= LogLevel.Normal)
				{
					Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
				}
				return;
			}
			ServerRpcParams serverRpcParams = default(ServerRpcParams);
			FastBufferWriter bufferWriter = __beginSendServerRpc(2133246198u, serverRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in newValue, default(FastBufferWriter.ForNetworkSerializable));
			__endSendServerRpc(ref bufferWriter, 2133246198u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			characterVisualId.Value = newValue;
		}
	}

	[ClientRpc]
	public void LevelChangeTeleport_ClientRpc(UnityEngine.Vector3 position, float rotation)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(2290216597u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in position);
				bufferWriter.WriteValueSafe(in rotation, default(FastBufferWriter.ForPrimitives));
				__endSendClientRpc(ref bufferWriter, 2290216597u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				PlayerController.TriggerLevelChangeTeleport(position, rotation);
			}
		}
	}

	protected override void __initializeVariables()
	{
		if (characterVisualId == null)
		{
			throw new Exception("PlayerReferenceManager.characterVisualId cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		characterVisualId.Initialize(this);
		__nameNetworkVariable(characterVisualId, "characterVisualId");
		NetworkVariableFields.Add(characterVisualId);
		if (userSlot == null)
		{
			throw new Exception("PlayerReferenceManager.userSlot cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		userSlot.Initialize(this);
		__nameNetworkVariable(userSlot, "userSlot");
		NetworkVariableFields.Add(userSlot);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2133246198u, __rpc_handler_2133246198, "UpdateCharacterVisualId_ServerRpc");
		__registerRpc(2290216597u, __rpc_handler_2290216597, "LevelChangeTeleport_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2133246198(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
		{
			if (networkManager.LogLevel <= LogLevel.Normal)
			{
				Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
			}
		}
		else
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerReferenceManager)target).UpdateCharacterVisualId_ServerRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2290216597(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out UnityEngine.Vector3 value);
			reader.ReadValueSafe(out float value2, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerReferenceManager)target).LevelChangeTeleport_ClientRpc(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "PlayerReferenceManager";
	}
}
