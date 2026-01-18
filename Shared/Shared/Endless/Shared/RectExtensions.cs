using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200005F RID: 95
	public static class RectExtensions
	{
		// Token: 0x060002E5 RID: 741 RVA: 0x0000E662 File Offset: 0x0000C862
		public static Rect AddX(this Rect rect, float x)
		{
			rect.x += x;
			return rect;
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x0000E674 File Offset: 0x0000C874
		public static Rect AddY(this Rect rect, float y)
		{
			rect.y += y;
			return rect;
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x0000E686 File Offset: 0x0000C886
		public static Rect AddXMin(this Rect rect, float xMin)
		{
			rect.xMin += xMin;
			return rect;
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x0000E698 File Offset: 0x0000C898
		public static Rect AddYMin(this Rect rect, float yMin)
		{
			rect.yMin += yMin;
			return rect;
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x0000E6AA File Offset: 0x0000C8AA
		public static Rect AddWidth(this Rect rect, float width)
		{
			rect.width += width;
			return rect;
		}

		// Token: 0x060002EA RID: 746 RVA: 0x0000E6BC File Offset: 0x0000C8BC
		public static Rect AddHeight(this Rect rect, float height)
		{
			rect.height += height;
			return rect;
		}

		// Token: 0x060002EB RID: 747 RVA: 0x0000E6CE File Offset: 0x0000C8CE
		public static Rect AddMin(this Rect rect, Vector2 min)
		{
			rect.min += min;
			return rect;
		}

		// Token: 0x060002EC RID: 748 RVA: 0x0000E6E4 File Offset: 0x0000C8E4
		public static Rect AddMax(this Rect rect, Vector2 max)
		{
			rect.max += max;
			return rect;
		}

		// Token: 0x060002ED RID: 749 RVA: 0x0000E6FA File Offset: 0x0000C8FA
		public static Rect AddPos(this Rect rect, Vector2 pos)
		{
			rect.position += pos;
			return rect;
		}

		// Token: 0x060002EE RID: 750 RVA: 0x0000E710 File Offset: 0x0000C910
		public static Rect AddSize(this Rect rect, Vector2 size)
		{
			rect.size += size;
			return rect;
		}

		// Token: 0x060002EF RID: 751 RVA: 0x0000E726 File Offset: 0x0000C926
		public static Rect WithX(this Rect rect, float x)
		{
			rect.x = x;
			return rect;
		}

		// Token: 0x060002F0 RID: 752 RVA: 0x0000E731 File Offset: 0x0000C931
		public static Rect WithY(this Rect rect, float y)
		{
			rect.y = y;
			return rect;
		}

		// Token: 0x060002F1 RID: 753 RVA: 0x0000E73C File Offset: 0x0000C93C
		public static Rect WithWidth(this Rect rect, float width)
		{
			rect.width = width;
			return rect;
		}

		// Token: 0x060002F2 RID: 754 RVA: 0x0000E747 File Offset: 0x0000C947
		public static Rect WithHeight(this Rect rect, float height)
		{
			rect.height = height;
			return rect;
		}

		// Token: 0x060002F3 RID: 755 RVA: 0x0000E752 File Offset: 0x0000C952
		public static Rect WithPosition(this Rect rect, Vector2 position)
		{
			rect.position = position;
			return rect;
		}

		// Token: 0x060002F4 RID: 756 RVA: 0x0000E75D File Offset: 0x0000C95D
		public static Rect WithSize(this Rect rect, Vector2 size)
		{
			rect.size = size;
			return rect;
		}

		// Token: 0x060002F5 RID: 757 RVA: 0x0000E768 File Offset: 0x0000C968
		public static Rect WithMin(this Rect rect, Vector2 min)
		{
			rect.min = min;
			return rect;
		}

		// Token: 0x060002F6 RID: 758 RVA: 0x0000E773 File Offset: 0x0000C973
		public static Rect WithMax(this Rect rect, Vector2 max)
		{
			rect.max = max;
			return rect;
		}
	}
}
