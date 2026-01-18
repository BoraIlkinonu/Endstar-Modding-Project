using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Gameplay.Serialization;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using Runtime.Shared.Matchmaking;
using Runtime.Shared.Utilities;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001FA RID: 506
	public abstract class UIBasePropertyView<TModel, TViewStyle> : UIBaseView<TModel, TViewStyle>, IUIInteractable, IUITypeStyleOverridable, IUIIEnumerableContainable, IUIBasePropertyViewable where TViewStyle : Enum
	{
		// Token: 0x1400000B RID: 11
		// (add) Token: 0x060007EC RID: 2028 RVA: 0x00026EF4 File Offset: 0x000250F4
		// (remove) Token: 0x060007ED RID: 2029 RVA: 0x00026F2C File Offset: 0x0002512C
		public event Action<object> OnUserChangedModel;

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x060007EE RID: 2030 RVA: 0x00026F64 File Offset: 0x00025164
		// (remove) Token: 0x060007EF RID: 2031 RVA: 0x00026F9C File Offset: 0x0002519C
		public event Action OnEnumerableCountChanged;

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x060007F0 RID: 2032 RVA: 0x00026FD4 File Offset: 0x000251D4
		private HashSet<Type> FullWidthTypes
		{
			get
			{
				if (!this.fullWidthTypesConstructed)
				{
					foreach (InterfaceReference<IUIPresentable> interfaceReference in this.fullWidthPresenters.Value)
					{
						this.fullWidthTypes.Add(interfaceReference.Interface.ModelType);
					}
					this.fullWidthTypesConstructed = true;
				}
				return this.fullWidthTypes;
			}
		}

		// Token: 0x060007F1 RID: 2033 RVA: 0x0002702B File Offset: 0x0002522B
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.nameText.maxVisibleLines = 2;
		}

		// Token: 0x060007F2 RID: 2034 RVA: 0x0002704C File Offset: 0x0002524C
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
			}
			IUIInteractable iuiinteractable = this.childView as IUIInteractable;
			if (iuiinteractable != null)
			{
				iuiinteractable.SetInteractable(interactable);
			}
		}

		// Token: 0x060007F3 RID: 2035 RVA: 0x00027097 File Offset: 0x00025297
		public void SetTypeStyleOverrideDictionary(Dictionary<Type, Enum> value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetTypeStyleOverrideDictionary", "value", value.Count), this);
			}
			this.typeStyleOverrideDictionary = value;
		}

		// Token: 0x060007F4 RID: 2036 RVA: 0x000270D0 File Offset: 0x000252D0
		public override float GetPreferredHeight(object model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetPreferredHeight", "model", model), this);
			}
			object obj = this.ExtractModel(model);
			float size = this.presenterDictionary.GetSize(obj, LayoutReferenceType.Height, null);
			Type type = UIPresenterModelTypeUtility.SanitizeType(obj.GetType());
			UIBasePropertyView<TModel, TViewStyle>.LayoutStyle layoutStyle = (this.FullWidthTypes.Contains(type) ? UIBasePropertyView<TModel, TViewStyle>.LayoutStyle.FullWidth : UIBasePropertyView<TModel, TViewStyle>.LayoutStyle.Standard);
			int num = this.heightPaddingDictionary[layoutStyle];
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}\n{2}: {3}\n{4}: {5}\n{6}: {7}\n{8}: {9}", new object[] { "extractedModel", obj, "typeToCheckLayoutStyleWith", type.Name, "layoutStyle", layoutStyle, "preferredHeight", size, "heightPadding", num }), this);
			}
			return size + (float)num;
		}

		// Token: 0x060007F5 RID: 2037 RVA: 0x000271BC File Offset: 0x000253BC
		protected virtual void ViewProperty(string name, object model, int dataTypeId, string description, ClampValue[] clampValues)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}, {2}, {3}, {4}, {5}, {6} {7}, {8}, {9}, {10} )", new object[]
				{
					"ViewProperty",
					"name",
					name,
					"model",
					model.DebugSafeJson(),
					"dataTypeId",
					dataTypeId,
					"description",
					description,
					"clampValues",
					clampValues.Length
				}), this);
			}
			if (model == null)
			{
				DebugUtility.LogException(new NullReferenceException("model '" + name + "' is null!"), this);
			}
			this.nameText.text = "<color=white>" + StringUtilities.PrettifyName(name) + "</color>";
			bool flag = !description.IsNullOrEmptyOrWhiteSpace();
			this.nameText.fontStyle = (flag ? FontStyles.Underline : FontStyles.Normal);
			Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(dataTypeId);
			if (typeFromId == null)
			{
				DebugUtility.LogError(string.Format("Could not get type from a {0} of {1}!", "dataTypeId", dataTypeId), this);
				return;
			}
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "type", typeFromId), this);
			}
			bool flag2 = TypeUtility.IsIEnumerable(typeFromId);
			if (typeFromId.IsEnum)
			{
				this.childPresenter = this.ViewEnum(model, typeFromId);
			}
			else if (flag2)
			{
				this.childPresenter = this.ViewIEnumerable(model);
				UIIEnumerablePresenter uiienumerablePresenter = this.childPresenter as UIIEnumerablePresenter;
				Type arrayOrListElementType = UIPresenterModelTypeUtility.GetArrayOrListElementType(typeFromId);
				uiienumerablePresenter.SetElementType(arrayOrListElementType);
				UIBaseIEnumerableView uibaseIEnumerableView = this.childPresenter.Viewable as UIBaseIEnumerableView;
				uibaseIEnumerableView.LayoutElement.FlexibleHeightLayoutDimension.ExplicitValue = 1f;
				uibaseIEnumerableView.SetInteractable(false);
				this.SetReadOnlyBasedOnRole(uibaseIEnumerableView);
				uibaseIEnumerableView.OnItemAdded += this.InvokeOnEnumerableCountChanged;
				uibaseIEnumerableView.OnItemRemoved += this.InvokeOnEnumerableCountChanged;
				UIIEnumerableStraightVirtualizedView uiienumerableStraightVirtualizedView = uibaseIEnumerableView as UIIEnumerableStraightVirtualizedView;
				if (uiienumerableStraightVirtualizedView != null)
				{
					uiienumerableStraightVirtualizedView.ReloadDataAndKeepPosition();
				}
			}
			else
			{
				Enum @enum;
				this.childPresenter = (this.typeStyleOverrideDictionary.TryGetValue(typeFromId, out @enum) ? MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithStyle(model, @enum, this.container, this.typeStyleOverrideDictionary) : MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithDefaultStyle(model, this.container, this.typeStyleOverrideDictionary));
				if (TypeUtility.IsNumeric(typeFromId))
				{
					IFieldCountable fieldCountable;
					this.childPresenter.RectTransform.TryGetComponent<IFieldCountable>(out fieldCountable);
					this.ApplyClampValues(clampValues, fieldCountable.FieldCount);
				}
			}
			this.childView = this.childPresenter.Viewable;
			this.childPresenter.RectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			if (this.childView == null)
			{
				DebugUtility.LogException(new NullReferenceException(string.Concat(new string[]
				{
					"childView is null! It was expected to be a type of IUITypedViewable<",
					typeFromId.Name,
					"> but it was a ",
					this.childPresenter.Viewable.GetType().Name,
					"!"
				})), this.childPresenter.RectTransform);
				return;
			}
			if (!this.subScribedToChildPresenterOnModelChanged)
			{
				this.childPresenter.OnModelChanged += this.OnChildPresenterModelChanged;
				this.subScribedToChildPresenterOnModelChanged = true;
			}
			this.childView.SetMaskable(true);
			base.LayoutElement.PreferredHeightLayoutDimension.ExplicitValue = this.GetPreferredHeight(model);
			this.nameText.alignment = ((base.LayoutElement.preferredHeight >= 100f && !flag2) ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Left);
			this.activeLayoutStyle = UIBasePropertyView<TModel, TViewStyle>.LayoutStyle.Standard;
			Type type = UIPresenterModelTypeUtility.SanitizeType(typeFromId);
			if (this.FullWidthTypes.Contains(type))
			{
				this.activeLayoutStyle = UIBasePropertyView<TModel, TViewStyle>.LayoutStyle.FullWidth;
			}
			UIRectTransformDictionary[] array = this.rectTransformDictionaries;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Apply(this.activeLayoutStyle.ToString());
			}
			this.childView.LayoutElement.FlexibleHeightLayoutDimension.ExplicitValue = 1f;
		}

		// Token: 0x060007F6 RID: 2038 RVA: 0x000275A4 File Offset: 0x000257A4
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			if (this.childPresenter == null)
			{
				return;
			}
			UIIEnumerablePresenter uiienumerablePresenter = this.childPresenter as UIIEnumerablePresenter;
			if (uiienumerablePresenter != null)
			{
				UIBaseIEnumerableView uibaseIEnumerableView = uiienumerablePresenter.Viewable as UIBaseIEnumerableView;
				uibaseIEnumerableView.OnItemAdded -= this.InvokeOnEnumerableCountChanged;
				uibaseIEnumerableView.OnItemRemoved -= this.InvokeOnEnumerableCountChanged;
				uibaseIEnumerableView.LayoutElement.FlexibleHeightLayoutDimension.ExplicitValue = -1f;
			}
			if (this.subScribedToChildPresenterOnModelChanged)
			{
				this.childPresenter.OnModelChanged -= this.OnChildPresenterModelChanged;
				this.subScribedToChildPresenterOnModelChanged = false;
			}
			this.childPresenter.ReturnToPool();
			this.childPresenter = null;
			this.typeStyleOverrideDictionary.Clear();
		}

		// Token: 0x060007F7 RID: 2039
		protected abstract object ExtractModel(object model);

		// Token: 0x060007F8 RID: 2040 RVA: 0x00027664 File Offset: 0x00025864
		private async void SetReadOnlyBasedOnRole(UIBaseIEnumerableView enumerableView)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetReadOnlyBasedOnRole ( enumerableView: " + enumerableView.gameObject.name + " )", this);
			}
			TaskAwaiter<GetAllRolesResult> taskAwaiter = MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID, null, false).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<GetAllRolesResult> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<GetAllRolesResult>);
			}
			bool flag = taskAwaiter.GetResult().Roles.GetRoleForUserId(EndlessServices.Instance.CloudService.ActiveUserId).IsGreaterThan(Roles.Viewer);
			enumerableView.SetInteractable(flag);
			enumerableView.SetCanAddAndRemoveItems(flag);
		}

		// Token: 0x060007F9 RID: 2041 RVA: 0x000276A4 File Offset: 0x000258A4
		private IUIPresentable ViewEnum(object model, Type enumType)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[]
				{
					"ViewEnum",
					"model",
					model.DebugSafeJson(),
					"enumType",
					enumType
				}), this);
			}
			Enum @enum = (Enum)Enum.ToObject(enumType, model);
			Enum enum2;
			UIEnumPresenter uienumPresenter;
			if (this.typeStyleOverrideDictionary.TryGetValue(typeof(Enum), out enum2))
			{
				UIBaseEnumView.Styles styles = (UIBaseEnumView.Styles)enum2;
				uienumPresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumWithStyle(@enum, styles, this.container);
			}
			else
			{
				uienumPresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumWithDefaultStyle(@enum, this.container);
			}
			(uienumPresenter.Viewable as UIEnumDropdownView).SetLabel(string.Empty);
			return uienumPresenter;
		}

		// Token: 0x060007FA RID: 2042 RVA: 0x00027760 File Offset: 0x00025960
		private IUIPresentable ViewIEnumerable(object model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ViewIEnumerable ( model, " + model.DebugSafeJson() + " )", this);
			}
			if (model == null)
			{
				DebugUtility.LogException(new NullReferenceException("model '" + base.name + "' is null!"), this);
			}
			IEnumerable enumerable = model as IEnumerable;
			if (enumerable == null)
			{
				DebugUtility.LogException(new NullReferenceException(string.Concat(new string[]
				{
					"modelAsIEnumerable '",
					base.name,
					"' is null! model was a type of ",
					model.GetType().Name,
					"."
				})), this);
			}
			Enum @enum;
			UIIEnumerablePresenter uiienumerablePresenter;
			if (this.typeStyleOverrideDictionary.TryGetValue(typeof(IEnumerable), out @enum))
			{
				UIBaseIEnumerableView.ArrangementStyle arrangementStyle = (UIBaseIEnumerableView.ArrangementStyle)@enum;
				uiienumerablePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumerableWithStyle(enumerable, arrangementStyle, this.container, this.typeStyleOverrideDictionary);
			}
			else
			{
				uiienumerablePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumerableWithDefaultStyle(enumerable, this.container, this.typeStyleOverrideDictionary);
			}
			uiienumerablePresenter.IEnumerableView.SetDisplayEnumLabels(false);
			return uiienumerablePresenter;
		}

		// Token: 0x060007FB RID: 2043 RVA: 0x00027864 File Offset: 0x00025A64
		private void ApplyClampValues(ClampValue[] clampValues, int fieldCount)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[]
				{
					"ApplyClampValues",
					"clampValues",
					(clampValues == null) ? "null" : clampValues.Length.ToString(),
					"fieldCount",
					fieldCount
				}), this);
			}
			if (clampValues == null)
			{
				clampValues = new ClampValue[fieldCount];
				for (int i = 0; i < fieldCount; i++)
				{
					clampValues[i] = new ClampValue(this.defaultClampValue);
				}
			}
			IUIPresentable iuipresentable = this.childPresenter;
			IUIFloatClampable iuifloatClampable = iuipresentable as IUIFloatClampable;
			if (iuifloatClampable != null)
			{
				for (int j = 0; j < clampValues.Length; j++)
				{
					ClampValue clampValue = clampValues[j];
					if (clampValue.ShouldClampValue)
					{
						iuifloatClampable.SetMinMax(j, clampValue.MinValue, clampValue.MaxValue);
					}
					else
					{
						iuifloatClampable.Unclamp();
					}
				}
				return;
			}
			IUIIntClampable iuiintClampable = iuipresentable as IUIIntClampable;
			if (iuiintClampable == null)
			{
				DebugUtility.LogException(new Exception(this.childPresenter.ModelType.Name + " is not supported!"), this);
				return;
			}
			for (int k = 0; k < clampValues.Length; k++)
			{
				ClampValue clampValue2 = clampValues[k];
				if (clampValue2.ShouldClampValue)
				{
					iuiintClampable.SetMinMax(k, (int)clampValue2.MinValue, (int)clampValue2.MaxValue);
				}
				else
				{
					iuiintClampable.Unclamp();
				}
			}
		}

		// Token: 0x060007FC RID: 2044 RVA: 0x000279B7 File Offset: 0x00025BB7
		private void OnChildPresenterModelChanged(object childModel)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnChildPresenterModelChanged", "childModel", childModel), this);
			}
			Action<object> onUserChangedModel = this.OnUserChangedModel;
			if (onUserChangedModel == null)
			{
				return;
			}
			onUserChangedModel(childModel);
		}

		// Token: 0x060007FD RID: 2045 RVA: 0x000279ED File Offset: 0x00025BED
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

		// Token: 0x060007FE RID: 2046 RVA: 0x00027A12 File Offset: 0x00025C12
		private void InvokeOnEnumerableCountChanged(object item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeOnEnumerableCountChanged", "item", item), this);
			}
			Action onEnumerableCountChanged = this.OnEnumerableCountChanged;
			if (onEnumerableCountChanged == null)
			{
				return;
			}
			onEnumerableCountChanged();
		}

		// Token: 0x04000706 RID: 1798
		private const float TOP_LEFT_NAME_TEXT_ALIGNMENT_THRESHOLD = 100f;

		// Token: 0x04000707 RID: 1799
		[Header("UIBasePropertyView")]
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x04000708 RID: 1800
		[SerializeField]
		private UIRectTransformDictionary[] rectTransformDictionaries = Array.Empty<UIRectTransformDictionary>();

		// Token: 0x04000709 RID: 1801
		[SerializeField]
		private RectTransform container;

		// Token: 0x0400070A RID: 1802
		[SerializeField]
		private ClampValue defaultClampValue;

		// Token: 0x0400070B RID: 1803
		[SerializeField]
		private UIPresenterScriptableObjectArray fullWidthPresenters;

		// Token: 0x0400070C RID: 1804
		[SerializeField]
		private UIPresenterDictionary presenterDictionary;

		// Token: 0x0400070D RID: 1805
		private readonly HashSet<Type> fullWidthTypes = new HashSet<Type>();

		// Token: 0x0400070E RID: 1806
		private readonly Dictionary<UIBasePropertyView<TModel, TViewStyle>.LayoutStyle, int> heightPaddingDictionary = new Dictionary<UIBasePropertyView<TModel, TViewStyle>.LayoutStyle, int>
		{
			{
				UIBasePropertyView<TModel, TViewStyle>.LayoutStyle.Standard,
				20
			},
			{
				UIBasePropertyView<TModel, TViewStyle>.LayoutStyle.FullWidth,
				93
			}
		};

		// Token: 0x0400070F RID: 1807
		private UIBasePropertyView<TModel, TViewStyle>.LayoutStyle activeLayoutStyle;

		// Token: 0x04000710 RID: 1808
		private IUIPresentable childPresenter;

		// Token: 0x04000711 RID: 1809
		private IUIViewable childView;

		// Token: 0x04000712 RID: 1810
		[NonSerialized]
		private bool fullWidthTypesConstructed;

		// Token: 0x04000713 RID: 1811
		private bool subScribedToChildPresenterOnModelChanged;

		// Token: 0x04000714 RID: 1812
		private Dictionary<Type, Enum> typeStyleOverrideDictionary = new Dictionary<Type, Enum>();

		// Token: 0x020001FB RID: 507
		private enum LayoutStyle
		{
			// Token: 0x04000716 RID: 1814
			Standard,
			// Token: 0x04000717 RID: 1815
			FullWidth
		}
	}
}
