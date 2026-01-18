using System;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public class FidgetStrategy : IActionStrategy
{
	private const float boredomFloor = 5f;

	private const float boredomCeiling = 15f;

	private readonly NpcEntity entity;

	private readonly CountdownTimer timer;

	private float boredomThreshold;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public FidgetStrategy(NpcEntity entity)
	{
		this.entity = entity;
		timer = new CountdownTimer(2f);
		CountdownTimer countdownTimer = timer;
		countdownTimer.OnTimerStop = (Action)Delegate.Combine(countdownTimer.OnTimerStop, (Action)delegate
		{
			Status = GoapAction.Status.InProgress;
		});
		CountdownTimer countdownTimer2 = timer;
		countdownTimer2.OnTimerStart = (Action)Delegate.Combine(countdownTimer2.OnTimerStart, (Action)delegate
		{
			Status = GoapAction.Status.Complete;
		});
		RandomizeBoredomThreshold();
		GetCost = GetCost_Imp;
	}

	public void Start()
	{
		entity.NpcBlackboard.Set(NpcBlackboard.Key.BoredomTime, 0f);
		AnimationClipInfo randomClip = entity.Fidgets.GetRandomClip();
		entity.Components.Animator.SetTrigger(NpcAnimator.Fidget);
		entity.Components.Animator.SetInteger(NpcAnimator.FidgetInt, randomClip.ClipIndex);
		entity.Components.IndividualStateUpdater.GetCurrentState().fidget = randomClip.ClipIndex + 1;
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
		RandomizeBoredomThreshold();
	}

	private float GetCost_Imp()
	{
		if (!(entity.NpcBlackboard.GetValueOrDefault(NpcBlackboard.Key.BoredomTime, 0f) > boredomThreshold))
		{
			return float.MaxValue;
		}
		return 0f;
	}

	private void RandomizeBoredomThreshold()
	{
		boredomThreshold = math.lerp(5f, 15f, UnityEngine.Random.value);
	}
}
