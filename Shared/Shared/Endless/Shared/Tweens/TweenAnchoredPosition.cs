using System;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000BB RID: 187
	public class TweenAnchoredPosition : BaseTween
	{
		// Token: 0x06000531 RID: 1329 RVA: 0x00016E6C File Offset: 0x0001506C
		public void SetTo(Vector2 to)
		{
			this.To = to;
		}

		// Token: 0x06000532 RID: 1330 RVA: 0x00016E78 File Offset: 0x00015078
		public override void Tween()
		{
			if (this.TargetRectTransform == null && base.Target)
			{
				base.Target.TryGetComponent<RectTransform>(out this.TargetRectTransform);
				if (!this.TargetRectTransform)
				{
					Debug.LogException(new NullReferenceException("TweenAnchoredPosition has no TargetRectTransform!"), this);
					return;
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetRectTransform.anchoredPosition;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x00016EF4 File Offset: 0x000150F4
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			Vector2 vector = Vector2.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetRectTransform.anchoredPosition = vector;
		}

		// Token: 0x040002A3 RID: 675
		[Header("TweenAnchoredPosition")]
		public Vector2 From = Vector2.zero;

		// Token: 0x040002A4 RID: 676
		public Vector2 To = Vector2.up * 50f;

		// Token: 0x040002A5 RID: 677
		public RectTransform TargetRectTransform;
	}
}
