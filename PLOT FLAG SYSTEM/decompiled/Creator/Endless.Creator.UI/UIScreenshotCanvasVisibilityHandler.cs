using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScreenshotCanvasVisibilityHandler : UIGameObject
{
	[SerializeField]
	private Canvas canvas;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.AddListener(DisableCanvas);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.AddListener(EnableCanvas);
	}

	private void DisableCanvas()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisableCanvas");
		}
		canvas.enabled = false;
	}

	private void EnableCanvas()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "EnableCanvas");
		}
		canvas.enabled = true;
	}
}
