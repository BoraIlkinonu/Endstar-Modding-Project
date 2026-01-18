using System;
using System.Collections.Generic;

namespace Endless.Gameplay
{
	// Token: 0x0200018C RID: 396
	public abstract class Goal : IComparable<Goal>
	{
		// Token: 0x06000914 RID: 2324 RVA: 0x0002A92C File Offset: 0x00028B2C
		protected Goal(string goalName, GenericWorldState worldState, NpcEntity npcEntity)
		{
			this.GoalName = goalName;
			this.DesiredWorldState = worldState;
			this.NpcEntity = npcEntity;
		}

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x06000915 RID: 2325 RVA: 0x0002A954 File Offset: 0x00028B54
		public int RecentFailures
		{
			get
			{
				return this.failures.Count;
			}
		}

		// Token: 0x170001BC RID: 444
		// (get) Token: 0x06000916 RID: 2326
		public abstract float Priority { get; }

		// Token: 0x06000917 RID: 2327 RVA: 0x0002A961 File Offset: 0x00028B61
		public bool IsSatisfied()
		{
			return this.DesiredWorldState.Evaluate(this.NpcEntity);
		}

		// Token: 0x06000918 RID: 2328 RVA: 0x0002A974 File Offset: 0x00028B74
		public void UpdateGoal(uint frame)
		{
			uint num;
			if (this.failures.TryPeek(out num) && frame > num + 20U)
			{
				this.failures.Dequeue();
			}
		}

		// Token: 0x06000919 RID: 2329
		public abstract void Activate();

		// Token: 0x0600091A RID: 2330 RVA: 0x0002A9A3 File Offset: 0x00028BA3
		public virtual void GoalTerminated(Goal.TerminationCode code)
		{
			if (code == Goal.TerminationCode.Failed)
			{
				this.failures.Enqueue(NetClock.CurrentFrame);
			}
		}

		// Token: 0x0600091B RID: 2331 RVA: 0x0002A9BC File Offset: 0x00028BBC
		public int CompareTo(Goal other)
		{
			if (this == other)
			{
				return 0;
			}
			if (other == null)
			{
				return 1;
			}
			return this.Priority.CompareTo(other.Priority);
		}

		// Token: 0x04000770 RID: 1904
		protected const int MAX_GOAL_PRIORITY = 100;

		// Token: 0x04000771 RID: 1905
		private const uint FAILURE_MEMORY_FRAMES = 20U;

		// Token: 0x04000772 RID: 1906
		private readonly Queue<uint> failures = new Queue<uint>();

		// Token: 0x04000773 RID: 1907
		public readonly string GoalName;

		// Token: 0x04000774 RID: 1908
		public readonly GenericWorldState DesiredWorldState;

		// Token: 0x04000775 RID: 1909
		public readonly NpcEntity NpcEntity;

		// Token: 0x04000776 RID: 1910
		public Action<Goal, Goal.TerminationCode> OnGoalTerminated;

		// Token: 0x0200018D RID: 397
		public enum TerminationCode
		{
			// Token: 0x04000778 RID: 1912
			Completed,
			// Token: 0x04000779 RID: 1913
			Interrupted,
			// Token: 0x0400077A RID: 1914
			Failed
		}
	}
}
