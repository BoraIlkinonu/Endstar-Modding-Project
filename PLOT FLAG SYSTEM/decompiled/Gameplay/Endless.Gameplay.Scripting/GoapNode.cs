using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.Scripting;

public abstract class GoapNode : AttributeModifierNode, IGoapNode, IInstructionNode, INpcAttributeModifier
{
	protected readonly Dictionary<NpcEntity, HashSet<Goal>> AddedGoalMap = new Dictionary<NpcEntity, HashSet<Goal>>();

	public void AddNodeGoals(NpcEntity entity)
	{
		int numberOfGoals = GetNumberOfGoals();
		AddedGoalMap.Add(entity, new HashSet<Goal>());
		for (int i = 0; i < numberOfGoals; i++)
		{
			AddGoal(entity, i + 1);
		}
	}

	protected abstract void AddGoal(NpcEntity entity, int goalNumber);

	private int GetNumberOfGoals()
	{
		if (!scriptComponent.TryExecuteFunction("GetNumberOfGoals", out long returnValue))
		{
			return 1;
		}
		return (int)returnValue;
	}

	public void RemoveNodeGoals(NpcEntity entity)
	{
		if (!AddedGoalMap.TryGetValue(entity, out var value))
		{
			return;
		}
		foreach (Goal item in value)
		{
			RemoveGoal(entity, item);
		}
		AddedGoalMap.Remove(entity);
	}

	protected virtual void RemoveGoal(NpcEntity entity, Goal goal)
	{
		entity.Components.GoapController.RemoveGoal(goal);
	}

	protected WorldState GetDesiredWorldState(int goalNumber)
	{
		if (scriptComponent.TryExecuteFunction("GetWorldState", out long returnValue, (object)goalNumber))
		{
			return (WorldState)returnValue;
		}
		return WorldState.Nothing;
	}
}
