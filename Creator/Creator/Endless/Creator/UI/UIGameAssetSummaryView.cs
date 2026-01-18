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

namespace Endless.Creator.UI
{
	// Token: 0x020000D0 RID: 208
	public class UIGameAssetSummaryView : UIBaseGameAssetView
	{
		// Token: 0x06000350 RID: 848 RVA: 0x00015DEC File Offset: 0x00013FEC
		public override void View(UIGameAsset model)
		{
			base.View(model);
			if (UIGameAsset.IsNullOrEmpty(model))
			{
				return;
			}
			this.typeText.text = model.Type.ToString();
			UIGameAssetTypes uigameAssetTypes = UIGameAssetTypes.Terrain;
			if ((model.Type & UIGameAssetTypes.Terrain) == UIGameAssetTypes.Terrain)
			{
				uigameAssetTypes = UIGameAssetTypes.Terrain;
			}
			else if ((model.Type & UIGameAssetTypes.Prop) == UIGameAssetTypes.Prop)
			{
				uigameAssetTypes = UIGameAssetTypes.Prop;
			}
			UIGameAssetTypeStyle uigameAssetTypeStyle = this.gameAssetTypeStyleDictionary[uigameAssetTypes];
			Image[] array = this.imagesToColorByType;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = uigameAssetTypeStyle.Color;
			}
			TextMeshProUGUI[] array2 = this.textsToColorByType;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].color = uigameAssetTypeStyle.Color;
			}
			if (this.displayVersionNotifications)
			{
				Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
				if (activeGame != null)
				{
					this.gameAssetVersionNotification.gameObject.SetActive(true);
					this.gameId = activeGame.AssetID;
					MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.SubscribeToAssetVersionUpdated(this.gameId, base.Model.AssetID, new Action<GameEditorAssetVersionManager.UpdateStatus>(this.gameAssetVersionNotification.View));
					this.subscribedToAssetVersionUpdated = true;
				}
			}
			if (this.versionText)
			{
				try
				{
					Version version = new Version(this.assetVersion);
					this.versionText.text = string.Format("Version {0}", version.Build);
				}
				catch (Exception ex)
				{
					DebugUtility.LogError("Could not construct Version from a assetVersion of '" + this.assetVersion + "'!", this);
					DebugUtility.LogException(ex, this);
					this.versionText.text = "Version " + this.assetVersion;
				}
			}
		}

		// Token: 0x06000351 RID: 849 RVA: 0x00015FA4 File Offset: 0x000141A4
		public void ViewInLibraryMarker(UIGameAsset model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewInLibraryMarker", "model", model), this);
			}
			Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
			if (activeGame == null)
			{
				return;
			}
			bool isAssetInGameLibrary = UIGameAssetSummaryView.GetIsAssetInGameLibrary(model, activeGame);
			this.inLibraryMarker.SetActive(isAssetInGameLibrary);
		}

		// Token: 0x06000352 RID: 850 RVA: 0x00015FF8 File Offset: 0x000141F8
		private static bool GetIsAssetInGameLibrary(UIGameAsset model, Game activeGame)
		{
			if (model == null)
			{
				return false;
			}
			UIGameAssetTypes type = model.Type;
			switch (type)
			{
			case UIGameAssetTypes.None:
			case UIGameAssetTypes.Terrain | UIGameAssetTypes.Prop:
			case UIGameAssetTypes.Terrain | UIGameAssetTypes.SFX:
			case UIGameAssetTypes.Prop | UIGameAssetTypes.SFX:
			case UIGameAssetTypes.Terrain | UIGameAssetTypes.Prop | UIGameAssetTypes.SFX:
				goto IL_00B7;
			case UIGameAssetTypes.Terrain:
				return activeGame.GameLibrary.TerrainEntries.Any((TerrainUsage entry) => entry.TilesetId == model.AssetID);
			case UIGameAssetTypes.Prop:
				return activeGame.GameLibrary.PropReferences.Any((AssetReference entry) => entry.AssetID == model.AssetID);
			case UIGameAssetTypes.SFX:
			case UIGameAssetTypes.Ambient:
				break;
			default:
				if (type != UIGameAssetTypes.Music)
				{
					goto IL_00B7;
				}
				break;
			}
			return activeGame.GameLibrary.AudioReferences.Any((AssetReference entry) => entry.AssetID == model.AssetID);
			IL_00B7:
			throw new ArgumentOutOfRangeException();
		}

		// Token: 0x06000353 RID: 851 RVA: 0x000160C4 File Offset: 0x000142C4
		public void SetBackgroundColor(bool isEven)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetBackgroundColor", "isEven", isEven), this);
			}
			this.backgroundImage.color = (isEven ? this.backgroundColorEven : this.backgroundColorOdd);
		}

		// Token: 0x06000354 RID: 852 RVA: 0x00016118 File Offset: 0x00014318
		public override void Clear()
		{
			if (this.displayVersionNotifications && this.subscribedToAssetVersionUpdated && !this.gameId.IsEmpty)
			{
				MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.UnsubscribeToAssetVersionUpdated(this.gameId, base.Model.AssetID, new Action<GameEditorAssetVersionManager.UpdateStatus>(this.gameAssetVersionNotification.View));
				this.subscribedToAssetVersionUpdated = false;
			}
			this.gameId = SerializableGuid.Empty;
			base.Clear();
			this.inLibraryMarker.SetActive(false);
		}

		// Token: 0x04000387 RID: 903
		[Header("UIGameAssetSummaryView")]
		[SerializeField]
		private TextMeshProUGUI typeText;

		// Token: 0x04000388 RID: 904
		[SerializeField]
		private Image[] imagesToColorByType = Array.Empty<Image>();

		// Token: 0x04000389 RID: 905
		[SerializeField]
		private TextMeshProUGUI[] textsToColorByType = Array.Empty<TextMeshProUGUI>();

		// Token: 0x0400038A RID: 906
		[Header("Version Notifications")]
		[SerializeField]
		private TextMeshProUGUI versionText;

		// Token: 0x0400038B RID: 907
		[SerializeField]
		private bool displayVersionNotifications;

		// Token: 0x0400038C RID: 908
		[SerializeField]
		private UIGameAssetVersionNotificationView gameAssetVersionNotification;

		// Token: 0x0400038D RID: 909
		[Header("Background")]
		[SerializeField]
		private Image backgroundImage;

		// Token: 0x0400038E RID: 910
		[SerializeField]
		private Color backgroundColorEven = Color.white;

		// Token: 0x0400038F RID: 911
		[SerializeField]
		private Color backgroundColorOdd = Color.black;

		// Token: 0x04000390 RID: 912
		[Header("In Library Marker")]
		[SerializeField]
		private GameObject inLibraryMarker;

		// Token: 0x04000391 RID: 913
		private bool subscribedToAssetVersionUpdated;

		// Token: 0x04000392 RID: 914
		private SerializableGuid gameId;
	}
}
