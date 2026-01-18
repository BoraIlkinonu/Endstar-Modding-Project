namespace Endless.Gameplay;

public class CountdownTimer : Timer
{
	public bool IsFinished => base.Time <= 0f;

	public float CountdownDuration => initialTime;

	public CountdownTimer(float value)
		: base(value)
	{
	}

	public override void Tick(float deltaTime)
	{
		if (base.IsRunning && base.Time > 0f)
		{
			base.Time -= deltaTime;
		}
		if (base.IsRunning && base.Time <= 0f)
		{
			Stop();
		}
	}

	public void Reset()
	{
		base.Time = initialTime;
	}

	public void Reset(float newTime)
	{
		initialTime = newTime;
		Reset();
	}
}
