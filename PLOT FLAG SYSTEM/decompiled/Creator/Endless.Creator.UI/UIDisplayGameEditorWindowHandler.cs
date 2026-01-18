using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIDisplayGameEditorWindowHandler : UIGameObject
{
	[SerializeField]
	private Transform windowParent;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(OnToolChange);
	}

	private void OnToolChange(EndlessTool activeTool)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnToolChange", activeTool.ToolTypeName);
		}
		if (activeTool.GetType() == typeof(GameEditorTool))
		{
			UIGameEditorWindowView.Display(windowParent);
		}
		else
		{
			MonoBehaviourSingleton<UIWindowManager>.Instance.CloseAllInstancesOf<UIGameEditorWindowView>();
		}
	}
}
