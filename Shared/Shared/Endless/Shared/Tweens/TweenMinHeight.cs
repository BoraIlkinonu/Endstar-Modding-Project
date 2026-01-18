using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000C5 RID: 197
	public class TweenMinHeight : BaseTween
	{
		// Token: 0x06000550 RID: 1360 RVA: 0x000177D8 File Offset: 0x000159D8
		public override void Tween()
		{
			if (this.TargetLayoutElement == null && base.Target)
			{
				base.Target.TryGetComponent<LayoutElement>(out this.TargetLayoutElement);
				if (!this.TargetLayoutElement)
				{
					Debug.LogException(new Exception("TweenMinHeight has no TargetLayoutElement!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetLayoutElement.minHeight;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x00017854 File Offset: 0x00015A54
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			float num = Mathf.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetLayoutElement.minHeight = num;
		}

		// Token: 0x040002C2 RID: 706
		[Header("TweenMinHeight")]
		public float From;

		// Token: 0x040002C3 RID: 707
		public float To = 50f;

		// Token: 0x040002C4 RID: 708
		public LayoutElement TargetLayoutElement;
	}
}
