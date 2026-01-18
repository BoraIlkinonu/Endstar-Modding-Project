using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001B8 RID: 440
	public class UIGameLibraryAssetReplacementModalView : UIEscapableModalView
	{
		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x0600068A RID: 1674 RVA: 0x00021CA7 File Offset: 0x0001FEA7
		// (set) Token: 0x0600068B RID: 1675 RVA: 0x00021CAF File Offset: 0x0001FEAF
		public UIGameAsset ToRemove { get; private set; }

		// Token: 0x0600068C RID: 1676 RVA: 0x00021CB8 File Offset: 0x0001FEB8
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.ToRemove = (UIGameAsset)modalData[0];
			this.listCellSizeType = (ListCellSizeTypes)modalData[1];
			HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid> { this.ToRemove.AssetID };
			MonoBehaviourSingleton<UICoroutineManager>.Instance.WaitFramesAndInvoke(new Action(this.SetListCellSizeType), 1);
			this.gameLibraryListModel.SetAssetIdsToIgnore(hashSet);
			this.gameLibraryListModel.SetAssetTypeFilter(this.ToRemove.Type, false);
			this.gameLibraryListModel.Synchronize();
		}

		// Token: 0x0600068D RID: 1677 RVA: 0x00021D4A File Offset: 0x0001FF4A
		public override void Close()
		{
			base.Close();
			this.gameLibraryListModel.Clear(true);
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x00021D60 File Offset: 0x0001FF60
		private void SetListCellSizeType()
		{
			UIBaseListView<UIGameAsset>[] array = this.listViews;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetListCellSizeType(this.listCellSizeType);
			}
		}

		// Token: 0x040005E0 RID: 1504
		[Header("UIGameLibraryAssetReplacementModalView")]
		[SerializeField]
		private UIGameLibraryListModel gameLibraryListModel;

		// Token: 0x040005E1 RID: 1505
		[SerializeField]
		private UIBaseListView<UIGameAsset>[] listViews = Array.Empty<UIBaseListView<UIGameAsset>>();

		// Token: 0x040005E2 RID: 1506
		private ListCellSizeTypes listCellSizeType;
	}
}
