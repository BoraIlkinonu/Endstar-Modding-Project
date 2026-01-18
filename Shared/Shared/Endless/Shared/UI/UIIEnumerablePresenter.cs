using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001F2 RID: 498
	public class UIIEnumerablePresenter : UIBasePresenter<IEnumerable>
	{
		// Token: 0x1400003B RID: 59
		// (add) Token: 0x06000C6A RID: 3178 RVA: 0x00035CF8 File Offset: 0x00033EF8
		// (remove) Token: 0x06000C6B RID: 3179 RVA: 0x00035D30 File Offset: 0x00033F30
		public event Action<IReadOnlyList<object>> OnSelectionChanged;

		// Token: 0x06000C6C RID: 3180 RVA: 0x00035D65 File Offset: 0x00033F65
		static UIIEnumerablePresenter()
		{
			UIIEnumerablePresenterFiltersShared.Register();
		}

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x06000C6D RID: 3181 RVA: 0x00035D76 File Offset: 0x00033F76
		public IReadOnlyList<object> ModelList
		{
			get
			{
				return this.modelList;
			}
		}

		// Token: 0x17000247 RID: 583
		// (get) Token: 0x06000C6E RID: 3182 RVA: 0x00035D7E File Offset: 0x00033F7E
		public int Count
		{
			get
			{
				return this.filteredList.Count;
			}
		}

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x06000C6F RID: 3183 RVA: 0x00035D8B File Offset: 0x00033F8B
		public UIBaseIEnumerableView IEnumerableView
		{
			get
			{
				if (!this.iEnumerableView)
				{
					this.iEnumerableView = base.View.Interface as UIBaseIEnumerableView;
				}
				return this.iEnumerableView;
			}
		}

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x06000C70 RID: 3184 RVA: 0x00035DB6 File Offset: 0x00033FB6
		public IReadOnlyList<object> SelectedItemsList
		{
			get
			{
				return this.selectedItemsList;
			}
		}

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x06000C71 RID: 3185 RVA: 0x000043C6 File Offset: 0x000025C6
		protected override bool ViewOnModelChanged
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000C72 RID: 3186 RVA: 0x00035DBE File Offset: 0x00033FBE
		protected override void Start()
		{
			base.Start();
			if (!this.subscribedToViewEvents)
			{
				this.SubscribeToViewEvents();
			}
		}

		// Token: 0x06000C73 RID: 3187 RVA: 0x00035DD4 File Offset: 0x00033FD4
		public static void RegisterTypeFilter(Type type, Func<string, object, bool> filterPredicate)
		{
			if (!UIIEnumerablePresenter.typeFilterDictionary.TryAdd(type, filterPredicate))
			{
				DebugUtility.LogError(type.Name + " already has a filter function registered!", null);
			}
		}

		// Token: 0x06000C74 RID: 3188 RVA: 0x00035DFC File Offset: 0x00033FFC
		public override void SetModel(IEnumerable model, bool triggerOnModelChanged)
		{
			if (!this.subscribedToViewEvents)
			{
				this.SubscribeToViewEvents();
			}
			this.modelList.Clear();
			this.modelList.AddRange(model.Cast<object>());
			base.SetModel(this.modelList, triggerOnModelChanged);
			if (base.VerboseLogging)
			{
				DebugUtility.DebugEnumerable<object>("modelList", this.modelList, this);
			}
			this.SetFilter(this.filter);
		}

		// Token: 0x06000C75 RID: 3189 RVA: 0x00035E68 File Offset: 0x00034068
		public override void Clear()
		{
			base.Clear();
			this.modelList.Clear();
			this.filteredList.Clear();
			this.selectedItemsList.Clear();
			this.selectedItemsHashSet.Clear();
			this.filter = string.Empty;
			Action<IReadOnlyList<object>> onSelectionChanged = this.OnSelectionChanged;
			if (onSelectionChanged == null)
			{
				return;
			}
			onSelectionChanged(this.SelectedItemsList);
		}

		// Token: 0x06000C76 RID: 3190 RVA: 0x00035EC8 File Offset: 0x000340C8
		public static void SetDynamicTypeFactory(IUIDynamicTypeFactory newValue)
		{
			UIIEnumerablePresenter.dynamicTypeFactory = newValue;
		}

		// Token: 0x06000C77 RID: 3191 RVA: 0x00035ED0 File Offset: 0x000340D0
		public void SetElementType(Type elementType)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetElementType", new object[] { elementType });
			}
			this.elementType = elementType;
		}

		// Token: 0x06000C78 RID: 3192 RVA: 0x00035EF8 File Offset: 0x000340F8
		public void SetSelectionType(SelectionType mode, bool updateSelectionVisuals)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetSelectionType", new object[] { mode, updateSelectionVisuals });
			}
			this.selectionType = mode;
			if (updateSelectionVisuals)
			{
				this.IEnumerableView.ReviewSelectedStatus();
			}
		}

		// Token: 0x06000C79 RID: 3193 RVA: 0x00035F48 File Offset: 0x00034148
		public void SetSelected(List<object> selectedItems, bool updateSelectionVisuals, bool invokeOnModelChangedAndOnSelectionChanged)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetSelected", new object[] { selectedItems.Count, updateSelectionVisuals, invokeOnModelChangedAndOnSelectionChanged });
			}
			this.selectedItemsList = selectedItems;
			this.selectedItemsHashSet = new HashSet<object>(selectedItems);
			if (updateSelectionVisuals)
			{
				this.IEnumerableView.ReviewSelectedStatus();
			}
			if (!invokeOnModelChangedAndOnSelectionChanged)
			{
				return;
			}
			base.InvokeOnModelChanged();
			Action<IReadOnlyList<object>> onSelectionChanged = this.OnSelectionChanged;
			if (onSelectionChanged == null)
			{
				return;
			}
			onSelectionChanged(this.SelectedItemsList);
		}

		// Token: 0x06000C7A RID: 3194 RVA: 0x00035FCF File Offset: 0x000341CF
		public bool GetIsSelected(object item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetIsSelected", new object[] { item });
			}
			return this.selectedItemsHashSet.Contains(item);
		}

		// Token: 0x06000C7B RID: 3195 RVA: 0x00035FFA File Offset: 0x000341FA
		public int GetSelectionOrder(object item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetSelectionOrder", new object[] { item });
			}
			if (!this.GetIsSelected(item))
			{
				return -1;
			}
			return this.selectedItemsList.IndexOf(item);
		}

		// Token: 0x06000C7C RID: 3196 RVA: 0x00036030 File Offset: 0x00034230
		public int GetItemIndex(object item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetItemIndex", new object[] { item });
			}
			return this.modelList.IndexOf(item);
		}

		// Token: 0x06000C7D RID: 3197 RVA: 0x0003605C File Offset: 0x0003425C
		private void SubscribeToViewEvents()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SubscribeToViewEvents", Array.Empty<object>());
			}
			if (this.subscribedToViewEvents)
			{
				return;
			}
			this.subscribedToViewEvents = true;
			UIBaseIEnumerableView ienumerableView = this.IEnumerableView;
			ienumerableView.GetItem = (Func<int, object>)Delegate.Combine(ienumerableView.GetItem, new Func<int, object>(this.GetItem));
			UIBaseIEnumerableView ienumerableView2 = this.IEnumerableView;
			ienumerableView2.GetCount = (Func<int>)Delegate.Combine(ienumerableView2.GetCount, new Func<int>(this.GetCount));
			UIBaseIEnumerableView ienumerableView3 = this.IEnumerableView;
			ienumerableView3.GetIsSelected = (Func<object, bool>)Delegate.Combine(ienumerableView3.GetIsSelected, new Func<object, bool>(this.GetIsSelected));
			UIBaseIEnumerableView ienumerableView4 = this.IEnumerableView;
			ienumerableView4.GetSelectionOrder = (Func<object, int>)Delegate.Combine(ienumerableView4.GetSelectionOrder, new Func<object, int>(this.GetSelectionOrder));
			this.IEnumerableView.OnItemModelChanged += this.OnItemModelChanged;
			this.IEnumerableView.OnItemAdded += this.AddNewItem;
			this.IEnumerableView.OnItemRemoved += this.RemoveItem;
			this.IEnumerableView.OnItemSelected += this.OnItemSelected;
			this.IEnumerableView.OnFilterChanged += this.SetFilter;
		}

		// Token: 0x06000C7E RID: 3198 RVA: 0x000361A0 File Offset: 0x000343A0
		private void SetFilter(string filter)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetFilter", new object[] { filter });
			}
			this.filter = filter;
			this.filteredList.Clear();
			if (filter.IsNullOrEmptyOrWhiteSpace())
			{
				this.filteredList = new List<object>(this.modelList);
			}
			else
			{
				foreach (object obj in this.modelList)
				{
					Type type = obj.GetType();
					Func<string, object, bool> func;
					bool flag;
					if (UIIEnumerablePresenter.typeFilterDictionary.TryGetValue(type, out func))
					{
						flag = func(filter, obj);
					}
					else
					{
						DebugUtility.LogWarning("No filter function was provided for a type of " + type.Name + "! Defaulting to ToString...", this);
						string text = obj.ToString();
						flag = text != null && text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
					}
					if (flag)
					{
						this.filteredList.Add(obj);
					}
				}
			}
			base.View.Interface.View(base.Model);
		}

		// Token: 0x06000C7F RID: 3199 RVA: 0x000362B8 File Offset: 0x000344B8
		private object GetItem(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetItem", new object[] { index });
			}
			if (index < 0 || index >= this.filteredList.Count)
			{
				DebugUtility.LogException(new ArgumentOutOfRangeException("index", string.Format("{0}.Count: {1}", "filteredList", this.filteredList.Count)), null);
				return null;
			}
			return this.filteredList[index];
		}

		// Token: 0x06000C80 RID: 3200 RVA: 0x00035D7E File Offset: 0x00033F7E
		private int GetCount()
		{
			return this.filteredList.Count;
		}

		// Token: 0x06000C81 RID: 3201 RVA: 0x00036338 File Offset: 0x00034538
		private void RemoveItem(object item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveItem", new object[] { item });
			}
			this.modelList.Remove(item);
			if (this.selectedItemsHashSet.Contains(item))
			{
				this.selectedItemsHashSet.Remove(item);
				this.selectedItemsList.Remove(item);
			}
			base.InvokeOnModelChanged();
			this.SetFilter(this.filter);
			Action<IReadOnlyList<object>> onSelectionChanged = this.OnSelectionChanged;
			if (onSelectionChanged == null)
			{
				return;
			}
			onSelectionChanged(this.SelectedItemsList);
		}

		// Token: 0x06000C82 RID: 3202 RVA: 0x000363C0 File Offset: 0x000345C0
		private void AddNewItem()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "AddNewItem", Array.Empty<object>());
			}
			if (this.elementType == null)
			{
				throw new InvalidOperationException("elementType must be set before invoking AddNewItem.");
			}
			if (base.Model == null)
			{
				throw new NullReferenceException("Model is null!");
			}
			if (UIIEnumerablePresenter.dynamicTypeFactory == null)
			{
				throw new InvalidOperationException("dynamicTypeFactory is null. Call SetDynamicTypeFactory before AddNewItem.");
			}
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Model Type: " + base.Model.GetType().Name, this);
				DebugUtility.Log(string.Format("{0}: {1}", "elementType", this.elementType), this);
			}
			object obj = UIIEnumerablePresenter.dynamicTypeFactory.Create(this.elementType);
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "newItem", obj), this);
			}
			IEnumerable model = base.Model;
			if (!(model is Array) && !(model is List<object>))
			{
				throw new NotSupportedException("Model's type '" + base.Model.GetType().Name + "' is not supported in the AddNewItem method.");
			}
			if (base.Model.GetType().IsArray)
			{
				Array array = base.Model as Array;
				if (array == null)
				{
					throw new NotSupportedException("Model is an array type but can't be cast to Array?");
				}
				int length = array.Length;
				Array array2 = Array.CreateInstance(this.elementType, length + 1);
				Array.Copy(array, array2, length);
				array2.SetValue(obj, length);
				this.SetModel(array2, true);
			}
			else
			{
				IList list = base.Model as IList;
				if (list == null)
				{
					throw new NotSupportedException("Model's type '" + base.Model.GetType().Name + "' is not supported by AddNewItem.");
				}
				list.Add(obj);
				this.SetModel(new List<object>(list.Cast<object>()), true);
			}
			base.InvokeOnModelChanged();
			this.SetFilter(this.filter);
		}

		// Token: 0x06000C83 RID: 3203 RVA: 0x00036594 File Offset: 0x00034794
		private void OnItemSelected(object item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnItemSelected", new object[] { item });
			}
			switch (this.selectionType)
			{
			case SelectionType.Select0To1:
			{
				bool flag = !this.selectedItemsHashSet.Contains(item);
				this.selectedItemsList.Clear();
				this.selectedItemsHashSet.Clear();
				if (flag)
				{
					this.selectedItemsList.Add(item);
					this.selectedItemsHashSet.Add(item);
				}
				break;
			}
			case SelectionType.Select0OrMore:
				if (this.selectedItemsHashSet.Contains(item))
				{
					this.selectedItemsList.Remove(item);
					this.selectedItemsHashSet.Remove(item);
				}
				else
				{
					this.selectedItemsList.Add(item);
					this.selectedItemsHashSet.Add(item);
				}
				break;
			case SelectionType.MustSelect1:
				if (this.selectedItemsList.Contains(item))
				{
					return;
				}
				this.selectedItemsList.Clear();
				this.selectedItemsHashSet.Clear();
				this.selectedItemsList.Add(item);
				this.selectedItemsHashSet.Add(item);
				if (this.selectedItemsHashSet.Count == 0 && this.modelList.Count > 0)
				{
					this.selectedItemsList.Add(this.modelList[0]);
					this.selectedItemsHashSet.Add(this.modelList[0]);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.IEnumerableView.ReviewSelectedStatus();
			base.InvokeOnModelChanged();
			Action<IReadOnlyList<object>> onSelectionChanged = this.OnSelectionChanged;
			if (onSelectionChanged == null)
			{
				return;
			}
			onSelectionChanged(this.SelectedItemsList);
		}

		// Token: 0x06000C84 RID: 3204 RVA: 0x00036728 File Offset: 0x00034928
		private void OnItemModelChanged(object itemModel, int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnItemModelChanged", new object[] { itemModel, index });
			}
			this.modelList[index] = itemModel;
			base.InvokeOnModelChanged();
			if (!this.filter.IsNullOrEmptyOrWhiteSpace())
			{
				this.SetFilter(this.filter);
			}
		}

		// Token: 0x04000808 RID: 2056
		private static readonly Dictionary<Type, Func<string, object, bool>> typeFilterDictionary = new Dictionary<Type, Func<string, object, bool>>();

		// Token: 0x04000809 RID: 2057
		private static IUIDynamicTypeFactory dynamicTypeFactory;

		// Token: 0x0400080A RID: 2058
		[Header("UIIEnumerablePresenter")]
		[SerializeField]
		private SelectionType selectionType;

		// Token: 0x0400080B RID: 2059
		private readonly List<object> modelList = new List<object>();

		// Token: 0x0400080C RID: 2060
		private Type elementType;

		// Token: 0x0400080D RID: 2061
		private string filter = string.Empty;

		// Token: 0x0400080E RID: 2062
		private List<object> filteredList = new List<object>();

		// Token: 0x0400080F RID: 2063
		private UIBaseIEnumerableView iEnumerableView;

		// Token: 0x04000810 RID: 2064
		private HashSet<object> selectedItemsHashSet = new HashSet<object>();

		// Token: 0x04000811 RID: 2065
		private List<object> selectedItemsList = new List<object>();

		// Token: 0x04000812 RID: 2066
		private bool subscribedToViewEvents;
	}
}
