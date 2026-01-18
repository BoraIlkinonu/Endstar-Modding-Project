using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.Serialization;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInspectorScriptValueTypeSelectionModalView : UIScriptModalView
{
	[Header("UIInspectorScriptValueTypeSelectionModalView")]
	[SerializeField]
	private InspectorScriptValueTypeInfoDictionary inspectorScriptValueTypeInfoDictionary;

	[SerializeField]
	private UIInspectorScriptValueTypeRadio inspectorScriptValueTypeRadio;

	[SerializeField]
	private UIToggle isCollectionToggle;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	private readonly HashSet<Type> nonCollectionSupported = new HashSet<Type> { typeof(NpcClassCustomizationData) };

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		inspectorScriptValueTypeRadio.SetDefaultValue(EndlessTypeMapping.Instance.LuaInspectorTypes[0]);
		inspectorScriptValueTypeRadio.SetValueToDefault(triggerOnValueChanged: true);
		isCollectionToggle.SetIsOn(state: false, suppressOnChange: false);
	}

	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
	}

	public void ViewInspectorScriptValueTypeInfo(Type inspectorScriptValueType)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewInspectorScriptValueTypeInfo", inspectorScriptValueType);
		}
		DisplayNameAndDescription displayNameAndDescription = inspectorScriptValueTypeInfoDictionary[inspectorScriptValueType.Name];
		displayNameText.text = displayNameAndDescription.DisplayName;
		descriptionText.text = displayNameAndDescription.Description;
	}

	public void HandleIsCollectionToggleVisibility(Type inspectorScriptValueType)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleIsCollectionToggleVisibility", inspectorScriptValueType);
		}
		Type value = inspectorScriptValueTypeRadio.Value;
		bool flag = !nonCollectionSupported.Contains(value);
		isCollectionToggle.gameObject.SetActive(flag);
		if (!flag)
		{
			isCollectionToggle.SetIsOn(state: false, suppressOnChange: false);
		}
	}
}
