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
	// Token: 0x02000115 RID: 277
	public abstract class UICarouselView : UIGameObject, IValidatable
	{
		// Token: 0x17000115 RID: 277
		// (get) Token: 0x060006A6 RID: 1702 RVA: 0x0001C490 File Offset: 0x0001A690
		// (set) Token: 0x060006A7 RID: 1703 RVA: 0x0001C498 File Offset: 0x0001A698
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x060006A8 RID: 1704 RVA: 0x0001C4A1 File Offset: 0x0001A6A1
		public UICarouselChildView ActiveCarouselChild
		{
			get
			{
				return this.Children[this.activeIndex];
			}
		}

		// Token: 0x060006A9 RID: 1705 RVA: 0x0001C4B4 File Offset: 0x0001A6B4
		private void Awake()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Awake", this);
			}
			this.childrenContainerTweenAnchoredPosition.OnTweenComplete.AddListener(new UnityAction(this.OnCentered));
			this.Children = this.childrenContainer.GetComponentsInChildren<UICarouselChildView>().ToList<UICarouselChildView>();
			this.Children[this.startingIndex].OnTweenToCenter();
			this.SetActiveIndex(this.startingIndex, false);
			this.HandleButtonInteractable();
		}

		// Token: 0x060006AA RID: 1706 RVA: 0x0001C530 File Offset: 0x0001A730
		private void OnEnable()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnEnable", this);
			}
			UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(this.Recenter));
			UIScreenObserver.OnFullScreenModeChange = (Action)Delegate.Combine(UIScreenObserver.OnFullScreenModeChange, new Action(this.Recenter));
			this.Recenter();
		}

		// Token: 0x060006AB RID: 1707 RVA: 0x0001C598 File Offset: 0x0001A798
		private void OnDisable()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDisable", this);
			}
			UIScreenObserver.OnSizeChange = (Action)Delegate.Remove(UIScreenObserver.OnSizeChange, new Action(this.Recenter));
			UIScreenObserver.OnFullScreenModeChange = (Action)Delegate.Remove(UIScreenObserver.OnFullScreenModeChange, new Action(this.Recenter));
		}

		// Token: 0x060006AC RID: 1708 RVA: 0x0001C5F8 File Offset: 0x0001A7F8
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (base.gameObject.scene.name == null)
			{
				return;
			}
			UICarouselChildView[] componentsInChildren = this.childrenContainer.GetComponentsInChildren<UICarouselChildView>();
			if (componentsInChildren.Length <= 1)
			{
				DebugUtility.LogError("UICarouselView requires at least 2 UICarouselChildViews beneath the childrenContainer!", this);
			}
			if (this.startingIndex >= componentsInChildren.Length)
			{
				DebugUtility.LogError("startingIndex is out of range of how many child UICarouselChildViews there are!", this);
			}
		}

		// Token: 0x060006AD RID: 1709 RVA: 0x0001C668 File Offset: 0x0001A868
		public void Back()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Back", this);
			}
			UICarouselChildView activeCarouselChild = this.ActiveCarouselChild;
			int num = this.activeIndex - 1;
			if (num < 0)
			{
				if (this.loop)
				{
					if (this.orderCanChange)
					{
						this.Rearrange(this.Children.Count - 1, 0);
						this.childrenContainer.anchoredPosition = this.IndexPosition(1);
						num = 0;
					}
					else
					{
						num = this.Children.Count - 1;
					}
				}
				else
				{
					num = 0;
				}
			}
			this.SetActiveIndex(num, this.tweenOnNextAndBack);
			this.OnBack.Invoke();
		}

		// Token: 0x060006AE RID: 1710 RVA: 0x0001C700 File Offset: 0x0001A900
		public void Next()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Next", this);
			}
			int num = this.activeIndex + 1;
			if (num >= this.Children.Count)
			{
				if (this.loop)
				{
					if (this.orderCanChange)
					{
						this.Rearrange(0, this.Children.Count - 1);
						this.childrenContainer.anchoredPosition = this.IndexPosition(this.Children.Count - 2);
						num = this.Children.Count - 1;
					}
					else
					{
						num = 0;
					}
				}
				else
				{
					num = this.Children.Count - 1;
				}
			}
			this.SetActiveIndex(num, this.tweenOnNextAndBack);
			this.OnNext.Invoke();
		}

		// Token: 0x060006AF RID: 1711 RVA: 0x0001C7B3 File Offset: 0x0001A9B3
		public int IndexOf(UICarouselChildView child)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("IndexOf ( child: " + child.name + " )", this);
			}
			return this.Children.IndexOf(child);
		}

		// Token: 0x060006B0 RID: 1712 RVA: 0x0001C7E4 File Offset: 0x0001A9E4
		public void TweenToIndex(int index)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "TweenToIndex", "index", index), this);
			}
			this.SetActiveIndex(index, true);
		}

		// Token: 0x060006B1 RID: 1713
		protected abstract Vector2 IndexPosition(int index);

		// Token: 0x060006B2 RID: 1714 RVA: 0x0001C818 File Offset: 0x0001AA18
		protected void Rearrange(int oldIndex, int newIndex)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "Rearrange", "oldIndex", oldIndex, "newIndex", newIndex }), this);
			}
			UICarouselChildView uicarouselChildView = this.Children[oldIndex];
			this.Children.RemoveAt(oldIndex);
			this.Children.Insert(newIndex, uicarouselChildView);
			uicarouselChildView.RectTransform.SetSiblingIndex(newIndex);
			LayoutRebuilder.ForceRebuildLayoutImmediate(this.childrenContainer);
		}

		// Token: 0x060006B3 RID: 1715 RVA: 0x0001C8AC File Offset: 0x0001AAAC
		private void SetActiveIndex(int newValue, bool tween)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} , {3}: {4} )", new object[] { "SetActiveIndex", "newValue", newValue, "tween", tween }), this);
			}
			this.previousActiveIndex = this.activeIndex;
			this.activeIndex = newValue;
			if (tween)
			{
				this.childrenContainerTweenAnchoredPosition.To = this.IndexPosition(this.activeIndex);
				this.childrenContainerTweenAnchoredPosition.Tween();
				this.Children[this.activeIndex].OnTweenToCenter();
				if (this.activeIndex != this.previousActiveIndex)
				{
					this.Children[this.previousActiveIndex].OnTweenAwayFromCenter();
				}
			}
			else
			{
				this.childrenContainer.anchoredPosition = this.IndexPosition(this.activeIndex);
				this.OnCentered();
			}
			this.HandleButtonInteractable();
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x0001C998 File Offset: 0x0001AB98
		private void OnCentered()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnCentered", this);
			}
			this.Children[this.activeIndex].OnCentered();
			if (this.activeIndex != this.previousActiveIndex)
			{
				this.Children[this.previousActiveIndex].OnLostCenterStatus();
			}
		}

		// Token: 0x060006B5 RID: 1717 RVA: 0x0001C9F4 File Offset: 0x0001ABF4
		private void HandleButtonInteractable()
		{
			if ((!this.backButton && !this.nextButton) || this.loop || !this.handleButtonInteractable)
			{
				return;
			}
			if (this.VerboseLogging)
			{
				DebugUtility.Log("HandleButtonInteractable", this);
			}
			if (this.backButton)
			{
				this.backButton.interactable = this.activeIndex > 0;
			}
			if (this.nextButton)
			{
				this.nextButton.interactable = this.activeIndex < this.Children.Count - 1;
			}
		}

		// Token: 0x060006B6 RID: 1718 RVA: 0x0001CA8D File Offset: 0x0001AC8D
		private void Recenter()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Recenter", this);
			}
			this.childrenContainer.anchoredPosition = this.IndexPosition(this.activeIndex);
			this.contentSizeFitter.RequestLayout();
		}

		// Token: 0x040003DF RID: 991
		public UnityEvent OnNext = new UnityEvent();

		// Token: 0x040003E0 RID: 992
		public UnityEvent OnBack = new UnityEvent();

		// Token: 0x040003E1 RID: 993
		[SerializeField]
		private bool loop = true;

		// Token: 0x040003E2 RID: 994
		[SerializeField]
		private bool orderCanChange = true;

		// Token: 0x040003E3 RID: 995
		[SerializeField]
		private bool tweenOnNextAndBack = true;

		// Token: 0x040003E4 RID: 996
		[SerializeField]
		private int startingIndex;

		// Token: 0x040003E5 RID: 997
		[SerializeField]
		private bool handleButtonInteractable = true;

		// Token: 0x040003E6 RID: 998
		[SerializeField]
		private UICarouselChildView carouselChildSource;

		// Token: 0x040003E7 RID: 999
		[Header("childrenContainer")]
		[SerializeField]
		private RectTransform childrenContainer;

		// Token: 0x040003E8 RID: 1000
		[SerializeField]
		private UIContentSizeFitter contentSizeFitter;

		// Token: 0x040003E9 RID: 1001
		[SerializeField]
		private TweenAnchoredPosition childrenContainerTweenAnchoredPosition;

		// Token: 0x040003EA RID: 1002
		[Header("Buttons")]
		[SerializeField]
		private UIButton backButton;

		// Token: 0x040003EB RID: 1003
		[SerializeField]
		private UIButton nextButton;

		// Token: 0x040003ED RID: 1005
		protected List<UICarouselChildView> Children = new List<UICarouselChildView>();

		// Token: 0x040003EE RID: 1006
		private int activeIndex;

		// Token: 0x040003EF RID: 1007
		private int previousActiveIndex;
	}
}
