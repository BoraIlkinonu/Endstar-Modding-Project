using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScreenshotToolVisibilityHandler : UIGameObject
{
	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	[SerializeField]
	private CanvasGroup canvasGroup;

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
		if (activeTool.GetType() == typeof(ScreenshotTool))
		{
			displayAndHideHandler.Hide();
			canvasGroup.blocksRaycasts = false;
		}
		else
		{
			displayAndHideHandler.Display();
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		}
	}
}
