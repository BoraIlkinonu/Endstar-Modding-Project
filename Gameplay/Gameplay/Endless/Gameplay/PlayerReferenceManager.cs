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

namespace Endless.Gameplay
{
	// Token: 0x0200027D RID: 637
	public abstract class PlayerReferenceManager : EndlessNetworkBehaviour
	{
		// Token: 0x17000272 RID: 626
		// (get) Token: 0x06000D8E RID: 3470 RVA: 0x00049B2A File Offset: 0x00047D2A
		// (set) Token: 0x06000D8F RID: 3471 RVA: 0x00049B32 File Offset: 0x00047D32
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06000D90 RID: 3472 RVA: 0x00049B3B File Offset: 0x00047D3B
		// (set) Token: 0x06000D91 RID: 3473 RVA: 0x00049B43 File Offset: 0x00047D43
		public HittableComponent HittableComponent { get; private set; }

		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06000D92 RID: 3474 RVA: 0x00049B4C File Offset: 0x00047D4C
		// (set) Token: 0x06000D93 RID: 3475 RVA: 0x00049B54 File Offset: 0x00047D54
		public TeamComponent TeamComponent { get; private set; }

		// Token: 0x17000275 RID: 629
		// (get) Token: 0x06000D94 RID: 3476 RVA: 0x00049B5D File Offset: 0x00047D5D
		public CharacterController CharacterController
		{
			get
			{
				return this.characterController;
			}
		}

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x06000D95 RID: 3477 RVA: 0x00049B65 File Offset: 0x00047D65
		public PlayerController PlayerController
		{
			get
			{
				return this.playerController;
			}
		}

		// Token: 0x17000277 RID: 631
		// (get) Token: 0x06000D96 RID: 3478 RVA: 0x00049B6D File Offset: 0x00047D6D
		public TeleportComponent TeleportComponent
		{
			get
			{
				return this.teleportComponent;
			}
		}

		// Token: 0x17000278 RID: 632
		// (get) Token: 0x06000D97 RID: 3479 RVA: 0x00049B75 File Offset: 0x00047D75
		public PlayerInteractor PlayerInteractor
		{
			get
			{
				return this.playerInteractor;
			}
		}

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x06000D98 RID: 3480 RVA: 0x00049B7D File Offset: 0x00047D7D
		public PlayerNetworkController PlayerNetworkController
		{
			get
			{
				return this.playerNetworkController;
			}
		}

		// Token: 0x1700027A RID: 634
		// (get) Token: 0x06000D99 RID: 3481 RVA: 0x00049B85 File Offset: 0x00047D85
		public SafeGroundComponent SafeGroundComponent
		{
			get
			{
				return this.safeGroundComponent;
			}
		}

		// Token: 0x1700027B RID: 635
		// (get) Token: 0x06000D9A RID: 3482 RVA: 0x00049B8D File Offset: 0x00047D8D
		public PlayerPhysicsTaker PhysicsTaker
		{
			get
			{
				return this.physicsTaker;
			}
		}

		// Token: 0x1700027C RID: 636
		// (get) Token: 0x06000D9B RID: 3483 RVA: 0x00049B95 File Offset: 0x00047D95
		public PlayerStunComponent PlayerStunComponent
		{
			get
			{
				return this.playerStunComponent;
			}
		}

		// Token: 0x1700027D RID: 637
		// (get) Token: 0x06000D9C RID: 3484 RVA: 0x00049B9D File Offset: 0x00047D9D
		public AppearanceController ApperanceController
		{
			get
			{
				return this.PlayerNetworkController.AppearanceController;
			}
		}

		// Token: 0x1700027E RID: 638
		// (get) Token: 0x06000D9D RID: 3485 RVA: 0x00049BAA File Offset: 0x00047DAA
		public virtual PlayerEquipmentManager PlayerEquipmentManager { get; }

		// Token: 0x1700027F RID: 639
		// (get) Token: 0x06000D9E RID: 3486 RVA: 0x00049BB2 File Offset: 0x00047DB2
		public virtual Inventory Inventory { get; }

		// Token: 0x17000280 RID: 640
		// (get) Token: 0x06000D9F RID: 3487 RVA: 0x00049BBA File Offset: 0x00047DBA
		public virtual HealthComponent HealthComponent { get; }

		// Token: 0x17000281 RID: 641
		// (get) Token: 0x06000DA0 RID: 3488 RVA: 0x00049BC2 File Offset: 0x00047DC2
		public virtual PlayerDownedComponent PlayerDownedComponent { get; }

		// Token: 0x17000282 RID: 642
		// (get) Token: 0x06000DA1 RID: 3489 RVA: 0x00049BCA File Offset: 0x00047DCA
		public virtual WorldCollidable WorldCollidable { get; }

		// Token: 0x17000283 RID: 643
		// (get) Token: 0x06000DA2 RID: 3490 RVA: 0x00049BD2 File Offset: 0x00047DD2
		public virtual Transform LosProbe { get; }

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x06000DA3 RID: 3491 RVA: 0x00049BDA File Offset: 0x00047DDA
		public virtual DamageReaction DamageReaction { get; }

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x06000DA4 RID: 3492 RVA: 0x00049BE2 File Offset: 0x00047DE2
		public virtual PlayerGhostController PlayerGhostController { get; }

		// Token: 0x17000286 RID: 646
		// (get) Token: 0x06000DA5 RID: 3493 RVA: 0x00049BEA File Offset: 0x00047DEA
		public SerializableGuid CharacterVisualId
		{
			get
			{
				return this.characterVisualId.Value;
			}
		}

		// Token: 0x17000287 RID: 647
		// (get) Token: 0x06000DA6 RID: 3494 RVA: 0x00049BF8 File Offset: 0x00047DF8
		public CharacterCosmeticsDefinition CharacterCosmetics
		{
			get
			{
				CharacterCosmeticsDefinition characterCosmeticsDefinition;
				if (MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultCharacterCosmetics.TryGetDefinition(this.CharacterVisualId, out characterCosmeticsDefinition))
				{
					return characterCosmeticsDefinition;
				}
				return this.fallbackCharacterCosmeticsDefinition;
			}
		}

		// Token: 0x17000288 RID: 648
		// (get) Token: 0x06000DA7 RID: 3495 RVA: 0x00049C26 File Offset: 0x00047E26
		// (set) Token: 0x06000DA8 RID: 3496 RVA: 0x00049C35 File Offset: 0x00047E35
		public int UserSlot
		{
			get
			{
				return this.userSlot.Value + 1;
			}
			set
			{
				if (NetworkManager.Singleton.IsServer)
				{
					this.userSlot = new NetworkVariable<int>(value, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
				}
			}
		}

		// Token: 0x06000DA9 RID: 3497 RVA: 0x00049C54 File Offset: 0x00047E54
		protected virtual void OnValidate()
		{
			if (this.playerController == null)
			{
				this.playerController = base.GetComponent<PlayerController>();
			}
			if (this.playerNetworkController == null)
			{
				this.playerNetworkController = base.GetComponent<PlayerNetworkController>();
			}
			if (this.playerInteractor == null)
			{
				this.playerInteractor = base.GetComponent<PlayerInteractor>();
			}
			if (this.characterController == null)
			{
				this.characterController = base.GetComponent<CharacterController>();
			}
			if (this.teleportComponent == null)
			{
				this.teleportComponent = base.GetComponent<TeleportComponent>();
			}
			if (this.safeGroundComponent == null)
			{
				this.safeGroundComponent = base.GetComponent<SafeGroundComponent>();
			}
		}

		// Token: 0x06000DAA RID: 3498 RVA: 0x00049D00 File Offset: 0x00047F00
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			MonoBehaviourSingleton<PlayerManager>.Instance.RegisterPlayer(base.OwnerClientId, this);
			NetworkVariable<SerializableGuid> networkVariable = this.characterVisualId;
			networkVariable.OnValueChanged = (NetworkVariable<SerializableGuid>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<SerializableGuid>.OnValueChangedDelegate(this.HandleCharacterVisualChanged));
			if (base.IsClient && base.IsOwner)
			{
				PlayerReferenceManager.LocalInstance = this;
				SerializableGuid clientCharacterVisualId = CharacterCosmeticsDefinitionUtility.GetClientCharacterVisualId();
				this.UpdateCharacterVisualId_ServerRpc(clientCharacterVisualId);
				CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction = (Action<SerializableGuid>)Delegate.Combine(CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction, new Action<SerializableGuid>(this.UpdateCharacterVisualId_ServerRpc));
				this.Track();
			}
		}

		// Token: 0x06000DAB RID: 3499 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void Track()
		{
		}

		// Token: 0x06000DAC RID: 3500 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void Untrack()
		{
		}

		// Token: 0x06000DAD RID: 3501 RVA: 0x00002DB0 File Offset: 0x00000FB0
		private void HandleDisplayNameChanged(NetworkString32 previousValue, NetworkString32 newValue)
		{
		}

		// Token: 0x06000DAE RID: 3502 RVA: 0x00049D94 File Offset: 0x00047F94
		private void HandleCharacterVisualChanged(SerializableGuid previousValue, SerializableGuid newValue)
		{
			this.OnCharacterCosmeticsChanged.Invoke(this.CharacterCosmetics);
		}

		// Token: 0x06000DAF RID: 3503 RVA: 0x00049DA8 File Offset: 0x00047FA8
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			if (MonoBehaviourSingleton<PlayerManager>.Instance)
			{
				MonoBehaviourSingleton<PlayerManager>.Instance.UnregisterPlayer(base.OwnerClientId, this);
			}
			if (base.IsClient && base.IsOwner)
			{
				CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction = (Action<SerializableGuid>)Delegate.Remove(CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction, new Action<SerializableGuid>(this.UpdateCharacterVisualId_ServerRpc));
				this.Untrack();
			}
		}

		// Token: 0x17000289 RID: 649
		// (get) Token: 0x06000DB0 RID: 3504 RVA: 0x00049E0E File Offset: 0x0004800E
		public Team Team
		{
			get
			{
				return this.TeamComponent.Team;
			}
		}

		// Token: 0x06000DB1 RID: 3505 RVA: 0x00049E1C File Offset: 0x0004801C
		[ServerRpc]
		public void UpdateCharacterVisualId_ServerRpc(SerializableGuid newValue)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				if (base.OwnerClientId != networkManager.LocalClientId)
				{
					if (networkManager.LogLevel <= LogLevel.Normal)
					{
						Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
					}
					return;
				}
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2133246198U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in newValue, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendServerRpc(ref fastBufferWriter, 2133246198U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.characterVisualId.Value = newValue;
		}

		// Token: 0x06000DB2 RID: 3506 RVA: 0x00049F5C File Offset: 0x0004815C
		[ClientRpc]
		public void LevelChangeTeleport_ClientRpc(global::UnityEngine.Vector3 position, float rotation)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2290216597U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe(in position);
				fastBufferWriter.WriteValueSafe<float>(in rotation, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 2290216597U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.PlayerController.TriggerLevelChangeTeleport(position, rotation);
		}

		// Token: 0x06000DB4 RID: 3508 RVA: 0x0004A098 File Offset: 0x00048298
		protected override void __initializeVariables()
		{
			bool flag = this.characterVisualId == null;
			if (flag)
			{
				throw new Exception("PlayerReferenceManager.characterVisualId cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.characterVisualId.Initialize(this);
			base.__nameNetworkVariable(this.characterVisualId, "characterVisualId");
			this.NetworkVariableFields.Add(this.characterVisualId);
			flag = this.userSlot == null;
			if (flag)
			{
				throw new Exception("PlayerReferenceManager.userSlot cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.userSlot.Initialize(this);
			base.__nameNetworkVariable(this.userSlot, "userSlot");
			this.NetworkVariableFields.Add(this.userSlot);
			base.__initializeVariables();
		}

		// Token: 0x06000DB5 RID: 3509 RVA: 0x0004A148 File Offset: 0x00048348
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2133246198U, new NetworkBehaviour.RpcReceiveHandler(PlayerReferenceManager.__rpc_handler_2133246198), "UpdateCharacterVisualId_ServerRpc");
			base.__registerRpc(2290216597U, new NetworkBehaviour.RpcReceiveHandler(PlayerReferenceManager.__rpc_handler_2290216597), "LevelChangeTeleport_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06000DB6 RID: 3510 RVA: 0x0004A198 File Offset: 0x00048398
		private static void __rpc_handler_2133246198(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
			{
				if (networkManager.LogLevel <= LogLevel.Normal)
				{
					Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
				}
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayerReferenceManager)target).UpdateCharacterVisualId_ServerRpc(serializableGuid);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000DB7 RID: 3511 RVA: 0x0004A258 File Offset: 0x00048458
		private static void __rpc_handler_2290216597(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			global::UnityEngine.Vector3 vector;
			reader.ReadValueSafe(out vector);
			float num;
			reader.ReadValueSafe<float>(out num, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayerReferenceManager)target).LevelChangeTeleport_ClientRpc(vector, num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000DB8 RID: 3512 RVA: 0x0004A2D9 File Offset: 0x000484D9
		protected internal override string __getTypeName()
		{
			return "PlayerReferenceManager";
		}

		// Token: 0x04000C7C RID: 3196
		[SerializeField]
		private CharacterController characterController;

		// Token: 0x04000C7D RID: 3197
		[SerializeField]
		private PlayerController playerController;

		// Token: 0x04000C7E RID: 3198
		[SerializeField]
		private TeleportComponent teleportComponent;

		// Token: 0x04000C7F RID: 3199
		[SerializeField]
		private PlayerInteractor playerInteractor;

		// Token: 0x04000C80 RID: 3200
		[SerializeField]
		private PlayerNetworkController playerNetworkController;

		// Token: 0x04000C81 RID: 3201
		[SerializeField]
		private SafeGroundComponent safeGroundComponent;

		// Token: 0x04000C82 RID: 3202
		[SerializeField]
		private PlayerPhysicsTaker physicsTaker;

		// Token: 0x04000C83 RID: 3203
		[SerializeField]
		private PlayerStunComponent playerStunComponent;

		// Token: 0x04000C87 RID: 3207
		[SerializeField]
		public AppearanceAnimator ApperanceBasePrefab;

		// Token: 0x04000C88 RID: 3208
		[SerializeField]
		public GameObject InteractableGameObject;

		// Token: 0x04000C89 RID: 3209
		[SerializeField]
		public EndlessVisuals EndlessVisuals;

		// Token: 0x04000C8A RID: 3210
		[SerializeField]
		private CharacterCosmeticsDefinition fallbackCharacterCosmeticsDefinition;

		// Token: 0x04000C8B RID: 3211
		public static PlayerReferenceManager LocalInstance;

		// Token: 0x04000C94 RID: 3220
		private NetworkVariable<SerializableGuid> characterVisualId = new NetworkVariable<SerializableGuid>(SerializableGuid.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000C95 RID: 3221
		public UnityEvent<CharacterCosmeticsDefinition> OnCharacterCosmeticsChanged = new UnityEvent<CharacterCosmeticsDefinition>();

		// Token: 0x04000C96 RID: 3222
		private NetworkVariable<int> userSlot = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	}
}
