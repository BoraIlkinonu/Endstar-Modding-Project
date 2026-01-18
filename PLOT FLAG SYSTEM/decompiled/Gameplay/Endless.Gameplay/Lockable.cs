using System;
using System.Collections;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class Lockable : EndlessNetworkBehaviour, IComponentBase, IScriptInjector, IPersistantStateSubscriber
{
	public UnityEvent<Lockable> Unlocked = new UnityEvent<Lockable>();

	private KeyLibraryReference keyReference = new KeyLibraryReference(SerializableGuid.Empty);

	private GameObject spawnedLockVisuals;

	private GameObject backSpawnedLockVisuals;

	private NetworkVariable<bool> hasBeenUnlocked = new NetworkVariable<bool>(value: false);

	private bool clientUnlocked;

	[SerializeField]
	private LockableComponentReferences references;

	private Endless.Gameplay.LuaInterfaces.Lockable luaInterface;

	private EndlessScriptComponent scriptComponent;

	public KeyLibraryReference KeyReference
	{
		get
		{
			return keyReference;
		}
		set
		{
			keyReference = value;
			if ((bool)spawnedLockVisuals)
			{
				UnityEngine.Object.Destroy(spawnedLockVisuals);
			}
			if ((bool)backSpawnedLockVisuals)
			{
				UnityEngine.Object.Destroy(backSpawnedLockVisuals);
			}
			if (keyReference.Id != SerializableGuid.Empty)
			{
				SpawnLockVisuals();
			}
		}
	}

	public bool IsLocked
	{
		get
		{
			if (KeyReference.Id != SerializableGuid.Empty && !hasBeenUnlocked.Value)
			{
				return !clientUnlocked;
			}
			return false;
		}
	}

	public Type ComponentReferenceType => typeof(LockableComponentReferences);

	[field: SerializeField]
	public WorldObject WorldObject { get; private set; }

	public object LuaObject => luaInterface ?? (luaInterface = new Endless.Gameplay.LuaInterfaces.Lockable(this));

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.Lockable);

	public bool ShouldSaveAndLoad => true;

	public void Unlock(Context context)
	{
		if (IsLocked)
		{
			hasBeenUnlocked.Value = true;
			PlayUnlockEffects_ClientRpc();
		}
		scriptComponent.TryExecuteFunction("HandleUnlocked", out var _, context);
		Unlocked.Invoke(this);
	}

	[ClientRpc]
	public void PlayUnlockEffects_ClientRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1982935570u, clientRpcParams, RpcDelivery.Reliable);
			__endSendClientRpc(ref bufferWriter, 1982935570u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			clientUnlocked = true;
			if ((bool)spawnedLockVisuals)
			{
				StartCoroutine(LockFallOffAnim(spawnedLockVisuals));
			}
			if ((bool)backSpawnedLockVisuals)
			{
				StartCoroutine(LockFallOffAnim(backSpawnedLockVisuals));
			}
		}
	}

	private IEnumerator LockFallOffAnim(GameObject lockVisuals)
	{
		Rigidbody rigidbody = lockVisuals.GetComponent<Rigidbody>();
		Collider[] componentsInChildren = lockVisuals.GetComponentsInChildren<Collider>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			(new BoxCollider[1])[0] = lockVisuals.AddComponent<BoxCollider>();
		}
		if (rigidbody == null)
		{
			rigidbody = lockVisuals.AddComponent<Rigidbody>();
		}
		Transform lockTransform = rigidbody.transform;
		lockTransform.parent = null;
		rigidbody.isKinematic = false;
		yield return new WaitForSeconds(1.5f);
		float shrinkTime = 1.5f;
		UnityEngine.Vector3 scale = lockTransform.localScale;
		UnityEngine.Vector3 goalScale = UnityEngine.Vector3.one * 0.001f;
		for (float elapsedTime = 0f; elapsedTime < shrinkTime; elapsedTime += Time.deltaTime)
		{
			lockTransform.localScale = UnityEngine.Vector3.Slerp(scale, goalScale, elapsedTime / shrinkTime);
			yield return null;
		}
		UnityEngine.Object.Destroy(lockVisuals);
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		SpawnLockVisuals();
	}

	private void SpawnLockVisuals()
	{
		if (!IsLocked || (!(references.LockVisualsSpawnTransform != null) && !(references.BackLockVisualsSpawnTransform != null)))
		{
			return;
		}
		PropLibrary.RuntimePropInfo reference = KeyReference.GetReference();
		if (reference == null)
		{
			return;
		}
		Key componentInChildren = reference.EndlessProp.GetComponentInChildren<Key>();
		if (!componentInChildren)
		{
			return;
		}
		GameObject lockVisuals = componentInChildren.GetLockVisuals();
		if (lockVisuals != null)
		{
			if (references.LockVisualsSpawnTransform != null)
			{
				spawnedLockVisuals = UnityEngine.Object.Instantiate(lockVisuals, references.LockVisualsSpawnTransform.position, references.LockVisualsSpawnTransform.rotation, references.LockVisualsSpawnTransform);
				spawnedLockVisuals.layer = LayerMask.NameToLayer("NonInteractivePhysics");
				spawnedLockVisuals.SetActive(value: true);
			}
			if (references.BackLockVisualsSpawnTransform != null)
			{
				backSpawnedLockVisuals = UnityEngine.Object.Instantiate(lockVisuals, references.BackLockVisualsSpawnTransform.position, references.BackLockVisualsSpawnTransform.rotation, references.LockVisualsSpawnTransform);
				backSpawnedLockVisuals.layer = LayerMask.NameToLayer("NonInteractivePhysics");
				backSpawnedLockVisuals.SetActive(value: true);
			}
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = (LockableComponentReferences)referenceBase;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	public object GetSaveState()
	{
		return hasBeenUnlocked.Value;
	}

	public void LoadState(object loadedState)
	{
		if (base.IsServer && loadedState != null)
		{
			hasBeenUnlocked.Value = (bool)loadedState;
			if (hasBeenUnlocked.Value && KeyReference.Id != SerializableGuid.Empty)
			{
				DestroyLocks_ClientRpc();
			}
		}
	}

	[ClientRpc]
	private void DestroyLocks_ClientRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2960941201u, clientRpcParams, RpcDelivery.Reliable);
			__endSendClientRpc(ref bufferWriter, 2960941201u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			clientUnlocked = true;
			if ((bool)spawnedLockVisuals)
			{
				UnityEngine.Object.Destroy(spawnedLockVisuals);
			}
			if ((bool)backSpawnedLockVisuals)
			{
				UnityEngine.Object.Destroy(backSpawnedLockVisuals);
			}
		}
	}

	protected override void __initializeVariables()
	{
		if (hasBeenUnlocked == null)
		{
			throw new Exception("Lockable.hasBeenUnlocked cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		hasBeenUnlocked.Initialize(this);
		__nameNetworkVariable(hasBeenUnlocked, "hasBeenUnlocked");
		NetworkVariableFields.Add(hasBeenUnlocked);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1982935570u, __rpc_handler_1982935570, "PlayUnlockEffects_ClientRpc");
		__registerRpc(2960941201u, __rpc_handler_2960941201, "DestroyLocks_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1982935570(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Lockable)target).PlayUnlockEffects_ClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2960941201(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Lockable)target).DestroyLocks_ClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "Lockable";
	}
}
