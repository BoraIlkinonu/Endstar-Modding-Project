using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class DynamicNavigationComponent : EndlessBehaviour, IScriptInjector, IComponentBase
{
	private Navigation luaObject;

	[field: SerializeField]
	[field: HideInInspector]
	private EndlessProp Prop { get; set; }

	[field: SerializeField]
	public bool StartsBlocking { get; private set; }

	public NavType NavValue => NavType.Dynamic;

	public object LuaObject => luaObject ?? (luaObject = new Navigation(this));

	public Type LuaObjectType => typeof(Navigation);

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	internal void SetBlockingBehavior(Context instigator, bool isBlocking)
	{
		MonoBehaviourSingleton<NavGraph>.Instance.PropStateChanged(WorldObject, isBlocking);
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		Prop = endlessProp;
	}
}
