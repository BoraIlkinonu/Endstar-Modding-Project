using System;

namespace Endless.Gameplay
{
	// Token: 0x0200018B RID: 395
	public class CoreGoal : Goal
	{
		// Token: 0x06000910 RID: 2320 RVA: 0x0002A8AC File Offset: 0x00028AAC
		public CoreGoal(string goalName, GenericWorldState worldState, NpcEntity npcEntity, Func<NpcEntity, float> priority, Action<NpcEntity> activate, Action<NpcEntity> deactivate)
			: base(goalName, worldState, npcEntity)
		{
			this.activate = activate;
			this.deactivate = deactivate;
			this.priority = priority;
		}

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x06000911 RID: 2321 RVA: 0x0002A8CF File Offset: 0x00028ACF
		public override float Priority
		{
			get
			{
				return this.priority(this.NpcEntity);
			}
		}

		// Token: 0x06000912 RID: 2322 RVA: 0x0002A8E2 File Offset: 0x00028AE2
		public override void Activate()
		{
			Action<NpcEntity> action = this.activate;
			if (action == null)
			{
				return;
			}
			action(this.NpcEntity);
		}

		// Token: 0x06000913 RID: 2323 RVA: 0x0002A8FA File Offset: 0x00028AFA
		public override void GoalTerminated(Goal.TerminationCode code)
		{
			base.GoalTerminated(code);
			Action<NpcEntity> action = this.deactivate;
			if (action != null)
			{
				action(this.NpcEntity);
			}
			Action<Goal, Goal.TerminationCode> onGoalTerminated = this.OnGoalTerminated;
			if (onGoalTerminated == null)
			{
				return;
			}
			onGoalTerminated(this, code);
		}

		// Token: 0x0400076D RID: 1901
		private readonly Func<NpcEntity, float> priority;

		// Token: 0x0400076E RID: 1902
		private readonly Action<NpcEntity> activate;

		// Token: 0x0400076F RID: 1903
		private readonly Action<NpcEntity> deactivate;
	}
}
