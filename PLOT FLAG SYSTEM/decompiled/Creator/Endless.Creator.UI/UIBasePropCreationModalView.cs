using Endless.Creator.DynamicPropCreation;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public abstract class UIBasePropCreationModalView : UIBaseModalView
{
	[Header("UIBasePropCreationModalView")]
	[SerializeField]
	private TextMeshProUGUI typeName;

	[SerializeField]
	private UIInputField nameInputField;

	[SerializeField]
	private UIInputField descriptionInputField;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private UIToggle grantEditRightsToCollaboratorsToggle;

	protected override void Start()
	{
		base.Start();
		nameInputField.onValueChanged.AddListener(SetNameText);
	}

	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnBack", this);
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		PropCreationScreenData propCreationScreenData = (PropCreationScreenData)modalData[0];
		typeName.text = propCreationScreenData.DisplayName;
		nameInputField.text = propCreationScreenData.DefaultName;
		descriptionInputField.text = propCreationScreenData.DefaultDescription;
		nameText.text = propCreationScreenData.DefaultName;
		grantEditRightsToCollaboratorsToggle.SetIsOn(state: false, suppressOnChange: false);
	}

	private void SetNameText(string newName)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("SetNameText ( newName: " + newName + " )", this);
		}
		nameText.text = newName;
	}
}
