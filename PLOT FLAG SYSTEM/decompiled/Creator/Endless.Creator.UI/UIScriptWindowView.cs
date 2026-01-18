using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Creator.Test.LuaParsing;
using Endless.Data.UI;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIScriptWindowView : UIBaseWindowView
{
	[SerializeField]
	private TextMeshProUGUI propNameText;

	[SerializeField]
	private UIScriptWindowController controller;

	[SerializeField]
	private GameObject readOnlyFrame;

	[SerializeField]
	private UIButton saveButton;

	[SerializeField]
	private UIButton revertButton;

	[SerializeField]
	private TextMeshProUGUI errorCountText;

	[SerializeField]
	private UIToggle openSourceToggle;

	[SerializeField]
	private UIButton createNewScriptDataButton;

	[SerializeField]
	private UIEnumEntryListModel enumEntryListModel;

	[SerializeField]
	private UIInspectorScriptValueListModel inspectorScriptValueListModel;

	[SerializeField]
	private UIEndlessEventInfoListModel wiringReceiverListModel;

	[SerializeField]
	private UIEndlessEventInfoListModel wiringEventListModel;

	[SerializeField]
	private UIScriptReferenceListModel scriptReferenceListModel;

	[SerializeField]
	private UIInputField scriptInputField;

	[SerializeField]
	private RectTransform textRectTransform;

	[SerializeField]
	private Scrollbar scriptInputFieldScrollbar;

	[SerializeField]
	private TextMeshProUGUI lineNumberText;

	[SerializeField]
	private TextMeshProUGUI styledUserScriptText;

	[SerializeField]
	private UITokenGroupTypeColorDictionary tokenTypeColorDictionary;

	[SerializeField]
	private Image lineHighlightImage;

	[SerializeField]
	private RectTransform autocompleteContainer;

	[SerializeField]
	private UIUserScriptAutocompleteListModel userScriptAutocompleteListModel;

	[Header("Debugging")]
	[SerializeField]
	private bool superVerboseLogging;

	private bool started;

	private Coroutine checkForTextChangesCoroutine;

	private bool readOnly;

	[field: Header("UIScriptWindowView")]
	[field: SerializeField]
	public UIScriptWindowModel Model { get; private set; }

	public UnityEvent<SerializableGuid> OnClosedUnityEvent { get; } = new UnityEvent<SerializableGuid>();

	protected override void Start()
	{
		base.Start();
		Model.OnModelChanged.AddListener(View);
		userScriptAutocompleteListModel.ModelChangedUnityEvent.AddListener(PlaceAutoCompleteContainer);
		scriptInputField.onSelect.AddListener(OnSelect);
		scriptInputField.onDeselect.AddListener(OnDeselect);
		scriptInputField.onTextSelection.AddListener(OnTextSelection);
		scriptInputField.onEndTextSelection.AddListener(OnEndTextSelection);
		lineHighlightImage.transform.SetParent(scriptInputField.textComponent.transform, worldPositionStays: true);
		lineHighlightImage.color = tokenTypeColorDictionary.LineHighlightColor;
		autocompleteContainer.SetParent(scriptInputField.textComponent.transform, worldPositionStays: true);
		started = true;
	}

	public static UIScriptWindowView Display(Script script, PropLibrary.RuntimePropInfo runtimePropInfo, bool readOnly, Transform parent = null)
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object>
		{
			{ "runtimePropInfo", runtimePropInfo },
			{ "script", script },
			{ "readOnly", readOnly }
		};
		return (UIScriptWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIScriptWindowView>(parent, supplementalData);
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		Script script = (Script)supplementalData["script"];
		PropLibrary.RuntimePropInfo runtimePropInfo = (PropLibrary.RuntimePropInfo)supplementalData["runtimePropInfo"];
		readOnly = (bool)supplementalData["readOnly"];
		string body = script.Body;
		scriptInputField.SetTextWithoutNotify(body);
		scriptInputFieldScrollbar.value = 0f;
		enumEntryListModel.SetUserCanRemove(!readOnly, triggerEvents: false);
		inspectorScriptValueListModel.SetUserCanRemove(!readOnly, triggerEvents: false);
		wiringReceiverListModel.SetUserCanRemove(!readOnly, triggerEvents: false);
		wiringEventListModel.SetUserCanRemove(!readOnly, triggerEvents: false);
		scriptReferenceListModel.SetUserCanRemove(!readOnly, triggerEvents: false);
		Model.Initialize(script, runtimePropInfo.PropData.AssetID);
		propNameText.text = runtimePropInfo.PropData.Name;
		if (!readOnly)
		{
			scriptInputField.ActivateInputField();
			scriptInputField.caretPosition = 0;
		}
		scriptInputField.readOnly = readOnly;
		createNewScriptDataButton.gameObject.SetActive(!readOnly);
		saveButton.gameObject.SetActive(!readOnly);
		revertButton.gameObject.SetActive(!readOnly);
		openSourceToggle.SetInteractable(!readOnly, tweenVisuals: false);
		openSourceToggle.SetIsOn(script.OpenSource, suppressOnChange: false);
		readOnlyFrame.SetActive(readOnly);
		lineHighlightImage.gameObject.SetActive(!readOnly);
		if (!started)
		{
			View();
		}
		if (checkForTextChangesCoroutine == null)
		{
			checkForTextChangesCoroutine = StartCoroutine(CheckForTextChanges());
		}
		MonoBehaviourSingleton<CameraController>.Instance.ToggleInput(state: false);
	}

	public override void Close()
	{
		if (Model.IsChanged)
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Before You Go!", null, "Do you want to save the changes you made to your script?\nYour changes will be lost if you don't save them.", UIModalManagerStackActions.ClearStack, new UIModalGenericViewAction(UIColors.AzureRadiance, "Save", delegate
			{
				controller.SaveScriptAndPropAndApplyToGame();
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
				ActuallyClose();
			}), new UIModalGenericViewAction(UIColors.MediumCarmine, "Don't Save", delegate
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
				ActuallyClose();
			}), new UIModalGenericViewAction(UIColors.Shark, "Cancel", delegate
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
				if (MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool.GetType() != typeof(PropTool))
				{
					PropTool activeTool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(ToolType.Prop) as PropTool;
					MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(activeTool);
				}
			}));
		}
		else
		{
			ActuallyClose();
		}
	}

	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		if (autocompleteContainer.gameObject.activeInHierarchy)
		{
			autocompleteContainer.gameObject.SetActive(value: false);
			controller.AutoCompleteEscaped();
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(null);
			controller.Close();
		}
	}

	private void View()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View");
		}
		scriptInputField.SetTextWithoutNotify(Model.ScriptBody);
		ViewStyledScript();
		if (!readOnly)
		{
			HighlightSelectedLine();
		}
		ViewLineNumbers();
		ViewErrors();
		bool isChanged = Model.IsChanged;
		saveButton.interactable = isChanged;
		revertButton.interactable = isChanged;
	}

	private void ViewStyledScript()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewStyledScript");
		}
		string text = Model.ScriptBody;
		if (text.Length > 0)
		{
			IReadOnlyList<Token> tokens = Model.Tokens;
			for (int num = tokens.Count - 1; num >= 0; num--)
			{
				Token token = tokens[num];
				if (superVerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}[{1}]: {2}", "tokens", num, token), this);
				}
				if (token.Type != TokenType.EndOfFile)
				{
					if (token.StartIndex >= text.Length)
					{
						DebugUtility.LogError(string.Format("This token is out of bounds to the body (which has a length of {0})!\n{1}[{2}]: {3}", text.Length, "tokens", num, token), this);
					}
					else
					{
						try
						{
							string text2 = Model.ScriptBody.Substring(token.StartIndex, token.Length);
							TokenGroupTypes tokenGroupType = TokenGroupTypeDictionary.GetTokenGroupType(token.Type);
							Color color = tokenTypeColorDictionary[tokenGroupType];
							text2 = UITextMeshProUtilities.Color(text2, color);
							text = text.Remove(token.StartIndex, token.Length);
							text = text.Insert(token.StartIndex, text2);
						}
						catch (Exception exception)
						{
							DebugUtility.Log(token.ToString(), this);
							DebugUtility.LogException(exception, this);
						}
					}
				}
			}
		}
		styledUserScriptText.text = text;
	}

	private void HighlightSelectedLine()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HighlightSelectedLine");
		}
		int caretPosition = scriptInputField.caretPosition;
		caretPosition = Mathf.Clamp(caretPosition, 0, scriptInputField.textComponent.textInfo.characterInfo.Length - 1);
		int lineNumber = scriptInputField.textComponent.textInfo.characterInfo[caretPosition].lineNumber;
		float y = scriptInputField.textComponent.textInfo.lineInfo[lineNumber].lineHeight * (float)(lineNumber + 1) * -1f;
		lineHighlightImage.rectTransform.anchoredPosition = new Vector2(0f, y);
	}

	private IEnumerator CheckForTextChanges()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CheckForTextChanges");
		}
		while (true)
		{
			int caretPositionBefore = scriptInputField.caretPosition;
			Vector2 textAnchoredPositionBefore = textRectTransform.anchoredPosition;
			yield return new WaitForEndOfFrame();
			if (scriptInputField.caretPosition != caretPositionBefore || textRectTransform.anchoredPosition != textAnchoredPositionBefore)
			{
				HighlightSelectedLine();
			}
		}
	}

	private void PlaceAutoCompleteContainer()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "PlaceAutoCompleteContainer");
		}
		autocompleteContainer.gameObject.SetActive(value: false);
		if (userScriptAutocompleteListModel.Count != 0)
		{
			if (superVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "AutoComplete", Model.AutoComplete.Count), this);
			}
			Vector3 bottomRight = scriptInputField.textComponent.textInfo.characterInfo[scriptInputField.caretPosition].bottomRight;
			Vector2 sizeDelta = autocompleteContainer.sizeDelta;
			autocompleteContainer.SetAnchor(AnchorPresets.TopLeft, 0f, 0f, sizeDelta.x, sizeDelta.y);
			autocompleteContainer.SetPivot(PivotPresets.TopLeft);
			autocompleteContainer.transform.localPosition = bottomRight;
			if (!scriptInputField.RectTransform.IsInside(autocompleteContainer))
			{
				autocompleteContainer.SetAnchor(AnchorPresets.BottomLeft, 0f, 0f, autocompleteContainer.sizeDelta.x, autocompleteContainer.sizeDelta.y);
				autocompleteContainer.SetPivot(PivotPresets.BottomLeft);
				autocompleteContainer.transform.localPosition = bottomRight;
			}
			autocompleteContainer.gameObject.SetActive(value: true);
		}
	}

	private void ViewLineNumbers()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewLineNumbers");
		}
		string text = string.Empty;
		int num = Model.ScriptBody.Split('\n').Length;
		for (int i = 0; i < num; i++)
		{
			text += i + 1;
			if (i < num)
			{
				text += "\n";
			}
		}
		lineNumberText.text = text;
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "lineCount", num), this);
		}
	}

	private void ViewErrors()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewErrors");
		}
		bool flag = Model.Errors.Count > 0;
		errorCountText.enabled = flag;
		if (!flag)
		{
			return;
		}
		errorCountText.text = $"There are {Model.Errors.Count:N0} errors!";
		foreach (ParsingError error in Model.Errors)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogWarning($"{error.Message}: Line - {error.Line}, Start Index: {error.CharacterIndex}, Global Error: {error.GlobalError}", this);
			}
			int num = error.Line - 1;
			if (num < 0)
			{
				num = 0;
			}
			string text = styledUserScriptText.text;
			string[] array = text.Split("\n");
			array[num] = GetLineStyledAsError(array[num]);
			text = string.Join("\n", array);
			styledUserScriptText.text = text;
		}
	}

	private string GetLineStyledAsError(string line)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetLineStyledAsError", line);
		}
		int i;
		for (i = 0; i < line.Length && line[i] == '\t'; i++)
		{
		}
		string text = line.Substring(0, i);
		string text2 = line.Substring(i);
		return UITextMeshProUtilities.Color(text + "<u>" + text2 + "</u>", tokenTypeColorDictionary.LineErrorColor);
	}

	private void OnSelect(string _)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSelect", _);
		}
		lineHighlightImage.gameObject.SetActive(value: true);
	}

	private void OnDeselect(string _)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDeselect", _);
		}
		lineHighlightImage.gameObject.SetActive(value: false);
	}

	private void OnTextSelection(string allText, int lastSelection, int firstSelection)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnTextSelection", allText, lastSelection, firstSelection);
		}
		StartCoroutine(WaitAndDeactivateLineHighlight());
	}

	private void OnEndTextSelection(string allText, int lastSelection, int firstSelection)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnTextSelection", allText, lastSelection, firstSelection);
		}
		StartCoroutine(WaitAndActivateLineHighlightIfFocused());
	}

	private IEnumerator WaitAndActivateLineHighlightIfFocused()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "WaitAndActivateLineHighlightIfFocused");
		}
		yield return new WaitForEndOfFrame();
		if (scriptInputField.isFocused)
		{
			lineHighlightImage.gameObject.SetActive(value: true);
		}
	}

	private IEnumerator WaitAndDeactivateLineHighlight()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "WaitAndDeactivateLineHighlight");
		}
		yield return new WaitForEndOfFrame();
		lineHighlightImage.gameObject.SetActive(value: false);
	}

	private void ActuallyClose()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ActuallyClose");
		}
		base.Close();
		scriptInputField.Clear();
		styledUserScriptText.text = string.Empty;
		if (checkForTextChangesCoroutine == null)
		{
			StopCoroutine(checkForTextChangesCoroutine);
			checkForTextChangesCoroutine = null;
		}
		OnClosedUnityEvent.Invoke(Model.PropId);
		MonoBehaviourSingleton<CameraController>.Instance.ToggleInput(state: true);
	}
}
