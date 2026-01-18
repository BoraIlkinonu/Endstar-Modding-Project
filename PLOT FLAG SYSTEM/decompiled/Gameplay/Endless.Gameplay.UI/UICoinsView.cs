using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay.UI;

[RequireComponent(typeof(UIDisplayAndHideHandler))]
public class UICoinsView : UIGameObject
{
	private const string RESOURCE_NAME = "Resource";

	[SerializeField]
	private TextMeshProUGUI amountText;

	[SerializeField]
	private UITooltip tooltip;

	[SerializeField]
	private float incrementDuration = 0.1f;

	[SerializeField]
	private float maxTotalIncrementDuration = 3f;

	[Header("Tweens")]
	[SerializeField]
	private TweenCollection updateAmountTweens;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIDisplayAndHideHandler displayAndHideHandler;

	private bool isDisplayed;

	private int amountDisplayed;

	private int countUpToAmount;

	private IEnumerator countUpToCoroutine;

	private bool isWaitingForInitialDisplay;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		amountText.text = StringUtility.AbbreviateQuantity(amountDisplayed);
		NetworkBehaviourSingleton<ResourceManager>.Instance.OnLocalCoinAmountUpdated.AddListener(CountUpTo);
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(Hide);
		TryGetComponent<UIDisplayAndHideHandler>(out displayAndHideHandler);
		displayAndHideHandler.OnDisplayComplete.AddListener(OnDisplayComplete);
		displayAndHideHandler.OnHideComplete.AddListener(OnHideComplete);
		displayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
	}

	private void Display()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Display");
		}
		if (amountDisplayed == 0)
		{
			isWaitingForInitialDisplay = true;
		}
		displayAndHideHandler.Display();
		isDisplayed = true;
	}

	private void OnDisplayComplete()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisplayComplete");
		}
		if (isWaitingForInitialDisplay)
		{
			isWaitingForInitialDisplay = false;
			if (countUpToCoroutine != null)
			{
				MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(StartCountUpAfterDelay());
			}
		}
	}

	private IEnumerator StartCountUpAfterDelay()
	{
		yield return new WaitForSeconds(incrementDuration);
		if (!(this == null) && countUpToCoroutine != null)
		{
			MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(countUpToCoroutine);
			countUpToCoroutine = null;
		}
	}

	private void CountUpTo(int target)
	{
		if (!isDisplayed)
		{
			Display();
		}
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CountUpTo", target);
		}
		tooltip.SetTooltip(string.Format("{0}: {1:N0}", "Resource", target));
		countUpToAmount = target;
		countUpToCoroutine = CountUpToCoroutine();
		if (!isWaitingForInitialDisplay)
		{
			MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(countUpToCoroutine);
		}
	}

	private IEnumerator CountUpToCoroutine()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CountUpToCoroutine");
		}
		int num = countUpToAmount - amountDisplayed;
		float num2 = incrementDuration * (float)num;
		if (num2 > maxTotalIncrementDuration)
		{
			num2 = Mathf.Clamp(num2, incrementDuration, maxTotalIncrementDuration);
		}
		int increments = Mathf.RoundToInt(num2 / incrementDuration);
		int incrementAmount = Mathf.RoundToInt((float)num / (float)increments);
		while (increments > 0)
		{
			increments--;
			amountDisplayed += incrementAmount;
			amountText.text = StringUtility.AbbreviateQuantity(amountDisplayed);
			updateAmountTweens.Tween();
			yield return new WaitForSeconds(incrementDuration);
		}
		if (amountDisplayed != countUpToAmount)
		{
			amountDisplayed = countUpToAmount;
			amountText.text = StringUtility.AbbreviateQuantity(amountDisplayed);
			updateAmountTweens.Tween();
		}
		countUpToCoroutine = null;
	}

	private void Hide()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Hide");
		}
		isDisplayed = false;
		displayAndHideHandler.Hide();
	}

	private void OnHideComplete()
	{
		if (countUpToCoroutine != null)
		{
			if (verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnHideComplete");
			}
			updateAmountTweens.ForceDone();
			amountDisplayed = 0;
			countUpToAmount = 0;
			amountText.text = StringUtility.AbbreviateQuantity(amountDisplayed);
			if (countUpToCoroutine != null)
			{
				MonoBehaviourSingleton<UICoroutineManager>.Instance.StopCoroutine(countUpToCoroutine);
				countUpToCoroutine = null;
			}
			base.gameObject.SetActive(value: false);
		}
	}
}
