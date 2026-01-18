using System;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIFriendsView : UIBaseView<UIFriendsModel, UIFriendsView.Styles>
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private UIButton friendshipCollapseButton;

	[SerializeField]
	private UIDisplayAndHideHandler friendshipUIDisplayAndHideHandler;

	[SerializeField]
	private UIText friendshipTitle;

	[SerializeField]
	private UIBadge blockedUsersBadge;

	[SerializeField]
	private UIBadge friendRequestsBadge;

	[SerializeField]
	private UIButton openBlockedUsersModalButton;

	[SerializeField]
	private UIButton openFriendRequestsModalButton;

	[SerializeField]
	private UIButton openSendFriendRequestModalButton;

	[SerializeField]
	private UIIEnumerablePresenter friendships;

	[field: Header("UIFriendsView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public event Action DisplayBlockedUsersModal;

	public event Action DisplayFriendRequestsModal;

	public event Action DisplaySendFriendRequestModal;

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(OnAuthenticationProcessSuccessful));
		friendshipCollapseButton.onClick.AddListener(friendshipUIDisplayAndHideHandler.Toggle);
		openBlockedUsersModalButton.onClick.AddListener(InvokeDisplayBlockedUsersModal);
		openFriendRequestsModalButton.onClick.AddListener(InvokeDisplayFriendRequestsModal);
		openSendFriendRequestModalButton.onClick.AddListener(InvokeDisplaySendFriendRequestModal);
	}

	private void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(OnAuthenticationProcessSuccessful));
		friendshipCollapseButton.onClick.RemoveListener(friendshipUIDisplayAndHideHandler.Toggle);
		openBlockedUsersModalButton.onClick.RemoveListener(InvokeDisplayBlockedUsersModal);
		openFriendRequestsModalButton.onClick.RemoveListener(InvokeDisplayFriendRequestsModal);
		openSendFriendRequestModalButton.onClick.RemoveListener(InvokeDisplaySendFriendRequestModal);
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
	}

	public override void View(UIFriendsModel model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		friendshipTitle.Value = $"Friends {model.Friendships.Count}";
		int count = model.BlockedUsers.Count;
		blockedUsersBadge.gameObject.SetActive(count > 0);
		blockedUsersBadge.Display((count > 99) ? "99+" : count.ToString());
		int num = model.FriendRequests.Count + model.SentFriendRequests.Count;
		friendRequestsBadge.gameObject.SetActive(num > 0);
		friendRequestsBadge.Display((num > 99) ? "99+" : num.ToString());
		friendships.SetModel(model.Friendships, triggerOnModelChanged: true);
		friendships.View.Interface.View(model.Friendships);
	}

	private void InvokeDisplayBlockedUsersModal()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeDisplayBlockedUsersModal");
		}
		this.DisplayBlockedUsersModal?.Invoke();
	}

	private void InvokeDisplayFriendRequestsModal()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeDisplayFriendRequestsModal");
		}
		this.DisplayFriendRequestsModal?.Invoke();
	}

	private void InvokeDisplaySendFriendRequestModal()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeDisplaySendFriendRequestModal");
		}
		this.DisplaySendFriendRequestModal?.Invoke();
	}

	private void OnAuthenticationProcessSuccessful(ClientData clientData)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAuthenticationProcessSuccessful", clientData.ToPrettyString());
		}
		friendshipTitle.Value = "Friends 0";
	}
}
