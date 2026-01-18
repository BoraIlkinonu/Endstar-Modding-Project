using System;
using System.Collections;
using System.Collections.Generic;
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

namespace Endless.Creator.UI;

public abstract class UIBasePropertyView<TModel, TViewStyle> : UIBaseView<TModel, TViewStyle>, IUIInteractable, IUITypeStyleOverridable, IUIIEnumerableContainable, IUIBasePropertyViewable where TViewStyle : Enum
{
	private enum LayoutStyle
	{
		Standard,
		FullWidth
	}

	private const float TOP_LEFT_NAME_TEXT_ALIGNMENT_THRESHOLD = 100f;

	[Header("UIBasePropertyView")]
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private UIRectTransformDictionary[] rectTransformDictionaries = Array.Empty<UIRectTransformDictionary>();

	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private ClampValue defaultClampValue;

	[SerializeField]
	private UIPresenterScriptableObjectArray fullWidthPresenters;

	[SerializeField]
	private UIPresenterDictionary presenterDictionary;

	private readonly HashSet<Type> fullWidthTypes = new HashSet<Type>();

	private readonly Dictionary<LayoutStyle, int> heightPaddingDictionary = new Dictionary<LayoutStyle, int>
	{
		{
			LayoutStyle.Standard,
			20
		},
		{
			LayoutStyle.FullWidth,
			93
		}
	};

	private LayoutStyle activeLayoutStyle;

	private IUIPresentable childPresenter;

	private IUIViewable childView;

	[NonSerialized]
	private bool fullWidthTypesConstructed;

	private bool subScribedToChildPresenterOnModelChanged;

	private Dictionary<Type, Enum> typeStyleOverrideDictionary = new Dictionary<Type, Enum>();

	private HashSet<Type> FullWidthTypes
	{
		get
		{
			if (!fullWidthTypesConstructed)
			{
				InterfaceReference<IUIPresentable>[] value = fullWidthPresenters.Value;
				foreach (InterfaceReference<IUIPresentable> interfaceReference in value)
				{
					fullWidthTypes.Add(interfaceReference.Interface.ModelType);
				}
				fullWidthTypesConstructed = true;
			}
			return fullWidthTypes;
		}
	}

	public event Action<object> OnUserChangedModel;

	public event Action OnEnumerableCountChanged;

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		nameText.maxVisibleLines = 2;
	}

	public void SetInteractable(bool interactable)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
		}
		if (childView is IUIInteractable iUIInteractable)
		{
			iUIInteractable.SetInteractable(interactable);
		}
	}

	public void SetTypeStyleOverrideDictionary(Dictionary<Type, Enum> value)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetTypeStyleOverrideDictionary", "value", value.Count), this);
		}
		typeStyleOverrideDictionary = value;
	}

	public override float GetPreferredHeight(object model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetPreferredHeight", "model", model), this);
		}
		object obj = ExtractModel(model);
		float size = presenterDictionary.GetSize(obj, LayoutReferenceType.Height, null);
		Type type = UIPresenterModelTypeUtility.SanitizeType(obj.GetType());
		LayoutStyle layoutStyle = (FullWidthTypes.Contains(type) ? LayoutStyle.FullWidth : LayoutStyle.Standard);
		int num = heightPaddingDictionary[layoutStyle];
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}\n{2}: {3}\n{4}: {5}\n{6}: {7}\n{8}: {9}", "extractedModel", obj, "typeToCheckLayoutStyleWith", type.Name, "layoutStyle", layoutStyle, "preferredHeight", size, "heightPadding", num), this);
		}
		return size + (float)num;
	}

	protected virtual void ViewProperty(string name, object model, int dataTypeId, string description, ClampValue[] clampValues)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}, {2}, {3}, {4}, {5}, {6} {7}, {8}, {9}, {10} )", "ViewProperty", "name", name, "model", model.DebugSafeJson(), "dataTypeId", dataTypeId, "description", description, "clampValues", clampValues.Length), this);
		}
		if (model == null)
		{
			DebugUtility.LogException(new NullReferenceException("model '" + name + "' is null!"), this);
		}
		nameText.text = "<color=white>" + StringUtilities.PrettifyName(name) + "</color>";
		bool flag = !description.IsNullOrEmptyOrWhiteSpace();
		nameText.fontStyle = (flag ? FontStyles.Underline : FontStyles.Normal);
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
			childPresenter = ViewEnum(model, typeFromId);
		}
		else if (flag2)
		{
			childPresenter = ViewIEnumerable(model);
			UIIEnumerablePresenter obj = childPresenter as UIIEnumerablePresenter;
			Type arrayOrListElementType = UIPresenterModelTypeUtility.GetArrayOrListElementType(typeFromId);
			obj.SetElementType(arrayOrListElementType);
			UIBaseIEnumerableView uIBaseIEnumerableView = childPresenter.Viewable as UIBaseIEnumerableView;
			uIBaseIEnumerableView.LayoutElement.FlexibleHeightLayoutDimension.ExplicitValue = 1f;
			uIBaseIEnumerableView.SetInteractable(interactable: false);
			SetReadOnlyBasedOnRole(uIBaseIEnumerableView);
			uIBaseIEnumerableView.OnItemAdded += InvokeOnEnumerableCountChanged;
			uIBaseIEnumerableView.OnItemRemoved += InvokeOnEnumerableCountChanged;
			if (uIBaseIEnumerableView is UIIEnumerableStraightVirtualizedView uIIEnumerableStraightVirtualizedView)
			{
				uIIEnumerableStraightVirtualizedView.ReloadDataAndKeepPosition();
			}
		}
		else
		{
			childPresenter = (typeStyleOverrideDictionary.TryGetValue(typeFromId, out var value) ? MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithStyle(model, value, container, typeStyleOverrideDictionary) : MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithDefaultStyle(model, container, typeStyleOverrideDictionary));
			if (TypeUtility.IsNumeric(typeFromId))
			{
				childPresenter.RectTransform.TryGetComponent<IFieldCountable>(out var component);
				ApplyClampValues(clampValues, component.FieldCount);
			}
		}
		childView = childPresenter.Viewable;
		childPresenter.RectTransform.SetAnchor(AnchorPresets.StretchAll);
		if (childView == null)
		{
			DebugUtility.LogException(new NullReferenceException("childView is null! It was expected to be a type of IUITypedViewable<" + typeFromId.Name + "> but it was a " + childPresenter.Viewable.GetType().Name + "!"), childPresenter.RectTransform);
			return;
		}
		if (!subScribedToChildPresenterOnModelChanged)
		{
			childPresenter.OnModelChanged += OnChildPresenterModelChanged;
			subScribedToChildPresenterOnModelChanged = true;
		}
		childView.SetMaskable(maskable: true);
		base.LayoutElement.PreferredHeightLayoutDimension.ExplicitValue = GetPreferredHeight(model);
		nameText.alignment = ((base.LayoutElement.preferredHeight >= 100f && !flag2) ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Left);
		activeLayoutStyle = LayoutStyle.Standard;
		Type item = UIPresenterModelTypeUtility.SanitizeType(typeFromId);
		if (FullWidthTypes.Contains(item))
		{
			activeLayoutStyle = LayoutStyle.FullWidth;
		}
		UIRectTransformDictionary[] array = rectTransformDictionaries;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Apply(activeLayoutStyle.ToString());
		}
		childView.LayoutElement.FlexibleHeightLayoutDimension.ExplicitValue = 1f;
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Clear", this);
		}
		if (childPresenter != null)
		{
			if (childPresenter is UIIEnumerablePresenter uIIEnumerablePresenter)
			{
				UIBaseIEnumerableView obj = uIIEnumerablePresenter.Viewable as UIBaseIEnumerableView;
				obj.OnItemAdded -= InvokeOnEnumerableCountChanged;
				obj.OnItemRemoved -= InvokeOnEnumerableCountChanged;
				obj.LayoutElement.FlexibleHeightLayoutDimension.ExplicitValue = -1f;
			}
			if (subScribedToChildPresenterOnModelChanged)
			{
				childPresenter.OnModelChanged -= OnChildPresenterModelChanged;
				subScribedToChildPresenterOnModelChanged = false;
			}
			childPresenter.ReturnToPool();
			childPresenter = null;
			typeStyleOverrideDictionary.Clear();
		}
	}

	protected abstract object ExtractModel(object model);

	private async void SetReadOnlyBasedOnRole(UIBaseIEnumerableView enumerableView)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("SetReadOnlyBasedOnRole ( enumerableView: " + enumerableView.gameObject.name + " )", this);
		}
		bool flag = (await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID)).Roles.GetRoleForUserId(EndlessServices.Instance.CloudService.ActiveUserId).IsGreaterThan(Roles.Viewer);
		enumerableView.SetInteractable(flag);
		enumerableView.SetCanAddAndRemoveItems(flag);
	}

	private IUIPresentable ViewEnum(object model, Type enumType)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", "ViewEnum", "model", model.DebugSafeJson(), "enumType", enumType), this);
		}
		Enum model2 = (Enum)Enum.ToObject(enumType, model);
		UIEnumPresenter uIEnumPresenter;
		if (typeStyleOverrideDictionary.TryGetValue(typeof(Enum), out var value))
		{
			UIBaseEnumView.Styles style = (UIBaseEnumView.Styles)(object)value;
			uIEnumPresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumWithStyle(model2, style, container);
		}
		else
		{
			uIEnumPresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumWithDefaultStyle(model2, container);
		}
		(uIEnumPresenter.Viewable as UIEnumDropdownView).SetLabel(string.Empty);
		return uIEnumPresenter;
	}

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
			DebugUtility.LogException(new NullReferenceException("modelAsIEnumerable '" + base.name + "' is null! model was a type of " + model.GetType().Name + "."), this);
		}
		UIIEnumerablePresenter uIIEnumerablePresenter;
		if (typeStyleOverrideDictionary.TryGetValue(typeof(IEnumerable), out var value))
		{
			UIBaseIEnumerableView.ArrangementStyle style = (UIBaseIEnumerableView.ArrangementStyle)(object)value;
			uIIEnumerablePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumerableWithStyle(enumerable, style, container, typeStyleOverrideDictionary);
		}
		else
		{
			uIIEnumerablePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumerableWithDefaultStyle(enumerable, container, typeStyleOverrideDictionary);
		}
		uIIEnumerablePresenter.IEnumerableView.SetDisplayEnumLabels(displayEnumLabels: false);
		return uIIEnumerablePresenter;
	}

	private void ApplyClampValues(ClampValue[] clampValues, int fieldCount)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", "ApplyClampValues", "clampValues", (clampValues == null) ? "null" : clampValues.Length.ToString(), "fieldCount", fieldCount), this);
		}
		if (clampValues == null)
		{
			clampValues = new ClampValue[fieldCount];
			for (int i = 0; i < fieldCount; i++)
			{
				clampValues[i] = new ClampValue(defaultClampValue);
			}
		}
		IUIPresentable iUIPresentable = childPresenter;
		if (!(iUIPresentable is IUIFloatClampable iUIFloatClampable))
		{
			if (iUIPresentable is IUIIntClampable iUIIntClampable)
			{
				for (int j = 0; j < clampValues.Length; j++)
				{
					ClampValue clampValue = clampValues[j];
					if (clampValue.ShouldClampValue)
					{
						iUIIntClampable.SetMinMax(j, (int)clampValue.MinValue, (int)clampValue.MaxValue);
					}
					else
					{
						iUIIntClampable.Unclamp();
					}
				}
			}
			else
			{
				DebugUtility.LogException(new Exception(childPresenter.ModelType.Name + " is not supported!"), this);
			}
			return;
		}
		for (int k = 0; k < clampValues.Length; k++)
		{
			ClampValue clampValue2 = clampValues[k];
			if (clampValue2.ShouldClampValue)
			{
				iUIFloatClampable.SetMinMax(k, clampValue2.MinValue, clampValue2.MaxValue);
			}
			else
			{
				iUIFloatClampable.Unclamp();
			}
		}
	}

	private void OnChildPresenterModelChanged(object childModel)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnChildPresenterModelChanged", "childModel", childModel), this);
		}
		this.OnUserChangedModel?.Invoke(childModel);
	}

	private void InvokeOnEnumerableCountChanged()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("InvokeOnEnumerableCountChanged", this);
		}
		this.OnEnumerableCountChanged?.Invoke();
	}

	private void InvokeOnEnumerableCountChanged(object item)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeOnEnumerableCountChanged", "item", item), this);
		}
		this.OnEnumerableCountChanged?.Invoke();
	}
}
