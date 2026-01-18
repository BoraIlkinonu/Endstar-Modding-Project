using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIGameAssetPublishModalModel : IUILoadingSpinnerViewCompatible
{
	private static readonly string betaState = UIPublishStates.Beta.ToEndlessCloudServicesCompatibleString();

	private static readonly string publicState = UIPublishStates.Public.ToEndlessCloudServicesCompatibleString();

	private static readonly string unpublishedState = UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();

	private readonly bool verboseLogging;

	public UIGameAsset GameAsset { get; }

	public string VersionBeta { get; private set; }

	public string VersionPublic { get; private set; }

	public bool IsLoading { get; private set; }

	public UnityEvent<List<string>, List<PublishedVersion>> OnModelChanged { get; } = new UnityEvent<List<string>, List<PublishedVersion>>();

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public UIGameAssetPublishModalModel(UIGameAsset gameAsset)
	{
		GameAsset = gameAsset;
		LoadVersions();
	}

	public void LoadVersions()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "LoadVersions");
		}
		LoadVersionsAsync();
	}

	public void ChangePublishState(string assetId, string version, UIPublishStates targetState)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ChangePublishState", assetId, version, targetState);
		}
		bool needsUnpublish = false;
		string text = string.Empty;
		switch (targetState)
		{
		case UIPublishStates.Beta:
			needsUnpublish = !VersionBeta.IsNullOrEmptyOrWhiteSpace();
			text = VersionBeta;
			break;
		case UIPublishStates.Public:
			needsUnpublish = !VersionPublic.IsNullOrEmptyOrWhiteSpace();
			text = VersionPublic;
			break;
		default:
			throw new ArgumentOutOfRangeException("targetState", targetState, null);
		case UIPublishStates.Unpublished:
			break;
		}
		if (!(text == version))
		{
			ChangePublishStateAsync(assetId, version, targetState, needsUnpublish, text);
		}
	}

	public void Unpublish(UIPublishStates state)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Unpublish", state);
		}
		switch (state)
		{
		case UIPublishStates.Beta:
			UnpublishAsync(VersionBeta);
			break;
		case UIPublishStates.Public:
			UnpublishAsync(VersionPublic);
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		}
	}

	private async Task LoadVersionsAsync()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "LoadVersionsAsync");
		}
		SetLoading(loading: true);
		try
		{
			EndlessCloudService cloudService = EndlessServices.Instance.CloudService;
			SerializableGuid assetId = GameAsset.AssetID;
			Task<GraphQlResult> getVersionsTask = cloudService.GetVersionsAsync(assetId);
			Task<GraphQlResult> getPublishedVersionsOfAssetTask = cloudService.GetPublishedVersionsOfAssetAsync(assetId);
			await Task.WhenAll<GraphQlResult>(getVersionsTask, getPublishedVersionsOfAssetTask);
			GraphQlResult result = getVersionsTask.Result;
			if (result.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalView_RetrievingAssetVersions, result.GetErrorMessage());
				return;
			}
			GraphQlResult result2 = getPublishedVersionsOfAssetTask.Result;
			if (result2.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalView_GetPublishedVersionsOfAsset, result2.GetErrorMessage());
				return;
			}
			List<string> list;
			try
			{
				list = VersionUtilities.GetParsedAndOrderedVersions(result.GetDataMember()).ToList();
			}
			catch (Exception exception)
			{
				ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalView_ParsingVersions, exception);
				return;
			}
			List<PublishedVersion> list2 = JsonConvert.DeserializeObject<List<PublishedVersion>>(result2.GetDataMember().ToString());
			foreach (PublishedVersion item in list2)
			{
				if (item.State == betaState)
				{
					VersionBeta = item.AssetVersion;
				}
				if (item.State == publicState)
				{
					VersionPublic = item.AssetVersion;
				}
			}
			if (verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "versions", list.Count()));
				DebugUtility.DebugEnumerable("publishedVersions", list2);
			}
			OnModelChanged.Invoke(list, list2);
		}
		catch (Exception exception2)
		{
			DebugUtility.LogException(exception2);
		}
		finally
		{
			SetLoading(loading: false);
		}
	}

	private async Task ChangePublishStateAsync(string assetId, string version, UIPublishStates targetState, bool needsUnpublish, string previousVersion)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ChangePublishStateAsync", assetId, version, targetState, needsUnpublish, previousVersion);
		}
		if (needsUnpublish)
		{
			await UnpublishAsync(previousVersion);
		}
		SetLoading(loading: true);
		try
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SetPublishStateOnAssetAsync(assetId, version, targetState.ToEndlessCloudServicesCompatibleString());
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalController_PublishGame, graphQlResult.GetErrorMessage());
			}
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalController_PublishGame, exception);
		}
		finally
		{
			SetLoading(loading: false);
			LoadVersions();
		}
		DebugUtility.LogMethodWithAppension(this, "ChangePublishStateAsync", "COMPLETE", assetId, version, targetState);
	}

	private async Task UnpublishAsync(string versionToUnpublish)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UnpublishAsync", versionToUnpublish);
		}
		SetLoading(loading: true);
		try
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SetPublishStateOnAssetAsync(GameAsset.AssetID, versionToUnpublish, unpublishedState);
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalController_UnpublishGame, graphQlResult.GetErrorMessage());
			}
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalController_UnpublishGame, exception);
		}
		finally
		{
			SetLoading(loading: false);
			LoadVersions();
		}
	}

	private void SetLoading(bool loading)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetLoading", loading);
		}
		IsLoading = loading;
		if (loading)
		{
			OnLoadingStarted.Invoke();
		}
		else
		{
			OnLoadingEnded.Invoke();
		}
	}
}
