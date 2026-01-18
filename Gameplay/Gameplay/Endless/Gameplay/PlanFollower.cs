using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200019E RID: 414
	public class PlanFollower
	{
		// Token: 0x06000957 RID: 2391 RVA: 0x000030D2 File Offset: 0x000012D2
		public PlanFollower(NpcEntity entity)
		{
		}

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x06000958 RID: 2392 RVA: 0x0002B382 File Offset: 0x00029582
		// (set) Token: 0x06000959 RID: 2393 RVA: 0x0002B38C File Offset: 0x0002958C
		public ActionPlan ActionPlan
		{
			get
			{
				return this.actionPlan;
			}
			set
			{
				if (this.actionPlan != null)
				{
					GoapAction goapAction = this.currentAction;
					if (goapAction != null)
					{
						goapAction.Stop();
					}
					this.currentAction = null;
					this.actionPlan.Goal.GoalTerminated(Goal.TerminationCode.Interrupted);
				}
				this.actionPlan = value;
				ActionPlan actionPlan = this.actionPlan;
				if (actionPlan != null)
				{
					actionPlan.Goal.Activate();
				}
				if (this.actionPlan != null && this.actionPlan.Actions.Count > 0)
				{
					this.currentAction = this.actionPlan.Actions.Pop();
					this.currentAction.Start();
				}
			}
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x0002B424 File Offset: 0x00029624
		public void Tick(uint frame)
		{
			if (this.currentAction != null)
			{
				this.currentAction.Tick(frame);
				switch (this.currentAction.ActionStatus)
				{
				case GoapAction.Status.InProgress:
					break;
				case GoapAction.Status.Complete:
					this.currentAction.Stop();
					this.currentAction = null;
					if (this.ActionPlan.Actions.Count == 0)
					{
						this.ActionPlan.Goal.GoalTerminated(Goal.TerminationCode.Completed);
						this.ActionPlan = null;
					}
					break;
				case GoapAction.Status.Failed:
					this.currentAction.Stop();
					this.currentAction = null;
					this.ActionPlan.Goal.GoalTerminated(Goal.TerminationCode.Failed);
					this.ActionPlan = null;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			if (this.currentAction == null && this.ActionPlan != null && this.ActionPlan.Actions.Count > 0)
			{
				this.currentAction = this.ActionPlan.Actions.Pop();
				this.currentAction.Start();
			}
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x0002B51C File Offset: 0x0002971C
		public void Update()
		{
			GoapAction goapAction = this.currentAction;
			if (goapAction == null)
			{
				return;
			}
			goapAction.Update(Time.deltaTime);
		}

		// Token: 0x040007A0 RID: 1952
		private GoapAction currentAction;

		// Token: 0x040007A1 RID: 1953
		private ActionPlan actionPlan;
	}
}
