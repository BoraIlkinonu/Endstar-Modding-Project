using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000278 RID: 632
	public static class UIDropUtility
	{
		// Token: 0x06000FE2 RID: 4066 RVA: 0x00043F14 File Offset: 0x00042114
		public static bool WouldBeValidDrop(RectTransform droppedRectTransform, RectTransform dropZoneRectTransform, float dropEventIfOverlapPercentageIsEqualToOrOver, bool verboseLogging = false)
		{
			if (verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[]
				{
					"WouldBeValidDrop",
					"droppedRectTransform",
					droppedRectTransform.DebugSafeName(true),
					"droppedRectTransform",
					dropZoneRectTransform.DebugSafeName(true),
					"dropEventIfOverlapPercentageIsEqualToOrOver",
					dropEventIfOverlapPercentageIsEqualToOrOver
				}), null);
			}
			ValueTuple<RectTransform, RectTransform> valueTuple = RectTransformUtility.SmallerAndLarger(droppedRectTransform, dropZoneRectTransform);
			RectTransform item = valueTuple.Item2;
			float num = RectTransformUtility.OverlapPercentage(valueTuple.Item1, item);
			float num2 = dropEventIfOverlapPercentageIsEqualToOrOver / 100f;
			bool flag = num > num2;
			if (verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1:P0}, ", "overlapPercentage", num) + string.Format("{0}: {1:P0}, ", "triggerPercentage", num2) + string.Format("{0}: {1}", "wouldBeValidDrop", flag), null);
			}
			return flag;
		}
	}
}
