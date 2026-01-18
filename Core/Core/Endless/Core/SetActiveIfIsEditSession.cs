using System;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Runtime.Shared.Matchmaking;

namespace Endless.Core
{
	// Token: 0x0200001F RID: 31
	public class SetActiveIfIsEditSession : BaseSetActiveIf
	{
		// Token: 0x0600006D RID: 109 RVA: 0x000041B4 File Offset: 0x000023B4
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MatchmakingClientController.MatchStart += this.OnMatchStarted;
			MatchmakingClientController.MatchLeft += this.OnMatchClosed;
			base.SetActive(false);
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00004204 File Offset: 0x00002404
		private void OnMatchStarted()
		{
			MatchData matchData = MatchmakingClientController.Instance.LocalMatch.GetMatchData();
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMatchStarted", new object[] { matchData });
			}
			base.SetActive(matchData.IsEditSession);
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00004250 File Offset: 0x00002450
		private void OnMatchClosed()
		{
			string text = "Match ended.";
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMatchClosed", new object[] { text });
			}
			base.SetActive(false);
		}
	}
}
