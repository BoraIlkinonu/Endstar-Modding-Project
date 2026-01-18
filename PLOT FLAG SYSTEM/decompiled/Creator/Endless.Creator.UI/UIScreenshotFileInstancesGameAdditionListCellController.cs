using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScreenshotFileInstancesGameAdditionListCellController : UIBaseListCellController<ScreenshotFileInstances>
{
	public static Action<ScreenshotFileInstances> SelectAction;

	[Header("UIScreenshotFileInstancesGameAdditionListCellController")]
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

	protected override void ToggleSelected()
	{
		base.ToggleSelected();
		SelectAction?.Invoke(base.Model);
	}
}
