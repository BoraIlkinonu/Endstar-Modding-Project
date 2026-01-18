using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200019A RID: 410
	public abstract class UIBaseListView<T> : UIGameObject, IListView, IValidatable
	{
		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x06000A6F RID: 2671 RVA: 0x0002CA4D File Offset: 0x0002AC4D
		public UnityEvent SnappedToStartUnityEvent { get; } = new UnityEvent();

		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x06000A70 RID: 2672 RVA: 0x0002CA55 File Offset: 0x0002AC55
		public UnityEvent SnappedToEndUnityEvent { get; } = new UnityEvent();

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x06000A71 RID: 2673 RVA: 0x0002CA5D File Offset: 0x0002AC5D
		public UnityEvent<Vector2, float> ScrollerScrolledUnityEvent { get; } = new UnityEvent<Vector2, float>();

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x06000A72 RID: 2674 RVA: 0x0002CA65 File Offset: 0x0002AC65
		public UnityEvent<int, int, UIBaseListItemView<T>> SnappedUnityEvent { get; } = new UnityEvent<int, int, UIBaseListItemView<T>>();

		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x06000A73 RID: 2675 RVA: 0x0002CA6D File Offset: 0x0002AC6D
		public UnityEvent<UIBaseListItemView<T>> CellVisibilityChangedUnityEvent { get; } = new UnityEvent<UIBaseListItemView<T>>();

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x06000A74 RID: 2676 RVA: 0x0002CA75 File Offset: 0x0002AC75
		public UnityEvent<UIBaseListItemView<T>> CellSpawnedUnityEvent { get; } = new UnityEvent<UIBaseListItemView<T>>();

		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x06000A75 RID: 2677 RVA: 0x0002CA7D File Offset: 0x0002AC7D
		public UnityEvent<UIBaseListItemView<T>> BeforeCellDespawnUnityEvent { get; } = new UnityEvent<UIBaseListItemView<T>>();

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x06000A76 RID: 2678 RVA: 0x0002CA85 File Offset: 0x0002AC85
		public UnityEvent<UIBaseListItemView<T>> CellDespawnedUnityEvent { get; } = new UnityEvent<UIBaseListItemView<T>>();

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x06000A77 RID: 2679 RVA: 0x0002CA8D File Offset: 0x0002AC8D
		public UnityEvent<ListCellSizeTypes> ListCellSizeTypeChangedUnityEvent { get; } = new UnityEvent<ListCellSizeTypes>();

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x06000A78 RID: 2680 RVA: 0x0002CA95 File Offset: 0x0002AC95
		public UnityEvent CellSourceChangedUnityEvent { get; } = new UnityEvent();

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x06000A79 RID: 2681 RVA: 0x0002CA9D File Offset: 0x0002AC9D
		// (set) Token: 0x06000A7A RID: 2682 RVA: 0x0002CAA5 File Offset: 0x0002ACA5
		public UIBaseListModel<T> Model { get; private set; }

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x06000A7B RID: 2683 RVA: 0x0002CAAE File Offset: 0x0002ACAE
		// (set) Token: 0x06000A7C RID: 2684 RVA: 0x0002CAB6 File Offset: 0x0002ACB6
		public ListCellSizeTypes ListCellSizeType { get; private set; }

		// Token: 0x170001FF RID: 511
		// (get) Token: 0x06000A7D RID: 2685 RVA: 0x0002CABF File Offset: 0x0002ACBF
		// (set) Token: 0x06000A7E RID: 2686 RVA: 0x0002CAC7 File Offset: 0x0002ACC7
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x06000A7F RID: 2687 RVA: 0x0002CAD0 File Offset: 0x0002ACD0
		// (set) Token: 0x06000A80 RID: 2688 RVA: 0x0002CAD8 File Offset: 0x0002ACD8
		protected bool SuperVerboseLogging { get; set; }

		// Token: 0x17000201 RID: 513
		// (get) Token: 0x06000A81 RID: 2689 RVA: 0x0002CAE1 File Offset: 0x0002ACE1
		// (set) Token: 0x06000A82 RID: 2690 RVA: 0x0002CAE9 File Offset: 0x0002ACE9
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

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x06000A83 RID: 2691 RVA: 0x0002CAF7 File Offset: 0x0002ACF7
		// (set) Token: 0x06000A84 RID: 2692 RVA: 0x0002CAFF File Offset: 0x0002ACFF
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

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x06000A85 RID: 2693 RVA: 0x0002CB0D File Offset: 0x0002AD0D
		// (set) Token: 0x06000A86 RID: 2694 RVA: 0x0002CB18 File Offset: 0x0002AD18
		public float ScrollPosition
		{
			get
			{
				return this.scrollPosition;
			}
			set
			{
				if (!this.loop)
				{
					value = Mathf.Clamp(value, 0f, this.ScrollSize);
				}
				if (Mathf.Approximately(this.scrollPosition, value))
				{
					return;
				}
				this.scrollPosition = value;
				float num = ((this.ScrollSize == 0f) ? 0f : (this.scrollPosition / this.ScrollSize));
				UIBaseListView<T>.Directions scrollDirection = this.ScrollDirection;
				if (scrollDirection == UIBaseListView<T>.Directions.Vertical)
				{
					this.scrollRect.verticalNormalizedPosition = 1f - num;
					return;
				}
				if (scrollDirection != UIBaseListView<T>.Directions.Horizontal)
				{
					DebugUtility.LogNoEnumSupportError<UIBaseListView<T>.Directions>(this, "ScrollPosition", this.ScrollDirection, Array.Empty<object>());
					return;
				}
				this.scrollRect.horizontalNormalizedPosition = num;
			}
		}

		// Token: 0x17000204 RID: 516
		// (get) Token: 0x06000A87 RID: 2695 RVA: 0x0002CBBE File Offset: 0x0002ADBE
		// (set) Token: 0x06000A88 RID: 2696 RVA: 0x0002CBC8 File Offset: 0x0002ADC8
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
					this.ScrollPosition = this.loopFirstScrollPosition + num;
					return;
				}
				this.ScrollPosition = num - this.loopFirstScrollPosition;
			}
		}

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x06000A89 RID: 2697 RVA: 0x0002CC19 File Offset: 0x0002AE19
		public bool ActiveCellSourceIsRow
		{
			get
			{
				return this.ActiveCellSource.IsRow;
			}
		}

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x06000A8A RID: 2698 RVA: 0x0002CC26 File Offset: 0x0002AE26
		public int RowItemCount
		{
			get
			{
				if (!this.ActiveCellSourceIsRow)
				{
					return 1;
				}
				return (this.ActiveCellSource as UIBaseListRowView<T>).Cells.Length;
			}
		}

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x06000A8B RID: 2699 RVA: 0x0002CC44 File Offset: 0x0002AE44
		// (set) Token: 0x06000A8C RID: 2700 RVA: 0x0002CC4C File Offset: 0x0002AE4C
		private protected UIBaseListView<T>.CellTypes CellType { protected get; private set; }

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x06000A8D RID: 2701 RVA: 0x0002CC55 File Offset: 0x0002AE55
		public UIBaseListView<T>.Directions ScrollDirection
		{
			get
			{
				if (!this.scrollRect.vertical)
				{
					return UIBaseListView<T>.Directions.Horizontal;
				}
				return UIBaseListView<T>.Directions.Vertical;
			}
		}

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x06000A8E RID: 2702 RVA: 0x0002CC67 File Offset: 0x0002AE67
		public UIScrollRect ScrollRect
		{
			get
			{
				return this.scrollRect;
			}
		}

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x06000A8F RID: 2703 RVA: 0x0002CC70 File Offset: 0x0002AE70
		public float CompleteHeight
		{
			get
			{
				float num = 0f;
				num += (float)this.padding.top;
				num += (float)this.padding.bottom;
				for (int i = 0; i < this.CellCount; i++)
				{
					float cellViewSize = this.GetCellViewSize(i);
					num += cellViewSize;
					num += this.spacing;
				}
				return num;
			}
		}

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x06000A90 RID: 2704 RVA: 0x0002CCC8 File Offset: 0x0002AEC8
		// (set) Token: 0x06000A91 RID: 2705 RVA: 0x0002CCF4 File Offset: 0x0002AEF4
		private float LinearVelocity
		{
			get
			{
				if (this.ScrollDirection != UIBaseListView<T>.Directions.Vertical)
				{
					return this.scrollRect.velocity.x;
				}
				return this.scrollRect.velocity.y;
			}
			set
			{
				UIBaseListView<T>.Directions scrollDirection = this.ScrollDirection;
				if (scrollDirection == UIBaseListView<T>.Directions.Vertical)
				{
					this.scrollRect.velocity = new Vector2(0f, value);
					return;
				}
				if (scrollDirection != UIBaseListView<T>.Directions.Horizontal)
				{
					Debug.LogErrorFormat(this, "_UIBaseListView has no support for a scrollDirection value of '{0}'!", new object[] { this.ScrollDirection });
					return;
				}
				this.scrollRect.velocity = new Vector2(value, 0f);
			}
		}

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x06000A92 RID: 2706 RVA: 0x0002CD5E File Offset: 0x0002AF5E
		// (set) Token: 0x06000A93 RID: 2707 RVA: 0x0002CD66 File Offset: 0x0002AF66
		private int VisibleStartCellViewIndex { get; set; }

		// Token: 0x1700020D RID: 525
		// (get) Token: 0x06000A94 RID: 2708 RVA: 0x0002CD6F File Offset: 0x0002AF6F
		// (set) Token: 0x06000A95 RID: 2709 RVA: 0x0002CD77 File Offset: 0x0002AF77
		private int VisibleEndCellViewIndex { get; set; }

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x06000A96 RID: 2710 RVA: 0x0002CD80 File Offset: 0x0002AF80
		private float ScrollRectSize
		{
			get
			{
				if (this.ScrollDirection != UIBaseListView<T>.Directions.Vertical)
				{
					return this.scrollRectTransform.rect.width;
				}
				return this.scrollRectTransform.rect.height;
			}
		}

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x06000A97 RID: 2711 RVA: 0x0002CDBC File Offset: 0x0002AFBC
		// (set) Token: 0x06000A98 RID: 2712 RVA: 0x0002CDC9 File Offset: 0x0002AFC9
		private Vector2 Velocity
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

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x06000A99 RID: 2713 RVA: 0x0002CDD7 File Offset: 0x0002AFD7
		private int CellCount
		{
			get
			{
				if (this.CellType == UIBaseListView<T>.CellTypes.Single)
				{
					return this.Model.Count;
				}
				return Mathf.CeilToInt((float)this.Model.Count / (float)this.rowSource.Cells.Length);
			}
		}

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x06000A9A RID: 2714 RVA: 0x0002CE10 File Offset: 0x0002B010
		private float ScrollSize
		{
			get
			{
				if (this.ScrollDirection != UIBaseListView<T>.Directions.Vertical)
				{
					return Mathf.Max(this.cellsParent.rect.width - this.scrollRectTransform.rect.width, 0f);
				}
				return Mathf.Max(this.cellsParent.rect.height - this.scrollRectTransform.rect.height, 0f);
			}
		}

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x06000A9B RID: 2715 RVA: 0x0002CE88 File Offset: 0x0002B088
		private float NormalizedScrollPosition
		{
			get
			{
				if (this.ScrollPosition > 0f)
				{
					return this.ScrollPosition / this.ScrollSize;
				}
				return 0f;
			}
		}

		// Token: 0x06000A9C RID: 2716 RVA: 0x0002CEAC File Offset: 0x0002B0AC
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.Model.ModelChangedUnityEvent.AddListener(new UnityAction(this.OnModelChanged));
			this.cellsParent = this.scrollRect.content;
			UIBaseListView<T>.Directions scrollDirection = this.ScrollDirection;
			if (scrollDirection != UIBaseListView<T>.Directions.Vertical)
			{
				if (scrollDirection != UIBaseListView<T>.Directions.Horizontal)
				{
					DebugUtility.LogNoEnumSupportError<UIBaseListView<T>.Directions>(this, this.ScrollDirection);
				}
				else
				{
					this.horizontalOrVerticalLayoutGroup = this.cellsParent.gameObject.AddComponent<HorizontalLayoutGroup>();
					this.cellsParent.SetAnchor(AnchorPresets.VertStretchLeft, 0f, 0f, 0f, 0f);
					this.cellsParent.pivot = new Vector2(0f, 0.5f);
				}
			}
			else
			{
				this.horizontalOrVerticalLayoutGroup = this.cellsParent.gameObject.AddComponent<VerticalLayoutGroup>();
				this.cellsParent.SetAnchor(AnchorPresets.HorStretchTop, 0f, 0f, 0f, 0f);
				this.cellsParent.pivot = new Vector2(0.5f, 1f);
			}
			this.cellsParent.localPosition = Vector3.zero;
			this.cellsParent.localRotation = Quaternion.identity;
			this.cellsParent.localScale = Vector3.one;
			this.cellsParent.offsetMax = Vector2.zero;
			this.cellsParent.offsetMin = Vector2.zero;
			this.scrollRect.viewport.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			this.horizontalOrVerticalLayoutGroup.spacing = this.spacing;
			this.horizontalOrVerticalLayoutGroup.padding = this.padding;
			this.horizontalOrVerticalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
			this.horizontalOrVerticalLayoutGroup.childForceExpandHeight = true;
			this.horizontalOrVerticalLayoutGroup.childForceExpandWidth = true;
			this.scrollRect.horizontal = this.ScrollDirection == UIBaseListView<T>.Directions.Horizontal;
			this.scrollRect.vertical = this.ScrollDirection == UIBaseListView<T>.Directions.Vertical;
			GameObject gameObject = new GameObject("First Padding", new Type[]
			{
				typeof(RectTransform),
				typeof(LayoutElement)
			});
			gameObject.transform.SetParent(this.cellsParent, false);
			gameObject.TryGetComponent<LayoutElement>(out this.firstPadding);
			GameObject gameObject2 = new GameObject("Last Padding", new Type[]
			{
				typeof(RectTransform),
				typeof(LayoutElement)
			});
			gameObject2.transform.SetParent(this.cellsParent, false);
			gameObject2.TryGetComponent<LayoutElement>(out this.lastPadding);
			if (this.loop)
			{
				if (this.scrollRect.verticalScrollbar)
				{
					this.scrollRect.verticalScrollbar.gameObject.SetActive(false);
					this.scrollRect.verticalScrollbar = null;
				}
				if (this.scrollRect.horizontalScrollbar)
				{
					this.scrollRect.horizontalScrollbar.gameObject.SetActive(false);
					this.scrollRect.horizontalScrollbar = null;
				}
			}
			this.scrollRect.onValueChanged.AddListener(new UnityAction<Vector2>(this.OnScroll));
			this.scrollRect.OnScrollStarted += this.OnScrollStarted;
			this.scrollRect.OnScrollEnded += this.OnScrollEnded;
			this.SetListCellSizeType(this.ListCellSizeType);
			this.lastScrollRectSize = this.ScrollRectSize;
			this.lastLoop = this.loop;
			this.reloadData = true;
			this.initialized = true;
			this.OnModelChanged();
		}

		// Token: 0x06000A9D RID: 2717 RVA: 0x0002D224 File Offset: 0x0002B424
		protected virtual void OnEnable()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnEnable", this);
			}
			if (this.setScrollPositionImmediatelyToOnEnable != null)
			{
				if (this.VerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", "setScrollPositionImmediatelyToOnEnable", this.setScrollPositionImmediatelyToOnEnable.Value), this);
				}
				this.SetScrollPositionImmediately(this.setScrollPositionImmediatelyToOnEnable.Value);
			}
			this.HandleEmptyFlairVisibility();
		}

		// Token: 0x06000A9E RID: 2718 RVA: 0x0002D298 File Offset: 0x0002B498
		private void Update()
		{
			if (this.initialized && !Mathf.Approximately(this.spacing, this.horizontalOrVerticalLayoutGroup.spacing))
			{
				this.updateSpacing = true;
			}
			if (this.updateSpacing)
			{
				this.SetSpacing(this.spacing);
				this.reloadData = false;
			}
			if (this.reloadData)
			{
				this.ReloadData(0f);
			}
			if ((!this.loop || Mathf.Approximately(this.lastScrollRectSize, this.ScrollRectSize)) && this.loop == this.lastLoop)
			{
				return;
			}
			this.Resize(true);
			this.lastScrollRectSize = this.ScrollRectSize;
			this.lastLoop = this.loop;
		}

		// Token: 0x06000A9F RID: 2719 RVA: 0x0002D344 File Offset: 0x0002B544
		private void LateUpdate()
		{
			UIBaseListView<T>.Directions directions;
			if (this.deferredScrollPosition != null)
			{
				float value = this.deferredScrollPosition.Value;
				this.deferredScrollPosition = null;
				directions = this.ScrollDirection;
				if (directions != UIBaseListView<T>.Directions.Vertical)
				{
					if (directions != UIBaseListView<T>.Directions.Horizontal)
					{
						DebugUtility.LogNoEnumSupportError<UIBaseListView<T>.Directions>(this, "LateUpdate", this.ScrollDirection, new object[] { value });
					}
					else
					{
						this.scrollRect.horizontalNormalizedPosition = value;
					}
				}
				else
				{
					this.scrollRect.verticalNormalizedPosition = 1f - value;
				}
			}
			if (this.maxScrollVelocity <= 0f)
			{
				return;
			}
			directions = this.ScrollDirection;
			if (directions == UIBaseListView<T>.Directions.Vertical)
			{
				float num = Mathf.Clamp(Mathf.Abs(this.Velocity.y), 0f, this.maxScrollVelocity);
				this.Velocity = new Vector2(this.Velocity.x, num * Mathf.Sign(this.Velocity.y));
				return;
			}
			if (directions != UIBaseListView<T>.Directions.Horizontal)
			{
				DebugUtility.LogNoEnumSupportError<UIBaseListView<T>.Directions>(this, this.ScrollDirection);
				return;
			}
			float num2 = Mathf.Clamp(Mathf.Abs(this.Velocity.x), 0f, this.maxScrollVelocity);
			this.Velocity = new Vector2(num2 * Mathf.Sign(this.Velocity.x), this.Velocity.y);
		}

		// Token: 0x06000AA0 RID: 2720 RVA: 0x0002D48C File Offset: 0x0002B68C
		[ContextMenu("Validate")]
		public virtual void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Validate", this);
			}
			if (this.IgnoreValidation)
			{
				return;
			}
			DebugUtility.DebugIsNull("Model", this.Model, this);
			DebugUtility.DebugIsNull("scrollRect", this.scrollRect, this);
			DebugUtility.DebugIsNull("scrollRectTransform", this.scrollRectTransform, this);
			DebugUtility.DebugIsNull("emptyFlair", this.emptyFlair, this);
		}

		// Token: 0x06000AA1 RID: 2721 RVA: 0x0002D500 File Offset: 0x0002B700
		public IReadOnlyList<UIBaseListItemView<T>> GetCellViews(bool ifRowReturnCellsInstead)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetCellViews", "ifRowReturnCellsInstead", ifRowReturnCellsInstead), this);
			}
			List<UIBaseListItemView<T>> list;
			if (ifRowReturnCellsInstead && this.ActiveCellSourceIsRow)
			{
				list = new List<UIBaseListItemView<T>>();
				for (int i = 0; i < this.cellViews.Count; i++)
				{
					UIBaseListRowView<T> uibaseListRowView = (UIBaseListRowView<T>)this.cellViews[i];
					list.AddRange(uibaseListRowView.ActiveCells);
				}
			}
			else
			{
				list = this.cellViews;
			}
			return list;
		}

		// Token: 0x06000AA2 RID: 2722 RVA: 0x0002D584 File Offset: 0x0002B784
		public IReadOnlyList<UIBaseListItemView<T>> GetVisibleCellViews(bool ifRowReturnCellsInstead, float visibleIfPercentageIsGreaterThan = 0.1f)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "GetVisibleCellViews", "ifRowReturnCellsInstead", ifRowReturnCellsInstead, "visibleIfPercentageIsGreaterThan", visibleIfPercentageIsGreaterThan }), this);
			}
			this.localPool.Clear();
			if (ifRowReturnCellsInstead && this.ActiveCellSourceIsRow)
			{
				for (int i = 0; i < this.cellViews.Count; i++)
				{
					foreach (UIBaseListItemView<T> uibaseListItemView in ((UIBaseListRowView<T>)this.cellViews[i]).ActiveCells)
					{
						if (this.IsCellVisible(uibaseListItemView.RectTransform, visibleIfPercentageIsGreaterThan))
						{
							this.localPool.Add(uibaseListItemView);
						}
					}
				}
			}
			else
			{
				for (int k = 0; k < this.cellViews.Count; k++)
				{
					UIBaseListItemView<T> uibaseListItemView2 = this.cellViews[k];
					if (this.IsCellVisible(uibaseListItemView2.RectTransform, visibleIfPercentageIsGreaterThan))
					{
						this.localPool.Add(uibaseListItemView2);
					}
				}
			}
			return this.localPool;
		}

		// Token: 0x06000AA3 RID: 2723 RVA: 0x0002D698 File Offset: 0x0002B898
		public int VisibleStartCellDataIndex(bool ifRowReturnCellsInstead)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "VisibleStartCellDataIndex", "ifRowReturnCellsInstead", ifRowReturnCellsInstead), this);
			}
			if (this.CellCount == 0)
			{
				return 0;
			}
			IReadOnlyList<UIBaseListItemView<T>> visibleCellViews = this.GetVisibleCellViews(ifRowReturnCellsInstead, 0.1f);
			if (visibleCellViews.Count == 0)
			{
				return 0;
			}
			return visibleCellViews[0].DataIndex;
		}

		// Token: 0x06000AA4 RID: 2724 RVA: 0x0002D6FC File Offset: 0x0002B8FC
		public int VisibleEndCellDataIndex(bool ifRowReturnCellsInstead)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "VisibleEndCellDataIndex", "ifRowReturnCellsInstead", ifRowReturnCellsInstead), this);
			}
			if (this.CellCount == 0)
			{
				return 0;
			}
			IReadOnlyList<UIBaseListItemView<T>> visibleCellViews = this.GetVisibleCellViews(ifRowReturnCellsInstead, 0.1f);
			if (visibleCellViews.Count == 0)
			{
				return 0;
			}
			IReadOnlyList<UIBaseListItemView<T>> readOnlyList = visibleCellViews;
			return readOnlyList[readOnlyList.Count - 1].DataIndex;
		}

		// Token: 0x06000AA5 RID: 2725 RVA: 0x0002D768 File Offset: 0x0002B968
		public UIBaseListItemView<T> SpawnCell()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SpawnCell", this);
			}
			if (!this.ActiveCellSource)
			{
				throw new NullReferenceException("ActiveCellSource");
			}
			if (!this.cellsParent)
			{
				throw new NullReferenceException("cellsParent");
			}
			if (!MonoBehaviourSingleton<PoolManagerT>.Instance)
			{
				throw new NullReferenceException("PoolManagerT.Instance");
			}
			PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIBaseListItemView<T> activeCellSource = this.ActiveCellSource;
			Transform transform = this.cellsParent;
			UIBaseListItemView<T> uibaseListItemView = instance.Spawn<UIBaseListItemView<T>>(activeCellSource, default(Vector3), default(Quaternion), transform);
			Transform transform2 = uibaseListItemView.transform;
			transform2.localPosition = Vector3.zero;
			transform2.localRotation = Quaternion.identity;
			this.CellSpawnedUnityEvent.Invoke(uibaseListItemView);
			return uibaseListItemView;
		}

		// Token: 0x06000AA6 RID: 2726 RVA: 0x0002D824 File Offset: 0x0002BA24
		public virtual float GetCellViewSize(int dataIndex)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetCellViewSize", "dataIndex", dataIndex), this);
			}
			switch (this.ActiveCellSource.SizeType)
			{
			case UIBaseListItemView<T>.SizeTypes.Explicit:
				return this.ActiveCellSource.ExplicitSize;
			case UIBaseListItemView<T>.SizeTypes.ListWidth:
				return base.RectTransform.rect.width;
			case UIBaseListItemView<T>.SizeTypes.ListHeight:
				return base.RectTransform.rect.height;
			default:
				DebugUtility.LogNoEnumSupportError<UIBaseListItemView<T>.SizeTypes>(this, "GetCellViewSize", this.ActiveCellSource.SizeType, new object[] { dataIndex });
				return this.ActiveCellSource.ExplicitSize;
			}
		}

		// Token: 0x06000AA7 RID: 2727 RVA: 0x0002D8DD File Offset: 0x0002BADD
		public void SetScrollRectEnabled(bool enabled)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetScrollRectEnabled", "enabled", enabled), this);
			}
			this.scrollRect.enabled = enabled;
		}

		// Token: 0x06000AA8 RID: 2728 RVA: 0x0002D914 File Offset: 0x0002BB14
		public void ResetAllCellPositions()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ResetAllCellPositions", this);
			}
			foreach (UIBaseListItemView<T> uibaseListItemView in this.cellViews)
			{
				if (this.ActiveCellSourceIsRow)
				{
					(uibaseListItemView as UIBaseListRowView<T>).ResetAllCellPositions();
				}
				else
				{
					(uibaseListItemView as UIBaseListCellView<T>).Container.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
				}
			}
		}

		// Token: 0x06000AA9 RID: 2729 RVA: 0x0002D9B0 File Offset: 0x0002BBB0
		protected void RefreshActive()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("RefreshActive", this);
			}
			if (this.lastRefreshActiveFrame == Time.frameCount)
			{
				return;
			}
			this.lastRefreshActiveFrame = Time.frameCount;
			if (this.loop && !this.ignoreLoopJump)
			{
				if (this.scrollPosition < this.loopFirstJumpTrigger)
				{
					Vector2 vector = this.scrollRect.velocity;
					this.ScrollPosition = this.loopLastScrollPosition - (this.loopFirstJumpTrigger - this.scrollPosition) + this.spacing;
					this.scrollRect.velocity = vector;
				}
				else if (this.scrollPosition > this.loopLastJumpTrigger)
				{
					Vector2 vector = this.scrollRect.velocity;
					this.ScrollPosition = this.loopFirstScrollPosition + (this.scrollPosition - this.loopLastJumpTrigger) - this.spacing;
					this.scrollRect.velocity = vector;
				}
			}
			int num;
			int num2;
			this.CalculateCurrentActiveCellRange(out num, out num2);
			if (num == this.VisibleStartCellViewIndex && num2 == this.VisibleEndCellViewIndex)
			{
				return;
			}
			this.ResetVisibleCellViews();
		}

		// Token: 0x06000AAA RID: 2730 RVA: 0x0002DAB4 File Offset: 0x0002BCB4
		protected void Resize(bool keepPosition)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Resize", "keepPosition", keepPosition), this);
			}
			float num = this.scrollPosition;
			this.cellViewSizes.Clear();
			float num2 = this.AddCellViewSizes();
			if (this.loop)
			{
				int count = this.cellViewSizes.Count;
				if (num2 < this.ScrollRectSize)
				{
					int num3 = Mathf.CeilToInt((float)Mathf.CeilToInt(this.ScrollRectSize / num2) / 2f) * 2;
					this.DuplicateCellViewSizes(num3, count);
					this.loopFirstCellIndex = count * (1 + num3 / 2);
				}
				else
				{
					this.loopFirstCellIndex = count;
				}
				this.loopLastCellIndex = this.loopFirstCellIndex + count - 1;
				this.DuplicateCellViewSizes(2, count);
			}
			this.CalculateCellViewOffsets();
			if (!this.cellsParent)
			{
				this.cellsParent = this.scrollRect.content;
			}
			UIBaseListView<T>.Directions scrollDirection = this.ScrollDirection;
			if (scrollDirection != UIBaseListView<T>.Directions.Vertical)
			{
				if (scrollDirection != UIBaseListView<T>.Directions.Horizontal)
				{
					DebugUtility.LogNoEnumSupportError<UIBaseListView<T>.Directions>(this, "Resize", this.ScrollDirection, new object[] { keepPosition });
				}
				else
				{
					float num4 = ((this.cellViewOffsets.Count == 0) ? 0f : this.cellViewOffsets[this.cellViewOffsets.Count - 1]);
					this.cellsParent.sizeDelta = new Vector2(num4 + (float)this.padding.left + (float)this.padding.right, this.cellsParent.sizeDelta.y);
				}
			}
			else
			{
				float num4 = ((this.cellViewOffsets.Count == 0) ? 0f : this.cellViewOffsets[this.cellViewOffsets.Count - 1]);
				this.cellsParent.sizeDelta = new Vector2(this.cellsParent.sizeDelta.x, num4 + (float)this.padding.top + (float)this.padding.bottom);
			}
			if (this.loop)
			{
				this.loopFirstScrollPosition = this.GetScrollPositionForCellViewIndex(this.loopFirstCellIndex, UIBaseListView<T>.CellPositions.Before) + this.spacing * 0.5f;
				this.loopLastScrollPosition = this.GetScrollPositionForCellViewIndex(this.loopLastCellIndex, UIBaseListView<T>.CellPositions.After) - this.ScrollRectSize + this.spacing * 0.5f;
				this.loopFirstJumpTrigger = this.loopFirstScrollPosition - this.ScrollRectSize;
				this.loopLastJumpTrigger = this.loopLastScrollPosition + this.ScrollRectSize;
			}
			this.ResetVisibleCellViews();
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (keepPosition)
			{
				this.ScrollPosition = num;
				return;
			}
			if (this.loop)
			{
				this.ScrollPosition = this.loopFirstScrollPosition;
				return;
			}
			this.ScrollPosition = 0f;
		}

		// Token: 0x06000AAB RID: 2731 RVA: 0x0002DD60 File Offset: 0x0002BF60
		protected void SetCompactCellSource(UIBaseListItemView<T> newCompactCellSource)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SetCompactCellSource ( newCompactCellSource: " + newCompactCellSource.DebugSafeName(true) + " )", this);
			}
			bool flag = this.ActiveCellSource == this.compactCellSource;
			this.compactCellSource = newCompactCellSource;
			if (flag)
			{
				this.SetCellSource(newCompactCellSource);
			}
		}

		// Token: 0x06000AAC RID: 2732 RVA: 0x0002DDB4 File Offset: 0x0002BFB4
		protected void SetCozyCellSource(UIBaseListItemView<T> newCozyCellSource)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SetCozyCellSource ( newCozyCellSource: " + newCozyCellSource.DebugSafeName(true) + " )", this);
			}
			bool flag = this.ActiveCellSource == this.cozyCellSource;
			this.cozyCellSource = newCozyCellSource;
			if (flag)
			{
				this.SetCellSource(newCozyCellSource);
			}
		}

		// Token: 0x06000AAD RID: 2733 RVA: 0x0002DE08 File Offset: 0x0002C008
		public void ReloadData(float setScrollPositionTo = 0f)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ReloadData", "setScrollPositionTo", setScrollPositionTo), this);
			}
			this.reloadData = false;
			this.DespawnAllCells();
			this.Resize(false);
			this.scrollPosition = Mathf.Clamp(setScrollPositionTo * this.ScrollSize, 0f, this.ScrollSize);
			if (base.gameObject.activeInHierarchy)
			{
				this.deferredScrollPosition = new float?(setScrollPositionTo);
			}
			this.RefreshActive();
		}

		// Token: 0x06000AAE RID: 2734 RVA: 0x0002DE8E File Offset: 0x0002C08E
		public void Clear()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			this.DespawnAllCells();
			this.SetScrollPositionImmediately(0f);
		}

		// Token: 0x06000AAF RID: 2735 RVA: 0x0002DEB4 File Offset: 0x0002C0B4
		public void SetIgnoreLoopJump(bool newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetIgnoreLoopJump", "newValue", newValue), this);
			}
			this.ignoreLoopJump = newValue;
		}

		// Token: 0x06000AB0 RID: 2736 RVA: 0x0002DEE8 File Offset: 0x0002C0E8
		public void SetScrollPositionImmediately(float setScrollPositionTo)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetScrollPositionImmediately", "setScrollPositionTo", setScrollPositionTo), this);
			}
			if (!base.gameObject.activeInHierarchy)
			{
				this.setScrollPositionImmediatelyToOnEnable = new float?(setScrollPositionTo);
				return;
			}
			this.setScrollPositionImmediatelyToOnEnable = null;
			this.ScrollPosition = setScrollPositionTo;
			this.RefreshActive();
		}

		// Token: 0x06000AB1 RID: 2737 RVA: 0x0002DF50 File Offset: 0x0002C150
		public float GetScrollPositionForCellViewIndex(int cellViewIndex, UIBaseListView<T>.CellPositions insertPosition)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "GetScrollPositionForCellViewIndex", "cellViewIndex", cellViewIndex, "insertPosition", insertPosition }), this);
			}
			if (this.CellCount == 0)
			{
				return 0f;
			}
			if (cellViewIndex < 0)
			{
				cellViewIndex = 0;
			}
			if (cellViewIndex == 0 && insertPosition == UIBaseListView<T>.CellPositions.Before)
			{
				return 0f;
			}
			if (cellViewIndex < this.cellViewOffsets.Count)
			{
				if (insertPosition == UIBaseListView<T>.CellPositions.Before)
				{
					return this.cellViewOffsets[cellViewIndex - 1] + this.spacing + (float)((this.ScrollDirection == UIBaseListView<T>.Directions.Vertical) ? this.padding.top : this.padding.left);
				}
				if (insertPosition != UIBaseListView<T>.CellPositions.After)
				{
					DebugUtility.LogNoEnumSupportError<UIBaseListView<T>.Directions>(this, "GetScrollPositionForCellViewIndex", this.ScrollDirection, new object[] { cellViewIndex, insertPosition });
					return 0f;
				}
				return this.cellViewOffsets[cellViewIndex] + (float)((this.ScrollDirection == UIBaseListView<T>.Directions.Vertical) ? this.padding.top : this.padding.left);
			}
			else
			{
				if (this.cellViewOffsets.Count >= 2)
				{
					List<float> list = this.cellViewOffsets;
					return list[list.Count - 2];
				}
				if (this.cellViewOffsets.Count == 1)
				{
					List<float> list2 = this.cellViewOffsets;
					return list2[list2.Count - 1];
				}
				return 0f;
			}
		}

		// Token: 0x06000AB2 RID: 2738 RVA: 0x0002E0C0 File Offset: 0x0002C2C0
		public float GetScrollPositionForDataIndex(int dataIndex, UIBaseListView<T>.CellPositions insertPosition)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "GetScrollPositionForDataIndex", "dataIndex", dataIndex, "insertPosition", insertPosition }), this);
			}
			int num = (this.loop ? (this.Model.Count + dataIndex) : dataIndex);
			return this.GetScrollPositionForCellViewIndex(num, insertPosition);
		}

		// Token: 0x06000AB3 RID: 2739 RVA: 0x0002E136 File Offset: 0x0002C336
		public int GetCellViewIndexAtPosition(float position)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetCellViewIndexAtPosition", "position", position), this);
			}
			return this.GetCellIndexAtPosition(position, 0, this.cellViewOffsets.Count - 1);
		}

		// Token: 0x06000AB4 RID: 2740 RVA: 0x0002E178 File Offset: 0x0002C378
		public UIBaseListItemView<T> GetCellViewAtDataIndex(int dataIndex)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetCellViewAtDataIndex", "dataIndex", dataIndex), this);
			}
			return this.GetCellViews(true).FirstOrDefault((UIBaseListItemView<T> listCellsView) => listCellsView.DataIndex == dataIndex);
		}

		// Token: 0x06000AB5 RID: 2741 RVA: 0x0002E1D8 File Offset: 0x0002C3D8
		public void SetDataToAllVisibleCells()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SetDataToAllVisibleCells", this);
			}
			foreach (UIBaseListItemView<T> uibaseListItemView in this.cellViews)
			{
				if (this.CellType == UIBaseListView<T>.CellTypes.Row)
				{
					UIBaseListRowView<T> uibaseListRowView = (UIBaseListRowView<T>)uibaseListItemView;
					if (this.invokeDespawnOnSetDataToAllVisibleCells)
					{
						uibaseListRowView.OnDespawn();
					}
					this.ApplyCellSize(uibaseListRowView.DataIndex, uibaseListRowView);
					uibaseListRowView.View(this, uibaseListRowView.DataIndex);
				}
				else
				{
					if (this.invokeDespawnOnSetDataToAllVisibleCells)
					{
						uibaseListItemView.OnDespawn();
					}
					this.ApplyCellSize(uibaseListItemView.DataIndex, uibaseListItemView);
					uibaseListItemView.View(this, uibaseListItemView.DataIndex);
				}
			}
		}

		// Token: 0x06000AB6 RID: 2742 RVA: 0x0002E29C File Offset: 0x0002C49C
		public void SetCellSource(UIBaseListItemView<T> newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SetCellSource ( newValue: " + newValue.DebugSafeName(true) + " )", this);
			}
			this.rowSource = null;
			this.ActiveCellSource = newValue;
			this.CellType = (this.ActiveCellSource.IsRow ? UIBaseListView<T>.CellTypes.Row : UIBaseListView<T>.CellTypes.Single);
			if (this.ActiveCellSource.IsRow)
			{
				this.rowSource = this.ActiveCellSource as UIBaseListRowView<T>;
			}
			this.ReloadData(0f);
			this.CellSourceChangedUnityEvent.Invoke();
		}

		// Token: 0x06000AB7 RID: 2743 RVA: 0x0002E328 File Offset: 0x0002C528
		public void SetListCellSizeType(ListCellSizeTypes value)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetListCellSizeType", "value", value), this);
			}
			this.ListCellSizeType = value;
			if (value != ListCellSizeTypes.Compact)
			{
				if (value != ListCellSizeTypes.Cozy)
				{
					DebugUtility.LogException(new Exception(string.Format("{0} ( {1}: {2} ) | No support for this type!", "SetListCellSizeType", "value", value)), null);
				}
				else
				{
					this.SetCellSource(this.cozyCellSource);
				}
			}
			else
			{
				this.SetCellSource(this.compactCellSource);
			}
			this.ListCellSizeTypeChangedUnityEvent.Invoke(this.ListCellSizeType);
		}

		// Token: 0x06000AB8 RID: 2744 RVA: 0x0002E3C0 File Offset: 0x0002C5C0
		public void SetAlphaOfEveryCell(float value)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetAlphaOfEveryCell", "value", value), this);
			}
			foreach (UIBaseListCellView<T> uibaseListCellView in this.GetCellViews(true).Cast<UIBaseListCellView<T>>())
			{
				uibaseListCellView.SetAlpha(1f);
			}
		}

		// Token: 0x06000AB9 RID: 2745 RVA: 0x0002E440 File Offset: 0x0002C640
		private bool IsCellVisible(RectTransform cell, float visibleIfPercentageIsGreaterThan)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[]
				{
					"IsCellVisible",
					"cell",
					cell.DebugSafeName(true),
					"visibleIfPercentageIsGreaterThan",
					visibleIfPercentageIsGreaterThan
				}), this);
			}
			ValueTuple<RectTransform, RectTransform> valueTuple = RectTransformUtility.SmallerAndLarger(base.RectTransform, cell);
			return RectTransformUtility.OverlapPercentage(valueTuple.Item1, valueTuple.Item2) >= visibleIfPercentageIsGreaterThan;
		}

		// Token: 0x06000ABA RID: 2746 RVA: 0x0002E4BC File Offset: 0x0002C6BC
		private UIBaseListItemView<T> GetCellView(int dataIndex)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetCellView", "dataIndex", dataIndex), this);
			}
			UIBaseListItemView<T> uibaseListItemView = this.SpawnCell();
			if (this.CellType == UIBaseListView<T>.CellTypes.Row)
			{
				dataIndex *= this.rowSource.Cells.Length;
			}
			else
			{
				this.ApplyCellSize(dataIndex, uibaseListItemView);
			}
			return uibaseListItemView;
		}

		// Token: 0x06000ABB RID: 2747 RVA: 0x0002E520 File Offset: 0x0002C720
		private void OnGoToDataIndexTween(float interpolation)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnGoToDataIndexTween", "interpolation", interpolation), this);
			}
			this.ScrollPosition = Mathf.LerpUnclamped(this.scrollPositionTweenStart, this.newScrollPosition, interpolation);
			this.RefreshActive();
		}

		// Token: 0x06000ABC RID: 2748 RVA: 0x0002E573 File Offset: 0x0002C773
		private void OnModelChanged()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnModelChanged", this);
			}
			this.Resize(true);
			if (this.CellCount > 0)
			{
				this.SetDataToAllVisibleCells();
			}
			else
			{
				this.Clear();
			}
			this.HandleEmptyFlairVisibility();
		}

		// Token: 0x06000ABD RID: 2749 RVA: 0x0002E5AC File Offset: 0x0002C7AC
		private void SetSpacing(float newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetSpacing", "newValue", newValue), this);
			}
			this.updateSpacing = false;
			this.spacing = newValue;
			this.horizontalOrVerticalLayoutGroup.spacing = this.spacing;
			this.ReloadData(this.NormalizedScrollPosition);
		}

		// Token: 0x06000ABE RID: 2750 RVA: 0x0002E60C File Offset: 0x0002C80C
		private float AddCellViewSizes()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}: {2}", "AddCellViewSizes", "CellCount", this.CellCount), this);
			}
			float num = 0f;
			this.singleLoopGroupSize = 0f;
			for (int i = 0; i < this.CellCount; i++)
			{
				float cellViewSize = this.GetCellViewSize(i);
				float num2 = ((i == 0) ? 0f : this.horizontalOrVerticalLayoutGroup.spacing);
				this.cellViewSizes.Add(cellViewSize + num2);
				float num3 = this.singleLoopGroupSize;
				List<float> list = this.cellViewSizes;
				this.singleLoopGroupSize = num3 + list[list.Count - 1];
				float num4 = num;
				List<float> list2 = this.cellViewSizes;
				num = num4 + list2[list2.Count - 1];
			}
			return num;
		}

		// Token: 0x06000ABF RID: 2751 RVA: 0x0002E6CC File Offset: 0x0002C8CC
		private void DuplicateCellViewSizes(int numberOfTimes, int cellCount)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "DuplicateCellViewSizes", "numberOfTimes", numberOfTimes, "cellCount", cellCount }), this);
			}
			for (int i = 0; i < numberOfTimes; i++)
			{
				for (int j = 0; j < cellCount; j++)
				{
					this.cellViewSizes.Add(this.cellViewSizes[j] + ((j == 0) ? this.horizontalOrVerticalLayoutGroup.spacing : 0f));
				}
			}
		}

		// Token: 0x06000AC0 RID: 2752 RVA: 0x0002E768 File Offset: 0x0002C968
		private void CalculateCellViewOffsets()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}.Count: {2}", "CalculateCellViewOffsets", "cellViewSizes", this.cellViewSizes.Count), this);
			}
			this.cellViewOffsets.Clear();
			float num = 0f;
			foreach (float num2 in this.cellViewSizes)
			{
				num += num2;
				this.cellViewOffsets.Add(num);
			}
		}

		// Token: 0x06000AC1 RID: 2753 RVA: 0x0002E808 File Offset: 0x0002CA08
		private void ResetVisibleCellViews()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}.Count: {2}", "ResetVisibleCellViews", "cellViews", this.cellViews.Count), this);
			}
			int num;
			int num2;
			this.CalculateCurrentActiveCellRange(out num, out num2);
			int i = 0;
			List<int> list = new List<int>();
			while (i < this.cellViews.Count)
			{
				if (this.cellViews[i].ViewIndex < num || this.cellViews[i].ViewIndex > num2)
				{
					this.DespawnCell(this.cellViews[i]);
				}
				else
				{
					list.Add(this.cellViews[i].ViewIndex);
					i++;
				}
			}
			if (list.Count == 0)
			{
				for (i = num; i <= num2; i++)
				{
					this.AddCellView(i, UIBaseListView<T>.ListPositions.Last);
				}
			}
			else
			{
				for (i = num2; i >= num; i--)
				{
					if (i < list[0])
					{
						this.AddCellView(i, UIBaseListView<T>.ListPositions.First);
					}
				}
				for (i = num; i <= num2; i++)
				{
					int num3 = i;
					List<int> list2 = list;
					if (num3 > list2[list2.Count - 1])
					{
						this.AddCellView(i, UIBaseListView<T>.ListPositions.Last);
					}
				}
			}
			this.VisibleStartCellViewIndex = num;
			this.VisibleEndCellViewIndex = num2;
			this.UpdatePadding();
		}

		// Token: 0x06000AC2 RID: 2754 RVA: 0x0002E938 File Offset: 0x0002CB38
		private void DespawnAllCells()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}.Count: {2}", "DespawnAllCells", "cellViews", this.cellViews.Count), this);
			}
			while (this.cellViews.Count > 0)
			{
				this.DespawnCell(this.cellViews[0]);
			}
			this.VisibleStartCellViewIndex = 0;
			this.VisibleEndCellViewIndex = 0;
		}

		// Token: 0x06000AC3 RID: 2755 RVA: 0x0002E9A8 File Offset: 0x0002CBA8
		private void DespawnCell(UIBaseListItemView<T> cellView)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("DespawnCell ( cellView: " + cellView.DebugSafeName(true) + " )", this);
			}
			this.BeforeCellDespawnUnityEvent.Invoke(cellView);
			this.cellViews.Remove(cellView);
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIBaseListItemView<T>>(cellView);
			this.CellVisibilityChangedUnityEvent.Invoke(cellView);
			this.CellDespawnedUnityEvent.Invoke(cellView);
		}

		// Token: 0x06000AC4 RID: 2756 RVA: 0x0002EA18 File Offset: 0x0002CC18
		private void AddCellView(int cellIndex, UIBaseListView<T>.ListPositions listPosition)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "AddCellView", "cellIndex", cellIndex, "listPosition", listPosition }), this);
			}
			if (this.CellCount == 0)
			{
				return;
			}
			int num = cellIndex % this.CellCount;
			UIBaseListItemView<T> cellView = this.GetCellView(num);
			cellView.Initialize(num, cellIndex);
			cellView.View(this, num);
			Transform transform = cellView.transform;
			transform.SetParent(this.cellsParent, false);
			transform.localScale = Vector3.one;
			this.ApplyCellSize(cellIndex, cellView);
			if (listPosition != UIBaseListView<T>.ListPositions.First)
			{
				if (listPosition != UIBaseListView<T>.ListPositions.Last)
				{
					DebugUtility.LogNoEnumSupportError<UIBaseListView<T>.ListPositions>(this, "AddCellView", listPosition, new object[] { cellIndex, listPosition });
				}
				else
				{
					this.cellViews.Add(cellView);
					cellView.transform.SetSiblingIndex(this.cellsParent.childCount - 2);
				}
			}
			else
			{
				this.cellViews.Insert(0, cellView);
				cellView.transform.SetSiblingIndex(1);
			}
			this.CellVisibilityChangedUnityEvent.Invoke(cellView);
		}

		// Token: 0x06000AC5 RID: 2757 RVA: 0x0002EB36 File Offset: 0x0002CD36
		public int IndexOf(UIBaseListItemView<T> item)
		{
			return this.cellViews.IndexOf(item);
		}

		// Token: 0x06000AC6 RID: 2758 RVA: 0x0002EB44 File Offset: 0x0002CD44
		private void ApplyCellSize(int dataIndex, UIBaseListItemView<T> cellView)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[]
				{
					"ApplyCellSize",
					"dataIndex",
					dataIndex,
					"cellView",
					cellView.DebugSafeName(true)
				}), this);
			}
			LayoutElement layoutElement = cellView.LayoutElement;
			UIBaseListView<T>.Directions scrollDirection = this.ScrollDirection;
			if (scrollDirection != UIBaseListView<T>.Directions.Vertical)
			{
				if (scrollDirection != UIBaseListView<T>.Directions.Horizontal)
				{
					DebugUtility.LogNoEnumSupportError<UIBaseListView<T>.Directions>(this, this.ScrollDirection);
				}
				else
				{
					float num = this.cellViewSizes[dataIndex] - ((dataIndex > 0) ? this.horizontalOrVerticalLayoutGroup.spacing : 0f);
					if (!Mathf.Approximately(layoutElement.minWidth, num))
					{
						layoutElement.minWidth = num;
						return;
					}
				}
			}
			else
			{
				float num2 = this.cellViewSizes[dataIndex] - ((dataIndex > 0) ? this.horizontalOrVerticalLayoutGroup.spacing : 0f);
				if (!Mathf.Approximately(layoutElement.minHeight, num2))
				{
					layoutElement.minHeight = num2;
					return;
				}
			}
		}

		// Token: 0x06000AC7 RID: 2759 RVA: 0x0002EC34 File Offset: 0x0002CE34
		private void UpdatePadding()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("UpdatePadding", this);
			}
			if (this.CellCount == 0)
			{
				return;
			}
			float num = this.cellViewOffsets[this.VisibleStartCellViewIndex] - this.cellViewSizes[this.VisibleStartCellViewIndex];
			List<float> list = this.cellViewOffsets;
			float num2 = list[list.Count - 1] - this.cellViewOffsets[this.VisibleEndCellViewIndex];
			UIBaseListView<T>.Directions scrollDirection = this.ScrollDirection;
			if (scrollDirection == UIBaseListView<T>.Directions.Vertical)
			{
				if (this.firstPadding.minHeight != num)
				{
					this.firstPadding.minHeight = num;
				}
				this.firstPadding.gameObject.SetActive(this.firstPadding.minHeight > 0f);
				this.lastPadding.minHeight = num2;
				this.lastPadding.gameObject.SetActive(this.lastPadding.minHeight > 0f);
				return;
			}
			if (scrollDirection != UIBaseListView<T>.Directions.Horizontal)
			{
				DebugUtility.LogNoEnumSupportError<UIBaseListView<T>.Directions>(this, "UpdatePadding", this.ScrollDirection, Array.Empty<object>());
				return;
			}
			this.firstPadding.minWidth = num;
			this.firstPadding.gameObject.SetActive(this.firstPadding.minWidth > 0f);
			this.lastPadding.minWidth = num2;
			this.lastPadding.gameObject.SetActive(this.lastPadding.minWidth > 0f);
		}

		// Token: 0x06000AC8 RID: 2760 RVA: 0x0002ED98 File Offset: 0x0002CF98
		private void CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log("CalculateCurrentActiveCellRange", this);
			}
			startIndex = 0;
			endIndex = 0;
			float num = this.scrollPosition - this.LookAheadBefore;
			float num2 = this.scrollPosition + ((this.ScrollDirection == UIBaseListView<T>.Directions.Vertical) ? this.scrollRectTransform.rect.height : this.scrollRectTransform.rect.width) + this.LookAheadAfter;
			startIndex = this.GetCellViewIndexAtPosition(num);
			endIndex = this.GetCellViewIndexAtPosition(num2);
		}

		// Token: 0x06000AC9 RID: 2761 RVA: 0x0002EE20 File Offset: 0x0002D020
		private int GetCellIndexAtPosition(float position, int startIndex, int endIndex)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "GetCellIndexAtPosition", "position", position, "startIndex", startIndex, "endIndex", endIndex }), this);
			}
			if (startIndex >= endIndex)
			{
				return startIndex;
			}
			int num = (startIndex + endIndex) / 2;
			int num2 = ((this.ScrollDirection == UIBaseListView<T>.Directions.Vertical) ? this.padding.top : this.padding.left);
			if (this.cellViewOffsets[num] + (float)num2 >= position + ((num2 == 0) ? 0f : 1.00001f))
			{
				return this.GetCellIndexAtPosition(position, startIndex, num);
			}
			return this.GetCellIndexAtPosition(position, num + 1, endIndex);
		}

		// Token: 0x06000ACA RID: 2762 RVA: 0x0002EEEC File Offset: 0x0002D0EC
		private void OnScroll(Vector2 scrollRectValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnScroll", "scrollRectValue", scrollRectValue), this);
			}
			if (this.lastScrollFrame == Time.frameCount)
			{
				return;
			}
			this.lastScrollFrame = Time.frameCount;
			UIBaseListView<T>.Directions scrollDirection = this.ScrollDirection;
			if (scrollDirection != UIBaseListView<T>.Directions.Vertical)
			{
				if (scrollDirection != UIBaseListView<T>.Directions.Horizontal)
				{
					DebugUtility.LogNoEnumSupportError<UIBaseListView<T>.Directions>(this, "OnScroll", this.ScrollDirection, new object[] { scrollRectValue });
				}
				else
				{
					this.scrollPosition = scrollRectValue.x * this.ScrollSize;
				}
			}
			else
			{
				this.scrollPosition = (1f - scrollRectValue.y) * this.ScrollSize;
			}
			this.scrollPosition = Mathf.Clamp(this.scrollPosition, 0f, this.ScrollSize);
			this.ScrollerScrolledUnityEvent.Invoke(scrollRectValue, this.scrollPosition);
			if (this.snapping && !this.isSnapJumping && Mathf.Abs(this.LinearVelocity) <= this.snapVelocityThreshold && this.LinearVelocity != 0f)
			{
				float normalizedScrollPosition = this.NormalizedScrollPosition;
				if (this.loop || (!this.loop && normalizedScrollPosition > 0f && normalizedScrollPosition < 1f))
				{
					this.Snap();
				}
			}
			this.RefreshActive();
		}

		// Token: 0x06000ACB RID: 2763 RVA: 0x0002F02C File Offset: 0x0002D22C
		public void GoToStart()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("GoToStart", this);
			}
			this.GoToDataIndex(0, 0f, 0f, true, null, 0.25f, null, UIBaseListView<T>.LoopJumpDirections.Closest, false, false);
		}

		// Token: 0x06000ACC RID: 2764 RVA: 0x0002F068 File Offset: 0x0002D268
		public void GoToDataIndex(int dataIndex, float scrollerOffset = 0f, float cellOffset = 0f, bool useSpacing = true, AnimationCurve ease = null, float inSeconds = 0.25f, Action onDone = null, UIBaseListView<T>.LoopJumpDirections loopJumpDirection = UIBaseListView<T>.LoopJumpDirections.Closest, bool forceCalculateRange = false, bool tween = true)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					string.Format("{0} ( {1}: {2}, ", "GoToDataIndex", "dataIndex", dataIndex),
					string.Format("{0}: {1}, ", "scrollerOffset", scrollerOffset),
					string.Format("{0}: {1}, ", "cellOffset", cellOffset),
					string.Format("{0}: {1}, ", "useSpacing", useSpacing),
					"ease: ",
					(ease == null) ? "null" : "true",
					", ",
					string.Format("{0}: {1}, ", "inSeconds", inSeconds),
					"onDone: ",
					onDone.DebugIsNull(),
					", ",
					string.Format("{0}: {1}, ", "loopJumpDirection", loopJumpDirection),
					string.Format("{0}: {1}, ", "forceCalculateRange", forceCalculateRange),
					string.Format("{0}: {1} )", "tween", tween)
				}), this);
			}
			if (ease == null)
			{
				ease = this.defaultJumpEase;
			}
			if (this.Loop)
			{
				if (dataIndex >= this.Model.Count)
				{
					dataIndex = 0;
				}
				else if (dataIndex < 0)
				{
					dataIndex = this.Model.Count - 1;
				}
			}
			dataIndex = ((this.Model.Count == 0) ? 0 : Mathf.Clamp(dataIndex, 0, this.Model.Count - 1));
			this.snapDataIndex = dataIndex;
			float num = 0f;
			if (cellOffset != 0f)
			{
				float num2 = this.GetCellViewSize(dataIndex);
				if (useSpacing)
				{
					num2 += this.spacing;
					if (dataIndex > 0 && dataIndex < this.CellCount - 1)
					{
						num2 += this.spacing;
					}
				}
				num = num2 * cellOffset;
			}
			if (Mathf.Approximately(scrollerOffset, 1f))
			{
				num += (float)this.padding.bottom;
			}
			float num3 = -(scrollerOffset * this.ScrollRectSize) + num;
			this.newScrollPosition = 0f;
			this.scrollPositionTweenStart = this.scrollPosition;
			if (this.loop)
			{
				int cellCount = this.CellCount;
				int num4 = this.loopFirstCellIndex - (cellCount - dataIndex);
				int num5 = this.loopFirstCellIndex + dataIndex;
				int num6 = this.loopFirstCellIndex + cellCount + dataIndex;
				float num7 = this.GetScrollPositionForCellViewIndex(num4, UIBaseListView<T>.CellPositions.Before) + num3;
				float num8 = this.GetScrollPositionForCellViewIndex(num5, UIBaseListView<T>.CellPositions.Before) + num3;
				float num9 = this.GetScrollPositionForCellViewIndex(num6, UIBaseListView<T>.CellPositions.Before) + num3;
				float num10 = Mathf.Abs(this.scrollPosition - num7);
				float num11 = Mathf.Abs(this.scrollPosition - num8);
				float num12 = Mathf.Abs(this.scrollPosition - num9);
				float num13 = -(scrollerOffset * this.ScrollRectSize);
				int num14 = 0;
				int num15 = 0;
				int num16 = 0;
				if (loopJumpDirection == UIBaseListView<T>.LoopJumpDirections.Up || loopJumpDirection == UIBaseListView<T>.LoopJumpDirections.Down)
				{
					num15 = this.GetCellViewIndexAtPosition(this.scrollPosition - num13 + 0.0001f);
					if (num15 < cellCount)
					{
						num14 = 1;
						num16 = dataIndex;
					}
					else if (num15 >= cellCount && num15 < cellCount * 2)
					{
						num14 = 2;
						num16 = dataIndex + cellCount;
					}
					else
					{
						num14 = 3;
						num16 = dataIndex + cellCount * 2;
					}
				}
				switch (loopJumpDirection)
				{
				case UIBaseListView<T>.LoopJumpDirections.Closest:
					if (num10 < num11)
					{
						this.newScrollPosition = ((num10 < num12) ? num7 : num9);
					}
					else
					{
						this.newScrollPosition = ((num11 < num12) ? num8 : num9);
					}
					break;
				case UIBaseListView<T>.LoopJumpDirections.Up:
					if (num16 < num15)
					{
						this.newScrollPosition = ((num14 == 1) ? num7 : ((num14 == 2) ? num8 : num9));
					}
					else if (num14 == 1 && num15 == dataIndex)
					{
						this.newScrollPosition = num7 - this.singleLoopGroupSize;
					}
					else
					{
						this.newScrollPosition = ((num14 == 1) ? num9 : ((num14 == 2) ? num7 : num8));
					}
					break;
				case UIBaseListView<T>.LoopJumpDirections.Down:
					if (num16 > num15)
					{
						this.newScrollPosition = ((num14 == 1) ? num7 : ((num14 == 2) ? num8 : num9));
					}
					else if (num14 == 3 && num15 == num16)
					{
						this.newScrollPosition = num9 + this.singleLoopGroupSize;
					}
					else
					{
						this.newScrollPosition = ((num14 == 1) ? num8 : ((num14 == 2) ? num9 : num7));
					}
					break;
				}
				if (useSpacing)
				{
					this.newScrollPosition -= this.spacing;
				}
			}
			else
			{
				this.newScrollPosition = this.GetScrollPositionForDataIndex(dataIndex, UIBaseListView<T>.CellPositions.Before) + num3;
				this.newScrollPosition = Mathf.Clamp(this.newScrollPosition - (useSpacing ? this.spacing : 0f), 0f, this.ScrollSize);
			}
			if (Mathf.Approximately(this.newScrollPosition, this.scrollPosition))
			{
				Action onDone2 = onDone;
				if (onDone2 == null)
				{
					return;
				}
				onDone2();
				return;
			}
			else
			{
				ValueTween.CancelAndNull(ref this.goToDataIndexTween);
				if (tween)
				{
					this.goToDataIndexEase = ease;
					this.goToDataIndexTween = MonoBehaviourSingleton<TweenManager>.Instance.TweenValue(inSeconds, new Action<float>(this.OnGoToDataIndexTween), delegate
					{
						this.OnGoToDataIndexComplete();
						Action onDone3 = onDone;
						if (onDone3 == null)
						{
							return;
						}
						onDone3();
					}, TweenTimeMode.Unscaled, ease);
					return;
				}
				this.ScrollPosition = this.newScrollPosition;
				this.RefreshActive();
				this.OnGoToDataIndexComplete();
				return;
			}
		}

		// Token: 0x06000ACD RID: 2765 RVA: 0x0002F56C File Offset: 0x0002D76C
		public void Snap()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Snap", this);
			}
			DebugUtility.Log("Snap", this);
			if (this.CellCount == 0)
			{
				return;
			}
			this.isSnapJumping = true;
			this.LinearVelocity = 0f;
			this.snapInertia = this.scrollRect.inertia;
			this.scrollRect.inertia = false;
			float num = this.ScrollPosition + this.ScrollRectSize * Mathf.Clamp01(this.snapWatchOffset);
			this.snapCellViewIndex = this.GetCellViewIndexAtPosition(num);
			this.snapDataIndex = this.snapCellViewIndex % this.CellCount;
			this.GoToDataIndex(this.snapDataIndex, this.snapJumpToOffset, this.snapCellCenterOffset, this.includeSpacingDuringSnap, this.defaultJumpEase, this.snapTweenTime, null, UIBaseListView<T>.LoopJumpDirections.Closest, false, true);
		}

		// Token: 0x06000ACE RID: 2766 RVA: 0x0002F638 File Offset: 0x0002D838
		public void SnapToDelta(int snapDataIndexDelta = 1)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SnapToDelta", "snapDataIndexDelta", snapDataIndexDelta), this);
			}
			this.snapDataIndex += snapDataIndexDelta;
			if (this.snapDataIndex < 0)
			{
				this.snapDataIndex = 0;
			}
			else if (this.snapDataIndex > this.CellCount - 1)
			{
				this.snapDataIndex = this.CellCount - 1;
			}
			this.GoToDataIndex(this.snapDataIndex, 0f, 0f, true, null, 0.25f, null, UIBaseListView<T>.LoopJumpDirections.Closest, false, true);
		}

		// Token: 0x06000ACF RID: 2767 RVA: 0x0002F6CC File Offset: 0x0002D8CC
		private void OnGoToDataIndexComplete()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnGoToDataIndexComplete", this);
			}
			this.isSnapJumping = false;
			this.scrollRect.inertia = this.snapInertia;
			UIBaseListItemView<T> uibaseListItemView = null;
			foreach (UIBaseListItemView<T> uibaseListItemView2 in this.cellViews)
			{
				if (uibaseListItemView2.DataIndex == this.snapDataIndex)
				{
					uibaseListItemView = uibaseListItemView2;
					break;
				}
			}
			this.SnappedUnityEvent.Invoke(this.snapCellViewIndex, this.snapDataIndex, uibaseListItemView);
			bool flag = this.VisibleStartCellDataIndex(true) != 0;
			int num = this.VisibleEndCellDataIndex(true);
			if (!flag)
			{
				this.SnappedToStartUnityEvent.Invoke();
				return;
			}
			if (num == this.Model.Count - 1)
			{
				this.SnappedToEndUnityEvent.Invoke();
			}
		}

		// Token: 0x06000AD0 RID: 2768 RVA: 0x0002F7A8 File Offset: 0x0002D9A8
		private void HandleEmptyFlairVisibility()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("HandleEmptyFlairVisibility", this);
			}
			this.emptyFlair.SetActive(this.Model.DataCount == 0);
		}

		// Token: 0x06000AD1 RID: 2769 RVA: 0x0002F7D8 File Offset: 0x0002D9D8
		private void OnScrollStarted()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnScrollStarted", this);
			}
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
		}

		// Token: 0x06000AD2 RID: 2770 RVA: 0x0002F848 File Offset: 0x0002DA48
		private void OnScrollEnded()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnScrollEnded", this);
			}
			this.dragFingerCount--;
			if (this.dragFingerCount < 0)
			{
				this.dragFingerCount = 0;
			}
			this.snapping = this.snapBeforeDrag;
			this.loop = this.loopBeforeDrag;
			if (this.snapping)
			{
				this.Snap();
			}
		}

		// Token: 0x04000698 RID: 1688
		protected UIBaseListItemView<T> ActiveCellSource;

		// Token: 0x04000699 RID: 1689
		[SerializeField]
		private UIBaseListItemView<T> compactCellSource;

		// Token: 0x0400069A RID: 1690
		[SerializeField]
		private UIBaseListItemView<T> cozyCellSource;

		// Token: 0x0400069C RID: 1692
		[Header("Layout")]
		[SerializeField]
		private float spacing = 5f;

		// Token: 0x0400069D RID: 1693
		[SerializeField]
		private RectOffset padding = new RectOffset();

		// Token: 0x0400069E RID: 1694
		[Header("Look Ahead")]
		private float lookAheadBefore;

		// Token: 0x0400069F RID: 1695
		private float lookAheadAfter;

		// Token: 0x040006A0 RID: 1696
		[Header("Moving")]
		[SerializeField]
		private float maxScrollVelocity;

		// Token: 0x040006A1 RID: 1697
		[SerializeField]
		private AnimationCurve defaultJumpEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		// Token: 0x040006A2 RID: 1698
		[Header("Looping")]
		[SerializeField]
		private bool loop;

		// Token: 0x040006A3 RID: 1699
		[SerializeField]
		private bool loopWhileDragging = true;

		// Token: 0x040006A4 RID: 1700
		[Header("Snapping")]
		[SerializeField]
		private bool snapping;

		// Token: 0x040006A5 RID: 1701
		[SerializeField]
		private float snapVelocityThreshold;

		// Token: 0x040006A6 RID: 1702
		[Tooltip("Adjusts what cell is chose to snap to. 0 = top left; 1 = bottom right")]
		[SerializeField]
		[Range(0f, 1f)]
		private float snapWatchOffset;

		// Token: 0x040006A7 RID: 1703
		[Tooltip("Adjusts what cell is chose to snap to <b>after a jump</b>. 0 = top left; 1 = bottom right")]
		[SerializeField]
		[Range(0f, 1f)]
		private float snapJumpToOffset;

		// Token: 0x040006A8 RID: 1704
		[Tooltip("Adjusts how the cell is centered. 0 = top left; 1 = bottom right")]
		[SerializeField]
		[Range(0f, 1f)]
		private float snapCellCenterOffset;

		// Token: 0x040006A9 RID: 1705
		[SerializeField]
		private bool includeSpacingDuringSnap;

		// Token: 0x040006AA RID: 1706
		[SerializeField]
		private float snapTweenTime;

		// Token: 0x040006AB RID: 1707
		[SerializeField]
		private bool snapWhileDragging;

		// Token: 0x040006AC RID: 1708
		[Header("Misc")]
		[SerializeField]
		private UIScrollRect scrollRect;

		// Token: 0x040006AD RID: 1709
		[SerializeField]
		private RectTransform scrollRectTransform;

		// Token: 0x040006AE RID: 1710
		[SerializeField]
		private GameObject emptyFlair;

		// Token: 0x040006AF RID: 1711
		[SerializeField]
		private bool invokeDespawnOnSetDataToAllVisibleCells = true;

		// Token: 0x040006B2 RID: 1714
		[Tooltip("Use this only if you know what you are doing :P")]
		[SerializeField]
		protected bool IgnoreValidation;

		// Token: 0x040006B3 RID: 1715
		private readonly List<float> cellViewOffsets = new List<float>();

		// Token: 0x040006B4 RID: 1716
		private readonly List<UIBaseListItemView<T>> localPool = new List<UIBaseListItemView<T>>();

		// Token: 0x040006B5 RID: 1717
		private List<UIBaseListItemView<T>> cellViews = new List<UIBaseListItemView<T>>();

		// Token: 0x040006B6 RID: 1718
		private List<float> cellViewSizes = new List<float>();

		// Token: 0x040006B7 RID: 1719
		private RectTransform cellsParent;

		// Token: 0x040006B8 RID: 1720
		private UIBaseListRowView<T> rowSource;

		// Token: 0x040006B9 RID: 1721
		private int dragFingerCount;

		// Token: 0x040006BA RID: 1722
		private LayoutElement firstPadding;

		// Token: 0x040006BB RID: 1723
		private LayoutElement lastPadding;

		// Token: 0x040006BC RID: 1724
		private HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup;

		// Token: 0x040006BD RID: 1725
		private bool ignoreLoopJump;

		// Token: 0x040006BE RID: 1726
		private bool initialized;

		// Token: 0x040006BF RID: 1727
		private bool isSnapJumping;

		// Token: 0x040006C0 RID: 1728
		private bool lastLoop;

		// Token: 0x040006C1 RID: 1729
		private float lastScrollRectSize;

		// Token: 0x040006C2 RID: 1730
		private bool loopBeforeDrag;

		// Token: 0x040006C3 RID: 1731
		private int loopFirstCellIndex;

		// Token: 0x040006C4 RID: 1732
		private float loopFirstJumpTrigger;

		// Token: 0x040006C5 RID: 1733
		private float loopFirstScrollPosition;

		// Token: 0x040006C6 RID: 1734
		private int loopLastCellIndex;

		// Token: 0x040006C7 RID: 1735
		private float loopLastJumpTrigger;

		// Token: 0x040006C8 RID: 1736
		private float loopLastScrollPosition;

		// Token: 0x040006C9 RID: 1737
		private float newScrollPosition;

		// Token: 0x040006CA RID: 1738
		private bool reloadData;

		// Token: 0x040006CB RID: 1739
		private float scrollPosition;

		// Token: 0x040006CC RID: 1740
		private float scrollPositionTweenStart;

		// Token: 0x040006CD RID: 1741
		private float? setScrollPositionImmediatelyToOnEnable;

		// Token: 0x040006CE RID: 1742
		private float? deferredScrollPosition;

		// Token: 0x040006CF RID: 1743
		private float singleLoopGroupSize;

		// Token: 0x040006D0 RID: 1744
		private bool snapBeforeDrag;

		// Token: 0x040006D1 RID: 1745
		private int snapCellViewIndex;

		// Token: 0x040006D2 RID: 1746
		private int snapDataIndex;

		// Token: 0x040006D3 RID: 1747
		private bool snapInertia;

		// Token: 0x040006D4 RID: 1748
		private bool updateSpacing;

		// Token: 0x040006D5 RID: 1749
		private ValueTween goToDataIndexTween;

		// Token: 0x040006D6 RID: 1750
		private AnimationCurve goToDataIndexEase;

		// Token: 0x040006D7 RID: 1751
		private int lastRefreshActiveFrame = -1;

		// Token: 0x040006DB RID: 1755
		private int lastScrollFrame = -1;

		// Token: 0x0200019B RID: 411
		public enum CellPositions
		{
			// Token: 0x040006DD RID: 1757
			Before,
			// Token: 0x040006DE RID: 1758
			After
		}

		// Token: 0x0200019C RID: 412
		public enum LoopJumpDirections
		{
			// Token: 0x040006E0 RID: 1760
			Closest,
			// Token: 0x040006E1 RID: 1761
			Up,
			// Token: 0x040006E2 RID: 1762
			Down
		}

		// Token: 0x0200019D RID: 413
		public enum Directions
		{
			// Token: 0x040006E4 RID: 1764
			Vertical,
			// Token: 0x040006E5 RID: 1765
			Horizontal
		}

		// Token: 0x0200019E RID: 414
		protected enum CellTypes
		{
			// Token: 0x040006E7 RID: 1767
			Single,
			// Token: 0x040006E8 RID: 1768
			Row
		}

		// Token: 0x0200019F RID: 415
		private enum ListPositions
		{
			// Token: 0x040006EA RID: 1770
			First,
			// Token: 0x040006EB RID: 1771
			Last
		}
	}
}
