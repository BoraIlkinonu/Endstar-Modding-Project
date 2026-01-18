using System;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000175 RID: 373
	public class FidgetStrategy : IActionStrategy
	{
		// Token: 0x0600085F RID: 2143 RVA: 0x00027BCC File Offset: 0x00025DCC
		public FidgetStrategy(NpcEntity entity)
		{
			this.entity = entity;
			this.timer = new CountdownTimer(2f);
			CountdownTimer countdownTimer = this.timer;
			countdownTimer.OnTimerStop = (Action)Delegate.Combine(countdownTimer.OnTimerStop, new Action(delegate
			{
				this.Status = GoapAction.Status.InProgress;
			}));
			CountdownTimer countdownTimer2 = this.timer;
			countdownTimer2.OnTimerStart = (Action)Delegate.Combine(countdownTimer2.OnTimerStart, new Action(delegate
			{
				this.Status = GoapAction.Status.Complete;
			}));
			this.RandomizeBoredomThreshold();
			this.GetCost = new Func<float>(this.GetCost_Imp);
		}

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x06000860 RID: 2144 RVA: 0x00027C5C File Offset: 0x00025E5C
		public Func<float> GetCost { get; }

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x06000861 RID: 2145 RVA: 0x00027C64 File Offset: 0x00025E64
		// (set) Token: 0x06000862 RID: 2146 RVA: 0x00027C6C File Offset: 0x00025E6C
		public GoapAction.Status Status { get; private set; }

		// Token: 0x06000863 RID: 2147 RVA: 0x00027C78 File Offset: 0x00025E78
		public void Start()
		{
			this.entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.BoredomTime, 0f);
			AnimationClipInfo randomClip = this.entity.Fidgets.GetRandomClip();
			this.entity.Components.Animator.SetTrigger(NpcAnimator.Fidget);
			this.entity.Components.Animator.SetInteger(NpcAnimator.FidgetInt, randomClip.ClipIndex);
			this.entity.Components.IndividualStateUpdater.GetCurrentState().fidget = randomClip.ClipIndex + 1;
			this.timer.Reset(randomClip.FrameLength * NetClock.FixedDeltaTime);
			this.timer.Start();
			this.entity.Components.GoapController.LockPlan = true;
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x00027D43 File Offset: 0x00025F43
		public void Update(float deltaTime)
		{
			this.timer.Tick(deltaTime);
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x00027D51 File Offset: 0x00025F51
		public void Stop()
		{
			this.entity.Components.GoapController.LockPlan = false;
			this.RandomizeBoredomThreshold();
		}

		// Token: 0x06000866 RID: 2150 RVA: 0x00027D6F File Offset: 0x00025F6F
		private float GetCost_Imp()
		{
			if (this.entity.NpcBlackboard.GetValueOrDefault<float>(NpcBlackboard.Key.BoredomTime, 0f) <= this.boredomThreshold)
			{
				return float.MaxValue;
			}
			return 0f;
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x00027D9B File Offset: 0x00025F9B
		private void RandomizeBoredomThreshold()
		{
			this.boredomThreshold = math.lerp(5f, 15f, global::UnityEngine.Random.value);
		}

		// Token: 0x040006F6 RID: 1782
		private const float boredomFloor = 5f;

		// Token: 0x040006F7 RID: 1783
		private const float boredomCeiling = 15f;

		// Token: 0x040006F8 RID: 1784
		private readonly NpcEntity entity;

		// Token: 0x040006F9 RID: 1785
		private readonly CountdownTimer timer;

		// Token: 0x040006FA RID: 1786
		private float boredomThreshold;
	}
}
