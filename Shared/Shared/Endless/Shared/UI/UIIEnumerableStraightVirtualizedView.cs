using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001F8 RID: 504
	public class UIIEnumerableStraightVirtualizedView : UIBaseIEnumerableView, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
	{
		// Token: 0x14000043 RID: 67
		// (add) Token: 0x06000CE8 RID: 3304 RVA: 0x00037F34 File Offset: 0x00036134
		// (remove) Token: 0x06000CE9 RID: 3305 RVA: 0x00037F6C File Offset: 0x0003616C
		public event Action<Vector2, float> Scrolled;

		// Token: 0x14000044 RID: 68
		// (add) Token: 0x06000CEA RID: 3306 RVA: 0x00037FA4 File Offset: 0x000361A4
		// (remove) Token: 0x06000CEB RID: 3307 RVA: 0x00037FDC File Offset: 0x000361DC
		public event Action<UIIEnumerableItem> Snapped;

		// Token: 0x14000045 RID: 69
		// (add) Token: 0x06000CEC RID: 3308 RVA: 0x00038014 File Offset: 0x00036214
		// (remove) Token: 0x06000CED RID: 3309 RVA: 0x0003804C File Offset: 0x0003624C
		public event Action<bool> ScrollingChanged;

		// Token: 0x14000046 RID: 70
		// (add) Token: 0x06000CEE RID: 3310 RVA: 0x00038084 File Offset: 0x00036284
		// (remove) Token: 0x06000CEF RID: 3311 RVA: 0x000380BC File Offset: 0x000362BC
		public event Action<bool> ScrollerTweeningChanged;

		// Token: 0x14000047 RID: 71
		// (add) Token: 0x06000CF0 RID: 3312 RVA: 0x000380F4 File Offset: 0x000362F4
		// (remove) Token: 0x06000CF1 RID: 3313 RVA: 0x0003812C File Offset: 0x0003632C
		public event Func<float, float, float, float> CustomTweenFunction;

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x06000CF2 RID: 3314 RVA: 0x00038161 File Offset: 0x00036361
		// (set) Token: 0x06000CF3 RID: 3315 RVA: 0x00038169 File Offset: 0x00036369
		public float LookAheadBefore
		{
			get
			{
				return this.lookAheadBefore;
			}
			set
			{
				this.lookAheadBefore = Mathf.Abs(value);
			}
		}

		// Token: 0x1700025E RID: 606
		// (get) Token: 0x06000CF4 RID: 3316 RVA: 0x00038177 File Offset: 0x00036377
		// (set) Token: 0x06000CF5 RID: 3317 RVA: 0x0003817F File Offset: 0x0003637F
		public float LookAheadAfter
		{
			get
			{
				return this.lookAheadAfter;
			}
			set
			{
				this.lookAheadAfter = Mathf.Abs(value);
			}
		}

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x06000CF6 RID: 3318 RVA: 0x0003818D File Offset: 0x0003638D
		// (set) Token: 0x06000CF7 RID: 3319 RVA: 0x00038195 File Offset: 0x00036395
		public float CalculateStartItemBias { get; set; }

		// Token: 0x17000260 RID: 608
		// (get) Token: 0x06000CF8 RID: 3320 RVA: 0x0003819E File Offset: 0x0003639E
		// (set) Token: 0x06000CF9 RID: 3321 RVA: 0x000381A8 File Offset: 0x000363A8
		public float ScrollPosition
		{
			get
			{
				return this.scrollPosition;
			}
			set
			{
				if (this.loop)
				{
					if (value > this.jumpAfterLoopThreshold)
					{
						value = this.lastLoopedScrollPosition + (value - this.jumpAfterLoopThreshold);
					}
					else if (value < this.jumpAfterLoopThreshold)
					{
						value = this.lastLoopedScrollPosition - (this.jumpAfterLoopThreshold - value);
					}
				}
				else
				{
					value = Mathf.Clamp(value, 0f, this.ScrollSize);
				}
				if (Mathf.Approximately(this.scrollPosition, value))
				{
					return;
				}
				this.scrollPosition = value;
				if (this.scrollDirection == Orientation.Vertical)
				{
					this.scrollRect.verticalNormalizedPosition = 1f - this.scrollPosition / this.ScrollSize;
					return;
				}
				this.scrollRect.horizontalNormalizedPosition = this.scrollPosition / this.ScrollSize;
			}
		}

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x06000CFA RID: 3322 RVA: 0x00038260 File Offset: 0x00036460
		public float ScrollSize
		{
			get
			{
				if (this.scrollDirection != Orientation.Vertical)
				{
					return Mathf.Max(this.scrollRect.content.rect.width - this.ScrollRectTransform.rect.width, 0f);
				}
				return Mathf.Max(this.scrollRect.content.rect.height - this.ScrollRectTransform.rect.height, 0f);
			}
		}

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x06000CFB RID: 3323 RVA: 0x000382E3 File Offset: 0x000364E3
		public float NormalizedScrollPosition
		{
			get
			{
				if (this.ScrollPosition > 0f)
				{
					return this.scrollPosition / this.ScrollSize;
				}
				return 0f;
			}
		}

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x06000CFC RID: 3324 RVA: 0x00038305 File Offset: 0x00036505
		// (set) Token: 0x06000CFD RID: 3325 RVA: 0x00038310 File Offset: 0x00036510
		public bool Loop
		{
			get
			{
				return this.loop;
			}
			set
			{
				if (this.loop == value)
				{
					return;
				}
				float num = this.scrollPosition;
				this.loop = value;
				this.Resize(false);
				if (this.loop)
				{
					this.ScrollPosition = this.firstLoopedScrollPosition + num;
				}
				else
				{
					this.ScrollPosition = num - this.firstLoopedScrollPosition;
				}
				this.ScrollbarVisibility = this.scrollbarVisibility;
			}
		}

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x06000CFE RID: 3326 RVA: 0x0003836E File Offset: 0x0003656E
		// (set) Token: 0x06000CFF RID: 3327 RVA: 0x00038378 File Offset: 0x00036578
		public UIIEnumerableStraightVirtualizedView.ScrollbarVisibilityEnum ScrollbarVisibility
		{
			get
			{
				return this.scrollbarVisibility;
			}
			set
			{
				this.scrollbarVisibility = value;
				if (!this.scrollbar)
				{
					return;
				}
				List<float> list = this.itemOffsetList;
				if (list == null || list.Count <= 0)
				{
					return;
				}
				if (this.scrollDirection == Orientation.Vertical)
				{
					this.scrollRect.verticalScrollbar = this.scrollbar;
				}
				else
				{
					this.scrollRect.horizontalScrollbar = this.scrollbar;
				}
				List<float> list2 = this.itemOffsetList;
				if (list2[list2.Count - 1] < this.ScrollRectSize || this.loop)
				{
					this.scrollbar.gameObject.SetActive(this.scrollbarVisibility == UIIEnumerableStraightVirtualizedView.ScrollbarVisibilityEnum.Always);
				}
				else
				{
					this.scrollbar.gameObject.SetActive(this.scrollbarVisibility != UIIEnumerableStraightVirtualizedView.ScrollbarVisibilityEnum.Never);
				}
				if (this.scrollbar.gameObject.activeSelf)
				{
					return;
				}
				this.scrollRect.verticalScrollbar = null;
				this.scrollRect.horizontalScrollbar = null;
			}
		}

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x06000D00 RID: 3328 RVA: 0x00038460 File Offset: 0x00036660
		// (set) Token: 0x06000D01 RID: 3329 RVA: 0x0003846D File Offset: 0x0003666D
		public Vector2 Velocity
		{
			get
			{
				return this.scrollRect.velocity;
			}
			set
			{
				this.scrollRect.velocity = value;
			}
		}

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x06000D02 RID: 3330 RVA: 0x0003847B File Offset: 0x0003667B
		// (set) Token: 0x06000D03 RID: 3331 RVA: 0x000384A7 File Offset: 0x000366A7
		public float LinearVelocity
		{
			get
			{
				if (this.scrollDirection != Orientation.Vertical)
				{
					return this.scrollRect.velocity.x;
				}
				return this.scrollRect.velocity.y;
			}
			set
			{
				if (this.scrollDirection == Orientation.Vertical)
				{
					this.scrollRect.velocity = new Vector2(0f, value);
					return;
				}
				this.scrollRect.velocity = new Vector2(value, 0f);
			}
		}

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x06000D04 RID: 3332 RVA: 0x000384DF File Offset: 0x000366DF
		// (set) Token: 0x06000D05 RID: 3333 RVA: 0x000384E7 File Offset: 0x000366E7
		public bool IsScrolling { get; private set; }

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06000D06 RID: 3334 RVA: 0x000384F0 File Offset: 0x000366F0
		// (set) Token: 0x06000D07 RID: 3335 RVA: 0x000384F8 File Offset: 0x000366F8
		public bool IsTweening { get; private set; }

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x06000D08 RID: 3336 RVA: 0x00038501 File Offset: 0x00036701
		// (set) Token: 0x06000D09 RID: 3337 RVA: 0x00038509 File Offset: 0x00036709
		public bool IsDragging { get; private set; }

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x06000D0A RID: 3338 RVA: 0x00038512 File Offset: 0x00036712
		// (set) Token: 0x06000D0B RID: 3339 RVA: 0x0003851A File Offset: 0x0003671A
		public int FirstVisibleViewIndex { get; private set; }

		// Token: 0x1700026B RID: 619
		// (get) Token: 0x06000D0C RID: 3340 RVA: 0x00038523 File Offset: 0x00036723
		// (set) Token: 0x06000D0D RID: 3341 RVA: 0x0003852B File Offset: 0x0003672B
		public int LastVisibleViewIndex { get; private set; }

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x06000D0E RID: 3342 RVA: 0x00038534 File Offset: 0x00036734
		public int StartDataIndex
		{
			get
			{
				return this.FirstVisibleViewIndex % this.GetCount();
			}
		}

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x06000D0F RID: 3343 RVA: 0x00038548 File Offset: 0x00036748
		public int EndDataIndex
		{
			get
			{
				return this.LastVisibleViewIndex % this.GetCount();
			}
		}

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x06000D10 RID: 3344 RVA: 0x0003855C File Offset: 0x0003675C
		public int ActiveItemCount
		{
			get
			{
				return this.visibleItems.Count;
			}
		}

		// Token: 0x1700026F RID: 623
		// (get) Token: 0x06000D11 RID: 3345 RVA: 0x0003856C File Offset: 0x0003676C
		public float ScrollRectSize
		{
			get
			{
				if (this.scrollDirection != Orientation.Vertical)
				{
					return this.ScrollRectTransform.rect.width;
				}
				return this.ScrollRectTransform.rect.height;
			}
		}

		// Token: 0x17000270 RID: 624
		// (get) Token: 0x06000D12 RID: 3346 RVA: 0x000385A9 File Offset: 0x000367A9
		// (set) Token: 0x06000D13 RID: 3347 RVA: 0x000385BF File Offset: 0x000367BF
		protected override HorizontalOrVerticalLayoutGroup HorizontalOrVerticalLayoutGroup
		{
			get
			{
				if (!this.initialized)
				{
					this.Initialize();
				}
				return base.HorizontalOrVerticalLayoutGroup;
			}
			set
			{
				base.HorizontalOrVerticalLayoutGroup = value;
			}
		}

		// Token: 0x17000271 RID: 625
		// (get) Token: 0x06000D14 RID: 3348 RVA: 0x000385C8 File Offset: 0x000367C8
		// (set) Token: 0x06000D15 RID: 3349 RVA: 0x000385D0 File Offset: 0x000367D0
		private LayoutElement FirstPadding { get; set; }

		// Token: 0x17000272 RID: 626
		// (get) Token: 0x06000D16 RID: 3350 RVA: 0x000385D9 File Offset: 0x000367D9
		// (set) Token: 0x06000D17 RID: 3351 RVA: 0x000385E1 File Offset: 0x000367E1
		private LayoutElement LastPadding { get; set; }

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06000D18 RID: 3352 RVA: 0x000385EA File Offset: 0x000367EA
		private RectTransform ScrollRectTransform
		{
			get
			{
				return this.scrollRect.RectTransform;
			}
		}

		// Token: 0x06000D19 RID: 3353 RVA: 0x000385F7 File Offset: 0x000367F7
		public override void Validate()
		{
			base.Validate();
			if (this.initialized && !Mathf.Approximately(this.spacing, this.HorizontalOrVerticalLayoutGroup.spacing))
			{
				this.updateSpacing = true;
			}
		}

		// Token: 0x06000D1A RID: 3354 RVA: 0x00038626 File Offset: 0x00036826
		protected override void OnEnable()
		{
			base.OnEnable();
			this.scrollRect.onValueChanged.AddListener(new UnityAction<Vector2>(this.OnScroll));
		}

		// Token: 0x06000D1B RID: 3355 RVA: 0x0003864B File Offset: 0x0003684B
		private void Awake()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
		}

		// Token: 0x06000D1C RID: 3356 RVA: 0x00038674 File Offset: 0x00036874
		private void Update()
		{
			if (this.updateSpacing)
			{
				this.SetSpacing(this.spacing);
				this.reloadData = false;
			}
			if (this.reloadData)
			{
				this.ReloadData(0f);
			}
			if ((this.loop && !Mathf.Approximately(this.lastScrollRectSize, this.ScrollRectSize)) || this.loop != this.lastLoop)
			{
				this.Resize(true);
				this.lastScrollRectSize = this.ScrollRectSize;
				this.lastLoop = this.loop;
			}
			if (this.lastScrollbarVisibility != this.scrollbarVisibility)
			{
				this.ScrollbarVisibility = this.scrollbarVisibility;
				this.lastScrollbarVisibility = this.scrollbarVisibility;
			}
			if (this.LinearVelocity == 0f || this.IsScrolling)
			{
				if (this.LinearVelocity == 0f && this.IsScrolling)
				{
					this.IsScrolling = false;
					Action<bool> scrollingChanged = this.ScrollingChanged;
					if (scrollingChanged == null)
					{
						return;
					}
					scrollingChanged(false);
				}
				return;
			}
			this.IsScrolling = true;
			Action<bool> scrollingChanged2 = this.ScrollingChanged;
			if (scrollingChanged2 == null)
			{
				return;
			}
			scrollingChanged2(true);
		}

		// Token: 0x06000D1D RID: 3357 RVA: 0x00038778 File Offset: 0x00036978
		private void LateUpdate()
		{
			if (this.maxVelocity <= 0f)
			{
				return;
			}
			this.Velocity = ((this.scrollDirection == Orientation.Horizontal) ? new Vector2(Mathf.Clamp(Mathf.Abs(this.Velocity.x), 0f, this.maxVelocity) * Mathf.Sign(this.Velocity.x), this.Velocity.y) : new Vector2(this.Velocity.x, Mathf.Clamp(Mathf.Abs(this.Velocity.y), 0f, this.maxVelocity) * Mathf.Sign(this.Velocity.y)));
		}

		// Token: 0x06000D1E RID: 3358 RVA: 0x00038825 File Offset: 0x00036A25
		protected override void OnDisable()
		{
			base.OnDisable();
			this.scrollRect.onValueChanged.RemoveListener(new UnityAction<Vector2>(this.OnScroll));
		}

		// Token: 0x06000D1F RID: 3359 RVA: 0x0003884C File Offset: 0x00036A4C
		public override void ReviewSelectedStatus()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ReviewSelectedStatus", Array.Empty<object>());
			}
			foreach (UIIEnumerableItem uiienumerableItem in this.visibleItems)
			{
				bool flag = this.GetIsSelected(uiienumerableItem.ModelAsObject);
				uiienumerableItem.ViewSelectedStatus(this, flag);
			}
		}

		// Token: 0x06000D20 RID: 3360 RVA: 0x000388CC File Offset: 0x00036ACC
		private void OnRectTransformDimensionsChange()
		{
			if (this.GetCount == null)
			{
				return;
			}
			MonoBehaviourSingleton<UICoroutineManager>.Instance.WaitFramesAndInvoke(new Action(this.ResizeAndKeepPosition), 1);
		}

		// Token: 0x06000D21 RID: 3361 RVA: 0x000388F0 File Offset: 0x00036AF0
		public void OnBeginDrag(PointerEventData data)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBeginDrag", new object[] { data });
			}
			this.IsDragging = true;
			this.dragFingerCount++;
			if (this.dragFingerCount > 1)
			{
				return;
			}
			this.snapBeforeDrag = this.snapping;
			if (!this.snapWhileDragging)
			{
				this.snapping = false;
			}
			this.loopBeforeDrag = this.loop;
			if (!this.loopWhileDragging)
			{
				this.loop = false;
			}
			if (this.IsTweening && this.interruptTweeningOnDrag)
			{
				this.interruptTween = true;
			}
		}

		// Token: 0x06000D22 RID: 3362 RVA: 0x00038986 File Offset: 0x00036B86
		public void OnDrag(PointerEventData data)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDrag", new object[] { data });
			}
			this.dragPreviousPos = data.position;
		}

		// Token: 0x06000D23 RID: 3363 RVA: 0x000389B4 File Offset: 0x00036BB4
		public void OnEndDrag(PointerEventData data)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEndDrag", new object[] { data });
			}
			this.IsDragging = false;
			this.dragFingerCount--;
			if (this.dragFingerCount < 0)
			{
				this.dragFingerCount = 0;
			}
			this.snapping = this.snapBeforeDrag;
			this.loop = this.loopBeforeDrag;
			if (this.forceSnapOnEndDrag && this.snapping && this.dragPreviousPos == data.position && !this.snapJumping)
			{
				this.Snap();
			}
		}

		// Token: 0x06000D24 RID: 3364 RVA: 0x00038A4C File Offset: 0x00036C4C
		public void OnPointerDown(PointerEventData data)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerDown", new object[] { data });
			}
			if (this.IsTweening && this.interruptTweeningOnPointerDown)
			{
				this.interruptTween = true;
			}
		}

		// Token: 0x06000D25 RID: 3365 RVA: 0x00038A84 File Offset: 0x00036C84
		public void OnPointerUp(PointerEventData data)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerUp", new object[] { data });
			}
			if (this.IsDragging)
			{
				this.snapping = this.snapBeforeDrag;
			}
			if (!this.forceSnapOnPointerUp || !this.snapping || Mathf.Abs(this.LinearVelocity) >= 1f)
			{
				return;
			}
			this.interruptTween = false;
			this.snapJumping = false;
			this.IsTweening = false;
			this.Snap();
		}

		// Token: 0x06000D26 RID: 3366 RVA: 0x00038B00 File Offset: 0x00036D00
		public override void View(IEnumerable model)
		{
			base.View(model);
			this.ReloadData(0f);
			if (base.IsReview)
			{
				this.scrollRect.normalizedPosition = base.CachedNormalizedPosition;
			}
			this.dynamicLayoutElement.PreferredHeightLayoutDimension.ExplicitValue = base.ContentSize;
		}

		// Token: 0x06000D27 RID: 3367 RVA: 0x00038B4E File Offset: 0x00036D4E
		public void ReloadDataAndKeepPosition()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ReloadDataAndKeepPosition", Array.Empty<object>());
			}
			this.ReloadData(this.ScrollPosition);
		}

		// Token: 0x06000D28 RID: 3368 RVA: 0x00038B74 File Offset: 0x00036D74
		public void ReloadData(float scrollPositionFactor = 0f)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ReloadData", new object[] { scrollPositionFactor });
			}
			this.reloadData = false;
			this.DespawnAllItems();
			this.Resize(false);
			if (!this.scrollRect || !this.ScrollRectTransform || !this.scrollRect.content)
			{
				this.scrollPosition = 0f;
				return;
			}
			this.scrollPosition = Mathf.Clamp(scrollPositionFactor * this.ScrollSize, 0f, this.ScrollSize);
			if (this.scrollDirection == Orientation.Vertical)
			{
				this.scrollRect.verticalNormalizedPosition = 1f - scrollPositionFactor;
			}
			else
			{
				this.scrollRect.horizontalNormalizedPosition = scrollPositionFactor;
			}
			this.RefreshActive();
		}

		// Token: 0x06000D29 RID: 3369 RVA: 0x00038C3C File Offset: 0x00036E3C
		public void ReviewVisibleItemsIfModelChanged()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ReviewVisibleItemsIfModelChanged", Array.Empty<object>());
			}
			foreach (UIIEnumerableItem uiienumerableItem in this.visibleItems)
			{
				object obj = this.GetItem(uiienumerableItem.DataIndex);
				if (!object.Equals(uiienumerableItem.ModelAsObject, obj))
				{
					uiienumerableItem.View(obj, this, uiienumerableItem.DataIndex, uiienumerableItem.ViewIndex);
				}
			}
		}

		// Token: 0x06000D2A RID: 3370 RVA: 0x00038CD4 File Offset: 0x00036ED4
		public override void SetInteractable(bool interactable)
		{
			base.SetInteractable(interactable);
			foreach (UIIEnumerableItem uiienumerableItem in this.visibleItems)
			{
				uiienumerableItem.SetInteractable(interactable);
			}
		}

		// Token: 0x06000D2B RID: 3371 RVA: 0x00038D2C File Offset: 0x00036F2C
		public override void SetChildIEnumerableCanAddAndRemoveItems(bool canAddAndRemoveItems)
		{
			base.SetChildIEnumerableCanAddAndRemoveItems(canAddAndRemoveItems);
			foreach (UIIEnumerableItem uiienumerableItem in this.visibleItems)
			{
				uiienumerableItem.OnChildIEnumerableCanAddAndRemoveItemsChanged(canAddAndRemoveItems);
			}
		}

		// Token: 0x06000D2C RID: 3372 RVA: 0x00038D84 File Offset: 0x00036F84
		public void ToggleLoop()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleLoop", Array.Empty<object>());
			}
			this.Loop = !this.loop;
		}

		// Token: 0x06000D2D RID: 3373 RVA: 0x00038DAD File Offset: 0x00036FAD
		public void IgnoreLoopJump(bool ignore)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "IgnoreLoopJump", new object[] { ignore });
			}
			this.ignoreLoopJump = ignore;
		}

		// Token: 0x06000D2E RID: 3374 RVA: 0x00038DD8 File Offset: 0x00036FD8
		public void SetScrollPositionImmediately(float scrollPosition)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetScrollPositionImmediately", new object[] { scrollPosition });
			}
			this.ScrollPosition = scrollPosition;
			this.RefreshActive();
		}

		// Token: 0x06000D2F RID: 3375 RVA: 0x00038E0C File Offset: 0x0003700C
		public void JumpToDataIndex(int dataIndex, float scrollerOffset = 0f, float itemOffset = 0f, bool useSpacing = true, TweenEase tweenType = TweenEase.Immediate, float tweenTime = 0f, Action jumpComplete = null, UIIEnumerableStraightVirtualizedView.LoopJumpDirectionEnum loopJumpDirection = UIIEnumerableStraightVirtualizedView.LoopJumpDirectionEnum.Closest, bool forceCalculateRange = false)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "JumpToDataIndex", new object[] { dataIndex, scrollerOffset, itemOffset, useSpacing, tweenType, tweenTime, jumpComplete, loopJumpDirection, forceCalculateRange });
			}
			float num = 0f;
			if (itemOffset != 0f)
			{
				float num2 = base.GetItemSize(dataIndex);
				if (useSpacing)
				{
					num2 += this.spacing;
					if (dataIndex > 0 && dataIndex < this.GetCount() - 1)
					{
						num2 += this.spacing;
					}
				}
				num = num2 * itemOffset;
			}
			if (Mathf.Approximately(scrollerOffset, 1f))
			{
				num += (float)this.padding.bottom;
			}
			float num3 = -(scrollerOffset * this.ScrollRectSize) + num;
			float num4 = 0f;
			if (this.loop)
			{
				int num5 = this.GetCount();
				int num6 = this.loopFirstItemViewIndex - (num5 - dataIndex);
				int num7 = this.loopFirstItemViewIndex + dataIndex;
				int num8 = this.loopFirstItemViewIndex + num5 + dataIndex;
				float num9 = this.GetScrollPositionForItemViewIndex(num6, UIIEnumerableStraightVirtualizedView.ItemPosition.Before) + num3;
				float num10 = this.GetScrollPositionForItemViewIndex(num7, UIIEnumerableStraightVirtualizedView.ItemPosition.Before) + num3;
				float num11 = this.GetScrollPositionForItemViewIndex(num8, UIIEnumerableStraightVirtualizedView.ItemPosition.Before) + num3;
				float num12 = Mathf.Abs(this.scrollPosition - num9);
				float num13 = Mathf.Abs(this.scrollPosition - num10);
				float num14 = Mathf.Abs(this.scrollPosition - num11);
				float num15 = -(scrollerOffset * this.ScrollRectSize);
				int num16 = 0;
				int num17 = 0;
				int num18 = 0;
				if (loopJumpDirection == UIIEnumerableStraightVirtualizedView.LoopJumpDirectionEnum.Up || loopJumpDirection == UIIEnumerableStraightVirtualizedView.LoopJumpDirectionEnum.Down)
				{
					num17 = this.GetItemViewIndexAtScrollPosition(this.scrollPosition - num15 + 0.0001f);
					if (num17 < num5)
					{
						num16 = 1;
						num18 = dataIndex;
					}
					else if (num17 >= num5 && num17 < num5 * 2)
					{
						num16 = 2;
						num18 = dataIndex + num5;
					}
					else
					{
						num16 = 3;
						num18 = dataIndex + num5 * 2;
					}
				}
				switch (loopJumpDirection)
				{
				case UIIEnumerableStraightVirtualizedView.LoopJumpDirectionEnum.Closest:
					if (num12 < num13)
					{
						num4 = ((num12 < num14) ? num9 : num11);
					}
					else
					{
						num4 = ((num13 < num14) ? num10 : num11);
					}
					break;
				case UIIEnumerableStraightVirtualizedView.LoopJumpDirectionEnum.Up:
					if (num18 < num17)
					{
						num4 = ((num16 == 1) ? num9 : ((num16 == 2) ? num10 : num11));
					}
					else if (num16 == 1 && num17 == dataIndex)
					{
						num4 = num9 - this.loopSetScrollSize;
					}
					else
					{
						num4 = ((num16 == 1) ? num11 : ((num16 == 2) ? num9 : num10));
					}
					break;
				case UIIEnumerableStraightVirtualizedView.LoopJumpDirectionEnum.Down:
					if (num18 > num17)
					{
						num4 = ((num16 == 1) ? num9 : ((num16 == 2) ? num10 : num11));
					}
					else if (num16 == 3 && num17 == num18)
					{
						num4 = num11 + this.loopSetScrollSize;
					}
					else
					{
						num4 = ((num16 == 1) ? num10 : ((num16 == 2) ? num11 : num9));
					}
					break;
				}
				if (useSpacing)
				{
					num4 -= this.spacing;
				}
			}
			else
			{
				num4 = this.GetScrollPositionForDataIndex(dataIndex, UIIEnumerableStraightVirtualizedView.ItemPosition.Before) + num3;
				num4 = Mathf.Clamp(num4 - (useSpacing ? this.spacing : 0f), 0f, this.ScrollSize);
			}
			if (Mathf.Approximately(num4, this.scrollPosition))
			{
				if (jumpComplete != null)
				{
					jumpComplete();
				}
				return;
			}
			base.StartCoroutine(this.TweenPosition(tweenType, tweenTime, this.ScrollPosition, num4, jumpComplete, forceCalculateRange));
		}

		// Token: 0x06000D30 RID: 3376 RVA: 0x0003913C File Offset: 0x0003733C
		public void Snap()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Snap", Array.Empty<object>());
			}
			if (this.GetCount() == 0)
			{
				return;
			}
			this.snapJumping = true;
			this.LinearVelocity = 0f;
			this.snapInertia = this.scrollRect.inertia;
			this.scrollRect.inertia = false;
			float num = this.ScrollPosition + this.ScrollRectSize * Mathf.Clamp01(this.snapWatchOffset);
			this.snapItemViewIndex = this.GetItemViewIndexAtScrollPosition(num);
			this.snapDataIndex = this.snapItemViewIndex % this.GetCount();
			this.JumpToDataIndex(this.snapDataIndex, this.snapJumpToOffset, this.snapItemCenterOffset, this.snapUseItemSpacing, this.snapTweenType, this.snapTweenTime, new Action(this.SnapJumpComplete), UIIEnumerableStraightVirtualizedView.LoopJumpDirectionEnum.Closest, false);
		}

		// Token: 0x06000D31 RID: 3377 RVA: 0x00039218 File Offset: 0x00037418
		public float GetScrollPositionForItemViewIndex(int itemViewIndex, UIIEnumerableStraightVirtualizedView.ItemPosition insertPosition)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetScrollPositionForItemViewIndex", new object[] { itemViewIndex, insertPosition });
			}
			if (this.GetCount() == 0)
			{
				return 0f;
			}
			if (itemViewIndex < 0)
			{
				itemViewIndex = 0;
			}
			if (itemViewIndex == 0 && insertPosition == UIIEnumerableStraightVirtualizedView.ItemPosition.Before)
			{
				return (float)((this.scrollDirection == Orientation.Vertical) ? this.padding.top : this.padding.left);
			}
			if (itemViewIndex >= this.itemOffsetList.Count)
			{
				List<float> list = this.itemOffsetList;
				return list[list.Count - 2];
			}
			if (insertPosition == UIIEnumerableStraightVirtualizedView.ItemPosition.Before)
			{
				return this.itemOffsetList[itemViewIndex - 1] + this.spacing + (float)((this.scrollDirection == Orientation.Vertical) ? this.padding.top : this.padding.left);
			}
			return this.itemOffsetList[itemViewIndex] + (float)((this.scrollDirection == Orientation.Vertical) ? this.padding.top : this.padding.left);
		}

		// Token: 0x06000D32 RID: 3378 RVA: 0x00039320 File Offset: 0x00037520
		public float GetScrollPositionForDataIndex(int dataIndex, UIIEnumerableStraightVirtualizedView.ItemPosition insertPosition)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetScrollPositionForDataIndex", new object[] { dataIndex, insertPosition });
			}
			return this.GetScrollPositionForItemViewIndex(this.loop ? (this.GetCount() + dataIndex) : dataIndex, insertPosition);
		}

		// Token: 0x06000D33 RID: 3379 RVA: 0x00039377 File Offset: 0x00037577
		public int GetItemViewIndexAtScrollPosition(float scrollPosition)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetItemViewIndexAtScrollPosition", new object[] { scrollPosition });
			}
			return this.GetItemViewIndexAtPosition(scrollPosition, 0, this.itemOffsetList.Count - 1);
		}

		// Token: 0x06000D34 RID: 3380 RVA: 0x000393B0 File Offset: 0x000375B0
		public UIIEnumerableItem GetItemViewAtDataIndex(int dataIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetItemViewAtDataIndex", new object[] { dataIndex });
			}
			foreach (UIIEnumerableItem uiienumerableItem in this.visibleItems)
			{
				if (uiienumerableItem.DataIndex == dataIndex)
				{
					return uiienumerableItem;
				}
			}
			return null;
		}

		// Token: 0x06000D35 RID: 3381 RVA: 0x00039430 File Offset: 0x00037630
		public void ToggleTweenPaused(float newTweenTime = -1f)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleTweenPaused", new object[] { newTweenTime });
			}
			if (!this.tweenPaused)
			{
				this.tweenPaused = true;
				this.tweenPauseToggledOff = false;
				return;
			}
			this.tweenPaused = false;
			this.tweenPauseToggledOff = true;
			this.tweenPauseNewTweenTime = newTweenTime;
		}

		// Token: 0x06000D36 RID: 3382 RVA: 0x0003948B File Offset: 0x0003768B
		public void InterruptTween()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InterruptTween", Array.Empty<object>());
			}
			if (this.IsTweening)
			{
				this.interruptTween = true;
			}
		}

		// Token: 0x06000D37 RID: 3383 RVA: 0x000394B4 File Offset: 0x000376B4
		[ContextMenu("ResizeAndKeepPosition")]
		public void ResizeAndKeepPosition()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ResizeAndKeepPosition", Array.Empty<object>());
			}
			this.Resize(true);
		}

		// Token: 0x06000D38 RID: 3384 RVA: 0x000394D5 File Offset: 0x000376D5
		protected override void ResetViewState()
		{
			base.ResetViewState();
			this.DespawnAllItems();
		}

		// Token: 0x06000D39 RID: 3385 RVA: 0x000394E3 File Offset: 0x000376E3
		protected override void DespawnItem(UIIEnumerableItem iEnumerableItem)
		{
			this.visibleItems.Remove(iEnumerableItem);
			base.DespawnItem(iEnumerableItem);
		}

		// Token: 0x06000D3A RID: 3386 RVA: 0x000394FC File Offset: 0x000376FC
		protected override void OnScroll(Vector2 scrollRectValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScroll", new object[] { scrollRectValue });
			}
			if (this.scrollDirection == Orientation.Vertical)
			{
				this.scrollPosition = (1f - scrollRectValue.y) * this.ScrollSize;
			}
			else
			{
				this.scrollPosition = scrollRectValue.x * this.ScrollSize;
			}
			this.scrollPosition = Mathf.Clamp(this.scrollPosition, 0f, this.ScrollSize);
			Action<Vector2, float> scrolled = this.Scrolled;
			if (scrolled != null)
			{
				scrolled(scrollRectValue, this.scrollPosition);
			}
			if (this.snapping && !this.snapJumping && Mathf.Abs(this.LinearVelocity) <= this.snapVelocityThreshold && this.LinearVelocity != 0f)
			{
				this.Snap();
			}
			this.RefreshActive();
		}

		// Token: 0x06000D3B RID: 3387 RVA: 0x000395D4 File Offset: 0x000377D4
		private void Initialize()
		{
			UIIEnumerableStraightVirtualizedView.<>c__DisplayClass179_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (this.initialized)
			{
				return;
			}
			this.initialized = true;
			this.<Initialize>g__SetUpScrollRect|179_3(ref CS$<>8__locals1);
			CS$<>8__locals1.viewportRectTransform = this.<Initialize>g__SetUpViewport|179_0(ref CS$<>8__locals1);
			this.<Initialize>g__SetUpContent|179_1(ref CS$<>8__locals1);
			this.<Initialize>g__SetUpLayout|179_2(ref CS$<>8__locals1);
			this.<Initialize>g__SetUpPadding|179_4(ref CS$<>8__locals1);
			this.lastScrollRectSize = this.ScrollRectSize;
			this.lastLoop = this.loop;
			this.lastScrollbarVisibility = this.scrollbarVisibility;
		}

		// Token: 0x06000D3C RID: 3388 RVA: 0x00039664 File Offset: 0x00037864
		public void Resize(bool keepPosition)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Resize", new object[] { keepPosition });
			}
			float num = this.scrollPosition;
			this.itemSizeList.Clear();
			float num2 = this.AddItemViewSizes();
			if (this.loop)
			{
				int count = this.itemSizeList.Count;
				if (num2 < this.ScrollRectSize)
				{
					int num3 = Mathf.CeilToInt((float)Mathf.CeilToInt(this.ScrollRectSize / num2) / 2f) * 2;
					this.DuplicateItemViewSizes(num3, count);
					this.loopFirstItemViewIndex = count * (1 + num3 / 2);
				}
				else
				{
					this.loopFirstItemViewIndex = count;
				}
				this.loopLastItemViewIndex = this.loopFirstItemViewIndex + count - 1;
				this.DuplicateItemViewSizes(2, count);
			}
			this.CalculateItemViewOffsets();
			float num4;
			if (this.itemOffsetList.Count != 0)
			{
				List<float> list = this.itemOffsetList;
				num4 = list[list.Count - 1];
			}
			else
			{
				num4 = 0f;
			}
			float num5 = num4;
			this.scrollRect.content.sizeDelta = ((this.scrollDirection == Orientation.Vertical) ? new Vector2(this.scrollRect.content.sizeDelta.x, num5 + (float)this.padding.top + (float)this.padding.bottom) : new Vector2(num5 + (float)this.padding.left + (float)this.padding.right, this.scrollRect.content.sizeDelta.y));
			if (this.loop)
			{
				this.firstLoopedScrollPosition = this.GetScrollPositionForItemViewIndex(this.loopFirstItemViewIndex, UIIEnumerableStraightVirtualizedView.ItemPosition.Before) + this.spacing * 0.5f;
				this.lastLoopedScrollPosition = this.GetScrollPositionForItemViewIndex(this.loopLastItemViewIndex, UIIEnumerableStraightVirtualizedView.ItemPosition.After) - this.ScrollRectSize + this.spacing * 0.5f;
				this.jumpBeforeLoopThreshold = this.firstLoopedScrollPosition - this.ScrollRectSize;
				this.jumpAfterLoopThreshold = this.lastLoopedScrollPosition + this.ScrollRectSize;
			}
			this.ResetVisibleItemViews();
			if (keepPosition)
			{
				this.ScrollPosition = num;
			}
			else if (this.loop)
			{
				this.ScrollPosition = this.firstLoopedScrollPosition;
			}
			else
			{
				this.ScrollPosition = 0f;
			}
			this.ScrollbarVisibility = this.scrollbarVisibility;
		}

		// Token: 0x06000D3D RID: 3389 RVA: 0x00039888 File Offset: 0x00037A88
		public void ReapplyItemSizes()
		{
			foreach (UIIEnumerableItem uiienumerableItem in this.visibleItems)
			{
				if (uiienumerableItem.DataIndex >= 0 && uiienumerableItem.DataIndex < this.GetCount())
				{
					float itemSize = base.GetItemSize(uiienumerableItem.DataIndex);
					if (base.Vertical)
					{
						uiienumerableItem.SetHeight(itemSize);
					}
					else
					{
						uiienumerableItem.SetWidth(itemSize);
					}
				}
			}
		}

		// Token: 0x06000D3E RID: 3390 RVA: 0x00039918 File Offset: 0x00037B18
		private void SetSpacing(float spacing)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetSpacing", new object[] { spacing });
			}
			this.updateSpacing = false;
			this.HorizontalOrVerticalLayoutGroup.spacing = spacing;
			this.ReloadData(this.NormalizedScrollPosition);
		}

		// Token: 0x06000D3F RID: 3391 RVA: 0x00039968 File Offset: 0x00037B68
		private float AddItemViewSizes()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "AddItemViewSizes", Array.Empty<object>());
			}
			float num = 0f;
			this.loopSetScrollSize = 0f;
			for (int i = 0; i < this.GetCount(); i++)
			{
				float itemSize = base.GetItemSize(i);
				float num2 = ((i == 0) ? 0f : this.HorizontalOrVerticalLayoutGroup.spacing);
				this.itemSizeList.Add(itemSize + num2);
				float num3 = this.loopSetScrollSize;
				List<float> list = this.itemSizeList;
				this.loopSetScrollSize = num3 + list[list.Count - 1];
				float num4 = num;
				List<float> list2 = this.itemSizeList;
				num = num4 + list2[list2.Count - 1];
			}
			return num;
		}

		// Token: 0x06000D40 RID: 3392 RVA: 0x00039A18 File Offset: 0x00037C18
		private void DuplicateItemViewSizes(int numberOfTimes, int itemCount)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DuplicateItemViewSizes", new object[] { numberOfTimes, itemCount });
			}
			for (int i = 0; i < numberOfTimes; i++)
			{
				for (int j = 0; j < itemCount; j++)
				{
					this.itemSizeList.Add(this.itemSizeList[j] + ((j == 0) ? this.HorizontalOrVerticalLayoutGroup.spacing : 0f));
				}
			}
		}

		// Token: 0x06000D41 RID: 3393 RVA: 0x00039A98 File Offset: 0x00037C98
		private void CalculateItemViewOffsets()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CalculateItemViewOffsets", Array.Empty<object>());
			}
			this.itemOffsetList.Clear();
			float num = 0f;
			foreach (float num2 in this.itemSizeList)
			{
				num += num2;
				this.itemOffsetList.Add(num);
			}
		}

		// Token: 0x06000D42 RID: 3394 RVA: 0x00039B20 File Offset: 0x00037D20
		private void ResetVisibleItemViews()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ResetVisibleItemViews", Array.Empty<object>());
			}
			int num;
			int num2;
			this.CalculateVisibleItemRange(out num, out num2);
			int count = this.itemOffsetList.Count;
			num = Mathf.Clamp(num, 0, count - 1);
			num2 = Mathf.Clamp(num2, 0, count - 1);
			if (this.visibleItems.Count == 0 || num > this.LastVisibleViewIndex || num2 < this.FirstVisibleViewIndex)
			{
				this.DespawnAllItems();
				for (int i = num; i <= num2; i++)
				{
					this.AddItemView(i, UIIEnumerableStraightVirtualizedView.Position.Last);
				}
				this.FirstVisibleViewIndex = num;
				this.LastVisibleViewIndex = num2;
				this.UpdatePadding();
				return;
			}
			for (int j = this.visibleItems.Count - 1; j >= 0; j--)
			{
				UIIEnumerableItem uiienumerableItem = this.visibleItems[j];
				if (uiienumerableItem.ViewIndex < num || uiienumerableItem.ViewIndex > num2)
				{
					this.DespawnItem(uiienumerableItem);
				}
			}
			HashSet<int> hashSet = new HashSet<int>();
			for (int k = 0; k < this.visibleItems.Count; k++)
			{
				hashSet.Add(this.visibleItems[k].ViewIndex);
			}
			int firstVisibleViewIndex = this.FirstVisibleViewIndex;
			int lastVisibleViewIndex = this.LastVisibleViewIndex;
			for (int l = firstVisibleViewIndex - 1; l >= num; l--)
			{
				if (!hashSet.Contains(l))
				{
					this.AddItemView(l, UIIEnumerableStraightVirtualizedView.Position.First);
				}
			}
			for (int m = lastVisibleViewIndex + 1; m <= num2; m++)
			{
				if (!hashSet.Contains(m))
				{
					this.AddItemView(m, UIIEnumerableStraightVirtualizedView.Position.Last);
				}
			}
			this.FirstVisibleViewIndex = num;
			this.LastVisibleViewIndex = num2;
			this.UpdatePadding();
		}

		// Token: 0x06000D43 RID: 3395 RVA: 0x00039CB0 File Offset: 0x00037EB0
		private void DespawnAllItems()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DespawnAllItems", Array.Empty<object>());
			}
			while (this.visibleItems.Count > 0)
			{
				if (this.visibleItems[0])
				{
					this.DespawnItem(this.visibleItems[0]);
				}
				else
				{
					DebugUtility.LogWarning("Tried to despawn visibleItems[0] but it was null!", this);
					this.visibleItems.RemoveAt(0);
				}
			}
			this.FirstVisibleViewIndex = 0;
			this.LastVisibleViewIndex = 0;
		}

		// Token: 0x06000D44 RID: 3396 RVA: 0x00039D34 File Offset: 0x00037F34
		private void AddItemView(int itemIndex, UIIEnumerableStraightVirtualizedView.Position position)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "AddItemView", new object[] { itemIndex, position });
			}
			if (this.GetCount() == 0)
			{
				return;
			}
			int num = itemIndex % this.GetCount();
			object obj = this.GetItem(num);
			UIIEnumerableItem uiienumerableItem = this.SpawnItem(obj, num, itemIndex, this.scrollRect.content);
			LayoutElement layoutElement = uiienumerableItem.GetComponent<LayoutElement>();
			if (!layoutElement)
			{
				layoutElement = uiienumerableItem.gameObject.AddComponent<LayoutElement>();
			}
			if (this.scrollDirection == Orientation.Vertical)
			{
				layoutElement.minHeight = this.itemSizeList[itemIndex] - ((itemIndex > 0) ? this.HorizontalOrVerticalLayoutGroup.spacing : 0f);
			}
			else
			{
				layoutElement.minWidth = this.itemSizeList[itemIndex] - ((itemIndex > 0) ? this.HorizontalOrVerticalLayoutGroup.spacing : 0f);
			}
			if (position != UIIEnumerableStraightVirtualizedView.Position.First)
			{
				if (position == UIIEnumerableStraightVirtualizedView.Position.Last)
				{
					this.visibleItems.Add(uiienumerableItem);
					uiienumerableItem.transform.SetSiblingIndex(this.scrollRect.content.childCount - 2);
					return;
				}
			}
			else
			{
				this.visibleItems.Insert(0, uiienumerableItem);
				uiienumerableItem.transform.SetSiblingIndex(1);
			}
		}

		// Token: 0x06000D45 RID: 3397 RVA: 0x00039E6C File Offset: 0x0003806C
		private void UpdatePadding()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdatePadding", Array.Empty<object>());
			}
			if (this.GetCount() == 0)
			{
				return;
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
			float num = this.itemOffsetList[this.FirstVisibleViewIndex] - this.itemSizeList[this.FirstVisibleViewIndex];
			List<float> list = this.itemOffsetList;
			float num2 = list[list.Count - 1] - this.itemOffsetList[this.LastVisibleViewIndex];
			if (this.scrollDirection == Orientation.Vertical)
			{
				this.FirstPadding.minHeight = num;
				this.FirstPadding.gameObject.SetActive(this.FirstPadding.minHeight > 0f);
				this.LastPadding.minHeight = num2;
				this.LastPadding.gameObject.SetActive(this.LastPadding.minHeight > 0f);
				return;
			}
			this.FirstPadding.minWidth = num;
			this.FirstPadding.gameObject.SetActive(this.FirstPadding.minWidth > 0f);
			this.LastPadding.minWidth = num2;
			this.LastPadding.gameObject.SetActive(this.LastPadding.minWidth > 0f);
		}

		// Token: 0x06000D46 RID: 3398 RVA: 0x00039FBC File Offset: 0x000381BC
		private void RefreshActive()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RefreshActive", Array.Empty<object>());
			}
			if (this.loop && !this.ignoreLoopJump)
			{
				if (this.scrollPosition < this.jumpBeforeLoopThreshold)
				{
					Vector2 vector = this.scrollRect.velocity;
					this.ScrollPosition = this.lastLoopedScrollPosition - (this.jumpBeforeLoopThreshold - this.scrollPosition) + this.spacing;
					this.scrollRect.velocity = vector;
				}
				else if (this.scrollPosition > this.jumpAfterLoopThreshold)
				{
					Vector2 vector = this.scrollRect.velocity;
					this.ScrollPosition = this.firstLoopedScrollPosition + (this.scrollPosition - this.jumpAfterLoopThreshold) - this.spacing;
					this.scrollRect.velocity = vector;
				}
			}
			int num;
			int num2;
			this.CalculateVisibleItemRange(out num, out num2);
			if (num == this.FirstVisibleViewIndex && num2 == this.LastVisibleViewIndex)
			{
				return;
			}
			this.ResetVisibleItemViews();
		}

		// Token: 0x06000D47 RID: 3399 RVA: 0x0003A0AC File Offset: 0x000382AC
		private void CalculateVisibleItemRange(out int firstVisibleViewIndex, out int lastVisibleViewIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("CalculateVisibleItemRange", this);
			}
			firstVisibleViewIndex = 0;
			lastVisibleViewIndex = 0;
			float num = this.scrollPosition - this.lookAheadBefore + this.CalculateStartItemBias;
			float num2 = this.scrollPosition + ((this.scrollDirection == Orientation.Vertical) ? this.ScrollRectTransform.rect.height : this.ScrollRectTransform.rect.width) + this.LookAheadAfter;
			firstVisibleViewIndex = this.GetItemViewIndexAtScrollPosition(num);
			lastVisibleViewIndex = this.GetItemViewIndexAtScrollPosition(num2);
		}

		// Token: 0x06000D48 RID: 3400 RVA: 0x0003A13C File Offset: 0x0003833C
		private int GetItemViewIndexAtPosition(float position, int startIndex, int endIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetItemViewIndexAtPosition", new object[] { position, startIndex, endIndex });
			}
			if (startIndex >= endIndex)
			{
				return startIndex;
			}
			int num = (startIndex + endIndex) / 2;
			int num2 = ((this.scrollDirection == Orientation.Vertical) ? this.padding.top : this.padding.left);
			if (this.itemOffsetList[num] + (float)num2 >= position + ((num2 == 0) ? 0f : 1.00001f))
			{
				return this.GetItemViewIndexAtPosition(position, startIndex, num);
			}
			return this.GetItemViewIndexAtPosition(position, num + 1, endIndex);
		}

		// Token: 0x06000D49 RID: 3401 RVA: 0x0003A1E4 File Offset: 0x000383E4
		private void SnapJumpComplete()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SnapJumpComplete", Array.Empty<object>());
			}
			this.snapJumping = false;
			this.scrollRect.inertia = this.snapInertia;
			UIIEnumerableItem uiienumerableItem = null;
			foreach (UIIEnumerableItem uiienumerableItem2 in this.visibleItems)
			{
				if (uiienumerableItem2.DataIndex == this.snapDataIndex)
				{
					uiienumerableItem = uiienumerableItem2;
					break;
				}
			}
			Action<UIIEnumerableItem> snapped = this.Snapped;
			if (snapped == null)
			{
				return;
			}
			snapped(uiienumerableItem);
		}

		// Token: 0x06000D4A RID: 3402 RVA: 0x0003A288 File Offset: 0x00038488
		private IEnumerator TweenPosition(TweenEase tweenEase, float time, float start, float end, Action tweenComplete, bool forceCalculateRange)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "TweenPosition", new object[]
				{
					tweenEase,
					time,
					start,
					end,
					tweenComplete.DebugIsNull(),
					forceCalculateRange
				});
			}
			if (tweenEase != TweenEase.Immediate && time != 0f)
			{
				this.scrollRect.velocity = Vector2.zero;
				this.IsTweening = true;
				Action<bool> scrollerTweeningChanged = this.ScrollerTweeningChanged;
				if (scrollerTweeningChanged != null)
				{
					scrollerTweeningChanged(true);
				}
				this.tweenTimeLeft = 0f;
				while (this.tweenTimeLeft < time && !this.interruptTween)
				{
					if (!this.tweenPaused)
					{
						if (this.tweenPauseToggledOff)
						{
							this.tweenPauseToggledOff = false;
							start = this.ScrollPosition;
							time = ((this.tweenPauseNewTweenTime < 0f) ? this.tweenTimeLeft : this.tweenPauseNewTweenTime);
							this.tweenTimeLeft = 0f;
						}
						float num;
						if (tweenEase == TweenEase.Custom)
						{
							Func<float, float, float, float> customTweenFunction = this.CustomTweenFunction;
							num = ((customTweenFunction != null) ? customTweenFunction(start, end, this.tweenTimeLeft / time) : TweenEaseUtility.Linear(start, end, this.tweenTimeLeft / time));
						}
						else
						{
							num = TweenEaseUtility.Interpolate(tweenEase, start, end, this.tweenTimeLeft / time);
						}
						this.ScrollPosition = num;
						this.tweenTimeLeft += Time.unscaledDeltaTime;
					}
					yield return null;
				}
			}
			if (this.interruptTween)
			{
				this.interruptTween = false;
				this.snapJumping = false;
				this.scrollRect.inertia = this.snapInertia;
			}
			else
			{
				this.ScrollPosition = end;
				if (forceCalculateRange || tweenEase == TweenEase.Immediate || time == 0f)
				{
					this.ReviewVisibleItemsIfModelChanged();
				}
				if (tweenComplete != null)
				{
					tweenComplete();
				}
			}
			this.IsTweening = false;
			Action<bool> scrollerTweeningChanged2 = this.ScrollerTweeningChanged;
			if (scrollerTweeningChanged2 != null)
			{
				scrollerTweeningChanged2(false);
			}
			yield break;
		}

		// Token: 0x06000D4C RID: 3404 RVA: 0x0003A2F4 File Offset: 0x000384F4
		[CompilerGenerated]
		private RectTransform <Initialize>g__SetUpViewport|179_0(ref UIIEnumerableStraightVirtualizedView.<>c__DisplayClass179_0 A_1)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUpViewport", Array.Empty<object>());
			}
			if (this.scrollRect.viewport)
			{
				global::UnityEngine.Object.Destroy(this.scrollRect.viewport.gameObject);
			}
			RectTransform rectTransform;
			new GameObject("Viewport", new Type[]
			{
				typeof(RectTransform),
				typeof(UIRectMask2D)
			})
			{
				layer = LayerMask.NameToLayer("UI")
			}.TryGetComponent<RectTransform>(out rectTransform);
			rectTransform.SetParent(this.ScrollRectTransform);
			rectTransform.SetAsFirstSibling();
			rectTransform.localScale = Vector3.one;
			rectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			this.scrollRect.viewport = rectTransform;
			return rectTransform;
		}

		// Token: 0x06000D4D RID: 3405 RVA: 0x0003A3C8 File Offset: 0x000385C8
		[CompilerGenerated]
		private void <Initialize>g__SetUpContent|179_1(ref UIIEnumerableStraightVirtualizedView.<>c__DisplayClass179_0 A_1)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUpContent", Array.Empty<object>());
			}
			if (this.scrollRect.content)
			{
				global::UnityEngine.Object.Destroy(this.scrollRect.content.gameObject);
			}
			GameObject gameObject = new GameObject("Content", new Type[] { typeof(RectTransform) });
			gameObject.layer = LayerMask.NameToLayer("UI");
			gameObject.transform.SetParent(A_1.viewportRectTransform);
			RectTransform rectTransform;
			gameObject.TryGetComponent<RectTransform>(out rectTransform);
			if (this.scrollDirection == Orientation.Vertical)
			{
				this.HorizontalOrVerticalLayoutGroup = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
			}
			else
			{
				this.HorizontalOrVerticalLayoutGroup = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
			}
			if (this.scrollDirection == Orientation.Vertical)
			{
				rectTransform.anchorMin = new Vector2(0f, 1f);
				rectTransform.anchorMax = Vector2.one;
				rectTransform.pivot = new Vector2(0.5f, 1f);
			}
			else
			{
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = new Vector2(0f, 1f);
				rectTransform.pivot = new Vector2(0f, 0.5f);
			}
			rectTransform.localPosition = Vector3.zero;
			rectTransform.localRotation = Quaternion.identity;
			rectTransform.localScale = Vector3.one;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.offsetMin = Vector2.zero;
			this.scrollRect.content = rectTransform;
		}

		// Token: 0x06000D4E RID: 3406 RVA: 0x0003A540 File Offset: 0x00038740
		[CompilerGenerated]
		private void <Initialize>g__SetUpLayout|179_2(ref UIIEnumerableStraightVirtualizedView.<>c__DisplayClass179_0 A_1)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUpLayout", Array.Empty<object>());
			}
			this.HorizontalOrVerticalLayoutGroup.spacing = this.spacing;
			this.HorizontalOrVerticalLayoutGroup.padding = this.padding;
			this.HorizontalOrVerticalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
			this.HorizontalOrVerticalLayoutGroup.childControlWidth = true;
			this.HorizontalOrVerticalLayoutGroup.childControlHeight = true;
			this.HorizontalOrVerticalLayoutGroup.childScaleWidth = true;
			this.HorizontalOrVerticalLayoutGroup.childScaleHeight = true;
			if (this.scrollDirection == Orientation.Vertical)
			{
				this.HorizontalOrVerticalLayoutGroup.childForceExpandWidth = true;
				this.HorizontalOrVerticalLayoutGroup.childForceExpandHeight = false;
				return;
			}
			this.HorizontalOrVerticalLayoutGroup.childForceExpandWidth = false;
			this.HorizontalOrVerticalLayoutGroup.childForceExpandHeight = true;
		}

		// Token: 0x06000D4F RID: 3407 RVA: 0x0003A600 File Offset: 0x00038800
		[CompilerGenerated]
		private void <Initialize>g__SetUpScrollRect|179_3(ref UIIEnumerableStraightVirtualizedView.<>c__DisplayClass179_0 A_1)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUpScrollRect", Array.Empty<object>());
			}
			this.scrollRect.horizontal = this.scrollDirection == Orientation.Horizontal;
			this.scrollRect.vertical = this.scrollDirection == Orientation.Vertical;
			this.scrollbar = ((this.scrollDirection == Orientation.Vertical) ? this.scrollRect.verticalScrollbar : this.scrollRect.horizontalScrollbar);
		}

		// Token: 0x06000D50 RID: 3408 RVA: 0x0003A674 File Offset: 0x00038874
		[CompilerGenerated]
		private void <Initialize>g__SetUpPadding|179_4(ref UIIEnumerableStraightVirtualizedView.<>c__DisplayClass179_0 A_1)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUpPadding", Array.Empty<object>());
			}
			GameObject gameObject = new GameObject("First Padding", new Type[]
			{
				typeof(RectTransform),
				typeof(LayoutElement)
			})
			{
				layer = LayerMask.NameToLayer("UI")
			};
			gameObject.transform.SetParent(this.scrollRect.content, false);
			this.FirstPadding = gameObject.GetComponent<LayoutElement>();
			gameObject = new GameObject("Last Padding", new Type[]
			{
				typeof(RectTransform),
				typeof(LayoutElement)
			})
			{
				layer = LayerMask.NameToLayer("UI")
			};
			gameObject.transform.SetParent(this.scrollRect.content, false);
			this.LastPadding = gameObject.GetComponent<LayoutElement>();
		}

		// Token: 0x04000856 RID: 2134
		[SerializeField]
		private float spacing;

		// Token: 0x04000857 RID: 2135
		[SerializeField]
		private RectOffset padding;

		// Token: 0x04000858 RID: 2136
		[SerializeField]
		private UILayoutElement dynamicLayoutElement;

		// Token: 0x04000859 RID: 2137
		[SerializeField]
		private float maxVelocity;

		// Token: 0x0400085A RID: 2138
		[SerializeField]
		private Orientation scrollDirection;

		// Token: 0x0400085B RID: 2139
		[SerializeField]
		private UIIEnumerableStraightVirtualizedView.ScrollbarVisibilityEnum scrollbarVisibility;

		// Token: 0x0400085C RID: 2140
		[Header("Loop Settings")]
		[SerializeField]
		private bool loop;

		// Token: 0x0400085D RID: 2141
		[SerializeField]
		private bool loopWhileDragging = true;

		// Token: 0x0400085E RID: 2142
		[Header("Snap Settings")]
		[SerializeField]
		private bool snapping;

		// Token: 0x0400085F RID: 2143
		[SerializeField]
		private float snapVelocityThreshold;

		// Token: 0x04000860 RID: 2144
		[SerializeField]
		private float snapWatchOffset;

		// Token: 0x04000861 RID: 2145
		[SerializeField]
		private float snapJumpToOffset;

		// Token: 0x04000862 RID: 2146
		[SerializeField]
		private float snapItemCenterOffset;

		// Token: 0x04000863 RID: 2147
		[SerializeField]
		private bool snapUseItemSpacing;

		// Token: 0x04000864 RID: 2148
		[SerializeField]
		private TweenEase snapTweenType;

		// Token: 0x04000865 RID: 2149
		[SerializeField]
		private float snapTweenTime;

		// Token: 0x04000866 RID: 2150
		[SerializeField]
		private bool snapWhileDragging;

		// Token: 0x04000867 RID: 2151
		[SerializeField]
		private bool forceSnapOnEndDrag;

		// Token: 0x04000868 RID: 2152
		[SerializeField]
		private bool forceSnapOnPointerUp;

		// Token: 0x04000869 RID: 2153
		[Header("Tween Settings")]
		[SerializeField]
		private bool interruptTweeningOnDrag;

		// Token: 0x0400086A RID: 2154
		[SerializeField]
		private bool interruptTweeningOnPointerDown;

		// Token: 0x0400086B RID: 2155
		private readonly List<float> itemSizeList = new List<float>();

		// Token: 0x0400086C RID: 2156
		private readonly List<float> itemOffsetList = new List<float>();

		// Token: 0x0400086D RID: 2157
		private readonly List<UIIEnumerableItem> visibleItems = new List<UIIEnumerableItem>();

		// Token: 0x0400086E RID: 2158
		private float scrollPosition;

		// Token: 0x0400086F RID: 2159
		private float lookAheadBefore;

		// Token: 0x04000870 RID: 2160
		private float lookAheadAfter;

		// Token: 0x04000871 RID: 2161
		private bool initialized;

		// Token: 0x04000872 RID: 2162
		private bool updateSpacing;

		// Token: 0x04000873 RID: 2163
		private Scrollbar scrollbar;

		// Token: 0x04000874 RID: 2164
		private UIIEnumerableStraightVirtualizedView.ScrollbarVisibilityEnum lastScrollbarVisibility;

		// Token: 0x04000875 RID: 2165
		private bool reloadData;

		// Token: 0x04000876 RID: 2166
		private RectTransform recycledItemViewContainer;

		// Token: 0x04000877 RID: 2167
		private int loopFirstItemViewIndex;

		// Token: 0x04000878 RID: 2168
		private int loopLastItemViewIndex;

		// Token: 0x04000879 RID: 2169
		private float firstLoopedScrollPosition;

		// Token: 0x0400087A RID: 2170
		private float lastLoopedScrollPosition;

		// Token: 0x0400087B RID: 2171
		private float jumpBeforeLoopThreshold;

		// Token: 0x0400087C RID: 2172
		private float jumpAfterLoopThreshold;

		// Token: 0x0400087D RID: 2173
		private float lastScrollRectSize;

		// Token: 0x0400087E RID: 2174
		private bool lastLoop;

		// Token: 0x0400087F RID: 2175
		private bool ignoreLoopJump;

		// Token: 0x04000880 RID: 2176
		private float loopSetScrollSize;

		// Token: 0x04000881 RID: 2177
		private int snapItemViewIndex;

		// Token: 0x04000882 RID: 2178
		private int snapDataIndex;

		// Token: 0x04000883 RID: 2179
		private bool snapJumping;

		// Token: 0x04000884 RID: 2180
		private bool snapInertia;

		// Token: 0x04000885 RID: 2181
		private bool snapBeforeDrag;

		// Token: 0x04000886 RID: 2182
		private bool loopBeforeDrag;

		// Token: 0x04000887 RID: 2183
		private Vector2 dragPreviousPos;

		// Token: 0x04000888 RID: 2184
		private int dragFingerCount;

		// Token: 0x04000889 RID: 2185
		private bool interruptTween;

		// Token: 0x0400088A RID: 2186
		private float tweenTimeLeft;

		// Token: 0x0400088B RID: 2187
		private bool tweenPauseToggledOff;

		// Token: 0x0400088C RID: 2188
		private float tweenPauseNewTweenTime;

		// Token: 0x0400088D RID: 2189
		private bool tweenPaused;

		// Token: 0x020001F9 RID: 505
		public enum ItemPosition
		{
			// Token: 0x04000897 RID: 2199
			Before,
			// Token: 0x04000898 RID: 2200
			After
		}

		// Token: 0x020001FA RID: 506
		public enum ScrollbarVisibilityEnum
		{
			// Token: 0x0400089A RID: 2202
			OnlyIfNeeded,
			// Token: 0x0400089B RID: 2203
			Always,
			// Token: 0x0400089C RID: 2204
			Never
		}

		// Token: 0x020001FB RID: 507
		public enum LoopJumpDirectionEnum
		{
			// Token: 0x0400089E RID: 2206
			Closest,
			// Token: 0x0400089F RID: 2207
			Up,
			// Token: 0x040008A0 RID: 2208
			Down
		}

		// Token: 0x020001FC RID: 508
		private enum Position
		{
			// Token: 0x040008A2 RID: 2210
			First,
			// Token: 0x040008A3 RID: 2211
			Last
		}
	}
}
