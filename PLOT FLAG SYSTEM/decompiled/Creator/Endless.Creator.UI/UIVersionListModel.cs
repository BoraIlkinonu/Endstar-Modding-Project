using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Data;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json.Linq;
using Runtime.Shared.Matchmaking;

namespace Endless.Creator.UI;

public class UIVersionListModel : UIBaseCloudListModel<string>
{
	private const string REVISION_META_DATA_KEY = "revision_meta_data";

	private readonly Dictionary<string, DateTime> versionTimestampCache = new Dictionary<string, DateTime>();

	private readonly Dictionary<string, CancellationTokenSource> timestampRequestTokens = new Dictionary<string, CancellationTokenSource>();

	private SerializableGuid assetId = SerializableGuid.Empty;

	private CancellationTokenSource requestCancellationTokenSource;

	public void Initialize(SerializableGuid assetId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", assetId);
		}
		if (this.assetId != assetId)
		{
			CancelAllOperations();
			versionTimestampCache.Clear();
			if (Count > 0)
			{
				Clear(triggerEvents: true);
			}
		}
		this.assetId = assetId;
		RequestAsync(null);
	}

	public override async Task RequestAsync(Action requestSuccessAction)
	{
		requestCancellationTokenSource?.Cancel();
		requestCancellationTokenSource?.Dispose();
		requestCancellationTokenSource = new CancellationTokenSource();
		CancellationToken cancellationToken = requestCancellationTokenSource.Token;
		try
		{
			await base.RequestAsync(requestSuccessAction);
			if (assetId.IsEmpty)
			{
				DebugUtility.LogException(new Exception("assetId is empty!"), this);
				return;
			}
			cancellationToken.ThrowIfCancellationRequested();
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(assetId);
			cancellationToken.ThrowIfCancellationRequested();
			if (graphQlResult.HasErrors)
			{
				OnRequestFailure(graphQlResult.GetErrorMessage());
				return;
			}
			object dataMember = graphQlResult.GetDataMember();
			OnRequestSuccess(dataMember);
		}
		catch (OperationCanceledException)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("RequestAsync was cancelled", this);
			}
			base.OnLoadingEnded?.Invoke();
		}
		catch (Exception exception)
		{
			HandleError(exception);
		}
	}

	public async Task GetTimestampAsync(string version, Action<string, DateTime> callback)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetTimestampAsync", version, (callback == null) ? "null" : "NOT null");
		}
		if (versionTimestampCache.TryGetValue(version, out var value))
		{
			callback?.Invoke(version, value);
			return;
		}
		if (timestampRequestTokens.TryGetValue(version, out var value2))
		{
			value2.Cancel();
			value2.Dispose();
			timestampRequestTokens.Remove(version);
		}
		CancellationTokenSource timestampRequestCancellationSource = new CancellationTokenSource();
		timestampRequestTokens[version] = timestampRequestCancellationSource;
		CancellationToken cancellationToken = timestampRequestCancellationSource.Token;
		try
		{
			AssetParams assetParams = new AssetParams("revision_meta_data");
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(assetId, version, assetParams, base.VerboseLogging);
			cancellationToken.ThrowIfCancellationRequested();
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UIVersionListModel_GetAssetForTimestamp, graphQlResult.GetErrorMessage());
				return;
			}
			JToken jToken = (graphQlResult.GetDataMember() as JObject)?["revision_meta_data"];
			if (jToken == null)
			{
				DebugUtility.LogException(new KeyNotFoundException("The key for REVISION_META_DATA_KEY 'revision_meta_data' could not be found in jObject"), this);
				return;
			}
			RevisionMetaData revisionMetaData = jToken.ToObject<RevisionMetaData>();
			DateTime dateTime = new DateTime(revisionMetaData.RevisionTimestamp).ToLocalTime();
			versionTimestampCache[version] = dateTime;
			callback?.Invoke(version, dateTime);
		}
		catch (OperationCanceledException)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("GetTimestampAsync was cancelled for version: " + version, this);
			}
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.UIVersionListModel_GetAssetForTimestamp, exception);
		}
		finally
		{
			if (timestampRequestTokens.TryGetValue(version, out var value3) && value3 == timestampRequestCancellationSource)
			{
				timestampRequestTokens.Remove(version);
				timestampRequestCancellationSource.Dispose();
			}
		}
	}

	public void CancelAllOperations()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CancelAllOperations");
		}
		requestCancellationTokenSource?.Cancel();
		requestCancellationTokenSource?.Dispose();
		requestCancellationTokenSource = null;
		foreach (CancellationTokenSource value in timestampRequestTokens.Values)
		{
			value.Cancel();
			value.Dispose();
		}
		timestampRequestTokens.Clear();
	}

	protected override void OnRequestSuccess(object result)
	{
		base.OnRequestSuccess(result);
		string[] parsedAndOrderedVersions = VersionUtilities.GetParsedAndOrderedVersions(result);
		Set(new List<string>(parsedAndOrderedVersions), triggerEvents: false);
		Select(0, triggerEvents: true);
	}

	protected override void HandleError(Exception exception)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleError", exception.Message);
		}
		base.OnLoadingEnded.Invoke();
		ErrorHandler.HandleError(ErrorCodes.UIVersionListModel_RetrievingAssetVersions, exception);
	}
}
