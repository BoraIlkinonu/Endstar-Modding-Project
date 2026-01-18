using System;
using Newtonsoft.Json;

namespace Endless.Matchmaking
{
	// Token: 0x02000034 RID: 52
	[Serializable]
	public class ModerationFlag : IEquatable<ModerationFlag>
	{
		// Token: 0x0600018F RID: 399 RVA: 0x00007AF8 File Offset: 0x00005CF8
		public bool Equals(ModerationFlag other)
		{
			return this.Id == other.Id && this.Code == other.Code && this.Action == other.Action && this.Description == other.Description && this.IsActive == other.IsActive;
		}

		// Token: 0x06000190 RID: 400 RVA: 0x00007B5C File Offset: 0x00005D5C
		public override bool Equals(object obj)
		{
			ModerationFlag moderationFlag = obj as ModerationFlag;
			return moderationFlag != null && this.Equals(moderationFlag);
		}

		// Token: 0x06000191 RID: 401 RVA: 0x00007B7C File Offset: 0x00005D7C
		public override int GetHashCode()
		{
			return HashCode.Combine<int, string, string, string, bool>(this.Id, this.Code, this.Action, this.Description, this.IsActive);
		}

		// Token: 0x04000093 RID: 147
		public static string Query = "flag { id code action description is_active }";

		// Token: 0x04000094 RID: 148
		[JsonProperty("id")]
		public int Id;

		// Token: 0x04000095 RID: 149
		[JsonProperty("code")]
		public string Code;

		// Token: 0x04000096 RID: 150
		[JsonProperty("action")]
		public string Action;

		// Token: 0x04000097 RID: 151
		[JsonProperty("description")]
		public string Description;

		// Token: 0x04000098 RID: 152
		[JsonProperty("is_active")]
		public bool IsActive;

		// Token: 0x04000099 RID: 153
		[JsonIgnore]
		public string NiceName;
	}
}
