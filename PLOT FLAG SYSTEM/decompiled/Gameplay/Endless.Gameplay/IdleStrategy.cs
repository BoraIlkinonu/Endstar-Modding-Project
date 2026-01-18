using System;

namespace Endless.Gameplay;

public class IdleStrategy : IActionStrategy
{
	private readonly CountdownTimer timer;

	private readonly NpcBlackboard blackboard;

	public Func<float> GetCost => null;

	public GoapAction.Status Status { get; private set; }

	public IdleStrategy(NpcBlackboard blackboard, float duration)
	{
		this.blackboard = blackboard;
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
	}

	public void Start()
	{
		timer.Start();
	}

	public void Update(float deltaTime)
	{
		timer.Tick(deltaTime);
	}

	public void Stop()
	{
		blackboard.Set(NpcBlackboard.Key.BoredomTime, timer.CountdownDuration + blackboard.GetValueOrDefault(NpcBlackboard.Key.BoredomTime, 0f));
	}
}
