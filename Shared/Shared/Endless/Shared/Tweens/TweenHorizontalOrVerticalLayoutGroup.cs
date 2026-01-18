using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000C2 RID: 194
	public class TweenHorizontalOrVerticalLayoutGroup : BaseTween
	{
		// Token: 0x06000547 RID: 1351 RVA: 0x00017460 File Offset: 0x00015660
		public override void Tween()
		{
			if (this.TargetHorizontalOrVerticalLayoutGroup == null && base.Target)
			{
				base.Target.TryGetComponent<HorizontalOrVerticalLayoutGroup>(out this.TargetHorizontalOrVerticalLayoutGroup);
				if (!this.TargetHorizontalOrVerticalLayoutGroup)
				{
					Debug.LogException(new Exception("TweenHorizontalOrVerticalLayoutGroup has no TargetHorizontalOrVerticalLayoutGroup!"), this);
				}
			}
			if (this.FromIsCurrentValue)
			{
				this.From = this.TargetHorizontalOrVerticalLayoutGroup.padding;
			}
			this.TargetHorizontalOrVerticalLayoutGroup.gameObject.TryGetComponent<RectTransform>(out this.targetHorizontalOrVerticalLayoutGroupRectTransform);
			MonoBehaviourSingleton<TweenManager>.Instance.RegisterTween(this);
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x000174F4 File Offset: 0x000156F4
		public override void OnTween(float interpolation)
		{
			base.OnTween(interpolation);
			int num = Mathf.RoundToInt(Mathf.LerpUnclamped((float)this.From.top, (float)this.To.top, base.GetEasedValue(interpolation)));
			int num2 = Mathf.RoundToInt(Mathf.LerpUnclamped((float)this.From.bottom, (float)this.To.bottom, base.GetEasedValue(interpolation)));
			int num3 = Mathf.RoundToInt(Mathf.LerpUnclamped((float)this.From.left, (float)this.To.left, base.GetEasedValue(interpolation)));
			int num4 = Mathf.RoundToInt(Mathf.LerpUnclamped((float)this.From.right, (float)this.To.right, base.GetEasedValue(interpolation)));
			this.TargetHorizontalOrVerticalLayoutGroup.padding.top = num;
			this.TargetHorizontalOrVerticalLayoutGroup.padding.bottom = num2;
			this.TargetHorizontalOrVerticalLayoutGroup.padding.left = num3;
			this.TargetHorizontalOrVerticalLayoutGroup.padding.right = num4;
			LayoutRebuilder.MarkLayoutForRebuild(this.targetHorizontalOrVerticalLayoutGroupRectTransform);
		}

		// Token: 0x040002B8 RID: 696
		[Header("TweenHorizontalOrVerticalLayoutGroup")]
		public RectOffset From = new RectOffset();

		// Token: 0x040002B9 RID: 697
		public RectOffset To = new RectOffset();

		// Token: 0x040002BA RID: 698
		public HorizontalOrVerticalLayoutGroup TargetHorizontalOrVerticalLayoutGroup;

		// Token: 0x040002BB RID: 699
		private RectTransform targetHorizontalOrVerticalLayoutGroupRectTransform;
	}
}
