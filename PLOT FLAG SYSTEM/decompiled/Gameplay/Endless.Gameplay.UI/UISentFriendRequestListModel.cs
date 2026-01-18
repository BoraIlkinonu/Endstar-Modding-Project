using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI;

public class UISentFriendRequestListModel : UIBasePaginatedSocialListModel<FriendRequest>
{
	protected override Task<GraphQlResult> RequestListTask => EndlessServices.Instance.CloudService.GetSentFriendRequests(base.PaginationParams);

	protected override ErrorCodes GetDataErrorCode => ErrorCodes.UISentFriendRequestList_RequestListTask;

	public override bool RemoveItemWithId(int itemId)
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RemoveItemWithId", itemId);
		}
		for (int i = 0; i < items.Count; i++)
		{
			if (int.TryParse(items[i].RequestId, out var result) && result == itemId)
			{
				items.RemoveAt(i);
				if (VerboseLogging)
				{
					DebugUtility.LogMethodWithAppension(this, "RemoveItemWithId", "Success", itemId);
				}
				InvokeOnModelChanged();
				return true;
			}
		}
		return false;
	}
}
