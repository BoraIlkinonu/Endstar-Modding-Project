using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class PlayerLuaComponent : EndlessNetworkBehaviour, IBaseType, IComponentBase, IScriptInjector
{
	[SerializeField]
	internal GameplayPlayerReferenceManager references;

	private Player luaInterface;

	private EndlessScriptComponent scriptComponent;

	public GameplayPlayerReferenceManager References => references;

	public Context Context { get; private set; }

	public WorldObject WorldObject => references.WorldObject;

	public Player Player => luaInterface;

	public object LuaObject
	{
		get
		{
			if (luaInterface == null)
			{
				luaInterface = new Player(this);
			}
			return luaInterface;
		}
	}

	public Type LuaObjectType => typeof(Player);

	private void Awake()
	{
		SetupContext();
		List<IComponentBase> list = new List<IComponentBase> { references.HealthComponent, references.HittableComponent, references.TeamComponent };
		references.WorldObject.Initialize(this, list, base.NetworkObject);
		foreach (IComponentBase item in list)
		{
			item.PrefabInitialize(references.WorldObject);
		}
	}

	private async void SetupContext()
	{
		Context = new Context(references.WorldObject);
		int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(references.OwnerClientId);
		Context context = Context;
		context.InternalId = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(userId);
	}

	public string GetUserName()
	{
		int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(References.OwnerClientId);
		return MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserNameSynchronous(userId);
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
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
		return "PlayerLuaComponent";
	}
}
