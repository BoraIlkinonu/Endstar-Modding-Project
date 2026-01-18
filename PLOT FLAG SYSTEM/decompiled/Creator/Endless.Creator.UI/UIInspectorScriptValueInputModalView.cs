using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIInspectorScriptValueInputModalView : UIScriptModalView
{
	[Header("UIInspectorScriptValueInputModalView")]
	[SerializeField]
	private UIInputField nameInputField;

	[SerializeField]
	private UIInputField descriptionInputField;

	[SerializeField]
	private UIInputField groupNameInputField;

	[SerializeField]
	private UIToggle hideToggle;

	[SerializeField]
	private RectTransform defaultValueContainer;

	[SerializeField]
	private InterfaceReference<IUIChildLayoutable>[] defaultValueContainerLayoutables = Array.Empty<InterfaceReference<IUIChildLayoutable>>();

	private readonly Dictionary<Type, Enum> typeStyleOverrideDictionary = new Dictionary<Type, Enum>
	{
		{
			typeof(InstanceReference),
			UIInstanceReferenceView.Styles.NoneOrContext
		},
		{
			typeof(NpcInstanceReference),
			UINpcInstanceReferenceView.Styles.NoneOrContext
		},
		{
			typeof(LevelDestination),
			UILevelDestinationView.Styles.None
		},
		{
			typeof(CellReference),
			UICellReferenceView.Styles.ReadOnly
		},
		{
			typeof(Color),
			UIBaseColorView.Styles.Detail
		}
	};

	public IUIPresentable Presentable { get; private set; }

	public Type InspectorScriptValueType { get; private set; }

	public bool IsCollection { get; private set; }

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		InspectorScriptValueType = (Type)modalData[0];
		IsCollection = (bool)modalData[1];
		Clear();
		Type type = InspectorScriptValueType;
		if (IsCollection)
		{
			type = type.MakeArrayType();
		}
		if (base.VerboseLogging)
		{
			DebugUtility.Log("typeToSpawn: " + type.Name, this);
		}
		object obj = MonoBehaviourSingleton<UIDynamicTypeFactory>.Instance.Create(type);
		Dictionary<Type, Enum> dictionary = new Dictionary<Type, Enum>(typeStyleOverrideDictionary);
		if (typeStyleOverrideDictionary.TryGetValue(type, out var value))
		{
			Presentable = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithStyle(obj, value, defaultValueContainer, dictionary);
		}
		else
		{
			Presentable = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithDefaultStyle(obj, defaultValueContainer, dictionary);
		}
		InterfaceReference<IUIChildLayoutable>[] array = defaultValueContainerLayoutables;
		foreach (InterfaceReference<IUIChildLayoutable> obj2 in array)
		{
			obj2.Interface.CollectChildLayoutItems();
			obj2.Interface.RequestLayout();
		}
		if (Presentable is UIIEnumerablePresenter uIIEnumerablePresenter)
		{
			uIIEnumerablePresenter.SetElementType(IsCollection ? type.GetElementType() : type);
			UIBaseIEnumerableView obj3 = uIIEnumerablePresenter.View.Interface as UIBaseIEnumerableView;
			obj3.SetDisplayEnumLabels(displayEnumLabels: false);
			obj3.LayoutElement.FlexibleHeightLayoutDimension.ExplicitValue = 1f;
			uIIEnumerablePresenter.IEnumerableView.SetCanAddAndRemoveItems(canAddAndRemoveItems: true);
			uIIEnumerablePresenter.SetModel((IEnumerable)obj, triggerOnModelChanged: true);
			obj3.SetInteractable(interactable: true);
		}
		if (Presentable.Viewable is UIEnumDropdownView uIEnumDropdownView)
		{
			uIEnumDropdownView.SetLabel(string.Empty);
		}
		LayoutRebuilder.MarkLayoutForRebuild(defaultValueContainer);
	}

	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}

	private void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		nameInputField.Clear();
		descriptionInputField.Clear();
		groupNameInputField.Clear();
		hideToggle.SetIsOn(state: false, suppressOnChange: true);
		if (Presentable != null)
		{
			Presentable.ReturnToPool();
			Presentable = null;
		}
	}
}
