using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using Unity.Cloud.UserReporting.Plugin.SimpleJson.Reflection;

namespace Unity.Cloud.UserReporting.Plugin.SimpleJson
{
	// Token: 0x0200002A RID: 42
	[GeneratedCode("simple-json", "1.0.0")]
	public static class SimpleJson
	{
		// Token: 0x06000138 RID: 312 RVA: 0x00006564 File Offset: 0x00004764
		static SimpleJson()
		{
			SimpleJson.EscapeTable[34] = '"';
			SimpleJson.EscapeTable[92] = '\\';
			SimpleJson.EscapeTable[8] = 'b';
			SimpleJson.EscapeTable[12] = 'f';
			SimpleJson.EscapeTable[10] = 'n';
			SimpleJson.EscapeTable[13] = 'r';
			SimpleJson.EscapeTable[9] = 't';
		}

		// Token: 0x06000139 RID: 313 RVA: 0x000065D8 File Offset: 0x000047D8
		public static object DeserializeObject(string json)
		{
			object obj;
			if (SimpleJson.TryDeserializeObject(json, out obj))
			{
				return obj;
			}
			throw new SerializationException("Invalid JSON string");
		}

		// Token: 0x0600013A RID: 314 RVA: 0x000065FC File Offset: 0x000047FC
		public static bool TryDeserializeObject(string json, out object obj)
		{
			bool flag = true;
			if (json != null)
			{
				char[] array = json.ToCharArray();
				int num = 0;
				obj = SimpleJson.ParseValue(array, ref num, ref flag);
			}
			else
			{
				obj = null;
			}
			return flag;
		}

		// Token: 0x0600013B RID: 315 RVA: 0x0000662C File Offset: 0x0000482C
		public static object DeserializeObject(string json, Type type, IJsonSerializerStrategy jsonSerializerStrategy)
		{
			object obj = SimpleJson.DeserializeObject(json);
			if (!(type == null) && (obj == null || !ReflectionUtils.IsAssignableFrom(obj.GetType(), type)))
			{
				return (jsonSerializerStrategy ?? SimpleJson.CurrentJsonSerializerStrategy).DeserializeObject(obj, type);
			}
			return obj;
		}

		// Token: 0x0600013C RID: 316 RVA: 0x0000666D File Offset: 0x0000486D
		public static object DeserializeObject(string json, Type type)
		{
			return SimpleJson.DeserializeObject(json, type, null);
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00006677 File Offset: 0x00004877
		public static T DeserializeObject<T>(string json, IJsonSerializerStrategy jsonSerializerStrategy)
		{
			return (T)((object)SimpleJson.DeserializeObject(json, typeof(T), jsonSerializerStrategy));
		}

		// Token: 0x0600013E RID: 318 RVA: 0x0000668F File Offset: 0x0000488F
		public static T DeserializeObject<T>(string json)
		{
			return (T)((object)SimpleJson.DeserializeObject(json, typeof(T), null));
		}

		// Token: 0x0600013F RID: 319 RVA: 0x000066A8 File Offset: 0x000048A8
		public static string SerializeObject(object json, IJsonSerializerStrategy jsonSerializerStrategy)
		{
			StringBuilder stringBuilder = new StringBuilder(2000);
			if (!SimpleJson.SerializeValue(jsonSerializerStrategy, json, stringBuilder))
			{
				return null;
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000140 RID: 320 RVA: 0x000066D2 File Offset: 0x000048D2
		public static string SerializeObject(object json)
		{
			return SimpleJson.SerializeObject(json, SimpleJson.CurrentJsonSerializerStrategy);
		}

		// Token: 0x06000141 RID: 321 RVA: 0x000066E0 File Offset: 0x000048E0
		public static string EscapeToJavascriptString(string jsonString)
		{
			if (string.IsNullOrEmpty(jsonString))
			{
				return jsonString;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int i = 0;
			while (i < jsonString.Length)
			{
				char c = jsonString[i++];
				if (c == '\\')
				{
					if (jsonString.Length - i >= 2)
					{
						char c2 = jsonString[i];
						if (c2 == '\\')
						{
							stringBuilder.Append('\\');
							i++;
						}
						else if (c2 == '"')
						{
							stringBuilder.Append("\"");
							i++;
						}
						else if (c2 == 't')
						{
							stringBuilder.Append('\t');
							i++;
						}
						else if (c2 == 'b')
						{
							stringBuilder.Append('\b');
							i++;
						}
						else if (c2 == 'n')
						{
							stringBuilder.Append('\n');
							i++;
						}
						else if (c2 == 'r')
						{
							stringBuilder.Append('\r');
							i++;
						}
					}
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000142 RID: 322 RVA: 0x000067C4 File Offset: 0x000049C4
		private static IDictionary<string, object> ParseObject(char[] json, ref int index, ref bool success)
		{
			IDictionary<string, object> dictionary = new JsonObject();
			SimpleJson.NextToken(json, ref index);
			bool flag = false;
			while (!flag)
			{
				int num = SimpleJson.LookAhead(json, index);
				if (num == 0)
				{
					success = false;
					return null;
				}
				if (num == 6)
				{
					SimpleJson.NextToken(json, ref index);
				}
				else
				{
					if (num == 2)
					{
						SimpleJson.NextToken(json, ref index);
						return dictionary;
					}
					string text = SimpleJson.ParseString(json, ref index, ref success);
					if (!success)
					{
						success = false;
						return null;
					}
					num = SimpleJson.NextToken(json, ref index);
					if (num != 5)
					{
						success = false;
						return null;
					}
					object obj = SimpleJson.ParseValue(json, ref index, ref success);
					if (!success)
					{
						success = false;
						return null;
					}
					dictionary[text] = obj;
				}
			}
			return dictionary;
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00006854 File Offset: 0x00004A54
		private static JsonArray ParseArray(char[] json, ref int index, ref bool success)
		{
			JsonArray jsonArray = new JsonArray();
			SimpleJson.NextToken(json, ref index);
			bool flag = false;
			while (!flag)
			{
				int num = SimpleJson.LookAhead(json, index);
				if (num == 0)
				{
					success = false;
					return null;
				}
				if (num == 6)
				{
					SimpleJson.NextToken(json, ref index);
				}
				else
				{
					if (num == 4)
					{
						SimpleJson.NextToken(json, ref index);
						break;
					}
					object obj = SimpleJson.ParseValue(json, ref index, ref success);
					if (!success)
					{
						return null;
					}
					jsonArray.Add(obj);
				}
			}
			return jsonArray;
		}

		// Token: 0x06000144 RID: 324 RVA: 0x000068BC File Offset: 0x00004ABC
		private static object ParseValue(char[] json, ref int index, ref bool success)
		{
			switch (SimpleJson.LookAhead(json, index))
			{
			case 1:
				return SimpleJson.ParseObject(json, ref index, ref success);
			case 3:
				return SimpleJson.ParseArray(json, ref index, ref success);
			case 7:
				return SimpleJson.ParseString(json, ref index, ref success);
			case 8:
				return SimpleJson.ParseNumber(json, ref index, ref success);
			case 9:
				SimpleJson.NextToken(json, ref index);
				return true;
			case 10:
				SimpleJson.NextToken(json, ref index);
				return false;
			case 11:
				SimpleJson.NextToken(json, ref index);
				return null;
			}
			success = false;
			return null;
		}

		// Token: 0x06000145 RID: 325 RVA: 0x0000695C File Offset: 0x00004B5C
		private static string ParseString(char[] json, ref int index, ref bool success)
		{
			StringBuilder stringBuilder = new StringBuilder(2000);
			SimpleJson.EatWhitespace(json, ref index);
			int num = index;
			index = num + 1;
			char c = json[num];
			bool flag = false;
			while (!flag && index != json.Length)
			{
				num = index;
				index = num + 1;
				c = json[num];
				if (c == '"')
				{
					flag = true;
					break;
				}
				if (c == '\\')
				{
					if (index == json.Length)
					{
						break;
					}
					num = index;
					index = num + 1;
					c = json[num];
					if (c == '"')
					{
						stringBuilder.Append('"');
					}
					else if (c == '\\')
					{
						stringBuilder.Append('\\');
					}
					else if (c == '/')
					{
						stringBuilder.Append('/');
					}
					else if (c == 'b')
					{
						stringBuilder.Append('\b');
					}
					else if (c == 'f')
					{
						stringBuilder.Append('\f');
					}
					else if (c == 'n')
					{
						stringBuilder.Append('\n');
					}
					else if (c == 'r')
					{
						stringBuilder.Append('\r');
					}
					else if (c == 't')
					{
						stringBuilder.Append('\t');
					}
					else if (c == 'u')
					{
						if (json.Length - index < 4)
						{
							break;
						}
						uint num2;
						if (!(success = uint.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num2)))
						{
							return "";
						}
						if (55296U <= num2 && num2 <= 56319U)
						{
							index += 4;
							uint num3;
							if (json.Length - index < 6 || !(new string(json, index, 2) == "\\u") || !uint.TryParse(new string(json, index + 2, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num3) || 56320U > num3 || num3 > 57343U)
							{
								success = false;
								return "";
							}
							stringBuilder.Append((char)num2);
							stringBuilder.Append((char)num3);
							index += 6;
						}
						else
						{
							stringBuilder.Append(SimpleJson.ConvertFromUtf32((int)num2));
							index += 4;
						}
					}
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			if (!flag)
			{
				success = false;
				return null;
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000146 RID: 326 RVA: 0x00006B64 File Offset: 0x00004D64
		private static string ConvertFromUtf32(int utf32)
		{
			if (utf32 < 0 || utf32 > 1114111)
			{
				throw new ArgumentOutOfRangeException("utf32", "The argument must be from 0 to 0x10FFFF.");
			}
			if (55296 <= utf32 && utf32 <= 57343)
			{
				throw new ArgumentOutOfRangeException("utf32", "The argument must not be in surrogate pair range.");
			}
			if (utf32 < 65536)
			{
				return new string((char)utf32, 1);
			}
			utf32 -= 65536;
			return new string(new char[]
			{
				(char)((utf32 >> 10) + 55296),
				(char)(utf32 % 1024 + 56320)
			});
		}

		// Token: 0x06000147 RID: 327 RVA: 0x00006BF4 File Offset: 0x00004DF4
		private static object ParseNumber(char[] json, ref int index, ref bool success)
		{
			SimpleJson.EatWhitespace(json, ref index);
			int lastIndexOfNumber = SimpleJson.GetLastIndexOfNumber(json, index);
			int num = lastIndexOfNumber - index + 1;
			string text = new string(json, index, num);
			object obj;
			if (text.IndexOf(".", StringComparison.OrdinalIgnoreCase) != -1 || text.IndexOf("e", StringComparison.OrdinalIgnoreCase) != -1)
			{
				double num2;
				success = double.TryParse(new string(json, index, num), NumberStyles.Any, CultureInfo.InvariantCulture, out num2);
				obj = num2;
			}
			else
			{
				long num3;
				success = long.TryParse(new string(json, index, num), NumberStyles.Any, CultureInfo.InvariantCulture, out num3);
				obj = num3;
			}
			index = lastIndexOfNumber + 1;
			return obj;
		}

		// Token: 0x06000148 RID: 328 RVA: 0x00006C90 File Offset: 0x00004E90
		private static int GetLastIndexOfNumber(char[] json, int index)
		{
			int num = index;
			while (num < json.Length && "0123456789+-.eE".IndexOf(json[num]) != -1)
			{
				num++;
			}
			return num - 1;
		}

		// Token: 0x06000149 RID: 329 RVA: 0x00006CBE File Offset: 0x00004EBE
		private static void EatWhitespace(char[] json, ref int index)
		{
			while (index < json.Length && " \t\n\r\b\f".IndexOf(json[index]) != -1)
			{
				index++;
			}
		}

		// Token: 0x0600014A RID: 330 RVA: 0x00006CE0 File Offset: 0x00004EE0
		private static int LookAhead(char[] json, int index)
		{
			int num = index;
			return SimpleJson.NextToken(json, ref num);
		}

		// Token: 0x0600014B RID: 331 RVA: 0x00006CF8 File Offset: 0x00004EF8
		private static int NextToken(char[] json, ref int index)
		{
			SimpleJson.EatWhitespace(json, ref index);
			if (index == json.Length)
			{
				return 0;
			}
			char c = json[index];
			index++;
			if (c <= '[')
			{
				switch (c)
				{
				case '"':
					return 7;
				case '#':
				case '$':
				case '%':
				case '&':
				case '\'':
				case '(':
				case ')':
				case '*':
				case '+':
				case '.':
				case '/':
					break;
				case ',':
					return 6;
				case '-':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					return 8;
				case ':':
					return 5;
				default:
					if (c == '[')
					{
						return 3;
					}
					break;
				}
			}
			else
			{
				if (c == ']')
				{
					return 4;
				}
				if (c == '{')
				{
					return 1;
				}
				if (c == '}')
				{
					return 2;
				}
			}
			index--;
			int num = json.Length - index;
			if (num >= 5 && json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
			{
				index += 5;
				return 10;
			}
			if (num >= 4 && json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
			{
				index += 4;
				return 9;
			}
			if (num >= 4 && json[index] == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 3] == 'l')
			{
				index += 4;
				return 11;
			}
			return 0;
		}

		// Token: 0x0600014C RID: 332 RVA: 0x00006E6C File Offset: 0x0000506C
		private static bool SerializeValue(IJsonSerializerStrategy jsonSerializerStrategy, object value, StringBuilder builder)
		{
			bool flag = true;
			string text = value as string;
			if (text != null)
			{
				flag = SimpleJson.SerializeString(text, builder);
			}
			else
			{
				IDictionary<string, object> dictionary = value as IDictionary<string, object>;
				if (dictionary != null)
				{
					flag = SimpleJson.SerializeObject(jsonSerializerStrategy, dictionary.Keys, dictionary.Values, builder);
				}
				else
				{
					IDictionary<string, string> dictionary2 = value as IDictionary<string, string>;
					if (dictionary2 != null)
					{
						flag = SimpleJson.SerializeObject(jsonSerializerStrategy, dictionary2.Keys, dictionary2.Values, builder);
					}
					else
					{
						IEnumerable enumerable = value as IEnumerable;
						if (enumerable != null)
						{
							flag = SimpleJson.SerializeArray(jsonSerializerStrategy, enumerable, builder);
						}
						else if (SimpleJson.IsNumeric(value))
						{
							flag = SimpleJson.SerializeNumber(value, builder);
						}
						else if (value is bool)
						{
							builder.Append(((bool)value) ? "true" : "false");
						}
						else if (value == null)
						{
							builder.Append("null");
						}
						else
						{
							object obj;
							flag = jsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, out obj);
							if (flag)
							{
								SimpleJson.SerializeValue(jsonSerializerStrategy, obj, builder);
							}
						}
					}
				}
			}
			return flag;
		}

		// Token: 0x0600014D RID: 333 RVA: 0x00006F50 File Offset: 0x00005150
		private static bool SerializeObject(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable keys, IEnumerable values, StringBuilder builder)
		{
			builder.Append("{");
			IEnumerator enumerator = keys.GetEnumerator();
			IEnumerator enumerator2 = values.GetEnumerator();
			bool flag = true;
			while (enumerator.MoveNext() && enumerator2.MoveNext())
			{
				object obj = enumerator.Current;
				object obj2 = enumerator2.Current;
				if (!flag)
				{
					builder.Append(",");
				}
				string text = obj as string;
				if (text != null)
				{
					SimpleJson.SerializeString(text, builder);
				}
				else if (!SimpleJson.SerializeValue(jsonSerializerStrategy, obj2, builder))
				{
					return false;
				}
				builder.Append(":");
				if (!SimpleJson.SerializeValue(jsonSerializerStrategy, obj2, builder))
				{
					return false;
				}
				flag = false;
			}
			builder.Append("}");
			return true;
		}

		// Token: 0x0600014E RID: 334 RVA: 0x00006FF0 File Offset: 0x000051F0
		private static bool SerializeArray(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable anArray, StringBuilder builder)
		{
			builder.Append("[");
			bool flag = true;
			foreach (object obj in anArray)
			{
				if (!flag)
				{
					builder.Append(",");
				}
				if (!SimpleJson.SerializeValue(jsonSerializerStrategy, obj, builder))
				{
					return false;
				}
				flag = false;
			}
			builder.Append("]");
			return true;
		}

		// Token: 0x0600014F RID: 335 RVA: 0x00007078 File Offset: 0x00005278
		private static bool SerializeString(string aString, StringBuilder builder)
		{
			if (aString.IndexOfAny(SimpleJson.EscapeCharacters) == -1)
			{
				builder.Append('"');
				builder.Append(aString);
				builder.Append('"');
				return true;
			}
			builder.Append('"');
			int num = 0;
			char[] array = aString.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				char c = array[i];
				if ((int)c >= SimpleJson.EscapeTable.Length || SimpleJson.EscapeTable[(int)c] == '\0')
				{
					num++;
				}
				else
				{
					if (num > 0)
					{
						builder.Append(array, i - num, num);
						num = 0;
					}
					builder.Append('\\');
					builder.Append(SimpleJson.EscapeTable[(int)c]);
				}
			}
			if (num > 0)
			{
				builder.Append(array, array.Length - num, num);
			}
			builder.Append('"');
			return true;
		}

		// Token: 0x06000150 RID: 336 RVA: 0x00007134 File Offset: 0x00005334
		private static bool SerializeNumber(object number, StringBuilder builder)
		{
			if (number is long)
			{
				builder.Append(((long)number).ToString(CultureInfo.InvariantCulture));
			}
			else if (number is ulong)
			{
				builder.Append(((ulong)number).ToString(CultureInfo.InvariantCulture));
			}
			else if (number is int)
			{
				builder.Append(((int)number).ToString(CultureInfo.InvariantCulture));
			}
			else if (number is uint)
			{
				builder.Append(((uint)number).ToString(CultureInfo.InvariantCulture));
			}
			else if (number is decimal)
			{
				builder.Append(((decimal)number).ToString(CultureInfo.InvariantCulture));
			}
			else if (number is float)
			{
				builder.Append(((float)number).ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				builder.Append(Convert.ToDouble(number, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture));
			}
			return true;
		}

		// Token: 0x06000151 RID: 337 RVA: 0x0000724C File Offset: 0x0000544C
		private static bool IsNumeric(object value)
		{
			return value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal;
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000152 RID: 338 RVA: 0x000072C8 File Offset: 0x000054C8
		// (set) Token: 0x06000153 RID: 339 RVA: 0x000072DE File Offset: 0x000054DE
		public static IJsonSerializerStrategy CurrentJsonSerializerStrategy
		{
			get
			{
				IJsonSerializerStrategy jsonSerializerStrategy;
				if ((jsonSerializerStrategy = SimpleJson._currentJsonSerializerStrategy) == null)
				{
					jsonSerializerStrategy = (SimpleJson._currentJsonSerializerStrategy = SimpleJson.PocoJsonSerializerStrategy);
				}
				return jsonSerializerStrategy;
			}
			set
			{
				SimpleJson._currentJsonSerializerStrategy = value;
			}
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000154 RID: 340 RVA: 0x000072E6 File Offset: 0x000054E6
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static PocoJsonSerializerStrategy PocoJsonSerializerStrategy
		{
			get
			{
				PocoJsonSerializerStrategy pocoJsonSerializerStrategy;
				if ((pocoJsonSerializerStrategy = SimpleJson._pocoJsonSerializerStrategy) == null)
				{
					pocoJsonSerializerStrategy = (SimpleJson._pocoJsonSerializerStrategy = new PocoJsonSerializerStrategy());
				}
				return pocoJsonSerializerStrategy;
			}
		}

		// Token: 0x04000092 RID: 146
		private const int TOKEN_NONE = 0;

		// Token: 0x04000093 RID: 147
		private const int TOKEN_CURLY_OPEN = 1;

		// Token: 0x04000094 RID: 148
		private const int TOKEN_CURLY_CLOSE = 2;

		// Token: 0x04000095 RID: 149
		private const int TOKEN_SQUARED_OPEN = 3;

		// Token: 0x04000096 RID: 150
		private const int TOKEN_SQUARED_CLOSE = 4;

		// Token: 0x04000097 RID: 151
		private const int TOKEN_COLON = 5;

		// Token: 0x04000098 RID: 152
		private const int TOKEN_COMMA = 6;

		// Token: 0x04000099 RID: 153
		private const int TOKEN_STRING = 7;

		// Token: 0x0400009A RID: 154
		private const int TOKEN_NUMBER = 8;

		// Token: 0x0400009B RID: 155
		private const int TOKEN_TRUE = 9;

		// Token: 0x0400009C RID: 156
		private const int TOKEN_FALSE = 10;

		// Token: 0x0400009D RID: 157
		private const int TOKEN_NULL = 11;

		// Token: 0x0400009E RID: 158
		private const int BUILDER_CAPACITY = 2000;

		// Token: 0x0400009F RID: 159
		private static readonly char[] EscapeTable = new char[93];

		// Token: 0x040000A0 RID: 160
		private static readonly char[] EscapeCharacters = new char[] { '"', '\\', '\b', '\f', '\n', '\r', '\t' };

		// Token: 0x040000A1 RID: 161
		private static IJsonSerializerStrategy _currentJsonSerializerStrategy;

		// Token: 0x040000A2 RID: 162
		private static PocoJsonSerializerStrategy _pocoJsonSerializerStrategy;
	}
}
