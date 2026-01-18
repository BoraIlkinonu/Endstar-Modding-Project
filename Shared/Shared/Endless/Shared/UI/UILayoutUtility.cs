using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200016F RID: 367
	public static class UILayoutUtility
	{
		// Token: 0x060008E6 RID: 2278 RVA: 0x000260D5 File Offset: 0x000242D5
		public static float GetMinSize(RectTransform rect, UIBaseLayoutGroup.Axes axis)
		{
			if (axis != UIBaseLayoutGroup.Axes.Horizontal)
			{
				return UILayoutUtility.GetMinHeight(rect);
			}
			return UILayoutUtility.GetMinWidth(rect);
		}

		// Token: 0x060008E7 RID: 2279 RVA: 0x000260E7 File Offset: 0x000242E7
		public static float GetPreferredSize(RectTransform rect, UIBaseLayoutGroup.Axes axis)
		{
			if (axis != UIBaseLayoutGroup.Axes.Horizontal)
			{
				return UILayoutUtility.GetPreferredHeight(rect);
			}
			return UILayoutUtility.GetPreferredWidth(rect);
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x000260F9 File Offset: 0x000242F9
		public static float GetFlexibleSize(RectTransform rect, UIBaseLayoutGroup.Axes axis)
		{
			if (axis != UIBaseLayoutGroup.Axes.Horizontal)
			{
				return UILayoutUtility.GetFlexibleHeight(rect);
			}
			return UILayoutUtility.GetFlexibleWidth(rect);
		}

		// Token: 0x060008E9 RID: 2281 RVA: 0x0002610B File Offset: 0x0002430B
		public static float GetMinWidth(RectTransform rect)
		{
			return UILayoutUtility.GetLayoutProperty(rect, (ILayoutElement e) => e.minWidth, 0f);
		}

		// Token: 0x060008EA RID: 2282 RVA: 0x00026138 File Offset: 0x00024338
		public static float GetPreferredWidth(RectTransform rect)
		{
			return Mathf.Max(UILayoutUtility.GetLayoutProperty(rect, (ILayoutElement e) => e.minWidth, 0f), UILayoutUtility.GetLayoutProperty(rect, (ILayoutElement e) => e.preferredWidth, 0f));
		}

		// Token: 0x060008EB RID: 2283 RVA: 0x0002619E File Offset: 0x0002439E
		public static float GetFlexibleWidth(RectTransform rect)
		{
			return UILayoutUtility.GetLayoutProperty(rect, (ILayoutElement e) => e.flexibleWidth, 0f);
		}

		// Token: 0x060008EC RID: 2284 RVA: 0x000261CA File Offset: 0x000243CA
		public static float GetMinHeight(RectTransform rect)
		{
			return UILayoutUtility.GetLayoutProperty(rect, (ILayoutElement e) => e.minHeight, 0f);
		}

		// Token: 0x060008ED RID: 2285 RVA: 0x000261F8 File Offset: 0x000243F8
		public static float GetPreferredHeight(RectTransform rect)
		{
			return Mathf.Max(UILayoutUtility.GetLayoutProperty(rect, (ILayoutElement e) => e.minHeight, 0f), UILayoutUtility.GetLayoutProperty(rect, (ILayoutElement e) => e.preferredHeight, 0f));
		}

		// Token: 0x060008EE RID: 2286 RVA: 0x0002625E File Offset: 0x0002445E
		public static float GetFlexibleHeight(RectTransform rect)
		{
			return UILayoutUtility.GetLayoutProperty(rect, (ILayoutElement e) => e.flexibleHeight, 0f);
		}

		// Token: 0x060008EF RID: 2287 RVA: 0x0002628C File Offset: 0x0002448C
		private static float GetLayoutProperty(RectTransform rect, Func<ILayoutElement, float> property, float defaultValue)
		{
			if (!rect)
			{
				return defaultValue;
			}
			float num = defaultValue;
			int num2 = int.MinValue;
			List<Component> list = CollectionPool<List<Component>, Component>.Get();
			rect.GetComponents(typeof(ILayoutElement), list);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				ILayoutElement layoutElement = list[i] as ILayoutElement;
				Behaviour behaviour = layoutElement as Behaviour;
				if (behaviour == null || behaviour.isActiveAndEnabled)
				{
					int layoutPriority = layoutElement.layoutPriority;
					if (layoutPriority >= num2)
					{
						float num3 = property(layoutElement);
						if (num3 >= 0f)
						{
							if (layoutPriority > num2)
							{
								num = num3;
								num2 = layoutPriority;
							}
							else if (num3 > num)
							{
								num = num3;
							}
						}
					}
				}
			}
			CollectionPool<List<Component>, Component>.Release(list);
			return num;
		}
	}
}
