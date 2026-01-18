using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay;

public class DashPackItem : Item, IScriptInjector
{
	[SerializeField]
	private VisualsInfo tempVisualsInfoGround;

	[SerializeField]
	private VisualsInfo tempVisualsInfoEqupped;

	[SerializeField]
	[HideInInspector]
	private DashPackVisualReferences dashPackVisualReferences;

	private bool startedEffect;

	private DashPack luaInterface;

	protected override VisualsInfo GroundVisualsInfo => tempVisualsInfoGround;

	protected override VisualsInfo EquippedVisualsInfo => tempVisualsInfoEqupped;

	public object LuaObject
	{
		get
		{
			if (luaInterface == null)
			{
				luaInterface = new DashPack(this);
			}
			return luaInterface;
		}
	}

	Type IScriptInjector.LuaObjectType => typeof(DashPack);

	protected override void HandleVisualReferenceInitialized(ComponentReferences references)
	{
		dashPackVisualReferences = references as DashPackVisualReferences;
	}

	protected override void HandleEquipmentUseStateChanged(UsableDefinition.UseState eus)
	{
		if (dashPackVisualReferences == null)
		{
			return;
		}
		if (eus != null)
		{
			GroundDashUsableDefinition groundDashUsableDefinition = inventoryUsableDefinition as GroundDashUsableDefinition;
			GroundDashUsableDefinition.GroundDashEquipmentUseState groundDashEquipmentUseState = (GroundDashUsableDefinition.GroundDashEquipmentUseState)eus;
			if (!startedEffect && groundDashEquipmentUseState.DashFrame >= groundDashUsableDefinition.DashDirectionDelayFramesCount)
			{
				startedEffect = true;
				switch (Mathf.RoundToInt(groundDashEquipmentUseState.DashAngleRelative * 4f))
				{
				case 1:
					dashPackVisualReferences.LeftSideParticleSystem.RuntimeParticleSystem.Play();
					break;
				case 3:
					dashPackVisualReferences.RightSideParticleSystem.RuntimeParticleSystem.Play();
					break;
				default:
					dashPackVisualReferences.LeftSideParticleSystem.RuntimeParticleSystem.Play();
					dashPackVisualReferences.RightSideParticleSystem.RuntimeParticleSystem.Play();
					break;
				}
			}
		}
		else
		{
			startedEffect = false;
			dashPackVisualReferences.LeftSideParticleSystem.RuntimeParticleSystem.Stop();
			dashPackVisualReferences.RightSideParticleSystem.RuntimeParticleSystem.Stop();
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
		return "DashPackItem";
	}
}
