using System;
using System.Linq;
using Endless.Assets;
using Endless.Data;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIRevisionListModel : UIBaseLocalFilterableListModel<string>, IUILoadingSpinnerViewCompatible
{
	private ParsedAssetRevision parsedAssetRevision;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	protected override Comparison<string> DefaultSort => (string x, string y) => string.Compare(x, y, StringComparison.Ordinal);

	private void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		OnLoadingEnded.Invoke();
		parsedAssetRevision = null;
	}

	public void Initialize(Asset asset, string version)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", asset.AssetID, version);
		}
		OnLoadingStarted.Invoke();
		parsedAssetRevision = new ParsedAssetRevision(asset.AssetID, version, OnRequestSuccess, OnRequestError);
	}

	private void OnRequestSuccess(string[] result)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnRequestSuccess", StringUtility.CommaSeparate(result));
		}
		OnLoadingEnded.Invoke();
		Set(result.ToList(), triggerEvents: true);
	}

	private void OnRequestError(Exception exception)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnRequestError", exception.Message);
		}
		OnLoadingEnded.Invoke();
		ErrorHandler.HandleError(ErrorCodes.UIRevisionListModel_RetrievingRevision, exception);
	}
}
