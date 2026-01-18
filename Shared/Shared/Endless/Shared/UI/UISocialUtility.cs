using System;
using Endless.Shared.DataTypes;
using Endless.Shared.Social;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200027B RID: 635
	public static class UISocialUtility
	{
		// Token: 0x06000FE6 RID: 4070 RVA: 0x0004420C File Offset: 0x0004240C
		public static User ExtractNonActiveUser(Friendship friendship)
		{
			if (friendship == null)
			{
				throw new NullReferenceException("friendship is null!");
			}
			if (!EndlessServices.Instance)
			{
				throw new NullReferenceException("EndlessServices.Initialized is null!");
			}
			if (EndlessServices.Instance.CloudService == null)
			{
				throw new NullReferenceException("EndlessServices.Initialized.CloudService is null!");
			}
			if (friendship.UserOne == null)
			{
				object obj = new NullReferenceException("friendship.UserOne is null!");
				Debug.Log(string.Format("{0}: {1}", "friendship", friendship));
				throw obj;
			}
			if (friendship.UserTwo == null)
			{
				object obj2 = new NullReferenceException("friendship.UserTwo is null!");
				Debug.Log(string.Format("{0}: {1}", "friendship", friendship));
				throw obj2;
			}
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			if (friendship.UserOne.Id != activeUserId)
			{
				return friendship.UserOne;
			}
			return friendship.UserTwo;
		}

		// Token: 0x06000FE7 RID: 4071 RVA: 0x000442D0 File Offset: 0x000424D0
		public static User ExtractNonActiveUser(BlockedUser blockedUser)
		{
			if (blockedUser == null)
			{
				throw new NullReferenceException("blockedUser is null!");
			}
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			if (blockedUser.UserOne.Id != activeUserId)
			{
				return blockedUser.UserOne;
			}
			return blockedUser.UserTwo;
		}

		// Token: 0x06000FE8 RID: 4072 RVA: 0x00044318 File Offset: 0x00042518
		public static User ExtractNonActiveUser(FriendRequest friendRequest)
		{
			if (friendRequest == null)
			{
				throw new NullReferenceException("friendRequest is null!");
			}
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			if (friendRequest.Recipient.Id != activeUserId)
			{
				return friendRequest.Recipient;
			}
			return friendRequest.Sender;
		}
	}
}
