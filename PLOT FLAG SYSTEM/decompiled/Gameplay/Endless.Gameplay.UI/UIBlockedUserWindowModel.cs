using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Shared.Social;

namespace Endless.Gameplay.UI;

public class UIBlockedUserWindowModel
{
	public List<BlockedUser> BlockedUsers;

	public List<User> Friends;

	public UIBlockedUserWindowModel(List<BlockedUser> blockedUsers, List<User> friends)
	{
		BlockedUsers = blockedUsers;
		Friends = friends;
	}
}
