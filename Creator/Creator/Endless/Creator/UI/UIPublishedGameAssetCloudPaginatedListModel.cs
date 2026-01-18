using System;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200010D RID: 269
	public class UIPublishedGameAssetCloudPaginatedListModel : UIBaseGameAssetCloudPaginatedListModel
	{
		// Token: 0x06000453 RID: 1107 RVA: 0x00019ED8 File Offset: 0x000180D8
		protected override Task<GraphQlResult> RequestPage(PaginationParams paginationParams, CancellationToken cancellationToken)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RequestPage", new object[] { paginationParams, cancellationToken });
			}
			AssetParams assetParams = new AssetParams(this.AssetFilter, this.PopulateRefs, GameAssetPreview.AssetReturnArgs);
			return EndlessServices.Instance.CloudService.GetAssetsByTypeAndPublishStateAsync(this.AssetType, this.publishingState.ToEndlessCloudServicesCompatibleString(), assetParams, paginationParams, false);
		}

		// Token: 0x04000432 RID: 1074
		[SerializeField]
		private UIPublishStates publishingState = UIPublishStates.Public;
	}
}
