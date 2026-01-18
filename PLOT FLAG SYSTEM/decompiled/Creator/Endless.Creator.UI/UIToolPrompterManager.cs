using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIToolPrompterManager : UIMonoBehaviourSingleton<UIToolPrompterManager>
{
	public static Action OnCancel;

	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	[SerializeField]
	private UIHorizontalLayoutGroup horizontalLayoutGroup;

	[SerializeField]
	private UIText toolPromptText;

	[Tooltip("When this tween complete, it will apply the text. Best to do when invisible.")]
	[SerializeField]
	private BaseTween initialDisplayTweenApplyTextOnComplete;

	[SerializeField]
	private TweenCollection displayNextTextTweens;

	[Tooltip("When this tween complete, it will apply the next text change. Best to do when invisible.")]
	[SerializeField]
	private BaseTween displayNextTextTweenSwapTextOnComplete;

	[SerializeField]
	private UIButton cancelButton;

	[SerializeField]
	private UIDisplayAndHideHandler cancelButtonDisplayAndHideHandler;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private string nextText;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		displayAndHideHandler.OnHideComplete.AddListener(ClearToolPromptText);
		initialDisplayTweenApplyTextOnComplete.OnTweenComplete.AddListener(ApplyNextText);
		displayNextTextTweenSwapTextOnComplete.OnTweenComplete.AddListener(ApplyNextText);
		cancelButton.onClick.AddListener(Cancel);
		displayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
		ClearToolPromptText();
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(Hide);
		cancelButtonDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
	}

	public void Display(string text, bool showCancelButton = false)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Display", text, showCancelButton);
		}
		if (!displayAndHideHandler.IsTweeningHide)
		{
			nextText = text;
			if (toolPromptText.IsNullOrEmptyOrWhiteSpace)
			{
				displayAndHideHandler.Display();
				base.gameObject.SetActive(value: true);
			}
			else
			{
				displayNextTextTweens.Tween();
			}
			horizontalLayoutGroup.Spacing = (showCancelButton ? 10 : 0);
			horizontalLayoutGroup.Padding.right = ((!showCancelButton) ? horizontalLayoutGroup.Padding.left : 0);
			if (showCancelButton)
			{
				cancelButtonDisplayAndHideHandler.Display();
			}
			else if (cancelButtonDisplayAndHideHandler.IsDisplaying)
			{
				cancelButtonDisplayAndHideHandler.Hide();
			}
		}
	}

	public void Hide()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Hide");
		}
		if (displayNextTextTweens.IsAnyTweening())
		{
			displayNextTextTweens.Cancel();
		}
		if (initialDisplayTweenApplyTextOnComplete.IsTweening)
		{
			initialDisplayTweenApplyTextOnComplete.Cancel();
		}
		if (displayNextTextTweenSwapTextOnComplete.IsTweening)
		{
			displayNextTextTweenSwapTextOnComplete.Cancel();
		}
		if (cancelButtonDisplayAndHideHandler.IsDisplaying)
		{
			cancelButtonDisplayAndHideHandler.Hide();
		}
		displayAndHideHandler.Hide();
	}

	private void ApplyNextText()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyNextText");
		}
		toolPromptText.Value = nextText;
	}

	private void ClearToolPromptText()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ClearToolPromptText");
		}
		toolPromptText.Clear();
	}

	private void Cancel()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Cancel");
		}
		OnCancel?.Invoke();
	}
}
