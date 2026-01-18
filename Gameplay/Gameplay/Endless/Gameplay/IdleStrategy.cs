using System;

namespace Endless.Gameplay
{
	// Token: 0x0200017D RID: 381
	public class IdleStrategy : IActionStrategy
	{
		// Token: 0x06000898 RID: 2200 RVA: 0x00028434 File Offset: 0x00026634
		public IdleStrategy(NpcBlackboard blackboard, float duration)
		{
			this.blackboard = blackboard;
			this.timer = new CountdownTimer(duration);
			CountdownTimer countdownTimer = this.timer;
			countdownTimer.OnTimerStart = (Action)Delegate.Combine(countdownTimer.OnTimerStart, new Action(delegate
			{
				this.Status = GoapAction.Status.InProgress;
			}));
			CountdownTimer countdownTimer2 = this.timer;
			countdownTimer2.OnTimerStop = (Action)Delegate.Combine(countdownTimer2.OnTimerStop, new Action(delegate
			{
				this.Status = GoapAction.Status.Complete;
			}));
		}

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06000899 RID: 2201 RVA: 0x00002D9F File Offset: 0x00000F9F
		public Func<float> GetCost
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x0600089A RID: 2202 RVA: 0x000284A8 File Offset: 0x000266A8
		// (set) Token: 0x0600089B RID: 2203 RVA: 0x000284B0 File Offset: 0x000266B0
		public GoapAction.Status Status { get; private set; }

		// Token: 0x0600089C RID: 2204 RVA: 0x000284B9 File Offset: 0x000266B9
		public void Start()
		{
			this.timer.Start();
		}

		// Token: 0x0600089D RID: 2205 RVA: 0x000284C6 File Offset: 0x000266C6
		public void Update(float deltaTime)
		{
			this.timer.Tick(deltaTime);
		}

		// Token: 0x0600089E RID: 2206 RVA: 0x000284D4 File Offset: 0x000266D4
		public void Stop()
		{
			this.blackboard.Set<float>(NpcBlackboard.Key.BoredomTime, this.timer.CountdownDuration + this.blackboard.GetValueOrDefault<float>(NpcBlackboard.Key.BoredomTime, 0f));
		}

		// Token: 0x04000722 RID: 1826
		private readonly CountdownTimer timer;

		// Token: 0x04000723 RID: 1827
		private readonly NpcBlackboard blackboard;
	}
}
