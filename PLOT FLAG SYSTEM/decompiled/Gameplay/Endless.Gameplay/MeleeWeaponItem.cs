using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class MeleeWeaponItem : Item, IScriptInjector
{
	[SerializeField]
	private VisualsInfo tempVisualsInfoGround;

	[SerializeField]
	private VisualsInfo tempVisualsInfoEqupped;

	private MeleeWeapon luaInterface;

	protected override VisualsInfo GroundVisualsInfo => tempVisualsInfoGround;

	protected override VisualsInfo EquippedVisualsInfo => tempVisualsInfoEqupped;

	public override Type ComponentReferenceType => null;

	public int DamageOnHit { get; internal set; } = -1;

	public object LuaObject
	{
		get
		{
			if (luaInterface == null)
			{
				luaInterface = new MeleeWeapon(this);
			}
			return luaInterface;
		}
	}

	Type IScriptInjector.LuaObjectType => typeof(MeleeWeapon);

	public void Hit(HittableComponent hittableComponent)
	{
		if (base.IsServer)
		{
			scriptComponent.ExecuteFunction("OnHit", hittableComponent.WorldObject.Context);
		}
	}

	protected override object SaveData()
	{
		return DamageOnHit;
	}

	protected override void LoadData(object data)
	{
		DamageOnHit = (int)data;
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
		return "MeleeWeaponItem";
	}
}
