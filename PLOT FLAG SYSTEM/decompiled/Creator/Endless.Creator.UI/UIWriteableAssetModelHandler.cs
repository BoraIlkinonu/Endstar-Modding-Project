using Endless.Assets;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared.Debugging;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;

namespace Endless.Creator.UI;

public abstract class UIWriteableAssetModelHandler<T> : UIAssetModelHandler<T> where T : Asset
{
	protected abstract ErrorCodes OnUpdateAssetFailErrorCode { get; }

	public async void Upload()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Upload", this);
		}
		base.OnLoadingStarted.Invoke();
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(base.Model);
		base.OnLoadingEnded.Invoke();
		if (graphQlResult.HasErrors)
		{
			ErrorHandler.HandleError(OnUpdateAssetFailErrorCode, graphQlResult.GetErrorMessage());
			return;
		}
		T newValue = JsonConvert.DeserializeObject<T>(graphQlResult.GetDataMember().ToString());
		Set(newValue);
	}
}
