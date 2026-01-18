using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Shared.Social;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200040F RID: 1039
	public class UIBlockedUserWindowModel
	{
		// Token: 0x060019E8 RID: 6632 RVA: 0x000770BE File Offset: 0x000752BE
		public UIBlockedUserWindowModel(List<BlockedUser> blockedUsers, List<User> friends)
		{
			this.BlockedUsers = blockedUsers;
			this.Friends = friends;
		}

		// Token: 0x0400149A RID: 5274
		public List<BlockedUser> BlockedUsers;

		// Token: 0x0400149B RID: 5275
		public List<User> Friends;
	}
}
