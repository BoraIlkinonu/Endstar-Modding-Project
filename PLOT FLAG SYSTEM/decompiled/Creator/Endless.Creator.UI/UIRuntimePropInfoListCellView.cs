using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIRuntimePropInfoListCellView : UIBaseListCellView<PropLibrary.RuntimePropInfo>
{
	[Header("UIRuntimePropInfoListCellView")]
	[SerializeField]
	private UIRuntimePropInfoPresenter runtimePropInfoPresenter;

	[SerializeField]
	private UIButton removeButton;

	[SerializeField]
	private TweenCollection selectedTweens;

	[SerializeField]
	private TweenCollection unselectedTweens;

	private PropTool propTool;

	public override void OnDespawn()
	{
		base.OnDespawn();
		runtimePropInfoPresenter.Clear();
		selectedTweens.Cancel();
		unselectedTweens.Cancel();
		unselectedTweens.SetToEnd();
		HideSelectedVisuals();
		if ((bool)propTool)
		{
			propTool.OnSelectedAssetChanged.RemoveListener(OnSelectedAssetChanged);
			propTool = null;
		}
	}

	public override void View(UIBaseListView<PropLibrary.RuntimePropInfo> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		runtimePropInfoPresenter.gameObject.SetActive(!IsAddButton);
		removeButton.gameObject.SetActive(!IsAddButton && base.ListModel.UserCanRemove);
		if (!IsAddButton)
		{
			if (!propTool)
			{
				propTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PropTool>();
				propTool.OnSelectedAssetChanged.AddListener(OnSelectedAssetChanged);
			}
			runtimePropInfoPresenter.SetModel(base.Model, triggerOnModelChanged: true);
			VisualizeSelectedState();
		}
	}

	private void OnSelectedAssetChanged(SerializableGuid assetId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSelectedAssetChanged", assetId);
		}
		VisualizeSelectedState();
	}

	private void VisualizeSelectedState()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "VisualizeSelectedState");
		}
		if (!IsAddButton)
		{
			if (((UIRuntimePropInfoListModel)base.ListModel).Context switch
			{
				UIRuntimePropInfoListModel.Contexts.PropTool => (SerializableGuid)base.Model.PropData.AssetID == propTool.SelectedAssetId, 
				UIRuntimePropInfoListModel.Contexts.InventoryLibraryReference => base.ListModel.IsSelected(base.DataIndex), 
				_ => throw new ArgumentOutOfRangeException(), 
			})
			{
				selectedTweens.Tween();
			}
			else
			{
				unselectedTweens.Tween();
			}
		}
	}
}
