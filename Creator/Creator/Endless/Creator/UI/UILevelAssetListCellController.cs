using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000128 RID: 296
	public class UILevelAssetListCellController : UIBaseListCellController<LevelAsset>
	{
		// Token: 0x17000073 RID: 115
		// (get) Token: 0x060004A3 RID: 1187 RVA: 0x0001AD67 File Offset: 0x00018F67
		private UILevelAssetListModel TypedListModel
		{
			get
			{
				return (UILevelAssetListModel)base.ListModel;
			}
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x0001AD74 File Offset: 0x00018F74
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.SelectLevelAsset));
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x0001AD98 File Offset: 0x00018F98
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.newLevelStateModalSource, UIModalManagerStackActions.ClearStack, new object[] { UINewLevelStateModalView.Contexts.Match });
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x0001ADD4 File Offset: 0x00018FD4
		private void SelectLevelAsset()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SelectLevelAsset", Array.Empty<object>());
			}
			UILevelAssetListView uilevelAssetListView = (UILevelAssetListView)base.ListView;
			UILevelAssetListView.SelectActions selectAction = uilevelAssetListView.SelectAction;
			if (selectAction == UILevelAssetListView.SelectActions.OpenLevelLoaderModal)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.levelLoaderModalSource, UIModalManagerStackActions.ClearStack, new object[] { base.Model });
				return;
			}
			if (selectAction != UILevelAssetListView.SelectActions.StartEditMatch)
			{
				DebugUtility.LogNoEnumSupportError<UILevelAssetListView.SelectActions>(this, uilevelAssetListView.SelectAction);
				return;
			}
			MainMenuGameContext mainMenuGameContext = (this.TypedListModel.OpenLevelInAdminMode ? MainMenuGameContext.Admin : MainMenuGameContext.Edit);
			MonoBehaviourSingleton<UIStartMatchHelper>.Instance.TryToStartMatch(this.TypedListModel.Game.AssetID, null, base.Model.AssetID, mainMenuGameContext);
		}

		// Token: 0x04000461 RID: 1121
		[Header("UILevelAssetListCellController")]
		[SerializeField]
		private UIButton selectButton;

		// Token: 0x04000462 RID: 1122
		[SerializeField]
		private UILevelLoaderModalView levelLoaderModalSource;

		// Token: 0x04000463 RID: 1123
		[SerializeField]
		private UINewLevelStateModalView newLevelStateModalSource;
	}
}
