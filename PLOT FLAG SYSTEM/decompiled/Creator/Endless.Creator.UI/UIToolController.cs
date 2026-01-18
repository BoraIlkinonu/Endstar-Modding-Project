using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

[RequireComponent(typeof(UIToolView))]
public class UIToolController : UIGameObject
{
	[SerializeField]
	private ToolType toolType;

	[SerializeField]
	private UIButton setActiveToolButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private EndlessTool endlessTool;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		setActiveToolButton.onClick.AddListener(SetActiveTool);
		endlessTool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(toolType);
	}

	private void SetActiveTool()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetActiveTool");
		}
		if (!MonoBehaviourSingleton<UIWindowManager>.Instance.IsDisplaying || !(MonoBehaviourSingleton<UIWindowManager>.Instance.Displayed.GetType() == typeof(UIScriptWindowView)))
		{
			MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(endlessTool);
		}
	}
}
