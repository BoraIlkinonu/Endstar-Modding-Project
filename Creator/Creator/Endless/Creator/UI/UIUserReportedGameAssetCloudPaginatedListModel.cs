using System;
using System.Threading;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Runtime.Shared.Matchmaking;

namespace Endless.Creator.UI
{
	// Token: 0x0200010E RID: 270
	public class UIUserReportedGameAssetCloudPaginatedListModel : UIBaseGameAssetCloudPaginatedListModel
	{
		// Token: 0x06000455 RID: 1109 RVA: 0x00019F54 File Offset: 0x00018154
		protected override Task<GraphQlResult> RequestPage(PaginationParams paginationParams, CancellationToken cancellationToken)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RequestPage", new object[] { paginationParams, cancellationToken });
			}
			return EndlessServices.Instance.CloudService.GetAssetsByTypeAsync(this.AssetType, null, paginationParams, false, false, 60);
		}
	}
}
