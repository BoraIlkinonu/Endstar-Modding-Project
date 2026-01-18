using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UILevelAssetAndScreenshotsListModel : UIBaseLocalFilterableListModel<UILevelAssetAndScreenshotsListModelEntry>
{
	public Dictionary<ScreenshotFileInstances, int> ExteriorSelected { get; private set; } = new Dictionary<ScreenshotFileInstances, int>();

	protected override Comparison<UILevelAssetAndScreenshotsListModelEntry> DefaultSort => (UILevelAssetAndScreenshotsListModelEntry x, UILevelAssetAndScreenshotsListModelEntry y) => string.Compare(x.LevelAsset.Name, y.LevelAsset.Name, StringComparison.Ordinal);

	public override void Clear(bool triggerEvents)
	{
		base.Clear(triggerEvents);
		ExteriorSelected.Clear();
	}

	public void SetExteriorSelected(Dictionary<ScreenshotFileInstances, int> newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetExteriorSelected", newValue.Count);
		}
		ExteriorSelected = newValue;
		UIBaseListModel<UILevelAssetAndScreenshotsListModelEntry>.ModelChangedAction?.Invoke(this);
		base.ModelChangedUnityEvent.Invoke();
	}
}
