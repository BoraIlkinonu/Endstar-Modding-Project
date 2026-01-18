using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200014C RID: 332
	public static class AxesExtensions
	{
		// Token: 0x06000819 RID: 2073 RVA: 0x00021FDE File Offset: 0x000201DE
		public static float Get(this Vector2 vector, UIBaseLayoutGroup.Axes axis)
		{
			return vector[(int)axis];
		}

		// Token: 0x0600081A RID: 2074 RVA: 0x00021FE8 File Offset: 0x000201E8
		public static void Set(this Vector2 vector, UIBaseLayoutGroup.Axes axis, float value)
		{
			vector[(int)axis] = value;
		}

		// Token: 0x0600081B RID: 2075 RVA: 0x00021FF2 File Offset: 0x000201F2
		public static float Get(this Vector3 vector, UIBaseLayoutGroup.Axes axis)
		{
			return vector[(int)axis];
		}

		// Token: 0x0600081C RID: 2076 RVA: 0x00021FFC File Offset: 0x000201FC
		public static void Set(this Vector3 vector, UIBaseLayoutGroup.Axes axis, float value)
		{
			vector[(int)axis] = value;
		}

		// Token: 0x0600081D RID: 2077 RVA: 0x00022008 File Offset: 0x00020208
		public static float GetSize(this Rect rect, UIBaseLayoutGroup.Axes axis)
		{
			return rect.size[(int)axis];
		}
	}
}
