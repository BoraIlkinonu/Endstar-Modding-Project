using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x020002AD RID: 685
	public class UIPropToolPanelController : UIItemSelectionToolPanelController<PropTool, PropLibrary.RuntimePropInfo>
	{
		// Token: 0x06000B84 RID: 2948 RVA: 0x00036223 File Offset: 0x00034423
		public override void Deselect()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Deselect", this);
			}
			this.Tool.UpdateSelectedAssetId(SerializableGuid.Empty);
		}
	}
}
