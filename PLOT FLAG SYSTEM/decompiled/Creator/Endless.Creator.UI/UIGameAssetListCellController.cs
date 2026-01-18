using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameAssetListCellController : UIBaseListCellController<UIGameAsset>
{
	public static Action<UIGameAsset> SelectAction;

	[Header("UIGameAssetListCellController")]
	[SerializeField]
	private UIButton selectButton;

	[SerializeField]
	private UIGameLibraryAssetDetailModalView gameLibraryAssetDetailModalSource;

	[SerializeField]
	private UIGameLibraryAssetAdditionModalView gameLibraryAssetAdditionModalSource;

	private UIGameAssetListView TypedListView => (UIGameAssetListView)base.ListView;

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
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(gameLibraryAssetAdditionModalSource, UIModalManagerStackActions.MaintainStack, base.ListView.ListCellSizeType, UIGameAssetTypes.Terrain);
	}

	protected override void Select()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} | {1}: {2}", "Select", "Model", base.Model), this);
		}
		switch (TypedListView.SelectAction)
		{
		case UIGameAssetListView.SelectActions.ListSelect:
			base.Select();
			break;
		case UIGameAssetListView.SelectActions.ViewDetails:
		{
			if (!base.ListModel.TryGetComponent<IGameAssetListModel>(out var component))
			{
				DebugUtility.LogError("Could not get IGameAssetListModel from ListModel!", this);
				break;
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(gameLibraryAssetDetailModalSource, UIModalManagerStackActions.MaintainStack, base.Model, component.Context);
			break;
		}
		case UIGameAssetListView.SelectActions.StaticSelect:
			SelectAction?.Invoke(base.Model);
			break;
		default:
			DebugUtility.LogNoEnumSupportError(this, TypedListView.SelectAction);
			break;
		}
	}
}
