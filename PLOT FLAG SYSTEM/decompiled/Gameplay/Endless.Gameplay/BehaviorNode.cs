using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class BehaviorNode : GoapNode
{
	[SerializeField]
	private string behaviorName;

	public override NpcEnum.AttributeRank AttributeRank => NpcEnum.AttributeRank.Behavior;

	public override string InstructionName => behaviorName;

	public override Endless.Gameplay.Scripting.InstructionNode GetNode()
	{
		return null;
	}

	public override void GiveInstruction(Context context)
	{
		base.GiveInstruction(context);
		if ((bool)base.Entity)
		{
			base.Entity.Components.GoapController.CurrentBehaviorNode = this;
		}
	}

	public override void RescindInstruction(Context context)
	{
		if ((bool)base.Entity)
		{
			base.Entity.Components.GoapController.CurrentBehaviorNode = null;
		}
		base.RescindInstruction(context);
	}

	public override void AddNodeGoals(NpcEntity entity)
	{
		foreach (Object instructionGoal in InstructionGoals)
		{
			if (!instructionGoal)
			{
				continue;
			}
			Goal goal = ((IGoalBuilder)instructionGoal).GetGoal(entity);
			if (entity.Components.GoapController.AddGoal(goal))
			{
				if (AddedGoalsByNpcEntity.TryGetValue(entity, out var value))
				{
					value.Add(goal);
					continue;
				}
				AddedGoalsByNpcEntity[entity] = new List<Goal> { goal };
			}
		}
	}

	public override void RemoveNodeGoals(NpcEntity entity)
	{
		if (!AddedGoalsByNpcEntity.TryGetValue(entity, out var value))
		{
			return;
		}
		foreach (Goal item in value)
		{
			entity.Components.GoapController.RemoveGoal(item);
		}
		AddedGoalsByNpcEntity[entity].Clear();
	}
}
