using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000C7 RID: 199
	public class TweenPreferredHeight : BaseTween
	{
		// Token: 0x06000556 RID: 1366 RVA: 0x00017968 File Offset: 0x00015B68
		public override void Tween()
		{
			if (this.TargetLayoutElement == null && base.Target)
			{
				base.Target.TryGetComponent<LayoutElement>(out this.TargetLayoutElement);
				if (!this.TargetLayoutElement)
				{
					Debug.LogException(new Exception("TweenPreferredHeight has no TargetLayoutElement!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetLayoutElement.preferredHeight;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000557 RID: 1367 RVA: 0x000179E4 File Offset: 0x00015BE4
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			float num = Mathf.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetLayoutElement.preferredHeight = num;
		}

		// Token: 0x040002C8 RID: 712
		[Header("TweenPreferredHeight")]
		public float From;

		// Token: 0x040002C9 RID: 713
		public float To = 50f;

		// Token: 0x040002CA RID: 714
		public LayoutElement TargetLayoutElement;
	}
}
