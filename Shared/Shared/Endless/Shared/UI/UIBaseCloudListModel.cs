using System;
using System.Threading.Tasks;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000194 RID: 404
	public abstract class UIBaseCloudListModel<T> : UIBaseListModel<T>, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x060009F9 RID: 2553 RVA: 0x0002ABE8 File Offset: 0x00028DE8
		// (set) Token: 0x060009FA RID: 2554 RVA: 0x0002ABF0 File Offset: 0x00028DF0
		private protected bool RequestAgainOnResult { protected get; private set; }

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x060009FB RID: 2555 RVA: 0x0002ABF9 File Offset: 0x00028DF9
		// (set) Token: 0x060009FC RID: 2556 RVA: 0x0002AC01 File Offset: 0x00028E01
		protected bool RequestInProgress { get; set; }

		// Token: 0x060009FD RID: 2557 RVA: 0x0002AC0A File Offset: 0x00028E0A
		protected virtual void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			MatchmakingClientController.OnDisconnectedFromServer += this.OnDisconnectedFromServer;
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x0002AC30 File Offset: 0x00028E30
		protected virtual void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDestroy", this);
			}
			MatchmakingClientController.OnDisconnectedFromServer -= this.OnDisconnectedFromServer;
		}

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x060009FF RID: 2559 RVA: 0x0002AC56 File Offset: 0x00028E56
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x06000A00 RID: 2560 RVA: 0x0002AC5E File Offset: 0x00028E5E
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000A01 RID: 2561 RVA: 0x0002AC68 File Offset: 0x00028E68
		public void Request(Action requestSuccessAction)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} ) | {3}: {4}, {5}: {6}", new object[]
				{
					"Request",
					"requestSuccessAction",
					requestSuccessAction.DebugIsNull(),
					"RequestInProgress",
					this.RequestInProgress,
					"RequestAgainOnResult",
					this.RequestAgainOnResult
				}), this);
			}
			this.RequestAsync(requestSuccessAction);
		}

		// Token: 0x06000A02 RID: 2562 RVA: 0x0002ACE0 File Offset: 0x00028EE0
		public virtual async Task RequestAsync(Action requestSuccessAction)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} ) | {3}: {4}, {5}: {6}", new object[]
				{
					"Request",
					"requestSuccessAction",
					requestSuccessAction.DebugIsNull(),
					"RequestInProgress",
					this.RequestInProgress,
					"RequestAgainOnResult",
					this.RequestAgainOnResult
				}), this);
			}
			if (this.RequestInProgress)
			{
				this.RequestAgainOnResult = true;
			}
			else
			{
				this.OnLoadingStarted.Invoke();
				this.RequestSuccessAction = requestSuccessAction;
				this.RequestInProgress = true;
				await Task.CompletedTask;
			}
		}

		// Token: 0x06000A03 RID: 2563 RVA: 0x0002AD2B File Offset: 0x00028F2B
		public override void SetSortOrder(SortOrders value)
		{
			base.SetSortOrder(value);
			this.Clear(true);
			this.Request(null);
		}

		// Token: 0x06000A04 RID: 2564 RVA: 0x0002AD42 File Offset: 0x00028F42
		public void SetStringFilter(string value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetStringFilter ( value: " + value + " )", this);
			}
			this.StringFilter = value;
		}

		// Token: 0x06000A05 RID: 2565
		protected abstract void HandleError(Exception exception);

		// Token: 0x06000A06 RID: 2566 RVA: 0x0002AD69 File Offset: 0x00028F69
		protected virtual void OnRequestSuccess(object result)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnRequestSuccess", "result", result), this);
			}
			this.OnLoadingEnded.Invoke();
			this.HandleRequestSuccess();
		}

		// Token: 0x06000A07 RID: 2567 RVA: 0x0002ADA0 File Offset: 0x00028FA0
		protected void HandleRequestSuccess()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("HandleRequestSuccess", this);
			}
			this.RequestInProgress = false;
			Action requestSuccessAction = this.RequestSuccessAction;
			if (requestSuccessAction != null)
			{
				requestSuccessAction();
			}
			this.RequestSuccessAction = null;
			if (!this.RequestAgainOnResult)
			{
				return;
			}
			this.RequestAgainOnResult = false;
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x0002ADF0 File Offset: 0x00028FF0
		protected virtual void OnRequestFailure(Exception exception)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnRequestFailure", "exception", exception), this);
			}
			this.OnLoadingEnded.Invoke();
			this.HandleError(exception);
			this.RequestInProgress = false;
			this.RequestAgainOnResult = false;
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x0002AE40 File Offset: 0x00029040
		private void OnDisconnectedFromServer(string disconnectedFromServerReason)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDisconnectedFromServer ( disconnectedFromServerReason: " + disconnectedFromServerReason + " )", this);
			}
			this.Clear(true);
		}

		// Token: 0x04000650 RID: 1616
		protected string StringFilter;

		// Token: 0x04000651 RID: 1617
		protected Action RequestSuccessAction;
	}
}
