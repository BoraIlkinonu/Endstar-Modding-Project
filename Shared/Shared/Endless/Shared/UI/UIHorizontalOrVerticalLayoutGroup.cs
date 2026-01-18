using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000164 RID: 356
	public abstract class UIHorizontalOrVerticalLayoutGroup : UIBaseLayoutGroup
	{
		// Token: 0x06000879 RID: 2169 RVA: 0x00024138 File Offset: 0x00022338
		public override float GetSpacingSize(Dimension dimension)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetSpacingSize", "dimension", dimension), this);
			}
			return this.Spacing;
		}

		// Token: 0x0600087A RID: 2170 RVA: 0x00024168 File Offset: 0x00022368
		protected void CalcAlongAxis(UIBaseLayoutGroup.Axes axis, bool isVertical)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "CalcAlongAxis", "axis", axis, "isVertical", isVertical }), this);
			}
			float combinedPadding = this.GetCombinedPadding(axis);
			bool controlSize = this.GetControlSize(axis);
			bool useScale = this.GetUseScale(axis);
			bool childForceExpand = this.GetChildForceExpand(axis);
			float num = combinedPadding;
			float num2 = combinedPadding;
			float num3 = 0f;
			bool flag = isVertical ^ (axis == UIBaseLayoutGroup.Axes.Vertical);
			for (int i = 0; i < this.childLayoutItems.Count; i++)
			{
				RectTransform rectTransform = this.childLayoutItems[i];
				float num4;
				float num5;
				float num6;
				UIHorizontalOrVerticalLayoutGroup.GetChildSizes(rectTransform, axis, controlSize, childForceExpand, out num4, out num5, out num6);
				if (useScale)
				{
					float num7 = rectTransform.localScale.Get(axis);
					num4 *= num7;
					num5 *= num7;
					num6 *= num7;
				}
				if (flag)
				{
					num = Mathf.Max(num4 + combinedPadding, num);
					num2 = Mathf.Max(num5 + combinedPadding, num2);
					num3 = Mathf.Max(num6, num3);
				}
				else
				{
					num += num4 + this.Spacing;
					num2 += num5 + this.Spacing;
					num3 += num6;
				}
			}
			if (!flag && this.childLayoutItems.Count > 0)
			{
				num -= this.Spacing;
				num2 -= this.Spacing;
			}
			num2 = Mathf.Max(num, num2);
			base.SetLayoutInputForAxis(num, num2, num3, axis);
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x000242E8 File Offset: 0x000224E8
		protected void SetChildrenAlongAxis(UIBaseLayoutGroup.Axes axis, bool isVertical)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetChildrenAlongAxis", "axis", axis, "isVertical", isVertical }), this);
			}
			float size = base.RectTransform.rect.GetSize(axis);
			bool controlSize = this.GetControlSize(axis);
			bool useScale = this.GetUseScale(axis);
			bool childForceExpand = this.GetChildForceExpand(axis);
			float alignmentOnAxis = base.GetAlignmentOnAxis(axis);
			bool flag = isVertical ^ (axis == UIBaseLayoutGroup.Axes.Vertical);
			int num;
			int num2;
			int num3;
			this.GetIterationParameters(out num, out num2, out num3);
			if (flag)
			{
				this.LayoutChildrenAlongOtherAxis(axis, size, controlSize, useScale, childForceExpand, alignmentOnAxis, num, num2, num3);
				return;
			}
			this.LayoutChildrenAlongMainAxis(axis, size, controlSize, useScale, childForceExpand, alignmentOnAxis, num, num2, num3);
		}

		// Token: 0x0600087C RID: 2172 RVA: 0x000243AC File Offset: 0x000225AC
		private static void GetChildSizes(RectTransform child, UIBaseLayoutGroup.Axes axis, bool controlSize, bool childForceExpand, out float min, out float preferred, out float flexible)
		{
			if (!child)
			{
				min = (preferred = (flexible = 0f));
				return;
			}
			if (!controlSize)
			{
				min = child.sizeDelta.Get(axis);
				preferred = min;
				flexible = 0f;
			}
			else
			{
				min = UILayoutUtility.GetMinSize(child, axis);
				preferred = UILayoutUtility.GetPreferredSize(child, axis);
				flexible = UILayoutUtility.GetFlexibleSize(child, axis);
			}
			if (childForceExpand)
			{
				flexible = Mathf.Max(flexible, 1f);
			}
		}

		// Token: 0x0600087D RID: 2173 RVA: 0x00024429 File Offset: 0x00022629
		private static float CalculateMinMaxLerp(float size, float totalMin, float totalPreferred)
		{
			if (Mathf.Approximately(totalMin, totalPreferred))
			{
				return 0f;
			}
			return Mathf.Clamp01((size - totalMin) / (totalPreferred - totalMin));
		}

		// Token: 0x0600087E RID: 2174 RVA: 0x00024448 File Offset: 0x00022648
		private void LayoutChildrenAlongOtherAxis(UIBaseLayoutGroup.Axes axis, float size, bool controlSize, bool useScale, bool childForceExpand, float alignmentOnAxis, int startIndex, int endIndex, int step)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8}, {9}: {10}, {11}: {12}, {13}: {14}, {15}: {16}, {17}: {18} )", new object[]
				{
					"LayoutChildrenAlongOtherAxis", "axis", axis, "size", size, "controlSize", controlSize, "useScale", useScale, "childForceExpand",
					childForceExpand, "alignmentOnAxis", alignmentOnAxis, "startIndex", startIndex, "endIndex", endIndex, "step", step
				}), this);
			}
			float num = size - this.GetCombinedPadding(axis);
			int num2 = startIndex;
			while (this.ReverseArrangement ? (num2 >= endIndex) : (num2 < endIndex))
			{
				RectTransform rectTransform = this.childLayoutItems[num2];
				float num3;
				float num4;
				float num5;
				UIHorizontalOrVerticalLayoutGroup.GetChildSizes(rectTransform, axis, controlSize, childForceExpand, out num3, out num4, out num5);
				float num6 = (useScale ? rectTransform.localScale.Get(axis) : 1f);
				float num7 = Mathf.Clamp(num, num3, (num5 > 0f) ? size : num4);
				float startOffset = base.GetStartOffset(axis, num7 * num6);
				if (controlSize)
				{
					UIBaseLayoutGroup.SetChildAlongAxisWithScale(rectTransform, axis, startOffset, new float?(num7), num6);
				}
				else
				{
					float num8 = (num7 - rectTransform.sizeDelta.Get(axis)) * alignmentOnAxis;
					UIBaseLayoutGroup.SetChildAlongAxisWithScale(rectTransform, axis, startOffset + num8, null, num6);
				}
				num2 += step;
			}
		}

		// Token: 0x0600087F RID: 2175 RVA: 0x000245FC File Offset: 0x000227FC
		private void LayoutChildrenAlongMainAxis(UIBaseLayoutGroup.Axes axis, float size, bool controlSize, bool useScale, bool childForceExpand, float alignmentOnAxis, int startIndex, int endIndex, int step)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8}, {9}: {10}, {11}: {12}, {13}: {14}, {15}: {16}, {17}: {18} )", new object[]
				{
					"LayoutChildrenAlongMainAxis", "axis", axis, "size", size, "controlSize", controlSize, "useScale", useScale, "childForceExpand",
					childForceExpand, "alignmentOnAxis", alignmentOnAxis, "startIndex", startIndex, "endIndex", endIndex, "step", step
				}), this);
			}
			float num = (float)((axis == UIBaseLayoutGroup.Axes.Horizontal) ? this.Padding.left : this.Padding.top);
			float totalMinSize = base.GetTotalMinSize(axis);
			float totalPreferredSize = base.GetTotalPreferredSize(axis);
			float totalFlexibleSize = base.GetTotalFlexibleSize(axis);
			float num2 = size - totalPreferredSize;
			float num3 = this.CalculateFlexibleMultiplier(axis, totalFlexibleSize, totalPreferredSize, num2, ref num);
			float num4 = UIHorizontalOrVerticalLayoutGroup.CalculateMinMaxLerp(size, totalMinSize, totalPreferredSize);
			int num5 = startIndex;
			while (this.ReverseArrangement ? (num5 >= endIndex) : (num5 < endIndex))
			{
				RectTransform rectTransform = this.childLayoutItems[num5];
				float num6;
				float num7;
				float num8;
				UIHorizontalOrVerticalLayoutGroup.GetChildSizes(rectTransform, axis, controlSize, childForceExpand, out num6, out num7, out num8);
				float num9 = (useScale ? rectTransform.localScale.Get(axis) : 1f);
				float num10 = Mathf.Lerp(num6, num7, num4) + num8 * num3;
				float num11 = num10 * num9;
				if (controlSize)
				{
					UIBaseLayoutGroup.SetChildAlongAxisWithScale(rectTransform, axis, num, new float?(num10), num9);
				}
				else
				{
					float num12 = (num10 - rectTransform.sizeDelta.Get(axis)) * alignmentOnAxis;
					UIBaseLayoutGroup.SetChildAlongAxisWithScale(rectTransform, axis, num + num12, null, num9);
				}
				num += num11 + this.Spacing;
				num5 += step;
			}
		}

		// Token: 0x06000880 RID: 2176 RVA: 0x00024804 File Offset: 0x00022A04
		private float CalculateFlexibleMultiplier(UIBaseLayoutGroup.Axes axis, float totalFlexible, float totalPreferred, float surplusSpace, ref float pos)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8} )", new object[] { "CalculateFlexibleMultiplier", "axis", axis, "totalFlexible", totalFlexible, "totalPreferred", totalPreferred, "surplusSpace", surplusSpace }), this);
			}
			if (surplusSpace <= 0f)
			{
				return 0f;
			}
			if (totalFlexible == 0f)
			{
				pos = base.GetStartOffset(axis, totalPreferred - this.GetCombinedPadding(axis));
				return 0f;
			}
			return surplusSpace / totalFlexible;
		}

		// Token: 0x06000881 RID: 2177 RVA: 0x000248B4 File Offset: 0x00022AB4
		private float GetCombinedPadding(UIBaseLayoutGroup.Axes axis)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetCombinedPadding", "axis", axis), this);
			}
			return (float)((axis == UIBaseLayoutGroup.Axes.Horizontal) ? (this.Padding.left + this.Padding.right) : (this.Padding.top + this.Padding.bottom));
		}

		// Token: 0x06000882 RID: 2178 RVA: 0x0002491D File Offset: 0x00022B1D
		private bool GetControlSize(UIBaseLayoutGroup.Axes axis)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetControlSize", "axis", axis), this);
			}
			if (axis != UIBaseLayoutGroup.Axes.Horizontal)
			{
				return this.ChildControlHeight;
			}
			return this.ChildControlWidth;
		}

		// Token: 0x06000883 RID: 2179 RVA: 0x00024957 File Offset: 0x00022B57
		private bool GetUseScale(UIBaseLayoutGroup.Axes axis)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetUseScale", "axis", axis), this);
			}
			if (axis != UIBaseLayoutGroup.Axes.Horizontal)
			{
				return this.ChildScaleHeight;
			}
			return this.ChildScaleWidth;
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x00024991 File Offset: 0x00022B91
		private bool GetChildForceExpand(UIBaseLayoutGroup.Axes axis)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetChildForceExpand", "axis", axis), this);
			}
			if (axis != UIBaseLayoutGroup.Axes.Horizontal)
			{
				return this.ChildForceExpandHeight;
			}
			return this.ChildForceExpandWidth;
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x000249CC File Offset: 0x00022BCC
		private void GetIterationParameters(out int startIndex, out int endIndex, out int step)
		{
			if (this.verboseLogging)
			{
				Debug.Log("GetIterationParameters", this);
			}
			startIndex = (this.ReverseArrangement ? (this.childLayoutItems.Count - 1) : 0);
			endIndex = (this.ReverseArrangement ? 0 : this.childLayoutItems.Count);
			step = (this.ReverseArrangement ? (-1) : 1);
		}

		// Token: 0x04000554 RID: 1364
		[Header("UIHorizontalOrVerticalLayoutGroup")]
		public float Spacing;

		// Token: 0x04000555 RID: 1365
		public bool ReverseArrangement;

		// Token: 0x04000556 RID: 1366
		public bool ChildControlWidth = true;

		// Token: 0x04000557 RID: 1367
		public bool ChildControlHeight = true;

		// Token: 0x04000558 RID: 1368
		public bool ChildScaleWidth;

		// Token: 0x04000559 RID: 1369
		public bool ChildScaleHeight;

		// Token: 0x0400055A RID: 1370
		public bool ChildForceExpandWidth = true;

		// Token: 0x0400055B RID: 1371
		public bool ChildForceExpandHeight = true;
	}
}
