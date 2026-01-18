using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000260 RID: 608
	public abstract class UIBaseTab<T> : UIGameObject, IPoolableT
	{
		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x06000F5B RID: 3931 RVA: 0x000422B7 File Offset: 0x000404B7
		// (set) Token: 0x06000F5C RID: 3932 RVA: 0x000422BF File Offset: 0x000404BF
		protected bool VerboseLogging { get; set; }

		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x06000F5D RID: 3933 RVA: 0x000422C8 File Offset: 0x000404C8
		// (set) Token: 0x06000F5E RID: 3934 RVA: 0x000422D0 File Offset: 0x000404D0
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x06000F5F RID: 3935 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170002E6 RID: 742
		// (get) Token: 0x06000F60 RID: 3936 RVA: 0x000422D9 File Offset: 0x000404D9
		protected T Value
		{
			get
			{
				return this.container.GetOption(this.index);
			}
		}

		// Token: 0x06000F61 RID: 3937 RVA: 0x000422EC File Offset: 0x000404EC
		private void Start()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("Start", this);
			}
			this.selectButton.onClick.AddListener(new UnityAction(this.Select));
			this.UpdateBadgeCanvasVisibility();
			this.UpdateBadgeCanvasSortingOrder();
		}

		// Token: 0x06000F62 RID: 3938 RVA: 0x00042329 File Offset: 0x00040529
		private void OnDestroy()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("OnDestroy", this);
			}
			this.selectButton.onClick.RemoveListener(new UnityAction(this.Select));
		}

		// Token: 0x06000F63 RID: 3939 RVA: 0x0004235C File Offset: 0x0004055C
		public void Initialize(UIBaseTabGroup<T> container, int index)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[]
				{
					"Initialize",
					"container",
					container.DebugSafeName(true),
					"index",
					index
				}), this);
			}
			this.container = container;
			this.index = index;
			this.View();
		}

		// Token: 0x06000F64 RID: 3940 RVA: 0x000423C6 File Offset: 0x000405C6
		public virtual void OnSpawn()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("OnSpawn", this);
			}
		}

		// Token: 0x06000F65 RID: 3941 RVA: 0x000423DB File Offset: 0x000405DB
		public virtual void OnDespawn()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("OnDespawn", this);
			}
		}

		// Token: 0x06000F66 RID: 3942 RVA: 0x000423F0 File Offset: 0x000405F0
		public void View()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("View", this);
			}
			T option = this.container.GetOption(this.index);
			this.ViewOption(option);
			bool flag = this.container.IsHidden(option);
			base.gameObject.SetActive(!flag);
			if (this.container.IsSelected(this.index))
			{
				this.selectedTweenCollection.Tween();
				return;
			}
			this.unselectedTweenCollection.Tween();
		}

		// Token: 0x06000F67 RID: 3943
		protected abstract void ViewOption(T option);

		// Token: 0x06000F68 RID: 3944 RVA: 0x0004246F File Offset: 0x0004066F
		protected void Select()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("Select", this);
			}
			this.container.SetValue(this.index, true);
		}

		// Token: 0x06000F69 RID: 3945 RVA: 0x00042496 File Offset: 0x00040696
		public void SetBadge(string text)
		{
			if (this.VerboseLogging)
			{
				Debug.Log("SetBadge ( text: " + text + " )", this);
			}
			this.badgeSet = true;
			this.badge.Display(text);
			this.UpdateBadgeCanvasVisibility();
		}

		// Token: 0x06000F6A RID: 3946 RVA: 0x000424D0 File Offset: 0x000406D0
		private void UpdateBadgeCanvasVisibility()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("UpdateBadgeCanvasVisibility", this);
			}
			this.badgeCanvas.enabled = this.badgeSet && !this.badge.BadgeText.IsNullOrEmptyOrWhiteSpace() && this.badge.BadgeText != "0";
		}

		// Token: 0x06000F6B RID: 3947 RVA: 0x00042530 File Offset: 0x00040730
		private void UpdateBadgeCanvasSortingOrder()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("UpdateBadgeCanvasSortingOrder", this);
			}
			foreach (Canvas canvas in base.GetComponentsInParent<Canvas>())
			{
				if (canvas.overrideSorting)
				{
					this.badgeCanvas.overrideSorting = true;
					this.badgeCanvas.sortingOrder = canvas.sortingOrder + 2;
					return;
				}
			}
		}

		// Token: 0x040009D1 RID: 2513
		[Header("UIBaseTab")]
		[SerializeField]
		private UIButton selectButton;

		// Token: 0x040009D2 RID: 2514
		[SerializeField]
		private UIBadge badge;

		// Token: 0x040009D3 RID: 2515
		[SerializeField]
		private Canvas badgeCanvas;

		// Token: 0x040009D4 RID: 2516
		[SerializeField]
		private TweenCollection selectedTweenCollection;

		// Token: 0x040009D5 RID: 2517
		[SerializeField]
		private TweenCollection unselectedTweenCollection;

		// Token: 0x040009D7 RID: 2519
		private UIBaseTabGroup<T> container;

		// Token: 0x040009D8 RID: 2520
		private int index;

		// Token: 0x040009D9 RID: 2521
		private bool badgeSet;
	}
}
