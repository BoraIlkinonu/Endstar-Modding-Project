using System.Collections;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScreenshotToolController : UIGameObject, IValidatable
{
	[SerializeField]
	private UIButton closeButton;

	[SerializeField]
	private UIToggle hideUiToggle;

	[SerializeField]
	private UIToggle hideCharacterToggle;

	[SerializeField]
	private UIButton screenshotButton;

	[SerializeField]
	private TweenCollection screenshotEffectTweens;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIScreenshotToolView view;

	private ScreenshotTool screenshotTool;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		TryGetComponent<UIScreenshotToolView>(out view);
		screenshotTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<ScreenshotTool>();
		closeButton.onClick.AddListener(Close);
		hideUiToggle.OnChange.AddListener(SetHideUi);
		hideCharacterToggle.OnChange.AddListener(SetHideCharacter);
		screenshotButton.onClick.AddListener(TakeScreenshot);
	}

	[ContextMenu("Validate")]
	public void Validate()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Validate");
		}
		DebugUtility.DebugHasComponent<UIScreenshotToolView>(base.gameObject);
		screenshotEffectTweens.ValidateForNumberOfTweens();
	}

	private void Close()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Close");
		}
		MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
	}

	private void SetHideUi(bool hide)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetHideUi", hide);
		}
		screenshotTool.SetHideUi(hide);
	}

	private void SetHideCharacter(bool hide)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetHideCharacter", hide);
		}
		screenshotTool.SetHideCharacter(hide);
	}

	private void TakeScreenshot()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "TakeScreenshot");
		}
		view.EnableCanvas(enabled: false);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.RequestSimpleScreenshot(screenshotTool.ScreenshotOptions);
		StartCoroutine(TakeScreenshotCoroutine());
	}

	private IEnumerator TakeScreenshotCoroutine()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "TakeScreenshotCoroutine");
		}
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		view.EnableCanvas(enabled: true);
		screenshotEffectTweens.Tween();
	}
}
