using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.Scripting;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIScriptDataWizard : MonoBehaviourSingleton<UIScriptDataWizard>
{
	[SerializeField]
	private UIScriptDataTypeSelectionModalView scriptDataTypeSelectionModalSource;

	[SerializeField]
	private UIInspectorScriptValueTypeSelectionModalView inspectorScriptValueTypeSelectionModalSource;

	[SerializeField]
	private UIInspectorScriptValueInputModalView inspectorScriptValueInputModalSource;

	[SerializeField]
	private UIScriptEventInputModalView scriptEventInputModalSource;

	[SerializeField]
	private UIScriptReferenceInputModalView scriptReferenceInputModalSource;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	[SerializeField]
	private bool superVerboseLogging;

	private ScriptDataTypes scriptDataType;

	private Prop prop;

	private Script script;

	private readonly HashSet<string> invalidNames = new HashSet<string>();

	public UnityEvent<Script> OnUpdatedScript { get; } = new UnityEvent<Script>();

	public SerializableGuid PropAssetId => prop.AssetID;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		Type[] luaEnums = EndlessScriptComponent.LuaEnums;
		foreach (Type type in luaEnums)
		{
			invalidNames.Add(type.Name);
		}
	}

	public bool IsValidScriptingName(string input)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "IsValidScriptingName", input);
		}
		if (string.IsNullOrEmpty(input))
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(input))
		{
			return false;
		}
		if (input.Length == 0)
		{
			return false;
		}
		if (char.IsNumber(input[0]))
		{
			return false;
		}
		if (input.Any((char character) => !char.IsLetterOrDigit(character)))
		{
			return false;
		}
		if (invalidNames.Contains(input))
		{
			return false;
		}
		return true;
	}

	public Prop GetCloneOfProp()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetCloneOfProp");
		}
		return prop.Clone();
	}

	public Script GetCloneOfScript()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetCloneOfScript");
		}
		return script.Clone();
	}

	public void Initialize(Prop prop, Script script)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", prop.AssetID, script.AssetID);
		}
		this.prop = prop;
		this.script = script;
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(scriptDataTypeSelectionModalSource, UIModalManagerStackActions.MaintainStack);
	}

	public void SetScriptDataType(ScriptDataTypes newScriptDataType)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetScriptDataType", scriptDataType);
		}
		scriptDataType = newScriptDataType;
		bool flag = false;
		switch (scriptDataType)
		{
		case ScriptDataTypes.Enum:
			DebugUtility.LogException(new NotImplementedException(), this);
			break;
		case ScriptDataTypes.InspectorScriptValue:
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(inspectorScriptValueTypeSelectionModalSource, UIModalManagerStackActions.MaintainStack);
			break;
		case ScriptDataTypes.Event:
			flag = true;
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(scriptEventInputModalSource, UIModalManagerStackActions.MaintainStack, flag);
			break;
		case ScriptDataTypes.Receiver:
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(scriptEventInputModalSource, UIModalManagerStackActions.MaintainStack, flag);
			break;
		case ScriptDataTypes.ScriptReferences:
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(scriptReferenceInputModalSource, UIModalManagerStackActions.MaintainStack);
			break;
		default:
			DebugUtility.LogNoEnumSupportError(this, "SetScriptDataType", scriptDataType, scriptDataType);
			break;
		}
	}

	public void Back()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Back");
		}
	}

	public void Exit()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Exit");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
	}

	public void UpdateScriptAndExit(Script script)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateScriptAndExit", script.AssetID);
		}
		OnUpdatedScript.Invoke(script);
		Exit();
	}

	public void UpdateScript(Script script)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateScript", script.AssetID);
		}
		this.script = script;
		OnUpdatedScript.Invoke(script);
	}
}
