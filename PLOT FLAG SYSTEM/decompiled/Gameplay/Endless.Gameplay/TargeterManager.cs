using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

public class TargeterManager : EndlessBehaviourSingleton<TargeterManager>, NetClock.ISimulateFrameLateSubscriber
{
	private const int MAX_TICK_OFFSETS = 10;

	private const int GOAL_TARGETERS_PER_TICK = 20;

	private readonly List<TargeterComponent> targeters = new List<TargeterComponent>();

	private int tickOffsetRange = 1;

	public int TickOffsetRange
	{
		get
		{
			return tickOffsetRange;
		}
		private set
		{
			if (tickOffsetRange != value)
			{
				tickOffsetRange = value;
				UpdateNpcTickOffset(tickOffsetRange);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		NetClock.Register(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		NetClock.Unregister(this);
	}

	public void RegisterTargeter(TargeterComponent targeter)
	{
		targeters.Add(targeter);
	}

	public void UnregisterTargeter(TargeterComponent targeter)
	{
		targeters.Remove(targeter);
	}

	private void UpdateNpcTickOffset(int offsetRange)
	{
		if (offsetRange == 1)
		{
			targeters.ForEach(delegate(TargeterComponent targeter)
			{
				targeter.TickOffset = 0;
			});
			return;
		}
		int num = 0;
		for (int num2 = 0; num2 < targeters.Count; num2++)
		{
			targeters[num2].TickOffset = num++;
			if (num >= offsetRange)
			{
				num = 0;
			}
		}
	}

	public void SimulateFrameLate(uint frame)
	{
		TickOffsetRange = Mathf.Clamp(targeters.Count / 20, 1, 10);
	}
}
