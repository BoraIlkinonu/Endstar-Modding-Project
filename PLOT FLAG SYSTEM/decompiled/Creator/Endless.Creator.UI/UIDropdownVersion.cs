using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIDropdownVersion : UIGameObject
{
	[SerializeField]
	private UIText valueText;

	[SerializeField]
	private string noValueText = string.Empty;

	[SerializeField]
	private UIButton toggleOptionsButton;

	[SerializeField]
	private RectTransform optionsContainer;

	[SerializeField]
	private Canvas optionsContainerCanvas;

	[SerializeField]
	private UIVersionView.Styles style;

	[SerializeField]
	private UIIEnumerablePresenter iEnumerablePresenter;

	[SerializeField]
	private UIIEnumerableStraightVirtualizedView iEnumerableView;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIVersion[] versions = Array.Empty<UIVersion>();

	private Dictionary<Type, Enum> typeStyleOverrideDictionary = new Dictionary<Type, Enum>();

	[field: SerializeField]
	public UnityEvent OnOptionsDisplayed { get; set; } = new UnityEvent();

	[field: SerializeField]
	public UnityEvent OnOptionsHidden { get; set; } = new UnityEvent();

	[field: SerializeField]
	public UnityEvent OnValueChanged { get; set; } = new UnityEvent();

	public string Value
	{
		get
		{
			if (iEnumerablePresenter.SelectedItemsList.Count != 1)
			{
				return string.Empty;
			}
			return ((UIVersion)iEnumerablePresenter.SelectedItemsList[0]).Version;
		}
	}

	public int IndexOfValue
	{
		get
		{
			if (iEnumerablePresenter.SelectedItemsList.Count != 1)
			{
				Debug.LogError(string.Format("{0}.{1} is expected to have a Count of 1! Instead it has a Count of {2}!", "iEnumerablePresenter", "SelectedItemsList", iEnumerablePresenter.SelectedItemsList.Count), this);
				return -1;
			}
			object item = iEnumerablePresenter.SelectedItemsList[0];
			return iEnumerablePresenter.ModelList.IndexOf(item);
		}
	}

	public string UserFacingVersion
	{
		get
		{
			if (iEnumerablePresenter.SelectedItemsList.Count != 1)
			{
				return noValueText;
			}
			return ((UIVersion)iEnumerablePresenter.SelectedItemsList[0]).UserFacingVersion;
		}
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		toggleOptionsButton.onClick.AddListener(ToggleOptionsVisibility);
		iEnumerablePresenter.OnSelectionChanged += OnItemSelected;
		HideDropdownOptions();
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		HideDropdownOptions();
	}

	private void OnDestroy()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		toggleOptionsButton.onClick.RemoveListener(ToggleOptionsVisibility);
		iEnumerablePresenter.OnSelectionChanged -= OnItemSelected;
	}

	public void SetOptionsAndValue(IEnumerable<string> newOptions, string newValue, bool triggerOnValueChanged)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetOptionsAndValue", newOptions, newValue, triggerOnValueChanged);
		}
		string[] array = ((newOptions == null) ? Array.Empty<string>() : newOptions.ToArray());
		if (verboseLogging)
		{
			DebugUtility.DebugEnumerable("newOptionsArray", array, this);
		}
		int num = -1;
		UIVersion[] array2;
		if (array.Length == 0)
		{
			array2 = Array.Empty<UIVersion>();
		}
		else
		{
			array2 = new UIVersion[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == newValue)
				{
					num = i;
				}
				array2[i] = new UIVersion(array[i]);
			}
		}
		versions = array2;
		if (typeStyleOverrideDictionary.Count == 0)
		{
			typeStyleOverrideDictionary = new Dictionary<Type, Enum> { 
			{
				typeof(UIVersion),
				style
			} };
		}
		iEnumerableView.SetTypeStyleOverrideDictionary(typeStyleOverrideDictionary);
		iEnumerablePresenter.SetModel(versions, triggerOnModelChanged: false);
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "indexOfValue", num), this);
		}
		if (num >= 0)
		{
			iEnumerablePresenter.SetSelected(new List<object> { iEnumerablePresenter.ModelList[num] }, updateSelectionVisuals: true, invokeOnModelChangedAndOnSelectionChanged: false);
			UpdateValueText();
		}
		else if (versions.Length != 0)
		{
			num = 0;
			iEnumerablePresenter.SetSelected(new List<object> { iEnumerablePresenter.ModelList[num] }, updateSelectionVisuals: true, invokeOnModelChangedAndOnSelectionChanged: false);
			UpdateValueText();
		}
		else
		{
			valueText.Value = noValueText;
		}
		if (triggerOnValueChanged)
		{
			OnValueChanged?.Invoke();
		}
	}

	public void SetValue(string newValue, bool triggerOnValueChanged)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetValue", newValue, triggerOnValueChanged);
		}
		if (versions == null)
		{
			Debug.LogError("Cannot set value when versions is null", this);
			return;
		}
		int num = -1;
		for (int i = 0; i < versions.Length; i++)
		{
			if (!(versions[i].Version != newValue))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			Debug.LogError("newValue '" + newValue + "' not found in versions array", this);
			return;
		}
		iEnumerablePresenter.SetSelected(new List<object> { iEnumerablePresenter.ModelList[num] }, updateSelectionVisuals: true, invokeOnModelChangedAndOnSelectionChanged: false);
		UpdateValueText();
		if (triggerOnValueChanged)
		{
			OnValueChanged?.Invoke();
		}
	}

	public void SetValueText(string newValueText)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetValueText", newValueText);
		}
		valueText.Value = newValueText;
	}

	public void SetIsInteractable(bool interactable)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetIsInteractable", interactable);
		}
		toggleOptionsButton.interactable = interactable;
	}

	private void UpdateValueText()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateValueText");
		}
		valueText.Value = UserFacingVersion;
	}

	private void ToggleOptionsVisibility()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleOptionsVisibility");
		}
		bool activeSelf = optionsContainer.gameObject.activeSelf;
		optionsContainer.gameObject.SetActive(!activeSelf);
		if (optionsContainer.gameObject.activeSelf)
		{
			MonoBehaviourSingleton<UICoroutineManager>.Instance.WaitFramesAndInvoke(iEnumerableView.ReloadDataAndKeepPosition);
			Canvas componentInParent = base.gameObject.GetComponentInParent<Canvas>();
			optionsContainerCanvas.sortingOrder = componentInParent.sortingOrder + 1;
			OnOptionsDisplayed.Invoke();
		}
		else
		{
			OnOptionsHidden.Invoke();
		}
	}

	private void OnItemSelected(object item)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnItemSelected", item);
		}
		UpdateValueText();
		HideDropdownOptions();
		OnValueChanged?.Invoke();
	}

	private void HideDropdownOptions()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HideDropdownOptions");
		}
		optionsContainer.gameObject.SetActive(value: false);
		OnOptionsHidden.Invoke();
	}
}
