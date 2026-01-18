using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class GoapController : IAttributeSourceController
{
	private readonly List<Goal> higherPriorityGoals = new List<Goal>();

	private readonly List<Goal> goals = new List<Goal>();

	private readonly NpcEntity entity;

	private readonly PlanFollower planFollower;

	private readonly DefaultBehaviorList defaultBehaviorList;

	private readonly uint replanFrames;

	private readonly Planner planner;

	private IGoapNode currentCommandNode;

	private IGoapNode currentBehaviorNode;

	private uint nextPlanFrame;

	public bool LockPlan;

	public int TickOffset;

	public bool HasCombatPlan { get; private set; }

	public IGoapNode CurrentCommandNode
	{
		get
		{
			return currentCommandNode;
		}
		set
		{
			if (currentCommandNode != value)
			{
				if (currentCommandNode != null)
				{
					currentCommandNode.RemoveNodeGoals(entity);
				}
				currentCommandNode = value;
				currentCommandNode?.AddNodeGoals(entity);
				this.OnAttributeSourceChanged?.Invoke();
			}
		}
	}

	public IGoapNode CurrentBehaviorNode
	{
		get
		{
			return currentBehaviorNode;
		}
		set
		{
			if (currentBehaviorNode != value)
			{
				if (currentBehaviorNode != null)
				{
					currentBehaviorNode.RemoveNodeGoals(entity);
				}
				BehaviorNode behaviorNode;
				if (value != null)
				{
					currentBehaviorNode = value;
				}
				else if (!defaultBehaviorList.TryGetBehaviorNode(entity.IdleBehavior, out behaviorNode))
				{
					Debug.LogError($"{entity.IdleBehavior} has no associated behavior in {defaultBehaviorList}");
				}
				else
				{
					currentBehaviorNode = behaviorNode;
				}
				if (currentBehaviorNode != null)
				{
					currentBehaviorNode.AddNodeGoals(entity);
				}
				this.OnAttributeSourceChanged?.Invoke();
			}
		}
	}

	public event Action OnAttributeSourceChanged;

	public GoapController(NpcEntity entity, ClassDataList classDataList, DefaultBehaviorList defaultBehaviorList, uint replanFrames)
	{
		this.entity = entity;
		this.defaultBehaviorList = defaultBehaviorList;
		this.replanFrames = replanFrames;
		if (!classDataList.TryGetClassData(entity.NpcClass.NpcClass, out var classData))
		{
			classDataList.TryGetClassData(NpcClass.Blank, out classData);
			Debug.LogException(new Exception($"No class data associated with the AiClass {classData.Class}, Initializing Ai as a blank Class"));
		}
		foreach (Goal goal in classData.Goals.GetGoals(entity))
		{
			goals.Add(goal);
		}
		HashSet<GoapAction> actions = ActionGenerator.InitializeActions(entity);
		planner = new Planner(entity, actions);
		planFollower = entity.Components.PlanFollower;
	}

	public void SetDefaultBehavior()
	{
		if (!defaultBehaviorList.TryGetBehaviorNode(entity.IdleBehavior, out var behaviorNode))
		{
			Debug.LogError($"{entity.IdleBehavior} has no associated behavior in {defaultBehaviorList}");
		}
		else
		{
			behaviorNode.GiveInstruction(entity.Context);
		}
	}

	public void TickGoap(uint frame)
	{
		if (LockPlan || frame % MonoBehaviourSingleton<NpcManager>.Instance.TickOffsetRange != TickOffset || (planFollower.ActionPlan != null && frame <= nextPlanFrame))
		{
			return;
		}
		goals.ForEach(delegate(Goal goal)
		{
			goal.UpdateGoal(frame);
		});
		ActionPlan newPlan = GetNewPlan();
		if (newPlan != null && newPlan.Actions.Count > 0)
		{
			nextPlanFrame = frame + replanFrames;
			entity.Components.PlanFollower.ActionPlan = newPlan;
			HasCombatPlan = newPlan.Actions.Any((GoapAction action) => action.SatisfiesState(MonoBehaviourSingleton<WorldStateDictionary>.Instance[WorldState.AttackTarget]) || action.SatisfiesState(MonoBehaviourSingleton<WorldStateDictionary>.Instance[WorldState.EngageTarget]));
		}
	}

	public bool AddGoal(Goal goal)
	{
		if (goals.Contains(goal))
		{
			return false;
		}
		goals.Add(goal);
		return true;
	}

	public void RemoveGoal(Goal goal)
	{
		if (planFollower.ActionPlan != null && goal == planFollower.ActionPlan.Goal)
		{
			planFollower.ActionPlan = null;
		}
		goals.Remove(goal);
	}

	private ActionPlan GetNewPlan()
	{
		higherPriorityGoals.Clear();
		if (planFollower.ActionPlan == null)
		{
			foreach (Goal goal2 in goals)
			{
				if (!goal2.DesiredWorldState.Evaluate(entity))
				{
					higherPriorityGoals.Add(goal2);
				}
			}
		}
		else
		{
			float priority = planFollower.ActionPlan.Goal.Priority;
			foreach (Goal goal3 in goals)
			{
				if (goal3.Priority > priority && !goal3.DesiredWorldState.Evaluate(entity))
				{
					higherPriorityGoals.Add(goal3);
				}
			}
		}
		higherPriorityGoals.Sort();
		for (int num = higherPriorityGoals.Count - 1; num >= 0; num--)
		{
			Goal goal = higherPriorityGoals[num];
			ActionPlan bestPlan = planner.GetBestPlan(goal);
			if (bestPlan != null)
			{
				return bestPlan;
			}
		}
		return null;
	}

	public void Uncontrolled()
	{
		planFollower.ActionPlan = null;
	}

	public List<INpcAttributeModifier> GetAttributeModifiers()
	{
		return new List<INpcAttributeModifier> { currentBehaviorNode, currentCommandNode };
	}
}
