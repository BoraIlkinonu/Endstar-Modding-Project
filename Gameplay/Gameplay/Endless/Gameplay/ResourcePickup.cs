using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.ParticleSystems.Components;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000332 RID: 818
	public class ResourcePickup : InstantPickupBase, IGameEndSubscriber, IBaseType, IComponentBase, IScriptInjector
	{
		// Token: 0x17000402 RID: 1026
		// (get) Token: 0x06001391 RID: 5009 RVA: 0x0005EC3E File Offset: 0x0005CE3E
		public ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.NonStatic | ReferenceFilter.InventoryItem | ReferenceFilter.Resource;
			}
		}

		// Token: 0x17000403 RID: 1027
		// (get) Token: 0x06001392 RID: 5010 RVA: 0x0005EC42 File Offset: 0x0005CE42
		// (set) Token: 0x06001393 RID: 5011 RVA: 0x0005EC4A File Offset: 0x0005CE4A
		public int Quantity
		{
			get
			{
				return this.quantity;
			}
			set
			{
				if (this.quantity != value)
				{
					this.quantity = value;
					this.UpdateVisuals();
				}
			}
		}

		// Token: 0x06001394 RID: 5012 RVA: 0x0005EC64 File Offset: 0x0005CE64
		private void UpdateVisuals()
		{
			this.references.OneVisuals.SetActive(false);
			this.references.FiveVisuals.SetActive(false);
			this.references.TenVisuals.SetActive(false);
			if (this.quantity >= 10)
			{
				this.references.TenVisuals.SetActive(true);
				return;
			}
			if (this.quantity >= 5)
			{
				this.references.FiveVisuals.SetActive(true);
				return;
			}
			this.references.OneVisuals.SetActive(true);
		}

		// Token: 0x06001395 RID: 5013 RVA: 0x0005ECEC File Offset: 0x0005CEEC
		protected override bool ExternalAttemptPickup(Context context)
		{
			bool flag;
			return !this.scriptComponent.TryExecuteFunction<bool>("AttemptPickup", out flag, new object[] { context }) || flag;
		}

		// Token: 0x06001396 RID: 5014 RVA: 0x0005ED1C File Offset: 0x0005CF1C
		protected override void ApplyPickupResult(WorldObject worldObject)
		{
			base.ApplyPickupResult(worldObject);
			object[] array;
			this.scriptComponent.TryExecuteFunction("PickedUp", out array, new object[] { worldObject.Context });
			ResourceLibraryReference resourceLibraryReference = new ResourceLibraryReference
			{
				Id = this.assetId
			};
			NetworkBehaviourSingleton<ResourceManager>.Instance.ResourceCollected(resourceLibraryReference, this.quantity, worldObject.NetworkObject.OwnerClientId);
		}

		// Token: 0x06001397 RID: 5015 RVA: 0x0005ED85 File Offset: 0x0005CF85
		protected override void SetActive(bool active)
		{
			this.references.gameObject.SetActive(active);
			this.references.WorldTriggerArea.SetCollidersEnabled(active);
		}

		// Token: 0x06001398 RID: 5016 RVA: 0x0005EDAC File Offset: 0x0005CFAC
		protected override void ShowCollectedFX()
		{
			SwappableParticleSystem swappableParticleSystem;
			if (this.quantity >= 10)
			{
				swappableParticleSystem = this.references.TenCollectedParticleSystem;
			}
			else if (this.quantity >= 5)
			{
				swappableParticleSystem = this.references.FiveCollectedParticleSystem;
			}
			else
			{
				swappableParticleSystem = this.references.OneCollectedParticleSystem;
			}
			if (swappableParticleSystem && swappableParticleSystem.RuntimeParticleSystem)
			{
				swappableParticleSystem.Spawn(false);
			}
		}

		// Token: 0x06001399 RID: 5017 RVA: 0x0005EE12 File Offset: 0x0005D012
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			this.UpdateVisuals();
		}

		// Token: 0x0600139A RID: 5018 RVA: 0x0005EE20 File Offset: 0x0005D020
		[ClientRpc]
		public void SetDynamicSpawnQuantity_ClientRpc(int value)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(627820990U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, value);
				base.__endSendClientRpc(ref fastBufferWriter, 627820990U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.Quantity = value;
		}

		// Token: 0x0600139B RID: 5019 RVA: 0x0005EF07 File Offset: 0x0005D107
		void IGameEndSubscriber.EndlessGameEnd()
		{
			if (base.NetworkManager.IsServer && !base.ShouldSaveAndLoad)
			{
				base.NetworkObject.Despawn(true);
				global::UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x17000404 RID: 1028
		// (get) Token: 0x0600139C RID: 5020 RVA: 0x0005EF35 File Offset: 0x0005D135
		// (set) Token: 0x0600139D RID: 5021 RVA: 0x0005EF3D File Offset: 0x0005D13D
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000405 RID: 1029
		// (get) Token: 0x0600139E RID: 5022 RVA: 0x0005EF48 File Offset: 0x0005D148
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x0600139F RID: 5023 RVA: 0x0005EF73 File Offset: 0x0005D173
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x060013A0 RID: 5024 RVA: 0x0005EF7C File Offset: 0x0005D17C
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.assetId = endlessProp.Prop.AssetID;
			Collider[] cachedColliders = this.references.WorldTriggerArea.CachedColliders;
			for (int i = 0; i < cachedColliders.Length; i++)
			{
				cachedColliders[i].gameObject.AddComponent<WorldTriggerCollider>().Initialize(this.worldTrigger);
			}
			endlessProp.PopulateParticleSystems(endlessProp.Prop, this.references.gameObject);
		}

		// Token: 0x17000406 RID: 1030
		// (get) Token: 0x060013A1 RID: 5025 RVA: 0x0005EFEC File Offset: 0x0005D1EC
		public object LuaObject
		{
			get
			{
				ResourcePickup resourcePickup;
				if ((resourcePickup = this.luaInterface) == null)
				{
					resourcePickup = (this.luaInterface = new ResourcePickup(this));
				}
				return resourcePickup;
			}
		}

		// Token: 0x17000407 RID: 1031
		// (get) Token: 0x060013A2 RID: 5026 RVA: 0x0005F012 File Offset: 0x0005D212
		public Type LuaObjectType
		{
			get
			{
				return typeof(ResourcePickup);
			}
		}

		// Token: 0x060013A3 RID: 5027 RVA: 0x0005F01E File Offset: 0x0005D21E
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
			if (!base.ShouldSaveAndLoad)
			{
				this.scriptComponent.ShouldSaveAndLoad = false;
			}
		}

		// Token: 0x060013A5 RID: 5029 RVA: 0x0005F04C File Offset: 0x0005D24C
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060013A6 RID: 5030 RVA: 0x0005F062 File Offset: 0x0005D262
		protected override void __initializeRpcs()
		{
			base.__registerRpc(627820990U, new NetworkBehaviour.RpcReceiveHandler(ResourcePickup.__rpc_handler_627820990), "SetDynamicSpawnQuantity_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060013A7 RID: 5031 RVA: 0x0005F088 File Offset: 0x0005D288
		private static void __rpc_handler_627820990(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ResourcePickup)target).SetDynamicSpawnQuantity_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060013A8 RID: 5032 RVA: 0x0005F0EA File Offset: 0x0005D2EA
		protected internal override string __getTypeName()
		{
			return "ResourcePickup";
		}

		// Token: 0x04001066 RID: 4198
		private int quantity = 1;

		// Token: 0x04001067 RID: 4199
		[SerializeField]
		private ResourcePickupReferences references;

		// Token: 0x04001069 RID: 4201
		[SerializeField]
		[HideInInspector]
		private string assetId;

		// Token: 0x0400106A RID: 4202
		private Context context;

		// Token: 0x0400106B RID: 4203
		private EndlessScriptComponent scriptComponent;

		// Token: 0x0400106C RID: 4204
		private ResourcePickup luaInterface;

		// Token: 0x02000333 RID: 819
		public enum ResourceQuantity
		{
			// Token: 0x0400106E RID: 4206
			One = 1,
			// Token: 0x0400106F RID: 4207
			Five = 5,
			// Token: 0x04001070 RID: 4208
			Ten = 10
		}
	}
}
