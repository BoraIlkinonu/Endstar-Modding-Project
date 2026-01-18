using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIRuntimePropInfoListCellController : UIBaseListCellController<PropLibrary.RuntimePropInfo>
{
	[Header("UIRuntimePropInfoListCellView")]
	[SerializeField]
	private UIButton selectButton;

	[SerializeField]
	private UIButton removeButton;

	[SerializeField]
	private UIAddPropGameAssetToGameLibraryModalView addPropGameAssetToGameLibraryModalSource;

	private PropTool propTool;

	protected override void Start()
	{
		base.Start();
		propTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PropTool>();
		selectButton.onClick.AddListener(SelectProp);
		removeButton.onClick.AddListener(Remove);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(addPropGameAssetToGameLibraryModalSource, UIModalManagerStackActions.ClearStack);
	}

	private void SelectProp()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SelectProp");
		}
		switch (((UIRuntimePropInfoListModel)base.ListModel).Context)
		{
		case UIRuntimePropInfoListModel.Contexts.PropTool:
			propTool.UpdateSelectedAssetId(base.Model.PropData.AssetID);
			((UIRuntimePropInfoListView)base.ListView).OnCellSelected.Invoke(base.Model);
			break;
		case UIRuntimePropInfoListModel.Contexts.InventoryLibraryReference:
			ToggleSelected();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
