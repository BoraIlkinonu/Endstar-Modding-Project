using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000289 RID: 649
	public class UILevelScreenshotsView : UIGameObject
	{
		// Token: 0x06000AC2 RID: 2754 RVA: 0x0003284D File Offset: 0x00030A4D
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			this.screenshotFileInstancesListModel.Set(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Screenshots, true);
		}

		// Token: 0x0400090B RID: 2315
		[SerializeField]
		private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;

		// Token: 0x0400090C RID: 2316
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
