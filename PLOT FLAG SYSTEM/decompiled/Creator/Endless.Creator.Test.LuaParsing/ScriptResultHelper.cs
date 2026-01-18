using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Runtime.Gameplay.LuaClasses;
using UnityEngine;

namespace Endless.Creator.Test.LuaParsing;

public static class ScriptResultHelper
{
	private static bool hasBuiltDefaultDefinitions = false;

	private static IntellisenseClassEntry[] defaultTypes = new IntellisenseClassEntry[12]
	{
		new IntellisenseClassEntry
		{
			Type = typeof(Context),
			Keywords = new string[1] { "context" }
		},
		new IntellisenseClassEntry
		{
			Type = typeof(Endless.Gameplay.Scripting.Vector3),
			Keywords = new string[1] { "Vector3" }
		},
		new IntellisenseClassEntry
		{
			Type = typeof(Endless.Gameplay.Scripting.Color),
			Keywords = new string[1] { "Color" }
		},
		new IntellisenseClassEntry
		{
			Type = typeof(Endless.Gameplay.Scripting.Game),
			Keywords = new string[1] { "Game" }
		},
		new IntellisenseClassEntry
		{
			Type = typeof(Player),
			Keywords = new string[1] { "Player" }
		},
		new IntellisenseClassEntry
		{
			Type = typeof(PhysicsComponent),
			Keywords = new string[1] { "PhysicsComponent" }
		},
		new IntellisenseClassEntry
		{
			Type = typeof(Endless.Gameplay.Scripting.NpcManager),
			Keywords = new string[1] { "NpcManager" }
		},
		new IntellisenseClassEntry
		{
			Type = typeof(AudioManager),
			Keywords = new string[1] { "AudioManager" }
		},
		new IntellisenseClassEntry
		{
			Type = typeof(NpcConfiguration),
			Keywords = new string[1] { "NpcConfiguration" }
		},
		new IntellisenseClassEntry
		{
			Type = typeof(Endless.Gameplay.Scripting.ResourceManager),
			Keywords = new string[1] { "ResourceManager" }
		},
		new IntellisenseClassEntry
		{
			Type = typeof(LocalizedStringFactory),
			Keywords = new string[1] { "LocalizedString" }
		},
		new IntellisenseClassEntry
		{
			Type = typeof(Endless.Gameplay.Scripting.CameraFadeManager),
			Keywords = new string[1] { "CameraFadeManager" }
		}
	};

	public static string[] DefaultDeclaredVariables = new string[7] { "Vector3", "Color", "Game", "NpcManager", "AudioManager", "ResourceManager", "LocalizedString" };

	private static Dictionary<string, string> defaultKeywordsToTypes = new Dictionary<string, string>();

	private static HashSet<string> defaultRegisteredTypes = new HashSet<string>();

	private static Dictionary<string, List<string>> defaultRegisteredTypeFunctions = new Dictionary<string, List<string>>();

	private static Dictionary<string, List<string>> defaultRegisteredProperties = new Dictionary<string, List<string>>();

	private static Dictionary<string, List<string>> cachedRegisteredTypeFunctions = new Dictionary<string, List<string>>();

	private static List<IScriptInjector> componentInjectors = new List<IScriptInjector>();

	public static void BuildDefaultDefinitions()
	{
		if (hasBuiltDefaultDefinitions)
		{
			return;
		}
		hasBuiltDefaultDefinitions = true;
		IntellisenseClassEntry[] array = defaultTypes;
		foreach (IntellisenseClassEntry intellisenseClassEntry in array)
		{
			string name = intellisenseClassEntry.Type.Name;
			for (int j = 0; j < intellisenseClassEntry.Keywords.Length; j++)
			{
				defaultKeywordsToTypes.TryAdd(intellisenseClassEntry.Keywords[j], name);
			}
			defaultRegisteredTypes.Add(name);
			List<string> value = BuildTableFunctions(intellisenseClassEntry.Type).ToList();
			defaultRegisteredTypeFunctions.TryAdd(name, value);
			List<string> value2 = BuildProperties(intellisenseClassEntry.Type).ToList();
			defaultRegisteredProperties.TryAdd(name, value2);
		}
		List<string> value3 = BuildTableFunctions(typeof(UnityEngine.Vector3)).ToList();
		defaultRegisteredTypeFunctions.TryAdd("unityVector3", value3);
		List<string> list = BuildProperties(typeof(UnityEngine.Vector3)).ToList();
		list.AddRange(new string[3] { "x", "y", "z" });
		defaultRegisteredProperties.TryAdd("unityVector3", list);
	}

	private static HashSet<string> BuildTableFunctions(Type type)
	{
		HashSet<string> hashSet = (from methodInfo in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
			where !methodInfo.IsSpecialName
			select methodInfo.Name).ToHashSet();
		if ((object)type.BaseType == null || type.BaseType == typeof(object) || type.BaseType == typeof(MonoBehaviour))
		{
			return hashSet;
		}
		HashSet<string> hashSet2 = BuildTableFunctions(type.BaseType);
		for (int num = 0; num < hashSet2.Count; num++)
		{
			hashSet.Add(hashSet2.ElementAt(num));
		}
		return hashSet;
	}

	private static HashSet<string> BuildProperties(Type type)
	{
		Type[] blacklistedDeclaringTypes = new Type[2]
		{
			typeof(IEquatable<>),
			typeof(IFormattable)
		};
		HashSet<string> hashSet = (from propertyInfo in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
			where !propertyInfo.IsSpecialName && !blacklistedDeclaringTypes.Contains(propertyInfo.DeclaringType)
			select propertyInfo.Name).ToHashSet();
		if ((object)type.BaseType == null || type.BaseType == typeof(object) || type.BaseType == typeof(MonoBehaviour))
		{
			return hashSet;
		}
		HashSet<string> hashSet2 = BuildProperties(type.BaseType);
		for (int num = 0; num < hashSet2.Count; num++)
		{
			hashSet.Add(hashSet2.ElementAt(num));
		}
		return hashSet;
	}

	public static Dictionary<string, List<string>> GetDefaultTableFunctions()
	{
		if (!hasBuiltDefaultDefinitions)
		{
			BuildDefaultDefinitions();
		}
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<string, List<string>> defaultRegisteredTypeFunction in defaultRegisteredTypeFunctions)
		{
			dictionary.Add(defaultRegisteredTypeFunction.Key, defaultRegisteredTypeFunction.Value.ToList());
		}
		return dictionary;
	}

	public static Dictionary<string, string> GetDefaultVariableToTypes()
	{
		if (!hasBuiltDefaultDefinitions)
		{
			BuildDefaultDefinitions();
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (KeyValuePair<string, string> defaultKeywordsToType in defaultKeywordsToTypes)
		{
			dictionary.Add(defaultKeywordsToType.Key, defaultKeywordsToType.Value);
		}
		return dictionary;
	}

	public static Dictionary<string, List<string>> GetDefaultProperties()
	{
		if (!hasBuiltDefaultDefinitions)
		{
			BuildDefaultDefinitions();
		}
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<string, List<string>> defaultRegisteredProperty in defaultRegisteredProperties)
		{
			dictionary.Add(defaultRegisteredProperty.Key, defaultRegisteredProperty.Value.ToList());
		}
		return dictionary;
	}

	public static HashSet<string> GetDefaultRegisteredTypes()
	{
		if (!hasBuiltDefaultDefinitions)
		{
			BuildDefaultDefinitions();
		}
		return defaultRegisteredTypes.ToHashSet();
	}

	public static void AddIntellisenseEntry(ScriptResult result, Type type, string[] keywords)
	{
		string name = type.Name;
		for (int i = 0; i < keywords.Length; i++)
		{
			result.AddVariableAndType(keywords[i], name);
		}
		result.AddType(name);
		if (!cachedRegisteredTypeFunctions.TryGetValue(name, out var value))
		{
			value = BuildTableFunctions(type).ToList();
			cachedRegisteredTypeFunctions.Add(name, value);
		}
		for (int j = 0; j < value.Count; j++)
		{
			result.AddTableFunction(name, value[j]);
		}
	}

	public static void AddIntellisenseForEnum(ScriptResult result, Type enumType)
	{
		string name = enumType.Name;
		string[] names = Enum.GetNames(enumType);
		result.AddType(name);
		result.AddVariableAndType(name, name);
		string[] array = names;
		foreach (string propertyName in array)
		{
			result.AddProperty(name, propertyName);
		}
	}

	public static void AddIntellisenseEntry(ScriptResult result, string name, List<string> properties)
	{
		result.AddType(name);
		result.AddVariableAndType(name, name);
		foreach (string property in properties)
		{
			result.AddProperty(name, property);
		}
	}

	public static void AddIntellisenseEntryForMethodName(ScriptResult result, string name)
	{
		result.DeclaredFreeFunctions.Add(name);
	}

	public static void AddComponents(ScriptResult scriptResult)
	{
		foreach (ComponentDefinition item in MonoBehaviourSingleton<StageManager>.Instance.ComponentList.AllDefinitions.Concat(MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.AllDefinitions))
		{
			IScriptInjector scriptInjector = item.ComponentBase as IScriptInjector;
			if ((object)scriptInjector?.LuaObjectType != null)
			{
				AddIntellisenseEntry(scriptResult, scriptInjector.LuaObjectType, new string[1] { scriptInjector.LuaObjectName });
			}
		}
	}
}
