using System;
using Endless.Networking;
using MatchmakingAPI.Matches;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000018 RID: 24
	public struct MatchData
	{
		// Token: 0x04000043 RID: 67
		public string MatchId;

		// Token: 0x04000044 RID: 68
		public string ProjectId;

		// Token: 0x04000045 RID: 69
		public string LevelId;

		// Token: 0x04000046 RID: 70
		public string Version;

		// Token: 0x04000047 RID: 71
		public string CustomData;

		// Token: 0x04000048 RID: 72
		public bool IsEditSession;

		// Token: 0x04000049 RID: 73
		public MatchServerTypes MatchServerType;

		// Token: 0x0400004A RID: 74
		public ClientData MatchHost;

		// Token: 0x0400004B RID: 75
		public string MatchAuthKey;

		// Token: 0x0400004C RID: 76
		public ServerInstance ServerInstance;
	}
}
