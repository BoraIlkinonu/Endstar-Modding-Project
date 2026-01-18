using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200016A RID: 362
	public class UIContentSizeFitter : UIGameObject, IUIChildLayoutable, IUILayoutable, IValidatable
	{
		// Token: 0x17000179 RID: 377
		// (get) Token: 0x060008B6 RID: 2230 RVA: 0x00025319 File Offset: 0x00023519
		private RectOffset Padding
		{
			get
			{
				if (!this.layoutGroup)
				{
					return this.emptyRectOffset;
				}
				return this.layoutGroup.Padding;
			}
		}

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x060008B7 RID: 2231 RVA: 0x0002533A File Offset: 0x0002353A
		private float MaxWidth
		{
			get
			{
				return UIContentSizeFitter.GetMaxSize(this.maxSizeModeWidth, this.maxWidth, this.maxWidthReference, true, this.maxWidthReferencePadding);
			}
		}

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x060008B8 RID: 2232 RVA: 0x0002535A File Offset: 0x0002355A
		private float MaxHeight
		{
			get
			{
				return UIContentSizeFitter.GetMaxSize(this.maxSizeModeHeight, this.maxHeight, this.maxHeightReference, false, this.maxHeightReferencePadding);
			}
		}

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x060008B9 RID: 2233 RVA: 0x0002537A File Offset: 0x0002357A
		private TweenSizeDelta TweenSizeDelta
		{
			get
			{
				if (base.TryGetComponent<TweenSizeDelta>(out this.tweenSizeDelta))
				{
					return this.tweenSizeDelta;
				}
				this.tweenSizeDelta = base.gameObject.AddComponent<TweenSizeDelta>();
				this.SubscribeToTweenEvents();
				return this.tweenSizeDelta;
			}
		}

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x060008BA RID: 2234 RVA: 0x0002505F File Offset: 0x0002325F
		public int LayoutPriority
		{
			get
			{
				return 100;
			}
		}

		// Token: 0x060008BB RID: 2235 RVA: 0x000253AE File Offset: 0x000235AE
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (this.tweenSizeDelta)
			{
				this.SubscribeToTweenEvents();
			}
		}

		// Token: 0x060008BC RID: 2236 RVA: 0x000253DB File Offset: 0x000235DB
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (this.tweenSizeDelta)
			{
				this.UnsubscribeFromTweenEvents();
			}
		}

		// Token: 0x060008BD RID: 2237 RVA: 0x00025408 File Offset: 0x00023608
		public void CollectChildLayoutItems()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CollectChildLayoutItems", Array.Empty<object>());
			}
			this.layoutGroupChildren.Clear();
			int childCount = base.RectTransform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				RectTransform rectTransform = base.RectTransform.GetChild(i) as RectTransform;
				if (rectTransform != null && rectTransform.gameObject.activeInHierarchy && !UIContentSizeFitter.ShouldIgnoreChild(rectTransform))
				{
					this.layoutGroupChildren.Add(rectTransform);
				}
			}
		}

		// Token: 0x060008BE RID: 2238 RVA: 0x00025486 File Offset: 0x00023686
		public void RequestLayout()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RequestLayout", Array.Empty<object>());
			}
			if (Application.isPlaying)
			{
				MonoBehaviourSingleton<UILayoutManager>.Instance.RequestLayout(this);
				return;
			}
			this.Layout();
		}

		// Token: 0x060008BF RID: 2239 RVA: 0x000254B9 File Offset: 0x000236B9
		public void CalculateLayout()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CalculateLayout", Array.Empty<object>());
			}
			this.CollectChildLayoutItems();
		}

		// Token: 0x060008C0 RID: 2240 RVA: 0x000254DC File Offset: 0x000236DC
		public void ApplyLayout()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyLayout", Array.Empty<object>());
			}
			Vector2 vector = this.CalculateTargetSize();
			if (base.RectTransform.sizeDelta != vector)
			{
				this.ApplySizeChange(vector);
			}
		}

		// Token: 0x060008C1 RID: 2241 RVA: 0x00025522 File Offset: 0x00023722
		public void Layout()
		{
			this.CalculateLayout();
			this.ApplyLayout();
		}

		// Token: 0x060008C2 RID: 2242 RVA: 0x00025530 File Offset: 0x00023730
		public void AddChildLayoutItem(RectTransform newChildLayoutItem, int? siblingIndex = null)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "AddChildLayoutItem", new object[]
				{
					newChildLayoutItem.DebugSafeName(true),
					siblingIndex
				});
			}
			if (this.layoutGroupChildren.Contains(newChildLayoutItem))
			{
				return;
			}
			newChildLayoutItem.SetParent(base.RectTransform, false);
			if (siblingIndex != null)
			{
				newChildLayoutItem.SetSiblingIndex(siblingIndex.Value);
				this.layoutGroupChildren.Insert(siblingIndex.Value, newChildLayoutItem);
				return;
			}
			newChildLayoutItem.SetAsLastSibling();
			this.layoutGroupChildren.Add(newChildLayoutItem);
		}

		// Token: 0x060008C3 RID: 2243 RVA: 0x000255C4 File Offset: 0x000237C4
		public void RemoveChildLayoutItem(RectTransform childLayoutItem)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveChildLayoutItem", new object[] { childLayoutItem.DebugSafeName(true) });
			}
			if (!this.layoutGroupChildren.Contains(childLayoutItem))
			{
				return;
			}
			this.layoutGroupChildren.Remove(childLayoutItem);
		}

		// Token: 0x060008C4 RID: 2244 RVA: 0x00025610 File Offset: 0x00023810
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.maxSizeModeWidth == UIContentSizeFitter.MaxSizeMode.Reference && !this.maxWidthReference)
			{
				DebugUtility.LogException(new NullReferenceException(string.Format("{0} is required when {1} is set to {2}!", "maxWidthReference", "maxSizeModeWidth", this.maxSizeModeWidth)), this);
			}
			if (this.maxSizeModeHeight == UIContentSizeFitter.MaxSizeMode.Reference && !this.maxHeightReference)
			{
				DebugUtility.LogException(new NullReferenceException(string.Format("{0} is required when {1} is set to {2}!", "maxHeightReference", "maxSizeModeHeight", this.maxSizeModeHeight)), this);
			}
		}

		// Token: 0x060008C5 RID: 2245 RVA: 0x000256B8 File Offset: 0x000238B8
		private Vector2 CalculateTargetSize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CalculateTargetSize", Array.Empty<object>());
			}
			float num = ((this.horizontalFit == UIContentSizeFitter.FitMode.Unconstrained) ? base.RectTransform.rect.width : this.GetDimension(this.horizontalFit, this.manualHorizontalSize, true));
			float num2 = ((this.verticalFit == UIContentSizeFitter.FitMode.Unconstrained) ? base.RectTransform.rect.height : this.GetDimension(this.verticalFit, this.manualVerticalSize, false));
			return new Vector2(num, num2);
		}

		// Token: 0x060008C6 RID: 2246 RVA: 0x00025744 File Offset: 0x00023944
		private void ApplySizeChange(Vector2 targetSize)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplySizeChange", new object[] { targetSize });
			}
			if (this.tweenToSize && Application.isPlaying)
			{
				this.TweenToSize(targetSize);
				return;
			}
			this.SetSize(targetSize);
		}

		// Token: 0x060008C7 RID: 2247 RVA: 0x00025794 File Offset: 0x00023994
		private void TweenToSize(Vector2 targetSize)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TweenToSize", new object[] { targetSize });
			}
			if (this.TweenSizeDelta.To == targetSize && this.TweenSizeDelta.IsTweening)
			{
				return;
			}
			this.TweenSizeDelta.To = targetSize;
			this.TweenSizeDelta.Tween(new Action(this.OnTweenComplete));
			this.SubscribeToTweenEvents();
		}

		// Token: 0x060008C8 RID: 2248 RVA: 0x00025810 File Offset: 0x00023A10
		private void SetSize(Vector2 targetSize)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetSize", new object[] { targetSize });
			}
			base.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
			base.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);
		}

		// Token: 0x060008C9 RID: 2249 RVA: 0x00025864 File Offset: 0x00023A64
		private float GetDimension(UIContentSizeFitter.FitMode fitMode, float manualSize, bool isHorizontal)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetDimension", new object[] { fitMode, manualSize, isHorizontal });
			}
			if (fitMode == UIContentSizeFitter.FitMode.Manual)
			{
				return manualSize;
			}
			float num = this.CalculateChildrenSize(fitMode, isHorizontal);
			num = this.AddPaddingAndSpacing(num, isHorizontal);
			return this.ApplyMaxSizeLimit(num, isHorizontal);
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x000258C8 File Offset: 0x00023AC8
		private float CalculateChildrenSize(UIContentSizeFitter.FitMode fitMode, bool isHorizontal)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CalculateChildrenSize", new object[] { fitMode, isHorizontal });
			}
			float num = 0f;
			foreach (RectTransform rectTransform in this.layoutGroupChildren)
			{
				float childSize = UIContentSizeFitter.GetChildSize(rectTransform, fitMode, isHorizontal);
				num += childSize;
			}
			return num;
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x00025954 File Offset: 0x00023B54
		private float AddPaddingAndSpacing(float totalSize, bool isHorizontal)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "AddPaddingAndSpacing", new object[] { totalSize, isHorizontal });
			}
			if (this.layoutGroupChildren.Count > 1)
			{
				totalSize += this.GetSpacingSize(isHorizontal ? Dimension.Width : Dimension.Height) * (float)(this.layoutGroupChildren.Count - 1);
			}
			RectOffset padding = this.Padding;
			totalSize += (float)(isHorizontal ? padding.horizontal : padding.vertical);
			return totalSize;
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x000259D8 File Offset: 0x00023BD8
		private float ApplyMaxSizeLimit(float totalSize, bool isHorizontal)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyMaxSizeLimit", new object[] { totalSize, isHorizontal });
			}
			if ((isHorizontal ? this.maxSizeModeWidth : this.maxSizeModeHeight) == UIContentSizeFitter.MaxSizeMode.DoNotUse)
			{
				return totalSize;
			}
			float num = (isHorizontal ? this.MaxWidth : this.MaxHeight);
			if (num <= -1f)
			{
				return totalSize;
			}
			return Mathf.Clamp(totalSize, 0f, num);
		}

		// Token: 0x060008CD RID: 2253 RVA: 0x00025A50 File Offset: 0x00023C50
		private float GetSpacingSize(Dimension dimension)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetSpacingSize", new object[] { dimension });
			}
			if (!this.layoutGroup)
			{
				return 0f;
			}
			return this.layoutGroup.GetSpacingSize(dimension);
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x00025AA0 File Offset: 0x00023CA0
		private static float GetChildSize(RectTransform child, UIContentSizeFitter.FitMode fitMode, bool isHorizontal)
		{
			if (!child)
			{
				return 0f;
			}
			UIBaseLayoutGroup.Axes axes = (isHorizontal ? UIBaseLayoutGroup.Axes.Horizontal : UIBaseLayoutGroup.Axes.Vertical);
			float num;
			if (fitMode != UIContentSizeFitter.FitMode.MinSize)
			{
				if (fitMode != UIContentSizeFitter.FitMode.PreferredSize)
				{
					throw new ArgumentOutOfRangeException("fitMode", fitMode, "Not accounted for!");
				}
				num = UILayoutUtility.GetPreferredSize(child, axes);
			}
			else
			{
				num = UILayoutUtility.GetMinSize(child, axes);
			}
			return num;
		}

		// Token: 0x060008CF RID: 2255 RVA: 0x00025AF8 File Offset: 0x00023CF8
		private static float GetMaxSize(UIContentSizeFitter.MaxSizeMode mode, float explicitSize, RectTransform reference, bool width, float padding)
		{
			if (reference)
			{
				float num;
				switch (mode)
				{
				case UIContentSizeFitter.MaxSizeMode.DoNotUse:
					num = -1f;
					break;
				case UIContentSizeFitter.MaxSizeMode.Explicit:
					num = explicitSize;
					break;
				case UIContentSizeFitter.MaxSizeMode.Reference:
					num = (width ? reference.rect.width : reference.rect.height) + padding;
					break;
				default:
					throw new ArgumentOutOfRangeException("mode", mode, "Not accounted for!");
				}
				return num;
			}
			if (mode != UIContentSizeFitter.MaxSizeMode.Reference)
			{
				return explicitSize;
			}
			return -1f;
		}

		// Token: 0x060008D0 RID: 2256 RVA: 0x00025B78 File Offset: 0x00023D78
		private static bool ShouldIgnoreChild(RectTransform child)
		{
			ILayoutIgnorer layoutIgnorer;
			return child.TryGetComponent<ILayoutIgnorer>(out layoutIgnorer) && layoutIgnorer.ignoreLayout;
		}

		// Token: 0x060008D1 RID: 2257 RVA: 0x00025B98 File Offset: 0x00023D98
		private void SubscribeToTweenEvents()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SubscribeToTweenEvents", Array.Empty<object>());
			}
			if (!this.tweenSizeDelta || !this.layoutGroup)
			{
				return;
			}
			this.UnsubscribeFromTweenEvents();
			this.tweenSizeDelta.OnTweenInProgress += this.OnTweenInProgress;
		}

		// Token: 0x060008D2 RID: 2258 RVA: 0x00025BF5 File Offset: 0x00023DF5
		private void UnsubscribeFromTweenEvents()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UnsubscribeFromTweenEvents", Array.Empty<object>());
			}
			if (!this.tweenSizeDelta)
			{
				return;
			}
			this.tweenSizeDelta.OnTweenInProgress -= this.OnTweenInProgress;
		}

		// Token: 0x060008D3 RID: 2259 RVA: 0x00025C34 File Offset: 0x00023E34
		private void OnTweenInProgress()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTweenInProgress", Array.Empty<object>());
			}
			Vector2 sizeDelta = base.RectTransform.sizeDelta;
			if (Vector2.Distance(sizeDelta, this.lastLayoutSize) < 1f)
			{
				return;
			}
			this.lastLayoutSize = sizeDelta;
			this.RequestLayout();
			UIBaseLayoutGroup uibaseLayoutGroup = this.layoutGroup;
			if (uibaseLayoutGroup == null)
			{
				return;
			}
			uibaseLayoutGroup.RequestLayout();
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x00025C98 File Offset: 0x00023E98
		private void OnTweenComplete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTweenComplete", Array.Empty<object>());
			}
			this.lastLayoutSize = base.RectTransform.sizeDelta;
			this.RequestLayout();
			UIBaseLayoutGroup uibaseLayoutGroup = this.layoutGroup;
			if (uibaseLayoutGroup == null)
			{
				return;
			}
			uibaseLayoutGroup.RequestLayout();
		}

		// Token: 0x0400056F RID: 1391
		private const float LAYOUT_THRESHOLD = 1f;

		// Token: 0x04000570 RID: 1392
		[SerializeField]
		private UIBaseLayoutGroup layoutGroup;

		// Token: 0x04000571 RID: 1393
		[Header("Width")]
		[SerializeField]
		private UIContentSizeFitter.FitMode horizontalFit;

		// Token: 0x04000572 RID: 1394
		[SerializeField]
		private float manualHorizontalSize;

		// Token: 0x04000573 RID: 1395
		[SerializeField]
		private UIContentSizeFitter.MaxSizeMode maxSizeModeWidth;

		// Token: 0x04000574 RID: 1396
		[SerializeField]
		private float maxWidth = 100f;

		// Token: 0x04000575 RID: 1397
		[SerializeField]
		private RectTransform maxWidthReference;

		// Token: 0x04000576 RID: 1398
		[SerializeField]
		private float maxWidthReferencePadding;

		// Token: 0x04000577 RID: 1399
		[Header("Height")]
		[SerializeField]
		private UIContentSizeFitter.FitMode verticalFit;

		// Token: 0x04000578 RID: 1400
		[SerializeField]
		private float manualVerticalSize;

		// Token: 0x04000579 RID: 1401
		[SerializeField]
		private UIContentSizeFitter.MaxSizeMode maxSizeModeHeight;

		// Token: 0x0400057A RID: 1402
		[SerializeField]
		private float maxHeight = 100f;

		// Token: 0x0400057B RID: 1403
		[SerializeField]
		private RectTransform maxHeightReference;

		// Token: 0x0400057C RID: 1404
		[SerializeField]
		private float maxHeightReferencePadding;

		// Token: 0x0400057D RID: 1405
		[Header("Tweening")]
		[SerializeField]
		private bool tweenToSize;

		// Token: 0x0400057E RID: 1406
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400057F RID: 1407
		[SerializeField]
		private List<RectTransform> layoutGroupChildren = new List<RectTransform>();

		// Token: 0x04000580 RID: 1408
		private readonly RectOffset emptyRectOffset = new RectOffset();

		// Token: 0x04000581 RID: 1409
		private TweenSizeDelta tweenSizeDelta;

		// Token: 0x04000582 RID: 1410
		private Vector2 lastLayoutSize;

		// Token: 0x0200016B RID: 363
		public enum FitMode
		{
			// Token: 0x04000584 RID: 1412
			Unconstrained,
			// Token: 0x04000585 RID: 1413
			MinSize,
			// Token: 0x04000586 RID: 1414
			PreferredSize,
			// Token: 0x04000587 RID: 1415
			Manual
		}

		// Token: 0x0200016C RID: 364
		public enum MaxSizeMode
		{
			// Token: 0x04000589 RID: 1417
			DoNotUse,
			// Token: 0x0400058A RID: 1418
			Explicit,
			// Token: 0x0400058B RID: 1419
			Reference
		}
	}
}
