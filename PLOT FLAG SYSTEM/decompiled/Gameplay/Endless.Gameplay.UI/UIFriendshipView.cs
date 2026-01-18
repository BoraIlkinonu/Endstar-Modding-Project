using System;
using System.Collections;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.Social;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIFriendshipView : UIBaseSocialView<Friendship>
{
	[Header("UIFriendshipView")]
	[SerializeField]
	private UIUserView userView;

	[SerializeField]
	private UIButton toggleActionsVisibilityButton;

	[SerializeField]
	private UIDisplayAndHideHandler actionsDisplayAndHideHandler;

	[SerializeField]
	[Tooltip("In Seconds")]
	private FloatVariable expiration;

	[SerializeField]
	private UIButton inviteToGroupButton;

	[SerializeField]
	private GameObject inviteToGroupButtonCooldownCover;

	[SerializeField]
	private UIButton unfriendButton;

	private Coroutine hideInvitedToGroupStatusCoroutine;

	public event Action InviteToGroup;

	public event Action Unfriend;

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		toggleActionsVisibilityButton.onClick.AddListener(actionsDisplayAndHideHandler.Toggle);
		inviteToGroupButton.onClick.AddListener(InvokeInviteToGroup);
		unfriendButton.onClick.AddListener(InvokeUnfriend);
	}

	public override void View(Friendship model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		User model2 = UISocialUtility.ExtractNonActiveUser(model);
		userView.View(model2);
		HandleInviteToGroupButtonVisibility(model);
	}

	public override void Clear()
	{
		base.Clear();
		actionsDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
		inviteToGroupButton.interactable = true;
		inviteToGroupButtonCooldownCover.SetActive(value: false);
		if (hideInvitedToGroupStatusCoroutine != null)
		{
			MonoBehaviourSingleton<UICoroutineManager>.Instance.StopThisCoroutine(hideInvitedToGroupStatusCoroutine);
			hideInvitedToGroupStatusCoroutine = null;
		}
	}

	public void DisplayInviteToGroupCooldown()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayInviteToGroupCooldown");
		}
		inviteToGroupButton.interactable = false;
		inviteToGroupButtonCooldownCover.SetActive(value: true);
		hideInvitedToGroupStatusCoroutine = MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(HideInvitedToGroupStatus());
	}

	public void HandleInviteToGroupButtonVisibility(Friendship model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleInviteToGroupButtonVisibility", model);
		}
		string text = UISocialUtility.ExtractNonActiveUser(model).Id.ToString();
		bool active = true;
		if (MatchmakingClientController.Instance.LocalGroup != null)
		{
			foreach (CoreClientData member in MatchmakingClientController.Instance.LocalGroup.Members)
			{
				if (member.PlatformId == text)
				{
					active = false;
					break;
				}
			}
		}
		inviteToGroupButton.gameObject.SetActive(active);
	}

	private IEnumerator HideInvitedToGroupStatus()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HideInvitedToGroupStatus");
		}
		yield return new WaitForSecondsRealtime(expiration.Value);
		inviteToGroupButton.interactable = true;
		inviteToGroupButtonCooldownCover.SetActive(value: false);
		hideInvitedToGroupStatusCoroutine = null;
	}

	private void InvokeInviteToGroup()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeInviteToGroup");
		}
		this.InviteToGroup?.Invoke();
		DisplayInviteToGroupCooldown();
	}

	private void InvokeUnfriend()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeUnfriend");
		}
		this.Unfriend?.Invoke();
	}
}
