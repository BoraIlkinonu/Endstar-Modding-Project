using System;
using System.Collections;
using System.Collections.Generic;
using NLua;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Endless.Gameplay.Scripting;

public static class LuaTableJsonConverter
{
	public static string Serialize(LuaTable table)
	{
		return JsonConvert.SerializeObject(LuaTableToObject(table), Formatting.Indented);
	}

	public static LuaTable Deserialize(Lua lua, string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return CreateAnonymousTable(lua);
		}
		object obj = NormalizeJsonToken(JToken.Parse(json));
		return ObjectToLuaTable(lua, obj);
	}

	public static LuaTable ObjectToLuaTable(Lua lua, object obj)
	{
		if (obj is string json)
		{
			try
			{
				obj = NormalizeJsonToken(JToken.Parse(json));
			}
			catch
			{
			}
		}
		else if (obj is JToken token)
		{
			obj = NormalizeJsonToken(token);
		}
		LuaTable luaTable = CreateAnonymousTable(lua);
		if (obj is IDictionary dictionary)
		{
			foreach (DictionaryEntry item in dictionary)
			{
				object key = item.Key;
				object value = item.Value;
				object value2 = ConvertToLuaValue(lua, value);
				luaTable[key] = value2;
			}
		}
		else if (obj is IEnumerable enumerable && !(obj is string))
		{
			int num = 1;
			foreach (object item2 in enumerable)
			{
				object value3 = ConvertToLuaValue(lua, item2);
				luaTable[num] = value3;
				num++;
			}
		}
		else
		{
			luaTable[1] = ConvertToLuaValue(lua, obj);
		}
		return luaTable;
	}

	public static object LuaTableToObject(LuaTable table)
	{
		if (IsArrayTable(table))
		{
			List<object> list = new List<object>();
			int count = table.Values.Count;
			for (int i = 1; i <= count; i++)
			{
				object value = table[i];
				list.Add(ConvertValueToPlain(value));
			}
			return list;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (object key2 in table.Keys)
		{
			string key = key2.ToString();
			object value2 = table[key2];
			dictionary[key] = ConvertValueToPlain(value2);
		}
		return dictionary;
	}

	private static LuaTable CreateAnonymousTable(Lua lua)
	{
		return (LuaTable)lua.DoString("return {}")[0];
	}

	private static object ConvertValueToPlain(object value)
	{
		if (value is LuaTable table)
		{
			return LuaTableToObject(table);
		}
		return value;
	}

	private static object ConvertToLuaValue(Lua lua, object obj)
	{
		if (obj is sbyte || obj is byte || obj is short || obj is ushort || obj is int || obj is uint || obj is long || obj is ulong || obj is float || obj is double || obj is decimal)
		{
			return Convert.ToDouble(obj);
		}
		if (obj is IDictionary || (obj is IEnumerable && !(obj is string)))
		{
			return ObjectToLuaTable(lua, obj);
		}
		return obj;
	}

	private static bool IsArrayTable(LuaTable table)
	{
		List<int> list = new List<int>();
		foreach (object key in table.Keys)
		{
			if (key is double num)
			{
				if (Math.Abs(num - Math.Truncate(num)) > double.Epsilon)
				{
					return false;
				}
				int num2 = (int)num;
				if (num2 < 1)
				{
					return false;
				}
				list.Add(num2);
				continue;
			}
			return false;
		}
		list.Sort();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != i + 1)
			{
				return false;
			}
		}
		return true;
	}

	private static object NormalizeJsonToken(JToken token)
	{
		switch (token.Type)
		{
		case JTokenType.Object:
		{
			JObject obj2 = (JObject)token;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			{
				foreach (KeyValuePair<string, JToken> item in obj2)
				{
					dictionary[item.Key] = NormalizeJsonToken(item.Value);
				}
				return dictionary;
			}
		}
		case JTokenType.Array:
		{
			JArray obj = (JArray)token;
			List<object> list = new List<object>(obj.Count);
			{
				foreach (JToken item2 in obj)
				{
					list.Add(NormalizeJsonToken(item2));
				}
				return list;
			}
		}
		case JTokenType.Integer:
		case JTokenType.Float:
			return token.Value<double>();
		case JTokenType.Boolean:
			return token.Value<bool>();
		case JTokenType.String:
			return token.Value<string>();
		case JTokenType.Null:
		case JTokenType.Undefined:
			return null;
		case JTokenType.Date:
			return ((DateTime)token).ToString("o");
		default:
			return token.ToString();
		}
	}
}
