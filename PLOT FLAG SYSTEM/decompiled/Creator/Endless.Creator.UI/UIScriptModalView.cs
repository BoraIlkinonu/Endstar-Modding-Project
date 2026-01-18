using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public abstract class UIScriptModalView : UIBaseModalView
{
	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(OnToolChange);
	}

	public override void Close()
	{
		base.Close();
		MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.RemoveListener(OnToolChange);
	}

	private void OnToolChange(EndlessTool activeTool)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnToolChange", activeTool.name);
		}
		if (!(activeTool.GetType() == typeof(PropTool)))
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}
	}
}
