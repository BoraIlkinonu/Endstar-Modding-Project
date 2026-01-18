using System;
using Endless.Gameplay.Screenshotting;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200011D RID: 285
	public class UIInMemoryScreenshotListView : UIBaseListView<ScreenshotAPI.InMemoryScreenShot>
	{
		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000480 RID: 1152 RVA: 0x0001A621 File Offset: 0x00018821
		// (set) Token: 0x06000481 RID: 1153 RVA: 0x0001A629 File Offset: 0x00018829
		public bool Selectable { get; private set; }
	}
}
