using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.Debugging;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000196 RID: 406
	public abstract class UIBaseListModel<T> : UIGameObject, IListModel
	{
		// Token: 0x170001CC RID: 460
		// (get) Token: 0x06000A0D RID: 2573 RVA: 0x0002AFE6 File Offset: 0x000291E6
		// (set) Token: 0x06000A0E RID: 2574 RVA: 0x0002AFEE File Offset: 0x000291EE
		public SortOrders SortOrder { get; private set; }

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x06000A0F RID: 2575 RVA: 0x0002AFF7 File Offset: 0x000291F7
		// (set) Token: 0x06000A10 RID: 2576 RVA: 0x0002AFFF File Offset: 0x000291FF
		public bool DisplayAddButton { get; protected set; }

		// Token: 0x170001CE RID: 462
		// (get) Token: 0x06000A11 RID: 2577 RVA: 0x0002B008 File Offset: 0x00029208
		// (set) Token: 0x06000A12 RID: 2578 RVA: 0x0002B010 File Offset: 0x00029210
		public bool AddButtonIsInteractable { get; private set; }

		// Token: 0x170001CF RID: 463
		// (get) Token: 0x06000A13 RID: 2579 RVA: 0x0002B019 File Offset: 0x00029219
		// (set) Token: 0x06000A14 RID: 2580 RVA: 0x0002B021 File Offset: 0x00029221
		public bool UserCanRemove { get; private set; }

		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x06000A15 RID: 2581 RVA: 0x0002B02A File Offset: 0x0002922A
		// (set) Token: 0x06000A16 RID: 2582 RVA: 0x0002B032 File Offset: 0x00029232
		public bool RestrictSelectionCountTo1 { get; private set; }

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x06000A17 RID: 2583 RVA: 0x0002B03B File Offset: 0x0002923B
		// (set) Token: 0x06000A18 RID: 2584 RVA: 0x0002B043 File Offset: 0x00029243
		public bool MinimumSelectionCountTo1 { get; private set; }

		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x06000A19 RID: 2585 RVA: 0x0002B04C File Offset: 0x0002924C
		// (set) Token: 0x06000A1A RID: 2586 RVA: 0x0002B054 File Offset: 0x00029254
		protected bool VerboseLogging { get; set; }

		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x06000A1B RID: 2587 RVA: 0x0002B05D File Offset: 0x0002925D
		// (set) Token: 0x06000A1C RID: 2588 RVA: 0x0002B065 File Offset: 0x00029265
		protected bool SuperVerboseLogging { get; set; }

		// Token: 0x170001D4 RID: 468
		public virtual T this[int index]
		{
			get
			{
				return this.List[index];
			}
		}

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x06000A1E RID: 2590 RVA: 0x0002B07C File Offset: 0x0002927C
		// (set) Token: 0x06000A1F RID: 2591 RVA: 0x0002B084 File Offset: 0x00029284
		public bool AddButtonInserted { get; protected set; }

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x06000A20 RID: 2592 RVA: 0x0002B08D File Offset: 0x0002928D
		public int DataCount
		{
			get
			{
				if (!this.AddButtonInserted)
				{
					return this.Count;
				}
				return this.Count - 1;
			}
		}

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x06000A21 RID: 2593 RVA: 0x0002B0A6 File Offset: 0x000292A6
		public virtual int Count
		{
			get
			{
				return this.List.Count;
			}
		}

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x06000A22 RID: 2594 RVA: 0x0002B0B3 File Offset: 0x000292B3
		public IReadOnlyList<T> ReadOnlyList
		{
			get
			{
				if (!this.AddButtonInserted)
				{
					return this.List;
				}
				return this.List.Skip(1).ToList<T>();
			}
		}

		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x06000A23 RID: 2595 RVA: 0x0002B0D5 File Offset: 0x000292D5
		public IReadOnlyList<int> ReadOnlySelectedList
		{
			get
			{
				return this.selectedList;
			}
		}

		// Token: 0x170001DA RID: 474
		// (get) Token: 0x06000A24 RID: 2596 RVA: 0x0002B0E0 File Offset: 0x000292E0
		public virtual IReadOnlyList<T> SelectedTypedList
		{
			get
			{
				List<T> list = new List<T>();
				foreach (int num in this.selectedList)
				{
					if (num < 0 || num >= this.Count)
					{
						DebugUtility.LogError(string.Format("{0} is {1} while {2} is {3}! {4}!", new object[] { "selectedItemIndex", num, "Count", this.Count, "IndexOutOfRangeException" }), this);
					}
					else
					{
						list.Add(this.List[num]);
					}
				}
				return list;
			}
		}

		// Token: 0x170001DB RID: 475
		// (get) Token: 0x06000A25 RID: 2597 RVA: 0x0002B19C File Offset: 0x0002939C
		protected bool AddButtonNeedsInserting
		{
			get
			{
				return this.DisplayAddButton && !this.AddButtonInserted;
			}
		}

		// Token: 0x06000A26 RID: 2598 RVA: 0x0002B1B4 File Offset: 0x000293B4
		public virtual void Set(List<T> list, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "Set", "list", list.Count, "triggerEvents", triggerEvents }), this);
			}
			this.PruneSelectedOutOfBounds(false);
			this.List = list;
			this.AddButtonInserted = false;
			if (this.DisplayAddButton)
			{
				this.InsertAddButton(false);
			}
			if (!triggerEvents)
			{
				return;
			}
			this.TriggerModelChanged();
		}

		// Token: 0x06000A27 RID: 2599 RVA: 0x0002B23C File Offset: 0x0002943C
		public virtual void Add(T item, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "Add", "item", item, "triggerEvents", triggerEvents }), this);
			}
			this.List.Add(item);
			if (this.AddButtonNeedsInserting)
			{
				this.InsertAddButton(false);
			}
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, T> itemAddedAction = UIBaseListModel<T>.ItemAddedAction;
			if (itemAddedAction != null)
			{
				itemAddedAction(this, item);
			}
			this.ItemAddedUnityEvent.Invoke(item);
			this.TriggerModelChanged();
		}

		// Token: 0x06000A28 RID: 2600 RVA: 0x0002B2D4 File Offset: 0x000294D4
		public virtual void AddRange(List<T> collection, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "AddRange", "collection", collection.Count, "triggerEvents", triggerEvents }), this);
			}
			this.List.AddRange(collection);
			if (this.AddButtonNeedsInserting)
			{
				this.InsertAddButton(false);
			}
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, List<T>> rangeAddedAction = UIBaseListModel<T>.RangeAddedAction;
			if (rangeAddedAction != null)
			{
				rangeAddedAction(this, collection);
			}
			this.RangeAddedUnityEvent.Invoke(collection);
			this.TriggerModelChanged();
		}

		// Token: 0x06000A29 RID: 2601 RVA: 0x0002B374 File Offset: 0x00029574
		public virtual void Insert(int index, T item, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "Insert", "index", index, "item", item, "triggerEvents", triggerEvents }), this);
			}
			if (index < 0 || index > this.Count)
			{
				DebugUtility.LogException(new ArgumentOutOfRangeException("index", string.Format("{0} is out of range ( {1}: {2} ).", "index", "Count", this.Count)), this);
				return;
			}
			this.List.Insert(index, item);
			if (this.AddButtonNeedsInserting)
			{
				this.InsertAddButton(false);
			}
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, int, T> itemInsertedAction = UIBaseListModel<T>.ItemInsertedAction;
			if (itemInsertedAction != null)
			{
				itemInsertedAction(this, index, item);
			}
			this.ItemInsertedUnityEvent.Invoke(index, item);
			this.TriggerModelChanged();
		}

		// Token: 0x06000A2A RID: 2602 RVA: 0x0002B460 File Offset: 0x00029660
		public virtual void RemoveAt(int index, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "RemoveAt", "index", index, "triggerEvents", triggerEvents }), this);
			}
			if (index < 0 || index >= this.Count)
			{
				DebugUtility.LogException(new ArgumentOutOfRangeException("index", string.Format("{0} is out of range ( {1}: {2} ).", "index", "Count", this.Count)), this);
				return;
			}
			if (this.IsSelected(index))
			{
				this.Unselect(index, false);
			}
			T t = this.List[index];
			this.List.RemoveAt(index);
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, int, T> itemRemovedAction = UIBaseListModel<T>.ItemRemovedAction;
			if (itemRemovedAction != null)
			{
				itemRemovedAction(this, index, t);
			}
			this.ItemRemovedUnityEvent.Invoke(index, t);
			this.TriggerModelChanged();
		}

		// Token: 0x06000A2B RID: 2603 RVA: 0x0002B548 File Offset: 0x00029748
		public virtual void RemoveRange(int index, int count, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "RemoveRange", "index", index, "count", count, "triggerEvents", triggerEvents }), this);
			}
			if (index < 0 || index >= this.Count)
			{
				DebugUtility.LogException(new ArgumentOutOfRangeException("index", string.Format("{0} is out of range ( {1}: {2} ).", "index", "Count", this.Count)), this);
				return;
			}
			List<T> range = this.List.GetRange(index, count);
			this.List.RemoveRange(index, count);
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, int, int, List<T>> rangeRemovedAction = UIBaseListModel<T>.RangeRemovedAction;
			if (rangeRemovedAction != null)
			{
				rangeRemovedAction(this, index, count, range);
			}
			this.RangeRemovedUnityEvent.Invoke(index, count, range);
			this.TriggerModelChanged();
		}

		// Token: 0x06000A2C RID: 2604 RVA: 0x0002B634 File Offset: 0x00029834
		public virtual void Clear(bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Clear", "triggerEvents", triggerEvents), this);
			}
			this.selectedHashSet.Clear();
			this.selectedList.Clear();
			this.List.Clear();
			if (this.DisplayAddButton)
			{
				this.InsertAddButton(false);
			}
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>> clearedAction = UIBaseListModel<T>.ClearedAction;
			if (clearedAction != null)
			{
				clearedAction(this);
			}
			this.ClearedUnityEvent.Invoke();
			this.TriggerModelChanged();
		}

		// Token: 0x06000A2D RID: 2605 RVA: 0x0002B6C0 File Offset: 0x000298C0
		public virtual void SetItem(int index, T newValue, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "SetItem", "index", index, "newValue", newValue, "triggerEvents", triggerEvents }), this);
			}
			if (index < 0 || index >= this.Count)
			{
				DebugUtility.LogException(new ArgumentOutOfRangeException("index", string.Format("{0} is out of range ( {1}: {2} ).", "index", "Count", this.Count)), this);
				return;
			}
			this.List[index] = newValue;
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, int, T> itemSetAction = UIBaseListModel<T>.ItemSetAction;
			if (itemSetAction != null)
			{
				itemSetAction(this, index, newValue);
			}
			this.ItemSetUnityEvent.Invoke(index, newValue);
			this.TriggerModelChanged();
		}

		// Token: 0x06000A2E RID: 2606 RVA: 0x0002B79C File Offset: 0x0002999C
		public void ClearSelected(bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ClearSelected", "triggerEvents", triggerEvents), this);
			}
			this.selectedHashSet.Clear();
			this.selectedList.Clear();
			if (!triggerEvents)
			{
				return;
			}
			this.TriggerModelChanged();
		}

		// Token: 0x06000A2F RID: 2607 RVA: 0x0002B7F4 File Offset: 0x000299F4
		public virtual void Select(int index, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "Select", "index", index, "triggerEvents", triggerEvents }), this);
			}
			if (this.IsSelected(index))
			{
				return;
			}
			if (this.RestrictSelectionCountTo1 && this.selectedHashSet.Count > 0)
			{
				this.ClearSelected(false);
			}
			this.selectedHashSet.Add(index);
			this.selectedList.Add(index);
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, int> itemSelectedAction = UIBaseListModel<T>.ItemSelectedAction;
			if (itemSelectedAction != null)
			{
				itemSelectedAction(this, index);
			}
			this.ItemSelectedUnityEvent.Invoke(index);
			Action<UIBaseListModel<T>, int, bool> selectionChangedAction = UIBaseListModel<T>.SelectionChangedAction;
			if (selectionChangedAction != null)
			{
				selectionChangedAction(this, index, true);
			}
			this.SelectionChangedUnityEvent.Invoke(index, true);
			this.TriggerModelChanged();
		}

		// Token: 0x06000A30 RID: 2608 RVA: 0x0002B8D4 File Offset: 0x00029AD4
		public virtual void Unselect(int index, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "Unselect", "index", index, "triggerEvents", triggerEvents }), this);
			}
			if (!this.IsSelected(index))
			{
				return;
			}
			if ((this.RestrictSelectionCountTo1 || this.MinimumSelectionCountTo1) && this.selectedHashSet.Count == 1)
			{
				return;
			}
			this.selectedHashSet.Remove(index);
			this.selectedList.Remove(index);
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, int> itemUnselectedAction = UIBaseListModel<T>.ItemUnselectedAction;
			if (itemUnselectedAction != null)
			{
				itemUnselectedAction(this, index);
			}
			this.ItemUnselectedUnityEvent.Invoke(index);
			Action<UIBaseListModel<T>, int, bool> selectionChangedAction = UIBaseListModel<T>.SelectionChangedAction;
			if (selectionChangedAction != null)
			{
				selectionChangedAction(this, index, false);
			}
			this.SelectionChangedUnityEvent.Invoke(index, false);
			this.TriggerModelChanged();
		}

		// Token: 0x06000A31 RID: 2609 RVA: 0x0002B9B4 File Offset: 0x00029BB4
		public void ToggleSelected(int index, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "ToggleSelected", "index", index, "triggerEvents", triggerEvents }), this);
			}
			if (this.IsSelected(index))
			{
				this.Unselect(index, triggerEvents);
				return;
			}
			this.Select(index, triggerEvents);
		}

		// Token: 0x06000A32 RID: 2610 RVA: 0x0002BA23 File Offset: 0x00029C23
		public bool IsSelected(int index)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "IsSelected", "index", index), this);
			}
			return this.selectedHashSet.Contains(index);
		}

		// Token: 0x06000A33 RID: 2611 RVA: 0x0002BA5C File Offset: 0x00029C5C
		public int SelectedOrder(int index)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SelectedOrder", "index", index), this);
			}
			if (!this.selectedHashSet.Contains(index))
			{
				return -1;
			}
			return this.selectedList.IndexOf(index) + 1;
		}

		// Token: 0x06000A34 RID: 2612 RVA: 0x0002BAB0 File Offset: 0x00029CB0
		public virtual void SetSortOrder(SortOrders value)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetSortOrder", "value", value), this);
			}
			this.SortOrder = value;
			Action<UIBaseListModel<T>, SortOrders> sortOrderChangedAction = UIBaseListModel<T>.SortOrderChangedAction;
			if (sortOrderChangedAction != null)
			{
				sortOrderChangedAction(this, this.SortOrder);
			}
			this.SortOrderChangedUnityEvent.Invoke(this.SortOrder);
		}

		// Token: 0x06000A35 RID: 2613 RVA: 0x0002BB14 File Offset: 0x00029D14
		public virtual void Swap(int indexA, int indexB, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "Swap", "indexA", indexA, "indexB", indexB, "triggerEvents", triggerEvents }), this);
			}
			T t = this.List[indexA];
			T t2 = this.List[indexB];
			bool flag = this.IsSelected(indexA);
			bool flag2 = this.IsSelected(indexB);
			if (flag)
			{
				this.Unselect(indexA, false);
			}
			if (flag2)
			{
				this.Unselect(indexB, false);
			}
			this.List[indexA] = t2;
			this.List[indexB] = t;
			if (flag)
			{
				this.Select(indexB, false);
			}
			if (flag2)
			{
				this.Select(indexA, false);
			}
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, int, int> itemSwappedAction = UIBaseListModel<T>.ItemSwappedAction;
			if (itemSwappedAction != null)
			{
				itemSwappedAction(this, indexA, indexB);
			}
			this.ItemSwappedUnityEvent.Invoke(indexA, indexB);
			this.TriggerModelChanged();
		}

		// Token: 0x06000A36 RID: 2614 RVA: 0x0002BC14 File Offset: 0x00029E14
		public virtual void MoveUp(int index, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "MoveUp", "index", index, "triggerEvents", triggerEvents }), this);
			}
			this.Swap(index, index - 1, triggerEvents);
		}

		// Token: 0x06000A37 RID: 2615 RVA: 0x0002BC74 File Offset: 0x00029E74
		public virtual void MoveDown(int index, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "MoveDown", "index", index, "triggerEvents", triggerEvents }), this);
			}
			this.Swap(index, index + 1, triggerEvents);
		}

		// Token: 0x06000A38 RID: 2616 RVA: 0x0002BCD4 File Offset: 0x00029ED4
		public void SetAddButtonIsInteractable(bool value)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetAddButtonIsInteractable", "value", value), this);
			}
			this.AddButtonIsInteractable = value;
			Action<UIBaseListModel<T>, bool> addButtonIsInteractableChangedAction = UIBaseListModel<T>.AddButtonIsInteractableChangedAction;
			if (addButtonIsInteractableChangedAction != null)
			{
				addButtonIsInteractableChangedAction(this, this.AddButtonIsInteractable);
			}
			this.AddButtonIsInteractableChangedUnityEvent.Invoke(this.AddButtonIsInteractable);
		}

		// Token: 0x06000A39 RID: 2617 RVA: 0x0002BD38 File Offset: 0x00029F38
		public void SetUserCanRemove(bool userCanRemove, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetUserCanRemove", "userCanRemove", userCanRemove, "triggerEvents", triggerEvents }), this);
			}
			this.UserCanRemove = userCanRemove;
			if (triggerEvents)
			{
				this.TriggerModelChanged();
			}
		}

		// Token: 0x06000A3A RID: 2618 RVA: 0x0002BDA0 File Offset: 0x00029FA0
		public void SetRestrictSelectionCountTo1(bool newValue, bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetRestrictSelectionCountTo1", "newValue", newValue, "triggerEvents", triggerEvents }), this);
			}
			this.RestrictSelectionCountTo1 = newValue;
			if (triggerEvents)
			{
				this.TriggerModelChanged();
			}
		}

		// Token: 0x06000A3B RID: 2619 RVA: 0x0002BE05 File Offset: 0x0002A005
		public void TriggerModelChanged()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("TriggerModelChanged", this);
			}
			Action<UIBaseListModel<T>> modelChangedAction = UIBaseListModel<T>.ModelChangedAction;
			if (modelChangedAction != null)
			{
				modelChangedAction(this);
			}
			this.ModelChangedUnityEvent.Invoke();
		}

		// Token: 0x06000A3C RID: 2620 RVA: 0x0002BE38 File Offset: 0x0002A038
		protected virtual void InsertAddButton(bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InsertAddButton", "triggerEvents", triggerEvents), this);
			}
			this.AddButtonInserted = true;
			this.List.Insert(0, default(T));
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, int, T> itemInsertedAction = UIBaseListModel<T>.ItemInsertedAction;
			if (itemInsertedAction != null)
			{
				itemInsertedAction(this, 0, this.List[0]);
			}
			this.ItemInsertedUnityEvent.Invoke(0, this.List[0]);
			this.TriggerModelChanged();
		}

		// Token: 0x06000A3D RID: 2621 RVA: 0x0002BECC File Offset: 0x0002A0CC
		protected virtual void DebugList()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}.Count: {2}", "DebugList", "List", this.List.Count), this);
			}
			foreach (T t in this.List)
			{
				DebugUtility.Log(t.ToString(), this);
			}
		}

		// Token: 0x06000A3E RID: 2622 RVA: 0x0002BF60 File Offset: 0x0002A160
		protected virtual void DebugSelectedList()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}.Count: {2}", "DebugSelectedList", "SelectedTypedList", this.SelectedTypedList.Count), this);
			}
			foreach (T t in this.SelectedTypedList)
			{
				DebugUtility.Log(t.ToString(), this);
			}
		}

		// Token: 0x06000A3F RID: 2623 RVA: 0x0002BFEC File Offset: 0x0002A1EC
		private void PruneSelectedOutOfBounds(bool triggerEvents)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "PruneSelectedOutOfBounds", "triggerEvents", triggerEvents), this);
			}
			for (int i = this.selectedList.Count - 1; i >= 0; i--)
			{
				if (this.selectedList[i] < 0 || this.selectedList[i] >= this.Count)
				{
					if (this.VerboseLogging)
					{
						DebugUtility.Log(string.Format("Pruning {0} '{1}' out of selected | {2}: {3}", new object[]
						{
							i,
							this.selectedList[i],
							"Count",
							this.Count
						}), this);
					}
					this.selectedHashSet.Remove(this.selectedList[i]);
					this.selectedList.RemoveAt(i);
				}
			}
			if (!triggerEvents)
			{
				return;
			}
			this.TriggerModelChanged();
		}

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x06000A40 RID: 2624 RVA: 0x0002C0E1 File Offset: 0x0002A2E1
		public UnityEvent ModelChangedUnityEvent { get; } = new UnityEvent();

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x06000A41 RID: 2625 RVA: 0x0002C0E9 File Offset: 0x0002A2E9
		public UnityEvent<T> ItemAddedUnityEvent { get; } = new UnityEvent<T>();

		// Token: 0x170001DE RID: 478
		// (get) Token: 0x06000A42 RID: 2626 RVA: 0x0002C0F1 File Offset: 0x0002A2F1
		public UnityEvent<List<T>> RangeAddedUnityEvent { get; } = new UnityEvent<List<T>>();

		// Token: 0x170001DF RID: 479
		// (get) Token: 0x06000A43 RID: 2627 RVA: 0x0002C0F9 File Offset: 0x0002A2F9
		public UnityEvent<int, T> ItemInsertedUnityEvent { get; } = new UnityEvent<int, T>();

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x06000A44 RID: 2628 RVA: 0x0002C101 File Offset: 0x0002A301
		public UnityEvent<int, T> ItemRemovedUnityEvent { get; } = new UnityEvent<int, T>();

		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06000A45 RID: 2629 RVA: 0x0002C109 File Offset: 0x0002A309
		public UnityEvent<int, int, List<T>> RangeRemovedUnityEvent { get; } = new UnityEvent<int, int, List<T>>();

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x06000A46 RID: 2630 RVA: 0x0002C111 File Offset: 0x0002A311
		public UnityEvent ClearedUnityEvent { get; } = new UnityEvent();

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x06000A47 RID: 2631 RVA: 0x0002C119 File Offset: 0x0002A319
		public UnityEvent<int, T> ItemSetUnityEvent { get; } = new UnityEvent<int, T>();

		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x06000A48 RID: 2632 RVA: 0x0002C121 File Offset: 0x0002A321
		public UnityEvent<int, bool> SelectionChangedUnityEvent { get; } = new UnityEvent<int, bool>();

		// Token: 0x170001E5 RID: 485
		// (get) Token: 0x06000A49 RID: 2633 RVA: 0x0002C129 File Offset: 0x0002A329
		public UnityEvent<int> ItemSelectedUnityEvent { get; } = new UnityEvent<int>();

		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x06000A4A RID: 2634 RVA: 0x0002C131 File Offset: 0x0002A331
		public UnityEvent<int> ItemUnselectedUnityEvent { get; } = new UnityEvent<int>();

		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x06000A4B RID: 2635 RVA: 0x0002C139 File Offset: 0x0002A339
		public UnityEvent<SortOrders> SortOrderChangedUnityEvent { get; } = new UnityEvent<SortOrders>();

		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x06000A4C RID: 2636 RVA: 0x0002C141 File Offset: 0x0002A341
		public UnityEvent<bool> AddButtonIsInteractableChangedUnityEvent { get; } = new UnityEvent<bool>();

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x06000A4D RID: 2637 RVA: 0x0002C149 File Offset: 0x0002A349
		public UnityEvent<int, int> ItemSwappedUnityEvent { get; } = new UnityEvent<int, int>();

		// Token: 0x04000663 RID: 1635
		protected List<T> List = new List<T>();

		// Token: 0x04000664 RID: 1636
		private readonly HashSet<int> selectedHashSet = new HashSet<int>();

		// Token: 0x04000665 RID: 1637
		private readonly List<int> selectedList = new List<int>();

		// Token: 0x04000667 RID: 1639
		public static readonly Action<UIBaseListModel<T>> ModelChangedAction;

		// Token: 0x04000668 RID: 1640
		public static readonly Action<UIBaseListModel<T>, T> ItemAddedAction;

		// Token: 0x04000669 RID: 1641
		public static readonly Action<UIBaseListModel<T>, List<T>> RangeAddedAction;

		// Token: 0x0400066A RID: 1642
		public static readonly Action<UIBaseListModel<T>, int, T> ItemInsertedAction;

		// Token: 0x0400066B RID: 1643
		public static readonly Action<UIBaseListModel<T>, int, T> ItemRemovedAction;

		// Token: 0x0400066C RID: 1644
		public static readonly Action<UIBaseListModel<T>, int, int, List<T>> RangeRemovedAction;

		// Token: 0x0400066D RID: 1645
		public static readonly Action<UIBaseListModel<T>> ClearedAction;

		// Token: 0x0400066E RID: 1646
		public static readonly Action<UIBaseListModel<T>, int, T> ItemSetAction;

		// Token: 0x0400066F RID: 1647
		public static readonly Action<UIBaseListModel<T>, int, bool> SelectionChangedAction;

		// Token: 0x04000670 RID: 1648
		public static readonly Action<UIBaseListModel<T>, int> ItemSelectedAction;

		// Token: 0x04000671 RID: 1649
		public static readonly Action<UIBaseListModel<T>, int> ItemUnselectedAction;

		// Token: 0x04000672 RID: 1650
		public static readonly Action<UIBaseListModel<T>, SortOrders> SortOrderChangedAction;

		// Token: 0x04000673 RID: 1651
		public static readonly Action<UIBaseListModel<T>, bool> AddButtonIsInteractableChangedAction;

		// Token: 0x04000674 RID: 1652
		public static readonly Action<UIBaseListModel<T>, int, int> ItemSwappedAction;
	}
}
