using System;
using Newtonsoft.Json;

namespace Endless.Shared.DataTypes
{
	// Token: 0x02000018 RID: 24
	[Serializable]
	public class User : IEquatable<User>
	{
		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x00004004 File Offset: 0x00002204
		// (set) Token: 0x060000B4 RID: 180 RVA: 0x0000400C File Offset: 0x0000220C
		[JsonProperty("id")]
		public int Id { get; private set; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000B5 RID: 181 RVA: 0x00004015 File Offset: 0x00002215
		// (set) Token: 0x060000B6 RID: 182 RVA: 0x0000401D File Offset: 0x0000221D
		[JsonProperty("public_id")]
		public string PublicId { get; private set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000B7 RID: 183 RVA: 0x00004026 File Offset: 0x00002226
		// (set) Token: 0x060000B8 RID: 184 RVA: 0x0000402E File Offset: 0x0000222E
		[JsonProperty("username")]
		public string UserName { get; private set; }

		// Token: 0x060000B9 RID: 185 RVA: 0x00004037 File Offset: 0x00002237
		public User(int id, string publicId, string userName)
		{
			this.Id = id;
			this.PublicId = publicId ?? "null";
			this.UserName = userName ?? "null";
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00004068 File Offset: 0x00002268
		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}", new object[] { "Id", this.Id, "PublicId", this.PublicId, "UserName", this.UserName });
		}

		// Token: 0x060000BB RID: 187 RVA: 0x000040BD File Offset: 0x000022BD
		public bool Equals(User other)
		{
			return other != null && (this == other || this.Id == other.Id);
		}

		// Token: 0x060000BC RID: 188 RVA: 0x000040D8 File Offset: 0x000022D8
		public override bool Equals(object obj)
		{
			return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((User)obj)));
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00004106 File Offset: 0x00002306
		public override int GetHashCode()
		{
			return HashCode.Combine<int>(this.Id);
		}
	}
}
