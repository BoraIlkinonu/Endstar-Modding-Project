using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Serialization;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using NLua;
using NLua.Event;
using NLua.Exceptions;
using Runtime.Gameplay.LuaClasses;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class EndlessScriptComponent : EndlessBehaviour, IStartSubscriber, IPersistantStateSubscriber
{
	private class CoroutineWrapper
	{
		public IEnumerator Enumerator { get; set; }
	}

	[SerializeField]
	private Prop prop;

	[SerializeField]
	private Script script;

	[HideInInspector]
	[SerializeField]
	private GameObject prefab;

	[SerializeField]
	private Component baseTypeComponent;

	private static string componentSnippet = null;

	private int insertedLines;

	private SerializableGuid instanceId;

	private SerializableGuid assetId;

	private Lua state = new Lua();

	private ArrayList endlessEventList = new ArrayList();

	private List<string> endlessEventNames = new List<string>();

	[TextArea(10, 40)]
	private string runtimeBody;

	private Dictionary<string, List<CoroutineWrapper>> activeInvokes = new Dictionary<string, List<CoroutineWrapper>>();

	public static (string, MethodInfo)[] AutoRegisteredMethodInfos = new(string, MethodInfo)[10]
	{
		("print", typeof(EndlessScriptComponent).GetMethod("Print")),
		("Log", typeof(EndlessScriptComponent).GetMethod("Log")),
		("LogWarning", typeof(EndlessScriptComponent).GetMethod("LogWarning")),
		("LogError", typeof(EndlessScriptComponent).GetMethod("LogError")),
		("Invoke", typeof(EndlessScriptComponent).GetMethod("CallDelay")),
		("CancelInvoke", typeof(EndlessScriptComponent).GetMethod("CancelDelayedCall")),
		("FireEvent", typeof(EndlessScriptComponent).GetMethod("AttemptFireEvent")),
		("SubscribeToLuaEvent", typeof(EndlessScriptComponent).GetMethod("SubscribeToLuaEvent")),
		("UnsubscribeToLuaEvent", typeof(EndlessScriptComponent).GetMethod("UnsubscribeToLuaEvent")),
		("ResetInspectorValues", typeof(EndlessScriptComponent).GetMethod("ApplyInspectorVariables"))
	};

	public static Type[] LuaEnums = new Type[36]
	{
		typeof(CurrentTargetHandlingMode),
		typeof(DamageType),
		typeof(HealthChangeResult),
		typeof(HealthZeroedBehavior),
		typeof(InteractionAnimation),
		typeof(Language),
		typeof(PickupFilter),
		typeof(PropCombatMode),
		typeof(PropDamageMode),
		typeof(PropPhysicsMode),
		typeof(PropMovementMode),
		typeof(SenseShape),
		typeof(SentryDamageLevel),
		typeof(TargetPrioritizationMode),
		typeof(TargetSelectionMode),
		typeof(Team),
		typeof(TeamSense),
		typeof(TextAlignmentOptions),
		typeof(WorldState),
		typeof(CombatMode),
		typeof(DamageMode),
		typeof(PhysicsMode),
		typeof(IdleBehavior),
		typeof(MovementMode),
		typeof(PathfindingRange),
		typeof(NpcGroup),
		typeof(TargetHostilityMode),
		typeof(ZeroHealthTargetMode),
		typeof(NpcSpawnAnimation),
		typeof(TeleportType),
		typeof(Scope),
		typeof(CameraTransition),
		typeof(InputSettings),
		typeof(ResourceCollectionRule),
		typeof(ContextTypes),
		typeof(ThreatLevel)
	};

	public static string ComponentSnippet
	{
		get
		{
			if (componentSnippet == null)
			{
				componentSnippet = CreateComponentSnippet();
			}
			return componentSnippet;
		}
	}

	public bool IsScriptReady { get; private set; }

	public Context Context { get; private set; }

	public Script Script => script;

	public Lua Lua => state;

	public bool ShouldSaveAndLoad { get; set; } = true;

	public void Setup(Script script, Prop prop, Component baseTypeComponent)
	{
		this.script = script;
		this.script.UpdateOrganizationData();
		this.prop = prop;
		this.baseTypeComponent = baseTypeComponent;
	}

	public void Initialize()
	{
		if (!(baseTypeComponent == null))
		{
			instanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.gameObject);
			if (instanceId == SerializableGuid.Empty)
			{
				assetId = SerializableGuid.Empty;
			}
			else
			{
				assetId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
			}
			runtimeBody = script.Body;
			InitializeScriptDefaults();
		}
	}

	public void RunScript()
	{
		if (!(baseTypeComponent == null))
		{
			try
			{
				state.LoadString(runtimeBody, "body").Call();
			}
			catch (Exception exception)
			{
				LogException(exception);
			}
			IsScriptReady = true;
			ExecuteFunction("Awake");
		}
	}

	public void EndlessStart()
	{
		if (!(baseTypeComponent == null) && base.IsServer)
		{
			ExecuteFunction("Start");
		}
	}

	public Context GetContext()
	{
		return Context;
	}

	private void InsertLine(string line)
	{
		if (!string.IsNullOrWhiteSpace(line))
		{
			runtimeBody = runtimeBody.Insert(0, line);
			insertedLines++;
		}
	}

	private void InitializeScriptDefaults()
	{
		foreach (EnumEntry enumType2 in script.EnumTypes)
		{
			InsertLine(enumType2.ToSnippet());
		}
		Type[] luaEnums = LuaEnums;
		foreach (Type enumType in luaEnums)
		{
			InsertEnum(enumType);
		}
		InsertLine(ComponentSnippet);
		InsertLine(script.GetEventSnippet());
		InsertLine(script.GetTransformReferenceSnippet(prop));
		Context = (baseTypeComponent as IBaseType).Context;
		(string, MethodInfo)[] autoRegisteredMethodInfos = AutoRegisteredMethodInfos;
		for (int i = 0; i < autoRegisteredMethodInfos.Length; i++)
		{
			var (path, function) = autoRegisteredMethodInfos[i];
			state.RegisterFunction(path, this, function);
		}
		state.SetObjectToPath("Vector3", Vector3.Instance);
		state.SetObjectToPath("Color", Color.Instance);
		state.SetObjectToPath("MyContext", Context);
		state.SetObjectToPath("Game", Game.Instance);
		state.SetObjectToPath("NpcManager", NpcManager.Instance);
		state.SetObjectToPath("AudioManager", AudioManager.Instance);
		state.SetObjectToPath("ResourceManager", ResourceManager.Instance);
		state.SetObjectToPath("CameraFadeManager", CameraFadeManager.Instance);
		state.SetObjectToPath("LocalizedString", LocalizedStringFactory.Instance);
		PurgeTable("os");
		PurgeTable("io");
		state.SetObjectToPath("require", null);
		ApplyInspectorVariables();
		state.DebugHook += OnDebug;
	}

	private void PurgeTable(string tableName)
	{
		if (!(state.GetObjectFromPath(tableName) is LuaTable luaTable))
		{
			return;
		}
		foreach (string key in luaTable.Keys)
		{
			luaTable[key] = null;
		}
		state.SetObjectToPath(tableName, null);
	}

	public static (string key, List<(string name, string id)> pairs) GatherComponentSnippetNameIdPairs()
	{
		List<(string, string)> list = new List<(string, string)>();
		foreach (ComponentDefinition item in MonoBehaviourSingleton<StageManager>.Instance.ComponentList.AllDefinitions.Concat(MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.AllDefinitions))
		{
			if (item.ComponentBase is IScriptInjector { AllowLuaReference: not false } scriptInjector)
			{
				list.Add((scriptInjector.LuaObjectName, item.ComponentId));
			}
		}
		list.Add(("PhysicsComponent", "PhysicsComponent"));
		list.Add(("Player", "Player"));
		return (key: "Component", pairs: list);
	}

	private static string CreateComponentSnippet()
	{
		(string key, List<(string name, string id)> pairs) tuple = GatherComponentSnippetNameIdPairs();
		string item = tuple.key;
		List<(string name, string id)> item2 = tuple.pairs;
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (var item3 in item2)
		{
			list.Add(item3.name);
			list2.Add(item3.id);
		}
		return EnumEntry.ToSnippet(item, list.ToArray(), list2.ToArray());
	}

	public void RegisterFunction(string name, object target, string methodName)
	{
		RegisterFunction(name, target, target.GetType().GetMethod(methodName));
	}

	public void RegisterFunction(string name, object target, MethodInfo methodInfo)
	{
		state.RegisterFunction(name, target, methodInfo);
	}

	public void RegisterObject(string name, object target)
	{
		try
		{
			state[name] = target;
		}
		catch (Exception innerException)
		{
			throw new ArgumentException("Error registering object with name of " + name + " of type " + target?.GetType().Name, innerException);
		}
	}

	public void InsertEnum(Type enumType)
	{
		string enumName = enumType.Name;
		string[] names = Enum.GetNames(enumType);
		int[] enumValues = Enum.GetValues(enumType) as int[];
		InsertLine(EnumEntry.ToSnippet(enumName, names, enumValues));
	}

	public void ApplyInspectorVariables()
	{
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.TryGetPropEntry(instanceId, out var propEntry))
		{
			foreach (InspectorScriptValue scriptInspectorValue in script.InspectorValues)
			{
				MemberChange memberChange = propEntry.LuaMemberChanges.FirstOrDefault((MemberChange c) => c.MemberName == scriptInspectorValue.Name);
				if (memberChange != null)
				{
					object obj = memberChange.ToObject();
					state[scriptInspectorValue.Name] = (obj.GetType().IsEnum ? ((object)(int)obj) : obj);
				}
				else
				{
					string assemblyQualifiedTypeName = EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(scriptInspectorValue.DataType);
					object defaultObject = scriptInspectorValue.GetDefaultObject(Type.GetType(assemblyQualifiedTypeName));
					state[scriptInspectorValue.Name] = (defaultObject.GetType().IsEnum ? ((object)(int)defaultObject) : defaultObject);
				}
			}
			return;
		}
		foreach (InspectorScriptValue inspectorValue in script.InspectorValues)
		{
			string assemblyQualifiedTypeName2 = EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(inspectorValue.DataType);
			object defaultObject2 = inspectorValue.GetDefaultObject(Type.GetType(assemblyQualifiedTypeName2));
			state[inspectorValue.Name] = (defaultObject2.GetType().IsEnum ? ((object)(int)defaultObject2) : defaultObject2);
		}
	}

	public void Print(params object[] values)
	{
		if (values != null && values.Length > 0)
		{
			string message = string.Join("\t", values.Select((object v) => v?.ToString() ?? "nil"));
			NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(instanceId, assetId, LogType.Log, message));
		}
	}

	public void Log(object value)
	{
		if (value != null)
		{
			NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(instanceId, assetId, LogType.Log, value.ToString()));
		}
	}

	public void LogWarning(object value)
	{
		if (value != null)
		{
			NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(instanceId, assetId, LogType.Warning, value.ToString()));
		}
	}

	public void LogError(object value)
	{
		if (value != null)
		{
			Debug.LogError(value, base.gameObject);
			NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(instanceId, assetId, LogType.Error, value.ToString()));
		}
	}

	public void LogException(Exception exception, string functionName = null)
	{
		try
		{
			if (exception is LuaException ex)
			{
				Match match = Regex.Match(ex.Message, ":(\\d+):(.+)");
				int num = int.Parse(match.Groups[1].Value) - insertedLines;
				string value = match.Groups[2].Value;
				Debug.LogError(new LuaException(ex.Message.Replace(":" + match.Groups[1].Value, $":{num}")));
				NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(instanceId, assetId, LogType.Exception, value, num));
			}
			else
			{
				HandleNonNumberedException(exception, functionName);
			}
		}
		catch (Exception)
		{
			HandleNonNumberedException(exception, functionName);
		}
		if (exception.InnerException != null)
		{
			HandleNonNumberedException(exception.InnerException, functionName);
		}
	}

	private void HandleNonNumberedException(Exception exception, string functionName)
	{
		string text = ((!string.IsNullOrEmpty(functionName)) ? ("Error occured in " + functionName + ": ") : string.Empty);
		text = ((exception.InnerException == null) ? (text + exception.Message) : (text + exception.InnerException.Message));
		NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(instanceId, assetId, LogType.Exception, text));
		Debug.LogException(exception);
	}

	private void OnDebug(object sender, DebugHookEventArgs e)
	{
	}

	public void CallDelay(string functionName, float delay, params object[] args)
	{
		if (delay < 0.1f)
		{
			delay = 0.1f;
		}
		if (!activeInvokes.ContainsKey(functionName))
		{
			activeInvokes.Add(functionName, new List<CoroutineWrapper>());
		}
		CoroutineWrapper coroutineWrapper = new CoroutineWrapper();
		activeInvokes[functionName].Add(coroutineWrapper);
		coroutineWrapper.Enumerator = DelayFunction(coroutineWrapper, functionName, delay, args);
		StartCoroutine(coroutineWrapper.Enumerator);
	}

	private IEnumerator DelayFunction(CoroutineWrapper wrapper, string functionName, float delay, params object[] args)
	{
		yield return new WaitForSeconds(delay);
		if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			ExecuteFunction(functionName, args);
		}
		activeInvokes[functionName].Remove(wrapper);
	}

	public void CancelDelayedCall(string functionName)
	{
		if (!activeInvokes.TryGetValue(functionName, out var value))
		{
			return;
		}
		foreach (CoroutineWrapper item in value)
		{
			StopCoroutine(item.Enumerator);
		}
		value.Clear();
	}

	public object[] ExecuteFunction(string functionName, params object[] arguments)
	{
		if (CheckScriptReady())
		{
			LuaFunction luaFunction = state[functionName] as LuaFunction;
			try
			{
				return luaFunction?.Call(arguments);
			}
			catch (Exception exception)
			{
				LogException(exception, functionName);
				return null;
			}
		}
		LogError(functionName + " was called before it was allowed");
		return null;
	}

	private bool CheckScriptReady()
	{
		if (!IsScriptReady)
		{
			LogError("Script code cannot be called from or before awake!");
			return false;
		}
		return true;
	}

	public void SubscribeToLuaEvent(LuaInterfaceEvent luaEvent, string functionName)
	{
		luaEvent.Subscribe(this, functionName);
	}

	public void UnsubscribeToLuaEvent(LuaInterfaceEvent luaEvent, string functionName)
	{
		luaEvent.Unsubscribe(this, functionName);
	}

	public void FireToBoundReceiver(string functionName, object[] args)
	{
		if (!(state[functionName] is LuaFunction luaFunction))
		{
			LogException(new Exception("No function named " + functionName + " found on target object."));
			return;
		}
		try
		{
			luaFunction.Call(args);
		}
		catch (Exception exception)
		{
			LogException(exception);
		}
	}

	public T ExecuteFunction<T>(string functionName, params object[] arguments)
	{
		if (state[functionName] is LuaFunction luaFunction)
		{
			try
			{
				return (T)luaFunction.Call(arguments).First();
			}
			catch (Exception exception)
			{
				LogException(exception, functionName);
				return default(T);
			}
		}
		return default(T);
	}

	public bool TryExecuteFunction<T>(string functionName, out T returnValue, params object[] arguments)
	{
		returnValue = default(T);
		if (state[functionName] is LuaFunction luaFunction)
		{
			try
			{
				object[] source = luaFunction.Call(arguments);
				returnValue = (T)source.First();
				return true;
			}
			catch (Exception exception)
			{
				LogException(exception, functionName);
				return false;
			}
		}
		return false;
	}

	public bool TryExecuteFunction(string functionName, out object[] returnValues, params object[] arguments)
	{
		if (state[functionName] is LuaFunction luaFunction)
		{
			try
			{
				returnValues = luaFunction.Call(arguments);
				return true;
			}
			catch (Exception exception)
			{
				LogException(exception, functionName);
				returnValues = null;
				return false;
			}
		}
		returnValues = null;
		return false;
	}

	public void ExecuteReceiver(string functionName, object[] arguments)
	{
		LuaFunction luaFunction = state[functionName] as LuaFunction;
		try
		{
			luaFunction?.Call(arguments);
		}
		catch (Exception exception)
		{
			LogException(exception, functionName);
		}
	}

	public void AttemptFireEvent(string name, Context context, params object[] arguments)
	{
		StartCoroutine(FireEventCoroutine(name, context, arguments));
	}

	public void FireEvent(string name, Context context, params object[] arguments)
	{
		if (MonoBehaviourSingleton<EndlessLoop>.Instance.HasAwoken)
		{
			EndlessEventInfo endlessEventInfo = script.Events.FirstOrDefault((EndlessEventInfo e) => e.MemberName == name);
			if (endlessEventInfo == null || endlessEventInfo.ParamList.Count != arguments.Length)
			{
				return;
			}
			Context.StaticLastContext = context;
			object eventObject = GetEventObject(name);
			if (eventObject == null)
			{
				return;
			}
			MethodInfo method = eventObject.GetType().GetMethod("Invoke");
			for (int num = 0; num < arguments.Length; num++)
			{
				object obj = arguments[num];
				if (obj.GetType() == typeof(long))
				{
					arguments[num] = Convert.ToInt32((long)obj);
				}
				else if (obj.GetType() == typeof(double))
				{
					arguments[num] = Convert.ToSingle(obj);
				}
			}
			try
			{
				method.Invoke(eventObject, new Context[1] { context }.Concat(arguments).ToArray());
				return;
			}
			catch (Exception exception)
			{
				LogException(exception, name);
				return;
			}
		}
		LogError("You cannot fire events from or before awake!");
	}

	private IEnumerator FireEventCoroutine(string name, Context context, params object[] arguments)
	{
		yield return null;
		if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			FireEvent(name, context, arguments);
		}
	}

	public object GetEventObject(string name)
	{
		int num = endlessEventNames.IndexOf(name);
		if (num == -1)
		{
			return null;
		}
		return endlessEventList[num];
	}

	public void AddEndlessEventObject(object endlessEvent, string eventName)
	{
		int num = endlessEventNames.IndexOf(eventName);
		if (num == -1)
		{
			endlessEventList.Add(endlessEvent);
			endlessEventNames.Add(eventName);
		}
		else
		{
			endlessEventList[num] = endlessEvent;
		}
	}

	public void ClearEndlessEventObjects()
	{
		endlessEventList.Clear();
		endlessEventNames.Clear();
	}

	public EndlessEventInfo GetEventInfo(string wireEntryEmitterMemberName)
	{
		return script.Events.FirstOrDefault((EndlessEventInfo entry) => entry.MemberName == wireEntryEmitterMemberName);
	}

	public object GetSaveState()
	{
		if (baseTypeComponent == null)
		{
			return null;
		}
		return (baseTypeComponent as IBaseType).Context?.ToJson();
	}

	public void LoadState(object loadedState)
	{
		if (!(baseTypeComponent == null))
		{
			if (!(baseTypeComponent is IBaseType baseType))
			{
				Debug.LogError(string.Format("{0} is not an {1}", baseTypeComponent, "IBaseType"), baseTypeComponent.gameObject);
			}
			else
			{
				baseType.Context?.LoadFromJson(loadedState as string);
			}
		}
	}
}
