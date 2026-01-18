using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Endless.Creator.Test.LuaParsing;
using Endless.Data;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.Serialization;
using Endless.GraphQl;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002E5 RID: 741
	public class UIScriptWindowModel : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000CA7 RID: 3239 RVA: 0x0003C4CF File Offset: 0x0003A6CF
		// (set) Token: 0x06000CA8 RID: 3240 RVA: 0x0003C4D7 File Offset: 0x0003A6D7
		public SerializableGuid PropId { get; private set; } = SerializableGuid.Empty;

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06000CA9 RID: 3241 RVA: 0x0003C4E0 File Offset: 0x0003A6E0
		public UnityEvent OnModelChanged { get; } = new UnityEvent();

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x06000CAA RID: 3242 RVA: 0x0003C4E8 File Offset: 0x0003A6E8
		public string ScriptBody
		{
			get
			{
				return this.activeScript.Body;
			}
		}

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x06000CAB RID: 3243 RVA: 0x0003C4F5 File Offset: 0x0003A6F5
		public IReadOnlyList<UIUserScriptAutocompleteListModelItem> AutoComplete
		{
			get
			{
				return this.autoComplete;
			}
		}

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x06000CAC RID: 3244 RVA: 0x0003C4FD File Offset: 0x0003A6FD
		public bool IsChanged
		{
			get
			{
				return JsonConvert.SerializeObject(this.originalScript) != JsonConvert.SerializeObject(this.activeScript);
			}
		}

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x06000CAD RID: 3245 RVA: 0x0003C51C File Offset: 0x0003A71C
		public IReadOnlyList<Token> Tokens
		{
			get
			{
				if (this.scriptResult != null)
				{
					return this.scriptResult.Tokens;
				}
				return Array.Empty<Token>();
			}
		}

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000CAE RID: 3246 RVA: 0x0003C546 File Offset: 0x0003A746
		public IReadOnlyList<ParsingError> Errors
		{
			get
			{
				return this.errors;
			}
		}

		// Token: 0x06000CAF RID: 3247 RVA: 0x0003C550 File Offset: 0x0003A750
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.enumEntryListModel.ItemRemovedUnityEvent.AddListener(new UnityAction<int, EnumEntry>(this.OnEnumEntryRemoved));
			this.inspectorScriptValueListModel.ItemRemovedUnityEvent.AddListener(new UnityAction<int, InspectorScriptValue>(this.OnInspectorScriptValueRemoved));
			this.wiringReceiverListModel.ItemRemovedUnityEvent.AddListener(new UnityAction<int, EndlessEventInfo>(this.OnWireEventRemoved));
			this.wiringEventListModel.ItemRemovedUnityEvent.AddListener(new UnityAction<int, EndlessEventInfo>(this.OnWireReceiverRemoved));
			this.scriptReferenceListModel.ItemRemovedUnityEvent.AddListener(new UnityAction<int, ScriptReference>(this.OnScriptReferenceRemoved));
			MonoBehaviourSingleton<UIScriptDataWizard>.Instance.OnUpdatedScript.AddListener(new UnityAction<Script>(this.SetScript));
		}

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000CB0 RID: 3248 RVA: 0x0003C61C File Offset: 0x0003A81C
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06000CB1 RID: 3249 RVA: 0x0003C624 File Offset: 0x0003A824
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000CB2 RID: 3250 RVA: 0x0003C62C File Offset: 0x0003A82C
		public Prop GetCloneOfProp()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetCloneOfProp", Array.Empty<object>());
			}
			return this.GetRuntimePropInfo().PropData.Clone();
		}

		// Token: 0x06000CB3 RID: 3251 RVA: 0x0003C656 File Offset: 0x0003A856
		public Script GetCloneOfScript()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetCloneOfScript", Array.Empty<object>());
			}
			return this.activeScript.Clone();
		}

		// Token: 0x06000CB4 RID: 3252 RVA: 0x0003C67B File Offset: 0x0003A87B
		public PropLibrary.RuntimePropInfo GetRuntimePropInfo()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetRuntimePropInfo", Array.Empty<object>());
			}
			return MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(this.PropId);
		}

		// Token: 0x06000CB5 RID: 3253 RVA: 0x0003C6AC File Offset: 0x0003A8AC
		public void Initialize(Script newScript, SerializableGuid propId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { newScript, propId });
			}
			this.originalScript = newScript.Clone();
			this.PropId = propId;
			this.SetScript(newScript);
			this.GetPropVersions(propId);
		}

		// Token: 0x06000CB6 RID: 3254 RVA: 0x0003C700 File Offset: 0x0003A900
		public void SetScriptBody(string newValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetScriptBody", new object[] { newValue });
			}
			this.activeScript.Body = newValue;
			this.tokenDictionary.Clear();
			this.errors.Clear();
			try
			{
				ScriptResult scriptResult = this.luaIntellisensor.Execute(this.activeScript.Body, false);
				this.scriptResult = scriptResult;
				ScriptResultHelper.AddComponents(this.scriptResult);
				this.scriptResult.AddVariableAndType("MyContext", "Context");
				foreach (InspectorScriptValue inspectorScriptValue in this.activeScript.InspectorValues)
				{
					this.scriptResult.DeclaredVariables.Add(inspectorScriptValue.Name);
					Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(inspectorScriptValue.DataType);
					if (typeFromId != null)
					{
						ScriptResultHelper.AddIntellisenseEntry(this.scriptResult, typeFromId, new string[] { inspectorScriptValue.Name });
						this.scriptResult.AddVariableAndType(inspectorScriptValue.Name, typeFromId.Name);
					}
				}
				foreach (Type type in EndlessScriptComponent.LuaEnums)
				{
					ScriptResultHelper.AddIntellisenseForEnum(this.scriptResult, type);
				}
				ValueTuple<string, List<string>> events = this.activeScript.GetEvents();
				string item = events.Item1;
				List<string> item2 = events.Item2;
				ScriptResultHelper.AddIntellisenseEntry(this.scriptResult, item, item2);
				ValueTuple<string, List<ValueTuple<string, string>>> valueTuple = EndlessScriptComponent.GatherComponentSnippetNameIdPairs();
				string item3 = valueTuple.Item1;
				List<ValueTuple<string, string>> item4 = valueTuple.Item2;
				ScriptResultHelper.AddIntellisenseEntry(this.scriptResult, item3, item4.Select(([TupleElementNames(new string[] { "name", "id" })] ValueTuple<string, string> pair) => pair.Item1).ToList<string>());
				ValueTuple<string, List<string>> transforms = this.activeScript.GetTransforms();
				string item5 = transforms.Item1;
				List<string> item6 = transforms.Item2;
				ScriptResultHelper.AddIntellisenseEntry(this.scriptResult, item5, item6);
				ValueTuple<string, MethodInfo>[] autoRegisteredMethodInfos = EndlessScriptComponent.AutoRegisteredMethodInfos;
				for (int i = 0; i < autoRegisteredMethodInfos.Length; i++)
				{
					string item7 = autoRegisteredMethodInfos[i].Item1;
					ScriptResultHelper.AddIntellisenseEntryForMethodName(this.scriptResult, item7);
				}
				foreach (Token token in this.scriptResult.Tokens)
				{
					Vector2Int vector2Int = new Vector2Int(token.StartIndex, token.StartIndex + token.Length);
					this.tokenDictionary.Add(vector2Int, token);
				}
				this.errors = scriptResult.Errors;
			}
			catch (Exception ex)
			{
				DebugUtility.LogException(ex, this);
			}
			this.OnModelChanged.Invoke();
		}

		// Token: 0x06000CB7 RID: 3255 RVA: 0x0003C9F8 File Offset: 0x0003ABF8
		public void UpdateAutoComplete(string textBehindCaret, string filter, char? accessOperator)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateAutoComplete", new object[]
				{
					textBehindCaret,
					filter,
					(accessOperator == null) ? "null" : accessOperator
				});
			}
			this.autoComplete.Clear();
			if (filter.IsNullOrEmptyOrWhiteSpace())
			{
				if (textBehindCaret.IsNullOrEmptyOrWhiteSpace())
				{
					this.userScriptAutocompleteListModel.Clear(true);
					return;
				}
				foreach (UIUserScriptAutocompleteListModelItem uiuserScriptAutocompleteListModelItem in this.builtInAutoComplete)
				{
					this.TryToAddToAutoComplete(uiuserScriptAutocompleteListModelItem, textBehindCaret);
				}
				foreach (string text in ((this.scriptResult == null) ? new List<string>() : this.scriptResult.DeclaredVariables))
				{
					UIUserScriptAutocompleteListModelItem uiuserScriptAutocompleteListModelItem2 = new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.VariablesAndProperties, text);
					this.TryToAddToAutoComplete(uiuserScriptAutocompleteListModelItem2, textBehindCaret);
				}
				using (IEnumerator<UIUserScriptAutocompleteListModelItem> enumerator3 = ((this.scriptResult == null) ? new List<string>() : this.scriptResult.DeclaredFreeFunctions).Select((string declaredFreeFunction) => new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.FunctionsAndMethods, declaredFreeFunction)).GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						UIUserScriptAutocompleteListModelItem uiuserScriptAutocompleteListModelItem3 = enumerator3.Current;
						this.TryToAddToAutoComplete(uiuserScriptAutocompleteListModelItem3, textBehindCaret);
					}
					goto IL_0203;
				}
			}
			List<string> list = new List<string>();
			try
			{
				if (accessOperator != null)
				{
					if (accessOperator.Value == '.')
					{
						list = this.scriptResult.GetPropertiesCallableByVariable(filter);
					}
					else if (accessOperator.Value == ':')
					{
						list = this.scriptResult.GetFunctionsCallableByVariable(filter);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex, this);
				this.userScriptAutocompleteListModel.Set(this.autoComplete, true);
			}
			foreach (string text2 in list)
			{
				UIUserScriptAutocompleteListModelItem uiuserScriptAutocompleteListModelItem4 = new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.FunctionsAndMethods, text2);
				this.TryToAddToAutoComplete(uiuserScriptAutocompleteListModelItem4, textBehindCaret);
			}
			IL_0203:
			this.userScriptAutocompleteListModel.Set(this.autoComplete, true);
		}

		// Token: 0x06000CB8 RID: 3256 RVA: 0x0003CC5C File Offset: 0x0003AE5C
		public void Revert()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Revert", Array.Empty<object>());
			}
			this.SetScript(this.originalScript);
		}

		// Token: 0x06000CB9 RID: 3257 RVA: 0x0003CC84 File Offset: 0x0003AE84
		public async void SetScript(Script newScript)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetScript", new object[] { newScript.AssetID });
			}
			DebugUtility.LogMethod(this, "SetScript", new object[] { newScript.AssetID });
			this.activeScript = newScript.Clone();
			this.enumEntryListModel.Set(this.activeScript.EnumTypes, true);
			this.inspectorScriptValueListModel.Set(this.activeScript.InspectorValues, true);
			this.wiringReceiverListModel.Set(this.activeScript.Receivers, true);
			this.wiringEventListModel.Set(this.activeScript.Events, true);
			this.scriptReferenceListModel.Set(this.activeScript.ScriptReferences, true);
			if (!this.multiRequestLoadingSpinnerEventHandler.Initialized)
			{
				this.multiRequestLoadingSpinnerEventHandler.Initialize(this);
			}
			this.multiRequestLoadingSpinnerEventHandler.TrackRequest(UIScriptWindowModel.Requests.GetScriptVersions);
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(this.originalScript.AssetID, false);
			if (graphQlResult.HasErrors)
			{
				this.multiRequestLoadingSpinnerEventHandler.UntrackRequest(UIScriptWindowModel.Requests.GetScriptVersions);
				ErrorHandler.HandleError(ErrorCodes.UIScriptWindowModel_GetScriptVersions, graphQlResult.GetErrorMessage(0), true, false);
			}
			else
			{
				this.multiRequestLoadingSpinnerEventHandler.UntrackRequest(UIScriptWindowModel.Requests.GetScriptVersions);
				this.scriptVersions = JsonConvert.DeserializeObject<string[]>(graphQlResult.GetDataMember().ToString());
				this.scriptVersions = this.scriptVersions.OrderByDescending(new Func<string, Version>(Version.Parse)).ToArray<string>();
			}
			this.SetScriptBody(this.activeScript.Body);
		}

		// Token: 0x06000CBA RID: 3258 RVA: 0x0003CCC4 File Offset: 0x0003AEC4
		public async void GetPropVersions(SerializableGuid assetId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetPropVersions", new object[] { assetId });
			}
			this.multiRequestLoadingSpinnerEventHandler.TrackRequest(UIScriptWindowModel.Requests.GetPropVersions);
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(assetId, false);
			if (graphQlResult.HasErrors)
			{
				this.multiRequestLoadingSpinnerEventHandler.UntrackRequest(UIScriptWindowModel.Requests.GetPropVersions);
				ErrorHandler.HandleError(ErrorCodes.UIScriptWindowModel_GetPropVersions, graphQlResult.GetErrorMessage(0), true, false);
			}
			else
			{
				this.multiRequestLoadingSpinnerEventHandler.UntrackRequest(UIScriptWindowModel.Requests.GetPropVersions);
				this.propVersions = VersionUtilities.GetParsedAndOrderedVersions(graphQlResult.GetDataMember());
			}
		}

		// Token: 0x06000CBB RID: 3259 RVA: 0x0003CD03 File Offset: 0x0003AF03
		public void SetOpenSource(bool openSource)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOpenSource", new object[] { openSource });
			}
			this.activeScript.OpenSource = openSource;
			this.OnModelChanged.Invoke();
		}

		// Token: 0x06000CBC RID: 3260 RVA: 0x0003CD40 File Offset: 0x0003AF40
		public void ApplySaveScriptAndPropAndApplyToGameResult(UISaveScriptAndPropAndApplyToGameHandler.Result result)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplySaveScriptAndPropAndApplyToGameResult", new object[] { result });
			}
			if (result.Script != null)
			{
				this.originalScript = result.Script.Clone();
				this.SetScript(result.Script);
			}
			this.OnModelChanged.Invoke();
		}

		// Token: 0x06000CBD RID: 3261 RVA: 0x0003CDA0 File Offset: 0x0003AFA0
		private void TryToAddToAutoComplete(UIUserScriptAutocompleteListModelItem item, string textBehindCaret)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TryToAddToAutoComplete", new object[] { item, textBehindCaret });
			}
			if (this.autoComplete.Any((UIUserScriptAutocompleteListModelItem existing) => existing.Value == item.Value))
			{
				DebugUtility.Log("Not added since item.Value|" + item.Value + "| already exists in auto complete options.", null);
				return;
			}
			if (item.Value == textBehindCaret)
			{
				if (this.superVerboseLogging)
				{
					DebugUtility.Log(string.Concat(new string[] { "Not added since item.Value|", item.Value, "| is exactly equal to ( textBehindCaret: ", textBehindCaret, " )." }), this);
				}
				return;
			}
			textBehindCaret = textBehindCaret.ToLower();
			if (!item.Value.ToLower().StartsWith(textBehindCaret))
			{
				if (this.superVerboseLogging)
				{
					DebugUtility.Log(string.Concat(new string[] { "Not added since item.Value|", item.Value, "|.StartsWith( textBehindCaret: ", textBehindCaret, " ) is FALSE!" }), this);
				}
				return;
			}
			this.autoComplete.Add(item);
		}

		// Token: 0x06000CBE RID: 3262 RVA: 0x0003CEEB File Offset: 0x0003B0EB
		private void OnEnumEntryRemoved(int index, EnumEntry removedItem)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnumEntryRemoved", new object[] { index, removedItem });
			}
			this.OnModelChanged.Invoke();
		}

		// Token: 0x06000CBF RID: 3263 RVA: 0x0003CF20 File Offset: 0x0003B120
		private void OnInspectorScriptValueRemoved(int index, InspectorScriptValue removedItem)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnInspectorScriptValueRemoved", new object[] { index, removedItem });
			}
			for (int i = 0; i < this.activeScript.InspectorOrganizationData.Count; i++)
			{
				InspectorOrganizationData inspectorOrganizationData = this.activeScript.InspectorOrganizationData[i];
				if (!(inspectorOrganizationData.MemberName != removedItem.Name) && inspectorOrganizationData.DataType == removedItem.DataType)
				{
					this.activeScript.InspectorOrganizationData.RemoveAt(i);
					break;
				}
			}
			this.OnModelChanged.Invoke();
		}

		// Token: 0x06000CC0 RID: 3264 RVA: 0x0003CFBF File Offset: 0x0003B1BF
		private void OnWireEventRemoved(int index, EndlessEventInfo removedItem)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnWireEventRemoved", new object[] { index, removedItem });
			}
			this.OnModelChanged.Invoke();
		}

		// Token: 0x06000CC1 RID: 3265 RVA: 0x0003CFF2 File Offset: 0x0003B1F2
		private void OnWireReceiverRemoved(int index, EndlessEventInfo removedItem)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnWireReceiverRemoved", new object[] { index, removedItem });
			}
			this.OnModelChanged.Invoke();
		}

		// Token: 0x06000CC2 RID: 3266 RVA: 0x0003D025 File Offset: 0x0003B225
		private void OnScriptReferenceRemoved(int index, ScriptReference removedItem)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScriptReferenceRemoved", new object[] { index, removedItem });
			}
			this.OnModelChanged.Invoke();
		}

		// Token: 0x04000ADE RID: 2782
		[SerializeField]
		private Script activeScript;

		// Token: 0x04000ADF RID: 2783
		[SerializeField]
		private UIUserScriptAutocompleteListModel userScriptAutocompleteListModel;

		// Token: 0x04000AE0 RID: 2784
		[Header("Script Data")]
		[SerializeField]
		private UIEnumEntryListModel enumEntryListModel;

		// Token: 0x04000AE1 RID: 2785
		[SerializeField]
		private UIInspectorScriptValueListModel inspectorScriptValueListModel;

		// Token: 0x04000AE2 RID: 2786
		[SerializeField]
		private UIEndlessEventInfoListModel wiringReceiverListModel;

		// Token: 0x04000AE3 RID: 2787
		[SerializeField]
		private UIEndlessEventInfoListModel wiringEventListModel;

		// Token: 0x04000AE4 RID: 2788
		[SerializeField]
		private UIScriptReferenceListModel scriptReferenceListModel;

		// Token: 0x04000AE5 RID: 2789
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000AE6 RID: 2790
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x04000AE7 RID: 2791
		private readonly LuaIntellisensor luaIntellisensor = new LuaIntellisensor();

		// Token: 0x04000AE8 RID: 2792
		private readonly Dictionary<Vector2Int, Token> tokenDictionary = new Dictionary<Vector2Int, Token>();

		// Token: 0x04000AE9 RID: 2793
		private readonly UIMultiRequestLoadingSpinnerEventHandler<UIScriptWindowModel.Requests> multiRequestLoadingSpinnerEventHandler = new UIMultiRequestLoadingSpinnerEventHandler<UIScriptWindowModel.Requests>();

		// Token: 0x04000AEA RID: 2794
		private readonly List<UIUserScriptAutocompleteListModelItem> autoComplete = new List<UIUserScriptAutocompleteListModelItem>();

		// Token: 0x04000AEB RID: 2795
		private readonly List<UIUserScriptAutocompleteListModelItem> builtInAutoComplete = new List<UIUserScriptAutocompleteListModelItem>
		{
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "and"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "break"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "do"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "if"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "else"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "elseif"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "end"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "true"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "false"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "for"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "function"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "in"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "local"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "nil"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "not"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "or"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "repeat"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "return"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "then"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "until"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "while"),
			new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.Keywords, "print")
		};

		// Token: 0x04000AEC RID: 2796
		private Script originalScript;

		// Token: 0x04000AED RID: 2797
		private string[] propVersions = Array.Empty<string>();

		// Token: 0x04000AEE RID: 2798
		private string[] scriptVersions = Array.Empty<string>();

		// Token: 0x04000AEF RID: 2799
		private ScriptResult scriptResult;

		// Token: 0x04000AF0 RID: 2800
		private List<ParsingError> errors = new List<ParsingError>();

		// Token: 0x020002E6 RID: 742
		private enum Requests
		{
			// Token: 0x04000AF6 RID: 2806
			GetPropVersions,
			// Token: 0x04000AF7 RID: 2807
			GetScriptVersions
		}
	}
}
