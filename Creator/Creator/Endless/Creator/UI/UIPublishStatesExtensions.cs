using System;

namespace Endless.Creator.UI
{
	// Token: 0x020002D3 RID: 723
	public static class UIPublishStatesExtensions
	{
		// Token: 0x06000C3F RID: 3135 RVA: 0x0003A88E File Offset: 0x00038A8E
		public static string ToEndlessCloudServicesCompatibleString(this UIPublishStates publishState)
		{
			return publishState.ToString().ToLower();
		}
	}
}
