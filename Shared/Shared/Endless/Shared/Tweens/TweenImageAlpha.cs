using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000C3 RID: 195
	public class TweenImageAlpha : BaseTween
	{
		// Token: 0x0600054A RID: 1354 RVA: 0x00017620 File Offset: 0x00015820
		public override void Tween()
		{
			if (this.TargetImage == null && base.Target)
			{
				base.Target.TryGetComponent<Image>(out this.TargetImage);
				if (!this.TargetImage)
				{
					Debug.LogException(new Exception("TweenImageAlpha has no TargetImage!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetImage.color.a;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x000176A0 File Offset: 0x000158A0
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			float num = Mathf.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			Color color = this.TargetImage.color;
			this.TargetImage.color = new Color(color.r, color.g, color.b, num);
		}

		// Token: 0x040002BC RID: 700
		[Header("TweenImageAlpha")]
		public float From;

		// Token: 0x040002BD RID: 701
		public float To = 1f;

		// Token: 0x040002BE RID: 702
		public Image TargetImage;
	}
}
