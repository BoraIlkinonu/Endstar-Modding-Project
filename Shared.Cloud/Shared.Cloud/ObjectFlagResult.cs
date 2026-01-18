using System;
using Newtonsoft.Json;

namespace Endless.Matchmaking
{
	// Token: 0x02000036 RID: 54
	[Serializable]
	public struct ObjectFlagResult
	{
		// Token: 0x0400009E RID: 158
		[JsonProperty("identifier")]
		public string Identifier;

		// Token: 0x0400009F RID: 159
		[JsonProperty("moderations")]
		public Moderation[] Moderations;
	}
}
