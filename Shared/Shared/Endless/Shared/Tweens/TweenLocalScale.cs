using System;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000B7 RID: 183
	public class TweenLocalScale : BaseTween
	{
		// Token: 0x06000517 RID: 1303 RVA: 0x00016684 File Offset: 0x00014884
		public override void Tween()
		{
			if (this.FromIsCurrentValue)
			{
				this.From = base.Target.transform.localScale;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x000166B0 File Offset: 0x000148B0
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			Vector3 vector = Vector3.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			base.Target.transform.localScale = vector;
		}

		// Token: 0x04000296 RID: 662
		[Header("TweenLocalScale")]
		public Vector3 From = Vector3.one;

		// Token: 0x04000297 RID: 663
		public Vector3 To = Vector3.one * 2f;
	}
}
