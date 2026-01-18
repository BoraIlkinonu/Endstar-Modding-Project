using Endless.Assets;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public abstract class UIAssetModelHandler<T> : UIGameObject, IUILoadingSpinnerViewCompatible where T : Asset
{
	protected bool VerboseLogging { get; set; }

	public UnityEvent<T> OnSet { get; } = new UnityEvent<T>();

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public T Model { get; private set; }

	protected abstract ErrorCodes OnGetAssetFailErrorCode { get; }

	public virtual void Clear()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Clear", this);
		}
		Model = null;
	}

	public void Set(T newValue)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Set", "newValue", newValue), this);
		}
		Model = newValue;
		OnSet.Invoke(Model);
	}

	public async void GetAsset()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("GetAsset", this);
		}
		OnLoadingStarted.Invoke();
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(Model.AssetID);
		OnLoadingEnded.Invoke();
		if (graphQlResult.HasErrors)
		{
			ErrorHandler.HandleError(OnGetAssetFailErrorCode, graphQlResult.GetErrorMessage());
			return;
		}
		T newValue = JsonConvert.DeserializeObject<T>(graphQlResult.GetDataMember().ToString());
		Set(newValue);
	}
}
