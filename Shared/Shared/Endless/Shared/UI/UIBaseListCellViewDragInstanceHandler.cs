using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000177 RID: 375
	[RequireComponent(typeof(UIDragHandler))]
	public abstract class UIBaseListCellViewDragInstanceHandler<T> : UIGameObject, IDragInstance
	{
		// Token: 0x1700018D RID: 397
		// (get) Token: 0x0600092D RID: 2349 RVA: 0x00027940 File Offset: 0x00025B40
		public UIDragHandler DragHandler
		{
			get
			{
				if (!this.dragHandler)
				{
					base.TryGetComponent<UIDragHandler>(out this.dragHandler);
				}
				return this.dragHandler;
			}
		}

		// Token: 0x0600092E RID: 2350 RVA: 0x00027964 File Offset: 0x00025B64
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.DragHandler.DragUnityEvent.AddListener(new UnityAction(this.OnDrag));
			this.dragInstanceHandler.InstancePositionResetTweenCompleteUnityEvent.AddListener(new UnityAction(this.SetListCellVisible));
			this.dragInstanceHandler.InstancePositionResetTweenCompleteUnityEvent.AddListener(new UnityAction(this.ResetRectTransform));
			base.TryGetComponent<UIDragPositionResetter>(out this.dragPositionResetter);
		}

		// Token: 0x0600092F RID: 2351 RVA: 0x000279E8 File Offset: 0x00025BE8
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("OnDisable", this);
			}
			if (this.waitAndReloadCoroutine != null)
			{
				this.StopAndClearCoroutine(ref this.waitAndReloadCoroutine);
			}
			if (this.cellView.ListView == null)
			{
				return;
			}
			UIBaseListItemView<T> cellViewAtDataIndex = this.cellView.ListView.GetCellViewAtDataIndex(this.cellView.DataIndex);
			if (cellViewAtDataIndex)
			{
				((UIBaseListCellView<T>)cellViewAtDataIndex).SetAlpha(1f);
			}
		}

		// Token: 0x06000930 RID: 2352 RVA: 0x00027A64 File Offset: 0x00025C64
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("OnDestroy", this);
			}
			if (this.waitAndReloadCoroutine != null)
			{
				this.StopAndClearCoroutine(ref this.waitAndReloadCoroutine);
			}
		}

		// Token: 0x06000931 RID: 2353 RVA: 0x00027A90 File Offset: 0x00025C90
		private void OnDrag()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("OnDrag", this);
			}
			if (!this.triggeredInitialDragEvents)
			{
				Action<RectTransform> beginDragWithRectTransformAction = UIBaseListCellViewDragInstanceHandler<T>.BeginDragWithRectTransformAction;
				if (beginDragWithRectTransformAction != null)
				{
					beginDragWithRectTransformAction(base.RectTransform);
				}
				this.TryToSetListViewCellInvisible();
				this.tweenCanvasGroupAlpha.Tween();
				this.triggeredInitialDragEvents = true;
			}
			Action<RectTransform> dragWithRectTransformAction = UIBaseListCellViewDragInstanceHandler<T>.DragWithRectTransformAction;
			if (dragWithRectTransformAction != null)
			{
				dragWithRectTransformAction(base.RectTransform);
			}
			if (!this.insert)
			{
				return;
			}
			if (this.cellView.ListModel.Count <= 1)
			{
				return;
			}
			IReadOnlyList<UIBaseListItemView<T>> cellViews = this.cellView.ListView.GetCellViews(true);
			this.TryToMoveCellsThatOverlap(cellViews);
		}

		// Token: 0x06000932 RID: 2354 RVA: 0x00027B34 File Offset: 0x00025D34
		private void TryToMoveCellsThatOverlap(IReadOnlyList<UIBaseListItemView<T>> listCellViews)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "TryToMoveCellsThatOverlap", "listCellViews", listCellViews.Count<UIBaseListItemView<T>>()), this);
			}
			float num = this.insertIfOverlapPercentageIsEqualToOrOver / 100f;
			foreach (UIBaseListItemView<T> uibaseListItemView in listCellViews)
			{
				if (this.cellView.DataIndex != uibaseListItemView.DataIndex && (!uibaseListItemView.ListModel.AddButtonInserted || uibaseListItemView.DataIndex != 0))
				{
					RectTransform rectTransform = uibaseListItemView.RectTransform;
					ValueTuple<RectTransform, RectTransform> valueTuple = RectTransformUtility.SmallerAndLarger(base.RectTransform, rectTransform);
					float num2 = RectTransformUtility.OverlapPercentage(valueTuple.Item1, valueTuple.Item2);
					if (this.verboseLogging)
					{
						DebugUtility.Log(string.Format("{0}: {1:P0}, ", "overlapPercentage", num2) + string.Format("{0}: {1:P0}", "triggerPercentage", num), this);
					}
					if (num2 >= num)
					{
						UIBaseRearrangeableListModel<T> uibaseRearrangeableListModel = (UIBaseRearrangeableListModel<T>)this.cellView.ListModel;
						int dataIndex = this.cellView.DataIndex;
						int dataIndex2 = uibaseListItemView.DataIndex;
						List<int> list = new List<int>();
						list.Add(dataIndex);
						list.Add(dataIndex2);
						int num3 = list.Min();
						int num4 = list.Max();
						uibaseRearrangeableListModel.Move(dataIndex, dataIndex2, true);
						this.cellView.SetDataIndex(uibaseListItemView.DataIndex);
						bool flag = dataIndex2 < dataIndex;
						this.TweenListCells(listCellViews, flag, num3, num4);
						break;
					}
				}
			}
		}

		// Token: 0x06000933 RID: 2355 RVA: 0x00027CD8 File Offset: 0x00025ED8
		private void TweenListCells(IReadOnlyList<UIBaseListItemView<T>> listCellViews, bool forward, int smallerDataIndex, int largerDataIndex)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8} )", new object[] { "TweenListCells", "listCellViews", listCellViews.Count, "forward", forward, "smallerDataIndex", smallerDataIndex, "largerDataIndex", largerDataIndex }), this);
				DebugUtility.Log(base.gameObject.name, base.gameObject);
			}
			int num = this.cellView.ListView.VisibleStartCellDataIndex(true);
			if (smallerDataIndex < num)
			{
				return;
			}
			int num2 = this.cellView.ListView.VisibleEndCellDataIndex(true);
			if (largerDataIndex > num2)
			{
				return;
			}
			for (int i = smallerDataIndex; i <= largerDataIndex; i++)
			{
				if ((i != 0 || forward) && (i != listCellViews.Count - 1 || !forward))
				{
					int num3 = i + (forward ? 1 : (-1));
					if (this.verboseLogging)
					{
						DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}", new object[] { "i", i, "tweenToIndex", num3 }), base.gameObject);
					}
					if (forward && num3 > largerDataIndex)
					{
						break;
					}
					if ((forward || num3 >= smallerDataIndex) && (!this.cellView.ListModel.AddButtonInserted || num3 != 0))
					{
						UIBaseListCellView<T> uibaseListCellView = (UIBaseListCellView<T>)this.cellView.ListView.GetCellViewAtDataIndex(i);
						if (!uibaseListCellView)
						{
							DebugUtility.LogError(string.Format("Could not find cell with data index '{0}'!", i), this);
						}
						UIBaseListCellView<T> uibaseListCellView2 = (UIBaseListCellView<T>)this.cellView.ListView.GetCellViewAtDataIndex(num3);
						if (!uibaseListCellView2)
						{
							DebugUtility.LogError(string.Format("Could not find cell with data index '{0}'!", num3), this);
						}
						if (this.verboseLogging)
						{
							DebugUtility.Log(string.Format("{0} | {1}: {2} | {3} => {4}", new object[] { "TweenContainerTo", "tweenToIndex", num3, uibaseListCellView.DataIndex, uibaseListCellView2.DataIndex }), uibaseListCellView);
						}
						uibaseListCellView.TweenContainerTo(uibaseListCellView2.Container, this.insertTweenDuration);
					}
				}
			}
			if (this.dragPositionResetter)
			{
				UIBaseListItemView<T> cellViewAtDataIndex = this.cellView.ListView.GetCellViewAtDataIndex(this.cellView.DataIndex);
				if (cellViewAtDataIndex)
				{
					this.dragPositionResetter.SetOriginalPosition(cellViewAtDataIndex.transform.position);
				}
			}
			this.waitAndReloadCoroutine = base.StartCoroutine(this.WaitAndReload());
		}

		// Token: 0x06000934 RID: 2356 RVA: 0x00027F79 File Offset: 0x00026179
		private IEnumerator WaitAndReload()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}: {2}", "WaitAndReload", "insertTweenDuration", this.insertTweenDuration), this);
			}
			yield return new WaitForSeconds(this.insertTweenDuration);
			this.cellView.ListModel.TriggerModelChanged();
			this.TryToSetListViewCellInvisible();
			if (this.dragPositionResetter)
			{
				UIBaseListItemView<T> cellViewAtDataIndex = this.cellView.ListView.GetCellViewAtDataIndex(this.cellView.DataIndex);
				if (cellViewAtDataIndex)
				{
					this.dragPositionResetter.SetOriginalPosition(cellViewAtDataIndex.transform.position);
				}
			}
			this.waitAndReloadCoroutine = null;
			yield break;
		}

		// Token: 0x06000935 RID: 2357 RVA: 0x00027F88 File Offset: 0x00026188
		private void StopAndClearCoroutine(ref Coroutine coroutine)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("StopAndClearCoroutine", this);
			}
			if (coroutine == null)
			{
				return;
			}
			base.StopCoroutine(coroutine);
			coroutine = null;
		}

		// Token: 0x06000936 RID: 2358 RVA: 0x00027FB0 File Offset: 0x000261B0
		private void TryToSetListViewCellInvisible()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("TryToSetListViewCellInvisible", this);
			}
			UIBaseListItemView<T> cellViewAtDataIndex = this.cellView.ListView.GetCellViewAtDataIndex(this.cellView.DataIndex);
			if (!cellViewAtDataIndex)
			{
				DebugUtility.Log("TryToSetListViewCellInvisible | " + base.gameObject.name + " | RETURNED!?!", this);
				return;
			}
			((UIBaseListCellView<T>)cellViewAtDataIndex).SetAlpha(0f);
		}

		// Token: 0x06000937 RID: 2359 RVA: 0x00028025 File Offset: 0x00026225
		private void SetListCellVisible()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("SetListCellVisible", this);
			}
			this.cellView.SetAlpha(1f);
		}

		// Token: 0x06000938 RID: 2360 RVA: 0x0002804A File Offset: 0x0002624A
		private void ResetRectTransform()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("ResetRectTransform", this);
			}
			this.cellView.ListView.ResetAllCellPositions();
		}

		// Token: 0x040005CD RID: 1485
		public static Action<RectTransform> BeginDragWithRectTransformAction;

		// Token: 0x040005CE RID: 1486
		public static Action<RectTransform> DragWithRectTransformAction;

		// Token: 0x040005CF RID: 1487
		[SerializeField]
		private UIBaseListCellView<T> cellView;

		// Token: 0x040005D0 RID: 1488
		[SerializeField]
		private UIDragInstanceHandler dragInstanceHandler;

		// Token: 0x040005D1 RID: 1489
		[SerializeField]
		private TweenCanvasGroupAlpha tweenCanvasGroupAlpha;

		// Token: 0x040005D2 RID: 1490
		[SerializeField]
		private bool insert;

		// Token: 0x040005D3 RID: 1491
		[Range(0f, 100f)]
		[SerializeField]
		private float insertIfOverlapPercentageIsEqualToOrOver = 50f;

		// Token: 0x040005D4 RID: 1492
		[Min(0f)]
		[SerializeField]
		private float insertTweenDuration = 0.25f;

		// Token: 0x040005D5 RID: 1493
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040005D6 RID: 1494
		private UIDragHandler dragHandler;

		// Token: 0x040005D7 RID: 1495
		private bool triggeredInitialDragEvents;

		// Token: 0x040005D8 RID: 1496
		private Coroutine waitAndReloadCoroutine;

		// Token: 0x040005D9 RID: 1497
		private UIDragPositionResetter dragPositionResetter;
	}
}
