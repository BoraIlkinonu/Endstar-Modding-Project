using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000176 RID: 374
	public abstract class UIBaseListCellView<T> : UIBaseListItemView<T>, IListCellViewable
	{
		// Token: 0x1700018B RID: 395
		// (get) Token: 0x0600091F RID: 2335 RVA: 0x000274F5 File Offset: 0x000256F5
		// (set) Token: 0x06000920 RID: 2336 RVA: 0x000274FD File Offset: 0x000256FD
		public RectTransform Container { get; private set; }

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x06000921 RID: 2337 RVA: 0x00027506 File Offset: 0x00025706
		public override bool IsRow { get; }

		// Token: 0x06000922 RID: 2338 RVA: 0x00027510 File Offset: 0x00025710
		protected virtual void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			if (!MobileUtility.IsMobile)
			{
				return;
			}
			foreach (Image image in base.gameObject.GetComponentsInChildren<Image>())
			{
				if (image.raycastTarget)
				{
					image.gameObject.AddComponent<UIDragInterceptAndPassToParentScrollRect>();
					return;
				}
			}
		}

		// Token: 0x06000923 RID: 2339 RVA: 0x0002756C File Offset: 0x0002576C
		public override void Validate()
		{
			base.Validate();
			DebugUtility.DebugIsNull("Container", this.Container, this);
			DebugUtility.DebugIsNull("canvasGroup", this.canvasGroup, this);
			DebugUtility.DebugIsNull("selectedVisuals", this.selectedVisuals, this);
			DebugUtility.DebugIsNull("selectedOrderText", this.selectedOrderText, this);
			DebugUtility.DebugIsNull("addButton", this.addButton, this);
		}

		// Token: 0x06000924 RID: 2340 RVA: 0x000275D9 File Offset: 0x000257D9
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.Container.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			if (this.addButtonIsInteractableChangedUnityEventHookedUp)
			{
				this.RemoveAddButtonIsInteractableChangedUnityEventListener();
			}
		}

		// Token: 0x06000925 RID: 2341 RVA: 0x00027610 File Offset: 0x00025810
		public override void View(UIBaseListView<T> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.IsAddButton = base.ListView.Model.DisplayAddButton && base.ListView.Model.AddButtonInserted && base.DataIndex == 0;
			this.addButton.gameObject.SetActive(this.IsAddButton);
			if (this.IsAddButton && !this.addButtonIsInteractableChangedUnityEventHookedUp)
			{
				base.ListView.Model.AddButtonIsInteractableChangedUnityEvent.AddListener(new UnityAction<bool>(this.OnAddButtonIsInteractableChanged));
				this.addButtonIsInteractableChangedUnityEventHookedUp = true;
				this.OnAddButtonIsInteractableChanged(base.ListView.Model.AddButtonIsInteractable);
			}
			bool flag = base.ListModel.IsSelected(dataIndex);
			this.selectedVisuals.SetActive(flag);
			if (flag)
			{
				int num = base.ListModel.SelectedOrder(dataIndex);
				this.ViewSelected(num);
			}
			Behaviour[] array = this.behavioursToDisableIfAddButton;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = !this.IsAddButton;
			}
			if (Mathf.Approximately(this.canvasGroup.alpha, 1f))
			{
				return;
			}
			this.SetAlpha(1f);
		}

		// Token: 0x06000926 RID: 2342 RVA: 0x00027738 File Offset: 0x00025938
		public void ViewSelected(int selectedOrder)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewSelected", "selectedOrder", selectedOrder), this);
			}
			this.selectedOrderText.text = selectedOrder.ToString();
			this.selectedVisuals.SetActive(true);
		}

		// Token: 0x06000927 RID: 2343 RVA: 0x0002778B File Offset: 0x0002598B
		public void HideSelectedVisuals()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("HideSelectedVisuals", this);
			}
			this.selectedVisuals.SetActive(false);
		}

		// Token: 0x06000928 RID: 2344 RVA: 0x000277AC File Offset: 0x000259AC
		public void SetAlpha(float value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetAlpha", "value", value), this);
			}
			this.canvasGroup.alpha = value;
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x000277E4 File Offset: 0x000259E4
		public void TweenContainerTo(RectTransform to, float tweenDuration)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[]
				{
					"TweenContainerTo",
					"to",
					to.DebugSafeName(true),
					"tweenDuration",
					tweenDuration
				}), this);
			}
			if (!this.tweenPosition)
			{
				this.tweenPosition = base.gameObject.AddComponent<TweenPosition>();
			}
			this.tweenPosition.Target = this.Container.gameObject;
			this.tweenPosition.InSeconds = tweenDuration;
			this.tweenPosition.To = to.transform.position;
			this.tweenPosition.Tween();
		}

		// Token: 0x0600092A RID: 2346 RVA: 0x0002789B File Offset: 0x00025A9B
		private void OnAddButtonIsInteractableChanged(bool addButtonIsInteractable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnAddButtonIsInteractableChanged", "addButtonIsInteractable", addButtonIsInteractable), this);
			}
			if (!this.IsAddButton)
			{
				return;
			}
			this.addButton.interactable = addButtonIsInteractable;
		}

		// Token: 0x0600092B RID: 2347 RVA: 0x000278DC File Offset: 0x00025ADC
		private void RemoveAddButtonIsInteractableChangedUnityEventListener()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("RemoveAddButtonIsInteractableChangedUnityEventListener", this);
			}
			if (!this.addButtonIsInteractableChangedUnityEventHookedUp)
			{
				return;
			}
			base.ListView.Model.AddButtonIsInteractableChangedUnityEvent.RemoveListener(new UnityAction<bool>(this.OnAddButtonIsInteractableChanged));
			this.addButtonIsInteractableChangedUnityEventHookedUp = false;
		}

		// Token: 0x040005C4 RID: 1476
		[SerializeField]
		private CanvasGroup canvasGroup;

		// Token: 0x040005C5 RID: 1477
		[SerializeField]
		private GameObject selectedVisuals;

		// Token: 0x040005C6 RID: 1478
		[SerializeField]
		private TextMeshProUGUI selectedOrderText;

		// Token: 0x040005C7 RID: 1479
		[SerializeField]
		private UIButton addButton;

		// Token: 0x040005C8 RID: 1480
		[SerializeField]
		private Behaviour[] behavioursToDisableIfAddButton = Array.Empty<Behaviour>();

		// Token: 0x040005C9 RID: 1481
		protected bool IsAddButton;

		// Token: 0x040005CA RID: 1482
		private TweenPosition tweenPosition;

		// Token: 0x040005CB RID: 1483
		private bool addButtonIsInteractableChangedUnityEventHookedUp;
	}
}
