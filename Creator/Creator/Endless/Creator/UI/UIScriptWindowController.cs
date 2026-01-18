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

namespace Endless.Creator.UI
{
	// Token: 0x020002E1 RID: 737
	public class UIScriptWindowController : UIWindowController, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x06000C85 RID: 3205 RVA: 0x0003BB00 File Offset: 0x00039D00
		private void Awake()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			this.scriptInputField.OnTabInputUnityEvent.AddListener(new UnityAction(this.OnTabInput));
			this.scriptInputField.OnNewLineInputUnityEvent.AddListener(new UnityAction(this.OnEnterInput));
		}

		// Token: 0x06000C86 RID: 3206 RVA: 0x0003BB60 File Offset: 0x00039D60
		protected override void Start()
		{
			base.Start();
			this.saveScriptAndPropAndApplyToGameHandler.OnLoadingStarted.AddListener(new UnityAction(this.OnScriptSavingStarted));
			this.saveScriptAndPropAndApplyToGameHandler.OnLoadingEnded.AddListener(new UnityAction(this.OnScriptSavingEnded));
			this.scriptInputField.onValueChanged.AddListener(new UnityAction<string>(this.model.SetScriptBody));
			this.scriptInputField.CaretPositionChangedUnityEvent.AddListener(new UnityAction<int>(this.UpdateAutoComplete));
			this.saveScriptAndPropAndApplyToGameButton.onClick.AddListener(new UnityAction(this.SaveScriptAndPropAndApplyToGame));
			this.revertButton.onClick.AddListener(new UnityAction(this.model.Revert));
			this.openSourceToggle.OnChange.AddListener(new UnityAction<bool>(this.model.SetOpenSource));
			this.createNewButton.onClick.AddListener(new UnityAction(this.ViewNewScriptDataModal));
			this.showDocumentationButton.onClick.AddListener(new UnityAction(this.OpenEndstarLuaDocumentation));
			this.showConsoleButton.onClick.AddListener(new UnityAction(this.OnShowConsoleButtonClicked));
		}

		// Token: 0x06000C87 RID: 3207 RVA: 0x0003BC9A File Offset: 0x00039E9A
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIUserScriptAutocompleteListCellController.OnSelect = (Action<string>)Delegate.Combine(UIUserScriptAutocompleteListCellController.OnSelect, new Action<string>(this.AutocompleteByClick));
		}

		// Token: 0x06000C88 RID: 3208 RVA: 0x0003BCD4 File Offset: 0x00039ED4
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			UIUserScriptAutocompleteListCellController.OnSelect = (Action<string>)Delegate.Remove(UIUserScriptAutocompleteListCellController.OnSelect, new Action<string>(this.AutocompleteByClick));
		}

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06000C89 RID: 3209 RVA: 0x0003BD0E File Offset: 0x00039F0E
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x06000C8A RID: 3210 RVA: 0x0003BD16 File Offset: 0x00039F16
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000C8B RID: 3211 RVA: 0x0003BD1E File Offset: 0x00039F1E
		private void OnTabInput()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTabInput", Array.Empty<object>());
			}
			if (this.model.AutoComplete.Count == 0)
			{
				return;
			}
			this.AutocompleteToFirstItem();
		}

		// Token: 0x06000C8C RID: 3212 RVA: 0x0003BD51 File Offset: 0x00039F51
		private void OnEnterInput()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnterInput", Array.Empty<object>());
			}
			if (this.model.AutoComplete.Count == 0)
			{
				return;
			}
			this.AutocompleteToFirstItem();
		}

		// Token: 0x06000C8D RID: 3213 RVA: 0x0003BD84 File Offset: 0x00039F84
		private void OpenEndstarLuaDocumentation()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenEndstarLuaDocumentation", Array.Empty<object>());
			}
			Application.OpenURL(this.endstarLuaDocumentationURL.Value);
		}

		// Token: 0x06000C8E RID: 3214 RVA: 0x0003BDB0 File Offset: 0x00039FB0
		private void OnShowConsoleButtonClicked()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnShowConsoleButtonClicked", Array.Empty<object>());
			}
			UIConsoleWindowView.Display(NetworkBehaviourSingleton<UserScriptingConsole>.Instance.GetConsoleMessagesForAssetId(this.model.PropId), ConsoleScope.Asset, this.model.PropId, null);
		}

		// Token: 0x06000C8F RID: 3215 RVA: 0x0003BE00 File Offset: 0x0003A000
		private void ViewNewScriptDataModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewNewScriptDataModal", Array.Empty<object>());
			}
			Prop cloneOfProp = this.model.GetCloneOfProp();
			Script cloneOfScript = this.model.GetCloneOfScript();
			MonoBehaviourSingleton<UIScriptDataWizard>.Instance.Initialize(cloneOfProp, cloneOfScript);
		}

		// Token: 0x06000C90 RID: 3216 RVA: 0x0003BE4C File Offset: 0x0003A04C
		private void AutocompleteToFirstItem()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "AutocompleteToFirstItem", Array.Empty<object>());
			}
			string value = this.model.AutoComplete[0].Value;
			this.Autocomplete(value, false);
		}

		// Token: 0x06000C91 RID: 3217 RVA: 0x0003BE90 File Offset: 0x0003A090
		private void AutocompleteByClick(string autoCompleteTo)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "AutocompleteByClick", new object[] { autoCompleteTo });
			}
			this.Autocomplete(autoCompleteTo, true);
		}

		// Token: 0x06000C92 RID: 3218 RVA: 0x0003BEB8 File Offset: 0x0003A0B8
		private void Autocomplete(string autoCompleteTo, bool inputWasClick)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Autocomplete", new object[] { autoCompleteTo, inputWasClick });
			}
			UIScriptWindowController.TextBehindResult textBehind = this.GetTextBehind(this.scriptInputField.caretPosition);
			int num = textBehind.StartIndex + 1 + autoCompleteTo.Length;
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}", new object[] { "StartIndex", textBehind.StartIndex, "newCaretPosition", num }), this);
			}
			string text = this.scriptInputField.text.Remove(textBehind.StartIndex + 1, this.scriptInputField.caretPosition - textBehind.StartIndex - 1);
			text = text.Insert(textBehind.StartIndex + 1, autoCompleteTo);
			this.scriptInputField.text = text;
			this.model.UpdateAutoComplete(null, null, null);
			if (inputWasClick)
			{
				this.scriptInputField.Select();
				base.StartCoroutine(this.WaitForEndOfFrameAndSetCaretPosition(num));
				return;
			}
			this.scriptInputField.SetCaretPositionWithoutNotify(num);
			this.scriptInputField.Rebuild(CanvasUpdate.LatePreRender);
		}

		// Token: 0x06000C93 RID: 3219 RVA: 0x0003BFE6 File Offset: 0x0003A1E6
		private IEnumerator WaitForEndOfFrameAndSetCaretPosition(int caretPosition)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "WaitForEndOfFrameAndSetCaretPosition", new object[] { caretPosition });
			}
			yield return new WaitForEndOfFrame();
			this.scriptInputField.SetCaretPositionWithoutNotify(caretPosition);
			this.scriptInputField.Rebuild(CanvasUpdate.LatePreRender);
			yield break;
		}

		// Token: 0x06000C94 RID: 3220 RVA: 0x0003BFFC File Offset: 0x0003A1FC
		public void SaveScriptAndPropAndApplyToGame()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SaveScriptAndPropAndApplyToGame", Array.Empty<object>());
			}
			this.SaveScriptAndPropAndApplyToGameAsync();
		}

		// Token: 0x06000C95 RID: 3221 RVA: 0x0003C020 File Offset: 0x0003A220
		private async Task SaveScriptAndPropAndApplyToGameAsync()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SaveScriptAndPropAndApplyToGameAsync", Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
			Script cloneOfScript = this.model.GetCloneOfScript();
			Prop cloneOfProp = this.model.GetCloneOfProp();
			UISaveScriptAndPropAndApplyToGameHandler.Result result = await this.saveScriptAndPropAndApplyToGameHandler.SaveScriptAndPropAndApplyToGame(cloneOfScript, cloneOfProp);
			this.OnLoadingEnded.Invoke();
			this.model.ApplySaveScriptAndPropAndApplyToGameResult(result);
		}

		// Token: 0x06000C96 RID: 3222 RVA: 0x0003C064 File Offset: 0x0003A264
		private void UpdateAutoComplete(int newCaretPosition)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateAutoComplete", new object[] { newCaretPosition });
			}
			if (newCaretPosition < this.scriptInputField.text.Length && !char.IsWhiteSpace(this.scriptInputField.text[newCaretPosition]))
			{
				this.model.UpdateAutoComplete(null, null, null);
				return;
			}
			UIScriptWindowController.TextBehindResult textBehind = this.GetTextBehind(newCaretPosition);
			string text = null;
			if (textBehind.HasAccessOperator)
			{
				text = this.GetTextBehind(textBehind.StartIndex).Text;
			}
			this.model.UpdateAutoComplete(textBehind.Text, text, textBehind.AccessOperator);
		}

		// Token: 0x06000C97 RID: 3223 RVA: 0x0003C114 File Offset: 0x0003A314
		private UIScriptWindowController.TextBehindResult GetTextBehind(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetTextBehind", new object[] { index });
			}
			string text = string.Empty;
			char? c = null;
			while (index > 0)
			{
				index--;
				char c2 = this.scriptInputField.text[index];
				bool flag = c2 == '.' || c2 == ':';
				if (flag)
				{
					c = new char?(c2);
				}
				if (flag || char.IsWhiteSpace(c2) || c2 == '(' || c2 == ')')
				{
					break;
				}
				text = text.Insert(0, c2.ToString());
			}
			return new UIScriptWindowController.TextBehindResult(text, index, c);
		}

		// Token: 0x06000C98 RID: 3224 RVA: 0x0003C1B1 File Offset: 0x0003A3B1
		private void OnScriptSavingStarted()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScriptSavingStarted", Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
		}

		// Token: 0x06000C99 RID: 3225 RVA: 0x0003C1D6 File Offset: 0x0003A3D6
		private void OnScriptSavingEnded()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScriptSavingEnded", Array.Empty<object>());
			}
			this.OnLoadingEnded.Invoke();
		}

		// Token: 0x06000C9A RID: 3226 RVA: 0x0003C1FB File Offset: 0x0003A3FB
		public void AutoCompleteEscaped()
		{
			this.scriptInputField.ActivateInputField();
		}

		// Token: 0x04000AC3 RID: 2755
		private const string TAB = "\t";

		// Token: 0x04000AC4 RID: 2756
		[Header("UIScriptWindowController")]
		[SerializeField]
		private UIScriptWindowModel model;

		// Token: 0x04000AC5 RID: 2757
		[SerializeField]
		private UISaveScriptAndPropAndApplyToGameHandler saveScriptAndPropAndApplyToGameHandler;

		// Token: 0x04000AC6 RID: 2758
		[SerializeField]
		private UIButton saveScriptAndPropAndApplyToGameButton;

		// Token: 0x04000AC7 RID: 2759
		[SerializeField]
		private UIButton revertButton;

		// Token: 0x04000AC8 RID: 2760
		[SerializeField]
		private UIToggle openSourceToggle;

		// Token: 0x04000AC9 RID: 2761
		[SerializeField]
		private UIButton createNewButton;

		// Token: 0x04000ACA RID: 2762
		[SerializeField]
		private StringVariable endstarLuaDocumentationURL;

		// Token: 0x04000ACB RID: 2763
		[SerializeField]
		private UIButton showDocumentationButton;

		// Token: 0x04000ACC RID: 2764
		[SerializeField]
		private UIButton showConsoleButton;

		// Token: 0x04000ACD RID: 2765
		[SerializeField]
		private UIScriptInputField scriptInputField;

		// Token: 0x04000ACE RID: 2766
		[Header("Close Confirm Save Colors")]
		[SerializeField]
		private global::UnityEngine.Color saveButtonColor = global::UnityEngine.Color.white;

		// Token: 0x04000ACF RID: 2767
		[SerializeField]
		private global::UnityEngine.Color dontSaveButtonColor = global::UnityEngine.Color.white;

		// Token: 0x04000AD0 RID: 2768
		[SerializeField]
		private global::UnityEngine.Color cancelButtonColor = global::UnityEngine.Color.white;

		// Token: 0x020002E2 RID: 738
		private struct TextBehindResult
		{
			// Token: 0x170001A1 RID: 417
			// (get) Token: 0x06000C9C RID: 3228 RVA: 0x0003C248 File Offset: 0x0003A448
			public bool HasAccessOperator
			{
				get
				{
					char? accessOperator = this.AccessOperator;
					if (accessOperator != null)
					{
						char valueOrDefault = accessOperator.GetValueOrDefault();
						if (valueOrDefault == '.' || valueOrDefault == ':')
						{
							return true;
						}
					}
					return false;
				}
			}

			// Token: 0x06000C9D RID: 3229 RVA: 0x0003C27E File Offset: 0x0003A47E
			public TextBehindResult(string text, int startIndex, char? accessOperator)
			{
				this.Text = text;
				this.StartIndex = startIndex;
				this.AccessOperator = accessOperator;
			}

			// Token: 0x06000C9E RID: 3230 RVA: 0x0003C298 File Offset: 0x0003A498
			public override string ToString()
			{
				return string.Format("| {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7} |", new object[] { "Text", this.Text, "AccessOperator", this.AccessOperator, "StartIndex", this.StartIndex, "HasAccessOperator", this.HasAccessOperator });
			}

			// Token: 0x04000AD3 RID: 2771
			public readonly string Text;

			// Token: 0x04000AD4 RID: 2772
			public readonly int StartIndex;

			// Token: 0x04000AD5 RID: 2773
			public readonly char? AccessOperator;
		}
	}
}
