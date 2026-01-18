using System;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000BC RID: 188
	public class TweenCanvasGroupAlpha : BaseTween
	{
		// Token: 0x06000535 RID: 1333 RVA: 0x00016F58 File Offset: 0x00015158
		public override void Tween()
		{
			if (this.TargetCanvasGroup == null && base.Target)
			{
				base.Target.TryGetComponent<CanvasGroup>(out this.TargetCanvasGroup);
				if (!this.TargetCanvasGroup)
				{
					Debug.LogException(new Exception("TweenCanvasGroupAlpha has no TargetCanvasGroup!"), this);
					return;
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetCanvasGroup.alpha;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000536 RID: 1334 RVA: 0x00016FD4 File Offset: 0x000151D4
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			float num = Mathf.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetCanvasGroup.alpha = num;
		}

		// Token: 0x040002A6 RID: 678
		public float From;

		// Token: 0x040002A7 RID: 679
		[Header("TweenCanvasGroupAlpha")]
		public float To = 1f;

		// Token: 0x040002A8 RID: 680
		public CanvasGroup TargetCanvasGroup;
	}
}
