using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameLibraryAssetReplacementModalView : UIEscapableModalView
{
	[Header("UIGameLibraryAssetReplacementModalView")]
	[SerializeField]
	private UIGameLibraryListModel gameLibraryListModel;

	[SerializeField]
	private UIBaseListView<UIGameAsset>[] listViews = Array.Empty<UIBaseListView<UIGameAsset>>();

	private ListCellSizeTypes listCellSizeType;

	public UIGameAsset ToRemove { get; private set; }

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		ToRemove = (UIGameAsset)modalData[0];
		listCellSizeType = (ListCellSizeTypes)modalData[1];
		HashSet<SerializableGuid> assetIdsToIgnore = new HashSet<SerializableGuid> { ToRemove.AssetID };
		MonoBehaviourSingleton<UICoroutineManager>.Instance.WaitFramesAndInvoke(SetListCellSizeType);
		gameLibraryListModel.SetAssetIdsToIgnore(assetIdsToIgnore);
		gameLibraryListModel.SetAssetTypeFilter(ToRemove.Type, triggerEvents: false);
		gameLibraryListModel.Synchronize();
	}

	public override void Close()
	{
		base.Close();
		gameLibraryListModel.Clear(triggerEvents: true);
	}

	private void SetListCellSizeType()
	{
		UIBaseListView<UIGameAsset>[] array = listViews;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetListCellSizeType(listCellSizeType);
		}
	}
}
