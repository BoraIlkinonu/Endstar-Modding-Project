using System;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Runtime.Shared.Matchmaking;

namespace Endless.Creator.UI
{
	// Token: 0x0200010C RID: 268
	public class UIOwnedGameAssetCloudPaginatedListModel : UIBaseGameAssetCloudPaginatedListModel
	{
		// Token: 0x06000451 RID: 1105 RVA: 0x00019E64 File Offset: 0x00018064
		protected override Task<GraphQlResult> RequestPage(PaginationParams paginationParams, CancellationToken cancellationToken)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RequestPage", new object[] { paginationParams, cancellationToken });
			}
			AssetParams assetParams = new AssetParams(this.AssetFilter, this.PopulateRefs, GameAssetPreview.AssetReturnArgs);
			return EndlessServices.Instance.CloudService.GetAssetsByTypeAvailableForRoleAsync(this.AssetType, 10, assetParams, paginationParams, true, base.VerboseLogging);
		}
	}
}
