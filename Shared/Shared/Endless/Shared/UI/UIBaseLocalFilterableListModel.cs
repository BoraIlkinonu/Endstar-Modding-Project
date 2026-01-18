using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000197 RID: 407
	public abstract class UIBaseLocalFilterableListModel<T> : UIBaseListModel<T>
	{
		// Token: 0x170001EA RID: 490
		// (get) Token: 0x06000A4F RID: 2639 RVA: 0x0002C222 File Offset: 0x0002A422
		// (set) Token: 0x06000A50 RID: 2640 RVA: 0x0002C22A File Offset: 0x0002A42A
		public bool SortOnChange { get; private set; }

		// Token: 0x170001EB RID: 491
		public override T this[int index]
		{
			get
			{
				return this.filteredList[index];
			}
		}

		// Token: 0x170001EC RID: 492
		// (get) Token: 0x06000A52 RID: 2642 RVA: 0x0002C241 File Offset: 0x0002A441
		public UnityEvent<bool> SortChangedUnityEvent { get; } = new UnityEvent<bool>();

		// Token: 0x170001ED RID: 493
		// (get) Token: 0x06000A53 RID: 2643 RVA: 0x0002C249 File Offset: 0x0002A449
		public override int Count
		{
			get
			{
				return this.filteredList.Count;
			}
		}

		// Token: 0x170001EE RID: 494
		// (get) Token: 0x06000A54 RID: 2644 RVA: 0x0002C256 File Offset: 0x0002A456
		public IReadOnlyList<T> FilteredList
		{
			get
			{
				return this.filteredList;
			}
		}

		// Token: 0x170001EF RID: 495
		// (get) Token: 0x06000A55 RID: 2645 RVA: 0x0002C260 File Offset: 0x0002A460
		public override IReadOnlyList<T> SelectedTypedList
		{
			get
			{
				List<T> list = new List<T>();
				foreach (int num in base.ReadOnlySelectedList)
				{
					if (num < 0 || num >= this.Count)
					{
						DebugUtility.LogError(string.Format("{0} is {1} while {2} is {3}! {4}!", new object[] { "selectedItemIndex", num, "Count", this.Count, "IndexOutOfRangeException" }), this);
					}
					else
					{
						list.Add(this.filteredList[num]);
					}
				}
				return list;
			}
		}

		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x06000A56 RID: 2646
		protected abstract Comparison<T> DefaultSort { get; }

		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x06000A57 RID: 2647 RVA: 0x0002C314 File Offset: 0x0002A514
		// (set) Token: 0x06000A58 RID: 2648 RVA: 0x0002C31C File Offset: 0x0002A51C
		private Comparison<T> ActiveSort { get; set; }

		// Token: 0x06000A59 RID: 2649 RVA: 0x0002C328 File Offset: 0x0002A528
		public override void Set(List<T> list, bool triggerEvents)
		{
			bool displayAddButton = base.DisplayAddButton;
			base.DisplayAddButton = false;
			base.Set(list, false);
			this.HandleFilterAndSort(displayAddButton, triggerEvents);
		}

		// Token: 0x06000A5A RID: 2650 RVA: 0x0002C354 File Offset: 0x0002A554
		public override void Add(T item, bool triggerEvents)
		{
			bool displayAddButton = base.DisplayAddButton;
			base.DisplayAddButton = false;
			base.Add(item, false);
			this.HandleFilterAndSort(displayAddButton, triggerEvents);
		}

		// Token: 0x06000A5B RID: 2651 RVA: 0x0002C380 File Offset: 0x0002A580
		public override void Insert(int index, T item, bool triggerEvents)
		{
			bool displayAddButton = base.DisplayAddButton;
			base.DisplayAddButton = false;
			base.Insert(index, item, false);
			this.HandleFilterAndSort(displayAddButton, triggerEvents);
		}

		// Token: 0x06000A5C RID: 2652 RVA: 0x0002C3AC File Offset: 0x0002A5AC
		public override void RemoveAt(int index, bool triggerEvents)
		{
			DebugUtility.LogWarning("Consider using RemoveFilteredAt instead of RemoveAt!", this);
			base.RemoveAt(index, triggerEvents);
		}

		// Token: 0x06000A5D RID: 2653 RVA: 0x0002C3C4 File Offset: 0x0002A5C4
		public override void RemoveRange(int index, int count, bool triggerEvents)
		{
			bool displayAddButton = base.DisplayAddButton;
			base.DisplayAddButton = false;
			base.RemoveRange(index, count, false);
			this.HandleFilterAndSort(displayAddButton, triggerEvents);
		}

		// Token: 0x06000A5E RID: 2654 RVA: 0x0002C3F0 File Offset: 0x0002A5F0
		public override void Clear(bool triggerEvents)
		{
			this.filteredList.Clear();
			base.Clear(triggerEvents);
		}

		// Token: 0x06000A5F RID: 2655 RVA: 0x0002C404 File Offset: 0x0002A604
		public override void SetSortOrder(SortOrders value)
		{
			bool displayAddButton = base.DisplayAddButton;
			base.DisplayAddButton = false;
			base.SetSortOrder(value);
			this.HandleFilterAndSort(displayAddButton, true);
		}

		// Token: 0x06000A60 RID: 2656 RVA: 0x0002C430 File Offset: 0x0002A630
		public override void Swap(int indexA, int indexB, bool triggerEvents)
		{
			bool displayAddButton = base.DisplayAddButton;
			base.DisplayAddButton = false;
			base.Swap(indexA, indexB, false);
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, int, int> itemSwappedAction = UIBaseListModel<T>.ItemSwappedAction;
			if (itemSwappedAction != null)
			{
				itemSwappedAction(this, indexA, indexB);
			}
			base.ItemSwappedUnityEvent.Invoke(indexA, indexB);
			this.HandleFilterAndSort(displayAddButton, triggerEvents);
		}

		// Token: 0x06000A61 RID: 2657 RVA: 0x0002C480 File Offset: 0x0002A680
		public void ReSort(bool triggerEvents)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ReSort", "triggerEvents", triggerEvents), this);
			}
			this.Sort(this.ActiveSort, triggerEvents);
		}

		// Token: 0x06000A62 RID: 2658 RVA: 0x0002C4B8 File Offset: 0x0002A6B8
		public void Sort(Comparison<T> comparison, bool triggerEvents)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[]
				{
					"Sort",
					"comparison",
					(comparison == null) ? "IS null" : "is NOT null",
					"triggerEvents",
					triggerEvents
				}), this);
			}
			if (comparison == null)
			{
				comparison = this.DefaultSort;
			}
			this.ActiveSort = comparison;
			bool flag = base.SortOrder == SortOrders.asc;
			this.filteredList.Sort(flag ? comparison : ((T x, T y) => comparison(y, x)));
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, bool> sortChangedAction = UIBaseLocalFilterableListModel<T>.SortChangedAction;
			if (sortChangedAction != null)
			{
				sortChangedAction(this, flag);
			}
			this.SortChangedUnityEvent.Invoke(flag);
			base.TriggerModelChanged();
		}

		// Token: 0x06000A63 RID: 2659 RVA: 0x0002C59C File Offset: 0x0002A79C
		public virtual void Filter(Func<T, bool> predicate, bool triggerEvents)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[]
				{
					"Filter",
					"predicate",
					(predicate == null) ? "IS null" : "is NOT null",
					"triggerEvents",
					triggerEvents
				}), this);
			}
			this.activeFilter = predicate;
			bool flag = predicate != null;
			this.filteredList.Clear();
			this.originalIndices.Clear();
			if (base.AddButtonInserted)
			{
				this.List.RemoveAt(0);
				base.AddButtonInserted = false;
			}
			for (int i = 0; i < this.List.Count; i++)
			{
				T t = this.List[i];
				if (!flag || predicate(t))
				{
					int count = this.filteredList.Count;
					this.filteredList.Add(t);
					this.originalIndices.Add(count, i);
				}
			}
			if (base.AddButtonNeedsInserting)
			{
				this.InsertAddButton(!triggerEvents);
			}
			if (!triggerEvents)
			{
				return;
			}
			base.TriggerModelChanged();
		}

		// Token: 0x06000A64 RID: 2660 RVA: 0x0002C6A7 File Offset: 0x0002A8A7
		public void ReFilter(bool triggerEvents)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ReFilter", "triggerEvents", triggerEvents), this);
			}
			this.Filter(this.activeFilter, triggerEvents);
		}

		// Token: 0x06000A65 RID: 2661 RVA: 0x0002C6E0 File Offset: 0x0002A8E0
		public void RemoveFilteredAt(int index, bool triggerEvents)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "RemoveFilteredAt", "index", index, "triggerEvents", triggerEvents }), this);
			}
			bool displayAddButton = base.DisplayAddButton;
			base.DisplayAddButton = false;
			index = this.originalIndices[index];
			base.RemoveAt(index, triggerEvents);
			this.HandleFilterAndSort(displayAddButton, triggerEvents);
		}

		// Token: 0x06000A66 RID: 2662 RVA: 0x0002C764 File Offset: 0x0002A964
		protected override void InsertAddButton(bool triggerEvents)
		{
			this.filteredList.Insert(0, default(T));
			base.InsertAddButton(triggerEvents);
		}

		// Token: 0x06000A67 RID: 2663 RVA: 0x0002C790 File Offset: 0x0002A990
		protected override void DebugList()
		{
			base.DebugList();
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}.Count: {2}", "DebugList", "filteredList", this.filteredList.Count), this);
			}
			foreach (T t in this.filteredList)
			{
				Debug.Log(t, this);
			}
		}

		// Token: 0x06000A68 RID: 2664 RVA: 0x0002C820 File Offset: 0x0002AA20
		private void HandleFilterAndSort(bool displayAddButtonCache, bool triggerEvents)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "HandleFilterAndSort", "displayAddButtonCache", displayAddButtonCache, "triggerEvents", triggerEvents }), this);
			}
			bool flag = triggerEvents && !this.SortOnChange && !base.AddButtonNeedsInserting;
			this.ReFilter(flag);
			base.DisplayAddButton = displayAddButtonCache;
			if (this.SortOnChange)
			{
				bool flag2 = triggerEvents && !base.AddButtonNeedsInserting;
				this.ReSort(flag2);
			}
			if (base.AddButtonNeedsInserting)
			{
				this.InsertAddButton(true);
			}
		}

		// Token: 0x04000683 RID: 1667
		public static readonly Action<UIBaseListModel<T>, bool> SortChangedAction;

		// Token: 0x04000685 RID: 1669
		private readonly Dictionary<int, int> originalIndices = new Dictionary<int, int>();

		// Token: 0x04000686 RID: 1670
		private readonly List<T> filteredList = new List<T>();

		// Token: 0x04000687 RID: 1671
		private Func<T, bool> activeFilter;
	}
}
