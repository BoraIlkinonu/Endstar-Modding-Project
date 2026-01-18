using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200024D RID: 589
	[Serializable]
	public struct RectTransformValue
	{
		// Token: 0x06000EEE RID: 3822 RVA: 0x000402C0 File Offset: 0x0003E4C0
		public RectTransformValue(RectTransform copyFrom)
		{
			this.LocalEulerAngles = copyFrom.localEulerAngles;
			this.LocalPosition = copyFrom.localPosition;
			this.LocalScale = copyFrom.localScale;
			this.AnchorMin = copyFrom.anchorMin;
			this.AnchorMax = copyFrom.anchorMax;
			this.AnchoredPosition = copyFrom.anchoredPosition;
			this.SizeDelta = copyFrom.sizeDelta;
			this.Pivot = copyFrom.pivot;
		}

		// Token: 0x06000EEF RID: 3823 RVA: 0x00040330 File Offset: 0x0003E530
		public void ApplyTo(RectTransform target)
		{
			target.localEulerAngles = this.LocalEulerAngles;
			target.localPosition = this.LocalPosition;
			target.localScale = this.LocalScale;
			target.anchorMin = this.AnchorMin;
			target.anchorMax = this.AnchorMax;
			target.anchoredPosition = this.AnchoredPosition;
			target.sizeDelta = this.SizeDelta;
			target.pivot = this.Pivot;
		}

		// Token: 0x06000EF0 RID: 3824 RVA: 0x000403A0 File Offset: 0x0003E5A0
		public static RectTransformValue Lerp(RectTransformValue from, RectTransformValue to, float interpolation)
		{
			return new RectTransformValue
			{
				LocalEulerAngles = Vector3.Lerp(from.LocalEulerAngles, to.LocalEulerAngles, interpolation),
				LocalPosition = Vector3.Lerp(from.LocalPosition, to.LocalPosition, interpolation),
				LocalScale = Vector3.Lerp(from.LocalScale, to.LocalScale, interpolation),
				AnchorMin = Vector2.Lerp(from.AnchorMin, to.AnchorMin, interpolation),
				AnchorMax = Vector2.Lerp(from.AnchorMax, to.AnchorMax, interpolation),
				AnchoredPosition = Vector2.Lerp(from.AnchoredPosition, to.AnchoredPosition, interpolation),
				SizeDelta = Vector2.Lerp(from.SizeDelta, to.SizeDelta, interpolation),
				Pivot = Vector2.Lerp(from.Pivot, to.Pivot, interpolation)
			};
		}

		// Token: 0x06000EF1 RID: 3825 RVA: 0x00040480 File Offset: 0x0003E680
		public static RectTransformValue LerpUnclamped(RectTransformValue from, RectTransformValue to, float interpolation)
		{
			return new RectTransformValue
			{
				LocalEulerAngles = Vector3.LerpUnclamped(from.LocalEulerAngles, to.LocalEulerAngles, interpolation),
				LocalPosition = Vector3.LerpUnclamped(from.LocalPosition, to.LocalPosition, interpolation),
				LocalScale = Vector3.LerpUnclamped(from.LocalScale, to.LocalScale, interpolation),
				AnchorMin = Vector2.LerpUnclamped(from.AnchorMin, to.AnchorMin, interpolation),
				AnchorMax = Vector2.LerpUnclamped(from.AnchorMax, to.AnchorMax, interpolation),
				AnchoredPosition = Vector2.LerpUnclamped(from.AnchoredPosition, to.AnchoredPosition, interpolation),
				SizeDelta = Vector2.LerpUnclamped(from.SizeDelta, to.SizeDelta, interpolation),
				Pivot = Vector2.LerpUnclamped(from.Pivot, to.Pivot, interpolation)
			};
		}

		// Token: 0x06000EF2 RID: 3826 RVA: 0x00040560 File Offset: 0x0003E760
		public override bool Equals(object target)
		{
			if (target is RectTransformValue)
			{
				RectTransformValue rectTransformValue = (RectTransformValue)target;
				return this.LocalEulerAngles == rectTransformValue.LocalEulerAngles && this.LocalPosition == rectTransformValue.LocalPosition && this.LocalScale == rectTransformValue.LocalScale && this.AnchorMin == rectTransformValue.AnchorMin && this.AnchorMax == rectTransformValue.AnchorMax && this.AnchoredPosition == rectTransformValue.AnchoredPosition && this.SizeDelta == rectTransformValue.SizeDelta && this.Pivot == rectTransformValue.Pivot;
			}
			return false;
		}

		// Token: 0x06000EF3 RID: 3827 RVA: 0x0004061B File Offset: 0x0003E81B
		public static bool operator ==(RectTransformValue leftValue, RectTransformValue rightValue)
		{
			return leftValue.Equals(rightValue);
		}

		// Token: 0x06000EF4 RID: 3828 RVA: 0x00040630 File Offset: 0x0003E830
		public static bool operator !=(RectTransformValue leftValue, RectTransformValue rightValue)
		{
			return !(leftValue == rightValue);
		}

		// Token: 0x06000EF5 RID: 3829 RVA: 0x0004063C File Offset: 0x0003E83C
		public override int GetHashCode()
		{
			return (((((((17 * 23 + this.LocalEulerAngles.GetHashCode()) * 23 + this.LocalPosition.GetHashCode()) * 23 + this.LocalScale.GetHashCode()) * 23 + this.AnchorMin.GetHashCode()) * 23 + this.AnchorMax.GetHashCode()) * 23 + this.AnchoredPosition.GetHashCode()) * 23 + this.SizeDelta.GetHashCode()) * 23 + this.Pivot.GetHashCode();
		}

		// Token: 0x04000968 RID: 2408
		public Vector3 LocalEulerAngles;

		// Token: 0x04000969 RID: 2409
		public Vector3 LocalPosition;

		// Token: 0x0400096A RID: 2410
		public Vector3 LocalScale;

		// Token: 0x0400096B RID: 2411
		public Vector2 AnchorMin;

		// Token: 0x0400096C RID: 2412
		public Vector2 AnchorMax;

		// Token: 0x0400096D RID: 2413
		public Vector2 AnchoredPosition;

		// Token: 0x0400096E RID: 2414
		public Vector2 SizeDelta;

		// Token: 0x0400096F RID: 2415
		public Vector2 Pivot;
	}
}
