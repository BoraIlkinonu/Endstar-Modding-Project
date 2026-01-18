using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000BE RID: 190
	public class TweenFlexibileWidth : BaseTween
	{
		// Token: 0x0600053B RID: 1339 RVA: 0x000170E8 File Offset: 0x000152E8
		public override void Tween()
		{
			if (this.TargetLayoutElement == null && base.Target)
			{
				base.Target.TryGetComponent<LayoutElement>(out this.TargetLayoutElement);
				if (!this.TargetLayoutElement)
				{
					DebugUtility.LogException(new Exception("TweenFlexibileWidth has no TargetLayoutElement!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetLayoutElement.flexibleWidth;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x0600053C RID: 1340 RVA: 0x00017164 File Offset: 0x00015364
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			float num = Mathf.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetLayoutElement.flexibleWidth = num;
		}

		// Token: 0x040002AC RID: 684
		[Header("TweenFlexibileWidth")]
		public float From;

		// Token: 0x040002AD RID: 685
		public float To = 1f;

		// Token: 0x040002AE RID: 686
		public LayoutElement TargetLayoutElement;
	}
}
