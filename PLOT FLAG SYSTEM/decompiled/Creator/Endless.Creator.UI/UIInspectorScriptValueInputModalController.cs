using System;
using Endless.Gameplay.Serialization;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInspectorScriptValueInputModalController : UIGameObject
{
	[SerializeField]
	private UIInspectorScriptValueInputModalView view;

	[SerializeField]
	private UIInputField nameInputField;

	[SerializeField]
	private UIInputField descriptionInputField;

	[SerializeField]
	private UIInputField groupNameInputField;

	[SerializeField]
	private UIToggle hideToggle;

	[SerializeField]
	private UIButton createInspectorScriptValueButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private Type InspectorScriptValueType => view.InspectorScriptValueType;

	private bool IsCollection => view.IsCollection;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		createInspectorScriptValueButton.onClick.AddListener(CreateInspectorScriptValueFromInput);
	}

	private bool ValidateInspectorScriptValue()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ValidateInspectorScriptValue");
		}
		bool result = true;
		if (!MonoBehaviourSingleton<UIScriptDataWizard>.Instance.IsValidScriptingName(nameInputField.text))
		{
			nameInputField.PlayInvalidInputTweens();
			result = false;
		}
		foreach (InspectorScriptValue inspectorValue in MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript().InspectorValues)
		{
			if (!(inspectorValue.Name != nameInputField.text))
			{
				nameInputField.PlayInvalidInputTweens();
				result = false;
				break;
			}
		}
		return result;
	}

	private void CreateInspectorScriptValueFromInput()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateInspectorScriptValueFromInput");
		}
		if (!ValidateInspectorScriptValue())
		{
			return;
		}
		string text = nameInputField.text;
		object defaultValueAsObject = GetDefaultValueAsObject();
		string text2 = JsonConvert.SerializeObject(defaultValueAsObject);
		Type type = defaultValueAsObject.GetType();
		if (InspectorScriptValueType.IsEnum)
		{
			type = InspectorScriptValueType;
			if (IsCollection)
			{
				type = type.MakeArrayType();
			}
		}
		else if (IsCollection)
		{
			type = InspectorScriptValueType.MakeArrayType();
		}
		string text3 = descriptionInputField.text;
		if (verboseLogging)
		{
			DebugUtility.Log("memberName: " + text, this);
			DebugUtility.Log(string.Format("{0}: {1}", "InspectorScriptValueType", InspectorScriptValueType), this);
			DebugUtility.Log("type: " + type.Name, this);
			DebugUtility.Log("defaultValue: " + text2, this);
		}
		CreateInspectorScriptValue(type, text, text3, text2, exitAfterUpdateScript: true);
	}

	private object GetDefaultValueAsObject()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetDefaultValueAsObject");
		}
		object modelAsObject = view.Presentable.ModelAsObject;
		Type inspectorScriptValueType = view.InspectorScriptValueType;
		if (verboseLogging)
		{
			DebugUtility.Log("model: " + JsonConvert.SerializeObject(modelAsObject), this);
			DebugUtility.Log("valueType: " + inspectorScriptValueType.Name, this);
		}
		return modelAsObject;
	}

	private void CreateInspectorScriptValue(Type type, string memberName, string description, string defaultValue, bool exitAfterUpdateScript)
	{
		if (verboseLogging || Application.isEditor)
		{
			DebugUtility.LogMethod(this, "CreateInspectorScriptValue", type.Name, memberName, description, defaultValue, exitAfterUpdateScript);
		}
		int typeId = EndlessTypeMapping.Instance.GetTypeId(type.AssemblyQualifiedName);
		InspectorScriptValue inspectorScriptValue = new InspectorScriptValue(memberName, string.Empty, typeId, description, defaultValue, Array.Empty<ClampValue>());
		if (verboseLogging || Application.isEditor)
		{
			DebugUtility.Log("inspectorScriptValue: " + JsonConvert.SerializeObject(inspectorScriptValue), this);
		}
		Script cloneOfScript = MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript();
		cloneOfScript.InspectorValues.Add(inspectorScriptValue);
		InspectorOrganizationData item = new InspectorOrganizationData(typeId, memberName, -1, groupNameInputField.text, hideToggle.IsOn);
		cloneOfScript.InspectorOrganizationData.Add(item);
		if (exitAfterUpdateScript)
		{
			MonoBehaviourSingleton<UIScriptDataWizard>.Instance.UpdateScriptAndExit(cloneOfScript);
		}
		else
		{
			MonoBehaviourSingleton<UIScriptDataWizard>.Instance.UpdateScript(cloneOfScript);
		}
	}
}
