using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPropEntryListCellController : UIBaseListCellController<PropEntry>
{
	[Header("UIPropEntryListCellView")]
	[SerializeField]
	private UIButton selectButton;

	protected override void Start()
	{
		base.Start();
		selectButton.onClick.AddListener(OnSelect);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		throw new NotImplementedException();
	}

	private void OnSelect()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Select");
		}
		base.ListModel.ClearSelected(triggerEvents: false);
		base.ListModel.Select(base.DataIndex, triggerEvents: true);
	}
}
