using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MatchmakingClientSDK
{
	// Token: 0x0200005F RID: 95
	public static class Utils
	{
		// Token: 0x04000268 RID: 616
		public static readonly ConcurrentQueue<Stopwatch> Stopwatches = new ConcurrentQueue<Stopwatch>();
	}
}
