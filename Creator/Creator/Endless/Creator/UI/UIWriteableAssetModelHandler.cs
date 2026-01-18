using System;
using Endless.Assets;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared.Debugging;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;

namespace Endless.Creator.UI
{
	// Token: 0x020000A0 RID: 160
	public abstract class UIWriteableAssetModelHandler<T> : UIAssetModelHandler<T> where T : Asset
	{
		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600028C RID: 652
		protected abstract ErrorCodes OnUpdateAssetFailErrorCode { get; }

		// Token: 0x0600028D RID: 653 RVA: 0x0001177C File Offset: 0x0000F97C
		public async void Upload()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Upload", this);
			}
			base.OnLoadingStarted.Invoke();
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(base.Model, false, false);
			base.OnLoadingEnded.Invoke();
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(this.OnUpdateAssetFailErrorCode, graphQlResult.GetErrorMessage(0), true, false);
			}
			else
			{
				base.Set(JsonConvert.DeserializeObject<T>(graphQlResult.GetDataMember().ToString()));
			}
		}
	}
}
