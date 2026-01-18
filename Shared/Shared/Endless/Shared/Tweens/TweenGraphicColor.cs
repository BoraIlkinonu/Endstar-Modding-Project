using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000C1 RID: 193
	public class TweenGraphicColor : BaseTween
	{
		// Token: 0x06000544 RID: 1348 RVA: 0x0001738C File Offset: 0x0001558C
		public override void Tween()
		{
			if (this.TargetGraphic == null && base.Target)
			{
				base.Target.TryGetComponent<Graphic>(out this.TargetGraphic);
				if (!this.TargetGraphic)
				{
					Debug.LogException(new Exception("TweenGraphicColor has no TargetGraphic!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetGraphic.color;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000545 RID: 1349 RVA: 0x00017408 File Offset: 0x00015608
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			Color color = Color.LerpUnclamped(this.From, this.To, base.GetEasedValue(interpolation));
			this.TargetGraphic.color = color;
		}

		// Token: 0x040002B5 RID: 693
		[Header("TweenGraphicColor")]
		public Color From = Color.black;

		// Token: 0x040002B6 RID: 694
		public Color To = Color.white;

		// Token: 0x040002B7 RID: 695
		[FormerlySerializedAs("TargetImage")]
		public Graphic TargetGraphic;
	}
}
