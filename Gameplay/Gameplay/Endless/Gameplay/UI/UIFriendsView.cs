using System;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003A6 RID: 934
	public class UIFriendsView : UIBaseView<UIFriendsModel, UIFriendsView.Styles>
	{
		// Token: 0x170004E9 RID: 1257
		// (get) Token: 0x060017D4 RID: 6100 RVA: 0x0006EEE4 File Offset: 0x0006D0E4
		// (set) Token: 0x060017D5 RID: 6101 RVA: 0x0006EEEC File Offset: 0x0006D0EC
		public override UIFriendsView.Styles Style { get; protected set; }

		// Token: 0x1400002E RID: 46
		// (add) Token: 0x060017D6 RID: 6102 RVA: 0x0006EEF8 File Offset: 0x0006D0F8
		// (remove) Token: 0x060017D7 RID: 6103 RVA: 0x0006EF30 File Offset: 0x0006D130
		public event Action DisplayBlockedUsersModal;

		// Token: 0x1400002F RID: 47
		// (add) Token: 0x060017D8 RID: 6104 RVA: 0x0006EF68 File Offset: 0x0006D168
		// (remove) Token: 0x060017D9 RID: 6105 RVA: 0x0006EFA0 File Offset: 0x0006D1A0
		public event Action DisplayFriendRequestsModal;

		// Token: 0x14000030 RID: 48
		// (add) Token: 0x060017DA RID: 6106 RVA: 0x0006EFD8 File Offset: 0x0006D1D8
		// (remove) Token: 0x060017DB RID: 6107 RVA: 0x0006F010 File Offset: 0x0006D210
		public event Action DisplaySendFriendRequestModal;

		// Token: 0x060017DC RID: 6108 RVA: 0x0006F048 File Offset: 0x0006D248
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnAuthenticationProcessSuccessful));
			this.friendshipCollapseButton.onClick.AddListener(new UnityAction(this.friendshipUIDisplayAndHideHandler.Toggle));
			this.openBlockedUsersModalButton.onClick.AddListener(new UnityAction(this.InvokeDisplayBlockedUsersModal));
			this.openFriendRequestsModalButton.onClick.AddListener(new UnityAction(this.InvokeDisplayFriendRequestsModal));
			this.openSendFriendRequestModalButton.onClick.AddListener(new UnityAction(this.InvokeDisplaySendFriendRequestModal));
		}

		// Token: 0x060017DD RID: 6109 RVA: 0x0006F104 File Offset: 0x0006D304
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnAuthenticationProcessSuccessful));
			this.friendshipCollapseButton.onClick.RemoveListener(new UnityAction(this.friendshipUIDisplayAndHideHandler.Toggle));
			this.openBlockedUsersModalButton.onClick.RemoveListener(new UnityAction(this.InvokeDisplayBlockedUsersModal));
			this.openFriendRequestsModalButton.onClick.RemoveListener(new UnityAction(this.InvokeDisplayFriendRequestsModal));
			this.openSendFriendRequestModalButton.onClick.RemoveListener(new UnityAction(this.InvokeDisplaySendFriendRequestModal));
		}

		// Token: 0x060017DE RID: 6110 RVA: 0x0006F1BE File Offset: 0x0006D3BE
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
		}

		// Token: 0x060017DF RID: 6111 RVA: 0x0006F1D8 File Offset: 0x0006D3D8
		public override void View(UIFriendsModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.friendshipTitle.Value = string.Format("Friends {0}", model.Friendships.Count);
			int count = model.BlockedUsers.Count;
			this.blockedUsersBadge.gameObject.SetActive(count > 0);
			this.blockedUsersBadge.Display((count > 99) ? "99+" : count.ToString());
			int num = model.FriendRequests.Count + model.SentFriendRequests.Count;
			this.friendRequestsBadge.gameObject.SetActive(num > 0);
			this.friendRequestsBadge.Display((num > 99) ? "99+" : num.ToString());
			this.friendships.SetModel(model.Friendships, true);
			this.friendships.View.Interface.View(model.Friendships);
		}

		// Token: 0x060017E0 RID: 6112 RVA: 0x0006F2DC File Offset: 0x0006D4DC
		private void InvokeDisplayBlockedUsersModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeDisplayBlockedUsersModal", Array.Empty<object>());
			}
			Action displayBlockedUsersModal = this.DisplayBlockedUsersModal;
			if (displayBlockedUsersModal == null)
			{
				return;
			}
			displayBlockedUsersModal();
		}

		// Token: 0x060017E1 RID: 6113 RVA: 0x0006F306 File Offset: 0x0006D506
		private void InvokeDisplayFriendRequestsModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeDisplayFriendRequestsModal", Array.Empty<object>());
			}
			Action displayFriendRequestsModal = this.DisplayFriendRequestsModal;
			if (displayFriendRequestsModal == null)
			{
				return;
			}
			displayFriendRequestsModal();
		}

		// Token: 0x060017E2 RID: 6114 RVA: 0x0006F330 File Offset: 0x0006D530
		private void InvokeDisplaySendFriendRequestModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeDisplaySendFriendRequestModal", Array.Empty<object>());
			}
			Action displaySendFriendRequestModal = this.DisplaySendFriendRequestModal;
			if (displaySendFriendRequestModal == null)
			{
				return;
			}
			displaySendFriendRequestModal();
		}

		// Token: 0x060017E3 RID: 6115 RVA: 0x0006F35A File Offset: 0x0006D55A
		private void OnAuthenticationProcessSuccessful(ClientData clientData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAuthenticationProcessSuccessful", new object[] { clientData.ToPrettyString() });
			}
			this.friendshipTitle.Value = "Friends 0";
		}

		// Token: 0x04001327 RID: 4903
		[SerializeField]
		private UIButton friendshipCollapseButton;

		// Token: 0x04001328 RID: 4904
		[SerializeField]
		private UIDisplayAndHideHandler friendshipUIDisplayAndHideHandler;

		// Token: 0x04001329 RID: 4905
		[SerializeField]
		private UIText friendshipTitle;

		// Token: 0x0400132A RID: 4906
		[SerializeField]
		private UIBadge blockedUsersBadge;

		// Token: 0x0400132B RID: 4907
		[SerializeField]
		private UIBadge friendRequestsBadge;

		// Token: 0x0400132C RID: 4908
		[SerializeField]
		private UIButton openBlockedUsersModalButton;

		// Token: 0x0400132D RID: 4909
		[SerializeField]
		private UIButton openFriendRequestsModalButton;

		// Token: 0x0400132E RID: 4910
		[SerializeField]
		private UIButton openSendFriendRequestModalButton;

		// Token: 0x0400132F RID: 4911
		[SerializeField]
		private UIIEnumerablePresenter friendships;

		// Token: 0x020003A7 RID: 935
		public enum Styles
		{
			// Token: 0x04001334 RID: 4916
			Default
		}
	}
}
