using System;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;

namespace Runtime.Shared
{
	// Token: 0x02000008 RID: 8
	public class ConnectionActions : MonoBehaviourSingleton<ConnectionActions>
	{
		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600004F RID: 79 RVA: 0x000044D0 File Offset: 0x000026D0
		private static bool ShouldLeaveGroup
		{
			get
			{
				if (MatchmakingClientController.Instance.LocalGroup == null)
				{
					return false;
				}
				if (MatchmakingClientController.Instance.LocalClientData == null)
				{
					return false;
				}
				ClientData value = MatchmakingClientController.Instance.LocalClientData.Value;
				CoreClientData host = MatchmakingClientController.Instance.LocalGroup.Host;
				return !value.CoreDataEquals(host);
			}
		}

		// Token: 0x06000050 RID: 80 RVA: 0x0000452C File Offset: 0x0000272C
		public void EndMatch(Action<int, string> onError = null)
		{
			if (ConnectionActions.ShouldLeaveGroup)
			{
				MatchmakingClientController.Instance.LeaveGroup(false, null);
				return;
			}
			MatchmakingClientController.Instance.EndMatch(onError);
		}
	}
}
