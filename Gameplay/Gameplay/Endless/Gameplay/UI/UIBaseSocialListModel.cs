using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared.Debugging;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003EF RID: 1007
	public abstract class UIBaseSocialListModel<T>
	{
		// Token: 0x17000523 RID: 1315
		// (get) Token: 0x06001933 RID: 6451 RVA: 0x00074791 File Offset: 0x00072991
		public IReadOnlyList<T> Items
		{
			get
			{
				return this.items;
			}
		}

		// Token: 0x17000524 RID: 1316
		// (get) Token: 0x06001934 RID: 6452
		protected abstract Task<GraphQlResult> RequestListTask { get; }

		// Token: 0x17000525 RID: 1317
		// (get) Token: 0x06001935 RID: 6453
		protected abstract ErrorCodes GetDataErrorCode { get; }

		// Token: 0x14000036 RID: 54
		// (add) Token: 0x06001936 RID: 6454 RVA: 0x0007479C File Offset: 0x0007299C
		// (remove) Token: 0x06001937 RID: 6455 RVA: 0x000747D4 File Offset: 0x000729D4
		public event Action OnModelChanged;

		// Token: 0x06001938 RID: 6456 RVA: 0x0007480C File Offset: 0x00072A0C
		public async Task<List<T>> RequestListAsync()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("RequestListAsync", null);
			}
			List<T> list;
			if (this.isLoading)
			{
				list = new List<T>();
			}
			else
			{
				this.isLoading = true;
				GraphQlResult graphQlResult = await this.RequestListTask;
				if (graphQlResult.HasErrors)
				{
					Exception errorMessage = graphQlResult.GetErrorMessage(0);
					ErrorHandler.HandleError(this.GetDataErrorCode, errorMessage, true, false);
					list = new List<T>();
				}
				else
				{
					T[] array = this.ExtractData(graphQlResult);
					this.AddExtractedData(array);
					this.isLoading = false;
					this.OnLoadComplete();
					list = this.items;
				}
			}
			return list;
		}

		// Token: 0x06001939 RID: 6457 RVA: 0x0007484F File Offset: 0x00072A4F
		protected virtual void OnLoadComplete()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnLoadComplete", null);
			}
			this.InvokeOnModelChanged();
		}

		// Token: 0x0600193A RID: 6458 RVA: 0x0007486A File Offset: 0x00072A6A
		protected virtual void AddExtractedData(T[] range)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "AddExtractedData", "range", range.Length), null);
			}
			this.items.AddRange(range);
		}

		// Token: 0x0600193B RID: 6459
		protected abstract T[] ExtractData(GraphQlResult graphQlResult);

		// Token: 0x0600193C RID: 6460 RVA: 0x000748A2 File Offset: 0x00072AA2
		public void Add(T item)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Add", "item", item), null);
			}
			this.items.Add(item);
			this.InvokeOnModelChanged();
		}

		// Token: 0x0600193D RID: 6461 RVA: 0x000748E0 File Offset: 0x00072AE0
		public void Remove(T item)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Remove", "item", item), null);
			}
			if (this.items.Contains(item))
			{
				this.items.Remove(item);
				this.InvokeOnModelChanged();
				return;
			}
			DebugUtility.LogWarning(string.Format("{0} ( {1}: {2} ) | {3} does not contain {4}", new object[] { "Remove", "item", item, "items", "item" }), null);
		}

		// Token: 0x0600193E RID: 6462 RVA: 0x00074976 File Offset: 0x00072B76
		public virtual void Clear()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Clear", null);
			}
			this.items.Clear();
		}

		// Token: 0x0600193F RID: 6463
		public abstract bool RemoveItemWithId(int itemId);

		// Token: 0x06001940 RID: 6464 RVA: 0x00074996 File Offset: 0x00072B96
		protected void InvokeOnModelChanged()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("InvokeOnModelChanged", null);
			}
			Action onModelChanged = this.OnModelChanged;
			if (onModelChanged == null)
			{
				return;
			}
			onModelChanged();
		}

		// Token: 0x04001429 RID: 5161
		protected readonly List<T> items = new List<T>();

		// Token: 0x0400142A RID: 5162
		protected bool isLoading;

		// Token: 0x0400142B RID: 5163
		public bool VerboseLogging;
	}
}
