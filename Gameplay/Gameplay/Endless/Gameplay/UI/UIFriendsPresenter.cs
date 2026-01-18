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
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003A5 RID: 933
	public class UIFriendsPresenter : UIBasePresenter<UIFriendsModel>
	{
		// Token: 0x060017C9 RID: 6089 RVA: 0x0006EA04 File Offset: 0x0006CC04
		protected override void Start()
		{
			base.Start();
			UIFriendsModel uifriendsModel = new UIFriendsModel();
			this.SetModel(uifriendsModel, true);
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnAuthenticationProcessSuccessful));
			MatchmakingClientController.OnDisconnectedFromServer += this.OnDisconnectedFromServer;
			base.Model.OnModelChanged += this.ReviewModel;
			this.typedView = base.View.Interface as UIFriendsView;
			this.typedView.DisplayBlockedUsersModal += this.OpenBlockedUsersModal;
			this.typedView.DisplayFriendRequestsModal += this.OpenFriendRequestsModal;
			this.typedView.DisplaySendFriendRequestModal += this.OpenSendFriendRequestModal;
		}

		// Token: 0x060017CA RID: 6090 RVA: 0x0006EAC8 File Offset: 0x0006CCC8
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			base.Model.Clear();
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnAuthenticationProcessSuccessful));
			MatchmakingClientController.OnDisconnectedFromServer -= this.OnDisconnectedFromServer;
			this.typedView = base.View.Interface as UIFriendsView;
			this.typedView.DisplayBlockedUsersModal -= this.OpenBlockedUsersModal;
			this.typedView.DisplayFriendRequestsModal -= this.OpenFriendRequestsModal;
			this.typedView.DisplaySendFriendRequestModal -= this.OpenSendFriendRequestModal;
		}

		// Token: 0x060017CB RID: 6091 RVA: 0x0006EB84 File Offset: 0x0006CD84
		private void OnAuthenticationProcessSuccessful(ClientData clientData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAuthenticationProcessSuccessful", new object[] { clientData.ToPrettyString() });
			}
			base.Model.Sync();
		}

		// Token: 0x060017CC RID: 6092 RVA: 0x0006EBB3 File Offset: 0x0006CDB3
		private void OnDisconnectedFromServer(string error)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisconnectedFromServer", new object[] { error });
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			base.Model.Clear();
		}

		// Token: 0x060017CD RID: 6093 RVA: 0x0006EBE8 File Offset: 0x0006CDE8
		private void OpenBlockedUsersModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenBlockedUsersModal", Array.Empty<object>());
			}
			List<BlockedUser> list = new List<BlockedUser>(base.Model.BlockedUsers);
			List<User> list2 = new List<User>();
			foreach (Friendship friendship in base.Model.Friendships)
			{
				User user = UISocialUtility.ExtractNonActiveUser(friendship);
				list2.Add(user);
			}
			UIBlockedUserWindowView.Display(new UIBlockedUserWindowModel(list, list2), null);
		}

		// Token: 0x060017CE RID: 6094 RVA: 0x0006EC7C File Offset: 0x0006CE7C
		private void OpenFriendRequestsModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenFriendRequestsModal", Array.Empty<object>());
			}
			if (this.friendRequestWindow)
			{
				return;
			}
			UIFriendRequestWindowModel uifriendRequestWindowModel = new UIFriendRequestWindowModel(base.Model.FriendRequests, base.Model.SentFriendRequests);
			this.friendRequestWindow = UIFriendRequestWindowView.Display(uifriendRequestWindowModel, null);
			this.friendRequestWindow.CloseUnityEvent.AddListener(new UnityAction(this.ClearFriendRequestWindowReference));
		}

		// Token: 0x060017CF RID: 6095 RVA: 0x0006ECF4 File Offset: 0x0006CEF4
		private void OpenSendFriendRequestModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenSendFriendRequestModal", Array.Empty<object>());
			}
			User user = new User(EndlessServices.Instance.CloudService.ActiveUserId, null, EndlessServices.Instance.CloudService.ActiveUserName);
			List<User> list = new List<User>
			{
				this.endlessStudiosUserId.User,
				user
			};
			for (int i = 0; i < base.Model.Friendships.Count; i++)
			{
				User user2 = UISocialUtility.ExtractNonActiveUser(base.Model.Friendships[i]);
				list.Add(user2);
			}
			UIUserSearchWindowView.Display(new UIUserSearchWindowModel("Send Friend Request", list, SelectionType.Select0OrMore, new Action<List<object>>(this.SendFriendRequests)), null);
		}

		// Token: 0x060017D0 RID: 6096 RVA: 0x0006EDB4 File Offset: 0x0006CFB4
		private void ReviewModel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ReviewModel", Array.Empty<object>());
			}
			this.typedView.View(base.Model);
			if (this.friendRequestWindow)
			{
				this.friendRequestWindow.View(base.Model.FriendRequests, base.Model.SentFriendRequests);
			}
		}

		// Token: 0x060017D1 RID: 6097 RVA: 0x0006EE18 File Offset: 0x0006D018
		private void SendFriendRequests(List<object> target)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SendFriendRequests", new object[] { target.Count });
			}
			foreach (object obj in target)
			{
				User user = obj as User;
				base.Model.SendFriendRequestAsync(user);
			}
		}

		// Token: 0x060017D2 RID: 6098 RVA: 0x0006EE98 File Offset: 0x0006D098
		private void ClearFriendRequestWindowReference()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearFriendRequestWindowReference", Array.Empty<object>());
			}
			this.friendRequestWindow.CloseUnityEvent.RemoveListener(new UnityAction(this.ClearFriendRequestWindowReference));
			this.friendRequestWindow = null;
		}

		// Token: 0x04001322 RID: 4898
		[Header("UIFriendsPresenter")]
		[SerializeField]
		private EndlessStudiosUserId endlessStudiosUserId;

		// Token: 0x04001323 RID: 4899
		[SerializeField]
		private int defaultIEnumerableWindowCanvasOverrideSorting = 4;

		// Token: 0x04001324 RID: 4900
		private UIFriendRequestWindowView friendRequestWindow;

		// Token: 0x04001325 RID: 4901
		private UIFriendsView typedView;
	}
}
