using System;

namespace Endless.Gameplay;

public interface IActionStrategy
{
	Func<float> GetCost { get; }

	GoapAction.Status Status { get; }

	void Start()
	{
	}

	void Update(float deltaTime)
	{
	}

	void Tick(uint frame)
	{
	}

	void Stop()
	{
	}
}
