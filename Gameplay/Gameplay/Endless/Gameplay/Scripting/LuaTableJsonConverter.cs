using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLua;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004AF RID: 1199
	public static class LuaTableJsonConverter
	{
		// Token: 0x06001DB2 RID: 7602 RVA: 0x000822B0 File Offset: 0x000804B0
		public static string Serialize(LuaTable table)
		{
			return JsonConvert.SerializeObject(LuaTableJsonConverter.LuaTableToObject(table), Formatting.Indented);
		}

		// Token: 0x06001DB3 RID: 7603 RVA: 0x000822C0 File Offset: 0x000804C0
		public static LuaTable Deserialize(Lua lua, string json)
		{
			if (string.IsNullOrEmpty(json))
			{
				return LuaTableJsonConverter.CreateAnonymousTable(lua);
			}
			object obj = LuaTableJsonConverter.NormalizeJsonToken(JToken.Parse(json));
			return LuaTableJsonConverter.ObjectToLuaTable(lua, obj);
		}

		// Token: 0x06001DB4 RID: 7604 RVA: 0x000822F0 File Offset: 0x000804F0
		public static LuaTable ObjectToLuaTable(Lua lua, object obj)
		{
			string text = obj as string;
			if (text != null)
			{
				try
				{
					obj = LuaTableJsonConverter.NormalizeJsonToken(JToken.Parse(text));
					goto IL_002E;
				}
				catch
				{
					goto IL_002E;
				}
			}
			JToken jtoken = obj as JToken;
			if (jtoken != null)
			{
				obj = LuaTableJsonConverter.NormalizeJsonToken(jtoken);
			}
			IL_002E:
			LuaTable luaTable = LuaTableJsonConverter.CreateAnonymousTable(lua);
			IDictionary dictionary = obj as IDictionary;
			if (dictionary != null)
			{
				using (IDictionaryEnumerator enumerator = dictionary.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						object obj2 = enumerator.Current;
						DictionaryEntry dictionaryEntry = (DictionaryEntry)obj2;
						object key = dictionaryEntry.Key;
						object value = dictionaryEntry.Value;
						object obj3 = LuaTableJsonConverter.ConvertToLuaValue(lua, value);
						luaTable[key] = obj3;
					}
					return luaTable;
				}
			}
			IEnumerable enumerable = obj as IEnumerable;
			if (enumerable != null && !(obj is string))
			{
				int num = 1;
				using (IEnumerator enumerator2 = enumerable.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						object obj4 = enumerator2.Current;
						object obj5 = LuaTableJsonConverter.ConvertToLuaValue(lua, obj4);
						luaTable[num] = obj5;
						num++;
					}
					return luaTable;
				}
			}
			luaTable[1] = LuaTableJsonConverter.ConvertToLuaValue(lua, obj);
			return luaTable;
		}

		// Token: 0x06001DB5 RID: 7605 RVA: 0x00082444 File Offset: 0x00080644
		public static object LuaTableToObject(LuaTable table)
		{
			if (LuaTableJsonConverter.IsArrayTable(table))
			{
				List<object> list = new List<object>();
				int count = table.Values.Count;
				for (int i = 1; i <= count; i++)
				{
					object obj = table[i];
					list.Add(LuaTableJsonConverter.ConvertValueToPlain(obj));
				}
				return list;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (object obj2 in table.Keys)
			{
				string text = obj2.ToString();
				object obj3 = table[obj2];
				dictionary[text] = LuaTableJsonConverter.ConvertValueToPlain(obj3);
			}
			return dictionary;
		}

		// Token: 0x06001DB6 RID: 7606 RVA: 0x00082508 File Offset: 0x00080708
		private static LuaTable CreateAnonymousTable(Lua lua)
		{
			return (LuaTable)lua.DoString("return {}", "chunk")[0];
		}

		// Token: 0x06001DB7 RID: 7607 RVA: 0x00082524 File Offset: 0x00080724
		private static object ConvertValueToPlain(object value)
		{
			LuaTable luaTable = value as LuaTable;
			if (luaTable != null)
			{
				return LuaTableJsonConverter.LuaTableToObject(luaTable);
			}
			return value;
		}

		// Token: 0x06001DB8 RID: 7608 RVA: 0x00082544 File Offset: 0x00080744
		private static object ConvertToLuaValue(Lua lua, object obj)
		{
			if (obj is sbyte || obj is byte || obj is short || obj is ushort || obj is int || obj is uint || obj is long || obj is ulong || obj is float || obj is double || obj is decimal)
			{
				return Convert.ToDouble(obj);
			}
			if (obj is IDictionary || (obj is IEnumerable && !(obj is string)))
			{
				return LuaTableJsonConverter.ObjectToLuaTable(lua, obj);
			}
			return obj;
		}

		// Token: 0x06001DB9 RID: 7609 RVA: 0x000825D8 File Offset: 0x000807D8
		private static bool IsArrayTable(LuaTable table)
		{
			List<int> list = new List<int>();
			foreach (object obj in table.Keys)
			{
				if (!(obj is double))
				{
					return false;
				}
				double num = (double)obj;
				if (Math.Abs(num - Math.Truncate(num)) > 5E-324)
				{
					return false;
				}
				int num2 = (int)num;
				if (num2 < 1)
				{
					return false;
				}
				list.Add(num2);
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

		// Token: 0x06001DBA RID: 7610 RVA: 0x000826A8 File Offset: 0x000808A8
		private static object NormalizeJsonToken(JToken token)
		{
			switch (token.Type)
			{
			case JTokenType.Object:
			{
				JObject jobject = (JObject)token;
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				foreach (KeyValuePair<string, JToken> keyValuePair in jobject)
				{
					dictionary[keyValuePair.Key] = LuaTableJsonConverter.NormalizeJsonToken(keyValuePair.Value);
				}
				return dictionary;
			}
			case JTokenType.Array:
			{
				JArray jarray = (JArray)token;
				List<object> list = new List<object>(jarray.Count);
				foreach (JToken jtoken in jarray)
				{
					list.Add(LuaTableJsonConverter.NormalizeJsonToken(jtoken));
				}
				return list;
			}
			case JTokenType.Integer:
			case JTokenType.Float:
				return token.Value<double>();
			case JTokenType.String:
				return token.Value<string>();
			case JTokenType.Boolean:
				return token.Value<bool>();
			case JTokenType.Null:
			case JTokenType.Undefined:
				return null;
			case JTokenType.Date:
				return ((DateTime)token).ToString("o");
			}
			return token.ToString();
		}
	}
}
