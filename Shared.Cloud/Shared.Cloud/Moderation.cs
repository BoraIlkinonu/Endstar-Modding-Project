using System;
using Newtonsoft.Json;

namespace Endless.Matchmaking
{
	// Token: 0x02000035 RID: 53
	[Serializable]
	public struct Moderation
	{
		// Token: 0x0400009A RID: 154
		[JsonProperty("id")]
		public int Id;

		// Token: 0x0400009B RID: 155
		[JsonProperty("reason")]
		public string Reason;

		// Token: 0x0400009C RID: 156
		[JsonProperty("moderator")]
		public object Moderator;

		// Token: 0x0400009D RID: 157
		[JsonProperty("flag")]
		public ModerationFlag flag;
	}
}
