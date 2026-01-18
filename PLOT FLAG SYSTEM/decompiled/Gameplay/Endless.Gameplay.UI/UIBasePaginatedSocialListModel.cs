using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Endless.Shared.Pagination;

namespace Endless.Gameplay.UI;

public abstract class UIBasePaginatedSocialListModel<T> : UIBaseSocialListModel<T>
{
	private int total;

	protected PaginationParams PaginationParams => new PaginationParams(Pagination.QueryString, base.Items.Count);

	protected override T[] ExtractData(GraphQlResult graphQlResult)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ExtractData", "graphQlResult", graphQlResult));
			DebugUtility.Log(graphQlResult.GetDataMember().ToString());
		}
		CollectionRequest<T> dataMemberAsCollection = graphQlResult.GetDataMemberAsCollection<T>();
		total = dataMemberAsCollection.Pagination.Total;
		return dataMemberAsCollection.Data;
	}

	protected override void OnLoadComplete()
	{
		if (base.Items.Count < total && !isLoading)
		{
			RequestListAsync();
		}
		else
		{
			base.OnLoadComplete();
		}
	}

	public override void Clear()
	{
		base.Clear();
		total = 0;
	}
}
