using System;
using System.Collections;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000181 RID: 385
	public abstract class UIBaseCloudPaginatedListController<T> : UIBaseCloudListController<T>
	{
		// Token: 0x0600097F RID: 2431 RVA: 0x00028AA8 File Offset: 0x00026CA8
		protected override void Start()
		{
			base.Start();
			base.View.ScrollerScrolledUnityEvent.AddListener(new UnityAction<Vector2, float>(this.OnScrollerScrolled));
			base.View.SnappedUnityEvent.AddListener(new UnityAction<int, int, UIBaseListItemView<T>>(this.OnSnapped));
			base.ListCellSwitch.OnButtonPressed += this.SetPageSize;
		}

		// Token: 0x06000980 RID: 2432 RVA: 0x00028B0C File Offset: 0x00026D0C
		protected override void ViewNavigationControls()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ViewNavigationControls", this);
			}
			int num = base.View.VisibleStartCellDataIndex(true);
			int num2 = base.View.VisibleEndCellDataIndex(true);
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}", new object[] { "visibleStartCellDataIndex", num, "visibleEndCellDataIndex", num2 }), this);
			}
			bool flag = (base.CanNavigate && num > 0) || base.View.Loop;
			bool flag2 = base.CanNavigate && num2 + 1 < this.cloudPaginatedListModel.Count;
			base.SetPreviousButtonVisibility(flag);
			base.SetNextButtonVisibility(flag2);
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x00028BCC File Offset: 0x00026DCC
		protected override void SnapToNext()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("UIBaseCloudPaginatedListController override SnapToNext", this);
			}
			int num = base.View.VisibleEndCellDataIndex(true);
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, {2}.{3}: {4}", new object[]
				{
					"visibleEndCellDataIndex",
					num,
					"Model",
					"DataCount",
					base.Model.DataCount
				}), this);
			}
			if (num + 1 < base.Model.DataCount)
			{
				base.SnapToNext();
			}
		}

		// Token: 0x06000982 RID: 2434 RVA: 0x00028C61 File Offset: 0x00026E61
		private void SetPageSize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetPageSize", this);
			}
			this.cloudPaginatedListModel.SetPageSize(base.View.RowItemCount);
		}

		// Token: 0x06000983 RID: 2435 RVA: 0x00028C8C File Offset: 0x00026E8C
		private void WaitFrameAndSnapToNext()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("WaitFrameAndSnapToNext", this);
			}
			base.StartCoroutine(this.WaitFrameAndSnapToNextCoroutine());
		}

		// Token: 0x06000984 RID: 2436 RVA: 0x00028CAE File Offset: 0x00026EAE
		private IEnumerator WaitFrameAndSnapToNextCoroutine()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("WaitFrameAndSnapToNextCoroutine", this);
			}
			yield return new WaitForEndOfFrame();
			this.SnapToNext();
			yield break;
		}

		// Token: 0x06000985 RID: 2437 RVA: 0x00028CC0 File Offset: 0x00026EC0
		private void OnScrollerScrolled(Vector2 position, float positionInPixels)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "OnScrollerScrolled", "position", position, "positionInPixels", positionInPixels }), this);
			}
			this.TryToRequestMore();
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x00028D1C File Offset: 0x00026F1C
		private void OnSnapped(int snapCellViewIndex, int snapDataIndex, UIBaseListItemView<T> cellView)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[]
				{
					"OnSnapped",
					"snapCellViewIndex",
					snapCellViewIndex,
					"snapDataIndex",
					snapDataIndex,
					"cellView",
					cellView.DebugSafeName(true)
				}), this);
			}
			this.TryToRequestMore();
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x00028D8C File Offset: 0x00026F8C
		private void TryToRequestMore()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("TryToRequestMore", this);
			}
			if (!base.View.ScrollRect.IsAtEnd)
			{
				return;
			}
			if (base.View.VisibleEndCellDataIndex(true) != base.Model.DataCount)
			{
				return;
			}
			if (base.Model.DataCount >= this.cloudPaginatedListModel.Count)
			{
				return;
			}
			this.cloudPaginatedListModel.Request(null);
		}

		// Token: 0x040005FF RID: 1535
		[SerializeField]
		private UIBaseCloudPaginatedListModel<T> cloudPaginatedListModel;
	}
}
