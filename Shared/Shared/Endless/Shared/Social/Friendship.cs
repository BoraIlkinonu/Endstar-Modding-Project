using System;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Shared.Social
{
	// Token: 0x020000E3 RID: 227
	[Serializable]
	public class Friendship
	{
		// Token: 0x170000DD RID: 221
		// (get) Token: 0x06000592 RID: 1426 RVA: 0x0001811D File Offset: 0x0001631D
		// (set) Token: 0x06000593 RID: 1427 RVA: 0x00018125 File Offset: 0x00016325
		[JsonProperty("user1")]
		public User UserOne { get; private set; }

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x06000594 RID: 1428 RVA: 0x0001812E File Offset: 0x0001632E
		// (set) Token: 0x06000595 RID: 1429 RVA: 0x00018136 File Offset: 0x00016336
		[JsonProperty("user2")]
		public User UserTwo { get; private set; }

		// Token: 0x06000596 RID: 1430 RVA: 0x0001813F File Offset: 0x0001633F
		public Friendship(User userOne, User userTwo)
		{
			this.UserOne = userOne;
			this.UserTwo = userTwo;
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x00018155 File Offset: 0x00016355
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3} }}", new object[] { "UserOne", this.UserOne, "UserTwo", this.UserTwo });
		}
	}
}
