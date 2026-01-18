using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000168 RID: 360
	[DisallowMultipleComponent]
	public class UIAspectRatioFitter : UIGameObject, IUILayoutable, ILayoutSelfController, ILayoutController
	{
		// Token: 0x17000176 RID: 374
		// (get) Token: 0x060008A1 RID: 2209 RVA: 0x00024FED File Offset: 0x000231ED
		// (set) Token: 0x060008A2 RID: 2210 RVA: 0x00024FF5 File Offset: 0x000231F5
		public UIAspectRatioFitter.AspectModes AspectMode
		{
			get
			{
				return this.aspectMode;
			}
			set
			{
				if (this.aspectMode == value)
				{
					return;
				}
				this.aspectMode = value;
				this.RequestLayout();
			}
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x060008A3 RID: 2211 RVA: 0x0002500E File Offset: 0x0002320E
		// (set) Token: 0x060008A4 RID: 2212 RVA: 0x00025018 File Offset: 0x00023218
		public float AspectRatio
		{
			get
			{
				return this.aspectRatio;
			}
			set
			{
				float num = Mathf.Clamp(value, 0.001f, 1000f);
				if (Mathf.Approximately(this.aspectRatio, num))
				{
					return;
				}
				this.aspectRatio = num;
				this.RequestLayout();
			}
		}

		// Token: 0x060008A5 RID: 2213 RVA: 0x00025052 File Offset: 0x00023252
		protected void OnDisable()
		{
			this.tracker.Clear();
		}

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x060008A6 RID: 2214 RVA: 0x0002505F File Offset: 0x0002325F
		public int LayoutPriority
		{
			get
			{
				return 100;
			}
		}

		// Token: 0x060008A7 RID: 2215 RVA: 0x000050BB File Offset: 0x000032BB
		public void CollectChildLayoutItems()
		{
		}

		// Token: 0x060008A8 RID: 2216 RVA: 0x00025063 File Offset: 0x00023263
		public void RequestLayout()
		{
			if (Application.isPlaying)
			{
				MonoBehaviourSingleton<UILayoutManager>.Instance.RequestLayout(this);
				return;
			}
			this.Layout();
		}

		// Token: 0x060008A9 RID: 2217 RVA: 0x000050BB File Offset: 0x000032BB
		public void CalculateLayout()
		{
		}

		// Token: 0x060008AA RID: 2218 RVA: 0x00025080 File Offset: 0x00023280
		public void ApplyLayout()
		{
			if (!base.isActiveAndEnabled || !this.IsComponentValidOnObject())
			{
				return;
			}
			this.tracker.Clear();
			switch (this.aspectMode)
			{
			case UIAspectRatioFitter.AspectModes.WidthControlsHeight:
				this.tracker.Add(this, base.RectTransform, DrivenTransformProperties.SizeDeltaY);
				base.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, base.RectTransform.rect.width / this.aspectRatio);
				return;
			case UIAspectRatioFitter.AspectModes.HeightControlsWidth:
				this.tracker.Add(this, base.RectTransform, DrivenTransformProperties.SizeDeltaX);
				base.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, base.RectTransform.rect.height * this.aspectRatio);
				return;
			case UIAspectRatioFitter.AspectModes.FitInParent:
			case UIAspectRatioFitter.AspectModes.EnvelopeParent:
				if (this.DoesParentExist())
				{
					this.tracker.Add(this, base.RectTransform, DrivenTransformProperties.AnchoredPositionX | DrivenTransformProperties.AnchoredPositionY | DrivenTransformProperties.AnchorMinX | DrivenTransformProperties.AnchorMinY | DrivenTransformProperties.AnchorMaxX | DrivenTransformProperties.AnchorMaxY | DrivenTransformProperties.SizeDeltaX | DrivenTransformProperties.SizeDeltaY);
					base.RectTransform.anchorMin = Vector2.zero;
					base.RectTransform.anchorMax = Vector2.one;
					base.RectTransform.anchoredPosition = Vector2.zero;
					Vector2 zero = Vector2.zero;
					Vector2 parentSize = this.GetParentSize();
					if ((parentSize.y * this.aspectRatio < parentSize.x) ^ (this.aspectMode == UIAspectRatioFitter.AspectModes.FitInParent))
					{
						zero.y = this.GetSizeDeltaToProduceSize(parentSize.x / this.aspectRatio, 1);
					}
					else
					{
						zero.x = this.GetSizeDeltaToProduceSize(parentSize.y * this.aspectRatio, 0);
					}
					base.RectTransform.sizeDelta = zero;
				}
				return;
			default:
				return;
			}
		}

		// Token: 0x060008AB RID: 2219 RVA: 0x00025209 File Offset: 0x00023409
		public void Layout()
		{
			this.CalculateLayout();
			this.ApplyLayout();
		}

		// Token: 0x060008AC RID: 2220 RVA: 0x000050BB File Offset: 0x000032BB
		public void AddChildLayoutItem(RectTransform newChildLayoutItem, int? siblingIndex = null)
		{
		}

		// Token: 0x060008AD RID: 2221 RVA: 0x000050BB File Offset: 0x000032BB
		public void RemoveChildLayoutItem(RectTransform childLayoutItem)
		{
		}

		// Token: 0x060008AE RID: 2222 RVA: 0x00025217 File Offset: 0x00023417
		public void SetLayoutHorizontal()
		{
			this.RequestLayout();
		}

		// Token: 0x060008AF RID: 2223 RVA: 0x000050BB File Offset: 0x000032BB
		public void SetLayoutVertical()
		{
		}

		// Token: 0x060008B0 RID: 2224 RVA: 0x00025220 File Offset: 0x00023420
		private float GetSizeDeltaToProduceSize(float size, int axis)
		{
			return size - this.GetParentSize()[axis] * (base.RectTransform.anchorMax[axis] - base.RectTransform.anchorMin[axis]);
		}

		// Token: 0x060008B1 RID: 2225 RVA: 0x00025268 File Offset: 0x00023468
		private Vector2 GetParentSize()
		{
			RectTransform rectTransform = base.RectTransform.parent as RectTransform;
			if (!rectTransform)
			{
				return Vector2.zero;
			}
			return rectTransform.rect.size;
		}

		// Token: 0x060008B2 RID: 2226 RVA: 0x000252A4 File Offset: 0x000234A4
		private bool IsComponentValidOnObject()
		{
			Canvas component = base.GetComponent<Canvas>();
			return !component || !component.isRootCanvas || component.renderMode == RenderMode.WorldSpace;
		}

		// Token: 0x060008B3 RID: 2227 RVA: 0x000252D4 File Offset: 0x000234D4
		private bool IsAspectModeValid()
		{
			return this.DoesParentExist() || (this.aspectMode != UIAspectRatioFitter.AspectModes.EnvelopeParent && this.aspectMode != UIAspectRatioFitter.AspectModes.FitInParent);
		}

		// Token: 0x060008B4 RID: 2228 RVA: 0x000252F3 File Offset: 0x000234F3
		private bool DoesParentExist()
		{
			return base.RectTransform.parent != null;
		}

		// Token: 0x04000566 RID: 1382
		[Header("Aspect Ratio")]
		[SerializeField]
		private UIAspectRatioFitter.AspectModes aspectMode;

		// Token: 0x04000567 RID: 1383
		[SerializeField]
		private float aspectRatio = 1f;

		// Token: 0x04000568 RID: 1384
		private DrivenRectTransformTracker tracker;

		// Token: 0x02000169 RID: 361
		public enum AspectModes
		{
			// Token: 0x0400056A RID: 1386
			None,
			// Token: 0x0400056B RID: 1387
			WidthControlsHeight,
			// Token: 0x0400056C RID: 1388
			HeightControlsWidth,
			// Token: 0x0400056D RID: 1389
			FitInParent,
			// Token: 0x0400056E RID: 1390
			EnvelopeParent
		}
	}
}
