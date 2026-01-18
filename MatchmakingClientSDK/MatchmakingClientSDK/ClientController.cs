using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using MatchmakingAPI.Matches;
using MatchmakingAPI.Notifications;
using MatchmakingAPI.Users;

namespace MatchmakingClientSDK
{
	// Token: 0x02000050 RID: 80
	public abstract class ClientController
	{
		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000299 RID: 665
		public abstract NetworkEnvironment NetworkEnv { get; }

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x0600029A RID: 666
		public abstract Func<string, string, Task<string>> SignIn { get; }

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x0600029B RID: 667
		public abstract Action<string, bool> Log { get; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x0600029C RID: 668
		public abstract Func<float> Time { get; }

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x0600029D RID: 669
		public abstract Action AllocateMatch { get; }

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x0600029E RID: 670 RVA: 0x0000C202 File Offset: 0x0000A402
		// (set) Token: 0x0600029F RID: 671 RVA: 0x0000C20A File Offset: 0x0000A40A
		private protected IMatchmakingClient client { protected get; private set; }

		// Token: 0x060002A0 RID: 672 RVA: 0x0000C213 File Offset: 0x0000A413
		public virtual void Update()
		{
			IMatchmakingClient client = this.client;
			if (client == null)
			{
				return;
			}
			client.Update();
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x0000C225 File Offset: 0x0000A425
		protected void OnGui()
		{
			if (!this.Connected)
			{
				this.LoginGui();
				return;
			}
			this.ConnectedGui();
		}

		// Token: 0x060002A2 RID: 674
		protected abstract void LoginGui();

		// Token: 0x060002A3 RID: 675
		protected abstract void ConnectedGui();

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060002A4 RID: 676 RVA: 0x0000C23C File Offset: 0x0000A43C
		// (set) Token: 0x060002A5 RID: 677 RVA: 0x0000C244 File Offset: 0x0000A444
		public bool Connecting { get; private set; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060002A6 RID: 678 RVA: 0x0000C24D File Offset: 0x0000A44D
		// (set) Token: 0x060002A7 RID: 679 RVA: 0x0000C255 File Offset: 0x0000A455
		public bool Connected { get; private set; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060002A8 RID: 680 RVA: 0x0000C25E File Offset: 0x0000A45E
		// (set) Token: 0x060002A9 RID: 681 RVA: 0x0000C266 File Offset: 0x0000A466
		public bool Authenticated { get; private set; }

		// Token: 0x060002AA RID: 682 RVA: 0x0000C270 File Offset: 0x0000A470
		public async void Connect()
		{
			IMatchmakingClient client = this.client;
			if (((client != null) ? client.WebsocketClient : null) != null)
			{
				this.Disconnect();
			}
			this.Connecting = true;
			string text;
			try
			{
				text = await this.SignIn(this.UserName, this.Password);
			}
			catch (Exception)
			{
				this.Connecting = false;
				return;
			}
			this.client = new MatchmakingClient(this.NetworkEnv.ToString(), this.Log, this.Time);
			this.client.WebsocketClient.OnConnectedToServer += this.OnConnected;
			this.client.WebsocketClient.OnConnectionToServerFailed += this.OnConnectionFailed;
			this.client.WebsocketClient.OnDisconnectedFromServer += this.OnDisconnected;
			this.client.UsersService.OnAuthenticationSuccess += this.OnAuthenticated;
			this.client.UsersService.OnAuthenticationFailed += this.OnAuthenticationFailed;
			this.client.NotificationsService.OnNotificationReceived += this.OnNotificationReceived;
			this.client.UsersService.OnLocalUserUpdated += this.LocalUserUpdated;
			this.client.GroupsService.OnGroupInvite += this.OnGroupInvite;
			this.client.GroupsService.OnGroupJoined += this.OnGroupJoined;
			this.client.GroupsService.OnGroupJoin += this.OnGroupJoin;
			this.client.GroupsService.OnGroupLeave += this.OnGroupLeave;
			this.client.GroupsService.OnGroupHostChanged += this.OnGroupHostChanged;
			this.client.GroupsService.OnGroupUpdated += this.OnGroupUpdated;
			this.client.GroupsService.OnGroupLeft += this.OnGroupLeft;
			this.client.MatchesService.OnMatchJoined += this.OnMatchStarted;
			this.client.MatchesService.OnMatchJoin += this.OnMatchJoin;
			this.client.MatchesService.OnMatchLeave += this.OnMatchLeave;
			this.client.MatchesService.OnMatchHostChanged += this.OnMatchHostChanged;
			this.client.MatchesService.OnMatchUpdated += this.OnMatchUpdated;
			this.client.MatchesService.OnHostMigration += this.OnHostMigration;
			this.client.MatchesService.OnMatchAllocated += this.OnMatchAllocated;
			this.client.MatchesService.OnMatchAllocationError += this.OnMatchAllocationError;
			this.client.MatchesService.OnMatchLeft += this.OnMatchLeft;
			this.client.ChatService.OnUserChatReceived += this.OnUserChatReceived;
			this.client.ChatService.OnGroupChatReceived += this.OnGroupChatReceived;
			this.client.ChatService.OnMatchChatReceived += this.OnMatchChatReceived;
			this.client.Connect(text, this.UserPlatform.ToString());
		}

		// Token: 0x060002AB RID: 683 RVA: 0x0000C2A7 File Offset: 0x0000A4A7
		private void OnConnected()
		{
			this.Connecting = false;
			this.Connected = true;
			this.Authenticated = false;
		}

		// Token: 0x060002AC RID: 684 RVA: 0x0000C2BE File Offset: 0x0000A4BE
		private void OnConnectionFailed()
		{
			this.Connecting = false;
			this.Connected = false;
			this.Authenticated = false;
		}

		// Token: 0x060002AD RID: 685 RVA: 0x0000C2D5 File Offset: 0x0000A4D5
		protected void Disconnect()
		{
			IMatchmakingClient client = this.client;
			if (client != null)
			{
				client.Dispose();
			}
			this.client = null;
		}

		// Token: 0x060002AE RID: 686 RVA: 0x0000C2EF File Offset: 0x0000A4EF
		private void OnDisconnected()
		{
			this.Connecting = false;
			this.Connected = false;
			this.Authenticated = false;
		}

		// Token: 0x060002AF RID: 687 RVA: 0x0000C306 File Offset: 0x0000A506
		private void OnAuthenticated()
		{
			this.Authenticated = true;
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x0000C30F File Offset: 0x0000A50F
		private void OnAuthenticationFailed()
		{
			this.Authenticated = false;
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x0000C318 File Offset: 0x0000A518
		public bool TryGetNotification(out Tuple<NotificationTypes, Document, DateTime> notification, bool peek = false)
		{
			if (this.notifications.Count == 0)
			{
				notification = null;
				return false;
			}
			notification = (peek ? this.notifications.Peek() : this.notifications.Dequeue());
			return true;
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x0000C34A File Offset: 0x0000A54A
		private void OnNotificationReceived(NotificationTypes type, Document data)
		{
			this.notifications.Enqueue(new Tuple<NotificationTypes, Document, DateTime>(type, data, DateTime.UtcNow));
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x0000C363 File Offset: 0x0000A563
		private void LocalUserUpdated(ReadOnlyDictionary<string, AttributeValue> attributes)
		{
			Dictionary<string, AttributeValue> attributes2 = this.client.UsersService.UserData.Attributes;
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x0000C37B File Offset: 0x0000A57B
		public bool TryGetGroupInvite(out Tuple<string, string, DateTime> invite, bool peek = false)
		{
			if (this.groupInvites.Count == 0)
			{
				invite = null;
				return false;
			}
			invite = (peek ? this.groupInvites.Peek() : this.groupInvites.Dequeue());
			return true;
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x0000C3AD File Offset: 0x0000A5AD
		private void OnGroupInvite(string inviteId, string hostId)
		{
			this.groupInvites.Enqueue(new Tuple<string, string, DateTime>(inviteId, hostId, DateTime.UtcNow));
		}

		// Token: 0x060002B6 RID: 694 RVA: 0x0000C3C8 File Offset: 0x0000A5C8
		private void OnGroupJoined()
		{
			ReadOnlyDictionary<string, AttributeValue> localAttributes = this.client.GroupsService.GetLocalAttributes();
			string s = localAttributes["groupId"].S;
			string s2 = localAttributes["groupHost"].S;
			localAttributes["groupMembers"].L.Select((AttributeValue m) => m.S).ToArray<string>();
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x0000C440 File Offset: 0x0000A640
		private void OnGroupJoin(string userId)
		{
			this.client.Log("User " + userId + " joined the group", false);
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x0000C463 File Offset: 0x0000A663
		private void OnGroupLeave(string userId)
		{
			this.client.Log("User " + userId + " left the group", false);
		}

		// Token: 0x060002B9 RID: 697 RVA: 0x0000C486 File Offset: 0x0000A686
		private void OnGroupHostChanged(string newHostId)
		{
			this.client.Log("User " + newHostId + " has become group's new host", false);
		}

		// Token: 0x060002BA RID: 698 RVA: 0x0000C4A9 File Offset: 0x0000A6A9
		private void OnGroupUpdated(ReadOnlyDictionary<string, AttributeValue> oldAttributes)
		{
		}

		// Token: 0x060002BB RID: 699 RVA: 0x0000C4AB File Offset: 0x0000A6AB
		private void OnGroupLeft()
		{
		}

		// Token: 0x060002BC RID: 700 RVA: 0x0000C4AD File Offset: 0x0000A6AD
		public void InviteToGroup(string userId)
		{
			IMatchmakingClient client = this.client;
			if (client == null)
			{
				return;
			}
			client.GroupsService.InviteToGroup(userId, delegate(int code, string message)
			{
				Action<string, bool> log = this.Log;
				string text = "InviteToGroup error: ";
				HttpStatusCode httpStatusCode = (HttpStatusCode)code;
				log(text + httpStatusCode.ToString() + " - " + message, true);
			});
		}

		// Token: 0x060002BD RID: 701 RVA: 0x0000C4D1 File Offset: 0x0000A6D1
		public void JoinGroup(string inviteId)
		{
			IMatchmakingClient client = this.client;
			if (client == null)
			{
				return;
			}
			client.GroupsService.JoinGroup(inviteId, delegate(int code, string message)
			{
				Action<string, bool> log = this.Log;
				string text = "JoinGroup error: ";
				HttpStatusCode httpStatusCode = (HttpStatusCode)code;
				log(text + httpStatusCode.ToString() + " - " + message, true);
			});
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0000C4F5 File Offset: 0x0000A6F5
		public void ChangeGroupHost(string newHostId)
		{
			IMatchmakingClient client = this.client;
			if (client == null)
			{
				return;
			}
			client.GroupsService.ChangeHost(newHostId, delegate(int code, string message)
			{
				Action<string, bool> log = this.Log;
				string text = "ChangeHost error: ";
				HttpStatusCode httpStatusCode = (HttpStatusCode)code;
				log(text + httpStatusCode.ToString() + " - " + message, true);
			});
		}

		// Token: 0x060002BF RID: 703 RVA: 0x0000C519 File Offset: 0x0000A719
		public void RemoveFromGroup(string userId)
		{
			IMatchmakingClient client = this.client;
			if (client == null)
			{
				return;
			}
			client.GroupsService.RemoveFromGroup(userId, delegate(int code, string message)
			{
				Action<string, bool> log = this.Log;
				string text = "RemoveFromGroup error: ";
				HttpStatusCode httpStatusCode = (HttpStatusCode)code;
				log(text + httpStatusCode.ToString() + " - " + message, true);
			});
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000C53D File Offset: 0x0000A73D
		public void LeaveGroup(bool stayInMatch = true)
		{
			IMatchmakingClient client = this.client;
			if (client == null)
			{
				return;
			}
			client.GroupsService.LeaveGroup(stayInMatch, delegate(int code, string message)
			{
				Action<string, bool> log = this.Log;
				string text = "LeaveGroup error: ";
				HttpStatusCode httpStatusCode = (HttpStatusCode)code;
				log(text + httpStatusCode.ToString() + " - " + message, true);
			});
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000C564 File Offset: 0x0000A764
		private void OnMatchStarted()
		{
			string stringProperty = this.client.MatchesService.GetStringProperty("allocationData");
			string stringProperty2 = this.client.MatchesService.GetStringProperty("serverType");
			if (string.IsNullOrWhiteSpace(stringProperty) && stringProperty2 == "USER" && this.TryAllocateMatch())
			{
				return;
			}
			this.MatchState = ClientController.MatchStates.InProgress;
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x0000C5C0 File Offset: 0x0000A7C0
		private void OnMatchJoin(string groupId)
		{
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x0000C5C2 File Offset: 0x0000A7C2
		private void OnMatchLeave(string groupId)
		{
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x0000C5C4 File Offset: 0x0000A7C4
		private void OnMatchHostChanged(string newHostId)
		{
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x0000C5C6 File Offset: 0x0000A7C6
		private void OnMatchUpdated(ReadOnlyDictionary<string, AttributeValue> oldAttributes)
		{
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x0000C5C8 File Offset: 0x0000A7C8
		private void OnHostMigration()
		{
			if (this.MatchState != ClientController.MatchStates.InProgress)
			{
				return;
			}
			MatchServerTypes matchServerTypes;
			if (Enum.TryParse<MatchServerTypes>(this.client.MatchesService.GetStringProperty("serverType"), out matchServerTypes) && matchServerTypes == MatchServerTypes.USER)
			{
				this.hostMigrationGuid = Guid.NewGuid();
				this.HostMigrationSequence();
				return;
			}
			this.Log("HostMigration() on Dedicated server", true);
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x0000C624 File Offset: 0x0000A824
		private async void HostMigrationSequence()
		{
			Guid guid = this.hostMigrationGuid;
			while (!this.TryAllocateMatch())
			{
				await Task.Yield();
				if (guid != this.hostMigrationGuid)
				{
					return;
				}
			}
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x0000C65C File Offset: 0x0000A85C
		private bool TryAllocateMatch()
		{
			bool flag = true;
			string userId = this.client.UsersService.GetUserId();
			string stringProperty = this.client.GroupsService.GetStringProperty("groupId");
			string stringProperty2 = this.client.GroupsService.GetStringProperty("groupHost");
			if (userId != stringProperty2)
			{
				flag = false;
			}
			string stringProperty3 = this.client.MatchesService.GetStringProperty("matchHost");
			if (stringProperty != stringProperty3)
			{
				flag = false;
			}
			if (flag)
			{
				this.MatchState = ClientController.MatchStates.Allocating;
				Action allocateMatch = this.AllocateMatch;
				if (allocateMatch != null)
				{
					allocateMatch();
				}
				return true;
			}
			return false;
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x0000C6F0 File Offset: 0x0000A8F0
		protected virtual void OnMatchAllocated(string publicIp, string localIp, string name, int port, string key, string serverType)
		{
			if (this.MatchState != ClientController.MatchStates.Allocating)
			{
				return;
			}
			this.MatchState = ClientController.MatchStates.InProgress;
			this.hostMigrationGuid = Guid.Empty;
		}

		// Token: 0x060002CA RID: 714 RVA: 0x0000C710 File Offset: 0x0000A910
		protected virtual void OnMatchAllocationError(int code, string message)
		{
			Action<string, bool> log = this.Log;
			string text = "MatchAllocationError: ";
			HttpStatusCode httpStatusCode = (HttpStatusCode)code;
			log(text + httpStatusCode.ToString() + " - " + message, true);
			if (this.MatchState != ClientController.MatchStates.Allocating)
			{
				return;
			}
			this.MatchState = ClientController.MatchStates.InProgress;
			this.hostMigrationGuid = Guid.Empty;
		}

		// Token: 0x060002CB RID: 715 RVA: 0x0000C764 File Offset: 0x0000A964
		private void OnMatchLeft()
		{
			this.MatchState = ClientController.MatchStates.None;
			this.hostMigrationGuid = Guid.Empty;
		}

		// Token: 0x060002CC RID: 716 RVA: 0x0000C778 File Offset: 0x0000A978
		public void StartMatch()
		{
			if (this.MatchState != ClientController.MatchStates.None)
			{
				this.Log("Cannot start match. Operation already in progress", true);
				return;
			}
			this.MatchState = ClientController.MatchStates.Starting;
			IMatchmakingClient client = this.client;
			if (client == null)
			{
				return;
			}
			client.MatchesService.StartMatch(this.GameId, this.LevelId, this.IsEditSession, this.Version, this.ServerType, null, delegate(int code, string message)
			{
				this.MatchState = ClientController.MatchStates.None;
				Action<string, bool> log = this.Log;
				string text = "StartMatch error: ";
				HttpStatusCode httpStatusCode = (HttpStatusCode)code;
				log(text + httpStatusCode.ToString() + " - " + message, true);
			});
		}

		// Token: 0x060002CD RID: 717 RVA: 0x0000C7E6 File Offset: 0x0000A9E6
		public void ChangeMatchHost(string newHostId)
		{
			IMatchmakingClient client = this.client;
			if (client == null)
			{
				return;
			}
			client.MatchesService.ChangeHost(newHostId, delegate(int code, string message)
			{
				Action<string, bool> log = this.Log;
				string text = "ChangeMatchHost error: ";
				HttpStatusCode httpStatusCode = (HttpStatusCode)code;
				log(text + httpStatusCode.ToString() + " - " + message, true);
			});
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0000C80C File Offset: 0x0000AA0C
		public void ChangeMatch(params string[] userIds)
		{
			IMatchmakingClient client = this.client;
			if (client == null)
			{
				return;
			}
			client.MatchesService.ChangeMatch(this.GameId, this.LevelId, this.IsEditSession, this.Version, this.ServerType, null, delegate(int code, string message)
			{
				Action<string, bool> log = this.Log;
				string text = "ChangeMatch error: ";
				HttpStatusCode httpStatusCode = (HttpStatusCode)code;
				log(text + httpStatusCode.ToString() + " - " + message, true);
			}, false, userIds);
		}

		// Token: 0x060002CF RID: 719 RVA: 0x0000C85B File Offset: 0x0000AA5B
		public void RemoveFromMatch(string groupId)
		{
			IMatchmakingClient client = this.client;
			if (client == null)
			{
				return;
			}
			client.MatchesService.RemoveFromMatch(groupId, delegate(int code, string message)
			{
				Action<string, bool> log = this.Log;
				string text = "RemoveFromMatch error: ";
				HttpStatusCode httpStatusCode = (HttpStatusCode)code;
				log(text + httpStatusCode.ToString() + " - " + message, true);
			});
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x0000C880 File Offset: 0x0000AA80
		public void EndMatch()
		{
			if (this.MatchState != ClientController.MatchStates.Allocating && this.MatchState != ClientController.MatchStates.InProgress)
			{
				this.Log("Cannot end match. Operation already in progress", true);
				return;
			}
			this.MatchState = ClientController.MatchStates.Ending;
			IMatchmakingClient client = this.client;
			if (client == null)
			{
				return;
			}
			client.MatchesService.EndMatch(delegate(int code, string message)
			{
				Action<string, bool> log = this.Log;
				string text = "EndMatch error: ";
				HttpStatusCode httpStatusCode = (HttpStatusCode)code;
				log(text + httpStatusCode.ToString() + " - " + message, true);
			});
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x0000C8D9 File Offset: 0x0000AAD9
		private void OnUserChatReceived(string senderId, string message)
		{
			this.client.Log("WHISPER: User " + senderId + " says: " + message, false);
		}

		// Token: 0x060002D2 RID: 722 RVA: 0x0000C8FD File Offset: 0x0000AAFD
		private void OnGroupChatReceived(string senderId, string message)
		{
			this.client.Log("PARTY: User " + senderId + " says: " + message, false);
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x0000C921 File Offset: 0x0000AB21
		private void OnMatchChatReceived(string senderId, string message)
		{
			this.client.Log("MATCH: User " + senderId + " says: " + message, false);
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x0000C945 File Offset: 0x0000AB45
		public void SendUserChat(string message, string recipientId, Action<int, string> onError = null)
		{
			this.client.ChatService.SendChatMessage(message, ChatChannel.User, recipientId, onError);
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x0000C95B File Offset: 0x0000AB5B
		public void SendGroupChat(string message, Action<int, string> onError = null)
		{
			this.client.ChatService.SendChatMessage(message, ChatChannel.Group, null, onError);
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x0000C971 File Offset: 0x0000AB71
		public void SendMatchChat(string message, Action<int, string> onError = null)
		{
			this.client.ChatService.SendChatMessage(message, ChatChannel.Match, null, onError);
		}

		// Token: 0x040001A9 RID: 425
		public string UserName = string.Empty;

		// Token: 0x040001AA RID: 426
		public string Password = string.Empty;

		// Token: 0x040001AB RID: 427
		public UserPlatforms UserPlatform = UserPlatforms.ENDLESS;

		// Token: 0x040001AC RID: 428
		protected readonly Queue<Tuple<NotificationTypes, Document, DateTime>> notifications = new Queue<Tuple<NotificationTypes, Document, DateTime>>();

		// Token: 0x040001AD RID: 429
		protected readonly Queue<Tuple<string, string, DateTime>> groupInvites = new Queue<Tuple<string, string, DateTime>>();

		// Token: 0x040001AE RID: 430
		public string GameId;

		// Token: 0x040001AF RID: 431
		public string LevelId;

		// Token: 0x040001B0 RID: 432
		public bool IsEditSession;

		// Token: 0x040001B1 RID: 433
		public string ServerType;

		// Token: 0x040001B2 RID: 434
		public string Version;

		// Token: 0x040001B3 RID: 435
		public ClientController.MatchStates MatchState;

		// Token: 0x040001B4 RID: 436
		private Guid hostMigrationGuid = Guid.Empty;

		// Token: 0x020000B8 RID: 184
		public enum MatchStates
		{
			// Token: 0x0400031B RID: 795
			None,
			// Token: 0x0400031C RID: 796
			Starting,
			// Token: 0x0400031D RID: 797
			Allocating,
			// Token: 0x0400031E RID: 798
			InProgress,
			// Token: 0x0400031F RID: 799
			Ending
		}
	}
}
