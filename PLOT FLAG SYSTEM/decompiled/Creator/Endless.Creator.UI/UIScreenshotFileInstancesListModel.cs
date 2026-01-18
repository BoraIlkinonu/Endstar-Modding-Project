using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScreenshotFileInstancesListModel : UIBaseRearrangeableListModel<ScreenshotFileInstances>
{
	private Dictionary<ScreenshotFileInstances, int> exteriorSelected = new Dictionary<ScreenshotFileInstances, int>();

	[field: SerializeField]
	public ScreenshotTypes ScreenshotType { get; private set; }

	public int ExteriorSelectedCount => exteriorSelected.Count;

	public override void Clear(bool triggerEvents)
	{
		base.Clear(triggerEvents);
		exteriorSelected.Clear();
	}

	public void SetExteriorSelected(Dictionary<ScreenshotFileInstances, int> newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetExteriorSelected", newValue.Count);
		}
		exteriorSelected = newValue;
	}

	public int GetExteriorSelectedValue(ScreenshotFileInstances screenshotFileInstances)
	{
		if (base.SuperVerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetExteriorSelectedValue", screenshotFileInstances);
			DebugUtility.DebugEnumerable("exteriorSelected", exteriorSelected, this);
		}
		if (!exteriorSelected.ContainsKey(screenshotFileInstances))
		{
			return -1;
		}
		return exteriorSelected[screenshotFileInstances];
	}
}
