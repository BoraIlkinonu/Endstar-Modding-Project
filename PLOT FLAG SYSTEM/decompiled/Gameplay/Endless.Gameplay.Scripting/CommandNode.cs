using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class CommandNode : GoapNode
{
	public enum GoalCompletionMode
	{
		All,
		Any
	}

	[HideInInspector]
	public EndlessEvent OnCommandSucceeded = new EndlessEvent();

	[HideInInspector]
	public EndlessEvent OnCommandFailed = new EndlessEvent();

	[HideInInspector]
	public EndlessEvent OnCommandFinished = new EndlessEvent();

	public GoalCompletionMode CompletionMode;

	public override object LuaObject => luaObject ?? (luaObject = new Command(this));

	public override string InstructionName => "Generated Command";

	public override Type LuaObjectType => typeof(Command);

	public override NpcEnum.AttributeRank AttributeRank => NpcEnum.AttributeRank.Command;

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
			scriptComponent.TryExecuteFunction("ConfigureNpc", out var _, npcEntity.Context);
		}
	}

	private void GiveInstruction(NpcEntity npcEntity)
	{
		npcEntity.Components.GoapController.CurrentCommandNode = this;
	}

	public override void RescindInstruction(Context context)
	{
		NpcEntity npcEntity = NpcReference.GetNpcEntity(context);
		if ((bool)npcEntity)
		{
			RescindInstruction(npcEntity);
		}
	}

	private void RescindInstruction(NpcEntity npcEntity)
	{
		if (npcEntity.Components.GoapController.CurrentCommandNode == this)
		{
			npcEntity.Components.GoapController.CurrentCommandNode = null;
		}
	}

	protected override void AddGoal(NpcEntity entity, int goalNumber)
	{
		GenericWorldState desiredWorldState = MonoBehaviourSingleton<WorldStateDictionary>.Instance[GetDesiredWorldState(goalNumber)];
		ScriptingGoal scriptingGoal = new ScriptingGoal(goalNumber, ClampFunc, "Command" + goalNumber, desiredWorldState, entity, scriptComponent);
		AddedGoalMap[entity].Add(scriptingGoal);
		entity.Components.GoapController.AddGoal(scriptingGoal);
		scriptingGoal.OnGoalTerminated = (Action<Goal, Goal.TerminationCode>)Delegate.Combine(scriptingGoal.OnGoalTerminated, new Action<Goal, Goal.TerminationCode>(HandleOnGoalTerminated));
	}

	protected void HandleOnGoalTerminated(Goal goal, Goal.TerminationCode terminationCode)
	{
		switch (terminationCode)
		{
		case Goal.TerminationCode.Completed:
			HandleOnGoalComplete(goal);
			break;
		case Goal.TerminationCode.Failed:
			HandleOnGoalFailed(goal);
			break;
		default:
			throw new ArgumentOutOfRangeException("terminationCode", terminationCode, null);
		case Goal.TerminationCode.Interrupted:
			break;
		}
	}

	protected override void RemoveGoal(NpcEntity entity, Goal goal)
	{
		entity.Components.GoapController.RemoveGoal(goal);
		goal.OnGoalTerminated = (Action<Goal, Goal.TerminationCode>)Delegate.Remove(goal.OnGoalTerminated, new Action<Goal, Goal.TerminationCode>(HandleOnGoalTerminated));
	}

	protected virtual void HandleOnGoalComplete(Goal goal)
	{
		if (AddedGoalMap.TryGetValue(goal.NpcEntity, out var value))
		{
			RemoveGoal(goal.NpcEntity, goal);
			value.Remove(goal);
			if (CompletionMode != GoalCompletionMode.All || value.Count <= 0)
			{
				RescindInstruction(goal.NpcEntity.Context);
				OnCommandSucceeded?.Invoke(goal.NpcEntity.Context);
				OnCommandFinished?.Invoke(goal.NpcEntity.Context);
			}
		}
	}

	protected virtual void HandleOnGoalFailed(Goal goal)
	{
		RescindInstruction(goal.NpcEntity.Context);
		OnCommandFailed?.Invoke(goal.NpcEntity.Context);
		OnCommandFinished?.Invoke(goal.NpcEntity.Context);
	}

	protected static float ClampFunc(float priority)
	{
		if (priority != 0f)
		{
			return Mathf.Clamp(priority, 40f, 69f);
		}
		return 0f;
	}
}
