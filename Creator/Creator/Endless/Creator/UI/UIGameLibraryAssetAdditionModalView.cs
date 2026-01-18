using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Gameplay.LevelEditing;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001B0 RID: 432
	public class UIGameLibraryAssetAdditionModalView : UIEscapableModalView
	{
		// Token: 0x06000666 RID: 1638 RVA: 0x000211FC File Offset: 0x0001F3FC
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			if (!this.gameAssetSourceDropdown.Initialized)
			{
				this.gameAssetSourceDropdown.InitializeDropdownWithEnum(typeof(GameAssetSources));
			}
			ListCellSizeTypes listCellSizeTypes = (ListCellSizeTypes)modalData[0];
			UIGameAssetTypes uigameAssetTypes = (UIGameAssetTypes)modalData[1];
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "listCellSizeType", listCellSizeTypes), this);
				DebugUtility.Log(string.Format("{0}: {1}", "gameAssetType", uigameAssetTypes), this);
			}
			UIBaseListView<UIGameAsset>[] array = this.listViews;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetListCellSizeType(listCellSizeTypes);
			}
			HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
			GameLibrary gameLibrary = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary;
			foreach (TerrainUsage terrainUsage in gameLibrary.TerrainEntries)
			{
				if (terrainUsage.IsActive)
				{
					hashSet.Add(terrainUsage.TerrainAssetReference.AssetID);
				}
			}
			foreach (AssetReference assetReference in gameLibrary.PropReferences)
			{
				hashSet.Add(assetReference.AssetID);
			}
			foreach (AssetReference assetReference2 in gameLibrary.AudioReferences)
			{
				hashSet.Add(assetReference2.AssetID);
			}
			foreach (DictionaryEntry<GameAssetSources, InterfaceReference<IGameAssetListModel>> dictionaryEntry in this.gameAssetSourceDictionary.Items)
			{
				dictionaryEntry.Value.Interface.SetAssetTypeFilter(uigameAssetTypes, false);
			}
			this.gameLibraryFilterListModel.SelectFilters(uigameAssetTypes);
			this.ViewSource(this.gameAssetSourceDropdown.EnumValue);
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x00021428 File Offset: 0x0001F628
		public void ViewSource(Enum filter)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewSource", new object[] { filter });
			}
			GameAssetSources gameAssetSources = (GameAssetSources)filter;
			foreach (DictionaryEntry<GameAssetSources, InterfaceReference<IGameAssetListModel>> dictionaryEntry in this.gameAssetSourceDictionary.Items)
			{
				bool flag = dictionaryEntry.Key == gameAssetSources;
				dictionaryEntry.Value.Interface.GameObject.transform.parent.parent.gameObject.SetActive(flag);
				if (flag)
				{
					dictionaryEntry.Value.Interface.Synchronize();
				}
			}
		}

		// Token: 0x040005BB RID: 1467
		[Header("UIGameLibraryAssetAdditionModalView")]
		[SerializeField]
		private GameAssetSources defaultSource;

		// Token: 0x040005BC RID: 1468
		[SerializeField]
		private UIGameLibraryFilterListModel gameLibraryFilterListModel;

		// Token: 0x040005BD RID: 1469
		[SerializeField]
		private UIDropdownEnum gameAssetSourceDropdown;

		// Token: 0x040005BE RID: 1470
		[SerializeField]
		private SerializableEnumDictionary<GameAssetSources, InterfaceReference<IGameAssetListModel>> gameAssetSourceDictionary = new SerializableEnumDictionary<GameAssetSources, InterfaceReference<IGameAssetListModel>>();

		// Token: 0x040005BF RID: 1471
		[SerializeField]
		private UIBaseListView<UIGameAsset>[] listViews = Array.Empty<UIBaseListView<UIGameAsset>>();
	}
}
