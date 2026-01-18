using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UINoneOrContextRadioButton : UIBaseRadioButton<NoneOrContext>
{
	[Header("UIScriptDataTypeRadioButton")]
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	public override void Initialize(UIBaseRadio<NoneOrContext> radio, NoneOrContext value)
	{
		base.Initialize(radio, value);
		displayNameText.text = value.ToString();
	}
}
