using System.Collections;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIBarkAnchor : UIBaseAnchor
{
	[Header("UIBarkAnchor")]
	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private TextMeshProUGUI barkText;

	[SerializeField]
	private Color defaultBackgroundColor = Color.blue;

	private Coroutine closeCoroutine;

	public static UIBarkAnchor CreateInstance(UIBarkAnchor prefab, Transform target, RectTransform container, string text, float secondsToDisplay = 5f, Color? backgroundColor = null, Vector3? offset = null)
	{
		UIBarkAnchor uIBarkAnchor = UIBaseAnchor.CreateAndInitialize(prefab, target, container, offset);
		uIBarkAnchor.SetText(text);
		uIBarkAnchor.SetBackgroundColor(backgroundColor);
		uIBarkAnchor.SetAutoCloseTime(secondsToDisplay);
		return uIBarkAnchor;
	}

	public void SetText(string text)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetText", text);
		}
		barkText.text = text;
	}

	public void SetBackgroundColor(Color? backgroundColor)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetBackgroundColor", backgroundColor);
		}
		backgroundImage.color = backgroundColor ?? defaultBackgroundColor;
	}

	public void SetAutoCloseTime(float secondsToDisplay)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetAutoCloseTime", secondsToDisplay);
		}
		if (closeCoroutine != null)
		{
			StopCoroutine(closeCoroutine);
		}
		closeCoroutine = StartCoroutine(WaitAndClose(secondsToDisplay));
	}

	protected override void UnregisterAnchorAndHide()
	{
		base.UnregisterAnchorAndHide();
		if (closeCoroutine != null)
		{
			StopCoroutine(closeCoroutine);
			closeCoroutine = null;
		}
	}

	private IEnumerator WaitAndClose(float secondsToWait)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "WaitAndClose", secondsToWait);
		}
		yield return new WaitForSeconds(secondsToWait);
		closeCoroutine = null;
		Close();
	}
}
