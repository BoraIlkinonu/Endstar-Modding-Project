using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200018B RID: 395
	public class UITilesetListView : UIBaseListView<Tileset>
	{
		// Token: 0x17000095 RID: 149
		// (get) Token: 0x060005C9 RID: 1481 RVA: 0x0001DF77 File Offset: 0x0001C177
		// (set) Token: 0x060005CA RID: 1482 RVA: 0x0001DF7F File Offset: 0x0001C17F
		public bool ViewPaintingToolActiveTilesetIndexAsSelect { get; private set; }

		// Token: 0x060005CB RID: 1483 RVA: 0x0001DF88 File Offset: 0x0001C188
		protected override void Start()
		{
			base.Start();
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.paintingTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PaintingTool>();
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x0001DFB8 File Offset: 0x0001C1B8
		public bool IsTilesetIndexActiveInPaintingTool(int tilesetIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "IsTilesetIndexActiveInPaintingTool", string.Format("{0}.{1}: {2}", "paintingTool", "ActiveTilesetIndex", this.paintingTool.ActiveTilesetIndex), new object[] { tilesetIndex });
			}
			return this.paintingTool.ActiveTilesetIndex == tilesetIndex;
		}

		// Token: 0x04000511 RID: 1297
		private PaintingTool paintingTool;
	}
}
