using System;
using System.Linq;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Runtime.Gameplay.LevelEditing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIGameAssetSummaryView : UIBaseGameAssetView
{
	[Header("UIGameAssetSummaryView")]
	[SerializeField]
	private TextMeshProUGUI typeText;

	[SerializeField]
	private Image[] imagesToColorByType = Array.Empty<Image>();

	[SerializeField]
	private TextMeshProUGUI[] textsToColorByType = Array.Empty<TextMeshProUGUI>();

	[Header("Version Notifications")]
	[SerializeField]
	private TextMeshProUGUI versionText;

	[SerializeField]
	private bool displayVersionNotifications;

	[SerializeField]
	private UIGameAssetVersionNotificationView gameAssetVersionNotification;

	[Header("Background")]
	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Color backgroundColorEven = Color.white;

	[SerializeField]
	private Color backgroundColorOdd = Color.black;

	[Header("In Library Marker")]
	[SerializeField]
	private GameObject inLibraryMarker;

	private bool subscribedToAssetVersionUpdated;

	private SerializableGuid gameId;

	public override void View(UIGameAsset model)
	{
		base.View(model);
		if (UIGameAsset.IsNullOrEmpty(model))
		{
			return;
		}
		typeText.text = model.Type.ToString();
		UIGameAssetTypes key = UIGameAssetTypes.Terrain;
		if ((model.Type & UIGameAssetTypes.Terrain) == UIGameAssetTypes.Terrain)
		{
			key = UIGameAssetTypes.Terrain;
		}
		else if ((model.Type & UIGameAssetTypes.Prop) == UIGameAssetTypes.Prop)
		{
			key = UIGameAssetTypes.Prop;
		}
		UIGameAssetTypeStyle uIGameAssetTypeStyle = gameAssetTypeStyleDictionary[key];
		Image[] array = imagesToColorByType;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = uIGameAssetTypeStyle.Color;
		}
		TextMeshProUGUI[] array2 = textsToColorByType;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].color = uIGameAssetTypeStyle.Color;
		}
		if (displayVersionNotifications)
		{
			Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
			if (activeGame != null)
			{
				gameAssetVersionNotification.gameObject.SetActive(value: true);
				gameId = activeGame.AssetID;
				MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.SubscribeToAssetVersionUpdated(gameId, base.Model.AssetID, gameAssetVersionNotification.View);
				subscribedToAssetVersionUpdated = true;
			}
		}
		if (!versionText)
		{
			return;
		}
		try
		{
			Version version = new Version(assetVersion);
			versionText.text = $"Version {version.Build}";
		}
		catch (Exception exception)
		{
			DebugUtility.LogError("Could not construct Version from a assetVersion of '" + assetVersion + "'!", this);
			DebugUtility.LogException(exception, this);
			versionText.text = "Version " + assetVersion;
		}
	}

	public void ViewInLibraryMarker(UIGameAsset model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewInLibraryMarker", "model", model), this);
		}
		Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
		if (activeGame != null)
		{
			bool isAssetInGameLibrary = GetIsAssetInGameLibrary(model, activeGame);
			inLibraryMarker.SetActive(isAssetInGameLibrary);
		}
	}

	private static bool GetIsAssetInGameLibrary(UIGameAsset model, Game activeGame)
	{
		if (model == null)
		{
			return false;
		}
		switch (model.Type)
		{
		case UIGameAssetTypes.Terrain:
			return activeGame.GameLibrary.TerrainEntries.Any((TerrainUsage entry) => entry.TilesetId == model.AssetID);
		case UIGameAssetTypes.Prop:
			return activeGame.GameLibrary.PropReferences.Any((AssetReference entry) => entry.AssetID == model.AssetID);
		case UIGameAssetTypes.SFX:
		case UIGameAssetTypes.Ambient:
		case UIGameAssetTypes.Music:
			return activeGame.GameLibrary.AudioReferences.Any((AssetReference entry) => entry.AssetID == model.AssetID);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SetBackgroundColor(bool isEven)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetBackgroundColor", "isEven", isEven), this);
		}
		backgroundImage.color = (isEven ? backgroundColorEven : backgroundColorOdd);
	}

	public override void Clear()
	{
		if (displayVersionNotifications && subscribedToAssetVersionUpdated && !gameId.IsEmpty)
		{
			MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.UnsubscribeToAssetVersionUpdated(gameId, base.Model.AssetID, gameAssetVersionNotification.View);
			subscribedToAssetVersionUpdated = false;
		}
		gameId = SerializableGuid.Empty;
		base.Clear();
		inLibraryMarker.SetActive(value: false);
	}
}
