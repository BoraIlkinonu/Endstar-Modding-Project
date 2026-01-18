using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI;

public class UIPropToolPanelController : UIItemSelectionToolPanelController<PropTool, PropLibrary.RuntimePropInfo>
{
	public override void Deselect()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Deselect", this);
		}
		Tool.UpdateSelectedAssetId(SerializableGuid.Empty);
	}
}
