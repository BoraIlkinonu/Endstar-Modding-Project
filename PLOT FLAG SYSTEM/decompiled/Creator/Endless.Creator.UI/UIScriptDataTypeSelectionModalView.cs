using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScriptDataTypeSelectionModalView : UIScriptModalView
{
	[Header("UIScriptDataTypeSelectionModalView")]
	[SerializeField]
	private ScriptDataTypeInfoDictionary scriptDataTypeInfoDictionary;

	[SerializeField]
	private UIScriptDataTypeRadio scriptDataTypeRadio;

	[SerializeField]
	private ScriptDataTypes scriptDataTypeRadioDefaultValue;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		scriptDataTypeRadio.SetValue(scriptDataTypeRadioDefaultValue, triggerOnValueChanged: true);
		ViewScriptDataTypeInfo(scriptDataTypeRadioDefaultValue);
	}

	public void ViewScriptDataTypeInfo(ScriptDataTypes scriptDataType)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewScriptDataTypeInfo", scriptDataType);
		}
		DisplayNameAndDescription displayNameAndDescription = scriptDataTypeInfoDictionary[scriptDataType];
		displayNameText.text = displayNameAndDescription.DisplayName;
		descriptionText.text = displayNameAndDescription.Description;
	}
}
