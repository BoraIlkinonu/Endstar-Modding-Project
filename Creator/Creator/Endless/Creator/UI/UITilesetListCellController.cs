using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000185 RID: 389
	public class UITilesetListCellController : UIBaseListCellController<Tileset>
	{
		// Token: 0x060005B8 RID: 1464 RVA: 0x0001DD1C File Offset: 0x0001BF1C
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.HandleSelect));
			this.paintingTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PaintingTool>();
		}

		// Token: 0x060005B9 RID: 1465 RVA: 0x0001DD50 File Offset: 0x0001BF50
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			ListCellSizeTypes listCellSizeTypes = ListCellSizeTypes.Cozy;
			UIGameAssetTypes uigameAssetTypes = UIGameAssetTypes.Terrain;
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.gameLibraryAssetAdditionModalSource, UIModalManagerStackActions.ClearStack, new object[] { listCellSizeTypes, uigameAssetTypes });
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x0001DDA4 File Offset: 0x0001BFA4
		private void HandleSelect()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "HandleSelect", string.Format("{0}: {1}", "Index", base.Model.Index), Array.Empty<object>());
			}
			if (((UITilesetListModel)base.ListModel).IsPaintTool)
			{
				this.paintingTool.SetActiveTilesetIndex(base.Model.Index);
				return;
			}
			this.Select();
		}

		// Token: 0x04000509 RID: 1289
		[Header("UITilesetListCellController")]
		[SerializeField]
		private UIButton selectButton;

		// Token: 0x0400050A RID: 1290
		[SerializeField]
		private UIGameLibraryAssetAdditionModalView gameLibraryAssetAdditionModalSource;

		// Token: 0x0400050B RID: 1291
		private PaintingTool paintingTool;
	}
}
