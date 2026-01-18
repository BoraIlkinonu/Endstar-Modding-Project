using System;
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

namespace Endless.Gameplay.UI
{
	// Token: 0x0200041D RID: 1053
	public class UIUserPaginatedGraphQlIEnumerableHandler : UIBasePaginatedGraphQlIEnumerableHandler<User>
	{
		// Token: 0x17000546 RID: 1350
		// (get) Token: 0x06001A27 RID: 6695 RVA: 0x00078706 File Offset: 0x00076906
		protected override User SkeletonItem { get; } = new User(0, null, "Loading...");

		// Token: 0x17000547 RID: 1351
		// (get) Token: 0x06001A28 RID: 6696 RVA: 0x0007870E File Offset: 0x0007690E
		private static string SortQuery
		{
			get
			{
				return string.Format("order_by: {{ Name: {0} }}", SortOrders.asc);
			}
		}

		// Token: 0x17000548 RID: 1352
		// (get) Token: 0x06001A29 RID: 6697 RVA: 0x00078720 File Offset: 0x00076920
		// (set) Token: 0x06001A2A RID: 6698 RVA: 0x00078728 File Offset: 0x00076928
		private string query { get; set; } = string.Empty;

		// Token: 0x06001A2B RID: 6699 RVA: 0x00078734 File Offset: 0x00076934
		public void SetUserNameToSearchFor(string userName, List<User> usersToHide)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUserNameToSearchFor", new object[] { userName, usersToHide.Count });
			}
			this.Clear();
			this.query = userName;
			foreach (User user in usersToHide)
			{
				this.iEnumerableView.HideItem(user);
			}
			if (userName.IsNullOrEmptyOrWhiteSpace())
			{
				return;
			}
			base.Request();
		}

		// Token: 0x06001A2C RID: 6700 RVA: 0x000787D0 File Offset: 0x000769D0
		public override void Clear()
		{
			base.Clear();
			this.query = string.Empty;
		}

		// Token: 0x06001A2D RID: 6701 RVA: 0x000787E4 File Offset: 0x000769E4
		protected override Task<GraphQlResult> RequestAsync(CancellationToken cancellationToken)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RequestAsync", new object[] { cancellationToken });
			}
			PaginationParams paginationParams = new PaginationParams("", 0, 50)
			{
				Offset = base.offset,
				Limit = this.limit,
				PaginationQuery = Pagination.QueryString
			};
			return EndlessServices.Instance.CloudService.SearchUsers(this.query, paginationParams, this.verboseLogging);
		}

		// Token: 0x040014E6 RID: 5350
		private const SortOrders SORT_ORDER = SortOrders.asc;

		// Token: 0x040014E7 RID: 5351
		[Header("UIUserPaginatedGraphQlIEnumerableHandler")]
		[SerializeField]
		private UIBaseIEnumerableView iEnumerableView;
	}
}
