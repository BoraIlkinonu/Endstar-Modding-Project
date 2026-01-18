using System;

namespace Unity.Cloud.UserReporting.Client
{
	// Token: 0x02000032 RID: 50
	public class UserReportingClientConfiguration
	{
		// Token: 0x060001BA RID: 442 RVA: 0x00008DCC File Offset: 0x00006FCC
		public UserReportingClientConfiguration()
		{
			this.MaximumEventCount = 100;
			this.MaximumMeasureCount = 300;
			this.FramesPerMeasure = 60;
			this.MaximumScreenshotCount = 10;
		}

		// Token: 0x060001BB RID: 443 RVA: 0x00008DF7 File Offset: 0x00006FF7
		public UserReportingClientConfiguration(int maximumEventCount, int maximumMeasureCount, int framesPerMeasure, int maximumScreenshotCount)
		{
			this.MaximumEventCount = maximumEventCount;
			this.MaximumMeasureCount = maximumMeasureCount;
			this.FramesPerMeasure = framesPerMeasure;
			this.MaximumScreenshotCount = maximumScreenshotCount;
		}

		// Token: 0x060001BC RID: 444 RVA: 0x00008E1C File Offset: 0x0000701C
		public UserReportingClientConfiguration(int maximumEventCount, MetricsGatheringMode metricsGatheringMode, int maximumMeasureCount, int framesPerMeasure, int maximumScreenshotCount)
		{
			this.MaximumEventCount = maximumEventCount;
			this.MetricsGatheringMode = metricsGatheringMode;
			this.MaximumMeasureCount = maximumMeasureCount;
			this.FramesPerMeasure = framesPerMeasure;
			this.MaximumScreenshotCount = maximumScreenshotCount;
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060001BD RID: 445 RVA: 0x00008E49 File Offset: 0x00007049
		// (set) Token: 0x060001BE RID: 446 RVA: 0x00008E51 File Offset: 0x00007051
		public int FramesPerMeasure { get; internal set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060001BF RID: 447 RVA: 0x00008E5A File Offset: 0x0000705A
		// (set) Token: 0x060001C0 RID: 448 RVA: 0x00008E62 File Offset: 0x00007062
		public int MaximumEventCount { get; internal set; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060001C1 RID: 449 RVA: 0x00008E6B File Offset: 0x0000706B
		// (set) Token: 0x060001C2 RID: 450 RVA: 0x00008E73 File Offset: 0x00007073
		public int MaximumMeasureCount { get; internal set; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060001C3 RID: 451 RVA: 0x00008E7C File Offset: 0x0000707C
		// (set) Token: 0x060001C4 RID: 452 RVA: 0x00008E84 File Offset: 0x00007084
		public int MaximumScreenshotCount { get; internal set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060001C5 RID: 453 RVA: 0x00008E8D File Offset: 0x0000708D
		// (set) Token: 0x060001C6 RID: 454 RVA: 0x00008E95 File Offset: 0x00007095
		public MetricsGatheringMode MetricsGatheringMode { get; internal set; }
	}
}
