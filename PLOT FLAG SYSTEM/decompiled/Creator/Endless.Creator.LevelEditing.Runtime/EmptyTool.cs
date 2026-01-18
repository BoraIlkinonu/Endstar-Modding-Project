using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine.InputSystem;

namespace Endless.Creator.LevelEditing.Runtime;

public class EmptyTool : EndlessTool
{
	private bool secretModeEnabled;

	public override ToolType ToolType => ToolType.Empty;

	public override void HandleDeselected()
	{
	}

	public override void HandleSelected()
	{
		if ((bool)base.UIToolPrompter)
		{
			base.UIToolPrompter.Hide();
		}
	}

	public override void ToolPressed()
	{
		base.ToolPressed();
		if (!secretModeEnabled)
		{
			return;
		}
		LineCastHit activeLineCastResult = base.ActiveLineCastResult;
		if (!activeLineCastResult.IntersectionOccured)
		{
			return;
		}
		Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition);
		if (cellFromCoordinate is TerrainCell terrainCell)
		{
			PaintingTool paintingTool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(ToolType.Painting) as PaintingTool;
			if ((bool)paintingTool)
			{
				MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(paintingTool);
				paintingTool.SetActiveTilesetIndex(terrainCell.TilesetIndex);
			}
		}
		else if (cellFromCoordinate is PropCell propCell)
		{
			InspectorTool inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(ToolType.Inspector) as InspectorTool;
			if ((bool)inspectorTool)
			{
				MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(inspectorTool);
				inspectorTool.SetInspectedId(propCell.InstanceId);
				inspectorTool.SetStateToInspect();
			}
		}
	}

	public override void UpdateTool()
	{
		base.UpdateTool();
		if (EndlessInput.GetKeyDown(Key.B))
		{
			secretModeEnabled = !secretModeEnabled;
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "EmptyTool";
	}
}
