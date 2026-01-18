using System;
using Endless.Networking;

namespace Endless.Core.UI
{
	// Token: 0x0200004E RID: 78
	public struct UIUserGroupInvite
	{
		// Token: 0x0600017F RID: 383 RVA: 0x00009C64 File Offset: 0x00007E64
		public UIUserGroupInvite(string groupId, CoreClientData inviteClientData, float expiration)
		{
			this.GroupId = groupId;
			this.InviteClientData = inviteClientData;
			this.DateTime = DateTime.Now;
			this.Expiration = DateTime.Now.AddSeconds((double)expiration);
		}

		// Token: 0x0400010F RID: 271
		public string GroupId;

		// Token: 0x04000110 RID: 272
		public CoreClientData InviteClientData;

		// Token: 0x04000111 RID: 273
		public DateTime DateTime;

		// Token: 0x04000112 RID: 274
		public DateTime Expiration;
	}
}
