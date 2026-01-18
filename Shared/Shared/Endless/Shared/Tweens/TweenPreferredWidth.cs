using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000C8 RID: 200
	public class TweenPreferredWidth : BaseTween
	{
		// Token: 0x06000559 RID: 1369 RVA: 0x00017A30 File Offset: 0x00015C30
		public override void Tween()
		{
			if (this.TargetLayoutElement == null && base.Target)
			{
				base.Target.TryGetComponent<LayoutElement>(out this.TargetLayoutElement);
				if (!this.TargetLayoutElement)
				{
					Debug.LogException(new Exception("TweenPreferredWidth has no TargetLayoutElement!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetLayoutElement.preferredWidth;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x0600055A RID: 1370 RVA: 0x00017AAC File Offset: 0x00015CAC
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			float num = Mathf.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetLayoutElement.preferredWidth = num;
		}

		// Token: 0x040002CB RID: 715
		[Header("TweenPreferredWidth")]
		public float From;

		// Token: 0x040002CC RID: 716
		public float To = 50f;

		// Token: 0x040002CD RID: 717
		public LayoutElement TargetLayoutElement;
	}
}
