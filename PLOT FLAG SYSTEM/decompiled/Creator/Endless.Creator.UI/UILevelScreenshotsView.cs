using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelScreenshotsView : UIGameObject
{
	[SerializeField]
	private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		screenshotFileInstancesListModel.Set(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Screenshots, triggerEvents: true);
	}
}
