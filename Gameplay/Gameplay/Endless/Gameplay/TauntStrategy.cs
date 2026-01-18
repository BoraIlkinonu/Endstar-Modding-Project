using System;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000187 RID: 391
	public class TauntStrategy : IActionStrategy
	{
		// Token: 0x060008EF RID: 2287 RVA: 0x00029DCC File Offset: 0x00027FCC
		public TauntStrategy(NpcEntity entity)
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
			this.SetNextTauntTime();
			this.GetCost = new Func<float>(this.GetCost_Imp);
		}

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x060008F0 RID: 2288 RVA: 0x00029E5C File Offset: 0x0002805C
		public Func<float> GetCost { get; }

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x060008F1 RID: 2289 RVA: 0x00029E64 File Offset: 0x00028064
		// (set) Token: 0x060008F2 RID: 2290 RVA: 0x00029E6C File Offset: 0x0002806C
		public GoapAction.Status Status { get; private set; }

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x060008F3 RID: 2291 RVA: 0x0001965C File Offset: 0x0001785C
		public bool Failed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x00029E78 File Offset: 0x00028078
		public void Start()
		{
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, true);
			AnimationClipInfo randomClip = this.entity.TauntClipSet.GetRandomClip();
			this.entity.Components.Animator.SetTrigger(NpcAnimator.Taunt);
			this.entity.Components.Animator.SetInteger(NpcAnimator.TauntInt, randomClip.ClipIndex);
			this.entity.Components.IndividualStateUpdater.GetCurrentState().taunt = randomClip.ClipIndex + 1;
			this.timer.Reset(randomClip.FrameLength * NetClock.FixedDeltaTime);
			this.timer.Start();
			this.entity.Components.GoapController.LockPlan = true;
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x00029F5D File Offset: 0x0002815D
		public void Update(float deltaTime)
		{
			this.timer.Tick(deltaTime);
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x00029F6B File Offset: 0x0002816B
		public void Stop()
		{
			this.entity.Components.GoapController.LockPlan = false;
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, false);
			this.SetNextTauntTime();
		}

		// Token: 0x060008F7 RID: 2295 RVA: 0x00029FA4 File Offset: 0x000281A4
		private float GetCost_Imp()
		{
			if (Time.time <= this.nextTauntTime)
			{
				return float.MaxValue;
			}
			return 0f;
		}

		// Token: 0x060008F8 RID: 2296 RVA: 0x00029FBE File Offset: 0x000281BE
		private void SetNextTauntTime()
		{
			this.nextTauntTime = Time.time + 5f + math.lerp(0f, 10f, global::UnityEngine.Random.value);
		}

		// Token: 0x0400075A RID: 1882
		private float nextTauntTime;

		// Token: 0x0400075B RID: 1883
		private readonly NpcEntity entity;

		// Token: 0x0400075C RID: 1884
		private readonly CountdownTimer timer;
	}
}
