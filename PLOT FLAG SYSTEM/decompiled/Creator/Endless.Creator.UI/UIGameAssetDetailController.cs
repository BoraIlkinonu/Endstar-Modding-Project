using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Data;
using Endless.FileManagement;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.GraphQl;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.TerrainCosmetics;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIGameAssetDetailController : UIGameObject, IUILoadingSpinnerViewCompatible
{
	public static Action AssetAdded;

	[SerializeField]
	private UIGameAssetDetailView view;

	[SerializeField]
	private UIDropdownVersion versionDropdown;

	[Header("Actions")]
	[SerializeField]
	private UIButton playAudioClipButton;

	[SerializeField]
	private UIButton stopAudioClipButton;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private UIAudioCategoryAudioMixerGroupDictionary audioCategoryAudioMixerGroupDictionary;

	[SerializeField]
	private UIButton moderatorateButton;

	[SerializeField]
	private UIAssetModerationModalView assetModerationModalSource;

	[SerializeField]
	private UIButton addButton;

	[SerializeField]
	private UIButton applyVersionButton;

	[SerializeField]
	private UIButton publishButton;

	[SerializeField]
	private UIGameAssetPublishModalView gameAssetPublishModalSource;

	[Header("Editable")]
	[SerializeField]
	private UIInputField nameInputField;

	[SerializeField]
	private UIInputField descriptionInputField;

	[SerializeField]
	private UIButton saveAsNewVersionButton;

	[SerializeField]
	private UIButton discardButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private AudioClip audioClip;

	private Coroutine waitForAudioClipAndViewPlayButtonCoroutine;

	private CancellationTokenSource viewVersionCancellationTokenSource;

	private CancellationTokenSource playAudioClipCancellationTokenSource;

	private CancellationTokenSource addCancellationTokenSource;

	private CancellationTokenSource applyVersionCancellationTokenSource;

	private CancellationTokenSource saveAsNewVersionCts;

	private UIGameAsset Model => view.Model;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		versionDropdown.OnValueChanged.AddListener(ViewVersionAsync);
		playAudioClipButton.onClick.AddListener(PlayAudioClip);
		stopAudioClipButton.onClick.AddListener(StopAudioClip);
		moderatorateButton.onClick.AddListener(DisplayAssetModerationModal);
		addButton.onClick.AddListener(Add);
		applyVersionButton.onClick.AddListener(ApplyVersion);
		publishButton.onClick.AddListener(Publish);
		saveAsNewVersionButton.onClick.AddListener(SaveAsNewVersion);
		discardButton.onClick.AddListener(Discard);
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		if (audioSource.isPlaying)
		{
			audioSource.Stop();
			audioSource.clip = null;
		}
		if (waitForAudioClipAndViewPlayButtonCoroutine != null)
		{
			StopCoroutine(waitForAudioClipAndViewPlayButtonCoroutine);
			waitForAudioClipAndViewPlayButtonCoroutine = null;
		}
		if ((bool)audioClip)
		{
			audioClip = null;
			AudioAsset audioAsset = (AudioAsset)Model.Asset;
			MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(base.gameObject, audioAsset.AudioFileInstanceId);
		}
		CancellationTokenSourceUtility.CancelAndCleanup(ref viewVersionCancellationTokenSource);
		CancellationTokenSourceUtility.CancelAndCleanup(ref playAudioClipCancellationTokenSource);
		CancellationTokenSourceUtility.CancelAndCleanup(ref addCancellationTokenSource);
		CancellationTokenSourceUtility.CancelAndCleanup(ref applyVersionCancellationTokenSource);
		CancellationTokenSourceUtility.CancelAndCleanup(ref saveAsNewVersionCts);
	}

	private async void ViewVersionAsync()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewVersionAsync");
		}
		CancellationTokenSourceUtility.RecreateTokenSource(ref viewVersionCancellationTokenSource);
		CancellationToken cancellationToken = viewVersionCancellationTokenSource.Token;
		string value = versionDropdown.Value;
		if (verboseLogging)
		{
			Debug.Log("version: " + value, this);
		}
		try
		{
			await view.GetAsset(value);
			cancellationToken.ThrowIfCancellationRequested();
			view.View(view.Model);
		}
		catch (OperationCanceledException)
		{
			if (verboseLogging)
			{
				Debug.Log("ViewVersionAsync was cancelled", this);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	private async void PlayAudioClip()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "PlayAudioClip");
		}
		CancellationTokenSourceUtility.RecreateTokenSource(ref playAudioClipCancellationTokenSource);
		CancellationToken cancellationToken = playAudioClipCancellationTokenSource.Token;
		AudioAsset audioAsset = (AudioAsset)Model.Asset;
		try
		{
			if (!audioClip)
			{
				audioClip = await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetAudioFileAsync(base.gameObject, audioAsset.AudioFileInstanceId, audioAsset.AudioType);
			}
			cancellationToken.ThrowIfCancellationRequested();
			audioSource.clip = audioClip;
			audioSource.outputAudioMixerGroup = audioCategoryAudioMixerGroupDictionary[audioAsset.AudioCategory];
			audioSource.Play();
			view.ViewAudioButton(isPlaying: true);
			waitForAudioClipAndViewPlayButtonCoroutine = StartCoroutine(WaitForAudioClipAndViewPlayButton());
		}
		catch (OperationCanceledException)
		{
			if (verboseLogging)
			{
				Debug.Log("PlayAudioClip was cancelled", this);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	private IEnumerator WaitForAudioClipAndViewPlayButton()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "WaitForAudioClipAndViewPlayButton");
		}
		yield return new WaitForSeconds(audioSource.clip.length);
		view.ViewAudioButton(isPlaying: false);
		waitForAudioClipAndViewPlayButtonCoroutine = null;
	}

	private void StopAudioClip()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "StopAudioClip");
		}
		if (waitForAudioClipAndViewPlayButtonCoroutine != null)
		{
			StopCoroutine(waitForAudioClipAndViewPlayButtonCoroutine);
			waitForAudioClipAndViewPlayButtonCoroutine = null;
		}
		view.ViewAudioButton(isPlaying: false);
		audioSource.Stop();
	}

	private void DisplayAssetModerationModal()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayAssetModerationModal");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(assetModerationModalSource, UIModalManagerStackActions.MaintainStack, view.Model);
	}

	private async void Add()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Add");
		}
		CancellationTokenSourceUtility.RecreateTokenSource(ref addCancellationTokenSource);
		CancellationToken token = addCancellationTokenSource.Token;
		OnLoadingStarted.Invoke();
		try
		{
			string value = versionDropdown.Value;
			token.ThrowIfCancellationRequested();
			switch (Model.Type)
			{
			case UIGameAssetTypes.Terrain:
			{
				await MonoBehaviourSingleton<GameEditor>.Instance.AddTerrainUsageToGameLibrary(Model.AssetID, value);
				int terrainIndex = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.GetTerrainIndex(Model.AssetID);
				PaintingTool paintingTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PaintingTool>();
				if (paintingTool.IsActive)
				{
					paintingTool.SetActiveTilesetIndex(terrainIndex);
				}
				break;
			}
			case UIGameAssetTypes.SFX:
			case UIGameAssetTypes.Ambient:
			case UIGameAssetTypes.Music:
			{
				AssetReference assetReference2 = new AssetReference
				{
					AssetID = Model.AssetID,
					AssetVersion = value,
					AssetType = "audio"
				};
				await MonoBehaviourSingleton<GameEditor>.Instance.AddAudioToGameLibrary(assetReference2);
				break;
			}
			case UIGameAssetTypes.Prop:
			{
				AssetReference assetReference = new AssetReference
				{
					AssetID = Model.AssetID,
					AssetVersion = value,
					AssetType = "prop"
				};
				if (await MonoBehaviourSingleton<GameEditor>.Instance.AddPropToGameLibrary(assetReference))
				{
					PropTool propTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PropTool>();
					if (propTool.IsActive)
					{
						propTool.UpdateSelectedAssetId(Model.AssetID);
					}
				}
				break;
			}
			default:
				DebugUtility.LogNoEnumSupportError(this, "Add", Model.Type);
				break;
			}
		}
		catch (OperationCanceledException)
		{
			if (verboseLogging)
			{
				Debug.Log("Add was cancelled", this);
			}
			OnLoadingEnded.Invoke();
			return;
		}
		catch (Exception exception)
		{
			DebugUtility.LogException(exception, this);
			ErrorHandler.HandleError(ErrorCodes.UIGameAssetDetailController_AddAsset, exception);
		}
		OnLoadingEnded.Invoke();
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		AssetAdded?.Invoke();
	}

	private void Publish()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Publish");
		}
		UIGameAssetPublishModalModel uIGameAssetPublishModalModel = new UIGameAssetPublishModalModel(Model);
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(gameAssetPublishModalSource, UIModalManagerStackActions.MaintainStack, uIGameAssetPublishModalModel);
	}

	private async void ApplyVersion()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyVersion");
		}
		if (MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame == null)
		{
			Debug.LogException(new Exception("You can't add an asset while not in a game!"), this);
			return;
		}
		CancellationTokenSourceUtility.RecreateTokenSource(ref applyVersionCancellationTokenSource);
		CancellationToken token = applyVersionCancellationTokenSource.Token;
		string value = versionDropdown.Value;
		string text = view.Versions[0];
		if (value == text)
		{
			Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
			MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.MarkAssetVersionAsLatest(activeGame.AssetID, Model.AssetID);
		}
		OnLoadingStarted.Invoke();
		string assetId = Model.AssetID;
		UIGameAsset model;
		try
		{
			token.ThrowIfCancellationRequested();
			model = await UpdateGameAsset(Model, value);
		}
		catch (OperationCanceledException)
		{
			if (verboseLogging)
			{
				Debug.Log("ApplyVersion was cancelled", this);
			}
			OnLoadingEnded.Invoke();
			return;
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.GameLibrary_VersionChangeFailed, exception);
			OnLoadingEnded.Invoke();
			return;
		}
		OnLoadingEnded.Invoke();
		if (base.gameObject.activeInHierarchy && !(assetId != Model.AssetID))
		{
			view.SetActiveAssetVersionAndHandleRelatedVisibility();
			view.View(model);
		}
	}

	private async Task<UIGameAsset> UpdateGameAsset(UIGameAsset gameAsset, string version)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateGameAsset", gameAsset.AssetID, version);
		}
		string assetId = gameAsset.AssetID;
		try
		{
			switch (Model.Type)
			{
			case UIGameAssetTypes.Terrain:
				await MonoBehaviourSingleton<GameEditor>.Instance.SetTerrainUsageVersionInGameLibrary(assetId, version);
				return new UIGameAsset(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RuntimePalette.DistinctTilesets.Find((Tileset tileset) => tileset.Asset.AssetID == assetId));
			case UIGameAssetTypes.Prop:
			{
				await MonoBehaviourSingleton<GameEditor>.Instance.SetPropVersionInGameLibrary(assetId, version);
				if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out var metadata2))
				{
					return new UIGameAsset(metadata2);
				}
				throw new Exception("Failed to set prop version");
			}
			case UIGameAssetTypes.SFX:
			case UIGameAssetTypes.Ambient:
			case UIGameAssetTypes.Music:
			{
				bool flag = await MonoBehaviourSingleton<GameEditor>.Instance.SetAudioVersionInGameLibrary(assetId, version);
				if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(assetId, out var metadata))
				{
					Debug.Log("Got new version " + metadata.AudioAsset.AssetVersion);
					return new UIGameAsset(metadata);
				}
				throw new Exception($"Failed to set audio version: {flag}");
			}
			default:
				DebugUtility.LogException(new ArgumentOutOfRangeException($"No support for a {Model.Type.GetType().Name} of {Model.Type}!"), this);
				break;
			}
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.UIGameAssetDetailController_UpdateGameAsset, exception);
		}
		return gameAsset;
	}

	private void Discard()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Discard");
		}
		nameInputField.text = Model.Name;
		descriptionInputField.text = Model.Asset.Description;
		view.SetWriteable(newValue: false);
	}

	private async void SaveAsNewVersion()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SaveAsNewVersion");
		}
		CancellationTokenSourceUtility.RecreateTokenSource(ref saveAsNewVersionCts);
		CancellationToken token = saveAsNewVersionCts.Token;
		if (Model.Type == UIGameAssetTypes.Terrain && Model.Tileset != null)
		{
			DebugUtility.LogException(new Exception("A Tileset is not an asset and therefore can not be updated!"), this);
		}
		Model.Asset.Name = nameInputField.text;
		Model.Asset.Description = descriptionInputField.text;
		Version version = new Version(view.Versions[0]);
		Version version2 = new Version(version.Major, version.Minor, version.Build + 1);
		Model.Asset.AssetVersion = version2.ToString();
		OnLoadingStarted.Invoke();
		GraphQlResult graphQlResult = null;
		try
		{
			token.ThrowIfCancellationRequested();
			switch (Model.Type)
			{
			case UIGameAssetTypes.Terrain:
			{
				TerrainTilesetCosmeticAsset asset2 = (TerrainTilesetCosmeticAsset)Model.Asset;
				graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(asset2);
				break;
			}
			case UIGameAssetTypes.Prop:
			{
				Prop asset = ((Prop)Model.Asset).Clone();
				graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(asset);
				break;
			}
			case UIGameAssetTypes.SFX:
			case UIGameAssetTypes.Ambient:
			case UIGameAssetTypes.Music:
				graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync((AudioAsset)Model.Asset);
				break;
			default:
				DebugUtility.LogNoEnumSupportError(this, "SaveAsNewVersion", Model.Type);
				break;
			}
			OnLoadingEnded.Invoke();
			if (graphQlResult != null)
			{
				if (graphQlResult.HasErrors)
				{
					Exception exception = graphQlResult.GetErrorMessage();
					if (exception.Message.Contains("already exists"))
					{
						OnLoadingStarted.Invoke();
						GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.GetVersionsAsync(Model.AssetID);
						OnLoadingEnded.Invoke();
						if (graphQlResult2.HasErrors)
						{
							ErrorHandler.HandleError(ErrorCodes.UIGameAssetDetailController_RetrievingAssetVersions, exception);
						}
						else
						{
							string value = graphQlResult2.GetDataMember().ToString();
							string[] versions = JsonConvert.DeserializeObject<string[]>(value);
							if (versions.Length == 0)
							{
								OnLoadingEnded.Invoke();
								ErrorHandler.HandleError(ErrorCodes.UIGameAssetDetailController_NoVersionsOfAsset, new Exception("There are no versions in the result!"));
								return;
							}
							MonoBehaviourSingleton<UIModalManager>.Instance.Confirm("Save Conflict\nA version of " + Model.Asset.AssetVersion + " already been saved. Do you wish to save what you have as a newer version?", delegate
							{
								DebugUtility.DebugEnumerable("versions", versions, this);
								string assetVersion = versions.Select((string item) => new Version(item)).Max().ToString();
								Model.Asset.AssetVersion = assetVersion;
								MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
								SaveAsNewVersion();
							}, MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack);
						}
					}
					else
					{
						ErrorHandler.HandleError(ErrorCodes.UIGameAssetDetailController_UpdateAsset, exception);
					}
				}
				else
				{
					object dataMember = graphQlResult.GetDataMember();
					switch (Model.Type)
					{
					case UIGameAssetTypes.Terrain:
					{
						TerrainTilesetCosmeticAsset asset5 = JsonConvert.DeserializeObject<TerrainTilesetCosmeticAsset>(dataMember.ToString());
						EndlessAssetCache.AddNewVersionToCache(asset5);
						Model.SetAsset(asset5);
						break;
					}
					case UIGameAssetTypes.Prop:
					{
						Prop asset4 = JsonConvert.DeserializeObject<Prop>(dataMember.ToString());
						EndlessAssetCache.AddNewVersionToCache(asset4);
						Model.SetAsset(asset4);
						break;
					}
					case UIGameAssetTypes.SFX:
					case UIGameAssetTypes.Ambient:
					case UIGameAssetTypes.Music:
					{
						AudioAsset asset3 = JsonConvert.DeserializeObject<AudioAsset>(dataMember.ToString());
						EndlessAssetCache.AddNewVersionToCache(asset3);
						Model.SetAsset(asset3);
						break;
					}
					default:
						DebugUtility.LogNoEnumSupportError(this, "SaveAsNewVersion", Model.Type);
						break;
					}
					view.View(Model);
				}
			}
			view.ViewAssetNameAndDescription();
			view.SetWriteable(newValue: false);
		}
		catch (OperationCanceledException)
		{
			if (verboseLogging)
			{
				Debug.Log("SaveAsNewVersion was cancelled", this);
			}
			OnLoadingEnded.Invoke();
		}
	}
}
