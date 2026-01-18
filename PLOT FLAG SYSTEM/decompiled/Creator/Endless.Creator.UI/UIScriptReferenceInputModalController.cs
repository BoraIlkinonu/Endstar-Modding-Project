using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScriptReferenceInputModalController : UIGameObject
{
	[Header("UIScriptReferenceInputModalController")]
	[SerializeField]
	private UIInputField nameInCodeInputField;

	[SerializeField]
	private UITransformIdentifierListModel transformIdentifierListModel;

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
		continueButton.onClick.AddListener(CreateScriptReference);
	}

	private bool ValidateScriptReference()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ValidateScriptReference");
		}
		bool result = true;
		if (!MonoBehaviourSingleton<UIScriptDataWizard>.Instance.IsValidScriptingName(nameInCodeInputField.text))
		{
			nameInCodeInputField.PlayInvalidInputTweens();
			result = false;
		}
		foreach (ScriptReference scriptReference in MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript().ScriptReferences)
		{
			if (!(scriptReference.NameInCode != nameInCodeInputField.text))
			{
				nameInCodeInputField.PlayInvalidInputTweens();
				result = false;
			}
		}
		return result;
	}

	private void CreateScriptReference()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateScriptReference");
		}
		if (ValidateScriptReference() && transformIdentifierListModel.SelectedTypedList.Count != 0)
		{
			UITransformIdentifier uITransformIdentifier = transformIdentifierListModel.SelectedTypedList[0];
			ScriptReference item = new ScriptReference(nameInCodeInputField.text, uITransformIdentifier.TransformIdentifier.UniqueId, ScriptReference.ReferenceType.Transform);
			Script cloneOfScript = MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript();
			cloneOfScript.ScriptReferences.Add(item);
			MonoBehaviourSingleton<UIScriptDataWizard>.Instance.UpdateScriptAndExit(cloneOfScript);
		}
	}
}
