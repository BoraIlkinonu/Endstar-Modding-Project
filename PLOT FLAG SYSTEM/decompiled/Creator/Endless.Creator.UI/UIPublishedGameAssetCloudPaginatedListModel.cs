using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPublishedGameAssetCloudPaginatedListModel : UIBaseGameAssetCloudPaginatedListModel
{
	[SerializeField]
	private UIPublishStates publishingState = UIPublishStates.Public;

	protected override Task<GraphQlResult> RequestPage(PaginationParams paginationParams, CancellationToken cancellationToken)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RequestPage", paginationParams, cancellationToken);
		}
		AssetParams assetParams = new AssetParams(AssetFilter, PopulateRefs, GameAssetPreview.AssetReturnArgs);
		return EndlessServices.Instance.CloudService.GetAssetsByTypeAndPublishStateAsync(AssetType, publishingState.ToEndlessCloudServicesCompatibleString(), assetParams, paginationParams);
	}
}
