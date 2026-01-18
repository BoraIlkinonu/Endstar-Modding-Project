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

namespace Endless.Gameplay
{
	// Token: 0x02000353 RID: 851
	public class Lockable : EndlessNetworkBehaviour, IComponentBase, IScriptInjector, IPersistantStateSubscriber
	{
		// Token: 0x1700046A RID: 1130
		// (get) Token: 0x0600153F RID: 5439 RVA: 0x000658EE File Offset: 0x00063AEE
		// (set) Token: 0x06001540 RID: 5440 RVA: 0x000658F8 File Offset: 0x00063AF8
		public KeyLibraryReference KeyReference
		{
			get
			{
				return this.keyReference;
			}
			set
			{
				this.keyReference = value;
				if (this.spawnedLockVisuals)
				{
					global::UnityEngine.Object.Destroy(this.spawnedLockVisuals);
				}
				if (this.backSpawnedLockVisuals)
				{
					global::UnityEngine.Object.Destroy(this.backSpawnedLockVisuals);
				}
				if (this.keyReference.Id != SerializableGuid.Empty)
				{
					this.SpawnLockVisuals();
				}
			}
		}

		// Token: 0x1700046B RID: 1131
		// (get) Token: 0x06001541 RID: 5441 RVA: 0x00065959 File Offset: 0x00063B59
		public bool IsLocked
		{
			get
			{
				return this.KeyReference.Id != SerializableGuid.Empty && !this.hasBeenUnlocked.Value && !this.clientUnlocked;
			}
		}

		// Token: 0x06001542 RID: 5442 RVA: 0x0006598C File Offset: 0x00063B8C
		public void Unlock(Context context)
		{
			if (this.IsLocked)
			{
				this.hasBeenUnlocked.Value = true;
				this.PlayUnlockEffects_ClientRpc();
			}
			object[] array;
			this.scriptComponent.TryExecuteFunction("HandleUnlocked", out array, new object[] { context });
			this.Unlocked.Invoke(this);
		}

		// Token: 0x06001543 RID: 5443 RVA: 0x000659DC File Offset: 0x00063BDC
		[ClientRpc]
		public void PlayUnlockEffects_ClientRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1982935570U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 1982935570U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.clientUnlocked = true;
			if (this.spawnedLockVisuals)
			{
				base.StartCoroutine(this.LockFallOffAnim(this.spawnedLockVisuals));
			}
			if (this.backSpawnedLockVisuals)
			{
				base.StartCoroutine(this.LockFallOffAnim(this.backSpawnedLockVisuals));
			}
		}

		// Token: 0x06001544 RID: 5444 RVA: 0x00065AF6 File Offset: 0x00063CF6
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
			global::UnityEngine.Vector3 scale = lockTransform.localScale;
			global::UnityEngine.Vector3 goalScale = global::UnityEngine.Vector3.one * 0.001f;
			for (float elapsedTime = 0f; elapsedTime < shrinkTime; elapsedTime += Time.deltaTime)
			{
				lockTransform.localScale = global::UnityEngine.Vector3.Slerp(scale, goalScale, elapsedTime / shrinkTime);
				yield return null;
			}
			global::UnityEngine.Object.Destroy(lockVisuals);
			yield break;
		}

		// Token: 0x06001545 RID: 5445 RVA: 0x00065B05 File Offset: 0x00063D05
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			this.SpawnLockVisuals();
		}

		// Token: 0x06001546 RID: 5446 RVA: 0x00065B14 File Offset: 0x00063D14
		private void SpawnLockVisuals()
		{
			if (this.IsLocked && (this.references.LockVisualsSpawnTransform != null || this.references.BackLockVisualsSpawnTransform != null))
			{
				PropLibrary.RuntimePropInfo reference = this.KeyReference.GetReference();
				if (reference != null)
				{
					Key componentInChildren = reference.EndlessProp.GetComponentInChildren<Key>();
					if (componentInChildren)
					{
						GameObject lockVisuals = componentInChildren.GetLockVisuals();
						if (lockVisuals != null)
						{
							if (this.references.LockVisualsSpawnTransform != null)
							{
								this.spawnedLockVisuals = global::UnityEngine.Object.Instantiate<GameObject>(lockVisuals, this.references.LockVisualsSpawnTransform.position, this.references.LockVisualsSpawnTransform.rotation, this.references.LockVisualsSpawnTransform);
								this.spawnedLockVisuals.layer = LayerMask.NameToLayer("NonInteractivePhysics");
								this.spawnedLockVisuals.SetActive(true);
							}
							if (this.references.BackLockVisualsSpawnTransform != null)
							{
								this.backSpawnedLockVisuals = global::UnityEngine.Object.Instantiate<GameObject>(lockVisuals, this.references.BackLockVisualsSpawnTransform.position, this.references.BackLockVisualsSpawnTransform.rotation, this.references.LockVisualsSpawnTransform);
								this.backSpawnedLockVisuals.layer = LayerMask.NameToLayer("NonInteractivePhysics");
								this.backSpawnedLockVisuals.SetActive(true);
							}
						}
					}
				}
			}
		}

		// Token: 0x1700046C RID: 1132
		// (get) Token: 0x06001547 RID: 5447 RVA: 0x00065C67 File Offset: 0x00063E67
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(LockableComponentReferences);
			}
		}

		// Token: 0x1700046D RID: 1133
		// (get) Token: 0x06001548 RID: 5448 RVA: 0x00065C73 File Offset: 0x00063E73
		// (set) Token: 0x06001549 RID: 5449 RVA: 0x00065C7B File Offset: 0x00063E7B
		public WorldObject WorldObject { get; private set; }

		// Token: 0x0600154A RID: 5450 RVA: 0x00065C84 File Offset: 0x00063E84
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x0600154B RID: 5451 RVA: 0x00065C8D File Offset: 0x00063E8D
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = (LockableComponentReferences)referenceBase;
		}

		// Token: 0x1700046E RID: 1134
		// (get) Token: 0x0600154C RID: 5452 RVA: 0x00065C9C File Offset: 0x00063E9C
		public object LuaObject
		{
			get
			{
				Lockable lockable;
				if ((lockable = this.luaInterface) == null)
				{
					lockable = (this.luaInterface = new Lockable(this));
				}
				return lockable;
			}
		}

		// Token: 0x1700046F RID: 1135
		// (get) Token: 0x0600154D RID: 5453 RVA: 0x00065CC2 File Offset: 0x00063EC2
		public Type LuaObjectType
		{
			get
			{
				return typeof(Lockable);
			}
		}

		// Token: 0x0600154E RID: 5454 RVA: 0x00065CCE File Offset: 0x00063ECE
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x17000470 RID: 1136
		// (get) Token: 0x0600154F RID: 5455 RVA: 0x00017586 File Offset: 0x00015786
		public bool ShouldSaveAndLoad
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001550 RID: 5456 RVA: 0x00065CD7 File Offset: 0x00063ED7
		public object GetSaveState()
		{
			return this.hasBeenUnlocked.Value;
		}

		// Token: 0x06001551 RID: 5457 RVA: 0x00065CEC File Offset: 0x00063EEC
		public void LoadState(object loadedState)
		{
			if (base.IsServer && loadedState != null)
			{
				this.hasBeenUnlocked.Value = (bool)loadedState;
				if (this.hasBeenUnlocked.Value && this.KeyReference.Id != SerializableGuid.Empty)
				{
					this.DestroyLocks_ClientRpc();
				}
			}
		}

		// Token: 0x06001552 RID: 5458 RVA: 0x00065D40 File Offset: 0x00063F40
		[ClientRpc]
		private void DestroyLocks_ClientRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2960941201U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 2960941201U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.clientUnlocked = true;
			if (this.spawnedLockVisuals)
			{
				global::UnityEngine.Object.Destroy(this.spawnedLockVisuals);
			}
			if (this.backSpawnedLockVisuals)
			{
				global::UnityEngine.Object.Destroy(this.backSpawnedLockVisuals);
			}
		}

		// Token: 0x06001554 RID: 5460 RVA: 0x00065E7C File Offset: 0x0006407C
		protected override void __initializeVariables()
		{
			bool flag = this.hasBeenUnlocked == null;
			if (flag)
			{
				throw new Exception("Lockable.hasBeenUnlocked cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.hasBeenUnlocked.Initialize(this);
			base.__nameNetworkVariable(this.hasBeenUnlocked, "hasBeenUnlocked");
			this.NetworkVariableFields.Add(this.hasBeenUnlocked);
			base.__initializeVariables();
		}

		// Token: 0x06001555 RID: 5461 RVA: 0x00065EE0 File Offset: 0x000640E0
		protected override void __initializeRpcs()
		{
			base.__registerRpc(1982935570U, new NetworkBehaviour.RpcReceiveHandler(Lockable.__rpc_handler_1982935570), "PlayUnlockEffects_ClientRpc");
			base.__registerRpc(2960941201U, new NetworkBehaviour.RpcReceiveHandler(Lockable.__rpc_handler_2960941201), "DestroyLocks_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06001556 RID: 5462 RVA: 0x00065F30 File Offset: 0x00064130
		private static void __rpc_handler_1982935570(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Lockable)target).PlayUnlockEffects_ClientRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001557 RID: 5463 RVA: 0x00065F84 File Offset: 0x00064184
		private static void __rpc_handler_2960941201(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Lockable)target).DestroyLocks_ClientRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001558 RID: 5464 RVA: 0x00065FD5 File Offset: 0x000641D5
		protected internal override string __getTypeName()
		{
			return "Lockable";
		}

		// Token: 0x04001172 RID: 4466
		public UnityEvent<Lockable> Unlocked = new UnityEvent<Lockable>();

		// Token: 0x04001173 RID: 4467
		private KeyLibraryReference keyReference = new KeyLibraryReference(SerializableGuid.Empty);

		// Token: 0x04001174 RID: 4468
		private GameObject spawnedLockVisuals;

		// Token: 0x04001175 RID: 4469
		private GameObject backSpawnedLockVisuals;

		// Token: 0x04001176 RID: 4470
		private NetworkVariable<bool> hasBeenUnlocked = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04001177 RID: 4471
		private bool clientUnlocked;

		// Token: 0x04001178 RID: 4472
		[SerializeField]
		private LockableComponentReferences references;

		// Token: 0x0400117A RID: 4474
		private Lockable luaInterface;

		// Token: 0x0400117B RID: 4475
		private EndlessScriptComponent scriptComponent;
	}
}
