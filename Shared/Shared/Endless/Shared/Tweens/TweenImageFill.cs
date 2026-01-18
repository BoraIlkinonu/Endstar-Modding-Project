using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000C4 RID: 196
	public class TweenImageFill : BaseTween
	{
		// Token: 0x0600054D RID: 1357 RVA: 0x00017710 File Offset: 0x00015910
		public override void Tween()
		{
			if (this.TargetImage == null && base.Target)
			{
				base.Target.TryGetComponent<Image>(out this.TargetImage);
				if (!this.TargetImage)
				{
					Debug.LogException(new Exception("TweenImageFill has no TargetImage!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetImage.fillAmount;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x0001778C File Offset: 0x0001598C
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			float num = Mathf.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetImage.fillAmount = num;
		}

		// Token: 0x040002BF RID: 703
		[Header("TweenImageFill")]
		public float From;

		// Token: 0x040002C0 RID: 704
		public float To = 1f;

		// Token: 0x040002C1 RID: 705
		public Image TargetImage;
	}
}
