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
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020002EB RID: 747
	public class UIScriptWindowView : UIBaseWindowView
	{
		// Token: 0x170001AD RID: 429
		// (get) Token: 0x06000CCE RID: 3278 RVA: 0x0003D646 File Offset: 0x0003B846
		// (set) Token: 0x06000CCF RID: 3279 RVA: 0x0003D64E File Offset: 0x0003B84E
		public UIScriptWindowModel Model { get; private set; }

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x06000CD0 RID: 3280 RVA: 0x0003D657 File Offset: 0x0003B857
		public UnityEvent<SerializableGuid> OnClosedUnityEvent { get; } = new UnityEvent<SerializableGuid>();

		// Token: 0x06000CD1 RID: 3281 RVA: 0x0003D660 File Offset: 0x0003B860
		protected override void Start()
		{
			base.Start();
			this.Model.OnModelChanged.AddListener(new UnityAction(this.View));
			this.userScriptAutocompleteListModel.ModelChangedUnityEvent.AddListener(new UnityAction(this.PlaceAutoCompleteContainer));
			this.scriptInputField.onSelect.AddListener(new UnityAction<string>(this.OnSelect));
			this.scriptInputField.onDeselect.AddListener(new UnityAction<string>(this.OnDeselect));
			this.scriptInputField.onTextSelection.AddListener(new UnityAction<string, int, int>(this.OnTextSelection));
			this.scriptInputField.onEndTextSelection.AddListener(new UnityAction<string, int, int>(this.OnEndTextSelection));
			this.lineHighlightImage.transform.SetParent(this.scriptInputField.textComponent.transform, true);
			this.lineHighlightImage.color = this.tokenTypeColorDictionary.LineHighlightColor;
			this.autocompleteContainer.SetParent(this.scriptInputField.textComponent.transform, true);
			this.started = true;
		}

		// Token: 0x06000CD2 RID: 3282 RVA: 0x0003D778 File Offset: 0x0003B978
		public static UIScriptWindowView Display(Script script, PropLibrary.RuntimePropInfo runtimePropInfo, bool readOnly, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{ "runtimePropInfo", runtimePropInfo },
				{ "script", script },
				{ "readOnly", readOnly }
			};
			return (UIScriptWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIScriptWindowView>(parent, dictionary);
		}

		// Token: 0x06000CD3 RID: 3283 RVA: 0x0003D7C8 File Offset: 0x0003B9C8
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			Script script = (Script)supplementalData["script"];
			PropLibrary.RuntimePropInfo runtimePropInfo = (PropLibrary.RuntimePropInfo)supplementalData["runtimePropInfo"];
			this.readOnly = (bool)supplementalData["readOnly"];
			string body = script.Body;
			this.scriptInputField.SetTextWithoutNotify(body);
			this.scriptInputFieldScrollbar.value = 0f;
			this.enumEntryListModel.SetUserCanRemove(!this.readOnly, false);
			this.inspectorScriptValueListModel.SetUserCanRemove(!this.readOnly, false);
			this.wiringReceiverListModel.SetUserCanRemove(!this.readOnly, false);
			this.wiringEventListModel.SetUserCanRemove(!this.readOnly, false);
			this.scriptReferenceListModel.SetUserCanRemove(!this.readOnly, false);
			this.Model.Initialize(script, runtimePropInfo.PropData.AssetID);
			this.propNameText.text = runtimePropInfo.PropData.Name;
			if (!this.readOnly)
			{
				this.scriptInputField.ActivateInputField();
				this.scriptInputField.caretPosition = 0;
			}
			this.scriptInputField.readOnly = this.readOnly;
			this.createNewScriptDataButton.gameObject.SetActive(!this.readOnly);
			this.saveButton.gameObject.SetActive(!this.readOnly);
			this.revertButton.gameObject.SetActive(!this.readOnly);
			this.openSourceToggle.SetInteractable(!this.readOnly, false);
			this.openSourceToggle.SetIsOn(script.OpenSource, false, true);
			this.readOnlyFrame.SetActive(this.readOnly);
			this.lineHighlightImage.gameObject.SetActive(!this.readOnly);
			if (!this.started)
			{
				this.View();
			}
			if (this.checkForTextChangesCoroutine == null)
			{
				this.checkForTextChangesCoroutine = base.StartCoroutine(this.CheckForTextChanges());
			}
			MonoBehaviourSingleton<CameraController>.Instance.ToggleInput(false);
		}

		// Token: 0x06000CD4 RID: 3284 RVA: 0x0003D9D4 File Offset: 0x0003BBD4
		public override void Close()
		{
			if (this.Model.IsChanged)
			{
				UIModalManager instance = MonoBehaviourSingleton<UIModalManager>.Instance;
				string text = "Before You Go!";
				Sprite sprite = null;
				string text2 = "Do you want to save the changes you made to your script?\nYour changes will be lost if you don't save them.";
				UIModalManagerStackActions uimodalManagerStackActions = UIModalManagerStackActions.ClearStack;
				UIModalGenericViewAction[] array = new UIModalGenericViewAction[3];
				array[0] = new UIModalGenericViewAction(UIColors.AzureRadiance, "Save", delegate
				{
					this.controller.SaveScriptAndPropAndApplyToGame();
					MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
					this.ActuallyClose();
				});
				array[1] = new UIModalGenericViewAction(UIColors.MediumCarmine, "Don't Save", delegate
				{
					MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
					this.ActuallyClose();
				});
				array[2] = new UIModalGenericViewAction(UIColors.Shark, "Cancel", delegate
				{
					MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
					if (MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool.GetType() != typeof(PropTool))
					{
						PropTool propTool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(ToolType.Prop) as PropTool;
						MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(propTool);
					}
				});
				instance.DisplayGenericModal(text, sprite, text2, uimodalManagerStackActions, array);
				return;
			}
			this.ActuallyClose();
		}

		// Token: 0x06000CD5 RID: 3285 RVA: 0x0003DA90 File Offset: 0x0003BC90
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			if (this.autocompleteContainer.gameObject.activeInHierarchy)
			{
				this.autocompleteContainer.gameObject.SetActive(false);
				this.controller.AutoCompleteEscaped();
				return;
			}
			EventSystem.current.SetSelectedGameObject(null);
			this.controller.Close();
		}

		// Token: 0x06000CD6 RID: 3286 RVA: 0x0003DAFC File Offset: 0x0003BCFC
		private void View()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", Array.Empty<object>());
			}
			this.scriptInputField.SetTextWithoutNotify(this.Model.ScriptBody);
			this.ViewStyledScript();
			if (!this.readOnly)
			{
				this.HighlightSelectedLine();
			}
			this.ViewLineNumbers();
			this.ViewErrors();
			bool isChanged = this.Model.IsChanged;
			this.saveButton.interactable = isChanged;
			this.revertButton.interactable = isChanged;
		}

		// Token: 0x06000CD7 RID: 3287 RVA: 0x0003DB7C File Offset: 0x0003BD7C
		private void ViewStyledScript()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewStyledScript", Array.Empty<object>());
			}
			string text = this.Model.ScriptBody;
			if (text.Length > 0)
			{
				IReadOnlyList<Token> tokens = this.Model.Tokens;
				for (int i = tokens.Count - 1; i >= 0; i--)
				{
					Token token = tokens[i];
					if (this.superVerboseLogging)
					{
						DebugUtility.Log(string.Format("{0}[{1}]: {2}", "tokens", i, token), this);
					}
					if (token.Type != TokenType.EndOfFile)
					{
						if (token.StartIndex >= text.Length)
						{
							DebugUtility.LogError(string.Format("This token is out of bounds to the body (which has a length of {0})!\n{1}[{2}]: {3}", new object[] { text.Length, "tokens", i, token }), this);
						}
						else
						{
							try
							{
								string text2 = this.Model.ScriptBody.Substring(token.StartIndex, token.Length);
								TokenGroupTypes tokenGroupType = TokenGroupTypeDictionary.GetTokenGroupType(token.Type);
								Color color = this.tokenTypeColorDictionary[tokenGroupType];
								text2 = UITextMeshProUtilities.Color(text2, color);
								text = text.Remove(token.StartIndex, token.Length);
								text = text.Insert(token.StartIndex, text2);
							}
							catch (Exception ex)
							{
								DebugUtility.Log(token.ToString(), this);
								DebugUtility.LogException(ex, this);
							}
						}
					}
				}
			}
			this.styledUserScriptText.text = text;
		}

		// Token: 0x06000CD8 RID: 3288 RVA: 0x0003DCFC File Offset: 0x0003BEFC
		private void HighlightSelectedLine()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HighlightSelectedLine", Array.Empty<object>());
			}
			int num = this.scriptInputField.caretPosition;
			num = Mathf.Clamp(num, 0, this.scriptInputField.textComponent.textInfo.characterInfo.Length - 1);
			int lineNumber = this.scriptInputField.textComponent.textInfo.characterInfo[num].lineNumber;
			float num2 = this.scriptInputField.textComponent.textInfo.lineInfo[lineNumber].lineHeight * (float)(lineNumber + 1) * -1f;
			this.lineHighlightImage.rectTransform.anchoredPosition = new Vector2(0f, num2);
		}

		// Token: 0x06000CD9 RID: 3289 RVA: 0x0003DDB6 File Offset: 0x0003BFB6
		private IEnumerator CheckForTextChanges()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CheckForTextChanges", Array.Empty<object>());
			}
			for (;;)
			{
				int caretPositionBefore = this.scriptInputField.caretPosition;
				Vector2 textAnchoredPositionBefore = this.textRectTransform.anchoredPosition;
				yield return new WaitForEndOfFrame();
				if (this.scriptInputField.caretPosition != caretPositionBefore || this.textRectTransform.anchoredPosition != textAnchoredPositionBefore)
				{
					this.HighlightSelectedLine();
					textAnchoredPositionBefore = default(Vector2);
				}
			}
			yield break;
		}

		// Token: 0x06000CDA RID: 3290 RVA: 0x0003DDC8 File Offset: 0x0003BFC8
		private void PlaceAutoCompleteContainer()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "PlaceAutoCompleteContainer", Array.Empty<object>());
			}
			this.autocompleteContainer.gameObject.SetActive(false);
			if (this.userScriptAutocompleteListModel.Count == 0)
			{
				return;
			}
			if (this.superVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "AutoComplete", this.Model.AutoComplete.Count), this);
			}
			Vector3 bottomRight = this.scriptInputField.textComponent.textInfo.characterInfo[this.scriptInputField.caretPosition].bottomRight;
			Vector2 sizeDelta = this.autocompleteContainer.sizeDelta;
			this.autocompleteContainer.SetAnchor(AnchorPresets.TopLeft, 0f, 0f, sizeDelta.x, sizeDelta.y);
			this.autocompleteContainer.SetPivot(PivotPresets.TopLeft);
			this.autocompleteContainer.transform.localPosition = bottomRight;
			if (!this.scriptInputField.RectTransform.IsInside(this.autocompleteContainer))
			{
				this.autocompleteContainer.SetAnchor(AnchorPresets.BottomLeft, 0f, 0f, this.autocompleteContainer.sizeDelta.x, this.autocompleteContainer.sizeDelta.y);
				this.autocompleteContainer.SetPivot(PivotPresets.BottomLeft);
				this.autocompleteContainer.transform.localPosition = bottomRight;
			}
			this.autocompleteContainer.gameObject.SetActive(true);
		}

		// Token: 0x06000CDB RID: 3291 RVA: 0x0003DF30 File Offset: 0x0003C130
		private void ViewLineNumbers()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewLineNumbers", Array.Empty<object>());
			}
			string text = string.Empty;
			int num = this.Model.ScriptBody.Split('\n', StringSplitOptions.None).Length;
			for (int i = 0; i < num; i++)
			{
				text += (i + 1).ToString();
				if (i < num)
				{
					text += "\n";
				}
			}
			this.lineNumberText.text = text;
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "lineCount", num), this);
			}
		}

		// Token: 0x06000CDC RID: 3292 RVA: 0x0003DFD0 File Offset: 0x0003C1D0
		private void ViewErrors()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewErrors", Array.Empty<object>());
			}
			bool flag = this.Model.Errors.Count > 0;
			this.errorCountText.enabled = flag;
			if (!flag)
			{
				return;
			}
			this.errorCountText.text = string.Format("There are {0:N0} errors!", this.Model.Errors.Count);
			foreach (ParsingError parsingError in this.Model.Errors)
			{
				if (base.VerboseLogging)
				{
					DebugUtility.LogWarning(string.Format("{0}: Line - {1}, Start Index: {2}, Global Error: {3}", new object[] { parsingError.Message, parsingError.Line, parsingError.CharacterIndex, parsingError.GlobalError }), this);
				}
				int num = parsingError.Line - 1;
				if (num < 0)
				{
					num = 0;
				}
				string text = this.styledUserScriptText.text;
				string[] array = text.Split("\n", StringSplitOptions.None);
				array[num] = this.GetLineStyledAsError(array[num]);
				text = string.Join("\n", array);
				this.styledUserScriptText.text = text;
			}
		}

		// Token: 0x06000CDD RID: 3293 RVA: 0x0003E12C File Offset: 0x0003C32C
		private string GetLineStyledAsError(string line)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetLineStyledAsError", new object[] { line });
			}
			int num = 0;
			while (num < line.Length && line[num] == '\t')
			{
				num++;
			}
			string text = line.Substring(0, num);
			string text2 = line.Substring(num);
			return UITextMeshProUtilities.Color(text + "<u>" + text2 + "</u>", this.tokenTypeColorDictionary.LineErrorColor);
		}

		// Token: 0x06000CDE RID: 3294 RVA: 0x0003E1A2 File Offset: 0x0003C3A2
		private void OnSelect(string _)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelect", new object[] { _ });
			}
			this.lineHighlightImage.gameObject.SetActive(true);
		}

		// Token: 0x06000CDF RID: 3295 RVA: 0x0003E1D2 File Offset: 0x0003C3D2
		private void OnDeselect(string _)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDeselect", new object[] { _ });
			}
			this.lineHighlightImage.gameObject.SetActive(false);
		}

		// Token: 0x06000CE0 RID: 3296 RVA: 0x0003E202 File Offset: 0x0003C402
		private void OnTextSelection(string allText, int lastSelection, int firstSelection)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTextSelection", new object[] { allText, lastSelection, firstSelection });
			}
			base.StartCoroutine(this.WaitAndDeactivateLineHighlight());
		}

		// Token: 0x06000CE1 RID: 3297 RVA: 0x0003E240 File Offset: 0x0003C440
		private void OnEndTextSelection(string allText, int lastSelection, int firstSelection)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTextSelection", new object[] { allText, lastSelection, firstSelection });
			}
			base.StartCoroutine(this.WaitAndActivateLineHighlightIfFocused());
		}

		// Token: 0x06000CE2 RID: 3298 RVA: 0x0003E27E File Offset: 0x0003C47E
		private IEnumerator WaitAndActivateLineHighlightIfFocused()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "WaitAndActivateLineHighlightIfFocused", Array.Empty<object>());
			}
			yield return new WaitForEndOfFrame();
			if (this.scriptInputField.isFocused)
			{
				this.lineHighlightImage.gameObject.SetActive(true);
			}
			yield break;
		}

		// Token: 0x06000CE3 RID: 3299 RVA: 0x0003E28D File Offset: 0x0003C48D
		private IEnumerator WaitAndDeactivateLineHighlight()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "WaitAndDeactivateLineHighlight", Array.Empty<object>());
			}
			yield return new WaitForEndOfFrame();
			this.lineHighlightImage.gameObject.SetActive(false);
			yield break;
		}

		// Token: 0x06000CE4 RID: 3300 RVA: 0x0003E29C File Offset: 0x0003C49C
		private void ActuallyClose()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ActuallyClose", Array.Empty<object>());
			}
			base.Close();
			this.scriptInputField.Clear(true);
			this.styledUserScriptText.text = string.Empty;
			if (this.checkForTextChangesCoroutine == null)
			{
				base.StopCoroutine(this.checkForTextChangesCoroutine);
				this.checkForTextChangesCoroutine = null;
			}
			this.OnClosedUnityEvent.Invoke(this.Model.PropId);
			MonoBehaviourSingleton<CameraController>.Instance.ToggleInput(true);
		}

		// Token: 0x04000B06 RID: 2822
		[SerializeField]
		private TextMeshProUGUI propNameText;

		// Token: 0x04000B07 RID: 2823
		[SerializeField]
		private UIScriptWindowController controller;

		// Token: 0x04000B08 RID: 2824
		[SerializeField]
		private GameObject readOnlyFrame;

		// Token: 0x04000B09 RID: 2825
		[SerializeField]
		private UIButton saveButton;

		// Token: 0x04000B0A RID: 2826
		[SerializeField]
		private UIButton revertButton;

		// Token: 0x04000B0B RID: 2827
		[SerializeField]
		private TextMeshProUGUI errorCountText;

		// Token: 0x04000B0C RID: 2828
		[SerializeField]
		private UIToggle openSourceToggle;

		// Token: 0x04000B0D RID: 2829
		[SerializeField]
		private UIButton createNewScriptDataButton;

		// Token: 0x04000B0E RID: 2830
		[SerializeField]
		private UIEnumEntryListModel enumEntryListModel;

		// Token: 0x04000B0F RID: 2831
		[SerializeField]
		private UIInspectorScriptValueListModel inspectorScriptValueListModel;

		// Token: 0x04000B10 RID: 2832
		[SerializeField]
		private UIEndlessEventInfoListModel wiringReceiverListModel;

		// Token: 0x04000B11 RID: 2833
		[SerializeField]
		private UIEndlessEventInfoListModel wiringEventListModel;

		// Token: 0x04000B12 RID: 2834
		[SerializeField]
		private UIScriptReferenceListModel scriptReferenceListModel;

		// Token: 0x04000B13 RID: 2835
		[SerializeField]
		private UIInputField scriptInputField;

		// Token: 0x04000B14 RID: 2836
		[SerializeField]
		private RectTransform textRectTransform;

		// Token: 0x04000B15 RID: 2837
		[SerializeField]
		private Scrollbar scriptInputFieldScrollbar;

		// Token: 0x04000B16 RID: 2838
		[SerializeField]
		private TextMeshProUGUI lineNumberText;

		// Token: 0x04000B17 RID: 2839
		[SerializeField]
		private TextMeshProUGUI styledUserScriptText;

		// Token: 0x04000B18 RID: 2840
		[SerializeField]
		private UITokenGroupTypeColorDictionary tokenTypeColorDictionary;

		// Token: 0x04000B19 RID: 2841
		[SerializeField]
		private Image lineHighlightImage;

		// Token: 0x04000B1A RID: 2842
		[SerializeField]
		private RectTransform autocompleteContainer;

		// Token: 0x04000B1B RID: 2843
		[SerializeField]
		private UIUserScriptAutocompleteListModel userScriptAutocompleteListModel;

		// Token: 0x04000B1C RID: 2844
		[Header("Debugging")]
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x04000B1D RID: 2845
		private bool started;

		// Token: 0x04000B1E RID: 2846
		private Coroutine checkForTextChangesCoroutine;

		// Token: 0x04000B1F RID: 2847
		private bool readOnly;
	}
}
