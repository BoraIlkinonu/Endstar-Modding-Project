using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000163 RID: 355
	public class UIRuntimePropInfoListCellController : UIBaseListCellController<PropLibrary.RuntimePropInfo>
	{
		// Token: 0x0600054B RID: 1355 RVA: 0x0001CA10 File Offset: 0x0001AC10
		protected override void Start()
		{
			base.Start();
			this.propTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PropTool>();
			this.selectButton.onClick.AddListener(new UnityAction(this.SelectProp));
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x0001CA6C File Offset: 0x0001AC6C
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.addPropGameAssetToGameLibraryModalSource, UIModalManagerStackActions.ClearStack, Array.Empty<object>());
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x0001CA9C File Offset: 0x0001AC9C
		private void SelectProp()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SelectProp", Array.Empty<object>());
			}
			UIRuntimePropInfoListModel.Contexts context = ((UIRuntimePropInfoListModel)base.ListModel).Context;
			if (context == UIRuntimePropInfoListModel.Contexts.PropTool)
			{
				this.propTool.UpdateSelectedAssetId(base.Model.PropData.AssetID);
				((UIRuntimePropInfoListView)base.ListView).OnCellSelected.Invoke(base.Model);
				return;
			}
			if (context != UIRuntimePropInfoListModel.Contexts.InventoryLibraryReference)
			{
				throw new ArgumentOutOfRangeException();
			}
			this.ToggleSelected();
		}

		// Token: 0x040004C6 RID: 1222
		[Header("UIRuntimePropInfoListCellView")]
		[SerializeField]
		private UIButton selectButton;

		// Token: 0x040004C7 RID: 1223
		[SerializeField]
		private UIButton removeButton;

		// Token: 0x040004C8 RID: 1224
		[SerializeField]
		private UIAddPropGameAssetToGameLibraryModalView addPropGameAssetToGameLibraryModalSource;

		// Token: 0x040004C9 RID: 1225
		private PropTool propTool;
	}
}
