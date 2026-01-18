using System;

namespace Endless.Core.UI
{
	// Token: 0x02000048 RID: 72
	public struct UIInviteNotification
	{
		// Token: 0x0600016A RID: 362 RVA: 0x00009304 File Offset: 0x00007504
		public UIInviteNotification(UIInviteNotification.UIInviteNotificationTypes inviteNotificationType, UIUserGroupInvite userGroupInvite, UIMatchInvite matchInvite, string inviterName)
		{
			this.InviteNotificationType = inviteNotificationType;
			this.UserGroupInvite = userGroupInvite;
			this.MatchInvite = matchInvite;
			this.InviterName = inviterName;
		}

		// Token: 0x040000EB RID: 235
		public readonly UIInviteNotification.UIInviteNotificationTypes InviteNotificationType;

		// Token: 0x040000EC RID: 236
		public UIUserGroupInvite UserGroupInvite;

		// Token: 0x040000ED RID: 237
		public UIMatchInvite MatchInvite;

		// Token: 0x040000EE RID: 238
		public readonly string InviterName;

		// Token: 0x02000049 RID: 73
		public enum UIInviteNotificationTypes
		{
			// Token: 0x040000F0 RID: 240
			UserGroupInvite,
			// Token: 0x040000F1 RID: 241
			MatchInvite
		}
	}
}
