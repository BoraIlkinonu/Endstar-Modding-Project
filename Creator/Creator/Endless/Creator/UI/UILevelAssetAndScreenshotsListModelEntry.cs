using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;

namespace Endless.Creator.UI
{
	// Token: 0x02000131 RID: 305
	public struct UILevelAssetAndScreenshotsListModelEntry
	{
		// Token: 0x060004CF RID: 1231 RVA: 0x0001B5D9 File Offset: 0x000197D9
		public UILevelAssetAndScreenshotsListModelEntry(LevelAsset levelAsset)
		{
			this.LevelAsset = levelAsset;
			this.SelectedScreenshots = new List<ScreenshotFileInstances>();
		}

		// Token: 0x04000479 RID: 1145
		public LevelAsset LevelAsset;

		// Token: 0x0400047A RID: 1146
		public List<ScreenshotFileInstances> SelectedScreenshots;
	}
}
