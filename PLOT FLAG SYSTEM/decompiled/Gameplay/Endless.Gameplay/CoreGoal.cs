using System;

namespace Endless.Gameplay;

public class CoreGoal : Goal
{
	private readonly Func<NpcEntity, float> priority;

	private readonly Action<NpcEntity> activate;

	private readonly Action<NpcEntity> deactivate;

	public override float Priority => priority(NpcEntity);

	public CoreGoal(string goalName, GenericWorldState worldState, NpcEntity npcEntity, Func<NpcEntity, float> priority, Action<NpcEntity> activate, Action<NpcEntity> deactivate)
		: base(goalName, worldState, npcEntity)
	{
		this.activate = activate;
		this.deactivate = deactivate;
		this.priority = priority;
	}

	public override void Activate()
	{
		activate?.Invoke(NpcEntity);
	}

	public override void GoalTerminated(TerminationCode code)
	{
		base.GoalTerminated(code);
		deactivate?.Invoke(NpcEntity);
		OnGoalTerminated?.Invoke(this, code);
	}
}
