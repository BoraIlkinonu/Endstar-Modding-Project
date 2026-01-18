using System;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000BF RID: 191
	public class TweenFlexibleLayoutDimension : BaseTween
	{
		// Token: 0x0600053E RID: 1342 RVA: 0x000171B0 File Offset: 0x000153B0
		public override void Tween()
		{
			if (this.TargetLayoutElement == null && base.Target)
			{
				base.Target.TryGetComponent<UILayoutElement>(out this.TargetLayoutElement);
				if (!this.TargetLayoutElement)
				{
					Debug.LogException(new Exception("TweenFlexibleLayoutDimension has no TargetLayoutElement!"), this);
					return;
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetLayoutElement.FlexibleHeightLayoutDimension.ExplicitValue;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x0600053F RID: 1343 RVA: 0x00017234 File Offset: 0x00015434
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			float num = Mathf.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetLayoutElement.FlexibleHeightLayoutDimension.ExplicitValue = num;
		}

		// Token: 0x040002AF RID: 687
		[Header("TweenFlexibleLayoutDimension")]
		public float From;

		// Token: 0x040002B0 RID: 688
		public float To = 1f;

		// Token: 0x040002B1 RID: 689
		public UILayoutElement TargetLayoutElement;
	}
}
