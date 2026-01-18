using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000CC RID: 204
	public class TweenSizeDelta : BaseTween
	{
		// Token: 0x06000564 RID: 1380 RVA: 0x00017CD4 File Offset: 0x00015ED4
		public override void Tween()
		{
			if (!this.TargetRectTransform && base.Target)
			{
				base.Target.TryGetComponent<RectTransform>(out this.TargetRectTransform);
				if (!this.TargetRectTransform)
				{
					DebugUtility.LogException(new Exception("TweenSizeDelta has no TargetRectTransform!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = new Vector2(this.TargetRectTransform.rect.width, this.TargetRectTransform.rect.height);
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x00017D70 File Offset: 0x00015F70
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			Vector2 vector = Vector2.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			if (!this.TargetRectTransform)
			{
				return;
			}
			if (this.TweenHorizontal)
			{
				this.TargetRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vector.x);
			}
			if (this.TweenVertical)
			{
				this.TargetRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vector.y);
			}
		}

		// Token: 0x040002D6 RID: 726
		[Header("TweenSizeDelta")]
		public Vector2 From = Vector2.one * 30f;

		// Token: 0x040002D7 RID: 727
		public Vector2 To = Vector2.one * 100f;

		// Token: 0x040002D8 RID: 728
		public RectTransform TargetRectTransform;

		// Token: 0x040002D9 RID: 729
		public bool TweenHorizontal = true;

		// Token: 0x040002DA RID: 730
		public bool TweenVertical = true;
	}
}
