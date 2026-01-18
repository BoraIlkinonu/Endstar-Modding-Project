using System;
using System.Collections.Generic;
using Endless.Data;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIFriendsPresenter : UIBasePresenter<UIFriendsModel>
{
	[Header("UIFriendsPresenter")]
	[SerializeField]
	private EndlessStudiosUserId endlessStudiosUserId;

	[SerializeField]
	private int defaultIEnumerableWindowCanvasOverrideSorting = 4;

	private UIFriendRequestWindowView friendRequestWindow;

	private UIFriendsView typedView;

	protected override void Start()
	{
		base.Start();
		UIFriendsModel model = new UIFriendsModel();
		SetModel(model, triggerOnModelChanged: true);
		MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(OnAuthenticationProcessSuccessful));
		MatchmakingClientController.OnDisconnectedFromServer += OnDisconnectedFromServer;
		base.Model.OnModelChanged += ReviewModel;
		typedView = base.View.Interface as UIFriendsView;
		typedView.DisplayBlockedUsersModal += OpenBlockedUsersModal;
		typedView.DisplayFriendRequestsModal += OpenFriendRequestsModal;
		typedView.DisplaySendFriendRequestModal += OpenSendFriendRequestModal;
	}

	private void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		base.Model.Clear();
		MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(OnAuthenticationProcessSuccessful));
		MatchmakingClientController.OnDisconnectedFromServer -= OnDisconnectedFromServer;
		typedView = base.View.Interface as UIFriendsView;
		typedView.DisplayBlockedUsersModal -= OpenBlockedUsersModal;
		typedView.DisplayFriendRequestsModal -= OpenFriendRequestsModal;
		typedView.DisplaySendFriendRequestModal -= OpenSendFriendRequestModal;
	}

	private void OnAuthenticationProcessSuccessful(ClientData clientData)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAuthenticationProcessSuccessful", clientData.ToPrettyString());
		}
		base.Model.Sync();
	}

	private void OnDisconnectedFromServer(string error)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisconnectedFromServer", error);
		}
		if (!ExitManager.IsQuitting)
		{
			base.Model.Clear();
		}
	}

	private void OpenBlockedUsersModal()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OpenBlockedUsersModal");
		}
		List<BlockedUser> blockedUsers = new List<BlockedUser>(base.Model.BlockedUsers);
		List<User> list = new List<User>();
		foreach (Friendship friendship in base.Model.Friendships)
		{
			User item = UISocialUtility.ExtractNonActiveUser(friendship);
			list.Add(item);
		}
		UIBlockedUserWindowView.Display(new UIBlockedUserWindowModel(blockedUsers, list));
	}

	private void OpenFriendRequestsModal()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OpenFriendRequestsModal");
		}
		if (!friendRequestWindow)
		{
			UIFriendRequestWindowModel model = new UIFriendRequestWindowModel(base.Model.FriendRequests, base.Model.SentFriendRequests);
			friendRequestWindow = UIFriendRequestWindowView.Display(model);
			friendRequestWindow.CloseUnityEvent.AddListener(ClearFriendRequestWindowReference);
		}
	}

	private void OpenSendFriendRequestModal()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OpenSendFriendRequestModal");
		}
		User item = new User(EndlessServices.Instance.CloudService.ActiveUserId, null, EndlessServices.Instance.CloudService.ActiveUserName);
		List<User> list = new List<User> { endlessStudiosUserId.User, item };
		for (int i = 0; i < base.Model.Friendships.Count; i++)
		{
			User item2 = UISocialUtility.ExtractNonActiveUser(base.Model.Friendships[i]);
			list.Add(item2);
		}
		UIUserSearchWindowView.Display(new UIUserSearchWindowModel("Send Friend Request", list, SelectionType.Select0OrMore, SendFriendRequests));
	}

	private void ReviewModel()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ReviewModel");
		}
		typedView.View(base.Model);
		if ((bool)friendRequestWindow)
		{
			friendRequestWindow.View(base.Model.FriendRequests, base.Model.SentFriendRequests);
		}
	}

	private void SendFriendRequests(List<object> target)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SendFriendRequests", target.Count);
		}
		foreach (object item in target)
		{
			User user = item as User;
			base.Model.SendFriendRequestAsync(user);
		}
	}

	private void ClearFriendRequestWindowReference()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ClearFriendRequestWindowReference");
		}
		friendRequestWindow.CloseUnityEvent.RemoveListener(ClearFriendRequestWindowReference);
		friendRequestWindow = null;
	}
}
