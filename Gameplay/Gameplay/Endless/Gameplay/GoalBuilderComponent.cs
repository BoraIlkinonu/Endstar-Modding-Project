using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200018F RID: 399
	public abstract class GoalBuilderComponent : MonoBehaviour, IGoalBuilder
	{
		// Token: 0x170001BD RID: 445
		// (get) Token: 0x0600091D RID: 2333 RVA: 0x0002A9F0 File Offset: 0x00028BF0
		public GenericWorldState DesiredWorldState
		{
			get
			{
				return MonoBehaviourSingleton<WorldStateDictionary>.Instance[this.DesiredWorldStateObject];
			}
		}

		// Token: 0x0600091E RID: 2334 RVA: 0x0002AA02 File Offset: 0x00028C02
		protected virtual float Priority(NpcEntity entity)
		{
			return this.BasePriority;
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void Activate(NpcEntity entity)
		{
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void Deactivate(NpcEntity entity)
		{
		}

		// Token: 0x06000921 RID: 2337 RVA: 0x0002AA0A File Offset: 0x00028C0A
		public Goal GetGoal(NpcEntity entity)
		{
			return new CoreGoal(this.GoalName, this.DesiredWorldState, entity, new Func<NpcEntity, float>(this.Priority), new Action<NpcEntity>(this.Activate), new Action<NpcEntity>(this.Deactivate));
		}

		// Token: 0x0400077B RID: 1915
		[SerializeField]
		protected string GoalName;

		// Token: 0x0400077C RID: 1916
		[SerializeField]
		protected WorldState DesiredWorldStateObject;

		// Token: 0x0400077D RID: 1917
		[SerializeField]
		protected float BasePriority;
	}
}
