using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000BD RID: 189
	public class TweenFlexibileHeight : BaseTween
	{
		// Token: 0x06000538 RID: 1336 RVA: 0x00017020 File Offset: 0x00015220
		public override void Tween()
		{
			if (this.TargetLayoutElement == null && base.Target)
			{
				base.Target.TryGetComponent<LayoutElement>(out this.TargetLayoutElement);
				if (!this.TargetLayoutElement)
				{
					Debug.LogException(new Exception("TweenFlexibileHeight has no TargetLayoutElement!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetLayoutElement.flexibleHeight;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000539 RID: 1337 RVA: 0x0001709C File Offset: 0x0001529C
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			float num = Mathf.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetLayoutElement.flexibleHeight = num;
		}

		// Token: 0x040002A9 RID: 681
		[Header("TweenFlexibileHeight")]
		public float From;

		// Token: 0x040002AA RID: 682
		public float To = 1f;

		// Token: 0x040002AB RID: 683
		public LayoutElement TargetLayoutElement;
	}
}
