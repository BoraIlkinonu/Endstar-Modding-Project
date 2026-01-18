using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInspectorScriptValueTypeSelectionModalController : UIGameObject
{
	[SerializeField]
	private UIInspectorScriptValueTypeSelectionModalView view;

	[SerializeField]
	private UIInspectorScriptValueTypeRadio inspectorScriptValueTypeRadio;

	[SerializeField]
	private UIToggle isCollectionToggle;

	[SerializeField]
	private UIButton continueButton;

	[SerializeField]
	private UIInspectorScriptValueInputModalView inspectorScriptValueInputModalSource;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Awake()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Awake");
		}
		inspectorScriptValueTypeRadio.OnValueChanged.AddListener(view.ViewInspectorScriptValueTypeInfo);
		inspectorScriptValueTypeRadio.OnValueChanged.AddListener(view.HandleIsCollectionToggleVisibility);
		continueButton.onClick.AddListener(Continue);
	}

	private void Continue()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Continue");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(inspectorScriptValueInputModalSource, UIModalManagerStackActions.MaintainStack, inspectorScriptValueTypeRadio.Value, isCollectionToggle.IsOn);
	}
}
