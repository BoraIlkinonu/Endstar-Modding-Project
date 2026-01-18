using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Runtime.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001F0 RID: 496
	public class UIIEnumerableItem : UIGameObject, IPoolableT, IUIInteractable
	{
		// Token: 0x14000036 RID: 54
		// (add) Token: 0x06000C3C RID: 3132 RVA: 0x00035194 File Offset: 0x00033394
		// (remove) Token: 0x06000C3D RID: 3133 RVA: 0x000351CC File Offset: 0x000333CC
		public event Action OnEnumerableCountChanged;

		// Token: 0x14000037 RID: 55
		// (add) Token: 0x06000C3E RID: 3134 RVA: 0x00035204 File Offset: 0x00033404
		// (remove) Token: 0x06000C3F RID: 3135 RVA: 0x0003523C File Offset: 0x0003343C
		public event Action<object, int> OnModelChanged;

		// Token: 0x14000038 RID: 56
		// (add) Token: 0x06000C40 RID: 3136 RVA: 0x00035274 File Offset: 0x00033474
		// (remove) Token: 0x06000C41 RID: 3137 RVA: 0x000352AC File Offset: 0x000334AC
		public event Action<object> OnRemoveButtonPressed;

		// Token: 0x14000039 RID: 57
		// (add) Token: 0x06000C42 RID: 3138 RVA: 0x000352E4 File Offset: 0x000334E4
		// (remove) Token: 0x06000C43 RID: 3139 RVA: 0x0003531C File Offset: 0x0003351C
		public event Action<object> OnSelectButtonPressed;

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x06000C44 RID: 3140 RVA: 0x00035351 File Offset: 0x00033551
		// (set) Token: 0x06000C45 RID: 3141 RVA: 0x00035359 File Offset: 0x00033559
		public int DataIndex { get; private set; }

		// Token: 0x17000240 RID: 576
		// (get) Token: 0x06000C46 RID: 3142 RVA: 0x00035362 File Offset: 0x00033562
		// (set) Token: 0x06000C47 RID: 3143 RVA: 0x0003536A File Offset: 0x0003356A
		public int ViewIndex { get; private set; }

		// Token: 0x17000241 RID: 577
		// (get) Token: 0x06000C48 RID: 3144 RVA: 0x00035373 File Offset: 0x00033573
		// (set) Token: 0x06000C49 RID: 3145 RVA: 0x0003537B File Offset: 0x0003357B
		public IUIPresentable Presenter { get; private set; }

		// Token: 0x17000242 RID: 578
		// (get) Token: 0x06000C4A RID: 3146 RVA: 0x00035384 File Offset: 0x00033584
		// (set) Token: 0x06000C4B RID: 3147 RVA: 0x0003538C File Offset: 0x0003358C
		public object ModelAsObject { get; private set; }

		// Token: 0x17000243 RID: 579
		// (get) Token: 0x06000C4C RID: 3148 RVA: 0x00035395 File Offset: 0x00033595
		// (set) Token: 0x06000C4D RID: 3149 RVA: 0x0003539D File Offset: 0x0003359D
		public Type ItemType { get; private set; }

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x06000C4E RID: 3150 RVA: 0x000353A6 File Offset: 0x000335A6
		// (set) Token: 0x06000C4F RID: 3151 RVA: 0x000353AE File Offset: 0x000335AE
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x06000C50 RID: 3152 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000C51 RID: 3153 RVA: 0x000353B8 File Offset: 0x000335B8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.selectButton.onClick.AddListener(new UnityAction(this.InvokeOnSelectButtonPressed));
			this.removeButton.onClick.AddListener(new UnityAction(this.InvokeOnRemoveButtonPressed));
		}

		// Token: 0x06000C52 RID: 3154 RVA: 0x00035415 File Offset: 0x00033615
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("OnDespawn", this);
			}
			this.ResetLayoutElement();
			this.Clear();
		}

		// Token: 0x06000C53 RID: 3155 RVA: 0x00035438 File Offset: 0x00033638
		public UIIEnumerableItem SpawnPooledInstance(Transform parent = null)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SpawnPooledInstance", new object[] { parent });
			}
			return MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<UIIEnumerableItem>(this, default(Vector3), default(Quaternion), parent);
		}

		// Token: 0x06000C54 RID: 3156 RVA: 0x00035482 File Offset: 0x00033682
		public void ReturnToPool()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ReturnToPool", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIIEnumerableItem>(this);
		}

		// Token: 0x06000C55 RID: 3157 RVA: 0x000354A7 File Offset: 0x000336A7
		public void PrewarmPool(int count)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PrewarmPool", new object[] { count });
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPoolOverTime<UIIEnumerableItem>(this, count);
		}

		// Token: 0x06000C56 RID: 3158 RVA: 0x000354D8 File Offset: 0x000336D8
		public void View(object model, UIBaseIEnumerableView iEnumerableView, int dataIndex, int viewIndex)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[]
				{
					model,
					iEnumerableView.DebugSafeName(true),
					dataIndex,
					viewIndex
				});
			}
			this.ModelAsObject = model;
			this.ClearVisuals();
			if (model == null)
			{
				DebugUtility.LogException(new NullReferenceException("model cannot be null!"), this);
				return;
			}
			Type type = model.GetType();
			this.iEnumerableView = iEnumerableView;
			this.DataIndex = dataIndex;
			this.ViewIndex = viewIndex;
			this.ItemType = UIPresenterModelTypeUtility.SanitizeType(type);
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}", new object[]
				{
					"ItemType",
					this.ItemType.Name,
					"model",
					model
				}), this);
			}
			this.HandleListModificationVisibility(iEnumerableView.CanAddAndRemoveItems);
			this.HandleSelectionVisibility(iEnumerableView.CanSelect);
			this.Presenter = this.SpawnPresenter(model, dataIndex, iEnumerableView);
			IUITypeStyleOverridable iuitypeStyleOverridable = this.Presenter.Viewable as IUITypeStyleOverridable;
			if (iuitypeStyleOverridable != null)
			{
				Dictionary<Type, Enum> dictionary = new Dictionary<Type, Enum>(iEnumerableView.TypeStyleOverrideDictionary);
				iuitypeStyleOverridable.SetTypeStyleOverrideDictionary(dictionary);
			}
			IUIInteractable iuiinteractable = this.Presenter.Viewable as IUIInteractable;
			if (iuiinteractable != null)
			{
				iuiinteractable.SetInteractable(iEnumerableView.CanModifyItemModel);
			}
			if (TypeUtility.IsIEnumerable(type))
			{
				UIIEnumerablePresenter uiienumerablePresenter = this.Presenter as UIIEnumerablePresenter;
				UIBaseIEnumerableView ienumerableView = uiienumerablePresenter.IEnumerableView;
				ienumerableView.LayoutElement.FlexibleHeightLayoutDimension.ExplicitValue = 1f;
				ienumerableView.SetCanAddAndRemoveItems(iEnumerableView.ChildIEnumerableCanAddAndRemoveItems);
				Type elementType = type.GetElementType();
				if (elementType == null)
				{
					Debug.LogException(new NullReferenceException("elementType cannot be null!"), this);
				}
				else
				{
					uiienumerablePresenter.SetElementType(elementType);
					if (elementType != null && elementType.IsEnum)
					{
						ienumerableView.SetDisplayEnumLabels(false);
					}
				}
			}
			this.selectButton.SetChildGraphicsToChildren();
			this.Presenter.RectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			this.Presenter.Viewable.SetMaskable(true);
			this.Presenter.OnModelChanged += this.InvokeOnModelChanged;
			IUIIEnumerableContainable iuiienumerableContainable = this.Presenter.Viewable as IUIIEnumerableContainable;
			if (iuiienumerableContainable != null)
			{
				this.iEnumerableContainable = iuiienumerableContainable;
				this.iEnumerableContainable.OnEnumerableCountChanged += this.InvokeOnEnumerableCountChanged;
			}
			if (this.ItemType == typeof(Enum))
			{
				((this.Presenter as UIEnumPresenter).Viewable as UIEnumDropdownView).SetLabel(iEnumerableView.DisplayEnumLabels ? this.ItemType.Name : string.Empty);
			}
		}

		// Token: 0x06000C57 RID: 3159 RVA: 0x00035772 File Offset: 0x00033972
		public void SetHeight(float height)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetHeight", new object[] { height });
			}
			this.layoutElement.minHeight = height;
		}

		// Token: 0x06000C58 RID: 3160 RVA: 0x000357A2 File Offset: 0x000339A2
		public void SetWidth(float width)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetWidth", new object[] { width });
			}
			this.layoutElement.minWidth = width;
		}

		// Token: 0x06000C59 RID: 3161 RVA: 0x000357D4 File Offset: 0x000339D4
		public void HandleSelectionVisibility(bool canSelect)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleSelectionVisibility", new object[] { canSelect });
			}
			this.selectedVisuals.SetActive(canSelect && this.iEnumerableView.SelectionVisualStyle != UIBaseIEnumerableView.SelectionVisualStyleEnum.Outline);
			this.selectButton.enabled = canSelect;
			this.selectGraphic.enabled = canSelect;
			if (canSelect)
			{
				bool flag = this.iEnumerableView.GetIsSelected(this.ModelAsObject);
				this.ViewSelectedStatus(this.iEnumerableView, flag);
			}
		}

		// Token: 0x06000C5A RID: 3162 RVA: 0x00035864 File Offset: 0x00033A64
		public void ViewSelectedStatus(UIBaseIEnumerableView iEnumerableView, bool isSelected)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewSelectedStatus", new object[]
				{
					iEnumerableView.DebugSafeName(true),
					isSelected
				});
			}
			this.iEnumerableView = iEnumerableView;
			if (!iEnumerableView.CanSelect)
			{
				return;
			}
			if (isSelected)
			{
				UIBaseIEnumerableView.SelectionVisualStyleEnum selectionVisualStyle = iEnumerableView.SelectionVisualStyle;
				this.check.SetActive(selectionVisualStyle == UIBaseIEnumerableView.SelectionVisualStyleEnum.Check);
				this.order.SetActive(selectionVisualStyle == UIBaseIEnumerableView.SelectionVisualStyleEnum.Order);
				this.orderText.text = iEnumerableView.GetSelectionOrder(this.ModelAsObject).ToString();
				this.outline.SetActive(selectionVisualStyle == UIBaseIEnumerableView.SelectionVisualStyleEnum.Outline);
				return;
			}
			this.HideSelectedStatus();
		}

		// Token: 0x06000C5B RID: 3163 RVA: 0x0003590F File Offset: 0x00033B0F
		private void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.ModelAsObject = null;
			this.ClearVisuals();
			this.OnModelChanged = null;
			this.OnSelectButtonPressed = null;
			this.OnRemoveButtonPressed = null;
		}

		// Token: 0x06000C5C RID: 3164 RVA: 0x0003594C File Offset: 0x00033B4C
		public void SetInteractable(bool interactable)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			IUIPresentable presenter = this.Presenter;
			IUIInteractable iuiinteractable = ((presenter != null) ? presenter.Viewable : null) as IUIInteractable;
			if (iuiinteractable != null)
			{
				iuiinteractable.SetInteractable(interactable);
			}
		}

		// Token: 0x06000C5D RID: 3165 RVA: 0x000359A0 File Offset: 0x00033BA0
		private void ClearVisuals()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearVisuals", Array.Empty<object>());
			}
			if (this.Presenter != null)
			{
				this.Presenter.OnModelChanged -= this.InvokeOnModelChanged;
				UIIEnumerablePresenter uiienumerablePresenter = this.Presenter as UIIEnumerablePresenter;
				if (uiienumerablePresenter != null)
				{
					uiienumerablePresenter.IEnumerableView.LayoutElement.FlexibleHeightLayoutDimension.ExplicitValue = -1f;
				}
				this.Presenter.ReturnToPool();
				this.Presenter = null;
			}
			if (this.iEnumerableContainable != null)
			{
				this.iEnumerableContainable.OnEnumerableCountChanged -= this.InvokeOnEnumerableCountChanged;
			}
			this.HideSelectedStatus();
		}

		// Token: 0x06000C5E RID: 3166 RVA: 0x00035A44 File Offset: 0x00033C44
		private IUIPresentable SpawnPresenter(object model, int dataIndex, UIBaseIEnumerableView iEnumerableView)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SpawnPresenter", new object[] { model, dataIndex, iEnumerableView });
			}
			IReadOnlyList<string> itemDisplayNames = iEnumerableView.ItemDisplayNames;
			string text = ((itemDisplayNames.Count > dataIndex) ? itemDisplayNames[dataIndex] : null);
			bool flag = !text.IsNullOrEmptyOrWhiteSpace();
			Dictionary<Type, Enum> dictionary = new Dictionary<Type, Enum>(iEnumerableView.TypeStyleOverrideDictionary);
			Enum @enum;
			IUIPresentable iuipresentable;
			if (dictionary.TryGetValue(this.ItemType, out @enum))
			{
				iuipresentable = (flag ? MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnNamedViewPresenter(this.container, dictionary).SpawnObjectModelWithStyle(model, @enum, text) : MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithStyle(model, @enum, this.container, dictionary));
			}
			else
			{
				iuipresentable = (flag ? MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnNamedViewPresenter(this.container, dictionary).SpawnObjectModelWithDefaultStyle(model, text) : MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithDefaultStyle(model, this.container, dictionary));
			}
			return iuipresentable;
		}

		// Token: 0x06000C5F RID: 3167 RVA: 0x00035B2C File Offset: 0x00033D2C
		private void HandleListModificationVisibility(bool canModifyList)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleListModificationVisibility", new object[] { canModifyList });
			}
			this.dragIndicator.gameObject.SetActive(false);
			this.removeButton.gameObject.SetActive(canModifyList);
		}

		// Token: 0x06000C60 RID: 3168 RVA: 0x00035B7D File Offset: 0x00033D7D
		private void InvokeOnModelChanged(object model)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnModelChanged", new object[] { model });
			}
			Action<object, int> onModelChanged = this.OnModelChanged;
			if (onModelChanged == null)
			{
				return;
			}
			onModelChanged(model, this.DataIndex);
		}

		// Token: 0x06000C61 RID: 3169 RVA: 0x00035BB3 File Offset: 0x00033DB3
		private void InvokeOnEnumerableCountChanged()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnEnumerableCountChanged", Array.Empty<object>());
			}
			Action onEnumerableCountChanged = this.OnEnumerableCountChanged;
			if (onEnumerableCountChanged == null)
			{
				return;
			}
			onEnumerableCountChanged();
		}

		// Token: 0x06000C62 RID: 3170 RVA: 0x00035BDD File Offset: 0x00033DDD
		private void InvokeOnSelectButtonPressed()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnSelectButtonPressed", Array.Empty<object>());
			}
			Action<object> onSelectButtonPressed = this.OnSelectButtonPressed;
			if (onSelectButtonPressed == null)
			{
				return;
			}
			onSelectButtonPressed(this.ModelAsObject);
		}

		// Token: 0x06000C63 RID: 3171 RVA: 0x00035C0D File Offset: 0x00033E0D
		private void InvokeOnRemoveButtonPressed()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnRemoveButtonPressed", Array.Empty<object>());
			}
			Action<object> onRemoveButtonPressed = this.OnRemoveButtonPressed;
			if (onRemoveButtonPressed == null)
			{
				return;
			}
			onRemoveButtonPressed(this.ModelAsObject);
		}

		// Token: 0x06000C64 RID: 3172 RVA: 0x00035C3D File Offset: 0x00033E3D
		private void HideSelectedStatus()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("HideSelectedStatus", this);
			}
			this.check.SetActive(false);
			this.order.SetActive(false);
			this.outline.SetActive(false);
		}

		// Token: 0x06000C65 RID: 3173 RVA: 0x00035C78 File Offset: 0x00033E78
		public void OnChildIEnumerableCanAddAndRemoveItemsChanged(bool canAddAndRemoveItems)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnChildIEnumerableCanAddAndRemoveItemsChanged", new object[] { canAddAndRemoveItems });
			}
			UIIEnumerablePresenter uiienumerablePresenter = this.Presenter as UIIEnumerablePresenter;
			if (uiienumerablePresenter != null)
			{
				uiienumerablePresenter.IEnumerableView.SetCanAddAndRemoveItems(canAddAndRemoveItems);
			}
		}

		// Token: 0x06000C66 RID: 3174 RVA: 0x00035CC2 File Offset: 0x00033EC2
		private void ResetLayoutElement()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("ResetLayoutElement", this);
			}
			this.layoutElement.minHeight = -1f;
			this.layoutElement.minWidth = -1f;
		}

		// Token: 0x040007F3 RID: 2035
		[SerializeField]
		private Image dragIndicator;

		// Token: 0x040007F4 RID: 2036
		[SerializeField]
		private RectTransform container;

		// Token: 0x040007F5 RID: 2037
		[SerializeField]
		private UIButton removeButton;

		// Token: 0x040007F6 RID: 2038
		[SerializeField]
		private LayoutElement layoutElement;

		// Token: 0x040007F7 RID: 2039
		[Header("Selection")]
		[SerializeField]
		private UIButton selectButton;

		// Token: 0x040007F8 RID: 2040
		[SerializeField]
		private Graphic selectGraphic;

		// Token: 0x040007F9 RID: 2041
		[SerializeField]
		private GameObject selectedVisuals;

		// Token: 0x040007FA RID: 2042
		[SerializeField]
		private GameObject check;

		// Token: 0x040007FB RID: 2043
		[SerializeField]
		private GameObject order;

		// Token: 0x040007FC RID: 2044
		[SerializeField]
		private TextMeshProUGUI orderText;

		// Token: 0x040007FD RID: 2045
		[SerializeField]
		private GameObject outline;

		// Token: 0x040007FE RID: 2046
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040007FF RID: 2047
		private IUIIEnumerableContainable iEnumerableContainable;

		// Token: 0x04000800 RID: 2048
		private UIBaseIEnumerableView iEnumerableView;
	}
}
