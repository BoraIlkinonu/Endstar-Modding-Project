using System;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Gameplay.LevelEditing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001B5 RID: 437
	public class UIGameLibraryAssetReplacementConfirmationModalView : UIEscapableModalView, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x0600067A RID: 1658 RVA: 0x000219AE File Offset: 0x0001FBAE
		// (set) Token: 0x0600067B RID: 1659 RVA: 0x000219B6 File Offset: 0x0001FBB6
		public UIGameAsset ToRemove { get; private set; }

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x0600067C RID: 1660 RVA: 0x000219BF File Offset: 0x0001FBBF
		// (set) Token: 0x0600067D RID: 1661 RVA: 0x000219C7 File Offset: 0x0001FBC7
		public UIGameAsset ToReplace { get; private set; }

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x0600067E RID: 1662 RVA: 0x000219D0 File Offset: 0x0001FBD0
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x0600067F RID: 1663 RVA: 0x000219D8 File Offset: 0x0001FBD8
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000680 RID: 1664 RVA: 0x000219E0 File Offset: 0x0001FBE0
		public override void Close()
		{
			base.Close();
			this.toRemove.Clear();
			this.toReplace.Clear();
		}

		// Token: 0x06000681 RID: 1665 RVA: 0x00021A00 File Offset: 0x0001FC00
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.ToRemove = (UIGameAsset)modalData[0];
			this.ToReplace = (UIGameAsset)modalData[1];
			this.toRemove.View(this.ToRemove);
			this.toReplace.View(this.ToReplace);
			this.toRemove.SetBackgroundColor(true);
			this.toReplace.SetBackgroundColor(false);
			this.affectedLevelsContainer.SetActive(false);
			this.affectedLevelsText.text = "";
		}

		// Token: 0x06000682 RID: 1666 RVA: 0x00021A88 File Offset: 0x0001FC88
		private void OnGetAssetSuccess(object result)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGetAssetSuccess", new object[] { result });
			}
			LevelState levelState = LevelStateLoader.Load(result.ToString());
			int toRemoveTileId = -1;
			foreach (TerrainUsage terrainUsage in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.TerrainEntries)
			{
				if (!(terrainUsage.TilesetId != this.toRemove.Model.AssetID))
				{
					toRemoveTileId = terrainUsage.RedirectIndex;
					break;
				}
			}
			if (toRemoveTileId == -1)
			{
				return;
			}
			int num = levelState.TerrainEntries.Count((TerrainEntry terrainEntry) => terrainEntry.TilesetId == toRemoveTileId);
			if (num > 0)
			{
				TextMeshProUGUI textMeshProUGUI = this.affectedLevelsText;
				textMeshProUGUI.text += string.Format("{0} - {1} Objects\n", levelState.Name, num);
				this.affectedLevelsContainer.SetActive(true);
			}
			this.OnLoadingEnded.Invoke();
		}

		// Token: 0x040005D4 RID: 1492
		[SerializeField]
		private UIGameAssetSummaryView toRemove;

		// Token: 0x040005D5 RID: 1493
		[SerializeField]
		private UIGameAssetSummaryView toReplace;

		// Token: 0x040005D6 RID: 1494
		[SerializeField]
		private GameObject affectedLevelsContainer;

		// Token: 0x040005D7 RID: 1495
		[SerializeField]
		private TextMeshProUGUI affectedLevelsText;
	}
}
