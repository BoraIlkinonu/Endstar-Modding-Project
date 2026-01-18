using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI;

public class UIPaintingToolPanelController : UIItemSelectionToolPanelController<PaintingTool, Tileset>
{
	public override void Deselect()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Deselect", this);
		}
		Tool.SetActiveTilesetIndex(PaintingTool.NoSelection);
	}
}
