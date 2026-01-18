using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay;

public class JetpackItem : Item, IScriptInjector
{
	[SerializeField]
	private VisualsInfo tempVisualsInfoGround;

	[SerializeField]
	private VisualsInfo tempVisualsInfoEqupped;

	[SerializeField]
	[HideInInspector]
	private JetpackVisualReferences jetpackVisualReferences;

	private Jetpack luaInterface;

	protected override VisualsInfo GroundVisualsInfo => tempVisualsInfoGround;

	protected override VisualsInfo EquippedVisualsInfo => tempVisualsInfoEqupped;

	public object LuaObject
	{
		get
		{
			if (luaInterface == null)
			{
				luaInterface = new Jetpack(this);
			}
			return luaInterface;
		}
	}

	Type IScriptInjector.LuaObjectType => typeof(Jetpack);

	protected override void HandleVisualReferenceInitialized(ComponentReferences references)
	{
		jetpackVisualReferences = references as JetpackVisualReferences;
	}

	protected override void HandleInUseChanged(bool inUse)
	{
		if ((bool)jetpackVisualReferences)
		{
			if (inUse)
			{
				jetpackVisualReferences.InUseParticleSystem.RuntimeParticleSystem.Play();
			}
			else
			{
				jetpackVisualReferences.InUseParticleSystem.RuntimeParticleSystem.Stop();
			}
		}
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
		return "JetpackItem";
	}
}
