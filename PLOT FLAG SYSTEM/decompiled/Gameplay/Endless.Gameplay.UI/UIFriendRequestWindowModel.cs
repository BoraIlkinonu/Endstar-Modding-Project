using System.Collections.Generic;
using Endless.Shared.Social;

namespace Endless.Gameplay.UI;

public class UIFriendRequestWindowModel
{
	public readonly IReadOnlyList<FriendRequest> ReceivedFriendRequests;

	public readonly IReadOnlyList<FriendRequest> SentFriendRequests;

	public UIFriendRequestWindowModel(IReadOnlyList<FriendRequest> receivedFriendRequests, IReadOnlyList<FriendRequest> sentFriendRequests)
	{
		ReceivedFriendRequests = receivedFriendRequests;
		SentFriendRequests = sentFriendRequests;
	}
}
