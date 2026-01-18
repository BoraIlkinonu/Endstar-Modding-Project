using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000105 RID: 261
	public static class RectTransformExtensions
	{
		// Token: 0x0600063B RID: 1595 RVA: 0x0001A578 File Offset: 0x00018778
		public static void SetAnchor(this RectTransform rectTransform, AnchorPresets align, float anchoredPositionOffsetX = 0f, float anchoredPositionOffsetY = 0f, float sizeDeltaX = 0f, float sizeDeltaY = 0f)
		{
			rectTransform.anchoredPosition = new Vector3(anchoredPositionOffsetX, anchoredPositionOffsetY, 0f);
			rectTransform.sizeDelta = new Vector2(sizeDeltaX, sizeDeltaY);
			rectTransform.anchorMin = RectTransformExtensions.AnchorPresets[align].Min;
			rectTransform.anchorMax = RectTransformExtensions.AnchorPresets[align].Max;
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x0001A5D8 File Offset: 0x000187D8
		public static void SetPivot(this RectTransform rectTransform, PivotPresets preset)
		{
			switch (preset)
			{
			case PivotPresets.TopLeft:
				rectTransform.pivot = new Vector2(0f, 1f);
				return;
			case PivotPresets.TopCenter:
				rectTransform.pivot = new Vector2(0.5f, 1f);
				return;
			case PivotPresets.TopRight:
				rectTransform.pivot = new Vector2(1f, 1f);
				return;
			case PivotPresets.MiddleLeft:
				rectTransform.pivot = new Vector2(0f, 0.5f);
				return;
			case PivotPresets.MiddleCenter:
				rectTransform.pivot = new Vector2(0.5f, 0.5f);
				return;
			case PivotPresets.MiddleRight:
				rectTransform.pivot = new Vector2(1f, 0.5f);
				return;
			case PivotPresets.BottomLeft:
				rectTransform.pivot = new Vector2(0f, 0f);
				return;
			case PivotPresets.BottomCenter:
				rectTransform.pivot = new Vector2(0.5f, 0f);
				return;
			case PivotPresets.BottomRight:
				rectTransform.pivot = new Vector2(1f, 0f);
				return;
			default:
				return;
			}
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x0001A6D8 File Offset: 0x000188D8
		public static Rect WorldRect(this RectTransform rectTransform)
		{
			Vector2 size = rectTransform.rect.size;
			Vector3 lossyScale = rectTransform.lossyScale;
			float num = size.x * lossyScale.x;
			float num2 = size.y * lossyScale.y;
			Vector3 position = rectTransform.position;
			return new Rect(position.x - num / 2f, position.y - num2 / 2f, num, num2);
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x0001A740 File Offset: 0x00018940
		public static Rect GetScreenRect(this RectTransform rectTransform, Canvas canvas)
		{
			Vector3[] array = new Vector3[4];
			Vector3[] array2 = new Vector3[2];
			rectTransform.GetWorldCorners(array);
			if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
			{
				array2[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, array[1]);
				array2[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, array[3]);
			}
			else
			{
				array2[0] = RectTransformUtility.WorldToScreenPoint(null, array[1]);
				array2[1] = RectTransformUtility.WorldToScreenPoint(null, array[3]);
			}
			array2[0].y = (float)Screen.height - array2[0].y;
			array2[1].y = (float)Screen.height - array2[1].y;
			return new Rect(array2[0], array2[1] - array2[0]);
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x0001A84C File Offset: 0x00018A4C
		public static bool Overlaps(this RectTransform rectTransformA, RectTransform rectTransformB)
		{
			return rectTransformA.WorldRect().Overlaps(rectTransformB.WorldRect());
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x0001A870 File Offset: 0x00018A70
		public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse)
		{
			return a.WorldRect().Overlaps(b.WorldRect(), allowInverse);
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x0001A894 File Offset: 0x00018A94
		public static AnchorPresets? TryAndGetAnchorPreset(this RectTransform rectTransform)
		{
			RectTransformExtensions.AnchorValue anchorValue = new RectTransformExtensions.AnchorValue(rectTransform.anchorMin, rectTransform.anchorMax);
			AnchorPresets anchorPresets;
			if (RectTransformExtensions.AnchorPresetsReversed.TryGetValue(anchorValue, out anchorPresets))
			{
				return new AnchorPresets?(anchorPresets);
			}
			return null;
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x0001A8D3 File Offset: 0x00018AD3
		public static void SetParent(this RectTransform rectTransform, Transform parent, AnchorPresets anchorPreset, bool setScaleTo1 = true)
		{
			rectTransform.SetParent(parent);
			rectTransform.SetAnchor(anchorPreset, 0f, 0f, 0f, 0f);
			if (setScaleTo1)
			{
				rectTransform.localScale = Vector3.one;
			}
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x0001A908 File Offset: 0x00018B08
		public static int ActiveChildCount(this RectTransform rectTransform)
		{
			int num = 0;
			int childCount = rectTransform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				if (rectTransform.GetChild(i).gameObject.activeSelf)
				{
					num++;
				}
			}
			return num;
		}

		// Token: 0x06000644 RID: 1604 RVA: 0x0001A944 File Offset: 0x00018B44
		public static RectTransform GetActiveChild(this RectTransform rectTransform, int index)
		{
			int num = 0;
			int childCount = rectTransform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = rectTransform.GetChild(i);
				if (child.gameObject.activeSelf)
				{
					if (num == index)
					{
						RectTransform rectTransform2;
						if (!child.TryGetComponent<RectTransform>(out rectTransform2))
						{
							Debug.LogWarning("RectTransformExtensions's GetActiveChild method expects its child '" + child.name + "' to have a RectTransform!", rectTransform);
							return null;
						}
						return rectTransform2;
					}
					else
					{
						num++;
					}
				}
			}
			return null;
		}

		// Token: 0x06000645 RID: 1605 RVA: 0x0001A9B0 File Offset: 0x00018BB0
		public static bool IsInside(this RectTransform parentRect, RectTransform childRect)
		{
			Vector3[] array = new Vector3[4];
			parentRect.GetWorldCorners(array);
			Vector3[] array2 = new Vector3[4];
			childRect.GetWorldCorners(array2);
			foreach (Vector3 vector in array2)
			{
				if (!RectTransformUtility.RectangleContainsScreenPoint(parentRect, vector))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000646 RID: 1606 RVA: 0x0001AA04 File Offset: 0x00018C04
		public static bool AreRectTransformsIntersecting(this RectTransform rectA, RectTransform rectB)
		{
			if (rectA == null || rectB == null)
			{
				return false;
			}
			rectA.GetWorldCorners(RectTransformExtensions.cornersA);
			rectB.GetWorldCorners(RectTransformExtensions.cornersB);
			float num = Mathf.Min(new float[]
			{
				RectTransformExtensions.cornersA[0].x,
				RectTransformExtensions.cornersA[1].x,
				RectTransformExtensions.cornersA[2].x,
				RectTransformExtensions.cornersA[3].x
			});
			float num2 = Mathf.Max(new float[]
			{
				RectTransformExtensions.cornersA[0].x,
				RectTransformExtensions.cornersA[1].x,
				RectTransformExtensions.cornersA[2].x,
				RectTransformExtensions.cornersA[3].x
			});
			float num3 = Mathf.Min(new float[]
			{
				RectTransformExtensions.cornersA[0].y,
				RectTransformExtensions.cornersA[1].y,
				RectTransformExtensions.cornersA[2].y,
				RectTransformExtensions.cornersA[3].y
			});
			float num4 = Mathf.Max(new float[]
			{
				RectTransformExtensions.cornersA[0].y,
				RectTransformExtensions.cornersA[1].y,
				RectTransformExtensions.cornersA[2].y,
				RectTransformExtensions.cornersA[3].y
			});
			float num5 = Mathf.Min(new float[]
			{
				RectTransformExtensions.cornersB[0].x,
				RectTransformExtensions.cornersB[1].x,
				RectTransformExtensions.cornersB[2].x,
				RectTransformExtensions.cornersB[3].x
			});
			float num6 = Mathf.Max(new float[]
			{
				RectTransformExtensions.cornersB[0].x,
				RectTransformExtensions.cornersB[1].x,
				RectTransformExtensions.cornersB[2].x,
				RectTransformExtensions.cornersB[3].x
			});
			float num7 = Mathf.Min(new float[]
			{
				RectTransformExtensions.cornersB[0].y,
				RectTransformExtensions.cornersB[1].y,
				RectTransformExtensions.cornersB[2].y,
				RectTransformExtensions.cornersB[3].y
			});
			float num8 = Mathf.Max(new float[]
			{
				RectTransformExtensions.cornersB[0].y,
				RectTransformExtensions.cornersB[1].y,
				RectTransformExtensions.cornersB[2].y,
				RectTransformExtensions.cornersB[3].y
			});
			bool flag = num < num6 && num2 > num5;
			bool flag2 = num3 < num8 && num4 > num7;
			return flag && flag2;
		}

		// Token: 0x06000647 RID: 1607 RVA: 0x0001AD20 File Offset: 0x00018F20
		public static float Area(this RectTransform rectTransform)
		{
			Rect rect = rectTransform.rect;
			return rect.width * rect.height;
		}

		// Token: 0x06000648 RID: 1608 RVA: 0x0001AD43 File Offset: 0x00018F43
		public static void SetHorizontalPadding(this RectTransform rectTransform, float left, float right)
		{
			rectTransform.offsetMin = new Vector2(left, rectTransform.offsetMin.y);
			rectTransform.offsetMax = new Vector2(-right, rectTransform.offsetMax.y);
		}

		// Token: 0x06000649 RID: 1609 RVA: 0x0001AD74 File Offset: 0x00018F74
		public static void SetVerticalPadding(this RectTransform rectTransform, float top, float bottom)
		{
			rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, bottom);
			rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -top);
		}

		// Token: 0x04000390 RID: 912
		private static readonly Dictionary<AnchorPresets, RectTransformExtensions.AnchorValue> AnchorPresets = new Dictionary<AnchorPresets, RectTransformExtensions.AnchorValue>
		{
			{
				Endless.Shared.UI.AnchorPresets.TopLeft,
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 1f), new Vector2(0f, 1f))
			},
			{
				Endless.Shared.UI.AnchorPresets.TopCenter,
				new RectTransformExtensions.AnchorValue(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f))
			},
			{
				Endless.Shared.UI.AnchorPresets.TopRight,
				new RectTransformExtensions.AnchorValue(new Vector2(1f, 1f), new Vector2(1f, 1f))
			},
			{
				Endless.Shared.UI.AnchorPresets.MiddleLeft,
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f))
			},
			{
				Endless.Shared.UI.AnchorPresets.MiddleCenter,
				new RectTransformExtensions.AnchorValue(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f))
			},
			{
				Endless.Shared.UI.AnchorPresets.MiddleRight,
				new RectTransformExtensions.AnchorValue(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f))
			},
			{
				Endless.Shared.UI.AnchorPresets.BottomLeft,
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0f), new Vector2(0f, 0f))
			},
			{
				Endless.Shared.UI.AnchorPresets.BottomCenter,
				new RectTransformExtensions.AnchorValue(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f))
			},
			{
				Endless.Shared.UI.AnchorPresets.BottomRight,
				new RectTransformExtensions.AnchorValue(new Vector2(1f, 0f), new Vector2(1f, 0f))
			},
			{
				Endless.Shared.UI.AnchorPresets.HorStretchTop,
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 1f), new Vector2(1f, 1f))
			},
			{
				Endless.Shared.UI.AnchorPresets.HorStretchMiddle,
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f))
			},
			{
				Endless.Shared.UI.AnchorPresets.HorStretchBottom,
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0f), new Vector2(1f, 0f))
			},
			{
				Endless.Shared.UI.AnchorPresets.VertStretchLeft,
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0f), new Vector2(0f, 1f))
			},
			{
				Endless.Shared.UI.AnchorPresets.VertStretchCenter,
				new RectTransformExtensions.AnchorValue(new Vector2(0.5f, 0f), new Vector2(0.5f, 1f))
			},
			{
				Endless.Shared.UI.AnchorPresets.VertStretchRight,
				new RectTransformExtensions.AnchorValue(new Vector2(1f, 0f), new Vector2(1f, 1f))
			},
			{
				Endless.Shared.UI.AnchorPresets.StretchAll,
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0f), new Vector2(1f, 1f))
			}
		};

		// Token: 0x04000391 RID: 913
		private static readonly Dictionary<RectTransformExtensions.AnchorValue, AnchorPresets> AnchorPresetsReversed = new Dictionary<RectTransformExtensions.AnchorValue, AnchorPresets>
		{
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 1f), new Vector2(0f, 1f)),
				Endless.Shared.UI.AnchorPresets.TopLeft
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f)),
				Endless.Shared.UI.AnchorPresets.TopCenter
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(1f, 1f), new Vector2(1f, 1f)),
				Endless.Shared.UI.AnchorPresets.TopRight
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f)),
				Endless.Shared.UI.AnchorPresets.MiddleLeft
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)),
				Endless.Shared.UI.AnchorPresets.MiddleCenter
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f)),
				Endless.Shared.UI.AnchorPresets.MiddleRight
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0f), new Vector2(0f, 0f)),
				Endless.Shared.UI.AnchorPresets.BottomLeft
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f)),
				Endless.Shared.UI.AnchorPresets.BottomCenter
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(1f, 0f), new Vector2(1f, 0f)),
				Endless.Shared.UI.AnchorPresets.BottomRight
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 1f), new Vector2(1f, 1f)),
				Endless.Shared.UI.AnchorPresets.HorStretchTop
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f)),
				Endless.Shared.UI.AnchorPresets.HorStretchMiddle
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0f), new Vector2(1f, 0f)),
				Endless.Shared.UI.AnchorPresets.HorStretchBottom
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0f), new Vector2(0f, 1f)),
				Endless.Shared.UI.AnchorPresets.VertStretchLeft
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0.5f, 0f), new Vector2(0.5f, 1f)),
				Endless.Shared.UI.AnchorPresets.VertStretchCenter
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(1f, 0f), new Vector2(1f, 1f)),
				Endless.Shared.UI.AnchorPresets.VertStretchRight
			},
			{
				new RectTransformExtensions.AnchorValue(new Vector2(0f, 0f), new Vector2(1f, 1f)),
				Endless.Shared.UI.AnchorPresets.StretchAll
			}
		};

		// Token: 0x04000392 RID: 914
		private static readonly Vector3[] cornersA = new Vector3[4];

		// Token: 0x04000393 RID: 915
		private static readonly Vector3[] cornersB = new Vector3[4];

		// Token: 0x02000106 RID: 262
		private struct AnchorValue
		{
			// Token: 0x0600064B RID: 1611 RVA: 0x0001B32D File Offset: 0x0001952D
			public AnchorValue(Vector2 min, Vector2 max)
			{
				this.Min = min;
				this.Max = max;
			}

			// Token: 0x04000394 RID: 916
			public readonly Vector2 Min;

			// Token: 0x04000395 RID: 917
			public readonly Vector2 Max;
		}
	}
}
