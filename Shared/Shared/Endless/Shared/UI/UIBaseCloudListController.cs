using System;
using System.Threading;
using System.Threading.Tasks;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200017F RID: 383
	public abstract class UIBaseCloudListController<T> : UIBaseFilterableListController<T>
	{
		// Token: 0x06000978 RID: 2424 RVA: 0x0002889C File Offset: 0x00026A9C
		public override void Validate()
		{
			base.Validate();
			if (this.IgnoreValidation)
			{
				return;
			}
			if (DebugUtility.DebugIsNull("cloudListModel", this.cloudListModel, this))
			{
				return;
			}
			if (base.Model != this.cloudListModel)
			{
				DebugUtility.LogError("Model needs to be the same object as cloudListModel!", this);
			}
		}

		// Token: 0x06000979 RID: 2425 RVA: 0x000288EC File Offset: 0x00026AEC
		protected override void SetStringFilter(string toFilterBy)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetStringFilter ( toFilterBy: " + toFilterBy + " )", this);
			}
			this.cancelTokenSource.Cancel();
			this.cancelTokenSource = new CancellationTokenSource();
			this.cloudListModel.SetStringFilter(toFilterBy);
			this.DelaySynchronize(this.cancelTokenSource.Token);
		}

		// Token: 0x0600097A RID: 2426 RVA: 0x0002894C File Offset: 0x00026B4C
		private async Task DelaySynchronize(CancellationToken cancelToken)
		{
			await Task.Delay(500);
			if (!cancelToken.IsCancellationRequested)
			{
				this.Synchronize();
			}
		}

		// Token: 0x0600097B RID: 2427 RVA: 0x00028997 File Offset: 0x00026B97
		protected override void Synchronize()
		{
			base.Synchronize();
			this.cloudListModel.Clear(false);
			this.cloudListModel.Request(null);
		}

		// Token: 0x040005F8 RID: 1528
		[SerializeField]
		private UIBaseCloudListModel<T> cloudListModel;

		// Token: 0x040005F9 RID: 1529
		private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
	}
}
