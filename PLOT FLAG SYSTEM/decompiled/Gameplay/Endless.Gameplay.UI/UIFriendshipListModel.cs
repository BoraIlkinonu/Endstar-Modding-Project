using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI;

public class UIFriendshipListModel : UIBasePaginatedSocialListModel<Friendship>
{
	protected override Task<GraphQlResult> RequestListTask => EndlessServices.Instance.CloudService.GetFriendshipsAsync(base.PaginationParams);

	protected override ErrorCodes GetDataErrorCode => ErrorCodes.UIFriendshipList_RequestListTask;

	protected override void AddExtractedData(Friendship[] range)
	{
		base.AddExtractedData(range);
		foreach (Friendship friendship in range)
		{
			MonoBehaviourSingleton<RuntimeDatabase>.Instance.CacheUser(friendship.UserOne);
			MonoBehaviourSingleton<RuntimeDatabase>.Instance.CacheUser(friendship.UserTwo);
		}
	}

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
