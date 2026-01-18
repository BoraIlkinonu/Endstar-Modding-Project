using System;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000408 RID: 1032
	public class SimpleExpandingCrosshair : CrosshairBase
	{
		// Token: 0x060019CA RID: 6602 RVA: 0x00076700 File Offset: 0x00074900
		private void Update()
		{
			float deltaTime = Time.deltaTime;
			if (this.visible)
			{
				if (this.alpha < 1f)
				{
					this.alpha = Mathf.MoveTowards(this.alpha, 1f, deltaTime * this.showHideSpeed);
					this.canvasGroup.alpha = this.alpha;
				}
			}
			else if (this.alpha > 0f)
			{
				this.alpha = Mathf.MoveTowards(this.alpha, 0f, deltaTime * this.showHideSpeed);
				this.canvasGroup.alpha = this.alpha;
				if (this.alpha == 0f && this.currentSpread == 0f)
				{
					base.gameObject.SetActive(false);
				}
			}
			if (this.currentSpread + this.currentMovePenalty > 0f)
			{
				if (this.spreadResetDelay <= 0f)
				{
					this.currentSpread = Mathf.MoveTowards(this.currentSpread, 0f, this.resetSpeed * deltaTime);
				}
				float num = this.resetCurve.Evaluate(1f - ((this.lastNewSpread > 0f) ? (this.currentSpread / this.lastNewSpread) : 0f));
				float num2 = this.startDistance + this.currentMovePenalty * this.distancePerSpreadUnit;
				Vector2 vector = new Vector2(0f, Mathf.Lerp(num2 + this.distancePerSpreadUnit * this.lastNewSpread, num2, num));
				for (int i = 0; i < this.numParts; i++)
				{
					this.SetAnchoredPosition(this.parts[i], vector);
				}
				if (Mathf.Approximately(this.currentSpread, 0f))
				{
					this.currentSpread = 0f;
					this.lastNewSpread = 0f;
					if (this.alpha == 0f)
					{
						base.gameObject.SetActive(false);
					}
				}
			}
			float num3 = 0.05f;
			bool flag = (Time.frameCount - (int)this.lastMoveFrame) * num3 < 0.25f;
			this.currentMovePenalty = Mathf.MoveTowards(this.currentMovePenalty, flag ? (this.movePenalty * this.lastMoveSpeedPercent) : 0f, deltaTime * this.movePenaltyInterpSpeed);
			this.spreadResetDelay = Mathf.MoveTowards(this.spreadResetDelay, 0f, Time.deltaTime);
		}

		// Token: 0x060019CB RID: 6603 RVA: 0x00076940 File Offset: 0x00074B40
		public override void Init(CrosshairSettings settings)
		{
			this.maxSpread = settings.maxSpread;
			this.resetSpeed = settings.resetSpeed;
			this.movePenalty = settings.movementPenalty;
			this.movePenaltyInterpSpeed = this.movePenalty * 4f;
			this.resetCurve = settings.recoilSettleCurve;
			this.partPrefab.gameObject.SetActive(false);
			base.TryGetComponent<RectTransform>(out this.thisRT);
			this.canvasGroup.alpha = this.alpha;
			int num = 360 / this.numParts;
			this.parts = new RectTransform[this.numParts];
			Vector2 vector = new Vector2(0f, this.startDistance);
			for (int i = 0; i < this.numParts; i++)
			{
				RectTransform rectTransform = global::UnityEngine.Object.Instantiate<RectTransform>(this.partPrefab, this.thisRT);
				rectTransform.localEulerAngles = Vector3.forward * (float)(i * num);
				this.SetAnchoredPosition(rectTransform, vector);
				this.parts[i] = rectTransform;
				rectTransform.gameObject.SetActive(true);
			}
		}

		// Token: 0x060019CC RID: 6604 RVA: 0x00076A42 File Offset: 0x00074C42
		public override void OnShow()
		{
			if (!this.visible)
			{
				this.visible = true;
				base.gameObject.SetActive(true);
			}
		}

		// Token: 0x060019CD RID: 6605 RVA: 0x00076A5F File Offset: 0x00074C5F
		public override void OnHide()
		{
			this.visible = false;
		}

		// Token: 0x060019CE RID: 6606 RVA: 0x00076A68 File Offset: 0x00074C68
		public override void SetHidden()
		{
			if (this.visible)
			{
				this.canvasGroup.alpha = (this.alpha = 0f);
				this.visible = false;
				base.gameObject.SetActive(false);
			}
		}

		// Token: 0x060019CF RID: 6607 RVA: 0x00076AAC File Offset: 0x00074CAC
		public override void ApplySpread(float normalRecoilAmount, float shotStrengthMultiplier, float maxRecoilMultiplier, float recoilSettleMultiplier, float recoilSettleDelay)
		{
			this.maxSpread = maxRecoilMultiplier;
			this.resetSpeed = recoilSettleMultiplier;
			this.spreadResetDelay = recoilSettleDelay;
			this.currentSpread = Mathf.Min(this.currentSpread + shotStrengthMultiplier, this.maxSpread);
			this.lastNewSpread = this.currentSpread;
			Vector2 vector = new Vector2(0f, this.startDistance + this.distancePerSpreadUnit * this.currentMovePenalty + this.distancePerSpreadUnit * this.currentSpread);
			for (int i = 0; i < this.numParts; i++)
			{
				this.SetAnchoredPosition(this.parts[i], vector);
			}
		}

		// Token: 0x060019D0 RID: 6608 RVA: 0x00076B43 File Offset: 0x00074D43
		public override void OnMoved(float moveSpeedPercent = 1f)
		{
			this.lastMoveFrame = (uint)Time.frameCount;
			this.lastMoveSpeedPercent = moveSpeedPercent;
		}

		// Token: 0x060019D1 RID: 6609 RVA: 0x00076B57 File Offset: 0x00074D57
		private void SetAnchoredPosition(RectTransform child, Vector2 position)
		{
			child.anchoredPosition = child.localRotation * position;
		}

		// Token: 0x04001470 RID: 5232
		[SerializeField]
		private CanvasGroup canvasGroup;

		// Token: 0x04001471 RID: 5233
		[SerializeField]
		private RectTransform partPrefab;

		// Token: 0x04001472 RID: 5234
		[SerializeField]
		private int numParts = 4;

		// Token: 0x04001473 RID: 5235
		[SerializeField]
		private float startDistance = 50f;

		// Token: 0x04001474 RID: 5236
		[SerializeField]
		private float distancePerSpreadUnit = 50f;

		// Token: 0x04001475 RID: 5237
		[SerializeField]
		private float showHideSpeed = 7f;

		// Token: 0x04001476 RID: 5238
		[NonSerialized]
		private RectTransform thisRT;

		// Token: 0x04001477 RID: 5239
		[NonSerialized]
		private RectTransform[] parts;

		// Token: 0x04001478 RID: 5240
		[NonSerialized]
		private bool visible;

		// Token: 0x04001479 RID: 5241
		[NonSerialized]
		private float alpha;

		// Token: 0x0400147A RID: 5242
		[NonSerialized]
		private float currentSpread;

		// Token: 0x0400147B RID: 5243
		[NonSerialized]
		private float lastNewSpread;

		// Token: 0x0400147C RID: 5244
		[NonSerialized]
		private float maxSpread;

		// Token: 0x0400147D RID: 5245
		[NonSerialized]
		private float resetSpeed;

		// Token: 0x0400147E RID: 5246
		[NonSerialized]
		private float movePenalty;

		// Token: 0x0400147F RID: 5247
		[NonSerialized]
		private uint lastMoveFrame;

		// Token: 0x04001480 RID: 5248
		[NonSerialized]
		private float lastMoveSpeedPercent;

		// Token: 0x04001481 RID: 5249
		[NonSerialized]
		private float currentMovePenalty;

		// Token: 0x04001482 RID: 5250
		[NonSerialized]
		private float movePenaltyInterpSpeed;

		// Token: 0x04001483 RID: 5251
		[NonSerialized]
		private AnimationCurve resetCurve;

		// Token: 0x04001484 RID: 5252
		[NonSerialized]
		private float spreadResetDelay;
	}
}
