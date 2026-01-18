using System;
using System.Threading.Tasks;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI;

public class UIFriendshipPresenter : UIBasePresenter<Friendship>
{
	private bool subscribedToMatchmakingClientController;

	private UIFriendshipView typedView;

	public static event Action<Friendship> OnUnfriend;

	protected override void Start()
	{
		base.Start();
		typedView = base.View.Interface as UIFriendshipView;
		typedView.InviteToGroup += InviteToGroup;
		typedView.Unfriend += Unfriend;
	}

	private void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		typedView.InviteToGroup -= InviteToGroup;
		typedView.Unfriend -= Unfriend;
	}

	private void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		if (!subscribedToMatchmakingClientController)
		{
			SubscribeToMatchmakingClientController();
		}
	}

	private void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		if (subscribedToMatchmakingClientController)
		{
			UnsubscribeToMatchmakingClientController();
		}
	}

	private void InviteToGroup()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InviteToGroup");
		}
		User user = UISocialUtility.ExtractNonActiveUser(base.Model);
		MatchmakingClientController.Instance.InviteToGroup(user.Id.ToString());
		MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Invite Sent", null, user.UserName + " has been invited to your group!", UIModalManagerStackActions.ClearStack);
	}

	private void Unfriend()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Unfriend");
		}
		UnfriendAsync();
	}

	private async Task UnfriendAsync()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Unfriend");
		}
		User nonActiveUser = UISocialUtility.ExtractNonActiveUser(base.Model);
		if (await MonoBehaviourSingleton<UIModalManager>.Instance.Confirm("Are you sure you want to unfriend " + nonActiveUser.UserName + "?", UIModalManagerStackActions.ClearStack))
		{
			EndlessServices.Instance.CloudService.UnfriendAsync(nonActiveUser.Id, debugQuery: true);
			UIFriendshipPresenter.OnUnfriend?.Invoke(base.Model);
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
	}

	private void SubscribeToMatchmakingClientController()
	{
		if (!subscribedToMatchmakingClientController)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SubscribeToMatchmakingClientController");
			}
			subscribedToMatchmakingClientController = true;
			MatchmakingClientController.GroupJoined += OnGroupJoined;
			MatchmakingClientController.GroupJoin += OnGroupJoin;
			MatchmakingClientController.GroupLeave += OnGroupLeave;
			MatchmakingClientController.GroupLeft += OnGroupLeft;
		}
	}

	private void UnsubscribeToMatchmakingClientController()
	{
		if (subscribedToMatchmakingClientController)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UnsubscribeToMatchmakingClientController");
			}
			subscribedToMatchmakingClientController = false;
			MatchmakingClientController.GroupJoined -= OnGroupJoined;
			MatchmakingClientController.GroupJoin -= OnGroupJoin;
			MatchmakingClientController.GroupLeave -= OnGroupLeave;
			MatchmakingClientController.GroupLeft -= OnGroupLeft;
		}
	}

	private void OnGroupJoined()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnGroupJoined");
		}
		typedView.HandleInviteToGroupButtonVisibility(base.Model);
	}

	private void OnGroupJoin(string joinedUserId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnGroupJoin", joinedUserId);
		}
		typedView.HandleInviteToGroupButtonVisibility(base.Model);
	}

	private void OnGroupLeave(string leaverUserId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnGroupLeave", leaverUserId);
		}
		typedView.HandleInviteToGroupButtonVisibility(base.Model);
	}

	private void OnGroupLeft()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnGroupLeft");
		}
		if (MatchmakingClientController.Instance.LocalClientData.HasValue)
		{
			OnGroupLeave(MatchmakingClientController.Instance.LocalClientData.Value.CoreData.PlatformId);
		}
	}
}
