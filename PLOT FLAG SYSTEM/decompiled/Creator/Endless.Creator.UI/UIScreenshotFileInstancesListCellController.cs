using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScreenshotFileInstancesListCellController : UIBaseListCellController<ScreenshotFileInstances>
{
	[Header("UIScreenshotFileInstancesListCellController")]
	[SerializeField]
	private UIButton selectButton;

	protected override void Start()
	{
		base.Start();
		selectButton.onClick.AddListener(Select);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		throw new NotImplementedException();
	}

	protected override void Select()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Select");
		}
		base.ListModel.ToggleSelected(base.DataIndex, triggerEvents: true);
	}
}
