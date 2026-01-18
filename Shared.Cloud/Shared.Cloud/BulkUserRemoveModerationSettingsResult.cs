using System;
using Newtonsoft.Json;

namespace Endless.Matchmaking
{
	// Token: 0x02000037 RID: 55
	[Serializable]
	public struct BulkUserRemoveModerationSettingsResult
	{
		// Token: 0x040000A0 RID: 160
		[JsonProperty("user_id")]
		public int UserId;

		// Token: 0x040000A1 RID: 161
		[JsonProperty("blocked_flags")]
		public ModerationFlag[] BlockedFlags;
	}
}
