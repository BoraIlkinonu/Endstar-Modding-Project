using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Endless.Shared.Debugging;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001F4 RID: 500
	public abstract class UIBaseIEnumerableView : UIBaseView<IEnumerable, UIBaseIEnumerableView.ArrangementStyle>, IUIInteractable, IUITypeStyleOverridable
	{
		// Token: 0x1400003C RID: 60
		// (add) Token: 0x06000C92 RID: 3218 RVA: 0x00036C78 File Offset: 0x00034E78
		// (remove) Token: 0x06000C93 RID: 3219 RVA: 0x00036CB0 File Offset: 0x00034EB0
		public event Action OnEnumerableCountChanged;

		// Token: 0x1400003D RID: 61
		// (add) Token: 0x06000C94 RID: 3220 RVA: 0x00036CE8 File Offset: 0x00034EE8
		// (remove) Token: 0x06000C95 RID: 3221 RVA: 0x00036D20 File Offset: 0x00034F20
		public event Action OnItemAdded;

		// Token: 0x1400003E RID: 62
		// (add) Token: 0x06000C96 RID: 3222 RVA: 0x00036D58 File Offset: 0x00034F58
		// (remove) Token: 0x06000C97 RID: 3223 RVA: 0x00036D90 File Offset: 0x00034F90
		public event Action<object, int> OnItemModelChanged;

		// Token: 0x1400003F RID: 63
		// (add) Token: 0x06000C98 RID: 3224 RVA: 0x00036DC8 File Offset: 0x00034FC8
		// (remove) Token: 0x06000C99 RID: 3225 RVA: 0x00036E00 File Offset: 0x00035000
		public event Action<object> OnItemRemoved;

		// Token: 0x14000040 RID: 64
		// (add) Token: 0x06000C9A RID: 3226 RVA: 0x00036E38 File Offset: 0x00035038
		// (remove) Token: 0x06000C9B RID: 3227 RVA: 0x00036E70 File Offset: 0x00035070
		public event Action<object> OnItemSelected;

		// Token: 0x14000041 RID: 65
		// (add) Token: 0x06000C9C RID: 3228 RVA: 0x00036EA8 File Offset: 0x000350A8
		// (remove) Token: 0x06000C9D RID: 3229 RVA: 0x00036EE0 File Offset: 0x000350E0
		public event Action<string> OnFilterChanged;

		// Token: 0x14000042 RID: 66
		// (add) Token: 0x06000C9E RID: 3230 RVA: 0x00036F18 File Offset: 0x00035118
		// (remove) Token: 0x06000C9F RID: 3231 RVA: 0x00036F50 File Offset: 0x00035150
		public event Action<int, Direction> OnItemViewed;

		// Token: 0x1700024B RID: 587
		// (get) Token: 0x06000CA0 RID: 3232 RVA: 0x00036F85 File Offset: 0x00035185
		// (set) Token: 0x06000CA1 RID: 3233 RVA: 0x00036F8D File Offset: 0x0003518D
		public override UIBaseIEnumerableView.ArrangementStyle Style { get; protected set; }

		// Token: 0x1700024C RID: 588
		// (get) Token: 0x06000CA2 RID: 3234 RVA: 0x00036F96 File Offset: 0x00035196
		// (set) Token: 0x06000CA3 RID: 3235 RVA: 0x00036F9E File Offset: 0x0003519E
		public bool CanAddAndRemoveItems { get; private set; }

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x06000CA4 RID: 3236 RVA: 0x00036FA7 File Offset: 0x000351A7
		// (set) Token: 0x06000CA5 RID: 3237 RVA: 0x00036FAF File Offset: 0x000351AF
		public bool ChildIEnumerableCanAddAndRemoveItems { get; private set; }

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x06000CA6 RID: 3238 RVA: 0x00036FB8 File Offset: 0x000351B8
		// (set) Token: 0x06000CA7 RID: 3239 RVA: 0x00036FC0 File Offset: 0x000351C0
		public bool CanModifyItemModel { get; private set; } = true;

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x06000CA8 RID: 3240 RVA: 0x00036FC9 File Offset: 0x000351C9
		// (set) Token: 0x06000CA9 RID: 3241 RVA: 0x00036FD1 File Offset: 0x000351D1
		public bool CanSelect { get; private set; }

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x06000CAA RID: 3242 RVA: 0x00036FDA File Offset: 0x000351DA
		// (set) Token: 0x06000CAB RID: 3243 RVA: 0x00036FE2 File Offset: 0x000351E2
		public UIBaseIEnumerableView.SelectionVisualStyleEnum SelectionVisualStyle { get; private set; }

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x06000CAC RID: 3244 RVA: 0x00036FEB File Offset: 0x000351EB
		// (set) Token: 0x06000CAD RID: 3245 RVA: 0x00036FF3 File Offset: 0x000351F3
		public bool CanFilter { get; private set; }

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x06000CAE RID: 3246 RVA: 0x00036FFC File Offset: 0x000351FC
		// (set) Token: 0x06000CAF RID: 3247 RVA: 0x00037004 File Offset: 0x00035204
		public bool DisplayEnumLabels { get; private set; } = true;

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x06000CB0 RID: 3248 RVA: 0x0003700D File Offset: 0x0003520D
		// (set) Token: 0x06000CB1 RID: 3249 RVA: 0x00037015 File Offset: 0x00035215
		protected bool SuperVerboseLogging { get; set; }

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x06000CB2 RID: 3250 RVA: 0x0003701E File Offset: 0x0003521E
		public IReadOnlyList<string> ItemDisplayNames
		{
			get
			{
				return this.itemDisplayNames;
			}
		}

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x06000CB3 RID: 3251 RVA: 0x00037026 File Offset: 0x00035226
		public IReadOnlyDictionary<Type, Enum> TypeStyleOverrideDictionary
		{
			get
			{
				return this.typeStyleOverrideDictionary;
			}
		}

		// Token: 0x17000256 RID: 598
		// (get) Token: 0x06000CB4 RID: 3252 RVA: 0x00037030 File Offset: 0x00035230
		public float ContentSize
		{
			get
			{
				if (!this.Vertical)
				{
					return this.scrollRect.content.rect.width;
				}
				return this.scrollRect.content.rect.height;
			}
		}

		// Token: 0x17000257 RID: 599
		// (get) Token: 0x06000CB5 RID: 3253 RVA: 0x00037076 File Offset: 0x00035276
		// (set) Token: 0x06000CB6 RID: 3254 RVA: 0x000370A2 File Offset: 0x000352A2
		protected virtual HorizontalOrVerticalLayoutGroup HorizontalOrVerticalLayoutGroup
		{
			get
			{
				if (!this.horizontalOrVerticalLayoutGroup)
				{
					this.scrollRect.content.TryGetComponent<HorizontalOrVerticalLayoutGroup>(out this.horizontalOrVerticalLayoutGroup);
				}
				return this.horizontalOrVerticalLayoutGroup;
			}
			set
			{
				this.horizontalOrVerticalLayoutGroup = value;
			}
		}

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x06000CB7 RID: 3255 RVA: 0x000370AB File Offset: 0x000352AB
		protected bool Vertical
		{
			get
			{
				return this.scrollRect.vertical;
			}
		}

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x06000CB8 RID: 3256 RVA: 0x000370B8 File Offset: 0x000352B8
		// (set) Token: 0x06000CB9 RID: 3257 RVA: 0x000370C0 File Offset: 0x000352C0
		private protected bool IsReview { protected get; private set; }

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x06000CBA RID: 3258 RVA: 0x000370C9 File Offset: 0x000352C9
		// (set) Token: 0x06000CBB RID: 3259 RVA: 0x000370D1 File Offset: 0x000352D1
		private protected Vector2 CachedNormalizedPosition { protected get; private set; }

		// Token: 0x06000CBC RID: 3260 RVA: 0x000370DC File Offset: 0x000352DC
		public override void Validate()
		{
			base.Validate();
			foreach (UIRectTransformDictionary uirectTransformDictionary in this.rectTransformDictionaries)
			{
				foreach (string text in UIBaseIEnumerableView.rectTransformDictionaryKeys)
				{
					if (!uirectTransformDictionary.Contains(text))
					{
						DebugUtility.LogException(new KeyNotFoundException("The rectTransformDictionary '" + uirectTransformDictionary.gameObject.name + "' does not contain a key with the name: " + text), uirectTransformDictionary);
					}
				}
			}
		}

		// Token: 0x06000CBD RID: 3261 RVA: 0x00037158 File Offset: 0x00035358
		protected virtual void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnEnable", this);
			}
			this.scrollRect.onValueChanged.AddListener(new UnityAction<Vector2>(this.OnScroll));
			this.addItemButton.onClick.AddListener(new UnityAction(this.InvokeOnItemAdded));
			this.filterInputField.onValueChanged.AddListener(new UnityAction<string>(this.InvokeOnFilterChanged));
		}

		// Token: 0x06000CBE RID: 3262 RVA: 0x000371CD File Offset: 0x000353CD
		protected virtual void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.SetCanAddAndRemoveItems(this.CanAddAndRemoveItems);
			this.SetCanFilter(this.CanFilter);
		}

		// Token: 0x06000CBF RID: 3263 RVA: 0x000371FC File Offset: 0x000353FC
		protected virtual void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDisable", this);
			}
			this.scrollRect.onValueChanged.RemoveListener(new UnityAction<Vector2>(this.OnScroll));
			this.addItemButton.onClick.RemoveListener(new UnityAction(this.InvokeOnItemAdded));
			this.filterInputField.onValueChanged.RemoveListener(new UnityAction<string>(this.InvokeOnFilterChanged));
		}

		// Token: 0x06000CC0 RID: 3264
		public abstract void ReviewSelectedStatus();

		// Token: 0x06000CC1 RID: 3265 RVA: 0x00037274 File Offset: 0x00035474
		public override float GetPreferredHeight(object model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetPreferredHeight", new object[] { model });
			}
			bool flag = base.gameObject.scene.name != null;
			IEnumerable<object> enumerable;
			if (flag)
			{
				int num = this.GetCount();
				enumerable = this.<GetPreferredHeight>g__EnumerateRuntimeItems|105_0(num);
			}
			else
			{
				List<object> list = model as List<object>;
				IEnumerable<object> enumerable3;
				if (list == null)
				{
					object[] array = model as object[];
					if (array == null)
					{
						IEnumerable enumerable2 = model as IEnumerable;
						if (enumerable2 == null)
						{
							throw new InvalidCastException(string.Format("Could not cast {0} (type: {1}) to any {2} type!", "model", model.GetType(), "IEnumerable"));
						}
						enumerable3 = enumerable2.Cast<object>().ToList<object>();
					}
					else
					{
						enumerable3 = array;
					}
				}
				else
				{
					enumerable3 = list;
				}
				enumerable = enumerable3;
			}
			float num2 = this.<GetPreferredHeight>g__CalculateTotalHeight|105_1(enumerable);
			float num3 = base.LayoutElement.minHeight + num2;
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}\n{2}: {3}\n{4}: {5}\n{6}: {7}", new object[]
				{
					"isInScene",
					flag,
					"items",
					enumerable.Count<object>(),
					"totalHeight",
					num2,
					"preferredHeight",
					num3
				}), this);
			}
			return num3;
		}

		// Token: 0x06000CC2 RID: 3266 RVA: 0x000373B4 File Offset: 0x000355B4
		public override void View(IEnumerable model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
			}
			this.IsReview = this.isViewing;
			this.CachedNormalizedPosition = this.scrollRect.normalizedPosition;
			this.ResetViewState();
			this.isViewing = true;
			bool flag = this.GetCount() == 0;
			this.emptyFlair.gameObject.SetActive(flag);
		}

		// Token: 0x06000CC3 RID: 3267 RVA: 0x00037430 File Offset: 0x00035630
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			this.ResetViewState();
			this.DisplayEnumLabels = true;
			this.typeStyleOverrideDictionary.Clear();
			this.filterInputField.Clear(false);
			this.hiddenItems.Clear();
		}

		// Token: 0x06000CC4 RID: 3268 RVA: 0x0003747F File Offset: 0x0003567F
		public virtual void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
			}
			this.CanModifyItemModel = interactable;
		}

		// Token: 0x06000CC5 RID: 3269 RVA: 0x000374B0 File Offset: 0x000356B0
		public void SetCanSelect(bool canSelect)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetCanSelect", "canSelect", canSelect), this);
			}
			this.CanSelect = canSelect;
		}

		// Token: 0x06000CC6 RID: 3270 RVA: 0x000374E4 File Offset: 0x000356E4
		public void SetCanFilter(bool canFilter)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetCanFilter", "canFilter", canFilter), this);
			}
			this.CanFilter = canFilter;
			this.filterInputField.gameObject.SetActive(canFilter);
			this.HandleFilterAndAddButtonLayout();
		}

		// Token: 0x06000CC7 RID: 3271 RVA: 0x00037538 File Offset: 0x00035738
		public void SetCanAddAndRemoveItems(bool canAddAndRemoveItems)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetCanAddAndRemoveItems", "canAddAndRemoveItems", canAddAndRemoveItems), this);
			}
			this.CanAddAndRemoveItems = canAddAndRemoveItems;
			this.addItemButton.gameObject.SetActive(canAddAndRemoveItems);
			Vector2 offsetMin = this.emptyFlair.offsetMin;
			offsetMin.y = (canAddAndRemoveItems ? this.addItemButton.RectTransform.rect.height : 0f);
			this.emptyFlair.offsetMin = offsetMin;
			this.HandleFilterAndAddButtonLayout();
		}

		// Token: 0x06000CC8 RID: 3272 RVA: 0x000375CC File Offset: 0x000357CC
		public virtual void SetChildIEnumerableCanAddAndRemoveItems(bool canAddAndRemoveItems)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetChildIEnumerableCanAddAndRemoveItems", "canAddAndRemoveItems", canAddAndRemoveItems), this);
			}
			this.ChildIEnumerableCanAddAndRemoveItems = canAddAndRemoveItems;
		}

		// Token: 0x06000CC9 RID: 3273 RVA: 0x000375FD File Offset: 0x000357FD
		public void SetTypeStyleOverrideDictionary(Dictionary<Type, Enum> value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetTypeStyleOverrideDictionary", "value", value.Count), this);
			}
			this.typeStyleOverrideDictionary = value;
		}

		// Token: 0x06000CCA RID: 3274 RVA: 0x00037633 File Offset: 0x00035833
		public void SetItemDisplayNames(List<string> value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetItemDisplayNames", "value", value.Count), this);
			}
			this.itemDisplayNames = value;
		}

		// Token: 0x06000CCB RID: 3275 RVA: 0x00037669 File Offset: 0x00035869
		public void SetSelectionVisualStyle(UIBaseIEnumerableView.SelectionVisualStyleEnum selectionVisualStyle)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetSelectionVisualStyle", "selectionVisualStyle", selectionVisualStyle), this);
			}
			this.SelectionVisualStyle = selectionVisualStyle;
		}

		// Token: 0x06000CCC RID: 3276 RVA: 0x0003769A File Offset: 0x0003589A
		public void SetDisplayEnumLabels(bool displayEnumLabels)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetDisplayEnumLabels", "displayEnumLabels", displayEnumLabels), this);
			}
			this.DisplayEnumLabels = displayEnumLabels;
		}

		// Token: 0x06000CCD RID: 3277 RVA: 0x000376CB File Offset: 0x000358CB
		public void HideItem(object item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "HideItem", "item", item), this);
			}
			this.hiddenItems.Add(item);
		}

		// Token: 0x06000CCE RID: 3278
		protected abstract void OnScroll(Vector2 scrollRectValue);

		// Token: 0x06000CCF RID: 3279 RVA: 0x00037700 File Offset: 0x00035900
		protected virtual UIIEnumerableItem SpawnItem(object model, int dataIndex, int viewIndex, RectTransform parent)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8} )", new object[]
				{
					"SpawnItem",
					"model",
					model,
					"dataIndex",
					dataIndex,
					"viewIndex",
					viewIndex,
					"parent",
					parent.DebugSafeName(true)
				}), this);
			}
			UIIEnumerableItem uiienumerableItem = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<UIIEnumerableItem>(this.iEnumerableItemSource, default(Vector3), default(Quaternion), parent);
			this.SetUpItem(uiienumerableItem, model, dataIndex, viewIndex);
			return uiienumerableItem;
		}

		// Token: 0x06000CD0 RID: 3280 RVA: 0x000377A8 File Offset: 0x000359A8
		protected virtual void SetUpItem(UIIEnumerableItem iEnumerableItem, object model, int dataIndex, int viewIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8} )", new object[]
				{
					"SetUpItem",
					"iEnumerableItem",
					iEnumerableItem.DebugSafeName(true),
					"model",
					model,
					"dataIndex",
					dataIndex,
					"viewIndex",
					viewIndex
				}), this);
			}
			if (!object.Equals(iEnumerableItem.ModelAsObject, model) || iEnumerableItem.DataIndex != dataIndex || iEnumerableItem.ViewIndex != viewIndex)
			{
				iEnumerableItem.View(model, this, dataIndex, viewIndex);
			}
			float itemSize = this.GetItemSize(dataIndex);
			if (this.Vertical)
			{
				iEnumerableItem.SetHeight(itemSize);
			}
			else
			{
				iEnumerableItem.SetWidth(itemSize);
			}
			if (!this.setUpItems.Add(iEnumerableItem))
			{
				return;
			}
			iEnumerableItem.OnModelChanged += this.InvokeOnItemModelChanged;
			iEnumerableItem.OnSelectButtonPressed += this.InvokeOnItemSelected;
			iEnumerableItem.OnRemoveButtonPressed += this.InvokeOnItemRemoved;
			iEnumerableItem.OnEnumerableCountChanged += this.InvokeOnEnumerableCountChanged;
		}

		// Token: 0x06000CD1 RID: 3281 RVA: 0x000378C0 File Offset: 0x00035AC0
		protected virtual void DespawnItem(UIIEnumerableItem iEnumerableItem)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "DespawnItem", "iEnumerableItem", iEnumerableItem.DataIndex), this);
			}
			if (this.setUpItems.Contains(iEnumerableItem))
			{
				this.setUpItems.Remove(iEnumerableItem);
				iEnumerableItem.OnModelChanged -= this.InvokeOnItemModelChanged;
				iEnumerableItem.OnSelectButtonPressed -= this.InvokeOnItemSelected;
				iEnumerableItem.OnRemoveButtonPressed -= this.InvokeOnItemRemoved;
				iEnumerableItem.OnEnumerableCountChanged -= this.InvokeOnEnumerableCountChanged;
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIIEnumerableItem>(iEnumerableItem);
		}

		// Token: 0x06000CD2 RID: 3282 RVA: 0x00037968 File Offset: 0x00035B68
		protected float GetItemSize(int dataIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetItemSize", "dataIndex", dataIndex), this);
			}
			object obj = this.GetItem(dataIndex);
			if (this.hiddenItems.Contains(obj))
			{
				return 0f;
			}
			LayoutReferenceType layoutReferenceType = (this.Vertical ? LayoutReferenceType.Height : LayoutReferenceType.Width);
			float size = this.presenterDictionary.GetSize(obj, layoutReferenceType, this.typeStyleOverrideDictionary);
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("type: {0}, {1}: {2}", obj.GetType().Name, "itemSize", size), this);
			}
			return size;
		}

		// Token: 0x06000CD3 RID: 3283 RVA: 0x00037A0E File Offset: 0x00035C0E
		protected virtual void ResetViewState()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ResetViewState", this);
			}
			this.scrollRect.ResetPosition();
			this.itemDisplayNames.Clear();
			this.isViewing = false;
		}

		// Token: 0x06000CD4 RID: 3284 RVA: 0x00037A40 File Offset: 0x00035C40
		protected void ProcessItemVisibility(List<UIIEnumerableItem> items, Direction direction)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "ProcessItemVisibility", "items", items.Count, "direction", direction }), this);
			}
			foreach (UIIEnumerableItem uiienumerableItem in items)
			{
				if (this.scrollRect.viewport.AreRectTransformsIntersecting(uiienumerableItem.RectTransform))
				{
					Action<int, Direction> onItemViewed = this.OnItemViewed;
					if (onItemViewed != null)
					{
						onItemViewed(uiienumerableItem.DataIndex, direction);
					}
				}
			}
		}

		// Token: 0x06000CD5 RID: 3285 RVA: 0x00037B04 File Offset: 0x00035D04
		private void HandleFilterAndAddButtonLayout()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("HandleFilterAndAddButtonLayout", this);
			}
			string text = "Can Not Filter Can Not Add";
			if (this.CanFilter && this.CanAddAndRemoveItems)
			{
				text = "Can Filter Can Add";
			}
			else if (!this.CanFilter && this.CanAddAndRemoveItems)
			{
				text = "Can Not Filter Can Add";
			}
			else if (this.CanFilter && !this.CanAddAndRemoveItems)
			{
				text = "Can Filter Can Not Add";
			}
			UIRectTransformDictionary[] array = this.rectTransformDictionaries;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Apply(text);
			}
		}

		// Token: 0x06000CD6 RID: 3286 RVA: 0x00037B90 File Offset: 0x00035D90
		private void InvokeOnItemModelChanged(object model, int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "InvokeOnItemModelChanged", "model", model, "index", index }), this);
			}
			Action<object, int> onItemModelChanged = this.OnItemModelChanged;
			if (onItemModelChanged == null)
			{
				return;
			}
			onItemModelChanged(model, index);
		}

		// Token: 0x06000CD7 RID: 3287 RVA: 0x00037BF2 File Offset: 0x00035DF2
		private void InvokeOnItemSelected(object item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeOnItemSelected", "item", item), this);
			}
			Action<object> onItemSelected = this.OnItemSelected;
			if (onItemSelected == null)
			{
				return;
			}
			onItemSelected(item);
		}

		// Token: 0x06000CD8 RID: 3288 RVA: 0x00037C28 File Offset: 0x00035E28
		private void InvokeOnItemRemoved(object item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeOnItemRemoved", "item", item), this);
			}
			Action<object> onItemRemoved = this.OnItemRemoved;
			if (onItemRemoved == null)
			{
				return;
			}
			onItemRemoved(item);
		}

		// Token: 0x06000CD9 RID: 3289 RVA: 0x00037C5E File Offset: 0x00035E5E
		private void InvokeOnEnumerableCountChanged()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("InvokeOnEnumerableCountChanged", this);
			}
			Action onEnumerableCountChanged = this.OnEnumerableCountChanged;
			if (onEnumerableCountChanged == null)
			{
				return;
			}
			onEnumerableCountChanged();
		}

		// Token: 0x06000CDA RID: 3290 RVA: 0x00037C83 File Offset: 0x00035E83
		private void InvokeOnItemAdded()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("InvokeOnItemAdded", this);
			}
			Action onItemAdded = this.OnItemAdded;
			if (onItemAdded == null)
			{
				return;
			}
			onItemAdded();
		}

		// Token: 0x06000CDB RID: 3291 RVA: 0x00037CA8 File Offset: 0x00035EA8
		private void InvokeOnFilterChanged(string filter)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("InvokeOnFilterChanged ( filter: " + filter + " )", this);
			}
			Action<string> onFilterChanged = this.OnFilterChanged;
			if (onFilterChanged == null)
			{
				return;
			}
			onFilterChanged(filter);
		}

		// Token: 0x06000CDE RID: 3294 RVA: 0x00037D61 File Offset: 0x00035F61
		[CompilerGenerated]
		private IEnumerable<object> <GetPreferredHeight>g__EnumerateRuntimeItems|105_0(int count)
		{
			int num;
			for (int i = 0; i < count; i = num + 1)
			{
				yield return this.GetItem(i);
				num = i;
			}
			yield break;
		}

		// Token: 0x06000CDF RID: 3295 RVA: 0x00037D78 File Offset: 0x00035F78
		[CompilerGenerated]
		private float <GetPreferredHeight>g__CalculateTotalHeight|105_1(IEnumerable<object> target)
		{
			float num = 0f;
			bool flag = true;
			int num2 = 0;
			foreach (object obj in target)
			{
				if (!flag)
				{
					num += this.HorizontalOrVerticalLayoutGroup.spacing;
				}
				else
				{
					flag = false;
				}
				Type type = obj.GetType();
				IUIViewable viewable = this.presenterDictionary.GetPresenterWithDefaultStyle(type).Viewable;
				num += viewable.GetPreferredHeight(obj);
				num2++;
			}
			if (num2 > 0)
			{
				num += (float)(this.Vertical ? this.HorizontalOrVerticalLayoutGroup.padding.vertical : this.HorizontalOrVerticalLayoutGroup.padding.horizontal);
			}
			return num;
		}

		// Token: 0x04000815 RID: 2069
		private const string CAN_FILTER_CAN_ADD_KEY = "Can Filter Can Add";

		// Token: 0x04000816 RID: 2070
		private const string CAN_NOT_FILTER_CAN_ADD_KEY = "Can Not Filter Can Add";

		// Token: 0x04000817 RID: 2071
		private const string CAN_FILTER_CANNOT_ADD_KEY = "Can Filter Can Not Add";

		// Token: 0x04000818 RID: 2072
		private const string CAN_NOT_FILTER_CAN_NOT_ADD_KEY = "Can Not Filter Can Not Add";

		// Token: 0x04000819 RID: 2073
		private static readonly string[] rectTransformDictionaryKeys = new string[] { "Can Filter Can Add", "Can Not Filter Can Add", "Can Filter Can Not Add", "Can Not Filter Can Not Add" };

		// Token: 0x04000821 RID: 2081
		public Func<int> GetCount;

		// Token: 0x04000822 RID: 2082
		public Func<int, object> GetItem;

		// Token: 0x04000823 RID: 2083
		public Func<object, bool> GetIsSelected;

		// Token: 0x04000824 RID: 2084
		public Func<object, int> GetSelectionOrder;

		// Token: 0x04000825 RID: 2085
		[Header("UIBaseIEnumerableView")]
		[SerializeField]
		protected UIScrollRect scrollRect;

		// Token: 0x04000826 RID: 2086
		[SerializeField]
		protected UIIEnumerableItem iEnumerableItemSource;

		// Token: 0x04000827 RID: 2087
		[Header("References")]
		[SerializeField]
		private UIRectTransformDictionary[] rectTransformDictionaries = Array.Empty<UIRectTransformDictionary>();

		// Token: 0x04000828 RID: 2088
		[SerializeField]
		private RectTransform emptyFlair;

		// Token: 0x04000829 RID: 2089
		[SerializeField]
		private UIInputField filterInputField;

		// Token: 0x0400082A RID: 2090
		[SerializeField]
		private UIButton addItemButton;

		// Token: 0x0400082B RID: 2091
		[SerializeField]
		private UIPresenterDictionary presenterDictionary;

		// Token: 0x0400082C RID: 2092
		private readonly HashSet<UIIEnumerableItem> setUpItems = new HashSet<UIIEnumerableItem>();

		// Token: 0x0400082D RID: 2093
		private HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup;

		// Token: 0x0400082E RID: 2094
		private bool isViewing;

		// Token: 0x0400082F RID: 2095
		private List<string> itemDisplayNames = new List<string>();

		// Token: 0x04000830 RID: 2096
		private readonly HashSet<object> hiddenItems = new HashSet<object>();

		// Token: 0x04000831 RID: 2097
		private Dictionary<Type, Enum> typeStyleOverrideDictionary = new Dictionary<Type, Enum>();

		// Token: 0x020001F5 RID: 501
		public enum SelectionVisualStyleEnum
		{
			// Token: 0x0400083E RID: 2110
			Check,
			// Token: 0x0400083F RID: 2111
			Order,
			// Token: 0x04000840 RID: 2112
			Outline
		}

		// Token: 0x020001F6 RID: 502
		public enum ArrangementStyle
		{
			// Token: 0x04000842 RID: 2114
			StraightVertical,
			// Token: 0x04000843 RID: 2115
			StraightVerticalVirtualized,
			// Token: 0x04000844 RID: 2116
			StraightHorizontal,
			// Token: 0x04000845 RID: 2117
			StraightHorizontalVirtualized,
			// Token: 0x04000846 RID: 2118
			GridVertical,
			// Token: 0x04000847 RID: 2119
			GridVerticalVirtualized,
			// Token: 0x04000848 RID: 2120
			GridHorizontal,
			// Token: 0x04000849 RID: 2121
			GridHorizontalVirtualized
		}
	}
}
