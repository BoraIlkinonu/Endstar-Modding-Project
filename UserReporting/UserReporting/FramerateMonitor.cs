using System;
using UnityEngine;

// Token: 0x02000002 RID: 2
public class FramerateMonitor : UserReportingMonitor
{
	// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	public FramerateMonitor()
	{
		this.MaximumDurationInSeconds = 10f;
		this.MinimumFramerate = 15f;
	}

	// Token: 0x06000002 RID: 2 RVA: 0x00002070 File Offset: 0x00000270
	private void Update()
	{
		float deltaTime = Time.deltaTime;
		if (1f / deltaTime < this.MinimumFramerate)
		{
			this.duration += deltaTime;
		}
		else
		{
			this.duration = 0f;
		}
		if (this.duration > this.MaximumDurationInSeconds)
		{
			this.duration = 0f;
			base.Trigger();
		}
	}

	// Token: 0x04000001 RID: 1
	private float duration;

	// Token: 0x04000002 RID: 2
	public float MaximumDurationInSeconds;

	// Token: 0x04000003 RID: 3
	public float MinimumFramerate;
}
