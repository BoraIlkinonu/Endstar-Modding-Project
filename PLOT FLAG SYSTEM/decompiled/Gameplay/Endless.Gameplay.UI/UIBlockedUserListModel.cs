using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI;

public class UIBlockedUserListModel : UIBasePaginatedSocialListModel<BlockedUser>
{
	protected override Task<GraphQlResult> RequestListTask => EndlessServices.Instance.CloudService.GetBlockedUsersAsync(base.PaginationParams);

	protected override ErrorCodes GetDataErrorCode => ErrorCodes.UIBlockedUserList_RequestListTask;

	public override bool RemoveItemWithId(int itemId)
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RemoveItemWithId", itemId);
		}
		for (int i = 0; i < items.Count; i++)
		{
			if (UISocialUtility.ExtractNonActiveUser(items[i]).Id == itemId)
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
