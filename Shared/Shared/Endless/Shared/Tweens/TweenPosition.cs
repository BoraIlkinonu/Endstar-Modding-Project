using System;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000B8 RID: 184
	public class TweenPosition : BaseTween
	{
		// Token: 0x0600051A RID: 1306 RVA: 0x00016716 File Offset: 0x00014916
		public override void Tween()
		{
			if (this.FromIsCurrentValue)
			{
				this.From = base.Target.transform.position;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x00016744 File Offset: 0x00014944
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			Vector3 vector = Vector3.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			base.Target.transform.position = vector;
		}

		// Token: 0x04000298 RID: 664
		[Header("TweenPosition")]
		public Vector3 From = Vector3.up;

		// Token: 0x04000299 RID: 665
		public Vector3 To = Vector3.zero;
	}
}
