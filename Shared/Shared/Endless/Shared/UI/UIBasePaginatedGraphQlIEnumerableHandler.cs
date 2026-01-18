using System;
using System.Collections.Generic;
using Endless.GraphQl;
using Endless.Shared.Debugging;
using Endless.Shared.Pagination;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001EE RID: 494
	public abstract class UIBasePaginatedGraphQlIEnumerableHandler<TItem> : UIBaseGraphQlIEnumerableHandler<TItem>
	{
		// Token: 0x1700023C RID: 572
		// (get) Token: 0x06000C32 RID: 3122 RVA: 0x00034C7E File Offset: 0x00032E7E
		protected int offset
		{
			get
			{
				return this.pagination.Offset;
			}
		}

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x06000C33 RID: 3123
		protected abstract TItem SkeletonItem { get; }

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x06000C34 RID: 3124 RVA: 0x00034C8B File Offset: 0x00032E8B
		private int LastPage
		{
			get
			{
				return this.pagination.LastPage;
			}
		}

		// Token: 0x06000C35 RID: 3125 RVA: 0x00034C98 File Offset: 0x00032E98
		private void Start()
		{
			this.iEnumerablePresenter.IEnumerableView.OnItemViewed += this.OnItemViewed;
		}

		// Token: 0x06000C36 RID: 3126 RVA: 0x00034CB8 File Offset: 0x00032EB8
		public void OnItemViewed(int index, Direction direction)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "OnItemViewed", "index", index, "direction", direction }), this);
			}
			int num = index / this.limit + 1;
			this.TryToLoadNextPages(direction, num);
		}

		// Token: 0x06000C37 RID: 3127 RVA: 0x00034D20 File Offset: 0x00032F20
		public override void Clear()
		{
			base.Clear();
			this.pageStatuses.Clear();
			this.pageLoadQueue.Clear();
			this.pagination = new Pagination
			{
				From = 0,
				LastPage = 0,
				Limit = 0,
				Offset = 0,
				To = 0,
				Total = 0
			};
		}

		// Token: 0x06000C38 RID: 3128 RVA: 0x00034D80 File Offset: 0x00032F80
		protected override void LoadCollectionRequest(CollectionRequest<TItem> collectionRequest)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "LoadCollectionRequest", "collectionRequest", collectionRequest), this);
			}
			this.pagination = collectionRequest.Pagination;
			int num = (this.pagination.From - 1) / this.pagination.Limit + 1;
			for (int i = 1; i <= this.LastPage; i++)
			{
				this.pageStatuses.TryAdd(i, UIBasePaginatedGraphQlIEnumerableHandler<TItem>.PageLoadStatus.NotLoaded);
			}
			this.pageStatuses[num] = UIBasePaginatedGraphQlIEnumerableHandler<TItem>.PageLoadStatus.Loaded;
			if (this.items.Capacity != this.pagination.Total)
			{
				this.items.Capacity = this.pagination.Total;
			}
			while (this.items.Count < this.pagination.Total)
			{
				this.items.Add(this.SkeletonItem);
			}
			if (this.items.Count > this.pagination.Total)
			{
				this.items.RemoveRange(this.pagination.Total, this.items.Count - this.pagination.Total);
			}
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "items", this.items.Count), this);
			}
			if (collectionRequest.Data.Length != 0)
			{
				int num2 = 0;
				for (int j = this.pagination.From - 1; j < this.pagination.To; j++)
				{
					this.items[j] = collectionRequest.Data[num2];
					num2++;
				}
			}
			this.iEnumerablePresenter.SetModel(this.items, true);
		}

		// Token: 0x06000C39 RID: 3129 RVA: 0x00034F2C File Offset: 0x0003312C
		private void TryToLoadNextPages(Direction direction, int page)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "TryToLoadNextPages", "direction", direction, "page", page }), this);
			}
			if (this.pageLoadQueue.Count >= this.pagesToLoadLimit)
			{
				return;
			}
			int num;
			int num2;
			int num3;
			int num4;
			if (direction != Direction.Forward)
			{
				if (direction != Direction.Backward)
				{
					throw new ArgumentOutOfRangeException("direction", direction, "Unsupported direction value.");
				}
				num = -1;
				num2 = 1;
				num3 = 1;
				num4 = this.LastPage;
			}
			else
			{
				num = 1;
				num2 = this.LastPage;
				num3 = -1;
				num4 = 1;
			}
			this.AttemptToQueuePagesInScanDirection(page, num2, num, page);
			if (this.pageLoadQueue.Count < this.pagesToLoadLimit)
			{
				this.AttemptToQueuePagesInScanDirection(page, num4, num3, page);
			}
		}

		// Token: 0x06000C3A RID: 3130 RVA: 0x00034FFC File Offset: 0x000331FC
		private void AttemptToQueuePagesInScanDirection(int scanStartPage, int targetBoundary, int step, int pivotPageForDistance)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8} )", new object[] { "AttemptToQueuePagesInScanDirection", "scanStartPage", scanStartPage, "targetBoundary", targetBoundary, "step", step, "pivotPageForDistance", pivotPageForDistance }), this);
			}
			int num = scanStartPage;
			while (((step == 1) ? (num <= targetBoundary) : (num >= targetBoundary)) && this.pageLoadQueue.Count < this.pagesToLoadLimit && (step != 1 || num <= pivotPageForDistance + this.maxPageLoadDistance) && (step != -1 || num >= pivotPageForDistance - this.maxPageLoadDistance))
			{
				UIBasePaginatedGraphQlIEnumerableHandler<TItem>.PageLoadStatus pageLoadStatus;
				if (!this.pageStatuses.TryGetValue(num, out pageLoadStatus))
				{
					if (this.verboseLogging)
					{
						DebugUtility.LogError(string.Format("Page {0} not found in {1}. Assuming it's effectively {2} or an issue!", num, "pageStatuses", UIBasePaginatedGraphQlIEnumerableHandler<TItem>.PageLoadStatus.NotLoaded), null);
					}
				}
				else if (pageLoadStatus == UIBasePaginatedGraphQlIEnumerableHandler<TItem>.PageLoadStatus.NotLoaded || pageLoadStatus == UIBasePaginatedGraphQlIEnumerableHandler<TItem>.PageLoadStatus.Failed)
				{
					this.pageLoadQueue.Add(num);
				}
				num += step;
			}
		}

		// Token: 0x040007E3 RID: 2019
		[Header("UIBasePaginatedGraphQlIEnumerableHandler")]
		[SerializeField]
		protected int limit = 60;

		// Token: 0x040007E4 RID: 2020
		[Tooltip("Don't load more than this many pages.")]
		[SerializeField]
		private int pagesToLoadLimit = 3;

		// Token: 0x040007E5 RID: 2021
		[Tooltip("Don't load pages more than this many pages ahead.")]
		[SerializeField]
		private int maxPageLoadDistance = 3;

		// Token: 0x040007E6 RID: 2022
		private readonly Dictionary<int, UIBasePaginatedGraphQlIEnumerableHandler<TItem>.PageLoadStatus> pageStatuses = new Dictionary<int, UIBasePaginatedGraphQlIEnumerableHandler<TItem>.PageLoadStatus>();

		// Token: 0x040007E7 RID: 2023
		private readonly SortedSet<int> pageLoadQueue = new SortedSet<int>();

		// Token: 0x040007E8 RID: 2024
		private Pagination pagination = new Pagination
		{
			From = 0,
			LastPage = 0,
			Limit = 0,
			Offset = 0,
			To = 0,
			Total = 0
		};

		// Token: 0x020001EF RID: 495
		private enum PageLoadStatus
		{
			// Token: 0x040007EA RID: 2026
			NotLoaded,
			// Token: 0x040007EB RID: 2027
			Queued,
			// Token: 0x040007EC RID: 2028
			Loading,
			// Token: 0x040007ED RID: 2029
			Loaded,
			// Token: 0x040007EE RID: 2030
			Failed
		}
	}
}
