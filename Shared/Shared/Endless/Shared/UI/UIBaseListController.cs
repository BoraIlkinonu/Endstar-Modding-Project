using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000184 RID: 388
	public abstract class UIBaseListController<T> : UIGameObject, IValidatable
	{
		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000998 RID: 2456 RVA: 0x00028FAE File Offset: 0x000271AE
		// (set) Token: 0x06000999 RID: 2457 RVA: 0x00028FB6 File Offset: 0x000271B6
		protected bool VerboseLogging { get; set; }

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x0600099A RID: 2458 RVA: 0x00028FBF File Offset: 0x000271BF
		// (set) Token: 0x0600099B RID: 2459 RVA: 0x00028FC7 File Offset: 0x000271C7
		protected bool SuperVerboseLogging { get; set; }

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x0600099C RID: 2460 RVA: 0x00028FD0 File Offset: 0x000271D0
		// (set) Token: 0x0600099D RID: 2461 RVA: 0x00028FD8 File Offset: 0x000271D8
		private protected UIBaseListModel<T> Model { protected get; private set; }

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x0600099E RID: 2462 RVA: 0x00028FE1 File Offset: 0x000271E1
		// (set) Token: 0x0600099F RID: 2463 RVA: 0x00028FE9 File Offset: 0x000271E9
		private protected UIBaseListView<T> View { protected get; private set; }

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x060009A0 RID: 2464 RVA: 0x00028FF2 File Offset: 0x000271F2
		protected bool CanNavigate
		{
			get
			{
				return this.canNavigate;
			}
		}

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x060009A1 RID: 2465 RVA: 0x00028FFA File Offset: 0x000271FA
		protected UIListCellSwitch ListCellSwitch
		{
			get
			{
				return this.listCellSwitch;
			}
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x00029004 File Offset: 0x00027204
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.View.ListCellSizeTypeChangedUnityEvent.AddListener(new UnityAction<ListCellSizeTypes>(this.OnListCellSizeTypeChanged));
			this.View.CellSourceChangedUnityEvent.AddListener(new UnityAction(this.ViewControls));
			this.Model.ModelChangedUnityEvent.AddListener(new UnityAction(this.ViewControls));
			this.synchronizeButton.onClick.AddListener(new UnityAction(this.Synchronize));
			this.previousButton.onClick.AddListener(new UnityAction(this.SnapToPrevious));
			this.nextButton.onClick.AddListener(new UnityAction(this.SnapToNext));
			this.ViewControls();
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x000290D8 File Offset: 0x000272D8
		[ContextMenu("Validate")]
		public virtual void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.IgnoreValidation)
			{
				return;
			}
			if (!this.Model)
			{
				DebugUtility.LogError(string.Format("{0} needs a {1}!", base.GetType(), "Model"), this);
			}
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x0002912E File Offset: 0x0002732E
		public void SetCanSort(bool newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetCanSort", "newValue", newValue), this);
			}
			this.canSort = newValue;
			this.ViewControls();
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x00029165 File Offset: 0x00027365
		public void SetCanSwitchCells(bool newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetCanSwitchCells", "newValue", newValue), this);
			}
			this.canSwitchCells = newValue;
			this.ViewControls();
		}

		// Token: 0x060009A6 RID: 2470 RVA: 0x0002919C File Offset: 0x0002739C
		protected virtual void ViewControls()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ViewControls", this);
			}
			this.synchronizeButton.gameObject.SetActive(this.canSynchronize);
			this.listSortSwitch.gameObject.SetActive(this.canSort);
			bool flag = this.canSwitchCells;
			this.listCellSwitch.gameObject.SetActive(flag);
			this.ViewNavigationControls();
			if (!this.canNavigate || !this.makeSpaceForNavigationButtons)
			{
				return;
			}
			UIBaseListView<T>.Directions scrollDirection = this.View.ScrollDirection;
			if (scrollDirection == UIBaseListView<T>.Directions.Vertical)
			{
				this.View.RectTransform.SetVerticalPadding(this.previousButton.RectTransform.rect.height, this.nextButton.RectTransform.rect.height);
				return;
			}
			if (scrollDirection != UIBaseListView<T>.Directions.Horizontal)
			{
				DebugUtility.LogNoEnumSupportError<UIBaseListView<T>.Directions>(this, this.View.ScrollDirection);
				return;
			}
			this.View.RectTransform.SetHorizontalPadding(this.previousButton.RectTransform.rect.height, this.nextButton.RectTransform.rect.height);
		}

		// Token: 0x060009A7 RID: 2471 RVA: 0x000292C4 File Offset: 0x000274C4
		protected virtual void ViewNavigationControls()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ViewNavigationControls", this);
			}
			int num = this.View.VisibleEndCellDataIndex(true);
			int num2 = this.View.VisibleStartCellDataIndex(true);
			bool flag = this.CanNavigate && (num2 > 0 || this.View.Loop) && this.Model.Count > 1;
			bool flag2 = this.CanNavigate && (num < this.Model.Count || this.View.Loop) && this.Model.Count > 1;
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}", new object[] { "visibleStartCellDataIndex", num2, "visibleEndCellDataIndex", num }), this);
			}
			this.SetPreviousButtonVisibility(flag);
			this.SetNextButtonVisibility(flag2);
		}

		// Token: 0x060009A8 RID: 2472 RVA: 0x000293AC File Offset: 0x000275AC
		protected void SetPreviousButtonVisibility(bool visible)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}, ( {1}: {2} )", "SetPreviousButtonVisibility", "visible", visible), this);
			}
			this.previousButton.gameObject.SetActive(visible);
		}

		// Token: 0x060009A9 RID: 2473 RVA: 0x000293E7 File Offset: 0x000275E7
		protected void SetNextButtonVisibility(bool visible)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}, ( {1}: {2} )", "SetNextButtonVisibility", "visible", visible), this);
			}
			this.nextButton.gameObject.SetActive(visible);
		}

		// Token: 0x060009AA RID: 2474 RVA: 0x00029422 File Offset: 0x00027622
		protected virtual void Synchronize()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Synchronize", this);
			}
		}

		// Token: 0x060009AB RID: 2475 RVA: 0x00029438 File Offset: 0x00027638
		protected virtual void SnapToNext()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SnapToNext", this);
			}
			int num = this.View.VisibleEndCellDataIndex(false);
			int num2 = this.View.VisibleStartCellDataIndex(false);
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}", new object[] { "visibleStartCellDataIndex", num2, "visibleEndCellDataIndex", num }), this);
			}
			UIBaseListController<T>.NavigationTypes navigationTypes = this.navigationType;
			if (navigationTypes != UIBaseListController<T>.NavigationTypes.NextCell)
			{
				if (navigationTypes != UIBaseListController<T>.NavigationTypes.NextCellRange)
				{
					DebugUtility.LogNoEnumSupportError<UIBaseListController<T>.NavigationTypes>(this, this.navigationType);
				}
				else
				{
					int num3 = num - num2;
					num += num3;
				}
			}
			else
			{
				num++;
			}
			this.View.GoToDataIndex(num, 0f, 0f, true, null, 0.25f, new Action(this.ViewControls), UIBaseListView<T>.LoopJumpDirections.Closest, false, true);
		}

		// Token: 0x060009AC RID: 2476 RVA: 0x0002950C File Offset: 0x0002770C
		private void SnapToPrevious()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SnapToPrevious", this);
			}
			int num = this.View.VisibleStartCellDataIndex(false);
			int num2 = this.View.VisibleEndCellDataIndex(false);
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}", new object[] { "visibleStartCellDataIndex", num, "visibleEndCellDataIndex", num2 }), this);
			}
			UIBaseListController<T>.NavigationTypes navigationTypes = this.navigationType;
			if (navigationTypes != UIBaseListController<T>.NavigationTypes.NextCell)
			{
				if (navigationTypes != UIBaseListController<T>.NavigationTypes.NextCellRange)
				{
					DebugUtility.LogNoEnumSupportError<UIBaseListController<T>.NavigationTypes>(this, this.navigationType);
				}
				else
				{
					int num3 = num2 - num;
					num -= num3;
				}
			}
			else
			{
				num--;
			}
			this.View.GoToDataIndex(num, 0f, 0f, true, null, 0.25f, new Action(this.ViewControls), UIBaseListView<T>.LoopJumpDirections.Closest, false, true);
		}

		// Token: 0x060009AD RID: 2477 RVA: 0x000295DF File Offset: 0x000277DF
		private void OnListCellSizeTypeChanged(ListCellSizeTypes listCellSizeTypes)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}, ( {1}: {2} )", "OnListCellSizeTypeChanged", "listCellSizeTypes", listCellSizeTypes), this);
			}
			this.ViewControls();
		}

		// Token: 0x04000609 RID: 1545
		[Tooltip("Use this only if you know what you are doing :P")]
		[SerializeField]
		protected bool IgnoreValidation;

		// Token: 0x0400060A RID: 1546
		[Header("Controls")]
		[SerializeField]
		private bool canSynchronize;

		// Token: 0x0400060B RID: 1547
		[SerializeField]
		private bool canSort = true;

		// Token: 0x0400060C RID: 1548
		[SerializeField]
		private bool canSwitchCells = true;

		// Token: 0x0400060D RID: 1549
		[SerializeField]
		private bool canNavigate;

		// Token: 0x0400060E RID: 1550
		[SerializeField]
		private bool makeSpaceForNavigationButtons = true;

		// Token: 0x0400060F RID: 1551
		[SerializeField]
		private UIButton synchronizeButton;

		// Token: 0x04000610 RID: 1552
		[SerializeField]
		private UIListSortSwitch listSortSwitch;

		// Token: 0x04000611 RID: 1553
		[SerializeField]
		private UIListCellSwitch listCellSwitch;

		// Token: 0x04000612 RID: 1554
		[Header("Navigation")]
		[SerializeField]
		private UIBaseListController<T>.NavigationTypes navigationType;

		// Token: 0x04000613 RID: 1555
		[SerializeField]
		private UIButton previousButton;

		// Token: 0x04000614 RID: 1556
		[SerializeField]
		private UIButton nextButton;

		// Token: 0x02000185 RID: 389
		private enum NavigationTypes
		{
			// Token: 0x04000618 RID: 1560
			NextCell,
			// Token: 0x04000619 RID: 1561
			NextCellRange
		}
	}
}
