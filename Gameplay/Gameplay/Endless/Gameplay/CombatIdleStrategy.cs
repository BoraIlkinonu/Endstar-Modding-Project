using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000173 RID: 371
	public class CombatIdleStrategy : IActionStrategy
	{
		// Token: 0x1700018F RID: 399
		// (get) Token: 0x0600084D RID: 2125 RVA: 0x0002770C File Offset: 0x0002590C
		public Func<float> GetCost { get; }

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x0600084E RID: 2126 RVA: 0x00027714 File Offset: 0x00025914
		// (set) Token: 0x0600084F RID: 2127 RVA: 0x0002771C File Offset: 0x0002591C
		public GoapAction.Status Status { get; private set; }

		// Token: 0x06000850 RID: 2128 RVA: 0x00027728 File Offset: 0x00025928
		public CombatIdleStrategy(float duration, NpcEntity entity)
		{
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
			this.entity = entity;
		}

		// Token: 0x06000851 RID: 2129 RVA: 0x0002779C File Offset: 0x0002599C
		public void Start()
		{
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, true);
			this.timer.Start();
		}

		// Token: 0x06000852 RID: 2130 RVA: 0x000277DC File Offset: 0x000259DC
		public void Update(float deltaTime)
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			Quaternion quaternion = Quaternion.LookRotation(this.entity.Target.Position - this.entity.transform.position);
			this.entity.transform.rotation = Quaternion.RotateTowards(this.entity.transform.rotation, quaternion, this.entity.Settings.RotationSpeed * deltaTime);
			this.timer.Tick(deltaTime);
		}

		// Token: 0x06000853 RID: 2131 RVA: 0x00027874 File Offset: 0x00025A74
		public void Stop()
		{
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, false);
			this.entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.BoredomTime, this.timer.CountdownDuration + this.entity.NpcBlackboard.GetValueOrDefault<float>(NpcBlackboard.Key.BoredomTime, 0f));
		}

		// Token: 0x040006ED RID: 1773
		private readonly CountdownTimer timer;

		// Token: 0x040006EE RID: 1774
		private readonly NpcEntity entity;
	}
}
