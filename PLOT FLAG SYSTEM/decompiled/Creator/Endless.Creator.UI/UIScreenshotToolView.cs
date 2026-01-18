using Endless.Creator.LevelEditing.Runtime;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Screenshotting;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScreenshotToolView : UIGameObject, IRoleInteractable, IValidatable
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private UIToggle characterVisibilityToggle;

	[SerializeField]
	private IntVariable screenshotLimit;

	[SerializeField]
	private TextMeshProUGUI screenshotCountText;

	[SerializeField]
	private TweenCollection screenshotCountChangedTweens;

	[SerializeField]
	private UIButton screenshotButton;

	[SerializeField]
	private UIAddScreenshotsToLevelModalView screenshotReviewModalView;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIDisplayAndHideHandler displayAndHideHandler;

	private ScreenshotTool screenshotTool;

	private int levelScreenshotCount;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		TryGetComponent<UIDisplayAndHideHandler>(out displayAndHideHandler);
		displayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
		screenshotTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<ScreenshotTool>();
		MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(OnToolChange);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.AddListener(DisplayScreenshotCount);
	}

	[ContextMenu("Validate")]
	public void Validate()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Validate");
		}
		DebugUtility.DebugHasComponent<UIDisplayAndHideHandler>(base.gameObject);
	}

	public void EnableCanvas(bool enabled)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "EnableCanvas", enabled);
		}
		canvas.enabled = enabled;
	}

	public void SetLocalUserCanInteract(bool localUserCanInteract)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetLocalUserCanInteract", localUserCanInteract);
		}
		screenshotButton.interactable = localUserCanInteract;
	}

	private void OnToolChange(EndlessTool activeTool)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnToolChange", activeTool.name);
		}
		if (activeTool.GetType() == typeof(ScreenshotTool))
		{
			if (!displayAndHideHandler.IsDisplaying)
			{
				Display();
			}
		}
		else if (displayAndHideHandler.IsDisplaying)
		{
			Hide();
		}
	}

	private async void Display()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Display");
		}
		displayAndHideHandler.Display();
		characterVisibilityToggle.SetIsOn(screenshotTool.ScreenshotOptions.HideCharacter, suppressOnChange: true);
		screenshotCountText.text = "Loading...";
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID);
		if (graphQlResult.HasErrors)
		{
			ErrorHandler.HandleError(ErrorCodes.UIScreenshotToolView_GetLevel, graphQlResult.GetErrorMessage());
			return;
		}
		LevelState levelState = LevelStateLoader.Load(graphQlResult.GetDataMember().ToString());
		levelScreenshotCount = levelState.Screenshots.Count;
		DisplayScreenshotCount();
	}

	private void Hide()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Hide");
		}
		displayAndHideHandler.Hide();
		if (MonoBehaviourSingleton<ScreenshotAPI>.Instance.InMemoryScreenshots.Count > 0)
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(screenshotReviewModalView, UIModalManagerStackActions.ClearStack);
		}
	}

	private void DisplayScreenshotCount()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayScreenshotCount");
		}
		int num = Mathf.Clamp(screenshotLimit.Value - levelScreenshotCount, 0, screenshotLimit.Value);
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "levelScreenshotCount", levelScreenshotCount), this);
			DebugUtility.Log(string.Format("{0}: {1}", "unusedScreenshots", num), this);
		}
		screenshotCountText.text = $"{MonoBehaviourSingleton<ScreenshotAPI>.Instance.InMemoryScreenshots.Count}/{num}";
		if (MonoBehaviourSingleton<ScreenshotAPI>.Instance.InMemoryScreenshots.Count > 0)
		{
			screenshotCountChangedTweens.Tween();
		}
	}
}
