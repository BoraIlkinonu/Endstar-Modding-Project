using System;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000047 RID: 71
	[RequireComponent(typeof(UIInviteNotificationView))]
	public class UIInviteNotificationController : UIGameObject
	{
		// Token: 0x06000166 RID: 358 RVA: 0x000091D4 File Offset: 0x000073D4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.declineButton.onClick.AddListener(new UnityAction(this.Decline));
			this.acceptButton.onClick.AddListener(new UnityAction(this.Accept));
			base.TryGetComponent<UIInviteNotificationView>(out this.view);
		}

		// Token: 0x06000167 RID: 359 RVA: 0x0000923E File Offset: 0x0000743E
		private void Decline()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Decline", Array.Empty<object>());
			}
			this.view.Hide();
		}

		// Token: 0x06000168 RID: 360 RVA: 0x00009264 File Offset: 0x00007464
		private void Accept()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Accept", Array.Empty<object>());
			}
			UIInviteNotification.UIInviteNotificationTypes inviteNotificationType = this.view.DisplayedInviteNotification.InviteNotificationType;
			if (inviteNotificationType != UIInviteNotification.UIInviteNotificationTypes.UserGroupInvite)
			{
				if (inviteNotificationType != UIInviteNotification.UIInviteNotificationTypes.MatchInvite)
				{
					DebugUtility.LogWarning(this, "Accept", string.Format("No support for a {0} value of {1}!", "UIInviteNotificationTypes", this.view.DisplayedInviteNotification.InviteNotificationType), Array.Empty<object>());
				}
			}
			else
			{
				MatchmakingClientController.Instance.JoinGroup(this.view.DisplayedInviteNotification.UserGroupInvite.GroupId, null);
			}
			this.view.Hide();
		}

		// Token: 0x040000E7 RID: 231
		[SerializeField]
		private UIButton declineButton;

		// Token: 0x040000E8 RID: 232
		[SerializeField]
		private UIButton acceptButton;

		// Token: 0x040000E9 RID: 233
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040000EA RID: 234
		private UIInviteNotificationView view;
	}
}
