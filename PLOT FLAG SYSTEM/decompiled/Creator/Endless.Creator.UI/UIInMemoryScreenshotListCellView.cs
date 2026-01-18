using Endless.Gameplay.Screenshotting;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInMemoryScreenshotListCellView : UIBaseListCellView<ScreenshotAPI.InMemoryScreenShot>
{
	[SerializeField]
	private UIButton selectButton;

	[field: Header("UIInMemoryScreenshotListCellView")]
	[field: SerializeField]
	public UIScreenshotView Screenshot { get; private set; }

	public override void OnDespawn()
	{
		base.OnDespawn();
		Screenshot.Clear();
	}

	public override void View(UIBaseListView<ScreenshotAPI.InMemoryScreenShot> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		Screenshot.Display(base.Model.Original);
		UIInMemoryScreenshotListView uIInMemoryScreenshotListView = (UIInMemoryScreenshotListView)base.ListView;
		selectButton.interactable = uIInMemoryScreenshotListView.Selectable;
	}
}
