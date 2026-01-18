using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay;

public class ScriptingGoal : Goal
{
	private readonly EndlessScriptComponent scriptComponent;

	private readonly int goalNumber;

	private readonly Func<float, float> clampFunc;

	public override float Priority
	{
		get
		{
			long returnValue;
			float arg = (scriptComponent.TryExecuteFunction("Priority", out returnValue, (object)NpcEntity.Context, (object)goalNumber) ? returnValue : 0);
			return clampFunc(arg);
		}
	}

	public ScriptingGoal(int goalNumber, Func<float, float> clampFunc, string goalName, GenericWorldState desiredWorldState, NpcEntity npcEntity, EndlessScriptComponent scriptComponent)
		: base(goalName, desiredWorldState, npcEntity)
	{
		this.scriptComponent = scriptComponent;
		this.goalNumber = goalNumber;
		this.clampFunc = clampFunc;
	}

	public override void Activate()
	{
		scriptComponent.TryExecuteFunction("Activate", out var _, NpcEntity.Context, goalNumber);
	}

	public override void GoalTerminated(TerminationCode code)
	{
		base.GoalTerminated(code);
		scriptComponent.TryExecuteFunction("Deactivate", out var _, NpcEntity.Context, goalNumber);
		OnGoalTerminated?.Invoke(this, code);
	}
}
