using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x020002AB RID: 683
	public class UIPaintingToolPanelController : UIItemSelectionToolPanelController<PaintingTool, Tileset>
	{
		// Token: 0x06000B78 RID: 2936 RVA: 0x00035FD8 File Offset: 0x000341D8
		public override void Deselect()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Deselect", this);
			}
			this.Tool.SetActiveTilesetIndex(PaintingTool.NoSelection);
		}
	}
}
