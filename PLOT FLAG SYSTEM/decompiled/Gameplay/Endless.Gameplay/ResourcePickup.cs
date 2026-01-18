using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.ParticleSystems.Components;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class ResourcePickup : InstantPickupBase, IGameEndSubscriber, IBaseType, IComponentBase, IScriptInjector
{
	public enum ResourceQuantity
	{
		One = 1,
		Five = 5,
		Ten = 10
	}

	private int quantity = 1;

	[SerializeField]
	private ResourcePickupReferences references;

	[SerializeField]
	[HideInInspector]
	private string assetId;

	private Context context;

	private EndlessScriptComponent scriptComponent;

	private Endless.Gameplay.LuaInterfaces.ResourcePickup luaInterface;

	public ReferenceFilter Filter => ReferenceFilter.NonStatic | ReferenceFilter.InventoryItem | ReferenceFilter.Resource;

	public int Quantity
	{
		get
		{
			return quantity;
		}
		set
		{
			if (quantity != value)
			{
				quantity = value;
				UpdateVisuals();
			}
		}
	}

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Context Context => context ?? (context = new Context(WorldObject));

	public object LuaObject => luaInterface ?? (luaInterface = new Endless.Gameplay.LuaInterfaces.ResourcePickup(this));

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.ResourcePickup);

	private void UpdateVisuals()
	{
		references.OneVisuals.SetActive(value: false);
		references.FiveVisuals.SetActive(value: false);
		references.TenVisuals.SetActive(value: false);
		if (quantity >= 10)
		{
			references.TenVisuals.SetActive(value: true);
		}
		else if (quantity >= 5)
		{
			references.FiveVisuals.SetActive(value: true);
		}
		else
		{
			references.OneVisuals.SetActive(value: true);
		}
	}

	protected override bool ExternalAttemptPickup(Context context)
	{
		if (scriptComponent.TryExecuteFunction("AttemptPickup", out bool returnValue, (object)context))
		{
			return returnValue;
		}
		return true;
	}

	protected override void ApplyPickupResult(WorldObject worldObject)
	{
		base.ApplyPickupResult(worldObject);
		scriptComponent.TryExecuteFunction("PickedUp", out var _, worldObject.Context);
		ResourceLibraryReference resource = new ResourceLibraryReference
		{
			Id = assetId
		};
		NetworkBehaviourSingleton<ResourceManager>.Instance.ResourceCollected(resource, quantity, worldObject.NetworkObject.OwnerClientId);
	}

	protected override void SetActive(bool active)
	{
		references.gameObject.SetActive(active);
		references.WorldTriggerArea.SetCollidersEnabled(active);
	}

	protected override void ShowCollectedFX()
	{
		SwappableParticleSystem swappableParticleSystem = null;
		swappableParticleSystem = ((quantity >= 10) ? references.TenCollectedParticleSystem : ((quantity < 5) ? references.OneCollectedParticleSystem : references.FiveCollectedParticleSystem));
		if ((bool)swappableParticleSystem && (bool)swappableParticleSystem.RuntimeParticleSystem)
		{
			swappableParticleSystem.Spawn();
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		UpdateVisuals();
	}

	[ClientRpc]
	public void SetDynamicSpawnQuantity_ClientRpc(int value)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(627820990u, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(bufferWriter, value);
				__endSendClientRpc(ref bufferWriter, 627820990u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				Quantity = value;
			}
		}
	}

	void IGameEndSubscriber.EndlessGameEnd()
	{
		if (base.NetworkManager.IsServer && !base.ShouldSaveAndLoad)
		{
			base.NetworkObject.Despawn();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		assetId = endlessProp.Prop.AssetID;
		Collider[] cachedColliders = references.WorldTriggerArea.CachedColliders;
		for (int i = 0; i < cachedColliders.Length; i++)
		{
			cachedColliders[i].gameObject.AddComponent<WorldTriggerCollider>().Initialize(worldTrigger);
		}
		endlessProp.PopulateParticleSystems(endlessProp.Prop, references.gameObject);
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
		if (!base.ShouldSaveAndLoad)
		{
			scriptComponent.ShouldSaveAndLoad = false;
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(627820990u, __rpc_handler_627820990, "SetDynamicSpawnQuantity_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_627820990(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ResourcePickup)target).SetDynamicSpawnQuantity_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "ResourcePickup";
	}
}
