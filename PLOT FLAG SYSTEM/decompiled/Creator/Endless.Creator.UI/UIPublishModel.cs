using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Data;
using Endless.Gameplay.RightsManagement;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIPublishModel : UIGameObject, IUILoadingSpinnerViewCompatible
{
	public struct PublishedVersion
	{
		public string Asset_Version;

		public string State;

		public override string ToString()
		{
			return "Asset_Version: " + Asset_Version + ", State: " + State;
		}
	}

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private SerializableGuid subscribedAssetId;

	public List<string> Versions { get; private set; } = new List<string>();

	public string BetaVersion { get; private set; }

	public string PublicVersion { get; private set; }

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public event Action OnSynchronizeStart;

	public event Action<List<string>> OnVersionsSet;

	public event Action<string> OnBetaVersionSet;

	public event Action<string> OnPublicVersionSet;

	public event Action<Roles> OnClientGameRoleSet;

	public string GetVersion(UIPublishStates publishState)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetVersion", publishState);
		}
		switch (publishState)
		{
		case UIPublishStates.Beta:
			return BetaVersion;
		case UIPublishStates.Public:
			return PublicVersion;
		default:
			DebugUtility.LogNoEnumSupportError(this, "GetVersion", publishState, publishState);
			return null;
		}
	}

	public async void Synchronize()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Synchronize");
		}
		if (!subscribedAssetId.IsEmpty)
		{
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(subscribedAssetId, UpdateClientGameRole);
		}
		Versions.Clear();
		BetaVersion = null;
		PublicVersion = null;
		OnLoadingStarted.Invoke();
		this.OnSynchronizeStart?.Invoke();
		subscribedAssetId = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID;
		MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(subscribedAssetId, UpdateClientGameRole);
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID);
		if (graphQlResult.HasErrors)
		{
			OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UIPublishModel_RetrievingAssetVersions, graphQlResult.GetErrorMessage());
			return;
		}
		Versions = JsonConvert.DeserializeObject<string[]>(graphQlResult.GetDataMember().ToString()).ToList();
		if (Versions == null)
		{
			NullReferenceException exception = new NullReferenceException("Attempted to retrieve asset versions for game, but versions was null");
			ErrorHandler.HandleError(ErrorCodes.UIPublishModel_NullGameVersions, exception);
			return;
		}
		Versions = Versions.OrderByDescending(Version.Parse).ToList();
		Versions.Insert(0, UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString());
		this.OnVersionsSet?.Invoke(Versions);
		GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.GetPublishedVersionsOfAssetAsync(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID);
		if (graphQlResult2.HasErrors)
		{
			OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UIPublishModel_GetPublishedVersionsOfAsset, graphQlResult2.GetErrorMessage());
			return;
		}
		PublishedVersion[] array = JsonConvert.DeserializeObject<PublishedVersion[]>(graphQlResult2.GetDataMember().ToString());
		PublishedVersion[] array2;
		if (verboseLogging)
		{
			array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				PublishedVersion publishedVersion = array2[i];
				DebugUtility.Log(publishedVersion.ToString(), this);
			}
		}
		bool flag = false;
		bool flag2 = false;
		array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			PublishedVersion publishedVersion2 = array2[i];
			if (verboseLogging)
			{
				DebugUtility.Log(publishedVersion2.ToString(), this);
			}
			if (publishedVersion2.State == UIPublishStates.Beta.ToEndlessCloudServicesCompatibleString())
			{
				BetaVersion = publishedVersion2.Asset_Version;
				flag = true;
			}
			if (publishedVersion2.State == UIPublishStates.Public.ToEndlessCloudServicesCompatibleString())
			{
				PublicVersion = publishedVersion2.Asset_Version;
				flag2 = true;
			}
			if (flag && flag2)
			{
				break;
			}
		}
		if (!flag)
		{
			BetaVersion = UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();
		}
		if (!flag2)
		{
			PublicVersion = UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();
		}
		this.OnBetaVersionSet?.Invoke(BetaVersion);
		this.OnPublicVersionSet?.Invoke(PublicVersion);
		OnLoadingEnded.Invoke();
	}

	private void UpdateClientGameRole(IReadOnlyList<UserRole> roles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateClientGameRole", roles.Count);
		}
		int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
		Roles roleForUserId = roles.GetRoleForUserId(activeUserId);
		this.OnClientGameRoleSet(roleForUserId);
	}
}
