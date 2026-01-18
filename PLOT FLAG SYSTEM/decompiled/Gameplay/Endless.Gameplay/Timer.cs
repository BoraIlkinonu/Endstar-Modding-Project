using System;

namespace Endless.Gameplay;

public abstract class Timer
{
	protected float initialTime;

	public Action OnTimerStart = delegate
	{
	};

	public Action OnTimerStop = delegate
	{
	};

	public float Time { get; set; }

	public bool IsRunning { get; protected set; }

	public float Progress => Time / initialTime;

	protected Timer(float value)
	{
		initialTime = value;
		IsRunning = false;
	}

	public void Start()
	{
		Time = initialTime;
		if (!IsRunning)
		{
			IsRunning = true;
			OnTimerStart();
		}
	}

	public void Stop()
	{
		if (IsRunning)
		{
			IsRunning = false;
			OnTimerStop();
		}
	}

	public void Resume()
	{
		IsRunning = true;
	}

	public void Pause()
	{
		IsRunning = false;
	}

	public abstract void Tick(float deltaTime);
}
