using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000A5 RID: 165
	public abstract class UIAssetWithScreenshotsView<T> : UIAssetReadAndWriteView<T> where T : Asset
	{
		// Token: 0x060002A1 RID: 673 RVA: 0x00011B20 File Offset: 0x0000FD20
		public override void View(T model)
		{
			base.View(model);
			LevelAsset levelAsset = model as LevelAsset;
			if (levelAsset == null)
			{
				Game game = model as Game;
				if (game != null)
				{
					this.DisplayScreenshots(new List<ScreenshotFileInstances>(game.Screenshots));
					return;
				}
			}
			else
			{
				this.DisplayScreenshots(new List<ScreenshotFileInstances>(levelAsset.Screenshots));
			}
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x00011B75 File Offset: 0x0000FD75
		public override void Clear()
		{
			base.Clear();
			this.firstScreenshot.Clear();
			this.screenshotFileInstancesListModel.Clear(true);
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x00011B94 File Offset: 0x0000FD94
		public void DisplayScreenshots(List<ScreenshotFileInstances> screenshotFileInstances)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "DisplayScreenshots", "screenshotFileInstances", screenshotFileInstances.Count), this);
				DebugUtility.DebugEnumerable<ScreenshotFileInstances>("screenshotFileInstances", screenshotFileInstances, null);
			}
			if (screenshotFileInstances.Count > 0)
			{
				this.firstScreenshot.Display(screenshotFileInstances[0]);
			}
			else
			{
				this.firstScreenshot.Clear();
			}
			this.screenshotFileInstancesListModel.Set(screenshotFileInstances, true);
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x00011C10 File Offset: 0x0000FE10
		public void AddScreenshots(List<ScreenshotFileInstances> screenshotFileInstances)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "AddScreenshots", "screenshotFileInstances", screenshotFileInstances.Count), this);
				DebugUtility.DebugEnumerable<ScreenshotFileInstances>("screenshotFileInstances", screenshotFileInstances, null);
			}
			if (this.screenshotFileInstancesListModel.Count == 0)
			{
				this.firstScreenshot.Display(screenshotFileInstances[0]);
			}
			this.screenshotFileInstancesListModel.AddRange(screenshotFileInstances, true);
		}

		// Token: 0x040002D0 RID: 720
		[SerializeField]
		private UIScreenshotView firstScreenshot;

		// Token: 0x040002D1 RID: 721
		[SerializeField]
		private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;
	}
}
