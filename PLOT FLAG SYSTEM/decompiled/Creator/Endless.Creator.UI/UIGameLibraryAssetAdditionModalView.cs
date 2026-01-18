using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Gameplay.LevelEditing;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameLibraryAssetAdditionModalView : UIEscapableModalView
{
	[Header("UIGameLibraryAssetAdditionModalView")]
	[SerializeField]
	private GameAssetSources defaultSource;

	[SerializeField]
	private UIGameLibraryFilterListModel gameLibraryFilterListModel;

	[SerializeField]
	private UIDropdownEnum gameAssetSourceDropdown;

	[SerializeField]
	private SerializableEnumDictionary<GameAssetSources, InterfaceReference<IGameAssetListModel>> gameAssetSourceDictionary = new SerializableEnumDictionary<GameAssetSources, InterfaceReference<IGameAssetListModel>>();

	[SerializeField]
	private UIBaseListView<UIGameAsset>[] listViews = Array.Empty<UIBaseListView<UIGameAsset>>();

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		if (!gameAssetSourceDropdown.Initialized)
		{
			gameAssetSourceDropdown.InitializeDropdownWithEnum(typeof(GameAssetSources));
		}
		ListCellSizeTypes listCellSizeTypes = (ListCellSizeTypes)modalData[0];
		UIGameAssetTypes uIGameAssetTypes = (UIGameAssetTypes)modalData[1];
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "listCellSizeType", listCellSizeTypes), this);
			DebugUtility.Log(string.Format("{0}: {1}", "gameAssetType", uIGameAssetTypes), this);
		}
		UIBaseListView<UIGameAsset>[] array = listViews;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetListCellSizeType(listCellSizeTypes);
		}
		HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
		GameLibrary gameLibrary = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary;
		foreach (TerrainUsage terrainEntry in gameLibrary.TerrainEntries)
		{
			if (terrainEntry.IsActive)
			{
				hashSet.Add(terrainEntry.TerrainAssetReference.AssetID);
			}
		}
		foreach (AssetReference propReference in gameLibrary.PropReferences)
		{
			hashSet.Add(propReference.AssetID);
		}
		foreach (AssetReference audioReference in gameLibrary.AudioReferences)
		{
			hashSet.Add(audioReference.AssetID);
		}
		foreach (DictionaryEntry<GameAssetSources, InterfaceReference<IGameAssetListModel>> item in gameAssetSourceDictionary.Items)
		{
			item.Value.Interface.SetAssetTypeFilter(uIGameAssetTypes, triggerRequest: false);
		}
		gameLibraryFilterListModel.SelectFilters(uIGameAssetTypes);
		ViewSource(gameAssetSourceDropdown.EnumValue);
	}

	public void ViewSource(Enum filter)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewSource", filter);
		}
		GameAssetSources gameAssetSources = (GameAssetSources)(object)filter;
		foreach (DictionaryEntry<GameAssetSources, InterfaceReference<IGameAssetListModel>> item in gameAssetSourceDictionary.Items)
		{
			bool flag = item.Key == gameAssetSources;
			item.Value.Interface.GameObject.transform.parent.parent.gameObject.SetActive(flag);
			if (flag)
			{
				item.Value.Interface.Synchronize();
			}
		}
	}
}
