using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelDestinationSelectionListCellController : UIBaseListCellController<LevelDestination>
{
	[Header("UILevelDestinationSelectionListCellController")]
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
			DebugUtility.LogMethod(this, "OnSelect");
		}
		switch (((UILevelDestinationSelectionListView)base.ListView).SelectionType)
		{
		case UILevelDestinationSelectionListView.SelectionTypes.ApplyToProperty:
			(MonoBehaviourSingleton<UIModalManager>.Instance.SpawnedModal as UILevelDestinationSelectionModalView).ApplyToProperty(base.Model);
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
			break;
		case UILevelDestinationSelectionListView.SelectionTypes.LocalListToggleSelected:
			base.ToggleSelected();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
