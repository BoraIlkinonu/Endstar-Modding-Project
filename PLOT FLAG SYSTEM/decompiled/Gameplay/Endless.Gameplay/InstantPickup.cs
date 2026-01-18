using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.ParticleSystems.Components;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay;

public class InstantPickup : InstantPickupBase, IBaseType, IComponentBase, IScriptInjector
{
	private Context context;

	[HideInInspector]
	[SerializeField]
	private InstantPickupReferences references;

	[HideInInspector]
	[SerializeField]
	private SwappableParticleSystem collectedParticleSystem;

	private EndlessScriptComponent scriptComponent;

	private Endless.Gameplay.LuaInterfaces.InstantPickup luaInterface;

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Context Context => context ?? (context = new Context(WorldObject));

	public Type ComponentReferenceType => typeof(InstantPickupReferences);

	public NavType NavValue => NavType.Intangible;

	public object LuaObject => luaInterface ?? (luaInterface = new Endless.Gameplay.LuaInterfaces.InstantPickup(this));

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.InstantPickup);

	protected override void ShowCollectedFX()
	{
		if ((bool)collectedParticleSystem && (bool)collectedParticleSystem.RuntimeParticleSystem)
		{
			collectedParticleSystem.Spawn();
		}
	}

	protected override bool ExternalAttemptPickup(Context context)
	{
		if (scriptComponent.TryExecuteFunction("AttemptPickup", out bool returnValue, (object)context))
		{
			return returnValue;
		}
		return false;
	}

	protected override void SetActive(bool active)
	{
		references.gameObject.SetActive(active);
		references.WorldTriggerArea.SetCollidersEnabled(active);
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = referenceBase as InstantPickupReferences;
		collectedParticleSystem = references.CollectedParticleSystem;
		Collider[] cachedColliders = references.WorldTriggerArea.CachedColliders;
		for (int i = 0; i < cachedColliders.Length; i++)
		{
			cachedColliders[i].gameObject.AddComponent<WorldTriggerCollider>().Initialize(worldTrigger);
		}
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "InstantPickup";
	}
}
