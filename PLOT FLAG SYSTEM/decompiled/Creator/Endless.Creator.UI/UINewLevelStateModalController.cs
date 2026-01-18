using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UINewLevelStateModalController : UIGameObject
{
	public static Action<string, string, LevelStateTemplateSourceBase> CreateLevel;

	[SerializeField]
	private UIInputField nameInputField;

	[SerializeField]
	private UIInputField descriptionInputField;

	[SerializeField]
	private UILevelStateTemplateListModel levelStateTemplateListModel;

	[SerializeField]
	private UIButton createButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		nameInputField.onValueChanged.AddListener(HandleCreateButtonInteractability);
		createButton.onClick.AddListener(Create);
	}

	private void HandleCreateButtonInteractability(string inputValue)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleCreateButtonInteractability", inputValue);
		}
		createButton.interactable = !nameInputField.IsNullOrEmptyOrWhiteSpace(playInvalidInputTweensIfSo: false);
	}

	private void Create()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Create");
		}
		if (!nameInputField.IsNullOrEmptyOrWhiteSpace())
		{
			string text = nameInputField.text;
			string text2 = descriptionInputField.text;
			LevelStateTemplateSourceBase arg = levelStateTemplateListModel.SelectedTypedList[0];
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			CreateLevel?.Invoke(text, text2, arg);
		}
	}
}
