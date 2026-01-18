using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Endless.Shared;

namespace Endless.Gameplay;

public class LoadTimeTester : MonoBehaviourSingleton<LoadTimeTester>
{
	public class StopwatchData
	{
		public Stopwatch Stopwatch = new Stopwatch();

		public double TimeElapsedLastCheck;

		private int depth;

		public StopwatchData(int depth = 0)
		{
			this.depth = depth;
		}

		public string GetDepthPrefix()
		{
			return string.Concat(Enumerable.Repeat("      ", depth));
		}

		public double GetTimeDelta()
		{
			double totalSeconds = Stopwatch.Elapsed.TotalSeconds;
			double result = totalSeconds - TimeElapsedLastCheck;
			TimeElapsedLastCheck = totalSeconds;
			return result;
		}
	}

	public const string MASTER_LOAD = "MatchLoad";

	private const bool SHOULD_TRACK = false;

	private Dictionary<string, StopwatchData> stopWatches = new Dictionary<string, StopwatchData>();

	public void StartTracking(string loadKey)
	{
	}

	public void LogTimeDelta(string message, string loadKey)
	{
	}

	public void Pause(string loadKey)
	{
	}

	public void Resume(string loadKey)
	{
	}

	public void StopTracking(string loadKey)
	{
	}
}
