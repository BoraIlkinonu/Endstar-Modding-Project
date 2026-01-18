using System;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class SimpleExpandingCrosshair : CrosshairBase
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private RectTransform partPrefab;

	[SerializeField]
	private int numParts = 4;

	[SerializeField]
	private float startDistance = 50f;

	[SerializeField]
	private float distancePerSpreadUnit = 50f;

	[SerializeField]
	private float showHideSpeed = 7f;

	[NonSerialized]
	private RectTransform thisRT;

	[NonSerialized]
	private RectTransform[] parts;

	[NonSerialized]
	private bool visible;

	[NonSerialized]
	private float alpha;

	[NonSerialized]
	private float currentSpread;

	[NonSerialized]
	private float lastNewSpread;

	[NonSerialized]
	private float maxSpread;

	[NonSerialized]
	private float resetSpeed;

	[NonSerialized]
	private float movePenalty;

	[NonSerialized]
	private uint lastMoveFrame;

	[NonSerialized]
	private float lastMoveSpeedPercent;

	[NonSerialized]
	private float currentMovePenalty;

	[NonSerialized]
	private float movePenaltyInterpSpeed;

	[NonSerialized]
	private AnimationCurve resetCurve;

	[NonSerialized]
	private float spreadResetDelay;

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		if (visible)
		{
			if (alpha < 1f)
			{
				alpha = Mathf.MoveTowards(alpha, 1f, deltaTime * showHideSpeed);
				canvasGroup.alpha = alpha;
			}
		}
		else if (alpha > 0f)
		{
			alpha = Mathf.MoveTowards(alpha, 0f, deltaTime * showHideSpeed);
			canvasGroup.alpha = alpha;
			if (alpha == 0f && currentSpread == 0f)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		if (currentSpread + currentMovePenalty > 0f)
		{
			if (spreadResetDelay <= 0f)
			{
				currentSpread = Mathf.MoveTowards(currentSpread, 0f, resetSpeed * deltaTime);
			}
			float t = resetCurve.Evaluate(1f - ((lastNewSpread > 0f) ? (currentSpread / lastNewSpread) : 0f));
			float num = startDistance + currentMovePenalty * distancePerSpreadUnit;
			Vector2 position = new Vector2(0f, Mathf.Lerp(num + distancePerSpreadUnit * lastNewSpread, num, t));
			for (int i = 0; i < numParts; i++)
			{
				SetAnchoredPosition(parts[i], position);
			}
			if (Mathf.Approximately(currentSpread, 0f))
			{
				currentSpread = 0f;
				lastNewSpread = 0f;
				if (alpha == 0f)
				{
					base.gameObject.SetActive(value: false);
				}
			}
		}
		float num2 = 0.05f;
		bool flag = (float)((uint)Time.frameCount - lastMoveFrame) * num2 < 0.25f;
		currentMovePenalty = Mathf.MoveTowards(currentMovePenalty, flag ? (movePenalty * lastMoveSpeedPercent) : 0f, deltaTime * movePenaltyInterpSpeed);
		spreadResetDelay = Mathf.MoveTowards(spreadResetDelay, 0f, Time.deltaTime);
	}

	public override void Init(CrosshairSettings settings)
	{
		maxSpread = settings.maxSpread;
		resetSpeed = settings.resetSpeed;
		movePenalty = settings.movementPenalty;
		movePenaltyInterpSpeed = movePenalty * 4f;
		resetCurve = settings.recoilSettleCurve;
		partPrefab.gameObject.SetActive(value: false);
		TryGetComponent<RectTransform>(out thisRT);
		canvasGroup.alpha = alpha;
		int num = 360 / numParts;
		parts = new RectTransform[numParts];
		Vector2 position = new Vector2(0f, startDistance);
		for (int i = 0; i < numParts; i++)
		{
			RectTransform rectTransform = UnityEngine.Object.Instantiate(partPrefab, thisRT);
			rectTransform.localEulerAngles = Vector3.forward * (i * num);
			SetAnchoredPosition(rectTransform, position);
			parts[i] = rectTransform;
			rectTransform.gameObject.SetActive(value: true);
		}
	}

	public override void OnShow()
	{
		if (!visible)
		{
			visible = true;
			base.gameObject.SetActive(value: true);
		}
	}

	public override void OnHide()
	{
		visible = false;
	}

	public override void SetHidden()
	{
		if (visible)
		{
			canvasGroup.alpha = (alpha = 0f);
			visible = false;
			base.gameObject.SetActive(value: false);
		}
	}

	public override void ApplySpread(float normalRecoilAmount, float shotStrengthMultiplier, float maxRecoilMultiplier, float recoilSettleMultiplier, float recoilSettleDelay)
	{
		maxSpread = maxRecoilMultiplier;
		resetSpeed = recoilSettleMultiplier;
		spreadResetDelay = recoilSettleDelay;
		currentSpread = Mathf.Min(currentSpread + shotStrengthMultiplier, maxSpread);
		lastNewSpread = currentSpread;
		Vector2 position = new Vector2(0f, startDistance + distancePerSpreadUnit * currentMovePenalty + distancePerSpreadUnit * currentSpread);
		for (int i = 0; i < numParts; i++)
		{
			SetAnchoredPosition(parts[i], position);
		}
	}

	public override void OnMoved(float moveSpeedPercent = 1f)
	{
		lastMoveFrame = (uint)Time.frameCount;
		lastMoveSpeedPercent = moveSpeedPercent;
	}

	private void SetAnchoredPosition(RectTransform child, Vector2 position)
	{
		child.anchoredPosition = child.localRotation * position;
	}
}
