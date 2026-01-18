using System;
using System.Collections;
using System.Threading.Tasks;
using Endless.Gameplay.Scripting;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIScriptWindowController : UIWindowController, IUILoadingSpinnerViewCompatible
{
	private struct TextBehindResult
	{
		public readonly string Text;

		public readonly int StartIndex;

		public readonly char? AccessOperator;

		public bool HasAccessOperator
		{
			get
			{
				switch (AccessOperator)
				{
				case '.':
				case ':':
					return true;
				default:
					return false;
				}
			}
		}

		public TextBehindResult(string text, int startIndex, char? accessOperator)
		{
			Text = text;
			StartIndex = startIndex;
			AccessOperator = accessOperator;
		}

		public override string ToString()
		{
			return string.Format("| {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7} |", "Text", Text, "AccessOperator", AccessOperator, "StartIndex", StartIndex, "HasAccessOperator", HasAccessOperator);
		}
	}

	private const string TAB = "\t";

	[Header("UIScriptWindowController")]
	[SerializeField]
	private UIScriptWindowModel model;

	[SerializeField]
	private UISaveScriptAndPropAndApplyToGameHandler saveScriptAndPropAndApplyToGameHandler;

	[SerializeField]
	private UIButton saveScriptAndPropAndApplyToGameButton;

	[SerializeField]
	private UIButton revertButton;

	[SerializeField]
	private UIToggle openSourceToggle;

	[SerializeField]
	private UIButton createNewButton;

	[SerializeField]
	private StringVariable endstarLuaDocumentationURL;

	[SerializeField]
	private UIButton showDocumentationButton;

	[SerializeField]
	private UIButton showConsoleButton;

	[SerializeField]
	private UIScriptInputField scriptInputField;

	[Header("Close Confirm Save Colors")]
	[SerializeField]
	private UnityEngine.Color saveButtonColor = UnityEngine.Color.white;

	[SerializeField]
	private UnityEngine.Color dontSaveButtonColor = UnityEngine.Color.white;

	[SerializeField]
	private UnityEngine.Color cancelButtonColor = UnityEngine.Color.white;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	private void Awake()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Awake");
		}
		scriptInputField.OnTabInputUnityEvent.AddListener(OnTabInput);
		scriptInputField.OnNewLineInputUnityEvent.AddListener(OnEnterInput);
	}

	protected override void Start()
	{
		base.Start();
		saveScriptAndPropAndApplyToGameHandler.OnLoadingStarted.AddListener(OnScriptSavingStarted);
		saveScriptAndPropAndApplyToGameHandler.OnLoadingEnded.AddListener(OnScriptSavingEnded);
		scriptInputField.onValueChanged.AddListener(model.SetScriptBody);
		scriptInputField.CaretPositionChangedUnityEvent.AddListener(UpdateAutoComplete);
		saveScriptAndPropAndApplyToGameButton.onClick.AddListener(SaveScriptAndPropAndApplyToGame);
		revertButton.onClick.AddListener(model.Revert);
		openSourceToggle.OnChange.AddListener(model.SetOpenSource);
		createNewButton.onClick.AddListener(ViewNewScriptDataModal);
		showDocumentationButton.onClick.AddListener(OpenEndstarLuaDocumentation);
		showConsoleButton.onClick.AddListener(OnShowConsoleButtonClicked);
	}

	private void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		UIUserScriptAutocompleteListCellController.OnSelect = (Action<string>)Delegate.Combine(UIUserScriptAutocompleteListCellController.OnSelect, new Action<string>(AutocompleteByClick));
	}

	private void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		UIUserScriptAutocompleteListCellController.OnSelect = (Action<string>)Delegate.Remove(UIUserScriptAutocompleteListCellController.OnSelect, new Action<string>(AutocompleteByClick));
	}

	private void OnTabInput()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnTabInput");
		}
		if (model.AutoComplete.Count != 0)
		{
			AutocompleteToFirstItem();
		}
	}

	private void OnEnterInput()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnterInput");
		}
		if (model.AutoComplete.Count != 0)
		{
			AutocompleteToFirstItem();
		}
	}

	private void OpenEndstarLuaDocumentation()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OpenEndstarLuaDocumentation");
		}
		Application.OpenURL(endstarLuaDocumentationURL.Value);
	}

	private void OnShowConsoleButtonClicked()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnShowConsoleButtonClicked");
		}
		UIConsoleWindowView.Display(NetworkBehaviourSingleton<UserScriptingConsole>.Instance.GetConsoleMessagesForAssetId(model.PropId), ConsoleScope.Asset, model.PropId, null);
	}

	private void ViewNewScriptDataModal()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewNewScriptDataModal");
		}
		Prop cloneOfProp = model.GetCloneOfProp();
		Script cloneOfScript = model.GetCloneOfScript();
		MonoBehaviourSingleton<UIScriptDataWizard>.Instance.Initialize(cloneOfProp, cloneOfScript);
	}

	private void AutocompleteToFirstItem()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "AutocompleteToFirstItem");
		}
		string value = model.AutoComplete[0].Value;
		Autocomplete(value, inputWasClick: false);
	}

	private void AutocompleteByClick(string autoCompleteTo)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "AutocompleteByClick", autoCompleteTo);
		}
		Autocomplete(autoCompleteTo, inputWasClick: true);
	}

	private void Autocomplete(string autoCompleteTo, bool inputWasClick)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Autocomplete", autoCompleteTo, inputWasClick);
		}
		TextBehindResult textBehind = GetTextBehind(scriptInputField.caretPosition);
		int num = textBehind.StartIndex + 1 + autoCompleteTo.Length;
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}", "StartIndex", textBehind.StartIndex, "newCaretPosition", num), this);
		}
		string text = scriptInputField.text.Remove(textBehind.StartIndex + 1, scriptInputField.caretPosition - textBehind.StartIndex - 1);
		text = text.Insert(textBehind.StartIndex + 1, autoCompleteTo);
		scriptInputField.text = text;
		model.UpdateAutoComplete(null, null, null);
		if (inputWasClick)
		{
			scriptInputField.Select();
			StartCoroutine(WaitForEndOfFrameAndSetCaretPosition(num));
		}
		else
		{
			scriptInputField.SetCaretPositionWithoutNotify(num);
			scriptInputField.Rebuild(CanvasUpdate.LatePreRender);
		}
	}

	private IEnumerator WaitForEndOfFrameAndSetCaretPosition(int caretPosition)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "WaitForEndOfFrameAndSetCaretPosition", caretPosition);
		}
		yield return new WaitForEndOfFrame();
		scriptInputField.SetCaretPositionWithoutNotify(caretPosition);
		scriptInputField.Rebuild(CanvasUpdate.LatePreRender);
	}

	public void SaveScriptAndPropAndApplyToGame()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SaveScriptAndPropAndApplyToGame");
		}
		SaveScriptAndPropAndApplyToGameAsync();
	}

	private async Task SaveScriptAndPropAndApplyToGameAsync()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SaveScriptAndPropAndApplyToGameAsync");
		}
		OnLoadingStarted.Invoke();
		Script cloneOfScript = model.GetCloneOfScript();
		Prop cloneOfProp = model.GetCloneOfProp();
		UISaveScriptAndPropAndApplyToGameHandler.Result result = await saveScriptAndPropAndApplyToGameHandler.SaveScriptAndPropAndApplyToGame(cloneOfScript, cloneOfProp);
		OnLoadingEnded.Invoke();
		model.ApplySaveScriptAndPropAndApplyToGameResult(result);
	}

	private void UpdateAutoComplete(int newCaretPosition)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateAutoComplete", newCaretPosition);
		}
		if (newCaretPosition < scriptInputField.text.Length && !char.IsWhiteSpace(scriptInputField.text[newCaretPosition]))
		{
			model.UpdateAutoComplete(null, null, null);
			return;
		}
		TextBehindResult textBehind = GetTextBehind(newCaretPosition);
		string filter = null;
		if (textBehind.HasAccessOperator)
		{
			filter = GetTextBehind(textBehind.StartIndex).Text;
		}
		model.UpdateAutoComplete(textBehind.Text, filter, textBehind.AccessOperator);
	}

	private TextBehindResult GetTextBehind(int index)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetTextBehind", index);
		}
		string text = string.Empty;
		char? accessOperator = null;
		while (index > 0)
		{
			index--;
			char c = scriptInputField.text[index];
			int num;
			if (c != '.')
			{
				num = ((c == ':') ? 1 : 0);
				if (num == 0)
				{
					goto IL_0061;
				}
			}
			else
			{
				num = 1;
			}
			accessOperator = c;
			goto IL_0061;
			IL_0061:
			if (num != 0 || char.IsWhiteSpace(c) || c == '(' || c == ')')
			{
				break;
			}
			text = text.Insert(0, c.ToString());
		}
		return new TextBehindResult(text, index, accessOperator);
	}

	private void OnScriptSavingStarted()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnScriptSavingStarted");
		}
		OnLoadingStarted.Invoke();
	}

	private void OnScriptSavingEnded()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnScriptSavingEnded");
		}
		OnLoadingEnded.Invoke();
	}

	public void AutoCompleteEscaped()
	{
		scriptInputField.ActivateInputField();
	}
}
