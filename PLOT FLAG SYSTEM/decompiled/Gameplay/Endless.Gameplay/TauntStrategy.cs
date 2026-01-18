using System;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public class TauntStrategy : IActionStrategy
{
	private float nextTauntTime;

	private readonly NpcEntity entity;

	private readonly CountdownTimer timer;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public bool Failed => false;

	public TauntStrategy(NpcEntity entity)
	{
		this.entity = entity;
		timer = new CountdownTimer(2f);
		CountdownTimer countdownTimer = timer;
		countdownTimer.OnTimerStop = (Action)Delegate.Combine(countdownTimer.OnTimerStop, (Action)delegate
		{
			Status = GoapAction.Status.Complete;
		});
		CountdownTimer countdownTimer2 = timer;
		countdownTimer2.OnTimerStart = (Action)Delegate.Combine(countdownTimer2.OnTimerStart, (Action)delegate
		{
			Status = GoapAction.Status.InProgress;
		});
		SetNextTauntTime();
		GetCost = GetCost_Imp;
	}

	public void Start()
	{
		entity.Components.PathFollower.StopPath();
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: true);
		AnimationClipInfo randomClip = entity.TauntClipSet.GetRandomClip();
		entity.Components.Animator.SetTrigger(NpcAnimator.Taunt);
		entity.Components.Animator.SetInteger(NpcAnimator.TauntInt, randomClip.ClipIndex);
		entity.Components.IndividualStateUpdater.GetCurrentState().taunt = randomClip.ClipIndex + 1;
		timer.Reset((float)randomClip.FrameLength * NetClock.FixedDeltaTime);
		timer.Start();
		entity.Components.GoapController.LockPlan = true;
	}

	public void Update(float deltaTime)
	{
		timer.Tick(deltaTime);
	}

	public void Stop()
	{
		entity.Components.GoapController.LockPlan = false;
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: false);
		SetNextTauntTime();
	}

	private float GetCost_Imp()
	{
		if (!(Time.time > nextTauntTime))
		{
			return float.MaxValue;
		}
		return 0f;
	}

	private void SetNextTauntTime()
	{
		nextTauntTime = Time.time + 5f + math.lerp(0f, 10f, UnityEngine.Random.value);
	}
}
