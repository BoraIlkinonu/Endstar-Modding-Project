using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Endless.Shared.Pagination;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200018E RID: 398
	public abstract class UIBaseCloudPaginatedListModel<T> : UIBaseCloudListModel<T>
	{
		// Token: 0x170001B8 RID: 440
		public override T this[int index]
		{
			get
			{
				int num = index / this.PageSize + 1;
				this.viewedPages.Add(num);
				this.viewedItemIndexes.Add(index);
				this.lastViewedItemIndex = index;
				this.lastViewedPage = num;
				UIBaseCloudPaginatedListModel<T>.PreloadStrategy preloadStrategy = this.preloadStrategy;
				if (preloadStrategy != UIBaseCloudPaginatedListModel<T>.PreloadStrategy.ButtonPagination)
				{
					if (preloadStrategy == UIBaseCloudPaginatedListModel<T>.PreloadStrategy.InfiniteScroll)
					{
						this.RequestPagesAroundPage(num);
					}
				}
				else
				{
					if (this.CanBeRequested(num))
					{
						this.immediatePageRequest[0] = num;
						this.RequestPages(this.immediatePageRequest);
					}
					this.MaybePreloadMore();
				}
				return base[index];
			}
		}

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x060009C6 RID: 2502 RVA: 0x00029948 File Offset: 0x00027B48
		// (set) Token: 0x060009C7 RID: 2503 RVA: 0x00029950 File Offset: 0x00027B50
		public int PageSize { get; private set; } = 3;

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x060009C8 RID: 2504 RVA: 0x00029959 File Offset: 0x00027B59
		// (set) Token: 0x060009C9 RID: 2505 RVA: 0x00029961 File Offset: 0x00027B61
		public int LastPage { get; private set; } = -1;

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x060009CA RID: 2506 RVA: 0x0002996A File Offset: 0x00027B6A
		public bool IsInitialized
		{
			get
			{
				return this.Count > -1 && this.LastPage > -1;
			}
		}

		// Token: 0x170001BC RID: 444
		// (get) Token: 0x060009CB RID: 2507 RVA: 0x00029980 File Offset: 0x00027B80
		public int PageCount
		{
			get
			{
				return (this.Count + this.PageSize - 1) / this.PageSize;
			}
		}

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x060009CC RID: 2508 RVA: 0x00029998 File Offset: 0x00027B98
		public bool AllPagesLoaded
		{
			get
			{
				if (!this.IsInitialized || this.LastPage < 1)
				{
					return false;
				}
				for (int i = 1; i <= this.LastPage; i++)
				{
					if (!this.loadedPages.Contains(i))
					{
						return false;
					}
				}
				return true;
			}
		}

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x060009CD RID: 2509 RVA: 0x000299DA File Offset: 0x00027BDA
		public int UnviewedLoadedOrPendingPages
		{
			get
			{
				return Mathf.Max(this.loadedPages.Count + this.pendingPages.Count - this.viewedPages.Count, 0);
			}
		}

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x060009CE RID: 2510
		protected abstract T SkeletonData { get; }

		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x060009CF RID: 2511 RVA: 0x000043C6 File Offset: 0x000025C6
		protected virtual bool PopulateRefs
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x060009D0 RID: 2512 RVA: 0x00029A05 File Offset: 0x00027C05
		protected string SortQuery
		{
			get
			{
				return string.Format("order_by: {{ Name: {0} }}", base.SortOrder);
			}
		}

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x060009D1 RID: 2513 RVA: 0x00029A1C File Offset: 0x00027C1C
		private int NextUnloadedPage
		{
			get
			{
				if (!this.IsInitialized)
				{
					return -1;
				}
				for (int i = 1; i <= this.LastPage; i++)
				{
					if (!this.loadedPages.Contains(i) && !this.pendingPages.Contains(i) && !this.pageRequests.Contains(i))
					{
						return i;
					}
				}
				return -1;
			}
		}

		// Token: 0x060009D2 RID: 2514 RVA: 0x00029A74 File Offset: 0x00027C74
		public override async Task RequestAsync(Action requestSuccessAction)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("RequestAsync ( requestSuccessAction: " + requestSuccessAction.DebugIsNull() + " )", this);
			}
			if (base.gameObject.activeInHierarchy && !this.AllPagesLoaded && !this.IsInitialized)
			{
				IEnumerable<int> enumerable = ((this.preloadStrategy == UIBaseCloudPaginatedListModel<T>.PreloadStrategy.InfiniteScroll) ? Enumerable.Repeat<int>(1, 1) : this.Range(1, this.initialPagesToLoad));
				this.RequestPages(enumerable);
				await base.RequestAsync(requestSuccessAction);
			}
		}

		// Token: 0x060009D3 RID: 2515 RVA: 0x00029AC0 File Offset: 0x00027CC0
		public override void Clear(bool triggerEvents)
		{
			base.Clear(triggerEvents);
			this.pageRequests.Clear();
			this.pendingPages.Clear();
			this.loadedPages.Clear();
			this.viewedPages.Clear();
			this.viewedItemIndexes.Clear();
			this.loadedItemIndexes.Clear();
			this.lastViewedItemIndex = 0;
			this.lastViewedPage = 1;
			if (base.RequestInProgress)
			{
				CancellationTokenSource cancellationTokenSource = this.requestPagesAsyncCancellationTokenSource;
				if (cancellationTokenSource != null)
				{
					cancellationTokenSource.Cancel();
				}
				this.requestPagesAsyncCancellationTokenSource = null;
				base.RequestInProgress = false;
			}
			this.LastPage = -1;
		}

		// Token: 0x060009D4 RID: 2516 RVA: 0x00029B54 File Offset: 0x00027D54
		public void SetPageSize(int newPageSize)
		{
			if (newPageSize <= 0 || newPageSize == this.PageSize)
			{
				return;
			}
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ➔ {1}", "SetPageSize", newPageSize), this);
			}
			HashSet<int> hashSet = new HashSet<int>(this.loadedItemIndexes);
			HashSet<int> hashSet2 = new HashSet<int>(this.viewedItemIndexes);
			int num = this.lastViewedItemIndex;
			this.PageSize = newPageSize;
			this.LastPage = (this.Count + this.PageSize - 1) / this.PageSize;
			this.pageRequests.Clear();
			this.pendingPages.Clear();
			this.loadedPages.Clear();
			this.viewedPages.Clear();
			foreach (int num2 in hashSet)
			{
				int num3 = num2 / this.PageSize + 1;
				this.loadedPages.Add(num3);
			}
			foreach (int num4 in hashSet2)
			{
				int num5 = num4 / this.PageSize + 1;
				this.viewedPages.Add(num5);
			}
			this.lastViewedPage = num / this.PageSize + 1;
			this.RequestPagesAroundPage(this.lastViewedPage);
		}

		// Token: 0x060009D5 RID: 2517
		protected abstract Task<GraphQlResult> RequestPage(PaginationParams paginationParams, CancellationToken cancellationToken);

		// Token: 0x060009D6 RID: 2518 RVA: 0x00029CB8 File Offset: 0x00027EB8
		protected override void OnRequestSuccess(object result)
		{
			base.OnRequestSuccess(result);
			UIPageRequestResult<T> uipageRequestResult = result as UIPageRequestResult<T>;
			while (this.Count < uipageRequestResult.Pagination.Total)
			{
				this.Add(this.SkeletonData, false);
			}
			while (this.Count > uipageRequestResult.Pagination.Total)
			{
				this.RemoveAt(this.Count - 1, false);
			}
			if (base.SuperVerboseLogging)
			{
				DebugUtility.DebugEnumerable<T>("Items", uipageRequestResult.Items, this);
			}
			if (uipageRequestResult.Pagination.Total > 0)
			{
				int num = 0;
				for (int i = uipageRequestResult.Pagination.From - 1; i <= uipageRequestResult.Pagination.To - 1; i++)
				{
					if (base.SuperVerboseLogging)
					{
						DebugUtility.Log(string.Format("{0}: {1}, {2}: {3} -> {4}", new object[]
						{
							"i",
							i,
							"index",
							num,
							uipageRequestResult.Items[num]
						}), this);
					}
					this.SetItem(i, uipageRequestResult.Items[num], false);
					this.loadedItemIndexes.Add(i);
					num++;
				}
			}
			base.TriggerModelChanged();
			this.RequestNextChunkOfPages();
		}

		// Token: 0x060009D7 RID: 2519 RVA: 0x00029DF1 File Offset: 0x00027FF1
		protected virtual UIPageRequestResult<T> ParseJson(string json)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ParseJson ( json: " + json + " )", this);
			}
			return UIPageRequestResult<T>.Parse(json);
		}

		// Token: 0x060009D8 RID: 2520 RVA: 0x00029E17 File Offset: 0x00028017
		private IEnumerable<int> Range(int startInclusive, int count)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "Range", "startInclusive", startInclusive, "count", count }), this);
			}
			int num;
			for (int i = 0; i < count; i = num + 1)
			{
				yield return startInclusive + i;
				num = i;
			}
			yield break;
		}

		// Token: 0x060009D9 RID: 2521 RVA: 0x00029E38 File Offset: 0x00028038
		private void RequestPages(IEnumerable<int> pages)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "RequestPages", "pages", pages), this);
			}
			foreach (int num in pages)
			{
				if (num >= 1 && !this.loadedPages.Contains(num) && this.pendingPages.Add(num))
				{
					this.pageRequests.Add(num);
				}
			}
			if (!base.RequestInProgress)
			{
				this.RequestNextChunkOfPages();
			}
		}

		// Token: 0x060009DA RID: 2522 RVA: 0x00029ED8 File Offset: 0x000280D8
		private void RequestNextChunkOfPages()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("RequestNextChunkOfPages", this);
			}
			if (base.RequestInProgress || this.pageRequests.Count == 0)
			{
				return;
			}
			int min = this.pageRequests.Min;
			this.pageRequests.Remove(min);
			int num = 1;
			for (int i = 1; i < this.pagesToLoadPerRequest; i++)
			{
				int num2 = min + i;
				if (!this.pageRequests.Contains(num2))
				{
					break;
				}
				this.pageRequests.Remove(num2);
				num++;
			}
			CancellationTokenSource cancellationTokenSource = this.requestPagesAsyncCancellationTokenSource;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			CancellationTokenSource cancellationTokenSource2 = this.requestPagesAsyncCancellationTokenSource;
			if (cancellationTokenSource2 != null)
			{
				cancellationTokenSource2.Dispose();
			}
			this.requestPagesAsyncCancellationTokenSource = new CancellationTokenSource();
			base.RequestInProgress = true;
			this.RequestPagesAsync(min, num, this.requestPagesAsyncCancellationTokenSource.Token);
		}

		// Token: 0x060009DB RID: 2523 RVA: 0x00029FA8 File Offset: 0x000281A8
		private async Task RequestPagesAsync(int page, int pagesToRequest, CancellationToken cancellationToken)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "RequestPagesAsync", "page", page, "pagesToRequest", pagesToRequest, "cancellationToken", cancellationToken }), this);
			}
			if (pagesToRequest < 1)
			{
				pagesToRequest = 1;
				DebugUtility.LogWarning("pagesToRequest must be at least 1!", this);
			}
			UIPageRequestResult<T> pageRequestResult = null;
			bool success = false;
			try
			{
				if (this.Count == 0)
				{
					base.OnLoadingStarted.Invoke();
				}
				int num = (page - 1) * this.PageSize;
				int num2 = this.PageSize * pagesToRequest;
				PaginationParams paginationParams = new PaginationParams(Pagination.QueryString, num, num2);
				if (base.VerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", "paginationParams", paginationParams), this);
				}
				GraphQlResult graphQlResult = await this.RequestPage(paginationParams, cancellationToken);
				if (cancellationToken.IsCancellationRequested)
				{
					DebugUtility.LogWarning(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} ) | canceled!", new object[] { "RequestPagesAsync", "page", page, "pagesToRequest", pagesToRequest, "cancellationToken", cancellationToken }), this);
					return;
				}
				if (graphQlResult.HasErrors)
				{
					this.OnRequestFailure(graphQlResult.GetErrorMessage(0));
					return;
				}
				string text = graphQlResult.GetDataMember().ToString();
				pageRequestResult = this.ParseJson(text);
				if (base.SuperVerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", "pageRequestResult", pageRequestResult), this);
				}
				if (pageRequestResult == null)
				{
					DebugUtility.LogError("pageRequestResult is null", this);
				}
				Pagination pagination = pageRequestResult.Pagination;
				if (pagination == null)
				{
					DebugUtility.LogError("pagination is null", this);
				}
				int total = pagination.Total;
				this.LastPage = (total + this.PageSize - 1) / this.PageSize;
				while (this.Count < total)
				{
					this.Add(this.SkeletonData, false);
				}
				if (this.Count > total)
				{
					while (this.Count > total)
					{
						this.RemoveAt(0, false);
						this.loadedPages.RemoveWhere((int p) => p > this.LastPage);
						this.pendingPages.RemoveWhere((int p) => p > this.LastPage);
					}
				}
				if (pageRequestResult.Items.Length != 0)
				{
					int num3 = pagination.From - 1;
					int num4 = pagination.To - 1;
					int num5 = 0;
					List<int> list = new List<int>();
					for (int i = num3; i <= num4; i++)
					{
						this.SetItem(i, pageRequestResult.Items[num5++], false);
						int num6 = i / this.PageSize + 1;
						if (!list.Contains(num6))
						{
							list.Add(num6);
						}
					}
					foreach (int num7 in list)
					{
						this.MarkPageAsLoaded(num7);
					}
				}
				success = true;
			}
			catch (Exception ex)
			{
				this.OnRequestFailure(ex);
			}
			finally
			{
				base.OnLoadingEnded.Invoke();
				base.RequestInProgress = false;
				if (this.requestPagesAsyncCancellationTokenSource.Token == cancellationToken)
				{
					this.requestPagesAsyncCancellationTokenSource.Dispose();
					this.requestPagesAsyncCancellationTokenSource = null;
				}
			}
			if (success)
			{
				this.OnRequestSuccess(pageRequestResult);
			}
		}

		// Token: 0x060009DC RID: 2524 RVA: 0x0002A004 File Offset: 0x00028204
		private void MaybePreloadMore()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("MaybePreloadMore", this);
			}
			if (!this.IsInitialized || this.preloadStrategy != UIBaseCloudPaginatedListModel<T>.PreloadStrategy.ButtonPagination || this.AllPagesLoaded || base.RequestInProgress)
			{
				return;
			}
			if (this.UnviewedLoadedOrPendingPages < this.pageSurplusThreshold)
			{
				int nextUnloadedPage = this.NextUnloadedPage;
				if (nextUnloadedPage > 0)
				{
					this.RequestPages(this.Range(nextUnloadedPage, this.initialPagesToLoad));
				}
			}
		}

		// Token: 0x060009DD RID: 2525 RVA: 0x0002A074 File Offset: 0x00028274
		private void RequestPagesAroundPage(int targetPage)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "RequestPagesAroundPage", "targetPage", targetPage), this);
			}
			if (!this.IsInitialized)
			{
				return;
			}
			HashSet<int> hashSet = new HashSet<int>();
			UIBaseCloudPaginatedListModel<T>.PreloadStrategy preloadStrategy = this.preloadStrategy;
			if (preloadStrategy != UIBaseCloudPaginatedListModel<T>.PreloadStrategy.ButtonPagination)
			{
				if (preloadStrategy != UIBaseCloudPaginatedListModel<T>.PreloadStrategy.InfiniteScroll)
				{
					throw new ArgumentOutOfRangeException();
				}
				int num = this.requestAroundPageDistanceLimit / 2;
				for (int i = -num; i <= num; i++)
				{
					int num2 = targetPage + i;
					if (this.CanBeRequested(num2))
					{
						hashSet.Add(num2);
					}
				}
			}
			else
			{
				int num3 = targetPage - 1;
				if (num3 >= 1 && this.CanBeRequested(num3))
				{
					hashSet.Add(num3);
				}
				int num4 = targetPage + 1;
				int num5 = num4 - targetPage;
				while (num4 <= this.PageCount && hashSet.Count < this.pagesToLoadPerRequest && num5 <= this.requestAroundPageDistanceLimit)
				{
					if (this.CanBeRequested(num4))
					{
						hashSet.Add(num4);
					}
					num4++;
					num5 = num4 - targetPage;
				}
				num3--;
				num5 = targetPage - num3;
				while (num3 >= 1 && hashSet.Count < this.pagesToLoadPerRequest)
				{
					if (num5 > this.requestAroundPageDistanceLimit)
					{
						break;
					}
					if (this.CanBeRequested(num3))
					{
						hashSet.Add(num3);
					}
					num3--;
					num5 = targetPage - num3;
				}
			}
			if (hashSet.Count > 0)
			{
				this.RequestPages(hashSet);
			}
		}

		// Token: 0x060009DE RID: 2526 RVA: 0x0002A1BC File Offset: 0x000283BC
		private void MarkPageAsLoaded(int page)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "MarkPageAsLoaded", "page", page), this);
			}
			this.pendingPages.Remove(page);
			this.loadedPages.Add(page);
		}

		// Token: 0x060009DF RID: 2527 RVA: 0x0002A20C File Offset: 0x0002840C
		private bool CanBeRequested(int page)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "CanBeRequested", "page", page), this);
			}
			return this.PageIsInRange(page) && !this.loadedPages.Contains(page) && !this.pendingPages.Contains(page) && !this.pageRequests.Contains(page);
		}

		// Token: 0x060009E0 RID: 2528 RVA: 0x0002A278 File Offset: 0x00028478
		private bool PageIsInRange(int page)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "PageIsInRange", "page", page), this);
			}
			if (!this.IsInitialized)
			{
				DebugUtility.LogWarning(string.Format("{0} ( {1}: {2} ) | {3} is false!", new object[] { "PageIsInRange", "page", page, "IsInitialized" }), this);
				return false;
			}
			return page > 0 && page <= this.LastPage;
		}

		// Token: 0x04000623 RID: 1571
		[Header("UIBaseCloudPaginatedListModel")]
		[Header("Pre‑loading")]
		[SerializeField]
		private UIBaseCloudPaginatedListModel<T>.PreloadStrategy preloadStrategy;

		// Token: 0x04000624 RID: 1572
		[SerializeField]
		[Min(1f)]
		private int initialPagesToLoad = 3;

		// Token: 0x04000625 RID: 1573
		[SerializeField]
		[Min(0f)]
		private int pageSurplusThreshold = 2;

		// Token: 0x04000626 RID: 1574
		[SerializeField]
		private int pagesToLoadPerRequest = 3;

		// Token: 0x04000627 RID: 1575
		[SerializeField]
		private int requestAroundPageDistanceLimit = 4;

		// Token: 0x04000628 RID: 1576
		private readonly SortedSet<int> pageRequests = new SortedSet<int>();

		// Token: 0x04000629 RID: 1577
		private readonly HashSet<int> pendingPages = new HashSet<int>();

		// Token: 0x0400062A RID: 1578
		private readonly HashSet<int> loadedPages = new HashSet<int>();

		// Token: 0x0400062B RID: 1579
		private readonly HashSet<int> viewedPages = new HashSet<int>();

		// Token: 0x0400062C RID: 1580
		private readonly HashSet<int> viewedItemIndexes = new HashSet<int>();

		// Token: 0x0400062D RID: 1581
		private readonly HashSet<int> loadedItemIndexes = new HashSet<int>();

		// Token: 0x0400062E RID: 1582
		private readonly int[] immediatePageRequest = new int[1];

		// Token: 0x0400062F RID: 1583
		private int lastViewedItemIndex;

		// Token: 0x04000630 RID: 1584
		private int lastViewedPage;

		// Token: 0x04000631 RID: 1585
		private CancellationTokenSource requestPagesAsyncCancellationTokenSource;

		// Token: 0x0200018F RID: 399
		private enum PreloadStrategy
		{
			// Token: 0x04000635 RID: 1589
			ButtonPagination,
			// Token: 0x04000636 RID: 1590
			InfiniteScroll
		}
	}
}
