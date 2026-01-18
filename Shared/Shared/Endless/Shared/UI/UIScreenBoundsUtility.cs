using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000279 RID: 633
	public static class UIScreenBoundsUtility
	{
		// Token: 0x06000FE3 RID: 4067 RVA: 0x00043FEC File Offset: 0x000421EC
		public static bool IsRectTransformCompletelyOnScreen(RectTransform rectTransform, Camera targetCamera, bool verboseLogging = false)
		{
			if (verboseLogging)
			{
				Debug.Log(string.Concat(new string[]
				{
					"IsRectTransformCompletelyOnScreen ( rectTransform: ",
					rectTransform.DebugSafeName(true),
					", targetCamera: ",
					((targetCamera != null) ? targetCamera.DebugSafeName(true) : null) ?? "null",
					" )"
				}), rectTransform);
			}
			if (!rectTransform)
			{
				return false;
			}
			rectTransform.GetWorldCorners(UIScreenBoundsUtility.reusableCorners);
			Rect rect = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);
			foreach (Vector3 vector in UIScreenBoundsUtility.reusableCorners)
			{
				Vector3 vector2;
				if (targetCamera)
				{
					vector2 = targetCamera.WorldToScreenPoint(vector);
					if (vector2.z <= 0f)
					{
						return false;
					}
				}
				else
				{
					vector2 = vector;
				}
				if (!rect.Contains(vector2))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000FE4 RID: 4068 RVA: 0x000440C8 File Offset: 0x000422C8
		public static UIScreenBoundsUtility.ExceededScreenEdges GetScreenEdgeExceeded(RectTransform rectTransform, Camera targetCamera, bool verboseLogging = false)
		{
			if (verboseLogging)
			{
				Debug.Log(string.Concat(new string[]
				{
					"GetScreenEdgeExceeded ( rectTransform: ",
					rectTransform.DebugSafeName(true),
					", targetCamera: ",
					targetCamera.DebugSafeName(true),
					" )"
				}), rectTransform);
			}
			if (!rectTransform)
			{
				return UIScreenBoundsUtility.ExceededScreenEdges.None;
			}
			rectTransform.GetWorldCorners(UIScreenBoundsUtility.reusableCorners);
			UIScreenBoundsUtility.ExceededScreenEdges exceededScreenEdges = UIScreenBoundsUtility.ExceededScreenEdges.None;
			float num = float.MaxValue;
			float num2 = float.MinValue;
			float num3 = float.MaxValue;
			float num4 = float.MinValue;
			Vector3[] array = UIScreenBoundsUtility.reusableCorners;
			int i = 0;
			while (i < array.Length)
			{
				Vector3 vector = array[i];
				Vector3 vector2;
				if (!targetCamera)
				{
					vector2 = vector;
					goto IL_00AD;
				}
				vector2 = targetCamera.WorldToScreenPoint(vector);
				if (vector2.z > 0f)
				{
					goto IL_00AD;
				}
				IL_00E7:
				i++;
				continue;
				IL_00AD:
				num = Mathf.Min(num, vector2.x);
				num2 = Mathf.Max(num2, vector2.x);
				num3 = Mathf.Min(num3, vector2.y);
				num4 = Mathf.Max(num4, vector2.y);
				goto IL_00E7;
			}
			if (num4 > (float)Screen.height)
			{
				exceededScreenEdges |= UIScreenBoundsUtility.ExceededScreenEdges.Top;
			}
			if (num3 < 0f)
			{
				exceededScreenEdges |= UIScreenBoundsUtility.ExceededScreenEdges.Bottom;
			}
			if (num2 > (float)Screen.width)
			{
				exceededScreenEdges |= UIScreenBoundsUtility.ExceededScreenEdges.Right;
			}
			if (num < 0f)
			{
				exceededScreenEdges |= UIScreenBoundsUtility.ExceededScreenEdges.Left;
			}
			return exceededScreenEdges;
		}

		// Token: 0x04000A20 RID: 2592
		private static readonly Vector3[] reusableCorners = new Vector3[4];

		// Token: 0x0200027A RID: 634
		[Flags]
		public enum ExceededScreenEdges
		{
			// Token: 0x04000A22 RID: 2594
			None = 0,
			// Token: 0x04000A23 RID: 2595
			Top = 1,
			// Token: 0x04000A24 RID: 2596
			Bottom = 2,
			// Token: 0x04000A25 RID: 2597
			Left = 4,
			// Token: 0x04000A26 RID: 2598
			Right = 8
		}
	}
}
