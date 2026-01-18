using System;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Shared.Social
{
	// Token: 0x020000E1 RID: 225
	public class BlockedUser
	{
		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x06000581 RID: 1409 RVA: 0x0001800D File Offset: 0x0001620D
		// (set) Token: 0x06000582 RID: 1410 RVA: 0x00018015 File Offset: 0x00016215
		[JsonProperty("user1")]
		public User UserOne { get; private set; }

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x06000583 RID: 1411 RVA: 0x0001801E File Offset: 0x0001621E
		// (set) Token: 0x06000584 RID: 1412 RVA: 0x00018026 File Offset: 0x00016226
		[JsonProperty("user2")]
		public User UserTwo { get; private set; }

		// Token: 0x06000585 RID: 1413 RVA: 0x0001802F File Offset: 0x0001622F
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3} }}", new object[] { "UserOne", this.UserOne, "UserTwo", this.UserTwo });
		}
	}
}
