using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200019B RID: 411
	public class GoapController : IAttributeSourceController
	{
		// Token: 0x170001BF RID: 447
		// (get) Token: 0x06000942 RID: 2370 RVA: 0x0002AD93 File Offset: 0x00028F93
		// (set) Token: 0x06000943 RID: 2371 RVA: 0x0002AD9B File Offset: 0x00028F9B
		public bool HasCombatPlan { get; private set; }

		// Token: 0x06000944 RID: 2372 RVA: 0x0002ADA4 File Offset: 0x00028FA4
		public GoapController(NpcEntity entity, ClassDataList classDataList, DefaultBehaviorList defaultBehaviorList, uint replanFrames)
		{
			this.entity = entity;
			this.defaultBehaviorList = defaultBehaviorList;
			this.replanFrames = replanFrames;
			ClassData classData;
			if (!classDataList.TryGetClassData(entity.NpcClass.NpcClass, out classData))
			{
				classDataList.TryGetClassData(NpcClass.Blank, out classData);
				Debug.LogException(new Exception(string.Format("No class data associated with the AiClass {0}, Initializing Ai as a blank Class", classData.Class)));
			}
			foreach (Goal goal in classData.Goals.GetGoals(entity))
			{
				this.goals.Add(goal);
			}
			HashSet<GoapAction> hashSet = ActionGenerator.InitializeActions(entity);
			this.planner = new Planner(entity, hashSet);
			this.planFollower = entity.Components.PlanFollower;
		}

		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x06000945 RID: 2373 RVA: 0x0002AE94 File Offset: 0x00029094
		// (set) Token: 0x06000946 RID: 2374 RVA: 0x0002AE9C File Offset: 0x0002909C
		public IGoapNode CurrentCommandNode
		{
			get
			{
				return this.currentCommandNode;
			}
			set
			{
				if (this.currentCommandNode == value)
				{
					return;
				}
				if (this.currentCommandNode != null)
				{
					this.currentCommandNode.RemoveNodeGoals(this.entity);
				}
				this.currentCommandNode = value;
				IGoapNode goapNode = this.currentCommandNode;
				if (goapNode != null)
				{
					goapNode.AddNodeGoals(this.entity);
				}
				Action onAttributeSourceChanged = this.OnAttributeSourceChanged;
				if (onAttributeSourceChanged == null)
				{
					return;
				}
				onAttributeSourceChanged();
			}
		}

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x06000947 RID: 2375 RVA: 0x0002AEFA File Offset: 0x000290FA
		// (set) Token: 0x06000948 RID: 2376 RVA: 0x0002AF04 File Offset: 0x00029104
		public IGoapNode CurrentBehaviorNode
		{
			get
			{
				return this.currentBehaviorNode;
			}
			set
			{
				if (this.currentBehaviorNode == value)
				{
					return;
				}
				if (this.currentBehaviorNode != null)
				{
					this.currentBehaviorNode.RemoveNodeGoals(this.entity);
				}
				BehaviorNode behaviorNode;
				if (value != null)
				{
					this.currentBehaviorNode = value;
				}
				else if (!this.defaultBehaviorList.TryGetBehaviorNode(this.entity.IdleBehavior, out behaviorNode))
				{
					Debug.LogError(string.Format("{0} has no associated behavior in {1}", this.entity.IdleBehavior, this.defaultBehaviorList));
				}
				else
				{
					this.currentBehaviorNode = behaviorNode;
				}
				if (this.currentBehaviorNode != null)
				{
					this.currentBehaviorNode.AddNodeGoals(this.entity);
				}
				Action onAttributeSourceChanged = this.OnAttributeSourceChanged;
				if (onAttributeSourceChanged == null)
				{
					return;
				}
				onAttributeSourceChanged();
			}
		}

		// Token: 0x06000949 RID: 2377 RVA: 0x0002AFB4 File Offset: 0x000291B4
		public void SetDefaultBehavior()
		{
			BehaviorNode behaviorNode;
			if (!this.defaultBehaviorList.TryGetBehaviorNode(this.entity.IdleBehavior, out behaviorNode))
			{
				Debug.LogError(string.Format("{0} has no associated behavior in {1}", this.entity.IdleBehavior, this.defaultBehaviorList));
				return;
			}
			behaviorNode.GiveInstruction(this.entity.Context);
		}

		// Token: 0x0600094A RID: 2378 RVA: 0x0002B014 File Offset: 0x00029214
		public void TickGoap(uint frame)
		{
			if (!this.LockPlan && (ulong)frame % (ulong)((long)MonoBehaviourSingleton<NpcManager>.Instance.TickOffsetRange) == (ulong)((long)this.TickOffset) && (this.planFollower.ActionPlan == null || frame > this.nextPlanFrame))
			{
				this.goals.ForEach(delegate(Goal goal)
				{
					goal.UpdateGoal(frame);
				});
				ActionPlan newPlan = this.GetNewPlan();
				if (newPlan != null && newPlan.Actions.Count > 0)
				{
					this.nextPlanFrame = frame + this.replanFrames;
					this.entity.Components.PlanFollower.ActionPlan = newPlan;
					this.HasCombatPlan = newPlan.Actions.Any((GoapAction action) => action.SatisfiesState(MonoBehaviourSingleton<WorldStateDictionary>.Instance[WorldState.AttackTarget]) || action.SatisfiesState(MonoBehaviourSingleton<WorldStateDictionary>.Instance[WorldState.EngageTarget]));
				}
			}
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x0002B0FE File Offset: 0x000292FE
		public bool AddGoal(Goal goal)
		{
			if (this.goals.Contains(goal))
			{
				return false;
			}
			this.goals.Add(goal);
			return true;
		}

		// Token: 0x0600094C RID: 2380 RVA: 0x0002B11D File Offset: 0x0002931D
		public void RemoveGoal(Goal goal)
		{
			if (this.planFollower.ActionPlan != null && goal == this.planFollower.ActionPlan.Goal)
			{
				this.planFollower.ActionPlan = null;
			}
			this.goals.Remove(goal);
		}

		// Token: 0x0600094D RID: 2381 RVA: 0x0002B158 File Offset: 0x00029358
		private ActionPlan GetNewPlan()
		{
			this.higherPriorityGoals.Clear();
			if (this.planFollower.ActionPlan == null)
			{
				using (List<Goal>.Enumerator enumerator = this.goals.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Goal goal = enumerator.Current;
						if (!goal.DesiredWorldState.Evaluate(this.entity))
						{
							this.higherPriorityGoals.Add(goal);
						}
					}
					goto IL_00D3;
				}
			}
			float priority = this.planFollower.ActionPlan.Goal.Priority;
			foreach (Goal goal2 in this.goals)
			{
				if (goal2.Priority > priority && !goal2.DesiredWorldState.Evaluate(this.entity))
				{
					this.higherPriorityGoals.Add(goal2);
				}
			}
			IL_00D3:
			this.higherPriorityGoals.Sort();
			for (int i = this.higherPriorityGoals.Count - 1; i >= 0; i--)
			{
				Goal goal3 = this.higherPriorityGoals[i];
				ActionPlan bestPlan = this.planner.GetBestPlan(goal3);
				if (bestPlan != null)
				{
					return bestPlan;
				}
			}
			return null;
		}

		// Token: 0x0600094E RID: 2382 RVA: 0x0002B2A4 File Offset: 0x000294A4
		public void Uncontrolled()
		{
			this.planFollower.ActionPlan = null;
		}

		// Token: 0x14000013 RID: 19
		// (add) Token: 0x0600094F RID: 2383 RVA: 0x0002B2B4 File Offset: 0x000294B4
		// (remove) Token: 0x06000950 RID: 2384 RVA: 0x0002B2EC File Offset: 0x000294EC
		public event Action OnAttributeSourceChanged;

		// Token: 0x06000951 RID: 2385 RVA: 0x0002B321 File Offset: 0x00029521
		public List<INpcAttributeModifier> GetAttributeModifiers()
		{
			return new List<INpcAttributeModifier> { this.currentBehaviorNode, this.currentCommandNode };
		}

		// Token: 0x0400078F RID: 1935
		private readonly List<Goal> higherPriorityGoals = new List<Goal>();

		// Token: 0x04000790 RID: 1936
		private readonly List<Goal> goals = new List<Goal>();

		// Token: 0x04000791 RID: 1937
		private readonly NpcEntity entity;

		// Token: 0x04000792 RID: 1938
		private readonly PlanFollower planFollower;

		// Token: 0x04000793 RID: 1939
		private readonly DefaultBehaviorList defaultBehaviorList;

		// Token: 0x04000794 RID: 1940
		private readonly uint replanFrames;

		// Token: 0x04000795 RID: 1941
		private readonly Planner planner;

		// Token: 0x04000796 RID: 1942
		private IGoapNode currentCommandNode;

		// Token: 0x04000797 RID: 1943
		private IGoapNode currentBehaviorNode;

		// Token: 0x04000798 RID: 1944
		private uint nextPlanFrame;

		// Token: 0x04000799 RID: 1945
		public bool LockPlan;

		// Token: 0x0400079A RID: 1946
		public int TickOffset;
	}
}
