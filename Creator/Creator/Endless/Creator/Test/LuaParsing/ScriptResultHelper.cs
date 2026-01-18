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

namespace Endless.Creator.Test.LuaParsing
{
	// Token: 0x02000320 RID: 800
	public static class ScriptResultHelper
	{
		// Token: 0x06000EB6 RID: 3766 RVA: 0x00045EE8 File Offset: 0x000440E8
		public static void BuildDefaultDefinitions()
		{
			if (ScriptResultHelper.hasBuiltDefaultDefinitions)
			{
				return;
			}
			ScriptResultHelper.hasBuiltDefaultDefinitions = true;
			foreach (IntellisenseClassEntry intellisenseClassEntry in ScriptResultHelper.defaultTypes)
			{
				string name = intellisenseClassEntry.Type.Name;
				for (int j = 0; j < intellisenseClassEntry.Keywords.Length; j++)
				{
					ScriptResultHelper.defaultKeywordsToTypes.TryAdd(intellisenseClassEntry.Keywords[j], name);
				}
				ScriptResultHelper.defaultRegisteredTypes.Add(name);
				List<string> list = ScriptResultHelper.BuildTableFunctions(intellisenseClassEntry.Type).ToList<string>();
				ScriptResultHelper.defaultRegisteredTypeFunctions.TryAdd(name, list);
				List<string> list2 = ScriptResultHelper.BuildProperties(intellisenseClassEntry.Type).ToList<string>();
				ScriptResultHelper.defaultRegisteredProperties.TryAdd(name, list2);
			}
			List<string> list3 = ScriptResultHelper.BuildTableFunctions(typeof(global::UnityEngine.Vector3)).ToList<string>();
			ScriptResultHelper.defaultRegisteredTypeFunctions.TryAdd("unityVector3", list3);
			List<string> list4 = ScriptResultHelper.BuildProperties(typeof(global::UnityEngine.Vector3)).ToList<string>();
			list4.AddRange(new string[] { "x", "y", "z" });
			ScriptResultHelper.defaultRegisteredProperties.TryAdd("unityVector3", list4);
		}

		// Token: 0x06000EB7 RID: 3767 RVA: 0x00046020 File Offset: 0x00044220
		private static HashSet<string> BuildTableFunctions(Type type)
		{
			HashSet<string> hashSet = (from methodInfo in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
				where !methodInfo.IsSpecialName
				select methodInfo.Name).ToHashSet<string>();
			if (type.BaseType == null || type.BaseType == typeof(object) || type.BaseType == typeof(MonoBehaviour))
			{
				return hashSet;
			}
			HashSet<string> hashSet2 = ScriptResultHelper.BuildTableFunctions(type.BaseType);
			for (int i = 0; i < hashSet2.Count; i++)
			{
				hashSet.Add(hashSet2.ElementAt(i));
			}
			return hashSet;
		}

		// Token: 0x06000EB8 RID: 3768 RVA: 0x000460E8 File Offset: 0x000442E8
		private static HashSet<string> BuildProperties(Type type)
		{
			Type[] blacklistedDeclaringTypes = new Type[]
			{
				typeof(IEquatable<>),
				typeof(IFormattable)
			};
			HashSet<string> hashSet = (from propertyInfo in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
				where !propertyInfo.IsSpecialName && !blacklistedDeclaringTypes.Contains(propertyInfo.DeclaringType)
				select propertyInfo.Name).ToHashSet<string>();
			if (type.BaseType == null || type.BaseType == typeof(object) || type.BaseType == typeof(MonoBehaviour))
			{
				return hashSet;
			}
			HashSet<string> hashSet2 = ScriptResultHelper.BuildProperties(type.BaseType);
			for (int i = 0; i < hashSet2.Count; i++)
			{
				hashSet.Add(hashSet2.ElementAt(i));
			}
			return hashSet;
		}

		// Token: 0x06000EB9 RID: 3769 RVA: 0x000461C8 File Offset: 0x000443C8
		public static Dictionary<string, List<string>> GetDefaultTableFunctions()
		{
			if (!ScriptResultHelper.hasBuiltDefaultDefinitions)
			{
				ScriptResultHelper.BuildDefaultDefinitions();
			}
			Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
			foreach (KeyValuePair<string, List<string>> keyValuePair in ScriptResultHelper.defaultRegisteredTypeFunctions)
			{
				dictionary.Add(keyValuePair.Key, keyValuePair.Value.ToList<string>());
			}
			return dictionary;
		}

		// Token: 0x06000EBA RID: 3770 RVA: 0x00046240 File Offset: 0x00044440
		public static Dictionary<string, string> GetDefaultVariableToTypes()
		{
			if (!ScriptResultHelper.hasBuiltDefaultDefinitions)
			{
				ScriptResultHelper.BuildDefaultDefinitions();
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> keyValuePair in ScriptResultHelper.defaultKeywordsToTypes)
			{
				dictionary.Add(keyValuePair.Key, keyValuePair.Value);
			}
			return dictionary;
		}

		// Token: 0x06000EBB RID: 3771 RVA: 0x000462B4 File Offset: 0x000444B4
		public static Dictionary<string, List<string>> GetDefaultProperties()
		{
			if (!ScriptResultHelper.hasBuiltDefaultDefinitions)
			{
				ScriptResultHelper.BuildDefaultDefinitions();
			}
			Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
			foreach (KeyValuePair<string, List<string>> keyValuePair in ScriptResultHelper.defaultRegisteredProperties)
			{
				dictionary.Add(keyValuePair.Key, keyValuePair.Value.ToList<string>());
			}
			return dictionary;
		}

		// Token: 0x06000EBC RID: 3772 RVA: 0x0004632C File Offset: 0x0004452C
		public static HashSet<string> GetDefaultRegisteredTypes()
		{
			if (!ScriptResultHelper.hasBuiltDefaultDefinitions)
			{
				ScriptResultHelper.BuildDefaultDefinitions();
			}
			return ScriptResultHelper.defaultRegisteredTypes.ToHashSet<string>();
		}

		// Token: 0x06000EBD RID: 3773 RVA: 0x00046344 File Offset: 0x00044544
		public static void AddIntellisenseEntry(ScriptResult result, Type type, string[] keywords)
		{
			string name = type.Name;
			for (int i = 0; i < keywords.Length; i++)
			{
				result.AddVariableAndType(keywords[i], name);
			}
			result.AddType(name);
			List<string> list;
			if (!ScriptResultHelper.cachedRegisteredTypeFunctions.TryGetValue(name, out list))
			{
				list = ScriptResultHelper.BuildTableFunctions(type).ToList<string>();
				ScriptResultHelper.cachedRegisteredTypeFunctions.Add(name, list);
			}
			for (int j = 0; j < list.Count; j++)
			{
				result.AddTableFunction(name, list[j]);
			}
		}

		// Token: 0x06000EBE RID: 3774 RVA: 0x000463C0 File Offset: 0x000445C0
		public static void AddIntellisenseForEnum(ScriptResult result, Type enumType)
		{
			string name = enumType.Name;
			string[] names = Enum.GetNames(enumType);
			result.AddType(name);
			result.AddVariableAndType(name, name);
			foreach (string text in names)
			{
				result.AddProperty(name, text);
			}
		}

		// Token: 0x06000EBF RID: 3775 RVA: 0x00046404 File Offset: 0x00044604
		public static void AddIntellisenseEntry(ScriptResult result, string name, List<string> properties)
		{
			result.AddType(name);
			result.AddVariableAndType(name, name);
			foreach (string text in properties)
			{
				result.AddProperty(name, text);
			}
		}

		// Token: 0x06000EC0 RID: 3776 RVA: 0x00046464 File Offset: 0x00044664
		public static void AddIntellisenseEntryForMethodName(ScriptResult result, string name)
		{
			result.DeclaredFreeFunctions.Add(name);
		}

		// Token: 0x06000EC1 RID: 3777 RVA: 0x00046474 File Offset: 0x00044674
		public static void AddComponents(ScriptResult scriptResult)
		{
			foreach (ComponentDefinition componentDefinition in MonoBehaviourSingleton<StageManager>.Instance.ComponentList.AllDefinitions.Concat(MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.AllDefinitions))
			{
				IScriptInjector scriptInjector = componentDefinition.ComponentBase as IScriptInjector;
				if (((scriptInjector != null) ? scriptInjector.LuaObjectType : null) != null)
				{
					ScriptResultHelper.AddIntellisenseEntry(scriptResult, scriptInjector.LuaObjectType, new string[] { scriptInjector.LuaObjectName });
				}
			}
		}

		// Token: 0x04000C42 RID: 3138
		private static bool hasBuiltDefaultDefinitions = false;

		// Token: 0x04000C43 RID: 3139
		private static IntellisenseClassEntry[] defaultTypes = new IntellisenseClassEntry[]
		{
			new IntellisenseClassEntry
			{
				Type = typeof(Context),
				Keywords = new string[] { "context" }
			},
			new IntellisenseClassEntry
			{
				Type = typeof(Endless.Gameplay.Scripting.Vector3),
				Keywords = new string[] { "Vector3" }
			},
			new IntellisenseClassEntry
			{
				Type = typeof(Endless.Gameplay.Scripting.Color),
				Keywords = new string[] { "Color" }
			},
			new IntellisenseClassEntry
			{
				Type = typeof(Endless.Gameplay.Scripting.Game),
				Keywords = new string[] { "Game" }
			},
			new IntellisenseClassEntry
			{
				Type = typeof(Player),
				Keywords = new string[] { "Player" }
			},
			new IntellisenseClassEntry
			{
				Type = typeof(PhysicsComponent),
				Keywords = new string[] { "PhysicsComponent" }
			},
			new IntellisenseClassEntry
			{
				Type = typeof(Endless.Gameplay.Scripting.NpcManager),
				Keywords = new string[] { "NpcManager" }
			},
			new IntellisenseClassEntry
			{
				Type = typeof(AudioManager),
				Keywords = new string[] { "AudioManager" }
			},
			new IntellisenseClassEntry
			{
				Type = typeof(NpcConfiguration),
				Keywords = new string[] { "NpcConfiguration" }
			},
			new IntellisenseClassEntry
			{
				Type = typeof(Endless.Gameplay.Scripting.ResourceManager),
				Keywords = new string[] { "ResourceManager" }
			},
			new IntellisenseClassEntry
			{
				Type = typeof(LocalizedStringFactory),
				Keywords = new string[] { "LocalizedString" }
			},
			new IntellisenseClassEntry
			{
				Type = typeof(Endless.Gameplay.Scripting.CameraFadeManager),
				Keywords = new string[] { "CameraFadeManager" }
			}
		};

		// Token: 0x04000C44 RID: 3140
		public static string[] DefaultDeclaredVariables = new string[] { "Vector3", "Color", "Game", "NpcManager", "AudioManager", "ResourceManager", "LocalizedString" };

		// Token: 0x04000C45 RID: 3141
		private static Dictionary<string, string> defaultKeywordsToTypes = new Dictionary<string, string>();

		// Token: 0x04000C46 RID: 3142
		private static HashSet<string> defaultRegisteredTypes = new HashSet<string>();

		// Token: 0x04000C47 RID: 3143
		private static Dictionary<string, List<string>> defaultRegisteredTypeFunctions = new Dictionary<string, List<string>>();

		// Token: 0x04000C48 RID: 3144
		private static Dictionary<string, List<string>> defaultRegisteredProperties = new Dictionary<string, List<string>>();

		// Token: 0x04000C49 RID: 3145
		private static Dictionary<string, List<string>> cachedRegisteredTypeFunctions = new Dictionary<string, List<string>>();

		// Token: 0x04000C4A RID: 3146
		private static List<IScriptInjector> componentInjectors = new List<IScriptInjector>();
	}
}
