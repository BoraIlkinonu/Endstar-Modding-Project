using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000C6 RID: 198
	public class TweenMinWidth : BaseTween
	{
		// Token: 0x06000553 RID: 1363 RVA: 0x000178A0 File Offset: 0x00015AA0
		public override void Tween()
		{
			if (this.TargetLayoutElement == null && base.Target)
			{
				base.Target.TryGetComponent<LayoutElement>(out this.TargetLayoutElement);
				if (!this.TargetLayoutElement)
				{
					Debug.LogException(new Exception("TweenMinWidth has no TargetLayoutElement!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetLayoutElement.minWidth;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000554 RID: 1364 RVA: 0x0001791C File Offset: 0x00015B1C
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			float num = Mathf.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetLayoutElement.minWidth = num;
		}

		// Token: 0x040002C5 RID: 709
		[Header("TweenMinWidth")]
		public float From;

		// Token: 0x040002C6 RID: 710
		public float To = 50f;

		// Token: 0x040002C7 RID: 711
		public LayoutElement TargetLayoutElement;
	}
}
