using System;
using System.Collections.Generic;
using Endless.Networking;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000016 RID: 22
	public class GroupInfo
	{
		// Token: 0x0400003B RID: 59
		public string Id;

		// Token: 0x0400003C RID: 60
		public List<CoreClientData> Members;

		// Token: 0x0400003D RID: 61
		public CoreClientData Host;
	}
}
