using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001EB RID: 491
	public abstract class UIBaseGraphQlIEnumerableHandler<TItem> : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000238 RID: 568
		// (get) Token: 0x06000C1F RID: 3103 RVA: 0x000345D7 File Offset: 0x000327D7
		public IReadOnlyList<TItem> Items
		{
			get
			{
				return this.items;
			}
		}

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x06000C20 RID: 3104 RVA: 0x000345DF File Offset: 0x000327DF
		// (set) Token: 0x06000C21 RID: 3105 RVA: 0x000345E7 File Offset: 0x000327E7
		public bool IsLoading { get; private set; }

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x06000C22 RID: 3106 RVA: 0x000345F0 File Offset: 0x000327F0
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x06000C23 RID: 3107 RVA: 0x000345F8 File Offset: 0x000327F8
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000C24 RID: 3108 RVA: 0x00034600 File Offset: 0x00032800
		public void Request()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Request: Initiating debounced request.", this);
			}
			CancellationTokenSource cancellationTokenSource = this.debounceCancellationTokenSource;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			CancellationTokenSource cancellationTokenSource2 = this.debounceCancellationTokenSource;
			if (cancellationTokenSource2 != null)
			{
				cancellationTokenSource2.Dispose();
			}
			this.debounceCancellationTokenSource = new CancellationTokenSource();
			this.DebouncedRequestAsync(this.debounceCancellationTokenSource.Token);
		}

		// Token: 0x06000C25 RID: 3109 RVA: 0x00034660 File Offset: 0x00032860
		public void CancelRequest()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("CancelRequest", this);
			}
			CancellationTokenSource cancellationTokenSource = this.requestCancellationTokenSource;
			if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
			{
				return;
			}
			this.requestCancellationTokenSource.Cancel();
		}

		// Token: 0x06000C26 RID: 3110 RVA: 0x000346A0 File Offset: 0x000328A0
		public virtual void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			CancellationTokenSource cancellationTokenSource = this.debounceCancellationTokenSource;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			CancellationTokenSource cancellationTokenSource2 = this.debounceCancellationTokenSource;
			if (cancellationTokenSource2 != null)
			{
				cancellationTokenSource2.Dispose();
			}
			this.debounceCancellationTokenSource = null;
			this.CancelRequest();
			this.items.Clear();
			this.iEnumerablePresenter.Clear();
		}

		// Token: 0x06000C27 RID: 3111 RVA: 0x00034705 File Offset: 0x00032905
		public void SetSelectionType(SelectionType selectionType)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetSelectionType", "selectionType", selectionType), this);
			}
			this.iEnumerablePresenter.SetSelectionType(selectionType, true);
		}

		// Token: 0x06000C28 RID: 3112 RVA: 0x0003473C File Offset: 0x0003293C
		protected virtual void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("OnDestroy", this);
			}
			CancellationTokenSource cancellationTokenSource = this.debounceCancellationTokenSource;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			CancellationTokenSource cancellationTokenSource2 = this.debounceCancellationTokenSource;
			if (cancellationTokenSource2 != null)
			{
				cancellationTokenSource2.Dispose();
			}
			this.CancelRequest();
			CancellationTokenSource cancellationTokenSource3 = this.requestCancellationTokenSource;
			if (cancellationTokenSource3 == null)
			{
				return;
			}
			cancellationTokenSource3.Dispose();
		}

		// Token: 0x06000C29 RID: 3113
		protected abstract Task<GraphQlResult> RequestAsync(CancellationToken cancellationToken);

		// Token: 0x06000C2A RID: 3114 RVA: 0x00034794 File Offset: 0x00032994
		protected virtual void LoadCollectionRequest(CollectionRequest<TItem> collectionRequest)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "LoadCollectionRequest", "collectionRequest", collectionRequest), this);
			}
			this.items.Clear();
			this.items.AddRange(collectionRequest.Data);
			this.iEnumerablePresenter.SetModel(this.items, true);
		}

		// Token: 0x06000C2B RID: 3115 RVA: 0x000347F4 File Offset: 0x000329F4
		private async Task DebouncedRequestAsync(CancellationToken debounceToken)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "DebouncedRequestAsync", "debounceToken", debounceToken), this);
			}
			try
			{
				if (this.requestDebounceTime > 0f)
				{
					await Task.Delay(TimeSpan.FromSeconds((double)this.requestDebounceTime), debounceToken);
				}
				debounceToken.ThrowIfCancellationRequested();
				await this.PerformActualDataFetchAsync();
			}
			catch (OperationCanceledException)
			{
				if (this.verboseLogging)
				{
					DebugUtility.Log("DebouncedRequestAsync: Debounced request was cancelled.", this);
				}
			}
		}

		// Token: 0x06000C2C RID: 3116 RVA: 0x00034840 File Offset: 0x00032A40
		private Task PerformActualDataFetchAsync()
		{
			UIBaseGraphQlIEnumerableHandler<TItem>.<PerformActualDataFetchAsync>d__26 <PerformActualDataFetchAsync>d__;
			<PerformActualDataFetchAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<PerformActualDataFetchAsync>d__.<>4__this = this;
			<PerformActualDataFetchAsync>d__.<>1__state = -1;
			<PerformActualDataFetchAsync>d__.<>t__builder.Start<UIBaseGraphQlIEnumerableHandler<TItem>.<PerformActualDataFetchAsync>d__26>(ref <PerformActualDataFetchAsync>d__);
			return <PerformActualDataFetchAsync>d__.<>t__builder.Task;
		}

		// Token: 0x040007D0 RID: 2000
		[Header("UIBaseGraphQlIEnumerableHandler")]
		[SerializeField]
		protected UIIEnumerablePresenter iEnumerablePresenter;

		// Token: 0x040007D1 RID: 2001
		[Tooltip("Time in seconds to wait after the last Request call before actually fetching data. Set to 0 to disable debouncing.")]
		[SerializeField]
		protected float requestDebounceTime = 0.3f;

		// Token: 0x040007D2 RID: 2002
		[Header("Debugging")]
		[SerializeField]
		protected bool verboseLogging;

		// Token: 0x040007D3 RID: 2003
		protected readonly List<TItem> items = new List<TItem>();

		// Token: 0x040007D4 RID: 2004
		private CancellationTokenSource debounceCancellationTokenSource;

		// Token: 0x040007D5 RID: 2005
		private CancellationTokenSource requestCancellationTokenSource;
	}
}
