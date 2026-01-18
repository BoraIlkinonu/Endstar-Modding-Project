using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Gameplay.LevelEditing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIGameLibraryAssetReplacementConfirmationModalView : UIEscapableModalView, IUILoadingSpinnerViewCompatible
{
	[SerializeField]
	private UIGameAssetSummaryView toRemove;

	[SerializeField]
	private UIGameAssetSummaryView toReplace;

	[SerializeField]
	private GameObject affectedLevelsContainer;

	[SerializeField]
	private TextMeshProUGUI affectedLevelsText;

	public UIGameAsset ToRemove { get; private set; }

	public UIGameAsset ToReplace { get; private set; }

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public override void Close()
	{
		base.Close();
		toRemove.Clear();
		toReplace.Clear();
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		ToRemove = (UIGameAsset)modalData[0];
		ToReplace = (UIGameAsset)modalData[1];
		toRemove.View(ToRemove);
		toReplace.View(ToReplace);
		toRemove.SetBackgroundColor(isEven: true);
		toReplace.SetBackgroundColor(isEven: false);
		affectedLevelsContainer.SetActive(value: false);
		affectedLevelsText.text = "";
	}

	private void OnGetAssetSuccess(object result)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnGetAssetSuccess", result);
		}
		LevelState levelState = LevelStateLoader.Load(result.ToString());
		int toRemoveTileId = -1;
		foreach (TerrainUsage terrainEntry in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.TerrainEntries)
		{
			if (!(terrainEntry.TilesetId != toRemove.Model.AssetID))
			{
				toRemoveTileId = terrainEntry.RedirectIndex;
				break;
			}
		}
		if (toRemoveTileId != -1)
		{
			int num = levelState.TerrainEntries.Count((TerrainEntry terrainEntry) => terrainEntry.TilesetId == toRemoveTileId);
			if (num > 0)
			{
				affectedLevelsText.text += $"{levelState.Name} - {num} Objects\n";
				affectedLevelsContainer.SetActive(value: true);
			}
			OnLoadingEnded.Invoke();
		}
	}
}
