using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003FF RID: 1023
	public class UIFriendshipView : UIBaseSocialView<Friendship>
	{
		// Token: 0x1400003F RID: 63
		// (add) Token: 0x06001990 RID: 6544 RVA: 0x00075C50 File Offset: 0x00073E50
		// (remove) Token: 0x06001991 RID: 6545 RVA: 0x00075C88 File Offset: 0x00073E88
		public event Action InviteToGroup;

		// Token: 0x14000040 RID: 64
		// (add) Token: 0x06001992 RID: 6546 RVA: 0x00075CC0 File Offset: 0x00073EC0
		// (remove) Token: 0x06001993 RID: 6547 RVA: 0x00075CF8 File Offset: 0x00073EF8
		public event Action Unfriend;

		// Token: 0x06001994 RID: 6548 RVA: 0x00075D30 File Offset: 0x00073F30
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.toggleActionsVisibilityButton.onClick.AddListener(new UnityAction(this.actionsDisplayAndHideHandler.Toggle));
			this.inviteToGroupButton.onClick.AddListener(new UnityAction(this.InvokeInviteToGroup));
			this.unfriendButton.onClick.AddListener(new UnityAction(this.InvokeUnfriend));
		}

		// Token: 0x06001995 RID: 6549 RVA: 0x00075DB0 File Offset: 0x00073FB0
		public override void View(Friendship model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			User user = UISocialUtility.ExtractNonActiveUser(model);
			this.userView.View(user);
			this.HandleInviteToGroupButtonVisibility(model);
		}

		// Token: 0x06001996 RID: 6550 RVA: 0x00075DF4 File Offset: 0x00073FF4
		public override void Clear()
		{
			base.Clear();
			this.actionsDisplayAndHideHandler.SetToHideEnd(true);
			this.inviteToGroupButton.interactable = true;
			this.inviteToGroupButtonCooldownCover.SetActive(false);
			if (this.hideInvitedToGroupStatusCoroutine != null)
			{
				MonoBehaviourSingleton<UICoroutineManager>.Instance.StopThisCoroutine(this.hideInvitedToGroupStatusCoroutine);
				this.hideInvitedToGroupStatusCoroutine = null;
			}
		}

		// Token: 0x06001997 RID: 6551 RVA: 0x00075E4C File Offset: 0x0007404C
		public void DisplayInviteToGroupCooldown()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayInviteToGroupCooldown", Array.Empty<object>());
			}
			this.inviteToGroupButton.interactable = false;
			this.inviteToGroupButtonCooldownCover.SetActive(true);
			this.hideInvitedToGroupStatusCoroutine = MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(this.HideInvitedToGroupStatus());
		}

		// Token: 0x06001998 RID: 6552 RVA: 0x00075EA0 File Offset: 0x000740A0
		public void HandleInviteToGroupButtonVisibility(Friendship model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleInviteToGroupButtonVisibility", new object[] { model });
			}
			string text = UISocialUtility.ExtractNonActiveUser(model).Id.ToString();
			bool flag = true;
			if (MatchmakingClientController.Instance.LocalGroup != null)
			{
				using (List<CoreClientData>.Enumerator enumerator = MatchmakingClientController.Instance.LocalGroup.Members.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.PlatformId == text)
						{
							flag = false;
							break;
						}
					}
				}
			}
			this.inviteToGroupButton.gameObject.SetActive(flag);
		}

		// Token: 0x06001999 RID: 6553 RVA: 0x00075F58 File Offset: 0x00074158
		private IEnumerator HideInvitedToGroupStatus()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HideInvitedToGroupStatus", Array.Empty<object>());
			}
			yield return new WaitForSecondsRealtime(this.expiration.Value);
			this.inviteToGroupButton.interactable = true;
			this.inviteToGroupButtonCooldownCover.SetActive(false);
			this.hideInvitedToGroupStatusCoroutine = null;
			yield break;
		}

		// Token: 0x0600199A RID: 6554 RVA: 0x00075F67 File Offset: 0x00074167
		private void InvokeInviteToGroup()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeInviteToGroup", Array.Empty<object>());
			}
			Action inviteToGroup = this.InviteToGroup;
			if (inviteToGroup != null)
			{
				inviteToGroup();
			}
			this.DisplayInviteToGroupCooldown();
		}

		// Token: 0x0600199B RID: 6555 RVA: 0x00075F98 File Offset: 0x00074198
		private void InvokeUnfriend()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeUnfriend", Array.Empty<object>());
			}
			Action unfriend = this.Unfriend;
			if (unfriend == null)
			{
				return;
			}
			unfriend();
		}

		// Token: 0x04001453 RID: 5203
		[Header("UIFriendshipView")]
		[SerializeField]
		private UIUserView userView;

		// Token: 0x04001454 RID: 5204
		[SerializeField]
		private UIButton toggleActionsVisibilityButton;

		// Token: 0x04001455 RID: 5205
		[SerializeField]
		private UIDisplayAndHideHandler actionsDisplayAndHideHandler;

		// Token: 0x04001456 RID: 5206
		[SerializeField]
		[Tooltip("In Seconds")]
		private FloatVariable expiration;

		// Token: 0x04001457 RID: 5207
		[SerializeField]
		private UIButton inviteToGroupButton;

		// Token: 0x04001458 RID: 5208
		[SerializeField]
		private GameObject inviteToGroupButtonCooldownCover;

		// Token: 0x04001459 RID: 5209
		[SerializeField]
		private UIButton unfriendButton;

		// Token: 0x0400145A RID: 5210
		private Coroutine hideInvitedToGroupStatusCoroutine;
	}
}
