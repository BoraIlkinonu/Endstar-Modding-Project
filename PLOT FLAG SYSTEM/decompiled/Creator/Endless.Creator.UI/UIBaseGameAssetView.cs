using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.RightsManagement;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.TerrainCosmetics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public abstract class UIBaseGameAssetView : UIGameObject, IUILoadingSpinnerViewCompatible
{
	protected enum RequestType
	{
		SkeletonLoading,
		GetVersions,
		GetAsset,
		GetCreator
	}

	[SerializeField]
	protected UIGameAssetTypeStyleDictionary gameAssetTypeStyleDictionary;

	[SerializeField]
	private EndlessStudiosUserId endlessStudiosUserId;

	[Header("Visuals")]
	[SerializeField]
	private GameObject skeletonLoadingVisual;

	[SerializeField]
	private bool visualizeIcon = true;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[Header("Icon")]
	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private UIFileInstanceTexture2DView fileInstanceTexture2D;

	[Header("Creator Profile")]
	[SerializeField]
	private TextMeshProUGUI creatorNameText;

	[SerializeField]
	private Image creatorProfileImage;

	[SerializeField]
	private Sprite officialCreatorSprite;

	[SerializeField]
	private Sprite communityCreatorSprite;

	[SerializeField]
	private Image creatorOutlineImage;

	[SerializeField]
	private UIGameAssetCreatorTypesColorDictionary creatorTypeColorDictionary;

	private readonly HashSet<RequestType> requestsInProgress = new HashSet<RequestType>();

	protected string assetId;

	protected string assetVersion;

	[field: Header("Debugging")]
	[field: SerializeField]
	protected bool VerboseLogging { get; set; }

	public UIGameAsset Model { get; private set; }

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public virtual void View(UIGameAsset model)
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model.Name);
		}
		ViewAsync(model);
	}

	private async Task ViewAsync(UIGameAsset model)
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewAsync", model.Name);
		}
		if (model != Model)
		{
			Clear();
		}
		Model = model;
		assetId = model.AssetID;
		assetVersion = model.AssetVersion;
		if (UIGameAsset.IsNullOrEmpty(model))
		{
			ShowSkeleton();
			return;
		}
		HideSkeleton();
		ViewAssetInformation(model);
		await LoadCreatorAsync();
	}

	private void ViewAssetInformation(UIGameAsset model)
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewAssetInformation", model.Name);
		}
		displayNameText.text = string.Empty;
		iconImage.enabled = false;
		fileInstanceTexture2D.enabled = false;
		switch (model.Type)
		{
		case UIGameAssetTypes.Terrain:
		case UIGameAssetTypes.Prop:
			ViewTerrainOrProp(model);
			break;
		case UIGameAssetTypes.SFX:
		case UIGameAssetTypes.Ambient:
		case UIGameAssetTypes.Music:
			ViewAudio(model);
			break;
		default:
			DebugUtility.LogNoEnumSupportError(this, "ViewAssetInformation", model.Type);
			break;
		}
	}

	private void ViewTerrainOrProp(UIGameAsset model)
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewTerrainOrProp", model.Name);
		}
		if (model.Tileset != null)
		{
			assetId = model.Tileset.Asset.AssetID;
			displayNameText.text = model.Tileset.DisplayName;
			if (visualizeIcon)
			{
				iconImage.sprite = model.Tileset.DisplayIcon;
				iconImage.enabled = true;
			}
			return;
		}
		displayNameText.text = model.Asset.Name;
		if (!visualizeIcon)
		{
			return;
		}
		if ((bool)model.Icon || model.IconFileInstance != -1)
		{
			fileInstanceTexture2D.enabled = true;
			if ((bool)model.Icon)
			{
				fileInstanceTexture2D.View(model.Icon);
			}
			else
			{
				fileInstanceTexture2D.View(model.IconFileInstance);
			}
		}
		else
		{
			DebugUtility.LogWarning($"No icon available on model {model}", this);
		}
	}

	private void ViewAudio(UIGameAsset model)
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewAudio", model.Name);
		}
		AudioAsset audioAsset = (AudioAsset)model.Asset;
		displayNameText.text = audioAsset.Name;
		if (visualizeIcon)
		{
			fileInstanceTexture2D.enabled = true;
			if ((bool)model.Icon)
			{
				fileInstanceTexture2D.View(model.Icon);
			}
			else
			{
				fileInstanceTexture2D.View(model.IconFileInstance);
			}
		}
	}

	private async Task LoadCreatorAsync()
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "LoadCreatorAsync");
		}
		TrackRequest(RequestType.GetCreator);
		creatorProfileImage.sprite = null;
		creatorNameText.text = "Loading...";
		creatorOutlineImage.color = creatorTypeColorDictionary[UIGameAssetCreatorTypes.Official];
		try
		{
			OnUserRolesChanged((await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(assetId, OnUserRolesChanged)).Roles);
		}
		catch (Exception exception)
		{
			DebugUtility.LogException(exception, this);
		}
		finally
		{
			UntrackRequest(RequestType.GetCreator);
		}
	}

	public async Task GetAsset(string version, Action onComplete = null)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("GetAsset ( version: " + version + ", onComplete: " + onComplete.DebugIsNull() + " )", this);
		}
		TrackRequest(RequestType.GetAsset);
		try
		{
			switch (Model.Type)
			{
			case UIGameAssetTypes.Terrain:
				await LoadModel<TerrainTilesetCosmeticAsset>(version);
				break;
			case UIGameAssetTypes.Prop:
				await LoadModel<Prop>(version);
				break;
			case UIGameAssetTypes.SFX:
			case UIGameAssetTypes.Ambient:
			case UIGameAssetTypes.Music:
				await LoadModel<AudioAsset>(version);
				break;
			default:
				DebugUtility.LogNoEnumSupportError(this, "GetAsset", Model.Type, version);
				break;
			}
		}
		catch (Exception exception)
		{
			DebugUtility.LogException(exception, this);
		}
		finally
		{
			UntrackRequest(RequestType.GetAsset);
		}
		onComplete?.Invoke();
	}

	public virtual void Clear()
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		if (visualizeIcon)
		{
			iconImage.enabled = false;
			iconImage.sprite = null;
			fileInstanceTexture2D.enabled = false;
			fileInstanceTexture2D.Clear();
		}
		Model = null;
		UntrackAllRequests();
	}

	protected void TrackRequest(RequestType request)
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "TrackRequest", request);
		}
		if (!requestsInProgress.Add(request))
		{
			DebugUtility.LogWarning($"{request} is already in progress.", this);
		}
		else if (requestsInProgress.Count == 1)
		{
			OnLoadingStarted.Invoke();
		}
	}

	protected void UntrackRequest(RequestType request)
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "UntrackRequest", request);
		}
		if (requestsInProgress.Remove(request) && requestsInProgress.Count == 0)
		{
			OnLoadingEnded.Invoke();
		}
	}

	protected void OnUserRolesChanged(IReadOnlyList<UserRole> roles)
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnUserRolesChanged", roles.Count);
		}
		bool isOfficial = roles.Any((UserRole r) => r.UserId == endlessStudiosUserId.InternalId);
		ViewCreatorType(isOfficial);
	}

	private void UntrackAllRequests()
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "UntrackAllRequests");
		}
		foreach (RequestType item in requestsInProgress.ToList())
		{
			UntrackRequest(item);
		}
	}

	private void ViewCreatorType(bool isOfficial)
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewCreatorType", isOfficial);
		}
		creatorProfileImage.sprite = (isOfficial ? officialCreatorSprite : communityCreatorSprite);
		creatorNameText.text = (isOfficial ? "Endless Studios" : "Community");
		UIGameAssetCreatorTypes key = ((!isOfficial) ? UIGameAssetCreatorTypes.Community : UIGameAssetCreatorTypes.Official);
		creatorOutlineImage.color = creatorTypeColorDictionary[key];
	}

	private async Task LoadModel<T>(string version) where T : Asset
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "LoadModel", version);
		}
		AssetCacheResult<T> assetCacheResult = await EndlessAssetCache.GetAssetAsync<T>(assetId, version);
		if (assetCacheResult.HasErrors)
		{
			ErrorHandler.HandleError(ErrorCodes.UIBaseGameAssetVew_GetAsset, assetCacheResult.GetErrorMessage());
			return;
		}
		switch (Model.Type)
		{
		case UIGameAssetTypes.Terrain:
			Model = new UIGameAsset(assetCacheResult.Asset as TerrainTilesetCosmeticAsset);
			break;
		case UIGameAssetTypes.Prop:
			Model = new UIGameAsset(assetCacheResult.Asset as Prop);
			break;
		case UIGameAssetTypes.SFX:
		case UIGameAssetTypes.Ambient:
		case UIGameAssetTypes.Music:
		{
			AudioAsset audioAsset = assetCacheResult.Asset as AudioAsset;
			switch (audioAsset.AudioCategory)
			{
			case AudioCategory.Music:
				Model = new UIGameAsset(audioAsset, UIGameAssetTypes.Music);
				break;
			case AudioCategory.SFX:
				Model = new UIGameAsset(audioAsset, UIGameAssetTypes.SFX);
				break;
			case AudioCategory.Ambient:
				Model = new UIGameAsset(audioAsset, UIGameAssetTypes.Ambient);
				break;
			default:
				DebugUtility.LogNoEnumSupportError(this, "LoadModel", audioAsset.AudioCategory);
				break;
			}
			break;
		}
		default:
			DebugUtility.LogNoEnumSupportError(this, "LoadModel", Model.Type);
			break;
		}
	}

	private void ShowSkeleton()
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ShowSkeleton");
		}
		TrackRequest(RequestType.SkeletonLoading);
		skeletonLoadingVisual.SetActive(value: true);
	}

	private void HideSkeleton()
	{
		if (VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HideSkeleton");
		}
		if (skeletonLoadingVisual.activeSelf)
		{
			skeletonLoadingVisual.SetActive(value: false);
			UntrackRequest(RequestType.SkeletonLoading);
		}
	}
}
