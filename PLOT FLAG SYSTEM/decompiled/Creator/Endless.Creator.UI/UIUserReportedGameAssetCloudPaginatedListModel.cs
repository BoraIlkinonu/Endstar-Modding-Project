using System.Threading;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Runtime.Shared.Matchmaking;

namespace Endless.Creator.UI;

public class UIUserReportedGameAssetCloudPaginatedListModel : UIBaseGameAssetCloudPaginatedListModel
{
	protected override Task<GraphQlResult> RequestPage(PaginationParams paginationParams, CancellationToken cancellationToken)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RequestPage", paginationParams, cancellationToken);
		}
		return EndlessServices.Instance.CloudService.GetAssetsByTypeAsync(AssetType, null, paginationParams, applyModerationFilter: false, debugQuery: false, 60);
	}
}
