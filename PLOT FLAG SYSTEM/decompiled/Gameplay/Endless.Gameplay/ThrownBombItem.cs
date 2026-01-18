using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class ThrownBombItem : StackableItem, IScriptInjector
{
	[SerializeField]
	private VisualsInfo tempVisualsInfoGround;

	[SerializeField]
	private VisualsInfo tempVisualsInfoEqupped;

	private Endless.Gameplay.LuaInterfaces.ThrownBomb luaInterface;

	protected override VisualsInfo GroundVisualsInfo => tempVisualsInfoGround;

	protected override VisualsInfo EquippedVisualsInfo => tempVisualsInfoEqupped;

	public int DamageAtCenter { get; internal set; } = 4;

	public int DamageAtEdge { get; internal set; } = 2;

	public float CenterRadius { get; internal set; } = 2f;

	public float TotalBlastRadius { get; internal set; } = 4f;

	public float CenterBlastForce { get; internal set; } = 12f;

	public float EdgeBlastForce { get; internal set; } = 3f;

	public object LuaObject
	{
		get
		{
			if (luaInterface == null)
			{
				luaInterface = new Endless.Gameplay.LuaInterfaces.ThrownBomb(this);
			}
			return luaInterface;
		}
	}

	Type IScriptInjector.LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.ThrownBomb);

	protected override object SaveData()
	{
		return (DamageAtCenter, DamageAtEdge, CenterRadius, TotalBlastRadius, CenterBlastForce, EdgeBlastForce);
	}

	protected override void LoadData(object data)
	{
		(DamageAtCenter, DamageAtEdge, CenterRadius, TotalBlastRadius, CenterBlastForce, EdgeBlastForce) = ((int, int, float, float, float, float))data;
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
		return "ThrownBombItem";
	}
}
