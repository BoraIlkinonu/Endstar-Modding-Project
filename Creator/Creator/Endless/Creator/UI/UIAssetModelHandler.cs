using System;
using Endless.Assets;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200009C RID: 156
	public abstract class UIAssetModelHandler<T> : UIGameObject, IUILoadingSpinnerViewCompatible where T : Asset
	{
		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000272 RID: 626 RVA: 0x000112DE File Offset: 0x0000F4DE
		// (set) Token: 0x06000273 RID: 627 RVA: 0x000112E6 File Offset: 0x0000F4E6
		protected bool VerboseLogging { get; set; }

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000274 RID: 628 RVA: 0x000112EF File Offset: 0x0000F4EF
		public UnityEvent<T> OnSet { get; } = new UnityEvent<T>();

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000275 RID: 629 RVA: 0x000112F7 File Offset: 0x0000F4F7
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000276 RID: 630 RVA: 0x000112FF File Offset: 0x0000F4FF
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000277 RID: 631 RVA: 0x00011307 File Offset: 0x0000F507
		// (set) Token: 0x06000278 RID: 632 RVA: 0x0001130F File Offset: 0x0000F50F
		public T Model { get; private set; }

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000279 RID: 633
		protected abstract ErrorCodes OnGetAssetFailErrorCode { get; }

		// Token: 0x0600027A RID: 634 RVA: 0x00011318 File Offset: 0x0000F518
		public virtual void Clear()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			this.Model = default(T);
		}

		// Token: 0x0600027B RID: 635 RVA: 0x00011348 File Offset: 0x0000F548
		public void Set(T newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Set", "newValue", newValue), this);
			}
			this.Model = newValue;
			this.OnSet.Invoke(this.Model);
		}

		// Token: 0x0600027C RID: 636 RVA: 0x00011398 File Offset: 0x0000F598
		public async void GetAsset()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("GetAsset", this);
			}
			this.OnLoadingStarted.Invoke();
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(this.Model.AssetID, "", null, false, 10);
			this.OnLoadingEnded.Invoke();
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(this.OnGetAssetFailErrorCode, graphQlResult.GetErrorMessage(0), true, false);
			}
			else
			{
				this.Set(JsonConvert.DeserializeObject<T>(graphQlResult.GetDataMember().ToString()));
			}
		}
	}
}
