using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class BehaviorNode : GoapNode
{
	public override object LuaObject => luaObject ?? (luaObject = new Behavior(this));

	public override string InstructionName => "Generated Behavior";

	public override Type LuaObjectType => typeof(Behavior);

	public override NpcEnum.AttributeRank AttributeRank => NpcEnum.AttributeRank.Behavior;

	public override InstructionNode GetNode()
	{
		return this;
	}

	public override void GiveInstruction(Context context)
	{
		NpcEntity npcEntity = NpcReference.GetNpcEntity(context);
		if ((bool)npcEntity)
		{
			GiveInstruction(npcEntity);
		}
	}

	protected void GiveInstruction(NpcEntity npcEntity)
	{
		npcEntity.Components.GoapController.CurrentBehaviorNode = this;
		scriptComponent.TryExecuteFunction("ConfigureNpc", out var _, npcEntity.Context);
	}

	protected override void AddGoal(NpcEntity entity, int goalNumber)
	{
		GenericWorldState desiredWorldState = MonoBehaviourSingleton<WorldStateDictionary>.Instance[GetDesiredWorldState(goalNumber)];
		ScriptingGoal scriptingGoal = new ScriptingGoal(goalNumber, ClampFunc, goalNumber.ToString(), desiredWorldState, entity, scriptComponent);
		AddedGoalMap[entity].Add(scriptingGoal);
		entity.Components.GoapController.AddGoal(scriptingGoal);
	}

	public override void RescindInstruction(Context context)
	{
		NpcEntity npcEntity = NpcReference.GetNpcEntity(context);
		if ((bool)npcEntity)
		{
			RescindInstruction(npcEntity);
		}
	}

	protected void RescindInstruction(NpcEntity npcEntity)
	{
		if (npcEntity.Components.GoapController.CurrentBehaviorNode == this)
		{
			npcEntity.Components.GoapController.CurrentBehaviorNode = null;
		}
	}

	private static float ClampFunc(float priority)
	{
		return Mathf.Clamp(priority, 0f, 20f);
	}
}
