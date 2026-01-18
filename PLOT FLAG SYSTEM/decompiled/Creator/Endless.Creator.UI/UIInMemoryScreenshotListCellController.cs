using System;
using Endless.Gameplay.Screenshotting;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInMemoryScreenshotListCellController : UIBaseListCellController<ScreenshotAPI.InMemoryScreenShot>
{
	[Header("UIInMemoryScreenshotListCellController")]
	[SerializeField]
	private UIButton selectButton;

	protected override void Start()
	{
		base.Start();
		selectButton.onClick.AddListener(ToggleSelected);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		throw new NotImplementedException();
	}
}
