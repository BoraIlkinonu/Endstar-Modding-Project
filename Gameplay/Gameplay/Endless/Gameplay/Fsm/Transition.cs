using System;
using System.Collections.Generic;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x02000440 RID: 1088
	public class Transition
	{
		// Token: 0x1700057E RID: 1406
		// (get) Token: 0x06001B38 RID: 6968 RVA: 0x0007BC98 File Offset: 0x00079E98
		public FsmState TargetState { get; }

		// Token: 0x1700057F RID: 1407
		// (get) Token: 0x06001B39 RID: 6969 RVA: 0x0007BCA0 File Offset: 0x00079EA0
		private List<Func<NpcEntity, bool>> Conditions { get; }

		// Token: 0x17000580 RID: 1408
		// (get) Token: 0x06001B3A RID: 6970 RVA: 0x0007BCA8 File Offset: 0x00079EA8
		public Action<NpcEntity> TransitionAction { get; }

		// Token: 0x06001B3B RID: 6971 RVA: 0x0007BCB0 File Offset: 0x00079EB0
		public Transition(FsmState targetState, List<Func<NpcEntity, bool>> conditions, Action<NpcEntity> transitionAction = null)
		{
			this.TargetState = targetState;
			this.Conditions = conditions;
			this.TransitionAction = transitionAction;
		}

		// Token: 0x06001B3C RID: 6972 RVA: 0x0007BCCD File Offset: 0x00079ECD
		public Transition(FsmState targetState, Func<NpcEntity, bool> condition, Action<NpcEntity> transitionAction = null)
		{
			this.TargetState = targetState;
			this.Conditions = new List<Func<NpcEntity, bool>> { condition };
			this.TransitionAction = transitionAction;
		}

		// Token: 0x06001B3D RID: 6973 RVA: 0x0007BCF8 File Offset: 0x00079EF8
		public bool AreConditionsMet(NpcEntity entity)
		{
			for (int i = 0; i < this.Conditions.Count; i++)
			{
				Func<NpcEntity, bool> func = this.Conditions[i];
				if (func != null && !func(entity))
				{
					return false;
				}
			}
			return true;
		}
	}
}
