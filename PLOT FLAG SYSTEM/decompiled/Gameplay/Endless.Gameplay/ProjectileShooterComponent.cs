using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay;

public class ProjectileShooterComponent : EndlessNetworkBehaviour, NetClock.ISimulateFrameEnvironmentSubscriber, IComponentBase, IScriptInjector
{
	[SerializeField]
	private ProjectileShooter projectileShooter;

	private bool shootThisFrame;

	[SerializeField]
	private ProjectileShooterReferences references;

	private Endless.Gameplay.LuaInterfaces.ProjectileShooter luaInterface;

	private EndlessScriptComponent scriptComponent;

	[field: SerializeField]
	public WorldObject WorldObject { get; private set; }

	Type IComponentBase.ComponentReferenceType => typeof(ProjectileShooterReferences);

	object IScriptInjector.LuaObject => luaInterface ?? (luaInterface = new Endless.Gameplay.LuaInterfaces.ProjectileShooter(this));

	Type IScriptInjector.LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.ProjectileShooter);

	public void Shoot()
	{
		shootThisFrame = true;
	}

	private void OnProjectileHitCallback(HealthChangeResult result, Context context)
	{
		scriptComponent.TryExecuteFunction("OnProjectileHit", out var _, context);
	}

	protected override void Start()
	{
		base.Start();
		NetClock.Register(this);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		NetClock.Unregister(this);
	}

	public void SimulateFrameEnvironment(uint frame)
	{
		if (base.IsServer && shootThisFrame)
		{
			projectileShooter.ShootProjectileLocal(references.FirePoint.position, references.FirePoint.eulerAngles, frame, OnProjectileHitCallback);
			shootThisFrame = false;
		}
	}

	void IComponentBase.ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = (ProjectileShooterReferences)referenceBase;
		projectileShooter.SetupProjectileShooterReferences(references);
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	void IScriptInjector.ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
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
		return "ProjectileShooterComponent";
	}
}
