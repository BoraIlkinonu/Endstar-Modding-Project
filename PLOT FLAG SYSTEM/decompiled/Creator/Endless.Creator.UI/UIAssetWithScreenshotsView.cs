using System.Collections.Generic;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public abstract class UIAssetWithScreenshotsView<T> : UIAssetReadAndWriteView<T> where T : Asset
{
	[SerializeField]
	private UIScreenshotView firstScreenshot;

	[SerializeField]
	private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;

	public override void View(T model)
	{
		base.View(model);
		if (!(model is LevelAsset levelAsset))
		{
			if (model is Game game)
			{
				DisplayScreenshots(new List<ScreenshotFileInstances>(game.Screenshots));
			}
		}
		else
		{
			DisplayScreenshots(new List<ScreenshotFileInstances>(levelAsset.Screenshots));
		}
	}

	public override void Clear()
	{
		base.Clear();
		firstScreenshot.Clear();
		screenshotFileInstancesListModel.Clear(triggerEvents: true);
	}

	public void DisplayScreenshots(List<ScreenshotFileInstances> screenshotFileInstances)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "DisplayScreenshots", "screenshotFileInstances", screenshotFileInstances.Count), this);
			DebugUtility.DebugEnumerable("screenshotFileInstances", screenshotFileInstances);
		}
		if (screenshotFileInstances.Count > 0)
		{
			firstScreenshot.Display(screenshotFileInstances[0]);
		}
		else
		{
			firstScreenshot.Clear();
		}
		screenshotFileInstancesListModel.Set(screenshotFileInstances, triggerEvents: true);
	}

	public void AddScreenshots(List<ScreenshotFileInstances> screenshotFileInstances)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "AddScreenshots", "screenshotFileInstances", screenshotFileInstances.Count), this);
			DebugUtility.DebugEnumerable("screenshotFileInstances", screenshotFileInstances);
		}
		if (screenshotFileInstancesListModel.Count == 0)
		{
			firstScreenshot.Display(screenshotFileInstances[0]);
		}
		screenshotFileInstancesListModel.AddRange(screenshotFileInstances, triggerEvents: true);
	}
}
