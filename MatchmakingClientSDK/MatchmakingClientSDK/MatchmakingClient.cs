using System;
using System.Collections.Generic;
using MatchmakingClientSDK.Groups;
using MatchmakingClientSDK.Users;

namespace MatchmakingClientSDK
{
	// Token: 0x02000056 RID: 86
	public class MatchmakingClient : IMatchmakingClient
	{
		// Token: 0x17000058 RID: 88
		// (get) Token: 0x06000336 RID: 822 RVA: 0x0000E85E File Offset: 0x0000CA5E
		public MatchmakingWebsocketClient WebsocketClient { get; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000337 RID: 823 RVA: 0x0000E866 File Offset: 0x0000CA66
		public UsersService UsersService { get; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000338 RID: 824 RVA: 0x0000E86E File Offset: 0x0000CA6E
		public GroupsService GroupsService { get; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000339 RID: 825 RVA: 0x0000E876 File Offset: 0x0000CA76
		public MatchesService MatchesService { get; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x0600033A RID: 826 RVA: 0x0000E87E File Offset: 0x0000CA7E
		public EventsService EventsService { get; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x0600033B RID: 827 RVA: 0x0000E886 File Offset: 0x0000CA86
		public NotificationsService NotificationsService { get; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x0600033C RID: 828 RVA: 0x0000E88E File Offset: 0x0000CA8E
		public ChatService ChatService { get; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x0600033D RID: 829 RVA: 0x0000E896 File Offset: 0x0000CA96
		public Action<string, bool> Log
		{
			get
			{
				return this.WebsocketClient.Log;
			}
		}

		// Token: 0x0600033E RID: 830 RVA: 0x0000E8A4 File Offset: 0x0000CAA4
		public MatchmakingClient(string stage, Action<string, bool> log, Func<float> time)
		{
			this.WebsocketClient = new MatchmakingWebsocketClient(stage, log, time, new Dictionary<string, string>
			{
				{ "DEV", "wss://endstar-matchmaking-dev.endlessstudios.com" },
				{ "STAGING", "wss://endstar-matchmaking-stage.endlessstudios.com" },
				{ "PROD", "wss://endstar-matchmaking.endlessstudios.com" }
			});
			this.EventsService = new EventsService(this);
			this.NotificationsService = new NotificationsService(this);
			this.ChatService = new ChatService(this);
			this.UsersService = new UsersService(this);
			this.WebsocketClient.OnAuthentication += this.UsersService.OnAuthentication;
			this.EventsService.OnUserConnected += this.UsersService.OnConnectedEventReceived;
			this.EventsService.OnUserUpdated += this.UsersService.OnUpdateEventReceived;
			this.EventsService.OnUserDisconnected += this.UsersService.OnDisconnectedEventReceived;
			this.GroupsService = new GroupsService(this);
			this.EventsService.OnGroupCreated += this.GroupsService.OnCreateEventReceived;
			this.EventsService.OnGroupUpdated += this.GroupsService.OnUpdateEventReceived;
			this.EventsService.OnGroupDeleted += this.GroupsService.OnDeleteEventReceived;
			this.MatchesService = new MatchesService(this);
			this.EventsService.OnMatchCreated += this.MatchesService.OnCreateEventReceived;
			this.EventsService.OnMatchUpdated += this.MatchesService.OnUpdateEventReceived;
			this.EventsService.OnMatchDeleted += this.MatchesService.OnDeleteEventReceived;
		}

		// Token: 0x0600033F RID: 831 RVA: 0x0000EA5A File Offset: 0x0000CC5A
		public void Connect(string authToken, string platform)
		{
			MatchmakingWebsocketClient websocketClient = this.WebsocketClient;
			if (websocketClient == null)
			{
				return;
			}
			websocketClient.Connect(authToken, platform);
		}

		// Token: 0x06000340 RID: 832 RVA: 0x0000EA6E File Offset: 0x0000CC6E
		public void Update()
		{
			MatchmakingWebsocketClient websocketClient = this.WebsocketClient;
			if (websocketClient == null)
			{
				return;
			}
			websocketClient.Update();
		}

		// Token: 0x06000341 RID: 833 RVA: 0x0000EA80 File Offset: 0x0000CC80
		public void Dispose()
		{
			MatchmakingWebsocketClient websocketClient = this.WebsocketClient;
			if (websocketClient != null)
			{
				websocketClient.Disconnect();
			}
			UsersService usersService = this.UsersService;
			if (usersService != null)
			{
				usersService.Clear();
			}
			GroupsService groupsService = this.GroupsService;
			if (groupsService != null)
			{
				groupsService.Clear();
			}
			MatchesService matchesService = this.MatchesService;
			if (matchesService != null)
			{
				matchesService.Clear();
			}
			EventsService eventsService = this.EventsService;
			if (eventsService != null)
			{
				eventsService.Clear();
			}
			NotificationsService notificationsService = this.NotificationsService;
			if (notificationsService != null)
			{
				notificationsService.Clear();
			}
			ChatService chatService = this.ChatService;
			if (chatService == null)
			{
				return;
			}
			chatService.Clear();
		}
	}
}
