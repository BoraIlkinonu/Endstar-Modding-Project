using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Pagination;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIUserPaginatedGraphQlIEnumerableHandler : UIBasePaginatedGraphQlIEnumerableHandler<User>
{
	private const SortOrders SORT_ORDER = SortOrders.asc;

	[Header("UIUserPaginatedGraphQlIEnumerableHandler")]
	[SerializeField]
	private UIBaseIEnumerableView iEnumerableView;

	protected override User SkeletonItem { get; } = new User(0, null, "Loading...");

	private static string SortQuery => $"order_by: {{ Name: {SortOrders.asc} }}";

	private string query { get; set; } = string.Empty;

	public void SetUserNameToSearchFor(string userName, List<User> usersToHide)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetUserNameToSearchFor", userName, usersToHide.Count);
		}
		Clear();
		query = userName;
		foreach (User item in usersToHide)
		{
			iEnumerableView.HideItem(item);
		}
		if (!userName.IsNullOrEmptyOrWhiteSpace())
		{
			Request();
		}
	}

	public override void Clear()
	{
		base.Clear();
		query = string.Empty;
	}

	protected override Task<GraphQlResult> RequestAsync(CancellationToken cancellationToken)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "RequestAsync", cancellationToken);
		}
		PaginationParams paginationParams = new PaginationParams
		{
			Offset = base.offset,
			Limit = limit,
			PaginationQuery = Pagination.QueryString
		};
		return EndlessServices.Instance.CloudService.SearchUsers(query, paginationParams, verboseLogging);
	}
}
