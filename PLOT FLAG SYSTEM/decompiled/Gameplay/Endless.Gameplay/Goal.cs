using System;
using System.Collections.Generic;

namespace Endless.Gameplay;

public abstract class Goal : IComparable<Goal>
{
	public enum TerminationCode
	{
		Completed,
		Interrupted,
		Failed
	}

	protected const int MAX_GOAL_PRIORITY = 100;

	private const uint FAILURE_MEMORY_FRAMES = 20u;

	private readonly Queue<uint> failures = new Queue<uint>();

	public readonly string GoalName;

	public readonly GenericWorldState DesiredWorldState;

	public readonly NpcEntity NpcEntity;

	public Action<Goal, TerminationCode> OnGoalTerminated;

	public int RecentFailures => failures.Count;

	public abstract float Priority { get; }

	protected Goal(string goalName, GenericWorldState worldState, NpcEntity npcEntity)
	{
		GoalName = goalName;
		DesiredWorldState = worldState;
		NpcEntity = npcEntity;
	}

	public bool IsSatisfied()
	{
		return DesiredWorldState.Evaluate(NpcEntity);
	}

	public void UpdateGoal(uint frame)
	{
		if (failures.TryPeek(out var result) && frame > result + 20)
		{
			failures.Dequeue();
		}
	}

	public abstract void Activate();

	public virtual void GoalTerminated(TerminationCode code)
	{
		if (code == TerminationCode.Failed)
		{
			failures.Enqueue(NetClock.CurrentFrame);
		}
	}

	public int CompareTo(Goal other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		return Priority.CompareTo(other.Priority);
	}
}
