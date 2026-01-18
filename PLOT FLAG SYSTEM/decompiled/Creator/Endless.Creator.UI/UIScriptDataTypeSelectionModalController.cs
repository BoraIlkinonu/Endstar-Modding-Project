using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScriptDataTypeSelectionModalController : UIGameObject
{
	[SerializeField]
	private UIScriptDataTypeSelectionModalView view;

	[SerializeField]
	private UIScriptDataTypeRadio scriptDataTypeRadio;

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
		scriptDataTypeRadio.OnValueChanged.AddListener(view.ViewScriptDataTypeInfo);
		continueButton.onClick.AddListener(Continue);
	}

	private void Continue()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Continue");
		}
		MonoBehaviourSingleton<UIScriptDataWizard>.Instance.SetScriptDataType(scriptDataTypeRadio.Value);
	}
}
