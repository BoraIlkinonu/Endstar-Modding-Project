using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000C0 RID: 192
	public class TweenGraphicAlpha : BaseTween
	{
		// Token: 0x06000541 RID: 1345 RVA: 0x00017288 File Offset: 0x00015488
		public override void Tween()
		{
			if (this.TargetGraphic == null && base.Target)
			{
				base.Target.TryGetComponent<Graphic>(out this.TargetGraphic);
				if (!this.TargetGraphic)
				{
					DebugUtility.LogException(new Exception("TweenGraphicAlpha has no TargetGraphic!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetGraphic.color.a;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x00017308 File Offset: 0x00015508
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			float num = Mathf.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetGraphic.color = new Color(this.TargetGraphic.color.r, this.TargetGraphic.color.g, this.TargetGraphic.color.b, num);
		}

		// Token: 0x040002B2 RID: 690
		[Header("TweenGraphicAlpha")]
		public float From;

		// Token: 0x040002B3 RID: 691
		public float To = 1f;

		// Token: 0x040002B4 RID: 692
		public Graphic TargetGraphic;
	}
}
