using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000166 RID: 358
	[DisallowMultipleComponent]
	public abstract class UIBaseLayoutGroup : UIGameObject, IUIChildLayoutable, IUILayoutable
	{
		// Token: 0x1700016F RID: 367
		// (get) Token: 0x0600088A RID: 2186 RVA: 0x00024A9A File Offset: 0x00022C9A
		public virtual float minWidth
		{
			get
			{
				return this.GetTotalMinSize(UIBaseLayoutGroup.Axes.Horizontal);
			}
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x0600088B RID: 2187 RVA: 0x00024AA3 File Offset: 0x00022CA3
		public virtual float preferredWidth
		{
			get
			{
				return this.GetTotalPreferredSize(UIBaseLayoutGroup.Axes.Horizontal);
			}
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x0600088C RID: 2188 RVA: 0x00024AAC File Offset: 0x00022CAC
		public virtual float flexibleWidth
		{
			get
			{
				return this.GetTotalFlexibleSize(UIBaseLayoutGroup.Axes.Horizontal);
			}
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x0600088D RID: 2189 RVA: 0x00024AB5 File Offset: 0x00022CB5
		public virtual float minHeight
		{
			get
			{
				return this.GetTotalMinSize(UIBaseLayoutGroup.Axes.Vertical);
			}
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x0600088E RID: 2190 RVA: 0x00024ABE File Offset: 0x00022CBE
		public virtual float preferredHeight
		{
			get
			{
				return this.GetTotalPreferredSize(UIBaseLayoutGroup.Axes.Vertical);
			}
		}

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x0600088F RID: 2191 RVA: 0x00024AC7 File Offset: 0x00022CC7
		public virtual float flexibleHeight
		{
			get
			{
				return this.GetTotalFlexibleSize(UIBaseLayoutGroup.Axes.Vertical);
			}
		}

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x06000890 RID: 2192 RVA: 0x000043C6 File Offset: 0x000025C6
		public virtual int LayoutPriority
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x06000891 RID: 2193 RVA: 0x00024AD0 File Offset: 0x00022CD0
		public void CollectChildLayoutItems()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CollectChildLayoutItems", this);
			}
			this.childLayoutItems.Clear();
			int childCount = base.RectTransform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				RectTransform rectTransform = base.RectTransform.GetChild(i) as RectTransform;
				ILayoutIgnorer layoutIgnorer;
				if (rectTransform && rectTransform.gameObject.activeInHierarchy && (!rectTransform.TryGetComponent<ILayoutIgnorer>(out layoutIgnorer) || !layoutIgnorer.ignoreLayout))
				{
					this.childLayoutItems.Add(rectTransform);
				}
			}
		}

		// Token: 0x06000892 RID: 2194 RVA: 0x00024B58 File Offset: 0x00022D58
		public void RequestLayout()
		{
			if (this.verboseLogging)
			{
				Debug.Log("RequestLayout", this);
			}
			if (Application.isPlaying)
			{
				MonoBehaviourSingleton<UILayoutManager>.Instance.RequestLayout(this);
				return;
			}
			this.Layout();
		}

		// Token: 0x06000893 RID: 2195
		public abstract void CalculateLayout();

		// Token: 0x06000894 RID: 2196
		public abstract void ApplyLayout();

		// Token: 0x06000895 RID: 2197 RVA: 0x00024B86 File Offset: 0x00022D86
		public void Layout()
		{
			this.CalculateLayout();
			this.ApplyLayout();
		}

		// Token: 0x06000896 RID: 2198
		public abstract float GetSpacingSize(Dimension dimension);

		// Token: 0x06000897 RID: 2199 RVA: 0x00024B94 File Offset: 0x00022D94
		public void AddChildLayoutItem(RectTransform newChildLayoutItem, int? siblingIndex = null)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[]
				{
					"AddChildLayoutItem",
					"newChildLayoutItem",
					newChildLayoutItem.DebugSafeName(true),
					"siblingIndex",
					siblingIndex
				}), this);
			}
			if (this.childLayoutItems.Contains(newChildLayoutItem))
			{
				return;
			}
			newChildLayoutItem.SetParent(base.RectTransform, false);
			if (siblingIndex != null)
			{
				newChildLayoutItem.SetSiblingIndex(siblingIndex.Value);
				this.childLayoutItems.Insert(siblingIndex.Value, newChildLayoutItem);
				return;
			}
			newChildLayoutItem.SetAsLastSibling();
			this.childLayoutItems.Add(newChildLayoutItem);
		}

		// Token: 0x06000898 RID: 2200 RVA: 0x00024C44 File Offset: 0x00022E44
		public void RemoveChildLayoutItem(RectTransform childLayoutItem)
		{
			if (this.verboseLogging)
			{
				Debug.Log("RemoveChildLayoutItem ( childLayoutItem: " + childLayoutItem.DebugSafeName(true) + " )", this);
			}
			if (!this.childLayoutItems.Contains(childLayoutItem))
			{
				return;
			}
			this.childLayoutItems.Remove(childLayoutItem);
		}

		// Token: 0x06000899 RID: 2201 RVA: 0x00024C91 File Offset: 0x00022E91
		protected float GetTotalMinSize(UIBaseLayoutGroup.Axes axis)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetTotalMinSize", "axis", axis), this);
			}
			return this.totalMinSize[(int)axis];
		}

		// Token: 0x0600089A RID: 2202 RVA: 0x00024CC3 File Offset: 0x00022EC3
		protected float GetTotalPreferredSize(UIBaseLayoutGroup.Axes axis)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetTotalPreferredSize", "axis", axis), this);
			}
			return this.totalPreferredSize[(int)axis];
		}

		// Token: 0x0600089B RID: 2203 RVA: 0x00024CF5 File Offset: 0x00022EF5
		protected float GetTotalFlexibleSize(UIBaseLayoutGroup.Axes axis)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetTotalFlexibleSize", "axis", axis), this);
			}
			return this.totalFlexibleSize[(int)axis];
		}

		// Token: 0x0600089C RID: 2204 RVA: 0x00024D28 File Offset: 0x00022F28
		protected void SetLayoutInputForAxis(float totalMin, float totalPreferred, float totalFlexible, UIBaseLayoutGroup.Axes axis)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8} )", new object[] { "SetLayoutInputForAxis", "totalMin", totalMin, "totalPreferred", totalPreferred, "totalFlexible", totalFlexible, "axis", axis }), this);
			}
			this.totalMinSize[(int)axis] = totalMin;
			this.totalPreferredSize[(int)axis] = totalPreferred;
			this.totalFlexibleSize[(int)axis] = totalFlexible;
		}

		// Token: 0x0600089D RID: 2205 RVA: 0x00024DC0 File Offset: 0x00022FC0
		protected float GetAlignmentOnAxis(UIBaseLayoutGroup.Axes axis)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetAlignmentOnAxis", "axis", axis), this);
			}
			if (axis == UIBaseLayoutGroup.Axes.Horizontal)
			{
				return (float)(this.ChildAlignment % TextAnchor.MiddleLeft) * 0.5f;
			}
			return (float)(this.ChildAlignment / TextAnchor.MiddleLeft) * 0.5f;
		}

		// Token: 0x0600089E RID: 2206 RVA: 0x00024E18 File Offset: 0x00023018
		protected float GetStartOffset(UIBaseLayoutGroup.Axes axis, float requiredSpaceWithoutPadding)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "GetStartOffset", "axis", axis, "requiredSpaceWithoutPadding", requiredSpaceWithoutPadding }), this);
			}
			float num = (float)((axis == UIBaseLayoutGroup.Axes.Horizontal) ? (this.Padding.left + this.Padding.right) : (this.Padding.top + this.Padding.bottom));
			float size = base.RectTransform.rect.GetSize(axis);
			float num2 = requiredSpaceWithoutPadding + num;
			float num3 = size - num2;
			float alignmentOnAxis = this.GetAlignmentOnAxis(axis);
			if (axis != UIBaseLayoutGroup.Axes.Horizontal)
			{
				return (float)this.Padding.top + num3 * alignmentOnAxis;
			}
			return (float)this.Padding.left + num3 * alignmentOnAxis;
		}

		// Token: 0x0600089F RID: 2207 RVA: 0x00024EE8 File Offset: 0x000230E8
		protected static void SetChildAlongAxisWithScale(RectTransform rectTransform, UIBaseLayoutGroup.Axes axis, float position, float? size = null, float scale = 1f)
		{
			rectTransform.anchorMin = Vector2.up;
			rectTransform.anchorMax = Vector2.up;
			float num = size ?? rectTransform.sizeDelta.Get(axis);
			if (size != null)
			{
				Vector2 sizeDelta = rectTransform.sizeDelta;
				(ref sizeDelta).Set(axis, size.Value);
				rectTransform.sizeDelta = sizeDelta;
			}
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			if (axis == UIBaseLayoutGroup.Axes.Horizontal)
			{
				anchoredPosition.x = position + num * rectTransform.pivot.x * scale;
			}
			else
			{
				anchoredPosition.y = -(position + num * (1f - rectTransform.pivot.y) * scale);
			}
			rectTransform.anchoredPosition = anchoredPosition;
		}

		// Token: 0x0400055C RID: 1372
		[Header("UIBaseLayoutGroup")]
		public RectOffset Padding = new RectOffset();

		// Token: 0x0400055D RID: 1373
		public TextAnchor ChildAlignment;

		// Token: 0x0400055E RID: 1374
		[SerializeField]
		protected List<RectTransform> childLayoutItems = new List<RectTransform>();

		// Token: 0x0400055F RID: 1375
		[Header("Debugging")]
		[SerializeField]
		protected bool verboseLogging;

		// Token: 0x04000560 RID: 1376
		private readonly float[] totalMinSize = new float[2];

		// Token: 0x04000561 RID: 1377
		private readonly float[] totalPreferredSize = new float[2];

		// Token: 0x04000562 RID: 1378
		private readonly float[] totalFlexibleSize = new float[2];

		// Token: 0x02000167 RID: 359
		public enum Axes
		{
			// Token: 0x04000564 RID: 1380
			Horizontal,
			// Token: 0x04000565 RID: 1381
			Vertical
		}
	}
}
