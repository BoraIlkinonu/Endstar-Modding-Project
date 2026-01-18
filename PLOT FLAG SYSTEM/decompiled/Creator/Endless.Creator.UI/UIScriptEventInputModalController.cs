using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.Serialization;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScriptEventInputModalController : UIGameObject
{
	[SerializeField]
	private UIScriptEventInputModalView view;

	[SerializeField]
	private IntVariable endlessParameterInfoLimit;

	[SerializeField]
	private UIInputField memberNameInputField;

	[SerializeField]
	private UIInputField descriptionInputField;

	[Header("EndlessParameterInfo Creation")]
	[SerializeField]
	private UIEndlessParameterInfoListModel endlessParameterInfoListModel;

	[SerializeField]
	private UIInputField endlessParameterInfoDisplayNameInputField;

	[SerializeField]
	private UIInspectorScriptValueTypeRadio endlessParameterInfoDataTypeRadio;

	[SerializeField]
	private UIToggle isCollectionToggle;

	[SerializeField]
	private UIButton createEndlessParameterButton;

	[SerializeField]
	private UIButton continueButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		createEndlessParameterButton.onClick.AddListener(CreateEndlessParameter);
		continueButton.onClick.AddListener(CreateEndlessEventInfo);
	}

	private bool ValidateEndlessParameter()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ValidateEndlessParameter");
		}
		bool result = true;
		if (endlessParameterInfoListModel.Count >= endlessParameterInfoLimit.Value)
		{
			result = false;
		}
		if (!MonoBehaviourSingleton<UIScriptDataWizard>.Instance.IsValidScriptingName(endlessParameterInfoDisplayNameInputField.text))
		{
			endlessParameterInfoDisplayNameInputField.PlayInvalidInputTweens();
			result = false;
		}
		foreach (EndlessParameterInfo readOnly in endlessParameterInfoListModel.ReadOnlyList)
		{
			if (readOnly.DisplayName == endlessParameterInfoDisplayNameInputField.text)
			{
				endlessParameterInfoDisplayNameInputField.PlayInvalidInputTweens();
				result = false;
			}
		}
		return result;
	}

	private void CreateEndlessParameter()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateEndlessParameter");
		}
		if (ValidateEndlessParameter())
		{
			Type type = endlessParameterInfoDataTypeRadio.Value;
			if (isCollectionToggle.IsOn)
			{
				type = type.MakeArrayType();
			}
			EndlessParameterInfo item = new EndlessParameterInfo(EndlessTypeMapping.Instance.GetTypeId(type.AssemblyQualifiedName), endlessParameterInfoDisplayNameInputField.text);
			endlessParameterInfoListModel.Add(item, triggerEvents: true);
			endlessParameterInfoDisplayNameInputField.Clear();
		}
	}

	private bool ValidateEndlessEventInfo()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ValidateEndlessEventInfo");
		}
		bool result = true;
		if (!MonoBehaviourSingleton<UIScriptDataWizard>.Instance.IsValidScriptingName(memberNameInputField.text))
		{
			memberNameInputField.PlayInvalidInputTweens();
			result = false;
		}
		Script cloneOfScript = MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript();
		foreach (EndlessEventInfo item in view.IsEvent ? cloneOfScript.Events : cloneOfScript.Receivers)
		{
			if (item.MemberName == memberNameInputField.text)
			{
				memberNameInputField.PlayInvalidInputTweens();
				result = false;
			}
		}
		return result;
	}

	private void CreateEndlessEventInfo()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateEndlessEventInfo");
		}
		if (ValidateEndlessEventInfo())
		{
			List<EndlessParameterInfo> paramList = endlessParameterInfoListModel.ReadOnlyList.ToList();
			EndlessEventInfo item = new EndlessEventInfo(memberNameInputField.text, paramList)
			{
				Description = descriptionInputField.text
			};
			Script cloneOfScript = MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript();
			if (view.IsEvent)
			{
				cloneOfScript.Events.Add(item);
			}
			else
			{
				cloneOfScript.Receivers.Add(item);
			}
			MonoBehaviourSingleton<UIScriptDataWizard>.Instance.UpdateScriptAndExit(cloneOfScript);
		}
	}
}
