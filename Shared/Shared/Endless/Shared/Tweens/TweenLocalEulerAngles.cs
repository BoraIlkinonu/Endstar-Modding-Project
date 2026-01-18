using System;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000B5 RID: 181
	public class TweenLocalEulerAngles : BaseTween
	{
		// Token: 0x06000511 RID: 1297 RVA: 0x00016569 File Offset: 0x00014769
		public override void Tween()
		{
			if (this.FromIsCurrentValue)
			{
				this.From = base.Target.transform.localEulerAngles;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000512 RID: 1298 RVA: 0x00016594 File Offset: 0x00014794
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			Vector3 vector = Vector3.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			base.Target.transform.localEulerAngles = vector;
		}

		// Token: 0x04000292 RID: 658
		[Header("TweenLocalEulerAngles")]
		public Vector3 From = Vector3.zero;

		// Token: 0x04000293 RID: 659
		public Vector3 To = Vector3.right * 180f;
	}
}
