using System;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInspectorScriptValueTypeRadioButton : UIBaseRadioButton<Type>
{
	[Header("UIInspectorScriptValueTypeRadioButton")]
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private InspectorScriptValueTypeInfoDictionary inspectorScriptValueTypeInfoDictionary;

	public override void Initialize(UIBaseRadio<Type> radio, Type value)
	{
		base.Initialize(radio, value);
		DisplayNameAndDescription displayNameAndDescription = inspectorScriptValueTypeInfoDictionary[value.Name];
		displayNameText.text = displayNameAndDescription.DisplayName;
	}
}
