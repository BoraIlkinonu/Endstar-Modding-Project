using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Unity.Collections;

namespace Endless.Gameplay;

public class Planner
{
	private readonly NpcEntity entity;

	private readonly Dictionary<WorldState, HashSet<GoapAction.ActionKind>> actionMap;

	private readonly Dictionary<GoapAction.ActionKind, GoapAction> goapActionMap;

	public Planner(NpcEntity entity, HashSet<GoapAction> actions)
	{
		this.entity = entity;
		actionMap = new Dictionary<WorldState, HashSet<GoapAction.ActionKind>>();
		goapActionMap = new Dictionary<GoapAction.ActionKind, GoapAction>();
		foreach (GoapAction action in actions)
		{
			goapActionMap.Add(action.Action, action);
			foreach (GenericWorldState effect in action.Effects)
			{
				if (actionMap.TryGetValue(effect.WorldState, out var value))
				{
					value.Add(action.Action);
					continue;
				}
				actionMap.Add(effect.WorldState, new HashSet<GoapAction.ActionKind> { action.Action });
			}
		}
	}

	public unsafe ActionPlan GetBestPlan(Goal goal)
	{
		ArenaAllocator arenaAllocator = new ArenaAllocator(sizeof(PlanNode) * 500, Allocator.Temp);
		PlanNode* bestPlan = GetBestPlan(arenaAllocator, goal.DesiredWorldState.WorldState);
		if (bestPlan == null)
		{
			return null;
		}
		Stack<GoapAction> actions = new Stack<GoapAction>();
		float cost = 0f;
		TraversePlan(bestPlan, actions, ref cost);
		arenaAllocator.Dispose();
		return new ActionPlan(goal, actions, cost);
	}

	private unsafe void TraversePlan(PlanNode* node, Stack<GoapAction> actions, ref float cost)
	{
		cost += node->Cost;
		actions.Push(goapActionMap[node->Action]);
		for (int i = 0; i < node->NumPrerequisites; i++)
		{
			PlanNode* node2 = node->Prerequisites[i];
			TraversePlan(node2, actions, ref cost);
		}
	}

	private unsafe PlanNode* GetBestPlan(ArenaAllocator arenaAllocator, WorldState worldState)
	{
		if (!actionMap.TryGetValue(worldState, out var value) || value.Count == 0)
		{
			return null;
		}
		PlanNode* result = null;
		float num = float.MaxValue;
		foreach (GoapAction.ActionKind item in value)
		{
			PlanNode* ptr = PlanAction(arenaAllocator, item);
			if (ptr != null && !(ptr->Cost > num))
			{
				result = ptr;
				num = ptr->Cost;
			}
		}
		return result;
	}

	private unsafe PlanNode* PlanAction(ArenaAllocator arenaAllocator, GoapAction.ActionKind action)
	{
		GoapAction goapAction = goapActionMap[action];
		int num = 0;
		foreach (GenericWorldState prerequisite in goapAction.Prerequisites)
		{
			if (!prerequisite.Evaluate(entity))
			{
				if (!actionMap.TryGetValue(prerequisite.WorldState, out var value) || value.Count == 0)
				{
					return null;
				}
				num++;
			}
		}
		PlanNode* ptr = arenaAllocator.Alloc<PlanNode>();
		ptr->Action = action;
		ptr->Cost = goapAction.Cost;
		if (num == 0)
		{
			ptr->NumPrerequisites = 0;
			ptr->Prerequisites = null;
			return ptr;
		}
		ptr->NumPrerequisites = num;
		ptr->Prerequisites = (PlanNode**)arenaAllocator.Alloc<byte>(sizeof(PlanNode) * ptr->NumPrerequisites);
		int num2 = 0;
		foreach (GenericWorldState prerequisite2 in goapAction.Prerequisites)
		{
			if (prerequisite2.Evaluate(entity))
			{
				continue;
			}
			PlanNode* ptr2 = null;
			float num3 = float.MaxValue;
			foreach (GoapAction.ActionKind item in actionMap[prerequisite2.WorldState])
			{
				PlanNode* ptr3 = PlanAction(arenaAllocator, item);
				if (ptr3 != null && !(ptr3->Cost > num3))
				{
					ptr2 = ptr3;
					num3 = ptr3->Cost;
				}
			}
			if (ptr2 == null)
			{
				return null;
			}
			ptr->Prerequisites[num2] = ptr2;
			ptr->Cost += ptr2->Cost;
			num2++;
		}
		return ptr;
	}
}
