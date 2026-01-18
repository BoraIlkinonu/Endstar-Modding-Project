using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200016D RID: 365
	public class UILayoutManager : MonoBehaviourSingleton<UILayoutManager>
	{
		// Token: 0x060008D6 RID: 2262 RVA: 0x00025D18 File Offset: 0x00023F18
		public void RequestLayout(IUILayoutable layoutable)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RequestLayout", new object[] { layoutable });
			}
			if (layoutable == null)
			{
				Debug.LogWarning("Attempted to request layout for null IUILayoutable", this);
				return;
			}
			this.pendingLayouts.Add(layoutable);
			if (!this.isLayoutScheduled)
			{
				this.isLayoutScheduled = true;
				base.StartCoroutine(this.ProcessLayoutsNextFrame());
			}
		}

		// Token: 0x060008D7 RID: 2263 RVA: 0x00025D7A File Offset: 0x00023F7A
		public void ForceLayout()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ForceLayout", Array.Empty<object>());
			}
			if (this.pendingLayouts.Count == 0)
			{
				return;
			}
			this.ProcessPendingLayouts();
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x00025DA8 File Offset: 0x00023FA8
		public void CancelLayoutRequest(IUILayoutable layoutable)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelLayoutRequest", new object[] { layoutable });
			}
			this.pendingLayouts.Remove(layoutable);
		}

		// Token: 0x060008D9 RID: 2265 RVA: 0x00025DD4 File Offset: 0x00023FD4
		public void ClearAllPendingLayouts()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearAllPendingLayouts", Array.Empty<object>());
			}
			this.pendingLayouts.Clear();
		}

		// Token: 0x060008DA RID: 2266 RVA: 0x00025DF9 File Offset: 0x00023FF9
		private IEnumerator ProcessLayoutsNextFrame()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ProcessLayoutsNextFrame", Array.Empty<object>());
			}
			yield return null;
			this.ProcessPendingLayouts();
			yield break;
		}

		// Token: 0x060008DB RID: 2267 RVA: 0x00025E08 File Offset: 0x00024008
		private void ProcessPendingLayouts()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ProcessPendingLayouts", Array.Empty<object>());
			}
			if (this.pendingLayouts.Count == 0)
			{
				this.isLayoutScheduled = false;
				return;
			}
			Canvas.ForceUpdateCanvases();
			List<IUILayoutable> list = this.SortByHierarchyDepth();
			foreach (IUILayoutable iuilayoutable in list)
			{
				if (iuilayoutable != null)
				{
					iuilayoutable.CalculateLayout();
				}
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				IUILayoutable iuilayoutable2 = list[i];
				if (iuilayoutable2 != null)
				{
					iuilayoutable2.ApplyLayout();
				}
			}
			this.pendingLayouts.Clear();
			this.sortedLayoutsCache.Clear();
			this.depthCache.Clear();
			this.isLayoutScheduled = false;
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x00025EE0 File Offset: 0x000240E0
		private List<IUILayoutable> SortByHierarchyDepth()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SortByHierarchyDepth", Array.Empty<object>());
			}
			this.sortedLayoutsCache.Clear();
			this.depthCache.Clear();
			foreach (IUILayoutable iuilayoutable in this.pendingLayouts)
			{
				if ((iuilayoutable != null) ? iuilayoutable.RectTransform : null)
				{
					this.sortedLayoutsCache.Add(iuilayoutable);
					this.depthCache[iuilayoutable.RectTransform] = UILayoutManager.GetHierarchyDepth(iuilayoutable.RectTransform);
				}
			}
			this.sortedLayoutsCache.Sort(new Comparison<IUILayoutable>(this.CompareLayoutablesByDepthThenPriority));
			return this.sortedLayoutsCache;
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x00025FB4 File Offset: 0x000241B4
		private int CompareLayoutablesByDepthThenPriority(IUILayoutable a, IUILayoutable b)
		{
			int num = this.depthCache[a.RectTransform];
			int num2 = this.depthCache[b.RectTransform];
			if (num != num2)
			{
				return num2.CompareTo(num);
			}
			return b.LayoutPriority.CompareTo(a.LayoutPriority);
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x00026008 File Offset: 0x00024208
		private static int GetHierarchyDepth(Transform transform)
		{
			int num = 0;
			Transform transform2 = transform;
			while (transform2.parent)
			{
				num++;
				transform2 = transform2.parent;
			}
			return num;
		}

		// Token: 0x0400058C RID: 1420
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400058D RID: 1421
		private readonly HashSet<IUILayoutable> pendingLayouts = new HashSet<IUILayoutable>();

		// Token: 0x0400058E RID: 1422
		private readonly List<IUILayoutable> sortedLayoutsCache = new List<IUILayoutable>();

		// Token: 0x0400058F RID: 1423
		private readonly Dictionary<RectTransform, int> depthCache = new Dictionary<RectTransform, int>();

		// Token: 0x04000590 RID: 1424
		private bool isLayoutScheduled;
	}
}
