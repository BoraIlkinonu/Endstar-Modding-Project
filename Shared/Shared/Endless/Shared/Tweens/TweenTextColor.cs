using System;
using TMPro;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000CD RID: 205
	public class TweenTextColor : BaseTween
	{
		// Token: 0x06000567 RID: 1383 RVA: 0x00017E20 File Offset: 0x00016020
		public override void Tween()
		{
			if (this.TargetText == null && base.Target)
			{
				base.Target.TryGetComponent<TextMeshProUGUI>(out this.TargetText);
				if (!this.TargetText)
				{
					Debug.LogException(new Exception("TweenTextColor has no TargetText!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetText.color;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x00017E9C File Offset: 0x0001609C
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			Color color = Color.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetText.color = color;
		}

		// Token: 0x040002DB RID: 731
		[Header("TweenTextColor")]
		public Color From = Color.black;

		// Token: 0x040002DC RID: 732
		public Color To = Color.white;

		// Token: 0x040002DD RID: 733
		public TextMeshProUGUI TargetText;
	}
}
