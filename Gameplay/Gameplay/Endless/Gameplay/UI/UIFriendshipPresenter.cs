using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003FD RID: 1021
	public class UIFriendshipPresenter : UIBasePresenter<Friendship>
	{
		// Token: 0x0600197E RID: 6526 RVA: 0x000756D4 File Offset: 0x000738D4
		protected override void Start()
		{
			base.Start();
			this.typedView = base.View.Interface as UIFriendshipView;
			this.typedView.InviteToGroup += this.InviteToGroup;
			this.typedView.Unfriend += this.Unfriend;
		}

		// Token: 0x0600197F RID: 6527 RVA: 0x0007572C File Offset: 0x0007392C
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.typedView.InviteToGroup -= this.InviteToGroup;
			this.typedView.Unfriend -= this.Unfriend;
		}

		// Token: 0x06001980 RID: 6528 RVA: 0x0007577F File Offset: 0x0007397F
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (!this.subscribedToMatchmakingClientController)
			{
				this.SubscribeToMatchmakingClientController();
			}
		}

		// Token: 0x06001981 RID: 6529 RVA: 0x000757A7 File Offset: 0x000739A7
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (this.subscribedToMatchmakingClientController)
			{
				this.UnsubscribeToMatchmakingClientController();
			}
		}

		// Token: 0x06001982 RID: 6530 RVA: 0x000757D0 File Offset: 0x000739D0
		private void InviteToGroup()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InviteToGroup", Array.Empty<object>());
			}
			User user = UISocialUtility.ExtractNonActiveUser(base.Model);
			MatchmakingClientController.Instance.InviteToGroup(user.Id.ToString(), null);
			MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Invite Sent", null, user.UserName + " has been invited to your group!", UIModalManagerStackActions.ClearStack, Array.Empty<UIModalGenericViewAction>());
		}

		// Token: 0x06001983 RID: 6531 RVA: 0x00075840 File Offset: 0x00073A40
		private void Unfriend()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Unfriend", Array.Empty<object>());
			}
			this.UnfriendAsync();
		}

		// Token: 0x06001984 RID: 6532 RVA: 0x00075864 File Offset: 0x00073A64
		private async Task UnfriendAsync()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Unfriend", Array.Empty<object>());
			}
			User nonActiveUser = UISocialUtility.ExtractNonActiveUser(base.Model);
			TaskAwaiter<bool> taskAwaiter = MonoBehaviourSingleton<UIModalManager>.Instance.Confirm("Are you sure you want to unfriend " + nonActiveUser.UserName + "?", UIModalManagerStackActions.ClearStack).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<bool> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<bool>);
			}
			if (taskAwaiter.GetResult())
			{
				EndlessServices.Instance.CloudService.UnfriendAsync(nonActiveUser.Id, true);
				Action<Friendship> onUnfriend = UIFriendshipPresenter.OnUnfriend;
				if (onUnfriend != null)
				{
					onUnfriend(base.Model);
				}
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}

		// Token: 0x06001985 RID: 6533 RVA: 0x000758A8 File Offset: 0x00073AA8
		private void SubscribeToMatchmakingClientController()
		{
			if (this.subscribedToMatchmakingClientController)
			{
				return;
			}
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SubscribeToMatchmakingClientController", Array.Empty<object>());
			}
			this.subscribedToMatchmakingClientController = true;
			MatchmakingClientController.GroupJoined += this.OnGroupJoined;
			MatchmakingClientController.GroupJoin += this.OnGroupJoin;
			MatchmakingClientController.GroupLeave += this.OnGroupLeave;
			MatchmakingClientController.GroupLeft += this.OnGroupLeft;
		}

		// Token: 0x06001986 RID: 6534 RVA: 0x00075924 File Offset: 0x00073B24
		private void UnsubscribeToMatchmakingClientController()
		{
			if (!this.subscribedToMatchmakingClientController)
			{
				return;
			}
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UnsubscribeToMatchmakingClientController", Array.Empty<object>());
			}
			this.subscribedToMatchmakingClientController = false;
			MatchmakingClientController.GroupJoined -= this.OnGroupJoined;
			MatchmakingClientController.GroupJoin -= this.OnGroupJoin;
			MatchmakingClientController.GroupLeave -= this.OnGroupLeave;
			MatchmakingClientController.GroupLeft -= this.OnGroupLeft;
		}

		// Token: 0x06001987 RID: 6535 RVA: 0x0007599D File Offset: 0x00073B9D
		private void OnGroupJoined()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGroupJoined", Array.Empty<object>());
			}
			this.typedView.HandleInviteToGroupButtonVisibility(base.Model);
		}

		// Token: 0x06001988 RID: 6536 RVA: 0x000759C8 File Offset: 0x00073BC8
		private void OnGroupJoin(string joinedUserId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGroupJoin", new object[] { joinedUserId });
			}
			this.typedView.HandleInviteToGroupButtonVisibility(base.Model);
		}

		// Token: 0x06001989 RID: 6537 RVA: 0x000759F8 File Offset: 0x00073BF8
		private void OnGroupLeave(string leaverUserId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGroupLeave", new object[] { leaverUserId });
			}
			this.typedView.HandleInviteToGroupButtonVisibility(base.Model);
		}

		// Token: 0x0600198A RID: 6538 RVA: 0x00075A28 File Offset: 0x00073C28
		private void OnGroupLeft()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGroupLeft", Array.Empty<object>());
			}
			if (MatchmakingClientController.Instance.LocalClientData != null)
			{
				this.OnGroupLeave(MatchmakingClientController.Instance.LocalClientData.Value.CoreData.PlatformId);
			}
		}

		// Token: 0x1400003E RID: 62
		// (add) Token: 0x0600198B RID: 6539 RVA: 0x00075A84 File Offset: 0x00073C84
		// (remove) Token: 0x0600198C RID: 6540 RVA: 0x00075AB8 File Offset: 0x00073CB8
		public static event Action<Friendship> OnUnfriend;

		// Token: 0x0400144B RID: 5195
		private bool subscribedToMatchmakingClientController;

		// Token: 0x0400144C RID: 5196
		private UIFriendshipView typedView;
	}
}
