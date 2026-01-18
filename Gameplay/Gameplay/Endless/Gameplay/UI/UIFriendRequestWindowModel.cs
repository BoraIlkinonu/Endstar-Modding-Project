using System;
using System.Collections.Generic;
using Endless.Shared.Social;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000414 RID: 1044
	public class UIFriendRequestWindowModel
	{
		// Token: 0x060019F8 RID: 6648 RVA: 0x0007726A File Offset: 0x0007546A
		public UIFriendRequestWindowModel(IReadOnlyList<FriendRequest> receivedFriendRequests, IReadOnlyList<FriendRequest> sentFriendRequests)
		{
			this.ReceivedFriendRequests = receivedFriendRequests;
			this.SentFriendRequests = sentFriendRequests;
		}

		// Token: 0x040014A2 RID: 5282
		public readonly IReadOnlyList<FriendRequest> ReceivedFriendRequests;

		// Token: 0x040014A3 RID: 5283
		public readonly IReadOnlyList<FriendRequest> SentFriendRequests;
	}
}
