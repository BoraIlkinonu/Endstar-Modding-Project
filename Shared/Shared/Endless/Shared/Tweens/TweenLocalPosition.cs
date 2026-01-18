using System;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000B6 RID: 182
	public class TweenLocalPosition : BaseTween
	{
		// Token: 0x06000514 RID: 1300 RVA: 0x000165FA File Offset: 0x000147FA
		public override void Tween()
		{
			if (this.FromIsCurrentValue)
			{
				this.From = base.Target.transform.localPosition;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x00016628 File Offset: 0x00014828
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			Vector3 vector = Vector3.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			base.Target.transform.localPosition = vector;
		}

		// Token: 0x04000294 RID: 660
		[Header("TweenLocalPosition")]
		public Vector3 From = Vector3.up;

		// Token: 0x04000295 RID: 661
		public Vector3 To = Vector3.zero;
	}
}
