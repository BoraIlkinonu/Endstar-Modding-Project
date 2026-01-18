using System;
using Newtonsoft.Json;

namespace Endless.Matchmaking
{
	// Token: 0x02000038 RID: 56
	[Serializable]
	public struct UserModerationSettings
	{
		// Token: 0x040000A2 RID: 162
		[JsonProperty("user_id")]
		public int UserId;

		// Token: 0x040000A3 RID: 163
		[JsonProperty("flag_id")]
		public int FlagId;

		// Token: 0x040000A4 RID: 164
		[JsonProperty("flag")]
		public ModerationFlag Flag;
	}
}
