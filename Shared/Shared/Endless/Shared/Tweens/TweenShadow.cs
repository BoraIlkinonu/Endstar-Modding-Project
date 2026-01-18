using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000CA RID: 202
	public class TweenShadow : BaseTween
	{
		// Token: 0x0600055F RID: 1375 RVA: 0x00017BBC File Offset: 0x00015DBC
		public override void Tween()
		{
			if (this.TargetShadow == null && base.Target)
			{
				base.Target.TryGetComponent<Shadow>(out this.TargetShadow);
				if (!this.TargetShadow)
				{
					Debug.LogException(new Exception("TweenShadow has no TargetShadow!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = new TweenShadow.ShadowValue(this.TargetShadow);
			}
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x00017C38 File Offset: 0x00015E38
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			Color color = Color.LerpUnclamped(this.From.EffectColor, this.To.EffectColor, base.GetEasedValue(interpolation));
			this.TargetShadow.effectColor = color;
			Vector2 vector = Vector2.LerpUnclamped(this.From.EffectDistance, this.To.EffectDistance, base.GetEasedValue(interpolation));
			this.TargetShadow.effectDistance = vector;
		}

		// Token: 0x040002D1 RID: 721
		[Header("TweenShadow")]
		public TweenShadow.ShadowValue From;

		// Token: 0x040002D2 RID: 722
		public TweenShadow.ShadowValue To;

		// Token: 0x040002D3 RID: 723
		public Shadow TargetShadow;

		// Token: 0x020000CB RID: 203
		[Serializable]
		public struct ShadowValue
		{
			// Token: 0x06000562 RID: 1378 RVA: 0x00017CAA File Offset: 0x00015EAA
			public ShadowValue(Shadow shadow)
			{
				this.EffectColor = shadow.effectColor;
				this.EffectDistance = shadow.effectDistance;
			}

			// Token: 0x06000563 RID: 1379 RVA: 0x00017CC4 File Offset: 0x00015EC4
			public ShadowValue(Color effectColor, Vector2 effectDistance)
			{
				this.EffectColor = effectColor;
				this.EffectDistance = effectDistance;
			}

			// Token: 0x040002D4 RID: 724
			public Color EffectColor;

			// Token: 0x040002D5 RID: 725
			public Vector2 EffectDistance;
		}
	}
}
