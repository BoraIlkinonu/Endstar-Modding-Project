using System;
using MatchmakingClientSDK.Groups;
using MatchmakingClientSDK.Users;

namespace MatchmakingClientSDK
{
	// Token: 0x02000052 RID: 82
	public interface IMatchmakingClient
	{
		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060002F7 RID: 759
		MatchmakingWebsocketClient WebsocketClient { get; }

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060002F8 RID: 760
		UsersService UsersService { get; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060002F9 RID: 761
		GroupsService GroupsService { get; }

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x060002FA RID: 762
		MatchesService MatchesService { get; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060002FB RID: 763
		EventsService EventsService { get; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060002FC RID: 764
		NotificationsService NotificationsService { get; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060002FD RID: 765
		ChatService ChatService { get; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060002FE RID: 766
		Action<string, bool> Log { get; }

		// Token: 0x060002FF RID: 767
		void Connect(string authToken, string platform);

		// Token: 0x06000300 RID: 768
		void Update();

		// Token: 0x06000301 RID: 769
		void Dispose();
	}
}
