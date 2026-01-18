using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Endless.Shared;

namespace Endless.Gameplay
{
	// Token: 0x020002CA RID: 714
	public class LoadTimeTester : MonoBehaviourSingleton<LoadTimeTester>
	{
		// Token: 0x06001036 RID: 4150 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void StartTracking(string loadKey)
		{
		}

		// Token: 0x06001037 RID: 4151 RVA: 0x0005231C File Offset: 0x0005051C
		public void LogTimeDelta(string message, string loadKey)
		{
		}

		// Token: 0x06001038 RID: 4152 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void Pause(string loadKey)
		{
		}

		// Token: 0x06001039 RID: 4153 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void Resume(string loadKey)
		{
		}

		// Token: 0x0600103A RID: 4154 RVA: 0x0005232C File Offset: 0x0005052C
		public void StopTracking(string loadKey)
		{
		}

		// Token: 0x04000DE6 RID: 3558
		public const string MASTER_LOAD = "MatchLoad";

		// Token: 0x04000DE7 RID: 3559
		private const bool SHOULD_TRACK = false;

		// Token: 0x04000DE8 RID: 3560
		private Dictionary<string, LoadTimeTester.StopwatchData> stopWatches = new Dictionary<string, LoadTimeTester.StopwatchData>();

		// Token: 0x020002CB RID: 715
		public class StopwatchData
		{
			// Token: 0x0600103C RID: 4156 RVA: 0x0005234C File Offset: 0x0005054C
			public StopwatchData(int depth = 0)
			{
				this.depth = depth;
			}

			// Token: 0x0600103D RID: 4157 RVA: 0x00052366 File Offset: 0x00050566
			public string GetDepthPrefix()
			{
				return string.Concat(Enumerable.Repeat<string>("      ", this.depth));
			}

			// Token: 0x0600103E RID: 4158 RVA: 0x00052380 File Offset: 0x00050580
			public double GetTimeDelta()
			{
				double totalSeconds = this.Stopwatch.Elapsed.TotalSeconds;
				double num = totalSeconds - this.TimeElapsedLastCheck;
				this.TimeElapsedLastCheck = totalSeconds;
				return num;
			}

			// Token: 0x04000DE9 RID: 3561
			public Stopwatch Stopwatch = new Stopwatch();

			// Token: 0x04000DEA RID: 3562
			public double TimeElapsedLastCheck;

			// Token: 0x04000DEB RID: 3563
			private int depth;
		}
	}
}
