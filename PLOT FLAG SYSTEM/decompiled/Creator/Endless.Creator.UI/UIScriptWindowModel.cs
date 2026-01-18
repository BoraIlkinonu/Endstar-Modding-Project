using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace Endless.Creator.UI;

public class UIScriptWindowModel : UIGameObject, IUILoadingSpinnerViewCompatible
{
	private enum Requests
	{
		GetPropVersions,
		GetScriptVersions
	}

	[SerializeField]
	private Script activeScript;

	[SerializeField]
	private UIUserScriptAutocompleteListModel userScriptAutocompleteListModel;

	[Header("Script Data")]
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

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	[SerializeField]
	private bool superVerboseLogging;

	private readonly LuaIntellisensor luaIntellisensor = new LuaIntellisensor();

	private readonly Dictionary<Vector2Int, Token> tokenDictionary = new Dictionary<Vector2Int, Token>();

	private readonly UIMultiRequestLoadingSpinnerEventHandler<Requests> multiRequestLoadingSpinnerEventHandler = new UIMultiRequestLoadingSpinnerEventHandler<Requests>();

	private readonly List<UIUserScriptAutocompleteListModelItem> autoComplete = new List<UIUserScriptAutocompleteListModelItem>();

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

	private Script originalScript;

	private string[] propVersions = Array.Empty<string>();

	private string[] scriptVersions = Array.Empty<string>();

	private ScriptResult scriptResult;

	private List<ParsingError> errors = new List<ParsingError>();

	public SerializableGuid PropId { get; private set; } = SerializableGuid.Empty;

	public UnityEvent OnModelChanged { get; } = new UnityEvent();

	public string ScriptBody => activeScript.Body;

	public IReadOnlyList<UIUserScriptAutocompleteListModelItem> AutoComplete => autoComplete;

	public bool IsChanged => JsonConvert.SerializeObject(originalScript) != JsonConvert.SerializeObject(activeScript);

	public IReadOnlyList<Token> Tokens
	{
		get
		{
			if (scriptResult != null)
			{
				return scriptResult.Tokens;
			}
			return Array.Empty<Token>();
		}
	}

	public IReadOnlyList<ParsingError> Errors => errors;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		enumEntryListModel.ItemRemovedUnityEvent.AddListener(OnEnumEntryRemoved);
		inspectorScriptValueListModel.ItemRemovedUnityEvent.AddListener(OnInspectorScriptValueRemoved);
		wiringReceiverListModel.ItemRemovedUnityEvent.AddListener(OnWireEventRemoved);
		wiringEventListModel.ItemRemovedUnityEvent.AddListener(OnWireReceiverRemoved);
		scriptReferenceListModel.ItemRemovedUnityEvent.AddListener(OnScriptReferenceRemoved);
		MonoBehaviourSingleton<UIScriptDataWizard>.Instance.OnUpdatedScript.AddListener(SetScript);
	}

	public Prop GetCloneOfProp()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetCloneOfProp");
		}
		return GetRuntimePropInfo().PropData.Clone();
	}

	public Script GetCloneOfScript()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetCloneOfScript");
		}
		return activeScript.Clone();
	}

	public PropLibrary.RuntimePropInfo GetRuntimePropInfo()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetRuntimePropInfo");
		}
		return MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(PropId);
	}

	public void Initialize(Script newScript, SerializableGuid propId)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", newScript, propId);
		}
		originalScript = newScript.Clone();
		PropId = propId;
		SetScript(newScript);
		GetPropVersions(propId);
	}

	public void SetScriptBody(string newValue)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetScriptBody", newValue);
		}
		activeScript.Body = newValue;
		tokenDictionary.Clear();
		errors.Clear();
		try
		{
			ScriptResult scriptResult = (this.scriptResult = luaIntellisensor.Execute(activeScript.Body));
			ScriptResultHelper.AddComponents(this.scriptResult);
			this.scriptResult.AddVariableAndType("MyContext", "Context");
			foreach (InspectorScriptValue inspectorValue in activeScript.InspectorValues)
			{
				this.scriptResult.DeclaredVariables.Add(inspectorValue.Name);
				Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(inspectorValue.DataType);
				if (typeFromId != null)
				{
					ScriptResultHelper.AddIntellisenseEntry(this.scriptResult, typeFromId, new string[1] { inspectorValue.Name });
					this.scriptResult.AddVariableAndType(inspectorValue.Name, typeFromId.Name);
				}
			}
			Type[] luaEnums = EndlessScriptComponent.LuaEnums;
			foreach (Type enumType in luaEnums)
			{
				ScriptResultHelper.AddIntellisenseForEnum(this.scriptResult, enumType);
			}
			var (text, properties) = activeScript.GetEvents();
			ScriptResultHelper.AddIntellisenseEntry(this.scriptResult, text, properties);
			var (text2, source) = EndlessScriptComponent.GatherComponentSnippetNameIdPairs();
			ScriptResultHelper.AddIntellisenseEntry(this.scriptResult, text2, source.Select<(string, string), string>(((string name, string id) pair) => pair.name).ToList());
			var (text3, properties2) = activeScript.GetTransforms();
			ScriptResultHelper.AddIntellisenseEntry(this.scriptResult, text3, properties2);
			(string, MethodInfo)[] autoRegisteredMethodInfos = EndlessScriptComponent.AutoRegisteredMethodInfos;
			for (int i = 0; i < autoRegisteredMethodInfos.Length; i++)
			{
				string item = autoRegisteredMethodInfos[i].Item1;
				ScriptResultHelper.AddIntellisenseEntryForMethodName(this.scriptResult, item);
			}
			foreach (Token token in this.scriptResult.Tokens)
			{
				Vector2Int key = new Vector2Int(token.StartIndex, token.StartIndex + token.Length);
				tokenDictionary.Add(key, token);
			}
			errors = scriptResult.Errors;
		}
		catch (Exception exception)
		{
			DebugUtility.LogException(exception, this);
		}
		OnModelChanged.Invoke();
	}

	public void UpdateAutoComplete(string textBehindCaret, string filter, char? accessOperator)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateAutoComplete", textBehindCaret, filter, (!accessOperator.HasValue) ? "null" : ((object)accessOperator));
		}
		autoComplete.Clear();
		if (filter.IsNullOrEmptyOrWhiteSpace())
		{
			if (textBehindCaret.IsNullOrEmptyOrWhiteSpace())
			{
				userScriptAutocompleteListModel.Clear(triggerEvents: true);
				return;
			}
			foreach (UIUserScriptAutocompleteListModelItem item3 in builtInAutoComplete)
			{
				TryToAddToAutoComplete(item3, textBehindCaret);
			}
			foreach (string item4 in (scriptResult == null) ? new List<string>() : scriptResult.DeclaredVariables)
			{
				UIUserScriptAutocompleteListModelItem item = new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.VariablesAndProperties, item4);
				TryToAddToAutoComplete(item, textBehindCaret);
			}
			foreach (UIUserScriptAutocompleteListModelItem item5 in ((scriptResult == null) ? new List<string>() : scriptResult.DeclaredFreeFunctions).Select((string declaredFreeFunction) => new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.FunctionsAndMethods, declaredFreeFunction)))
			{
				TryToAddToAutoComplete(item5, textBehindCaret);
			}
		}
		else
		{
			List<string> list = new List<string>();
			try
			{
				if (accessOperator.HasValue)
				{
					if (accessOperator.Value == '.')
					{
						list = scriptResult.GetPropertiesCallableByVariable(filter);
					}
					else if (accessOperator.Value == ':')
					{
						list = scriptResult.GetFunctionsCallableByVariable(filter);
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
				userScriptAutocompleteListModel.Set(autoComplete, triggerEvents: true);
			}
			foreach (string item6 in list)
			{
				UIUserScriptAutocompleteListModelItem item2 = new UIUserScriptAutocompleteListModelItem(TokenGroupTypes.FunctionsAndMethods, item6);
				TryToAddToAutoComplete(item2, textBehindCaret);
			}
		}
		userScriptAutocompleteListModel.Set(autoComplete, triggerEvents: true);
	}

	public void Revert()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Revert");
		}
		SetScript(originalScript);
	}

	public async void SetScript(Script newScript)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetScript", newScript.AssetID);
		}
		DebugUtility.LogMethod(this, "SetScript", newScript.AssetID);
		activeScript = newScript.Clone();
		enumEntryListModel.Set(activeScript.EnumTypes, triggerEvents: true);
		inspectorScriptValueListModel.Set(activeScript.InspectorValues, triggerEvents: true);
		wiringReceiverListModel.Set(activeScript.Receivers, triggerEvents: true);
		wiringEventListModel.Set(activeScript.Events, triggerEvents: true);
		scriptReferenceListModel.Set(activeScript.ScriptReferences, triggerEvents: true);
		if (!multiRequestLoadingSpinnerEventHandler.Initialized)
		{
			multiRequestLoadingSpinnerEventHandler.Initialize(this);
		}
		multiRequestLoadingSpinnerEventHandler.TrackRequest(Requests.GetScriptVersions);
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(originalScript.AssetID);
		if (graphQlResult.HasErrors)
		{
			multiRequestLoadingSpinnerEventHandler.UntrackRequest(Requests.GetScriptVersions);
			ErrorHandler.HandleError(ErrorCodes.UIScriptWindowModel_GetScriptVersions, graphQlResult.GetErrorMessage());
		}
		else
		{
			multiRequestLoadingSpinnerEventHandler.UntrackRequest(Requests.GetScriptVersions);
			scriptVersions = JsonConvert.DeserializeObject<string[]>(graphQlResult.GetDataMember().ToString());
			scriptVersions = scriptVersions.OrderByDescending(Version.Parse).ToArray();
		}
		SetScriptBody(activeScript.Body);
	}

	public async void GetPropVersions(SerializableGuid assetId)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetPropVersions", assetId);
		}
		multiRequestLoadingSpinnerEventHandler.TrackRequest(Requests.GetPropVersions);
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(assetId);
		if (graphQlResult.HasErrors)
		{
			multiRequestLoadingSpinnerEventHandler.UntrackRequest(Requests.GetPropVersions);
			ErrorHandler.HandleError(ErrorCodes.UIScriptWindowModel_GetPropVersions, graphQlResult.GetErrorMessage());
		}
		else
		{
			multiRequestLoadingSpinnerEventHandler.UntrackRequest(Requests.GetPropVersions);
			propVersions = VersionUtilities.GetParsedAndOrderedVersions(graphQlResult.GetDataMember());
		}
	}

	public void SetOpenSource(bool openSource)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetOpenSource", openSource);
		}
		activeScript.OpenSource = openSource;
		OnModelChanged.Invoke();
	}

	public void ApplySaveScriptAndPropAndApplyToGameResult(UISaveScriptAndPropAndApplyToGameHandler.Result result)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplySaveScriptAndPropAndApplyToGameResult", result);
		}
		if (result.Script != null)
		{
			originalScript = result.Script.Clone();
			SetScript(result.Script);
		}
		OnModelChanged.Invoke();
	}

	private void TryToAddToAutoComplete(UIUserScriptAutocompleteListModelItem item, string textBehindCaret)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "TryToAddToAutoComplete", item, textBehindCaret);
		}
		if (autoComplete.Any((UIUserScriptAutocompleteListModelItem existing) => existing.Value == item.Value))
		{
			DebugUtility.Log("Not added since item.Value|" + item.Value + "| already exists in auto complete options.");
			return;
		}
		if (item.Value == textBehindCaret)
		{
			if (superVerboseLogging)
			{
				DebugUtility.Log("Not added since item.Value|" + item.Value + "| is exactly equal to ( textBehindCaret: " + textBehindCaret + " ).", this);
			}
			return;
		}
		textBehindCaret = textBehindCaret.ToLower();
		if (!item.Value.ToLower().StartsWith(textBehindCaret))
		{
			if (superVerboseLogging)
			{
				DebugUtility.Log("Not added since item.Value|" + item.Value + "|.StartsWith( textBehindCaret: " + textBehindCaret + " ) is FALSE!", this);
			}
		}
		else
		{
			autoComplete.Add(item);
		}
	}

	private void OnEnumEntryRemoved(int index, EnumEntry removedItem)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnumEntryRemoved", index, removedItem);
		}
		OnModelChanged.Invoke();
	}

	private void OnInspectorScriptValueRemoved(int index, InspectorScriptValue removedItem)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnInspectorScriptValueRemoved", index, removedItem);
		}
		for (int i = 0; i < activeScript.InspectorOrganizationData.Count; i++)
		{
			InspectorOrganizationData inspectorOrganizationData = activeScript.InspectorOrganizationData[i];
			if (!(inspectorOrganizationData.MemberName != removedItem.Name) && inspectorOrganizationData.DataType == removedItem.DataType)
			{
				activeScript.InspectorOrganizationData.RemoveAt(i);
				break;
			}
		}
		OnModelChanged.Invoke();
	}

	private void OnWireEventRemoved(int index, EndlessEventInfo removedItem)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnWireEventRemoved", index, removedItem);
		}
		OnModelChanged.Invoke();
	}

	private void OnWireReceiverRemoved(int index, EndlessEventInfo removedItem)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnWireReceiverRemoved", index, removedItem);
		}
		OnModelChanged.Invoke();
	}

	private void OnScriptReferenceRemoved(int index, ScriptReference removedItem)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnScriptReferenceRemoved", index, removedItem);
		}
		OnModelChanged.Invoke();
	}
}
