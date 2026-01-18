using System;

namespace Networking.UDP.Utils
{
	// Token: 0x02000042 RID: 66
	public enum NtpLeapIndicator
	{
		// Token: 0x0400017E RID: 382
		NoWarning,
		// Token: 0x0400017F RID: 383
		LastMinuteHas61Seconds,
		// Token: 0x04000180 RID: 384
		LastMinuteHas59Seconds,
		// Token: 0x04000181 RID: 385
		AlarmCondition
	}
}
