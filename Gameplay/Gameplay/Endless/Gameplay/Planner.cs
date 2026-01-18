using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Unity.Collections;

namespace Endless.Gameplay
{
	// Token: 0x0200019F RID: 415
	public class Planner
	{
		// Token: 0x0600095C RID: 2396 RVA: 0x0002B534 File Offset: 0x00029734
		public Planner(NpcEntity entity, HashSet<GoapAction> actions)
		{
			this.entity = entity;
			this.actionMap = new Dictionary<WorldState, HashSet<GoapAction.ActionKind>>();
			this.goapActionMap = new Dictionary<GoapAction.ActionKind, GoapAction>();
			foreach (GoapAction goapAction in actions)
			{
				this.goapActionMap.Add(goapAction.Action, goapAction);
				foreach (GenericWorldState genericWorldState in goapAction.Effects)
				{
					HashSet<GoapAction.ActionKind> hashSet;
					if (this.actionMap.TryGetValue(genericWorldState.WorldState, out hashSet))
					{
						hashSet.Add(goapAction.Action);
					}
					else
					{
						this.actionMap.Add(genericWorldState.WorldState, new HashSet<GoapAction.ActionKind> { goapAction.Action });
					}
				}
			}
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x0002B63C File Offset: 0x0002983C
		public unsafe ActionPlan GetBestPlan(Goal goal)
		{
			ArenaAllocator arenaAllocator = new ArenaAllocator(sizeof(PlanNode) * 500, Allocator.Temp);
			PlanNode* bestPlan = this.GetBestPlan(arenaAllocator, goal.DesiredWorldState.WorldState);
			if (bestPlan == null)
			{
				return null;
			}
			Stack<GoapAction> stack = new Stack<GoapAction>();
			float num = 0f;
			this.TraversePlan(bestPlan, stack, ref num);
			arenaAllocator.Dispose();
			return new ActionPlan(goal, stack, num);
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x0002B69C File Offset: 0x0002989C
		private unsafe void TraversePlan(PlanNode* node, Stack<GoapAction> actions, ref float cost)
		{
			cost += node->Cost;
			actions.Push(this.goapActionMap[node->Action]);
			for (int i = 0; i < node->NumPrerequisites; i++)
			{
				PlanNode* ptr = *(IntPtr*)(node->Prerequisites + (IntPtr)i * (IntPtr)sizeof(PlanNode*) / (IntPtr)sizeof(PlanNode*));
				this.TraversePlan(ptr, actions, ref cost);
			}
		}

		// Token: 0x0600095F RID: 2399 RVA: 0x0002B6F8 File Offset: 0x000298F8
		private unsafe PlanNode* GetBestPlan(ArenaAllocator arenaAllocator, WorldState worldState)
		{
			HashSet<GoapAction.ActionKind> hashSet;
			if (!this.actionMap.TryGetValue(worldState, out hashSet) || hashSet.Count == 0)
			{
				return null;
			}
			PlanNode* ptr = null;
			float num = float.MaxValue;
			foreach (GoapAction.ActionKind actionKind in hashSet)
			{
				PlanNode* ptr2 = this.PlanAction(arenaAllocator, actionKind);
				if (ptr2 != null && ptr2->Cost <= num)
				{
					ptr = ptr2;
					num = ptr2->Cost;
				}
			}
			return ptr;
		}

		// Token: 0x06000960 RID: 2400 RVA: 0x0002B78C File Offset: 0x0002998C
		private unsafe PlanNode* PlanAction(ArenaAllocator arenaAllocator, GoapAction.ActionKind action)
		{
			GoapAction goapAction = this.goapActionMap[action];
			int num = 0;
			foreach (GenericWorldState genericWorldState in goapAction.Prerequisites)
			{
				if (!genericWorldState.Evaluate(this.entity))
				{
					HashSet<GoapAction.ActionKind> hashSet;
					if (!this.actionMap.TryGetValue(genericWorldState.WorldState, out hashSet) || hashSet.Count == 0)
					{
						return null;
					}
					num++;
				}
			}
			PlanNode* ptr = arenaAllocator.Alloc<PlanNode>(1);
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
			foreach (GenericWorldState genericWorldState2 in goapAction.Prerequisites)
			{
				if (!genericWorldState2.Evaluate(this.entity))
				{
					PlanNode* ptr2 = null;
					float num3 = float.MaxValue;
					foreach (GoapAction.ActionKind actionKind in this.actionMap[genericWorldState2.WorldState])
					{
						PlanNode* ptr3 = this.PlanAction(arenaAllocator, actionKind);
						if (ptr3 != null && ptr3->Cost <= num3)
						{
							ptr2 = ptr3;
							num3 = ptr3->Cost;
						}
					}
					if (ptr2 == null)
					{
						return null;
					}
					*(IntPtr*)(ptr->Prerequisites + (IntPtr)num2 * (IntPtr)sizeof(PlanNode*) / (IntPtr)sizeof(PlanNode*)) = ptr2;
					ptr->Cost += ptr2->Cost;
					num2++;
				}
			}
			return ptr;
		}

		// Token: 0x040007A2 RID: 1954
		private readonly NpcEntity entity;

		// Token: 0x040007A3 RID: 1955
		private readonly Dictionary<WorldState, HashSet<GoapAction.ActionKind>> actionMap;

		// Token: 0x040007A4 RID: 1956
		private readonly Dictionary<GoapAction.ActionKind, GoapAction> goapActionMap;
	}
}
