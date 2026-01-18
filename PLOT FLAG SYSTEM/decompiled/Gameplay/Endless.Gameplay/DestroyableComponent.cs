using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay;

public class DestroyableComponent : EndlessBehaviour, IComponentBase, IScriptInjector
{
	private Destroyable luaInterface;

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(DestroyableReferences);

	public object LuaObject => luaInterface ?? new Destroyable(this);

	public Type LuaObjectType => typeof(Destroyable);

	public void Destroy()
	{
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}
}
