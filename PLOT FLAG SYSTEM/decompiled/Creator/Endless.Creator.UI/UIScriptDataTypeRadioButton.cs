using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScriptDataTypeRadioButton : UIBaseRadioButton<ScriptDataTypes>
{
	[Header("UIScriptDataTypeRadioButton")]
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private ScriptDataTypeInfoDictionary scriptDataTypeInfoDictionary;

	public override void Initialize(UIBaseRadio<ScriptDataTypes> radio, ScriptDataTypes value)
	{
		base.Initialize(radio, value);
		DisplayNameAndDescription displayNameAndDescription = scriptDataTypeInfoDictionary[value];
		displayNameText.text = displayNameAndDescription.DisplayName;
	}
}
