using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class ConsumableHealingItem : StackableItem, IScriptInjector
{
	[SerializeField]
	private VisualsInfo tempVisualsInfoGround;

	[SerializeField]
	private VisualsInfo tempVisualsInfoEqupped;

	private ConsumableHealing item;

	protected override VisualsInfo GroundVisualsInfo => tempVisualsInfoGround;

	protected override VisualsInfo EquippedVisualsInfo => tempVisualsInfoEqupped;

	public int HealAmount { get; internal set; } = 4;

	public object LuaObject
	{
		get
		{
			if (item == null)
			{
				item = new ConsumableHealing(this);
			}
			return item;
		}
	}

	Type IScriptInjector.LuaObjectType => typeof(ConsumableHealing);

	protected override object SaveData()
	{
		return HealAmount;
	}

	protected override void LoadData(object data)
	{
		HealAmount = (int)data;
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
		return "ConsumableHealingItem";
	}
}
