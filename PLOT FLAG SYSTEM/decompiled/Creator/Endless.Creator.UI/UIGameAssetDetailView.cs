using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Gameplay.LevelEditing;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameAssetDetailView : UIBaseGameAssetView
{
	[Header("UIGameAssetDetailView")]
	[SerializeField]
	private UIDropdownVersion versionDropdown;

	[SerializeField]
	private TextMeshProUGUI versionInLibraryText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[Header("Actions")]
	[SerializeField]
	private UIButton playAudioClipButton;

	[SerializeField]
	private UIButton stopAudioClipButton;

	[SerializeField]
	private UIButton moderatorateButton;

	[SerializeField]
	private UIButton addButton;

	[SerializeField]
	private UIButton applyVersionButton;

	[SerializeField]
	private UIButton publishButton;

	[Header("Editable")]
	[SerializeField]
	private UIInputField nameInputField;

	[SerializeField]
	private UIInputField descriptionInputField;

	[SerializeField]
	private UIButton saveAsNewVersionButton;

	[SerializeField]
	private UIButton discardButton;

	[SerializeField]
	private UIUserRolesView userRolesView;

	private readonly string[] versionsValues = new string[1];

	private readonly Dictionary<UIGameAssetTypes, UIUserRoleWizard.AssetTypes> assetTypeDictionary = new Dictionary<UIGameAssetTypes, UIUserRoleWizard.AssetTypes>
	{
		{
			UIGameAssetTypes.Terrain,
			UIUserRoleWizard.AssetTypes.Tileset
		},
		{
			UIGameAssetTypes.Prop,
			UIUserRoleWizard.AssetTypes.Prop
		},
		{
			UIGameAssetTypes.SFX,
			UIUserRoleWizard.AssetTypes.Audio
		},
		{
			UIGameAssetTypes.Ambient,
			UIUserRoleWizard.AssetTypes.Audio
		},
		{
			UIGameAssetTypes.Music,
			UIUserRoleWizard.AssetTypes.Audio
		}
	};

	private AssetContexts assetContext;

	private List<string> versions = new List<string>();

	private bool listenersAdded;

	private CancellationTokenSource loadVersionsCancellationTokenSource;

	[field: Header("UIGameAssetDetailView")]
	[field: SerializeField]
	public UIUserRolesModel UserRolesModel { get; private set; }

	public IReadOnlyList<string> Versions => versions;

	public string ActiveAssetVersion { get; private set; }

	public bool Writeable { get; private set; }

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		if (!listenersAdded)
		{
			AddListeners();
		}
	}

	private void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		Clear();
		if (listenersAdded)
		{
			UserRolesModel.OnUserRolesSet.RemoveListener(base.OnUserRolesChanged);
			UserRolesModel.OnLocalClientRoleSet.RemoveListener(OnLocalClientRoleSet);
		}
	}

	public override void View(UIGameAsset model)
	{
		base.View(model);
		if (!UIGameAsset.IsNullOrEmpty(model))
		{
			DebugUtility.Log("Viewing asset: " + model.AssetID + ", " + model.AssetVersion, this);
			if (!listenersAdded)
			{
				AddListeners();
			}
			Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
			if (activeGame != null)
			{
				MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.SetAssetUpdateSeen(activeGame.AssetID, base.Model.AssetID);
			}
			SetWriteable(newValue: false);
			nameInputField.text = model.Name;
			descriptionInputField.text = model.Description;
			moderatorateButton.gameObject.SetActive(assetContext == AssetContexts.UserReported);
			if (model.Type.IsAudio())
			{
				ViewAudioButton(isPlaying: false);
			}
			else
			{
				playAudioClipButton.gameObject.SetActive(value: false);
				stopAudioClipButton.gameObject.SetActive(value: false);
			}
			if (model.Tileset == null)
			{
				SetWriteable(newValue: false);
				SetActiveAssetVersionAndHandleRelatedVisibility();
			}
			else
			{
				assetVersion = GetTerrainUsageVersion(model.AssetID);
			}
			UserRolesModel.Initialize(model.AssetID, model.Name, SerializableGuid.Empty, assetContext);
			if (assetTypeDictionary.TryGetValue(model.Type, out var value))
			{
				UserRolesModel.SetAssetType(value);
			}
			else
			{
				DebugUtility.LogError(string.Format("{0} has not entry for a key of {1}", "assetTypeDictionary", model.Type), this);
			}
			GetAsset(assetVersion, ViewDescriptionAndSetActiveAssetVersionAndHandleRelatedVisibility);
			LoadVersionsAsync();
		}
	}

	private void AddListeners()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "AddListeners");
		}
		if (!listenersAdded)
		{
			listenersAdded = true;
			UserRolesModel.OnUserRolesSet.AddListener(base.OnUserRolesChanged);
			UserRolesModel.OnLocalClientRoleSet.AddListener(OnLocalClientRoleSet);
		}
	}

	private async Task LoadVersionsAsync()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "LoadVersionsAsync");
		}
		CancellationTokenSourceUtility.RecreateTokenSource(ref loadVersionsCancellationTokenSource);
		CancellationToken cancellationToken = loadVersionsCancellationTokenSource.Token;
		TrackRequest(RequestType.GetVersions);
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(assetId);
			cancellationToken.ThrowIfCancellationRequested();
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UIGameAssetDetailView_RetrievingAssetVersions, graphQlResult.GetErrorMessage());
				return;
			}
			string value = graphQlResult.GetDataMember().ToString();
			versions = JsonConvert.DeserializeObject<string[]>(value).OrderByDescending(Version.Parse).ToList();
			cancellationToken.ThrowIfCancellationRequested();
			ViewVersionInDropdown(assetVersion);
		}
		catch (OperationCanceledException)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("LoadVersionsAsync was cancelled", this);
			}
		}
		finally
		{
			UntrackRequest(RequestType.GetVersions);
		}
	}

	public void ViewVersionInDropdown(string currentVersion)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewVersionInDropdown", currentVersion);
		}
		if (versions == null || versions.Count == 0)
		{
			versionDropdown.gameObject.SetActive(value: false);
			return;
		}
		List<string> list = versions.OrderByDescending(Version.Parse).ToList();
		if (list.IndexOf(currentVersion) < 0)
		{
			DebugUtility.LogException(new Exception("Version '" + currentVersion + "' not found!"), this);
			versionDropdown.gameObject.SetActive(value: false);
		}
		else
		{
			versionDropdown.SetOptionsAndValue(list, currentVersion, triggerOnValueChanged: false);
			versionDropdown.gameObject.SetActive(value: true);
		}
	}

	public void SetContext(AssetContexts context)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetContext", context);
		}
		assetContext = context;
		bool flag = context == AssetContexts.GameLibraryAddition;
		userRolesView.SetBlockActivationOfAddUserRoleButton(flag);
		versionDropdown.SetIsInteractable(!flag);
		versionInLibraryText.gameObject.SetActive(context == AssetContexts.GameLibrary);
		if (!flag)
		{
			addButton.gameObject.SetActive(value: false);
		}
	}

	public void ViewAssetNameAndDescription()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewAssetNameAndDescription");
		}
		if (base.Model.Asset == null)
		{
			DebugUtility.LogError(this, "ViewAssetNameAndDescription", "This should not be called on a Tileset!");
			return;
		}
		Asset asset = base.Model.Asset;
		nameInputField.text = asset.Name;
		descriptionInputField.text = asset.Description;
	}

	public void SetWriteable(bool newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "SetWriteable", string.Format("{0}: {1}", "assetContext", assetContext), newValue);
		}
		Writeable = newValue;
		versionDropdown.gameObject.SetActive(!Writeable);
		bool flag = assetContext == AssetContexts.GameLibraryAddition;
		bool flag2 = assetContext == AssetContexts.MainMenu;
		publishButton.gameObject.SetActive(Writeable && flag2);
		nameInputField.gameObject.SetActive(Writeable && !flag);
		descriptionInputField.gameObject.SetActive(Writeable && !flag);
		saveAsNewVersionButton.gameObject.SetActive(Writeable && !flag);
		discardButton.gameObject.SetActive(Writeable && !flag);
	}

	public void SetActiveAssetVersionAndHandleRelatedVisibility()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetActiveAssetVersionAndHandleRelatedVisibility");
		}
		ActiveAssetVersion = null;
		bool flag = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame != null;
		if (flag)
		{
			GameLibrary gameLibrary = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary;
			switch (base.Model.Type)
			{
			case UIGameAssetTypes.Terrain:
				foreach (TerrainUsage terrainEntry in gameLibrary.TerrainEntries)
				{
					if (terrainEntry.IsActive && terrainEntry.TerrainAssetReference.AssetID == base.Model.AssetID)
					{
						ActiveAssetVersion = terrainEntry.TerrainAssetReference.AssetVersion;
						break;
					}
				}
				break;
			case UIGameAssetTypes.Prop:
				foreach (AssetReference propReference in gameLibrary.PropReferences)
				{
					if (propReference.AssetID == base.Model.AssetID)
					{
						ActiveAssetVersion = propReference.AssetVersion;
						break;
					}
				}
				break;
			case UIGameAssetTypes.SFX:
			case UIGameAssetTypes.Ambient:
			case UIGameAssetTypes.Music:
				foreach (AssetReference audioReference in gameLibrary.AudioReferences)
				{
					if (audioReference.AssetID == base.Model.AssetID)
					{
						ActiveAssetVersion = audioReference.AssetVersion;
						break;
					}
				}
				break;
			default:
				DebugUtility.LogNoEnumSupportError(this, "SetActiveAssetVersionAndHandleRelatedVisibility", base.Model.Type);
				break;
			}
		}
		bool flag2 = ActiveAssetVersion != null;
		bool flag3 = assetContext == AssetContexts.GameLibraryAddition;
		bool active = flag && !flag2 && flag3;
		addButton.gameObject.SetActive(active);
		bool active2 = flag2 && base.Model.Asset.AssetVersion != ActiveAssetVersion && assetContext == AssetContexts.GameLibrary;
		applyVersionButton.gameObject.SetActive(active2);
	}

	public override void Clear()
	{
		base.Clear();
		UserRolesModel.Clear();
		versionDropdown.SetValueText("Loading...");
		nameInputField.text = string.Empty;
		descriptionInputField.text = string.Empty;
		descriptionText.text = string.Empty;
		addButton.gameObject.SetActive(value: false);
		applyVersionButton.gameObject.SetActive(value: false);
		publishButton.gameObject.SetActive(value: false);
		saveAsNewVersionButton.gameObject.SetActive(value: false);
		discardButton.gameObject.SetActive(value: false);
		CancellationTokenSourceUtility.CancelAndCleanup(ref loadVersionsCancellationTokenSource);
	}

	public void ViewAudioButton(bool isPlaying)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewAudioButton", isPlaying);
		}
		playAudioClipButton.gameObject.SetActive(!isPlaying);
		stopAudioClipButton.gameObject.SetActive(isPlaying);
	}

	private void ViewDescriptionAndSetActiveAssetVersionAndHandleRelatedVisibility()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewDescriptionAndSetActiveAssetVersionAndHandleRelatedVisibility");
		}
		descriptionText.text = base.Model.Description;
		SetActiveAssetVersionAndHandleRelatedVisibility();
	}

	private void OnLocalClientRoleSet(Roles localClientRole)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnLocalClientRoleSet", localClientRole);
		}
		bool active = assetContext == AssetContexts.MainMenu && localClientRole.IsGreaterThanOrEqualTo(Roles.Publisher);
		publishButton.gameObject.SetActive(active);
	}

	private string GetTerrainUsageVersion(SerializableGuid tilesetId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetTerrainUsageVersion", tilesetId);
		}
		if (MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame == null)
		{
			Debug.LogException(new Exception("You can't add an asset while not in a game!"), this);
			return null;
		}
		TerrainUsage terrainUsage = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.TerrainEntries.FirstOrDefault((TerrainUsage entry) => entry.TilesetId == tilesetId);
		if (terrainUsage != null)
		{
			return terrainUsage.TerrainAssetReference.AssetVersion;
		}
		DebugUtility.LogException(new Exception(string.Format("Could not find a {0} in {1}.{2} with a {3} of {4}!", "TerrainUsage", "GameLibrary", "TerrainEntries", "tilesetId", tilesetId)), this);
		return null;
	}
}
