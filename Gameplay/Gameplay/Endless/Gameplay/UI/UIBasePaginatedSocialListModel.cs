using System;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Endless.Shared.Pagination;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003EE RID: 1006
	public abstract class UIBasePaginatedSocialListModel<T> : UIBaseSocialListModel<T>
	{
		// Token: 0x17000522 RID: 1314
		// (get) Token: 0x0600192E RID: 6446 RVA: 0x000746D6 File Offset: 0x000728D6
		protected PaginationParams PaginationParams
		{
			get
			{
				return new PaginationParams(Pagination.QueryString, base.Items.Count, 50);
			}
		}

		// Token: 0x0600192F RID: 6447 RVA: 0x000746F0 File Offset: 0x000728F0
		protected override T[] ExtractData(GraphQlResult graphQlResult)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ExtractData", "graphQlResult", graphQlResult), null);
				DebugUtility.Log(graphQlResult.GetDataMember().ToString(), null);
			}
			CollectionRequest<T> dataMemberAsCollection = graphQlResult.GetDataMemberAsCollection<T>();
			this.total = dataMemberAsCollection.Pagination.Total;
			return dataMemberAsCollection.Data;
		}

		// Token: 0x06001930 RID: 6448 RVA: 0x0007474F File Offset: 0x0007294F
		protected override void OnLoadComplete()
		{
			if (base.Items.Count < this.total && !this.isLoading)
			{
				base.RequestListAsync();
				return;
			}
			base.OnLoadComplete();
		}

		// Token: 0x06001931 RID: 6449 RVA: 0x0007477A File Offset: 0x0007297A
		public override void Clear()
		{
			base.Clear();
			this.total = 0;
		}

		// Token: 0x04001428 RID: 5160
		private int total;
	}
}
