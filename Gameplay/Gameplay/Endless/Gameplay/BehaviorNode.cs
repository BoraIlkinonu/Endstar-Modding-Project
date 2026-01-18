using System;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020001A3 RID: 419
	public class BehaviorNode : GoapNode
	{
		// Token: 0x170001CA RID: 458
		// (get) Token: 0x0600096D RID: 2413 RVA: 0x00017586 File Offset: 0x00015786
		public override NpcEnum.AttributeRank AttributeRank
		{
			get
			{
				return NpcEnum.AttributeRank.Behavior;
			}
		}

		// Token: 0x0600096E RID: 2414 RVA: 0x00002D9F File Offset: 0x00000F9F
		public override InstructionNode GetNode()
		{
			return null;
		}

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x0600096F RID: 2415 RVA: 0x0002BACB File Offset: 0x00029CCB
		public override string InstructionName
		{
			get
			{
				return this.behaviorName;
			}
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x0002BAD3 File Offset: 0x00029CD3
		public override void GiveInstruction(Context context)
		{
			base.GiveInstruction(context);
			if (base.Entity)
			{
				base.Entity.Components.GoapController.CurrentBehaviorNode = this;
			}
		}

		// Token: 0x06000971 RID: 2417 RVA: 0x0002BAFF File Offset: 0x00029CFF
		public override void RescindInstruction(Context context)
		{
			if (base.Entity)
			{
				base.Entity.Components.GoapController.CurrentBehaviorNode = null;
			}
			base.RescindInstruction(context);
		}

		// Token: 0x06000972 RID: 2418 RVA: 0x0002BB2C File Offset: 0x00029D2C
		public override void AddNodeGoals(NpcEntity entity)
		{
			foreach (global::UnityEngine.Object @object in this.InstructionGoals)
			{
				if (@object)
				{
					Goal goal = ((IGoalBuilder)@object).GetGoal(entity);
					if (entity.Components.GoapController.AddGoal(goal))
					{
						List<Goal> list;
						if (this.AddedGoalsByNpcEntity.TryGetValue(entity, out list))
						{
							list.Add(goal);
						}
						else
						{
							this.AddedGoalsByNpcEntity[entity] = new List<Goal> { goal };
						}
					}
				}
			}
		}

		// Token: 0x06000973 RID: 2419 RVA: 0x0002BBD4 File Offset: 0x00029DD4
		public override void RemoveNodeGoals(NpcEntity entity)
		{
			List<Goal> list;
			if (this.AddedGoalsByNpcEntity.TryGetValue(entity, out list))
			{
				foreach (Goal goal in list)
				{
					entity.Components.GoapController.RemoveGoal(goal);
				}
				this.AddedGoalsByNpcEntity[entity].Clear();
			}
		}

		// Token: 0x040007B1 RID: 1969
		[SerializeField]
		private string behaviorName;
	}
}
