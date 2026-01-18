using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.UI.Anchors;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIDialogueBubbleAnchor : UIBaseAnchor
{
	public const float SHAKE_DURATION = 0.5f;

	[Header("UIDialogueBubbleAnchor")]
	[SerializeField]
	private float defaultFontSize = 22f;

	[SerializeField]
	private UIContentSizeFitter[] contentSizeFitters = Array.Empty<UIContentSizeFitter>();

	[SerializeField]
	private GameObject displayNameContainer;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TextMeshProUGUI positionText;

	[SerializeField]
	private GameObject nextPrompt;

	[SerializeField]
	private UIAnchoredPositionShaker anchoredPositionShaker;

	[SerializeField]
	private GameObject positionPrompt;

	[SerializeField]
	private TweenCollection onNextTextTweens;

	[SerializeField]
	private UIDialogueFloatingText poolingAlertText;

	[Tooltip("When this tween complete, it will apply the next text change. Best to do when invisible.")]
	[SerializeField]
	private BaseTween onNextTextTweenToSwapTextOnComplete;

	public string NextText { get; private set; }

	public float NextFontSize { get; private set; }

	public static UIDialogueBubbleAnchor CreateInstance(UIDialogueBubbleAnchor prefab, Transform target, RectTransform container, string displayName, Vector3? offset = null)
	{
		UIDialogueBubbleAnchor uIDialogueBubbleAnchor = UIBaseAnchor.CreateAndInitialize(prefab, target, container, offset);
		uIDialogueBubbleAnchor.SetDisplayName(displayName);
		uIDialogueBubbleAnchor.Initialize();
		return uIDialogueBubbleAnchor;
	}

	public void SetDisplayName(string displayName)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetDisplayName", displayName);
		}
		displayNameContainer.SetActive(!displayName.IsNullOrEmptyOrWhiteSpace());
		displayNameText.text = displayName;
	}

	public void Initialize()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize");
		}
		SetFontSize(defaultFontSize);
		onNextTextTweenToSwapTextOnComplete.OnTweenComplete.AddListener(ApplyNextText);
		base.transform.SetAsLastSibling();
	}

	public void DisplayText(string textToDisplay, int currentIndex, int maximumIndex, bool skipOnNextTextTween, float fontSize, bool showIndex = true, bool showInteract = true)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayText", textToDisplay, currentIndex, maximumIndex, skipOnNextTextTween, fontSize, showIndex, showInteract);
		}
		nextPrompt.SetActive(showInteract);
		if (onNextTextTweens.IsAnyTweening())
		{
			onNextTextTweens.Cancel();
		}
		if (showIndex)
		{
			positionText.text = $"{currentIndex + 1}/{maximumIndex}";
		}
		positionPrompt.SetActive(showIndex);
		if (currentIndex >= maximumIndex)
		{
			Close();
		}
		else if (skipOnNextTextTween)
		{
			text.text = textToDisplay;
			SetFontSize(fontSize);
		}
		else
		{
			NextText = textToDisplay;
			NextFontSize = fontSize;
			onNextTextTweens.Tween();
		}
		UIContentSizeFitter[] array = contentSizeFitters;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RequestLayout();
		}
	}

	public void SetFontSize(float newFontSize)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetFontSize", newFontSize);
		}
		text.fontSize = newFontSize;
	}

	public void Shake()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Shake");
		}
		StopShaking();
		anchoredPositionShaker.ShakeDuration = 0.5f;
		anchoredPositionShaker.Shake();
	}

	private void ApplyNextText()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyNextText");
		}
		text.text = NextText;
		SetFontSize(NextFontSize);
		UIContentSizeFitter[] array = contentSizeFitters;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RequestLayout();
		}
		StopShaking();
	}

	private void StopShaking()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "StopShaking");
		}
		if (anchoredPositionShaker.IsShaking)
		{
			anchoredPositionShaker.Stop();
		}
	}

	public void ShowAlert(string displayText)
	{
		UIDialogueFloatingText uIDialogueFloatingText = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(poolingAlertText);
		uIDialogueFloatingText.transform.SetParent(MonoBehaviourSingleton<UIAnchorManager>.Instance.transform);
		uIDialogueFloatingText.transform.SetPositionAndRotation(base.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
		uIDialogueFloatingText.SetDisplayText(displayText);
		uIDialogueFloatingText.SetupLocalPositionTween();
		uIDialogueFloatingText.Tween();
	}

	public override void Close()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Close");
		}
		if (!displayAndHideHandler.IsTweeningHide)
		{
			StopShaking();
			base.Close();
		}
	}
}
