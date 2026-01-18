using System;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000C9 RID: 201
	public class TweenRectTransform : BaseTween
	{
		// Token: 0x0600055C RID: 1372 RVA: 0x00017AF8 File Offset: 0x00015CF8
		public override void Tween()
		{
			if (this.TargetRectTransform == null && base.Target)
			{
				base.Target.TryGetComponent<RectTransform>(out this.TargetRectTransform);
				if (!this.TargetRectTransform)
				{
					Debug.LogException(new Exception("TweenRectTransform has no TargetRectTransform!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				base.TryGetComponent<RectTransform>(out this.From);
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x0600055D RID: 1373 RVA: 0x00017B70 File Offset: 0x00015D70
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			RectTransformValue.LerpUnclamped(new RectTransformValue(this.From), new RectTransformValue(this.To), base.GetEasedValue(interpolation)).ApplyTo(this.TargetRectTransform);
		}

		// Token: 0x040002CE RID: 718
		[Header("TweenRectTransform")]
		public RectTransform From;

		// Token: 0x040002CF RID: 719
		public RectTransform To;

		// Token: 0x040002D0 RID: 720
		public RectTransform TargetRectTransform;
	}
}
