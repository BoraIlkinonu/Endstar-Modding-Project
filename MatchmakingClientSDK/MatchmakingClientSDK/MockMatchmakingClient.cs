using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DocumentModel;
using MatchmakingClientSDK.Groups;
using MatchmakingClientSDK.Users;

namespace MatchmakingClientSDK
{
	// Token: 0x02000058 RID: 88
	public class MockMatchmakingClient : IMatchmakingClient
	{
		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000359 RID: 857 RVA: 0x0000F19B File Offset: 0x0000D39B
		// (set) Token: 0x0600035A RID: 858 RVA: 0x0000F1A3 File Offset: 0x0000D3A3
		public MatchmakingWebsocketClient WebsocketClient { get; set; }

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x0600035B RID: 859 RVA: 0x0000F1AC File Offset: 0x0000D3AC
		// (set) Token: 0x0600035C RID: 860 RVA: 0x0000F1B4 File Offset: 0x0000D3B4
		public UsersService UsersService { get; set; }

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x0600035D RID: 861 RVA: 0x0000F1BD File Offset: 0x0000D3BD
		// (set) Token: 0x0600035E RID: 862 RVA: 0x0000F1C5 File Offset: 0x0000D3C5
		public GroupsService GroupsService { get; set; }

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x0600035F RID: 863 RVA: 0x0000F1CE File Offset: 0x0000D3CE
		// (set) Token: 0x06000360 RID: 864 RVA: 0x0000F1D6 File Offset: 0x0000D3D6
		public MatchesService MatchesService { get; set; }

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000361 RID: 865 RVA: 0x0000F1DF File Offset: 0x0000D3DF
		// (set) Token: 0x06000362 RID: 866 RVA: 0x0000F1E7 File Offset: 0x0000D3E7
		public EventsService EventsService { get; set; }

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000363 RID: 867 RVA: 0x0000F1F0 File Offset: 0x0000D3F0
		// (set) Token: 0x06000364 RID: 868 RVA: 0x0000F1F8 File Offset: 0x0000D3F8
		public NotificationsService NotificationsService { get; set; }

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000365 RID: 869 RVA: 0x0000F201 File Offset: 0x0000D401
		// (set) Token: 0x06000366 RID: 870 RVA: 0x0000F209 File Offset: 0x0000D409
		public ChatService ChatService { get; set; }

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000367 RID: 871 RVA: 0x0000F212 File Offset: 0x0000D412
		public Action<string, bool> Log { get; } = delegate(string s, bool b)
		{
			if (!b)
			{
				Console.WriteLine(s);
				return;
			}
			Console.Error.WriteLine(s);
		};

		// Token: 0x06000368 RID: 872 RVA: 0x0000F21C File Offset: 0x0000D41C
		public MockMatchmakingClient()
		{
			this.WebsocketClient = new MatchmakingWebsocketClient("DEV", this.Log, () => 0f, new Dictionary<string, string>
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

		// Token: 0x06000369 RID: 873 RVA: 0x0000F429 File Offset: 0x0000D629
		public void Connect(string authToken, string platform)
		{
		}

		// Token: 0x0600036A RID: 874 RVA: 0x0000F42B File Offset: 0x0000D62B
		public void OnMessageReceived(Document document)
		{
			this.WebsocketClient.OnMessageReceived(document.ToJson());
		}

		// Token: 0x0600036B RID: 875 RVA: 0x0000F43E File Offset: 0x0000D63E
		public void Update()
		{
			this.WebsocketClient.Update();
		}

		// Token: 0x0600036C RID: 876 RVA: 0x0000F44C File Offset: 0x0000D64C
		public void Dispose()
		{
			this.WebsocketClient.Disconnect();
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

		// Token: 0x040001F8 RID: 504
		private Dictionary<string, Action<Document>> serviceHandlers = new Dictionary<string, Action<Document>>();
	}
}
