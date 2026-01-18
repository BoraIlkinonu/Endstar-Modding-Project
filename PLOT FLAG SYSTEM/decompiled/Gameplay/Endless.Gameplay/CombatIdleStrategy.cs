using System;
using UnityEngine;

namespace Endless.Gameplay;

public class CombatIdleStrategy : IActionStrategy
{
	private readonly CountdownTimer timer;

	private readonly NpcEntity entity;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public CombatIdleStrategy(float duration, NpcEntity entity)
	{
		timer = new CountdownTimer(duration);
		CountdownTimer countdownTimer = timer;
		countdownTimer.OnTimerStart = (Action)Delegate.Combine(countdownTimer.OnTimerStart, (Action)delegate
		{
			Status = GoapAction.Status.InProgress;
		});
		CountdownTimer countdownTimer2 = timer;
		countdownTimer2.OnTimerStop = (Action)Delegate.Combine(countdownTimer2.OnTimerStop, (Action)delegate
		{
			Status = GoapAction.Status.Complete;
		});
		this.entity = entity;
	}

	public void Start()
	{
		entity.Components.PathFollower.StopPath();
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: true);
		timer.Start();
	}

	public void Update(float deltaTime)
	{
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		Quaternion to = Quaternion.LookRotation(entity.Target.Position - entity.transform.position);
		entity.transform.rotation = Quaternion.RotateTowards(entity.transform.rotation, to, entity.Settings.RotationSpeed * deltaTime);
		timer.Tick(deltaTime);
	}

	public void Stop()
	{
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: false);
		entity.NpcBlackboard.Set(NpcBlackboard.Key.BoredomTime, timer.CountdownDuration + entity.NpcBlackboard.GetValueOrDefault(NpcBlackboard.Key.BoredomTime, 0f));
	}
}
