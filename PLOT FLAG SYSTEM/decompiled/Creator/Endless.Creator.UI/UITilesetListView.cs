using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UITilesetListView : UIBaseListView<Tileset>
{
	private PaintingTool paintingTool;

	[field: Header("UITilesetListView")]
	[field: SerializeField]
	public bool ViewPaintingToolActiveTilesetIndexAsSelect { get; private set; }

	protected override void Start()
	{
		base.Start();
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		paintingTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PaintingTool>();
	}

	public bool IsTilesetIndexActiveInPaintingTool(int tilesetIndex)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "IsTilesetIndexActiveInPaintingTool", string.Format("{0}.{1}: {2}", "paintingTool", "ActiveTilesetIndex", paintingTool.ActiveTilesetIndex), tilesetIndex);
		}
		return paintingTool.ActiveTilesetIndex == tilesetIndex;
	}
}
