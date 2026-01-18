using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004A5 RID: 1189
	public class EndlessScriptComponent : EndlessBehaviour, IStartSubscriber, IPersistantStateSubscriber
	{
		// Token: 0x170005AF RID: 1455
		// (get) Token: 0x06001D4A RID: 7498 RVA: 0x000804EE File Offset: 0x0007E6EE
		public static string ComponentSnippet
		{
			get
			{
				if (EndlessScriptComponent.componentSnippet == null)
				{
					EndlessScriptComponent.componentSnippet = EndlessScriptComponent.CreateComponentSnippet();
				}
				return EndlessScriptComponent.componentSnippet;
			}
		}

		// Token: 0x170005B0 RID: 1456
		// (get) Token: 0x06001D4B RID: 7499 RVA: 0x00080506 File Offset: 0x0007E706
		// (set) Token: 0x06001D4C RID: 7500 RVA: 0x0008050E File Offset: 0x0007E70E
		public bool IsScriptReady { get; private set; }

		// Token: 0x170005B1 RID: 1457
		// (get) Token: 0x06001D4D RID: 7501 RVA: 0x00080517 File Offset: 0x0007E717
		// (set) Token: 0x06001D4E RID: 7502 RVA: 0x0008051F File Offset: 0x0007E71F
		public Context Context { get; private set; }

		// Token: 0x170005B2 RID: 1458
		// (get) Token: 0x06001D4F RID: 7503 RVA: 0x00080528 File Offset: 0x0007E728
		public Script Script
		{
			get
			{
				return this.script;
			}
		}

		// Token: 0x170005B3 RID: 1459
		// (get) Token: 0x06001D50 RID: 7504 RVA: 0x00080530 File Offset: 0x0007E730
		public Lua Lua
		{
			get
			{
				return this.state;
			}
		}

		// Token: 0x06001D51 RID: 7505 RVA: 0x00080538 File Offset: 0x0007E738
		public void Setup(Script script, Prop prop, Component baseTypeComponent)
		{
			this.script = script;
			this.script.UpdateOrganizationData();
			this.prop = prop;
			this.baseTypeComponent = baseTypeComponent;
		}

		// Token: 0x06001D52 RID: 7506 RVA: 0x0008055C File Offset: 0x0007E75C
		public void Initialize()
		{
			if (this.baseTypeComponent == null)
			{
				return;
			}
			this.instanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.gameObject);
			if (this.instanceId == SerializableGuid.Empty)
			{
				this.assetId = SerializableGuid.Empty;
			}
			else
			{
				this.assetId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(this.instanceId);
			}
			this.runtimeBody = this.script.Body;
			this.InitializeScriptDefaults();
		}

		// Token: 0x06001D53 RID: 7507 RVA: 0x000805E4 File Offset: 0x0007E7E4
		public void RunScript()
		{
			if (this.baseTypeComponent == null)
			{
				return;
			}
			try
			{
				this.state.LoadString(this.runtimeBody, "body").Call(Array.Empty<object>());
			}
			catch (Exception ex)
			{
				this.LogException(ex, null);
			}
			this.IsScriptReady = true;
			this.ExecuteFunction("Awake", Array.Empty<object>());
		}

		// Token: 0x06001D54 RID: 7508 RVA: 0x00080658 File Offset: 0x0007E858
		public void EndlessStart()
		{
			if (this.baseTypeComponent == null)
			{
				return;
			}
			if (base.IsServer)
			{
				this.ExecuteFunction("Start", Array.Empty<object>());
			}
		}

		// Token: 0x06001D55 RID: 7509 RVA: 0x00080682 File Offset: 0x0007E882
		public Context GetContext()
		{
			return this.Context;
		}

		// Token: 0x06001D56 RID: 7510 RVA: 0x0008068A File Offset: 0x0007E88A
		private void InsertLine(string line)
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				return;
			}
			this.runtimeBody = this.runtimeBody.Insert(0, line);
			this.insertedLines++;
		}

		// Token: 0x06001D57 RID: 7511 RVA: 0x000806B8 File Offset: 0x0007E8B8
		private void InitializeScriptDefaults()
		{
			foreach (EnumEntry enumEntry in this.script.EnumTypes)
			{
				this.InsertLine(enumEntry.ToSnippet());
			}
			foreach (Type type in EndlessScriptComponent.LuaEnums)
			{
				this.InsertEnum(type);
			}
			this.InsertLine(EndlessScriptComponent.ComponentSnippet);
			this.InsertLine(this.script.GetEventSnippet());
			this.InsertLine(this.script.GetTransformReferenceSnippet(this.prop));
			this.Context = (this.baseTypeComponent as IBaseType).Context;
			foreach (ValueTuple<string, MethodInfo> valueTuple in EndlessScriptComponent.AutoRegisteredMethodInfos)
			{
				string item = valueTuple.Item1;
				MethodInfo item2 = valueTuple.Item2;
				this.state.RegisterFunction(item, this, item2);
			}
			this.state.SetObjectToPath("Vector3", Vector3.Instance);
			this.state.SetObjectToPath("Color", Color.Instance);
			this.state.SetObjectToPath("MyContext", this.Context);
			this.state.SetObjectToPath("Game", Game.Instance);
			this.state.SetObjectToPath("NpcManager", NpcManager.Instance);
			this.state.SetObjectToPath("AudioManager", AudioManager.Instance);
			this.state.SetObjectToPath("ResourceManager", ResourceManager.Instance);
			this.state.SetObjectToPath("CameraFadeManager", CameraFadeManager.Instance);
			this.state.SetObjectToPath("LocalizedString", LocalizedStringFactory.Instance);
			this.PurgeTable("os");
			this.PurgeTable("io");
			this.state.SetObjectToPath("require", null);
			this.ApplyInspectorVariables();
			this.state.DebugHook += this.OnDebug;
		}

		// Token: 0x06001D58 RID: 7512 RVA: 0x000808C0 File Offset: 0x0007EAC0
		private void PurgeTable(string tableName)
		{
			LuaTable luaTable = this.state.GetObjectFromPath(tableName) as LuaTable;
			if (luaTable == null)
			{
				return;
			}
			foreach (object obj in luaTable.Keys)
			{
				string text = (string)obj;
				luaTable[text] = null;
			}
			this.state.SetObjectToPath(tableName, null);
		}

		// Token: 0x06001D59 RID: 7513 RVA: 0x00080940 File Offset: 0x0007EB40
		[return: TupleElementNames(new string[] { "key", "pairs", "name", "id" })]
		public static ValueTuple<string, List<ValueTuple<string, string>>> GatherComponentSnippetNameIdPairs()
		{
			List<ValueTuple<string, string>> list = new List<ValueTuple<string, string>>();
			foreach (ComponentDefinition componentDefinition in MonoBehaviourSingleton<StageManager>.Instance.ComponentList.AllDefinitions.Concat(MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.AllDefinitions))
			{
				IScriptInjector scriptInjector = componentDefinition.ComponentBase as IScriptInjector;
				if (scriptInjector != null && scriptInjector.AllowLuaReference)
				{
					list.Add(new ValueTuple<string, string>(scriptInjector.LuaObjectName, componentDefinition.ComponentId));
				}
			}
			list.Add(new ValueTuple<string, string>("PhysicsComponent", "PhysicsComponent"));
			list.Add(new ValueTuple<string, string>("Player", "Player"));
			return new ValueTuple<string, List<ValueTuple<string, string>>>("Component", list);
		}

		// Token: 0x06001D5A RID: 7514 RVA: 0x00080A14 File Offset: 0x0007EC14
		private static string CreateComponentSnippet()
		{
			ValueTuple<string, List<ValueTuple<string, string>>> valueTuple = EndlessScriptComponent.GatherComponentSnippetNameIdPairs();
			string item = valueTuple.Item1;
			List<ValueTuple<string, string>> item2 = valueTuple.Item2;
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (ValueTuple<string, string> valueTuple2 in item2)
			{
				list.Add(valueTuple2.Item1);
				list2.Add(valueTuple2.Item2);
			}
			return EnumEntry.ToSnippet(item, list.ToArray(), list2.ToArray());
		}

		// Token: 0x06001D5B RID: 7515 RVA: 0x00080AA4 File Offset: 0x0007ECA4
		public void RegisterFunction(string name, object target, string methodName)
		{
			this.RegisterFunction(name, target, target.GetType().GetMethod(methodName));
		}

		// Token: 0x06001D5C RID: 7516 RVA: 0x00080ABA File Offset: 0x0007ECBA
		public void RegisterFunction(string name, object target, MethodInfo methodInfo)
		{
			this.state.RegisterFunction(name, target, methodInfo);
		}

		// Token: 0x06001D5D RID: 7517 RVA: 0x00080ACC File Offset: 0x0007ECCC
		public void RegisterObject(string name, object target)
		{
			try
			{
				this.state[name] = target;
			}
			catch (Exception ex)
			{
				throw new ArgumentException("Error registering object with name of " + name + " of type " + ((target != null) ? target.GetType().Name : null), ex);
			}
		}

		// Token: 0x06001D5E RID: 7518 RVA: 0x00080B24 File Offset: 0x0007ED24
		public void InsertEnum(Type enumType)
		{
			string name = enumType.Name;
			string[] names = Enum.GetNames(enumType);
			int[] array = Enum.GetValues(enumType) as int[];
			this.InsertLine(EnumEntry.ToSnippet(name, names, array));
		}

		// Token: 0x06001D5F RID: 7519 RVA: 0x00080B5C File Offset: 0x0007ED5C
		public void ApplyInspectorVariables()
		{
			PropEntry propEntry;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.TryGetPropEntry(this.instanceId, out propEntry))
			{
				using (List<InspectorScriptValue>.Enumerator enumerator = this.script.InspectorValues.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						InspectorScriptValue scriptInspectorValue = enumerator.Current;
						MemberChange memberChange = propEntry.LuaMemberChanges.FirstOrDefault((MemberChange c) => c.MemberName == scriptInspectorValue.Name);
						if (memberChange != null)
						{
							object obj = memberChange.ToObject();
							this.state[scriptInspectorValue.Name] = (obj.GetType().IsEnum ? ((int)obj) : obj);
						}
						else
						{
							string assemblyQualifiedTypeName = EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(scriptInspectorValue.DataType);
							object defaultObject = scriptInspectorValue.GetDefaultObject(Type.GetType(assemblyQualifiedTypeName));
							this.state[scriptInspectorValue.Name] = (defaultObject.GetType().IsEnum ? ((int)defaultObject) : defaultObject);
						}
					}
					return;
				}
			}
			foreach (InspectorScriptValue inspectorScriptValue in this.script.InspectorValues)
			{
				string assemblyQualifiedTypeName2 = EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(inspectorScriptValue.DataType);
				object defaultObject2 = inspectorScriptValue.GetDefaultObject(Type.GetType(assemblyQualifiedTypeName2));
				this.state[inspectorScriptValue.Name] = (defaultObject2.GetType().IsEnum ? ((int)defaultObject2) : defaultObject2);
			}
		}

		// Token: 0x06001D60 RID: 7520 RVA: 0x00080D30 File Offset: 0x0007EF30
		public void Print(params object[] values)
		{
			if (values != null && values.Length > 0)
			{
				string text = string.Join("\t", values.Select((object v) => ((v != null) ? v.ToString() : null) ?? "nil"));
				NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(this.instanceId, this.assetId, LogType.Log, text, -1));
			}
		}

		// Token: 0x06001D61 RID: 7521 RVA: 0x00080D94 File Offset: 0x0007EF94
		public void Log(object value)
		{
			if (value != null)
			{
				NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(this.instanceId, this.assetId, LogType.Log, value.ToString(), -1));
			}
		}

		// Token: 0x06001D62 RID: 7522 RVA: 0x00080DBC File Offset: 0x0007EFBC
		public void LogWarning(object value)
		{
			if (value != null)
			{
				NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(this.instanceId, this.assetId, LogType.Warning, value.ToString(), -1));
			}
		}

		// Token: 0x06001D63 RID: 7523 RVA: 0x00080DE4 File Offset: 0x0007EFE4
		public void LogError(object value)
		{
			if (value != null)
			{
				Debug.LogError(value, base.gameObject);
				NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(this.instanceId, this.assetId, LogType.Error, value.ToString(), -1));
			}
		}

		// Token: 0x06001D64 RID: 7524 RVA: 0x00080E18 File Offset: 0x0007F018
		public void LogException(Exception exception, string functionName = null)
		{
			try
			{
				LuaException ex = exception as LuaException;
				if (ex != null)
				{
					Match match = Regex.Match(ex.Message, ":(\\d+):(.+)");
					int num = int.Parse(match.Groups[1].Value) - this.insertedLines;
					string value = match.Groups[2].Value;
					Debug.LogError(new LuaException(ex.Message.Replace(":" + match.Groups[1].Value, string.Format(":{0}", num))));
					NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(this.instanceId, this.assetId, LogType.Exception, value, num));
				}
				else
				{
					this.HandleNonNumberedException(exception, functionName);
				}
			}
			catch (Exception)
			{
				this.HandleNonNumberedException(exception, functionName);
			}
			if (exception.InnerException != null)
			{
				this.HandleNonNumberedException(exception.InnerException, functionName);
			}
		}

		// Token: 0x06001D65 RID: 7525 RVA: 0x00080F10 File Offset: 0x0007F110
		private void HandleNonNumberedException(Exception exception, string functionName)
		{
			string text;
			if (string.IsNullOrEmpty(functionName))
			{
				text = string.Empty;
			}
			else
			{
				text = "Error occured in " + functionName + ": ";
			}
			if (exception.InnerException != null)
			{
				text += exception.InnerException.Message;
			}
			else
			{
				text += exception.Message;
			}
			NetworkBehaviourSingleton<UserScriptingConsole>.Instance.AddMessage(new ConsoleMessage(this.instanceId, this.assetId, LogType.Exception, text, -1));
			Debug.LogException(exception);
		}

		// Token: 0x06001D66 RID: 7526 RVA: 0x00002DB0 File Offset: 0x00000FB0
		private void OnDebug(object sender, DebugHookEventArgs e)
		{
		}

		// Token: 0x06001D67 RID: 7527 RVA: 0x00080F8C File Offset: 0x0007F18C
		public void CallDelay(string functionName, float delay, params object[] args)
		{
			if (delay < 0.1f)
			{
				delay = 0.1f;
			}
			if (!this.activeInvokes.ContainsKey(functionName))
			{
				this.activeInvokes.Add(functionName, new List<EndlessScriptComponent.CoroutineWrapper>());
			}
			EndlessScriptComponent.CoroutineWrapper coroutineWrapper = new EndlessScriptComponent.CoroutineWrapper();
			this.activeInvokes[functionName].Add(coroutineWrapper);
			coroutineWrapper.Enumerator = this.DelayFunction(coroutineWrapper, functionName, delay, args);
			base.StartCoroutine(coroutineWrapper.Enumerator);
		}

		// Token: 0x06001D68 RID: 7528 RVA: 0x00080FFC File Offset: 0x0007F1FC
		private IEnumerator DelayFunction(EndlessScriptComponent.CoroutineWrapper wrapper, string functionName, float delay, params object[] args)
		{
			yield return new WaitForSeconds(delay);
			if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				this.ExecuteFunction(functionName, args);
			}
			this.activeInvokes[functionName].Remove(wrapper);
			yield break;
		}

		// Token: 0x06001D69 RID: 7529 RVA: 0x00081028 File Offset: 0x0007F228
		public void CancelDelayedCall(string functionName)
		{
			List<EndlessScriptComponent.CoroutineWrapper> list;
			if (this.activeInvokes.TryGetValue(functionName, out list))
			{
				foreach (EndlessScriptComponent.CoroutineWrapper coroutineWrapper in list)
				{
					base.StopCoroutine(coroutineWrapper.Enumerator);
				}
				list.Clear();
			}
		}

		// Token: 0x06001D6A RID: 7530 RVA: 0x00081094 File Offset: 0x0007F294
		public object[] ExecuteFunction(string functionName, params object[] arguments)
		{
			if (this.CheckScriptReady())
			{
				LuaFunction luaFunction = this.state[functionName] as LuaFunction;
				try
				{
					return (luaFunction != null) ? luaFunction.Call(arguments) : null;
				}
				catch (Exception ex)
				{
					this.LogException(ex, functionName);
					return null;
				}
			}
			this.LogError(functionName + " was called before it was allowed");
			return null;
		}

		// Token: 0x06001D6B RID: 7531 RVA: 0x000810FC File Offset: 0x0007F2FC
		private bool CheckScriptReady()
		{
			if (!this.IsScriptReady)
			{
				this.LogError("Script code cannot be called from or before awake!");
				return false;
			}
			return true;
		}

		// Token: 0x06001D6C RID: 7532 RVA: 0x00081114 File Offset: 0x0007F314
		public void SubscribeToLuaEvent(LuaInterfaceEvent luaEvent, string functionName)
		{
			luaEvent.Subscribe(this, functionName);
		}

		// Token: 0x06001D6D RID: 7533 RVA: 0x0008111E File Offset: 0x0007F31E
		public void UnsubscribeToLuaEvent(LuaInterfaceEvent luaEvent, string functionName)
		{
			luaEvent.Unsubscribe(this, functionName);
		}

		// Token: 0x06001D6E RID: 7534 RVA: 0x00081128 File Offset: 0x0007F328
		public void FireToBoundReceiver(string functionName, object[] args)
		{
			LuaFunction luaFunction = this.state[functionName] as LuaFunction;
			if (luaFunction == null)
			{
				this.LogException(new Exception("No function named " + functionName + " found on target object."), null);
				return;
			}
			try
			{
				luaFunction.Call(args);
			}
			catch (Exception ex)
			{
				this.LogException(ex, null);
			}
		}

		// Token: 0x06001D6F RID: 7535 RVA: 0x00081190 File Offset: 0x0007F390
		public T ExecuteFunction<T>(string functionName, params object[] arguments)
		{
			LuaFunction luaFunction = this.state[functionName] as LuaFunction;
			if (luaFunction != null)
			{
				try
				{
					return (T)((object)luaFunction.Call(arguments).First<object>());
				}
				catch (Exception ex)
				{
					this.LogException(ex, functionName);
					return default(T);
				}
			}
			return default(T);
		}

		// Token: 0x06001D70 RID: 7536 RVA: 0x000811F8 File Offset: 0x0007F3F8
		public bool TryExecuteFunction<T>(string functionName, out T returnValue, params object[] arguments)
		{
			returnValue = default(T);
			LuaFunction luaFunction = this.state[functionName] as LuaFunction;
			if (luaFunction != null)
			{
				try
				{
					object[] array = luaFunction.Call(arguments);
					returnValue = (T)((object)array.First<object>());
					return true;
				}
				catch (Exception ex)
				{
					this.LogException(ex, functionName);
					return false;
				}
				return false;
			}
			return false;
		}

		// Token: 0x06001D71 RID: 7537 RVA: 0x00081260 File Offset: 0x0007F460
		public bool TryExecuteFunction(string functionName, out object[] returnValues, params object[] arguments)
		{
			LuaFunction luaFunction = this.state[functionName] as LuaFunction;
			if (luaFunction != null)
			{
				try
				{
					returnValues = luaFunction.Call(arguments);
					return true;
				}
				catch (Exception ex)
				{
					this.LogException(ex, functionName);
					returnValues = null;
					return false;
				}
			}
			returnValues = null;
			return false;
		}

		// Token: 0x06001D72 RID: 7538 RVA: 0x000812B8 File Offset: 0x0007F4B8
		public void ExecuteReceiver(string functionName, object[] arguments)
		{
			LuaFunction luaFunction = this.state[functionName] as LuaFunction;
			try
			{
				if (luaFunction != null)
				{
					luaFunction.Call(arguments);
				}
			}
			catch (Exception ex)
			{
				this.LogException(ex, functionName);
			}
		}

		// Token: 0x06001D73 RID: 7539 RVA: 0x00081300 File Offset: 0x0007F500
		public void AttemptFireEvent(string name, Context context, params object[] arguments)
		{
			base.StartCoroutine(this.FireEventCoroutine(name, context, arguments));
		}

		// Token: 0x06001D74 RID: 7540 RVA: 0x00081314 File Offset: 0x0007F514
		public void FireEvent(string name, Context context, params object[] arguments)
		{
			if (MonoBehaviourSingleton<EndlessLoop>.Instance.HasAwoken)
			{
				EndlessEventInfo endlessEventInfo = this.script.Events.FirstOrDefault((EndlessEventInfo e) => e.MemberName == name);
				if (endlessEventInfo == null || endlessEventInfo.ParamList.Count != arguments.Length)
				{
					return;
				}
				Context.StaticLastContext = context;
				object eventObject = this.GetEventObject(name);
				if (eventObject == null)
				{
					return;
				}
				MethodInfo method = eventObject.GetType().GetMethod("Invoke");
				for (int i = 0; i < arguments.Length; i++)
				{
					object obj = arguments[i];
					if (obj.GetType() == typeof(long))
					{
						arguments[i] = Convert.ToInt32((long)obj);
					}
					else if (obj.GetType() == typeof(double))
					{
						arguments[i] = Convert.ToSingle(obj);
					}
				}
				try
				{
					method.Invoke(eventObject, new Context[] { context }.Concat(arguments).ToArray<object>());
					return;
				}
				catch (Exception ex)
				{
					this.LogException(ex, name);
					return;
				}
			}
			this.LogError("You cannot fire events from or before awake!");
		}

		// Token: 0x06001D75 RID: 7541 RVA: 0x0008145C File Offset: 0x0007F65C
		private IEnumerator FireEventCoroutine(string name, Context context, params object[] arguments)
		{
			yield return null;
			if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				this.FireEvent(name, context, arguments);
			}
			yield break;
		}

		// Token: 0x06001D76 RID: 7542 RVA: 0x00081480 File Offset: 0x0007F680
		public object GetEventObject(string name)
		{
			int num = this.endlessEventNames.IndexOf(name);
			if (num == -1)
			{
				return null;
			}
			return this.endlessEventList[num];
		}

		// Token: 0x06001D77 RID: 7543 RVA: 0x000814AC File Offset: 0x0007F6AC
		public void AddEndlessEventObject(object endlessEvent, string eventName)
		{
			int num = this.endlessEventNames.IndexOf(eventName);
			if (num == -1)
			{
				this.endlessEventList.Add(endlessEvent);
				this.endlessEventNames.Add(eventName);
				return;
			}
			this.endlessEventList[num] = endlessEvent;
		}

		// Token: 0x06001D78 RID: 7544 RVA: 0x000814F1 File Offset: 0x0007F6F1
		public void ClearEndlessEventObjects()
		{
			this.endlessEventList.Clear();
			this.endlessEventNames.Clear();
		}

		// Token: 0x06001D79 RID: 7545 RVA: 0x0008150C File Offset: 0x0007F70C
		public EndlessEventInfo GetEventInfo(string wireEntryEmitterMemberName)
		{
			return this.script.Events.FirstOrDefault((EndlessEventInfo entry) => entry.MemberName == wireEntryEmitterMemberName);
		}

		// Token: 0x170005B4 RID: 1460
		// (get) Token: 0x06001D7A RID: 7546 RVA: 0x00081542 File Offset: 0x0007F742
		// (set) Token: 0x06001D7B RID: 7547 RVA: 0x0008154A File Offset: 0x0007F74A
		public bool ShouldSaveAndLoad { get; set; } = true;

		// Token: 0x06001D7C RID: 7548 RVA: 0x00081553 File Offset: 0x0007F753
		public object GetSaveState()
		{
			if (this.baseTypeComponent == null)
			{
				return null;
			}
			Context context = (this.baseTypeComponent as IBaseType).Context;
			if (context == null)
			{
				return null;
			}
			return context.ToJson();
		}

		// Token: 0x06001D7D RID: 7549 RVA: 0x00081580 File Offset: 0x0007F780
		public void LoadState(object loadedState)
		{
			if (this.baseTypeComponent == null)
			{
				return;
			}
			IBaseType baseType = this.baseTypeComponent as IBaseType;
			if (baseType == null)
			{
				Debug.LogError(string.Format("{0} is not an {1}", this.baseTypeComponent, "IBaseType"), this.baseTypeComponent.gameObject);
				return;
			}
			Context context = baseType.Context;
			if (context == null)
			{
				return;
			}
			context.LoadFromJson(loadedState as string);
		}

		// Token: 0x04001702 RID: 5890
		[SerializeField]
		private Prop prop;

		// Token: 0x04001703 RID: 5891
		[SerializeField]
		private Script script;

		// Token: 0x04001704 RID: 5892
		[HideInInspector]
		[SerializeField]
		private GameObject prefab;

		// Token: 0x04001705 RID: 5893
		[SerializeField]
		private Component baseTypeComponent;

		// Token: 0x04001706 RID: 5894
		private static string componentSnippet = null;

		// Token: 0x04001707 RID: 5895
		private int insertedLines;

		// Token: 0x04001708 RID: 5896
		private SerializableGuid instanceId;

		// Token: 0x04001709 RID: 5897
		private SerializableGuid assetId;

		// Token: 0x0400170A RID: 5898
		private Lua state = new Lua(true);

		// Token: 0x0400170C RID: 5900
		private ArrayList endlessEventList = new ArrayList();

		// Token: 0x0400170D RID: 5901
		private List<string> endlessEventNames = new List<string>();

		// Token: 0x0400170F RID: 5903
		[TextArea(10, 40)]
		private string runtimeBody;

		// Token: 0x04001710 RID: 5904
		private Dictionary<string, List<EndlessScriptComponent.CoroutineWrapper>> activeInvokes = new Dictionary<string, List<EndlessScriptComponent.CoroutineWrapper>>();

		// Token: 0x04001711 RID: 5905
		public static ValueTuple<string, MethodInfo>[] AutoRegisteredMethodInfos = new ValueTuple<string, MethodInfo>[]
		{
			new ValueTuple<string, MethodInfo>("print", typeof(EndlessScriptComponent).GetMethod("Print")),
			new ValueTuple<string, MethodInfo>("Log", typeof(EndlessScriptComponent).GetMethod("Log")),
			new ValueTuple<string, MethodInfo>("LogWarning", typeof(EndlessScriptComponent).GetMethod("LogWarning")),
			new ValueTuple<string, MethodInfo>("LogError", typeof(EndlessScriptComponent).GetMethod("LogError")),
			new ValueTuple<string, MethodInfo>("Invoke", typeof(EndlessScriptComponent).GetMethod("CallDelay")),
			new ValueTuple<string, MethodInfo>("CancelInvoke", typeof(EndlessScriptComponent).GetMethod("CancelDelayedCall")),
			new ValueTuple<string, MethodInfo>("FireEvent", typeof(EndlessScriptComponent).GetMethod("AttemptFireEvent")),
			new ValueTuple<string, MethodInfo>("SubscribeToLuaEvent", typeof(EndlessScriptComponent).GetMethod("SubscribeToLuaEvent")),
			new ValueTuple<string, MethodInfo>("UnsubscribeToLuaEvent", typeof(EndlessScriptComponent).GetMethod("UnsubscribeToLuaEvent")),
			new ValueTuple<string, MethodInfo>("ResetInspectorValues", typeof(EndlessScriptComponent).GetMethod("ApplyInspectorVariables"))
		};

		// Token: 0x04001712 RID: 5906
		public static Type[] LuaEnums = new Type[]
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

		// Token: 0x020004A6 RID: 1190
		private class CoroutineWrapper
		{
			// Token: 0x170005B5 RID: 1461
			// (get) Token: 0x06001D80 RID: 7552 RVA: 0x000819B1 File Offset: 0x0007FBB1
			// (set) Token: 0x06001D81 RID: 7553 RVA: 0x000819B9 File Offset: 0x0007FBB9
			public IEnumerator Enumerator { get; set; }
		}
	}
}
