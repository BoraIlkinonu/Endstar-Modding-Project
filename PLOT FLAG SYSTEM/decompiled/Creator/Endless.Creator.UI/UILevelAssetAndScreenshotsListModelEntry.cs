using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;

namespace Endless.Creator.UI;

public struct UILevelAssetAndScreenshotsListModelEntry
{
	public LevelAsset LevelAsset;

	public List<ScreenshotFileInstances> SelectedScreenshots;

	public UILevelAssetAndScreenshotsListModelEntry(LevelAsset levelAsset)
	{
		LevelAsset = levelAsset;
		SelectedScreenshots = new List<ScreenshotFileInstances>();
	}
}
