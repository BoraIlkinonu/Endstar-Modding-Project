using System;
using Endless.Gameplay.Screenshotting;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200011A RID: 282
	public class UIInMemoryScreenshotListModel : UIBaseLocalFilterableListModel<ScreenshotAPI.InMemoryScreenShot>
	{
		// Token: 0x1700006C RID: 108
		// (get) Token: 0x0600047A RID: 1146 RVA: 0x0001A5D0 File Offset: 0x000187D0
		protected override Comparison<ScreenshotAPI.InMemoryScreenShot> DefaultSort
		{
			get
			{
				return (ScreenshotAPI.InMemoryScreenShot x, ScreenshotAPI.InMemoryScreenShot y) => string.Compare(x.FileName, y.FileName, StringComparison.Ordinal);
			}
		}
	}
}
