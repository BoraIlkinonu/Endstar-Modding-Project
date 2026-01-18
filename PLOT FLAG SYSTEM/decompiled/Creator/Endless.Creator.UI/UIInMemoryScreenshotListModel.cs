using System;
using Endless.Gameplay.Screenshotting;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIInMemoryScreenshotListModel : UIBaseLocalFilterableListModel<ScreenshotAPI.InMemoryScreenShot>
{
	protected override Comparison<ScreenshotAPI.InMemoryScreenShot> DefaultSort => (ScreenshotAPI.InMemoryScreenShot x, ScreenshotAPI.InMemoryScreenShot y) => string.Compare(x.FileName, y.FileName, StringComparison.Ordinal);
}
