using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Runtime.Shared.Matchmaking;

namespace Endless.Creator.UI;

public class UIOwnedGameAssetCloudPaginatedListModel : UIBaseGameAssetCloudPaginatedListModel
{
	protected override Task<GraphQlResult> RequestPage(PaginationParams paginationParams, CancellationToken cancellationToken)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RequestPage", paginationParams, cancellationToken);
		}
		AssetParams assetParams = new AssetParams(AssetFilter, PopulateRefs, GameAssetPreview.AssetReturnArgs);
		return EndlessServices.Instance.CloudService.GetAssetsByTypeAvailableForRoleAsync(AssetType, 10, assetParams, paginationParams, applyModerationFilter: true, base.VerboseLogging);
	}
}
