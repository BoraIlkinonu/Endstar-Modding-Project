using System;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000172 RID: 370
	public class CombatFidgetStrategy : IActionStrategy
	{
		// Token: 0x06000842 RID: 2114 RVA: 0x000274E0 File Offset: 0x000256E0
		public CombatFidgetStrategy(NpcEntity entity)
		{
			this.entity = entity;
			this.timer = new CountdownTimer(2f);
			CountdownTimer countdownTimer = this.timer;
			countdownTimer.OnTimerStop = (Action)Delegate.Combine(countdownTimer.OnTimerStop, new Action(delegate
			{
				this.Status = GoapAction.Status.Complete;
			}));
			CountdownTimer countdownTimer2 = this.timer;
			countdownTimer2.OnTimerStart = (Action)Delegate.Combine(countdownTimer2.OnTimerStart, new Action(delegate
			{
				this.Status = GoapAction.Status.InProgress;
			}));
			this.SetNextFidgetTime();
			this.GetCost = new Func<float>(this.GetCost_Imp);
		}

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x06000843 RID: 2115 RVA: 0x00027570 File Offset: 0x00025770
		public Func<float> GetCost { get; }

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x06000844 RID: 2116 RVA: 0x00027578 File Offset: 0x00025778
		// (set) Token: 0x06000845 RID: 2117 RVA: 0x00027580 File Offset: 0x00025780
		public GoapAction.Status Status { get; private set; }

		// Token: 0x06000846 RID: 2118 RVA: 0x0002758C File Offset: 0x0002578C
		public void Start()
		{
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, true);
			AnimationClipInfo randomClip = this.entity.Fidgets.GetRandomClip();
			this.entity.Components.Animator.SetTrigger(NpcAnimator.Fidget);
			this.entity.Components.Animator.SetInteger(NpcAnimator.FidgetInt, randomClip.ClipIndex);
			this.entity.Components.IndividualStateUpdater.GetCurrentState().fidget = randomClip.ClipIndex + 1;
			this.timer.Reset(randomClip.FrameLength * NetClock.FixedDeltaTime);
			this.timer.Start();
			this.entity.Components.GoapController.LockPlan = true;
		}

		// Token: 0x06000847 RID: 2119 RVA: 0x00027671 File Offset: 0x00025871
		public void Update(float deltaTime)
		{
			this.timer.Tick(deltaTime);
		}

		// Token: 0x06000848 RID: 2120 RVA: 0x0002767F File Offset: 0x0002587F
		public void Stop()
		{
			this.entity.Components.GoapController.LockPlan = false;
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, false);
			this.SetNextFidgetTime();
		}

		// Token: 0x06000849 RID: 2121 RVA: 0x000276B8 File Offset: 0x000258B8
		private void SetNextFidgetTime()
		{
			this.nextFidgetTime = Time.time + 5f + math.lerp(0f, 10f, global::UnityEngine.Random.value);
		}

		// Token: 0x0600084A RID: 2122 RVA: 0x000276E0 File Offset: 0x000258E0
		private float GetCost_Imp()
		{
			if (Time.time <= this.nextFidgetTime)
			{
				return float.MaxValue;
			}
			return 0f;
		}

		// Token: 0x040006E8 RID: 1768
		private float nextFidgetTime;

		// Token: 0x040006E9 RID: 1769
		private readonly NpcEntity entity;

		// Token: 0x040006EA RID: 1770
		private readonly CountdownTimer timer;
	}
}
