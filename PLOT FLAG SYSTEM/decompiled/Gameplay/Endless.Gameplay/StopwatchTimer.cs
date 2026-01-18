namespace Endless.Gameplay;

public class StopwatchTimer : Timer
{
	public StopwatchTimer()
		: base(0f)
	{
	}

	public override void Tick(float deltaTime)
	{
		if (base.IsRunning)
		{
			base.Time += deltaTime;
		}
	}

	public void Reset()
	{
		base.Time = 0f;
	}

	public float GetTime()
	{
		return base.Time;
	}
}
