using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002AA RID: 682
	public abstract class UIItemSelectionToolPanelView<T, TItemType> : UIBaseToolPanelView<T> where T : EndlessTool
	{
		// Token: 0x17000172 RID: 370
		// (get) Token: 0x06000B6C RID: 2924 RVA: 0x00035DD5 File Offset: 0x00033FD5
		// (set) Token: 0x06000B6D RID: 2925 RVA: 0x00035DDD File Offset: 0x00033FDD
		private protected UIBaseListView<TItemType> ListView { protected get; private set; }

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x06000B6E RID: 2926
		protected abstract bool HasSelectedItem { get; }

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x06000B6F RID: 2927
		protected abstract bool CanViewDetail { get; }

		// Token: 0x06000B70 RID: 2928 RVA: 0x00035DE8 File Offset: 0x00033FE8
		protected override void Start()
		{
			base.Start();
			this.detailDisplayAndHideHandler.SetToHideEnd(true);
			this.detailDisplayAndHideHandler.OnHideComplete.AddListener(new UnityAction(this.detailView.Interface.Clear));
			this.IsMobile = MobileUtility.IsMobile;
			if (this.IsMobile)
			{
				this.detailController.Interface.OnHide.AddListener(new UnityAction(this.Dock));
			}
		}

		// Token: 0x06000B71 RID: 2929 RVA: 0x00035E62 File Offset: 0x00034062
		public override void Display()
		{
			base.Display();
			if (this.HasSelectedItem && this.IsMobile)
			{
				this.Dock();
			}
		}

		// Token: 0x06000B72 RID: 2930 RVA: 0x00035E80 File Offset: 0x00034080
		public override void Hide()
		{
			base.Hide();
			this.floatingSelectedItemContainerHideTweenCollection.Tween();
		}

		// Token: 0x06000B73 RID: 2931 RVA: 0x00035E94 File Offset: 0x00034094
		public void ViewSelectedItem(TItemType itemType)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewSelectedItem", "itemType", itemType), this);
			}
			if (this.CanViewDetail)
			{
				this.detailView.Interface.View(itemType);
				this.detailDisplayAndHideHandler.Display();
			}
			if (this.IsMobile)
			{
				this.floatingSelectedItemView.Interface.View(itemType);
				this.Dock();
			}
		}

		// Token: 0x06000B74 RID: 2932 RVA: 0x00035F0C File Offset: 0x0003410C
		public void Dock()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Dock", this);
			}
			this.dockTweenCollection.Tween();
			if (this.IsMobile && (!MonoBehaviourSingleton<UIWindowManager>.Instance.Displayed || !(MonoBehaviourSingleton<UIWindowManager>.Instance.Displayed is UIScriptWindowView)))
			{
				this.floatingSelectedItemContainerDisplayTweenCollection.Tween();
			}
		}

		// Token: 0x06000B75 RID: 2933 RVA: 0x00035F6C File Offset: 0x0003416C
		public void Undock()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Undock", this);
			}
			this.undockTweenCollection.Tween();
			if (this.IsMobile)
			{
				this.floatingSelectedItemContainerHideTweenCollection.Tween();
			}
			this.ListView.SetDataToAllVisibleCells();
		}

		// Token: 0x06000B76 RID: 2934 RVA: 0x00035FAA File Offset: 0x000341AA
		protected void OnItemSelectionEmpty()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnItemSelectionEmpty", this);
			}
			if (this.IsMobile)
			{
				this.Undock();
			}
			this.detailDisplayAndHideHandler.Hide();
		}

		// Token: 0x040009AC RID: 2476
		[Header("Detail")]
		[SerializeField]
		private UIDisplayAndHideHandler detailDisplayAndHideHandler;

		// Token: 0x040009AD RID: 2477
		[SerializeField]
		private InterfaceReference<IUIViewable<TItemType>> detailView;

		// Token: 0x040009AE RID: 2478
		[SerializeField]
		private InterfaceReference<IUIDetailControllable> detailController;

		// Token: 0x040009AF RID: 2479
		[Header("Panel Docking")]
		[SerializeField]
		private TweenCollection dockTweenCollection;

		// Token: 0x040009B0 RID: 2480
		[SerializeField]
		private TweenCollection undockTweenCollection;

		// Token: 0x040009B1 RID: 2481
		[Header("Floating Selected Item")]
		[SerializeField]
		private TweenCollection floatingSelectedItemContainerDisplayTweenCollection;

		// Token: 0x040009B2 RID: 2482
		[SerializeField]
		private TweenCollection floatingSelectedItemContainerHideTweenCollection;

		// Token: 0x040009B3 RID: 2483
		[SerializeField]
		private InterfaceReference<IUIViewable<TItemType>> floatingSelectedItemView;

		// Token: 0x040009B4 RID: 2484
		protected bool IsMobile;
	}
}
