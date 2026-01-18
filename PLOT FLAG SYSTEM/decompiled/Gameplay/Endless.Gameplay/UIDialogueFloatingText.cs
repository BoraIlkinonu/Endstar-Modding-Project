using Endless.Shared.Tweens;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay;

public class UIDialogueFloatingText : UISelfPoolingTween
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TweenLocalPosition localPositionTween;

	[SerializeField]
	private float tweenDistance;

	public void SetupLocalPositionTween()
	{
		localPositionTween.From = localPositionTween.Target.transform.localPosition;
		localPositionTween.To = localPositionTween.From + new Vector3(0f, tweenDistance, 0f);
	}

	public void SetDisplayText(string displayText)
	{
		text.SetText(displayText);
	}
}
