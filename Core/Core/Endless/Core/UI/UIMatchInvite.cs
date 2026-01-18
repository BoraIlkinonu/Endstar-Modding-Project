using System;
using Runtime.Shared.Matchmaking;

namespace Endless.Core.UI
{
	// Token: 0x0200004D RID: 77
	public struct UIMatchInvite
	{
		// Token: 0x0600017E RID: 382 RVA: 0x00009C38 File Offset: 0x00007E38
		public UIMatchInvite(MatchData matchData, float expiration)
		{
			this.MatchData = matchData;
			this.Expiration = DateTime.Now.AddSeconds((double)expiration);
		}

		// Token: 0x0400010D RID: 269
		public DateTime Expiration;

		// Token: 0x0400010E RID: 270
		public MatchData MatchData;
	}
}
