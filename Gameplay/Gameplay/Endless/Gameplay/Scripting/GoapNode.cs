using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004B7 RID: 1207
	public abstract class GoapNode : AttributeModifierNode, IGoapNode, IInstructionNode, INpcAttributeModifier
	{
		// Token: 0x06001E00 RID: 7680 RVA: 0x00083614 File Offset: 0x00081814
		public void AddNodeGoals(NpcEntity entity)
		{
			int numberOfGoals = this.GetNumberOfGoals();
			this.AddedGoalMap.Add(entity, new HashSet<Goal>());
			for (int i = 0; i < numberOfGoals; i++)
			{
				this.AddGoal(entity, i + 1);
			}
		}

		// Token: 0x06001E01 RID: 7681
		protected abstract void AddGoal(NpcEntity entity, int goalNumber);

		// Token: 0x06001E02 RID: 7682 RVA: 0x00083650 File Offset: 0x00081850
		private int GetNumberOfGoals()
		{
			long num;
			if (!this.scriptComponent.TryExecuteFunction<long>("GetNumberOfGoals", out num, Array.Empty<object>()))
			{
				return 1;
			}
			return (int)num;
		}

		// Token: 0x06001E03 RID: 7683 RVA: 0x0008367C File Offset: 0x0008187C
		public void RemoveNodeGoals(NpcEntity entity)
		{
			HashSet<Goal> hashSet;
			if (!this.AddedGoalMap.TryGetValue(entity, out hashSet))
			{
				return;
			}
			foreach (Goal goal in hashSet)
			{
				this.RemoveGoal(entity, goal);
			}
			this.AddedGoalMap.Remove(entity);
		}

		// Token: 0x06001E04 RID: 7684 RVA: 0x000836EC File Offset: 0x000818EC
		protected virtual void RemoveGoal(NpcEntity entity, Goal goal)
		{
			entity.Components.GoapController.RemoveGoal(goal);
		}

		// Token: 0x06001E05 RID: 7685 RVA: 0x00083700 File Offset: 0x00081900
		protected WorldState GetDesiredWorldState(int goalNumber)
		{
			long num;
			if (this.scriptComponent.TryExecuteFunction<long>("GetWorldState", out num, new object[] { goalNumber }))
			{
				return (WorldState)num;
			}
			return WorldState.Nothing;
		}

		// Token: 0x04001759 RID: 5977
		protected readonly Dictionary<NpcEntity, HashSet<Goal>> AddedGoalMap = new Dictionary<NpcEntity, HashSet<Goal>>();
	}
}
