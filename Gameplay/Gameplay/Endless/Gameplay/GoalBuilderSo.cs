using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000194 RID: 404
	[CreateAssetMenu(menuName = "ScriptableObject/GoalBuilders/GenericGoalBuilder", fileName = "GoalBuilder")]
	public class GoalBuilderSo : ScriptableObject, IGoalBuilder
	{
		// Token: 0x170001BE RID: 446
		// (get) Token: 0x06000931 RID: 2353 RVA: 0x0002AC74 File Offset: 0x00028E74
		public GenericWorldState DesiredWorldState
		{
			get
			{
				return MonoBehaviourSingleton<WorldStateDictionary>.Instance[this.DesiredWorldStateObject];
			}
		}

		// Token: 0x06000932 RID: 2354 RVA: 0x0002AC86 File Offset: 0x00028E86
		public Goal GetGoal(NpcEntity entity)
		{
			return new CoreGoal(this.GoalName, this.DesiredWorldState, entity, new Func<NpcEntity, float>(this.Priority), new Action<NpcEntity>(this.Activate), new Action<NpcEntity>(this.Deactivate));
		}

		// Token: 0x06000933 RID: 2355 RVA: 0x0002ACC1 File Offset: 0x00028EC1
		protected virtual float Priority(NpcEntity entity)
		{
			return this.basePriority;
		}

		// Token: 0x06000934 RID: 2356 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void Activate(NpcEntity entity)
		{
		}

		// Token: 0x06000935 RID: 2357 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void Deactivate(NpcEntity entity)
		{
		}

		// Token: 0x04000787 RID: 1927
		[SerializeField]
		protected string GoalName;

		// Token: 0x04000788 RID: 1928
		[SerializeField]
		protected WorldState DesiredWorldStateObject;

		// Token: 0x04000789 RID: 1929
		[SerializeField]
		protected float basePriority;
	}
}
