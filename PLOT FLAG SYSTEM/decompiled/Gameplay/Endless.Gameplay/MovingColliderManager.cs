using System.Collections.Generic;

namespace Endless.Gameplay;

public class MovingColliderManager : EndlessBehaviourSingleton<MovingColliderManager>, IStartSubscriber, IGameEndSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber
{
	private HashSet<MovingCollider> movingCollidersThisFrame = new HashSet<MovingCollider>();

	private bool playing;

	private void OnEnable()
	{
		NetClock.Register(this);
	}

	private void OnDisable()
	{
		NetClock.Unregister(this);
	}

	void NetClock.ISimulateFrameEnvironmentSubscriber.SimulateFrameEnvironment(uint frame)
	{
		if (!playing)
		{
			return;
		}
		foreach (MovingCollider item in movingCollidersThisFrame)
		{
			item.HandleMovementFrame();
		}
	}

	void IStartSubscriber.EndlessStart()
	{
		playing = true;
	}

	void IGameEndSubscriber.EndlessGameEnd()
	{
		playing = false;
		movingCollidersThisFrame.Clear();
	}

	public void ColliderMoved(MovingCollider movingCollider)
	{
		if (playing)
		{
			movingCollidersThisFrame.Add(movingCollider);
		}
	}
}
