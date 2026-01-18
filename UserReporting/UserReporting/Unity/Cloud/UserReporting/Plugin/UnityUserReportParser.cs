using System;
using Unity.Cloud.UserReporting.Plugin.SimpleJson;

namespace Unity.Cloud.UserReporting.Plugin
{
	// Token: 0x02000026 RID: 38
	public static class UnityUserReportParser
	{
		// Token: 0x0600010F RID: 271 RVA: 0x000058B2 File Offset: 0x00003AB2
		public static UserReport ParseUserReport(string json)
		{
			return SimpleJson.DeserializeObject<UserReport>(json);
		}

		// Token: 0x06000110 RID: 272 RVA: 0x000058BA File Offset: 0x00003ABA
		public static UserReportList ParseUserReportList(string json)
		{
			return SimpleJson.DeserializeObject<UserReportList>(json);
		}
	}
}
