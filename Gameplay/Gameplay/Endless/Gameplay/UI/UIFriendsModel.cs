using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003A2 RID: 930
	public class UIFriendsModel : IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1400002D RID: 45
		// (add) Token: 0x060017AE RID: 6062 RVA: 0x0006E034 File Offset: 0x0006C234
		// (remove) Token: 0x060017AF RID: 6063 RVA: 0x0006E06C File Offset: 0x0006C26C
		public event Action OnModelChanged;

		// Token: 0x170004E2 RID: 1250
		// (get) Token: 0x060017B0 RID: 6064 RVA: 0x0006E0A1 File Offset: 0x0006C2A1
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170004E3 RID: 1251
		// (get) Token: 0x060017B1 RID: 6065 RVA: 0x0006E0A9 File Offset: 0x0006C2A9
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x170004E4 RID: 1252
		// (get) Token: 0x060017B2 RID: 6066 RVA: 0x0006E0B1 File Offset: 0x0006C2B1
		public IReadOnlyList<BlockedUser> BlockedUsers
		{
			get
			{
				return this.blockedUsersListModel.Items;
			}
		}

		// Token: 0x170004E5 RID: 1253
		// (get) Token: 0x060017B3 RID: 6067 RVA: 0x0006E0BE File Offset: 0x0006C2BE
		public IReadOnlyList<FriendRequest> FriendRequests
		{
			get
			{
				return this.friendRequestsListModel.Items;
			}
		}

		// Token: 0x170004E6 RID: 1254
		// (get) Token: 0x060017B4 RID: 6068 RVA: 0x0006E0CB File Offset: 0x0006C2CB
		public IReadOnlyList<FriendRequest> SentFriendRequests
		{
			get
			{
				return this.sentFriendRequestsListModel.Items;
			}
		}

		// Token: 0x170004E7 RID: 1255
		// (get) Token: 0x060017B5 RID: 6069 RVA: 0x0006E0D8 File Offset: 0x0006C2D8
		public IReadOnlyList<Friendship> Friendships
		{
			get
			{
				return this.friendshipsListModel.Items;
			}
		}

		// Token: 0x170004E8 RID: 1256
		// (get) Token: 0x060017B6 RID: 6070 RVA: 0x0006E0E5 File Offset: 0x0006C2E5
		// (set) Token: 0x060017B7 RID: 6071 RVA: 0x0006E0ED File Offset: 0x0006C2ED
		public bool SyncInProgress { get; private set; }

		// Token: 0x060017B8 RID: 6072 RVA: 0x0006E0F6 File Offset: 0x0006C2F6
		public void Sync()
		{
			if (!this.subscribedToEvents)
			{
				this.SubscribeToEvents();
			}
			this.SyncAsync();
		}

		// Token: 0x060017B9 RID: 6073 RVA: 0x0006E10D File Offset: 0x0006C30D
		public void Clear()
		{
			this.blockedUsersListModel.Clear();
			this.friendRequestsListModel.Clear();
			this.friendshipsListModel.Clear();
			this.UnsubscribeFromEvents();
		}

		// Token: 0x060017BA RID: 6074 RVA: 0x0006E138 File Offset: 0x0006C338
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7} }}", new object[]
			{
				"blockedUsersListModel",
				this.blockedUsersListModel.Items.Count,
				"friendRequestsListModel",
				this.friendRequestsListModel.Items.Count,
				"friendshipsListModel",
				this.friendshipsListModel.Items.Count,
				"subscribedToEvents",
				this.subscribedToEvents
			});
		}

		// Token: 0x060017BB RID: 6075 RVA: 0x0006E1CC File Offset: 0x0006C3CC
		public async Task SendFriendRequestAsync(User user)
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SendFriendRequestAsync(user.UserName, false);
			if (graphQlResult.HasErrors)
			{
				DebugUtility.LogError(graphQlResult.GetErrorMessage(0).Message, null);
			}
			else
			{
				FriendRequest dataMember = graphQlResult.GetDataMember<FriendRequest>();
				this.sentFriendRequestsListModel.Add(dataMember);
			}
		}

		// Token: 0x060017BC RID: 6076 RVA: 0x0006E218 File Offset: 0x0006C418
		private async Task SyncAsync()
		{
			if (!this.SyncInProgress)
			{
				this.OnLoadingStarted.Invoke();
				this.SyncInProgress = true;
				try
				{
					Task<List<Friendship>> task = this.friendshipsListModel.RequestListAsync();
					Task<List<BlockedUser>> task2 = this.blockedUsersListModel.RequestListAsync();
					Task<List<FriendRequest>> task3 = this.friendRequestsListModel.RequestListAsync();
					Task<List<FriendRequest>> task4 = this.sentFriendRequestsListModel.RequestListAsync();
					await Task.WhenAll(new Task[] { task, task2, task3, task4 });
				}
				catch (Exception ex)
				{
					DebugUtility.LogException(ex, null);
				}
				finally
				{
					this.SyncInProgress = false;
					this.OnLoadingEnded.Invoke();
				}
			}
		}

		// Token: 0x060017BD RID: 6077 RVA: 0x0006E25C File Offset: 0x0006C45C
		private void OnFriendRequestSent(WebSocketPayload webSocketPayload)
		{
			var <>f__AnonymousType = new
			{
				friend_request_id = 0,
				sender_id = 0,
				recipient_id = 0,
				request_status = ""
			};
			if (JsonConvert.DeserializeAnonymousType(webSocketPayload.RawMetaData.ToString(), <>f__AnonymousType).sender_id == EndlessServices.Instance.CloudService.ActiveUserId)
			{
				this.sentFriendRequestsListModel.Clear();
				this.sentFriendRequestsListModel.RequestListAsync();
				return;
			}
			this.friendRequestsListModel.Clear();
			this.friendRequestsListModel.RequestListAsync();
		}

		// Token: 0x060017BE RID: 6078 RVA: 0x0006E2D0 File Offset: 0x0006C4D0
		private void OnFriendRequestAccepted(WebSocketPayload webSocketPayload)
		{
			var <>f__AnonymousType = new
			{
				friend_request_id = 0,
				sender_id = 0,
				recipient_id = 0,
				request_status = ""
			};
			var <>f__AnonymousType2 = JsonConvert.DeserializeAnonymousType(webSocketPayload.RawMetaData.ToString(), <>f__AnonymousType);
			if (<>f__AnonymousType2.sender_id == EndlessServices.Instance.CloudService.ActiveUserId)
			{
				this.sentFriendRequestsListModel.RemoveItemWithId(<>f__AnonymousType2.friend_request_id);
			}
			else
			{
				this.friendRequestsListModel.RemoveItemWithId(<>f__AnonymousType2.friend_request_id);
			}
			this.friendshipsListModel.Clear();
			this.friendshipsListModel.RequestListAsync();
		}

		// Token: 0x060017BF RID: 6079 RVA: 0x0006E354 File Offset: 0x0006C554
		private void OnFriendRequestRejected(WebSocketPayload webSocketPayload)
		{
			var <>f__AnonymousType = new
			{
				friend_request_id = 0,
				sender_id = 0,
				recipient_id = 0,
				request_status = ""
			};
			var <>f__AnonymousType2 = JsonConvert.DeserializeAnonymousType(webSocketPayload.RawMetaData.ToString(), <>f__AnonymousType);
			if (<>f__AnonymousType2.sender_id == EndlessServices.Instance.CloudService.ActiveUserId)
			{
				this.sentFriendRequestsListModel.RemoveItemWithId(<>f__AnonymousType2.friend_request_id);
				return;
			}
			this.friendRequestsListModel.RemoveItemWithId(<>f__AnonymousType2.friend_request_id);
		}

		// Token: 0x060017C0 RID: 6080 RVA: 0x0006E3C0 File Offset: 0x0006C5C0
		private void OnFriendRequestCancelled(WebSocketPayload webSocketPayload)
		{
			var <>f__AnonymousType = new
			{
				friend_request_id = 0,
				sender_id = 0,
				recipient_id = 0,
				request_status = ""
			};
			var <>f__AnonymousType2 = JsonConvert.DeserializeAnonymousType(webSocketPayload.RawMetaData.ToString(), <>f__AnonymousType);
			if (<>f__AnonymousType2.sender_id == EndlessServices.Instance.CloudService.ActiveUserId)
			{
				this.sentFriendRequestsListModel.RemoveItemWithId(<>f__AnonymousType2.friend_request_id);
				return;
			}
			this.friendRequestsListModel.RemoveItemWithId(<>f__AnonymousType2.friend_request_id);
		}

		// Token: 0x060017C1 RID: 6081 RVA: 0x0006E42C File Offset: 0x0006C62C
		private void OnAcceptFriendRequest(FriendRequest friendRequest)
		{
			User user = UISocialUtility.ExtractNonActiveUser(friendRequest);
			User user2 = new User(EndlessServices.Instance.CloudService.ActiveUserId, null, EndlessServices.Instance.CloudService.ActiveUserName);
			this.friendRequestsListModel.Remove(friendRequest);
			Friendship friendship = new Friendship(user, user2);
			this.friendshipsListModel.Add(friendship);
		}

		// Token: 0x060017C2 RID: 6082 RVA: 0x0006E484 File Offset: 0x0006C684
		private void SubscribeToEvents()
		{
			if (this.subscribedToEvents)
			{
				return;
			}
			this.blockedUsersListModel.OnModelChanged += this.OnModelChanged;
			this.friendRequestsListModel.OnModelChanged += this.OnModelChanged;
			this.sentFriendRequestsListModel.OnModelChanged += this.OnModelChanged;
			this.friendshipsListModel.OnModelChanged += this.OnModelChanged;
			EndlessServices.Instance.CloudService.AddWebSocketCallback(WebSocketMessageId.FriendRequestSent, new Action<WebSocketPayload>(this.OnFriendRequestSent));
			EndlessServices.Instance.CloudService.AddWebSocketCallback(WebSocketMessageId.FriendRequestAccepted, new Action<WebSocketPayload>(this.OnFriendRequestAccepted));
			EndlessServices.Instance.CloudService.AddWebSocketCallback(WebSocketMessageId.FriendRequestRejected, new Action<WebSocketPayload>(this.OnFriendRequestRejected));
			EndlessServices.Instance.CloudService.AddWebSocketCallback(WebSocketMessageId.FriendRequestCancelled, new Action<WebSocketPayload>(this.OnFriendRequestCancelled));
			UIBlockedUserPresenter.OnUnblock = (Action<BlockedUser>)Delegate.Combine(UIBlockedUserPresenter.OnUnblock, new Action<BlockedUser>(this.blockedUsersListModel.Remove));
			UIFriendRequestPresenter.OnAcceptFriendRequest += this.OnAcceptFriendRequest;
			UIFriendRequestPresenter.OnRejectFriendRequest += this.friendRequestsListModel.Remove;
			UIFriendRequestPresenter.OnCancelFriendRequest += this.sentFriendRequestsListModel.Remove;
			UIFriendshipPresenter.OnUnfriend += this.friendshipsListModel.Remove;
			this.subscribedToEvents = true;
		}

		// Token: 0x060017C3 RID: 6083 RVA: 0x0006E5D0 File Offset: 0x0006C7D0
		private void UnsubscribeFromEvents()
		{
			if (!this.subscribedToEvents)
			{
				return;
			}
			this.blockedUsersListModel.OnModelChanged -= this.OnModelChanged;
			this.friendRequestsListModel.OnModelChanged -= this.OnModelChanged;
			this.sentFriendRequestsListModel.OnModelChanged -= this.OnModelChanged;
			this.friendshipsListModel.OnModelChanged -= this.OnModelChanged;
			if (EndlessServices.Instance)
			{
				EndlessServices.Instance.CloudService.RemoveWebSocketCallback(WebSocketMessageId.FriendRequestSent, new Action<WebSocketPayload>(this.OnFriendRequestSent));
				EndlessServices.Instance.CloudService.RemoveWebSocketCallback(WebSocketMessageId.FriendRequestAccepted, new Action<WebSocketPayload>(this.OnFriendRequestAccepted));
				EndlessServices.Instance.CloudService.RemoveWebSocketCallback(WebSocketMessageId.FriendRequestRejected, new Action<WebSocketPayload>(this.OnFriendRequestRejected));
				EndlessServices.Instance.CloudService.RemoveWebSocketCallback(WebSocketMessageId.FriendRequestCancelled, new Action<WebSocketPayload>(this.OnFriendRequestCancelled));
			}
			UIBlockedUserPresenter.OnUnblock = (Action<BlockedUser>)Delegate.Remove(UIBlockedUserPresenter.OnUnblock, new Action<BlockedUser>(this.blockedUsersListModel.Remove));
			UIFriendRequestPresenter.OnAcceptFriendRequest -= this.OnAcceptFriendRequest;
			UIFriendRequestPresenter.OnRejectFriendRequest -= this.friendRequestsListModel.Remove;
			UIFriendRequestPresenter.OnCancelFriendRequest -= this.sentFriendRequestsListModel.Remove;
			UIFriendshipPresenter.OnUnfriend -= this.friendshipsListModel.Remove;
			this.subscribedToEvents = false;
		}

		// Token: 0x0400130F RID: 4879
		private const bool VERBOSE_LOGGING = false;

		// Token: 0x04001311 RID: 4881
		private readonly UIBlockedUserListModel blockedUsersListModel = new UIBlockedUserListModel();

		// Token: 0x04001312 RID: 4882
		private readonly UIFriendRequestListModel friendRequestsListModel = new UIFriendRequestListModel();

		// Token: 0x04001313 RID: 4883
		private readonly UIFriendshipListModel friendshipsListModel = new UIFriendshipListModel();

		// Token: 0x04001314 RID: 4884
		private readonly UISentFriendRequestListModel sentFriendRequestsListModel = new UISentFriendRequestListModel();

		// Token: 0x04001315 RID: 4885
		private bool subscribedToEvents;
	}
}
