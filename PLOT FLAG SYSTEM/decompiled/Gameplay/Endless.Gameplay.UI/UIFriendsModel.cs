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

namespace Endless.Gameplay.UI;

public class UIFriendsModel : IUILoadingSpinnerViewCompatible
{
	private const bool VERBOSE_LOGGING = false;

	private readonly UIBlockedUserListModel blockedUsersListModel = new UIBlockedUserListModel();

	private readonly UIFriendRequestListModel friendRequestsListModel = new UIFriendRequestListModel();

	private readonly UIFriendshipListModel friendshipsListModel = new UIFriendshipListModel();

	private readonly UISentFriendRequestListModel sentFriendRequestsListModel = new UISentFriendRequestListModel();

	private bool subscribedToEvents;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public IReadOnlyList<BlockedUser> BlockedUsers => blockedUsersListModel.Items;

	public IReadOnlyList<FriendRequest> FriendRequests => friendRequestsListModel.Items;

	public IReadOnlyList<FriendRequest> SentFriendRequests => sentFriendRequestsListModel.Items;

	public IReadOnlyList<Friendship> Friendships => friendshipsListModel.Items;

	public bool SyncInProgress { get; private set; }

	public event Action OnModelChanged;

	public void Sync()
	{
		if (!subscribedToEvents)
		{
			SubscribeToEvents();
		}
		SyncAsync();
	}

	public void Clear()
	{
		blockedUsersListModel.Clear();
		friendRequestsListModel.Clear();
		friendshipsListModel.Clear();
		UnsubscribeFromEvents();
	}

	public override string ToString()
	{
		return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7} }}", "blockedUsersListModel", blockedUsersListModel.Items.Count, "friendRequestsListModel", friendRequestsListModel.Items.Count, "friendshipsListModel", friendshipsListModel.Items.Count, "subscribedToEvents", subscribedToEvents);
	}

	public async Task SendFriendRequestAsync(User user)
	{
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SendFriendRequestAsync(user.UserName);
		if (graphQlResult.HasErrors)
		{
			DebugUtility.LogError(graphQlResult.GetErrorMessage().Message);
			return;
		}
		FriendRequest dataMember = graphQlResult.GetDataMember<FriendRequest>();
		sentFriendRequestsListModel.Add(dataMember);
	}

	private async Task SyncAsync()
	{
		if (SyncInProgress)
		{
			return;
		}
		OnLoadingStarted.Invoke();
		SyncInProgress = true;
		try
		{
			Task<List<Friendship>> task = friendshipsListModel.RequestListAsync();
			Task<List<BlockedUser>> task2 = blockedUsersListModel.RequestListAsync();
			Task<List<FriendRequest>> task3 = friendRequestsListModel.RequestListAsync();
			Task<List<FriendRequest>> task4 = sentFriendRequestsListModel.RequestListAsync();
			await Task.WhenAll(task, task2, task3, task4);
		}
		catch (Exception exception)
		{
			DebugUtility.LogException(exception);
		}
		finally
		{
			SyncInProgress = false;
			OnLoadingEnded.Invoke();
		}
	}

	private void OnFriendRequestSent(WebSocketPayload webSocketPayload)
	{
		var anonymousTypeObject = new
		{
			friend_request_id = 0,
			sender_id = 0,
			recipient_id = 0,
			request_status = ""
		};
		if (JsonConvert.DeserializeAnonymousType(webSocketPayload.RawMetaData.ToString(), anonymousTypeObject).sender_id == EndlessServices.Instance.CloudService.ActiveUserId)
		{
			sentFriendRequestsListModel.Clear();
			sentFriendRequestsListModel.RequestListAsync();
		}
		else
		{
			friendRequestsListModel.Clear();
			friendRequestsListModel.RequestListAsync();
		}
	}

	private void OnFriendRequestAccepted(WebSocketPayload webSocketPayload)
	{
		var anonymousTypeObject = new
		{
			friend_request_id = 0,
			sender_id = 0,
			recipient_id = 0,
			request_status = ""
		};
		var anon = JsonConvert.DeserializeAnonymousType(webSocketPayload.RawMetaData.ToString(), anonymousTypeObject);
		if (anon.sender_id == EndlessServices.Instance.CloudService.ActiveUserId)
		{
			sentFriendRequestsListModel.RemoveItemWithId(anon.friend_request_id);
		}
		else
		{
			friendRequestsListModel.RemoveItemWithId(anon.friend_request_id);
		}
		friendshipsListModel.Clear();
		friendshipsListModel.RequestListAsync();
	}

	private void OnFriendRequestRejected(WebSocketPayload webSocketPayload)
	{
		var anonymousTypeObject = new
		{
			friend_request_id = 0,
			sender_id = 0,
			recipient_id = 0,
			request_status = ""
		};
		var anon = JsonConvert.DeserializeAnonymousType(webSocketPayload.RawMetaData.ToString(), anonymousTypeObject);
		if (anon.sender_id == EndlessServices.Instance.CloudService.ActiveUserId)
		{
			sentFriendRequestsListModel.RemoveItemWithId(anon.friend_request_id);
		}
		else
		{
			friendRequestsListModel.RemoveItemWithId(anon.friend_request_id);
		}
	}

	private void OnFriendRequestCancelled(WebSocketPayload webSocketPayload)
	{
		var anonymousTypeObject = new
		{
			friend_request_id = 0,
			sender_id = 0,
			recipient_id = 0,
			request_status = ""
		};
		var anon = JsonConvert.DeserializeAnonymousType(webSocketPayload.RawMetaData.ToString(), anonymousTypeObject);
		if (anon.sender_id == EndlessServices.Instance.CloudService.ActiveUserId)
		{
			sentFriendRequestsListModel.RemoveItemWithId(anon.friend_request_id);
		}
		else
		{
			friendRequestsListModel.RemoveItemWithId(anon.friend_request_id);
		}
	}

	private void OnAcceptFriendRequest(FriendRequest friendRequest)
	{
		User userOne = UISocialUtility.ExtractNonActiveUser(friendRequest);
		User userTwo = new User(EndlessServices.Instance.CloudService.ActiveUserId, null, EndlessServices.Instance.CloudService.ActiveUserName);
		friendRequestsListModel.Remove(friendRequest);
		Friendship item = new Friendship(userOne, userTwo);
		friendshipsListModel.Add(item);
	}

	private void SubscribeToEvents()
	{
		if (!subscribedToEvents)
		{
			blockedUsersListModel.OnModelChanged += this.OnModelChanged;
			friendRequestsListModel.OnModelChanged += this.OnModelChanged;
			sentFriendRequestsListModel.OnModelChanged += this.OnModelChanged;
			friendshipsListModel.OnModelChanged += this.OnModelChanged;
			EndlessServices.Instance.CloudService.AddWebSocketCallback(WebSocketMessageId.FriendRequestSent, OnFriendRequestSent);
			EndlessServices.Instance.CloudService.AddWebSocketCallback(WebSocketMessageId.FriendRequestAccepted, OnFriendRequestAccepted);
			EndlessServices.Instance.CloudService.AddWebSocketCallback(WebSocketMessageId.FriendRequestRejected, OnFriendRequestRejected);
			EndlessServices.Instance.CloudService.AddWebSocketCallback(WebSocketMessageId.FriendRequestCancelled, OnFriendRequestCancelled);
			UIBlockedUserPresenter.OnUnblock = (Action<BlockedUser>)Delegate.Combine(UIBlockedUserPresenter.OnUnblock, new Action<BlockedUser>(blockedUsersListModel.Remove));
			UIFriendRequestPresenter.OnAcceptFriendRequest += OnAcceptFriendRequest;
			UIFriendRequestPresenter.OnRejectFriendRequest += friendRequestsListModel.Remove;
			UIFriendRequestPresenter.OnCancelFriendRequest += sentFriendRequestsListModel.Remove;
			UIFriendshipPresenter.OnUnfriend += friendshipsListModel.Remove;
			subscribedToEvents = true;
		}
	}

	private void UnsubscribeFromEvents()
	{
		if (subscribedToEvents)
		{
			blockedUsersListModel.OnModelChanged -= this.OnModelChanged;
			friendRequestsListModel.OnModelChanged -= this.OnModelChanged;
			sentFriendRequestsListModel.OnModelChanged -= this.OnModelChanged;
			friendshipsListModel.OnModelChanged -= this.OnModelChanged;
			if ((bool)EndlessServices.Instance)
			{
				EndlessServices.Instance.CloudService.RemoveWebSocketCallback(WebSocketMessageId.FriendRequestSent, OnFriendRequestSent);
				EndlessServices.Instance.CloudService.RemoveWebSocketCallback(WebSocketMessageId.FriendRequestAccepted, OnFriendRequestAccepted);
				EndlessServices.Instance.CloudService.RemoveWebSocketCallback(WebSocketMessageId.FriendRequestRejected, OnFriendRequestRejected);
				EndlessServices.Instance.CloudService.RemoveWebSocketCallback(WebSocketMessageId.FriendRequestCancelled, OnFriendRequestCancelled);
			}
			UIBlockedUserPresenter.OnUnblock = (Action<BlockedUser>)Delegate.Remove(UIBlockedUserPresenter.OnUnblock, new Action<BlockedUser>(blockedUsersListModel.Remove));
			UIFriendRequestPresenter.OnAcceptFriendRequest -= OnAcceptFriendRequest;
			UIFriendRequestPresenter.OnRejectFriendRequest -= friendRequestsListModel.Remove;
			UIFriendRequestPresenter.OnCancelFriendRequest -= sentFriendRequestsListModel.Remove;
			UIFriendshipPresenter.OnUnfriend -= friendshipsListModel.Remove;
			subscribedToEvents = false;
		}
	}
}
