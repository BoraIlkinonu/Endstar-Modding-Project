using Endless.Gameplay.Screenshotting;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInMemoryScreenshotListView : UIBaseListView<ScreenshotAPI.InMemoryScreenShot>
{
	[field: Header("UIInMemoryScreenshotListView")]
	[field: SerializeField]
	public bool Selectable { get; private set; }
}
