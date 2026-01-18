using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000FD RID: 253
	public class UIGameAssetListCellController : UIBaseListCellController<UIGameAsset>
	{
		// Token: 0x1700005A RID: 90
		// (get) Token: 0x0600040F RID: 1039 RVA: 0x0001938D File Offset: 0x0001758D
		private UIGameAssetListView TypedListView
		{
			get
			{
				return (UIGameAssetListView)base.ListView;
			}
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x0001939A File Offset: 0x0001759A
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.Select));
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x000193C0 File Offset: 0x000175C0
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.gameLibraryAssetAdditionModalSource, UIModalManagerStackActions.MaintainStack, new object[]
			{
				base.ListView.ListCellSizeType,
				UIGameAssetTypes.Terrain
			});
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x00019418 File Offset: 0x00017618
		protected override void Select()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}: {2}", "Select", "Model", base.Model), this);
			}
			switch (this.TypedListView.SelectAction)
			{
			case UIGameAssetListView.SelectActions.ListSelect:
				base.Select();
				return;
			case UIGameAssetListView.SelectActions.StaticSelect:
			{
				Action<UIGameAsset> selectAction = UIGameAssetListCellController.SelectAction;
				if (selectAction == null)
				{
					return;
				}
				selectAction(base.Model);
				return;
			}
			case UIGameAssetListView.SelectActions.ViewDetails:
			{
				IGameAssetListModel gameAssetListModel;
				if (!base.ListModel.TryGetComponent<IGameAssetListModel>(out gameAssetListModel))
				{
					DebugUtility.LogError("Could not get IGameAssetListModel from ListModel!", this);
					return;
				}
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.gameLibraryAssetDetailModalSource, UIModalManagerStackActions.MaintainStack, new object[] { base.Model, gameAssetListModel.Context });
				return;
			}
			default:
				DebugUtility.LogNoEnumSupportError<UIGameAssetListView.SelectActions>(this, this.TypedListView.SelectAction);
				return;
			}
		}

		// Token: 0x0400041B RID: 1051
		public static Action<UIGameAsset> SelectAction;

		// Token: 0x0400041C RID: 1052
		[Header("UIGameAssetListCellController")]
		[SerializeField]
		private UIButton selectButton;

		// Token: 0x0400041D RID: 1053
		[SerializeField]
		private UIGameLibraryAssetDetailModalView gameLibraryAssetDetailModalSource;

		// Token: 0x0400041E RID: 1054
		[SerializeField]
		private UIGameLibraryAssetAdditionModalView gameLibraryAssetAdditionModalSource;
	}
}
