using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000289 RID: 649
	public static class RectTransformUtility
	{
		// Token: 0x0600104C RID: 4172 RVA: 0x0004563C File Offset: 0x0004383C
		public static float OverlapPercentage(RectTransform rectTransformA, RectTransform rectTransformB)
		{
			Vector3[] array = new Vector3[4];
			Vector3[] array2 = new Vector3[4];
			rectTransformA.GetWorldCorners(array);
			rectTransformB.GetWorldCorners(array2);
			for (int i = 0; i < 4; i++)
			{
				array[i] = RectTransformUtility.WorldToScreenPoint(null, array[i]);
				array2[i] = RectTransformUtility.WorldToScreenPoint(null, array2[i]);
			}
			Rect rect = Rect.MinMaxRect(array[0].x, array[0].y, array[2].x, array[2].y);
			Rect rect2 = Rect.MinMaxRect(array2[0].x, array2[0].y, array2[2].x, array2[2].y);
			Rect rect3 = Rect.zero;
			if (rect.Overlaps(rect2))
			{
				rect3 = RectTransformUtility.GetOverlapRect(rect, rect2);
			}
			float num = rect3.width * rect3.height;
			float num2 = rect.width * rect.height;
			return num / num2;
		}

		// Token: 0x0600104D RID: 4173 RVA: 0x00045754 File Offset: 0x00043954
		public static RectTransform Min(RectTransform rectTransformA, RectTransform rectTransformB)
		{
			if (rectTransformB.Area() >= rectTransformA.Area())
			{
				return rectTransformA;
			}
			return rectTransformB;
		}

		// Token: 0x0600104E RID: 4174 RVA: 0x00045767 File Offset: 0x00043967
		public static RectTransform Max(RectTransform rectTransformA, RectTransform rectTransformB)
		{
			if (rectTransformB.Area() <= rectTransformA.Area())
			{
				return rectTransformA;
			}
			return rectTransformB;
		}

		// Token: 0x0600104F RID: 4175 RVA: 0x0004577C File Offset: 0x0004397C
		[return: TupleElementNames(new string[] { "Smaller", "Larger" })]
		public static ValueTuple<RectTransform, RectTransform> SmallerAndLarger(RectTransform rectTransformA, RectTransform rectTransformB)
		{
			RectTransform rectTransform = ((rectTransformB.Area() > rectTransformA.Area()) ? rectTransformB : rectTransformA);
			return new ValueTuple<RectTransform, RectTransform>((rectTransform == rectTransformB) ? rectTransformA : rectTransformB, rectTransform);
		}

		// Token: 0x06001050 RID: 4176 RVA: 0x000457B0 File Offset: 0x000439B0
		private static Rect GetOverlapRect(Rect rectA, Rect rectB)
		{
			float num = Math.Max(rectA.xMin, rectB.xMin);
			float num2 = Math.Min(rectA.xMax, rectB.xMax);
			float num3 = Math.Max(rectA.yMin, rectB.yMin);
			float num4 = Math.Min(rectA.yMax, rectB.yMax);
			float num5 = Math.Max(0f, num2 - num);
			float num6 = Math.Max(0f, num4 - num3);
			return new Rect(num, num3, num5, num6);
		}
	}
}
