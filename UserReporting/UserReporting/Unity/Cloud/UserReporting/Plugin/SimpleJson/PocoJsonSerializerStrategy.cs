using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Unity.Cloud.UserReporting.Plugin.SimpleJson.Reflection;

namespace Unity.Cloud.UserReporting.Plugin.SimpleJson
{
	// Token: 0x0200002C RID: 44
	[GeneratedCode("simple-json", "1.0.0")]
	public class PocoJsonSerializerStrategy : IJsonSerializerStrategy
	{
		// Token: 0x06000157 RID: 343 RVA: 0x000072FC File Offset: 0x000054FC
		public PocoJsonSerializerStrategy()
		{
			this.ConstructorCache = new ReflectionUtils.ThreadSafeDictionary<Type, ReflectionUtils.ConstructorDelegate>(new ReflectionUtils.ThreadSafeDictionaryValueFactory<Type, ReflectionUtils.ConstructorDelegate>(this.ContructorDelegateFactory));
			this.GetCache = new ReflectionUtils.ThreadSafeDictionary<Type, IDictionary<string, ReflectionUtils.GetDelegate>>(new ReflectionUtils.ThreadSafeDictionaryValueFactory<Type, IDictionary<string, ReflectionUtils.GetDelegate>>(this.GetterValueFactory));
			this.SetCache = new ReflectionUtils.ThreadSafeDictionary<Type, IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>>(new ReflectionUtils.ThreadSafeDictionaryValueFactory<Type, IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>>(this.SetterValueFactory));
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00007357 File Offset: 0x00005557
		protected virtual string MapClrMemberNameToJsonFieldName(string clrPropertyName)
		{
			return clrPropertyName;
		}

		// Token: 0x06000159 RID: 345 RVA: 0x0000735A File Offset: 0x0000555A
		internal virtual ReflectionUtils.ConstructorDelegate ContructorDelegateFactory(Type key)
		{
			return ReflectionUtils.GetContructor(key, key.IsArray ? PocoJsonSerializerStrategy.ArrayConstructorParameterTypes : PocoJsonSerializerStrategy.EmptyTypes);
		}

		// Token: 0x0600015A RID: 346 RVA: 0x00007378 File Offset: 0x00005578
		internal virtual IDictionary<string, ReflectionUtils.GetDelegate> GetterValueFactory(Type type)
		{
			IDictionary<string, ReflectionUtils.GetDelegate> dictionary = new Dictionary<string, ReflectionUtils.GetDelegate>();
			foreach (PropertyInfo propertyInfo in ReflectionUtils.GetProperties(type))
			{
				if (propertyInfo.CanRead)
				{
					MethodInfo getterMethodInfo = ReflectionUtils.GetGetterMethodInfo(propertyInfo);
					if (!getterMethodInfo.IsStatic && getterMethodInfo.IsPublic)
					{
						dictionary[this.MapClrMemberNameToJsonFieldName(propertyInfo.Name)] = ReflectionUtils.GetGetMethod(propertyInfo);
					}
				}
			}
			foreach (FieldInfo fieldInfo in ReflectionUtils.GetFields(type))
			{
				if (!fieldInfo.IsStatic && fieldInfo.IsPublic)
				{
					dictionary[this.MapClrMemberNameToJsonFieldName(fieldInfo.Name)] = ReflectionUtils.GetGetMethod(fieldInfo);
				}
			}
			return dictionary;
		}

		// Token: 0x0600015B RID: 347 RVA: 0x00007464 File Offset: 0x00005664
		internal virtual IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>> SetterValueFactory(Type type)
		{
			IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>> dictionary = new Dictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>();
			foreach (PropertyInfo propertyInfo in ReflectionUtils.GetProperties(type))
			{
				if (propertyInfo.CanWrite)
				{
					MethodInfo setterMethodInfo = ReflectionUtils.GetSetterMethodInfo(propertyInfo);
					if (!setterMethodInfo.IsStatic && setterMethodInfo.IsPublic)
					{
						dictionary[this.MapClrMemberNameToJsonFieldName(propertyInfo.Name)] = new KeyValuePair<Type, ReflectionUtils.SetDelegate>(propertyInfo.PropertyType, ReflectionUtils.GetSetMethod(propertyInfo));
					}
				}
			}
			foreach (FieldInfo fieldInfo in ReflectionUtils.GetFields(type))
			{
				if (!fieldInfo.IsInitOnly && !fieldInfo.IsStatic && fieldInfo.IsPublic)
				{
					dictionary[this.MapClrMemberNameToJsonFieldName(fieldInfo.Name)] = new KeyValuePair<Type, ReflectionUtils.SetDelegate>(fieldInfo.FieldType, ReflectionUtils.GetSetMethod(fieldInfo));
				}
			}
			return dictionary;
		}

		// Token: 0x0600015C RID: 348 RVA: 0x00007570 File Offset: 0x00005770
		public virtual bool TrySerializeNonPrimitiveObject(object input, out object output)
		{
			return this.TrySerializeKnownTypes(input, out output) || this.TrySerializeUnknownTypes(input, out output);
		}

		// Token: 0x0600015D RID: 349 RVA: 0x00007588 File Offset: 0x00005788
		public virtual object DeserializeObject(object value, Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			string text = value as string;
			if (type == typeof(Guid) && string.IsNullOrEmpty(text))
			{
				return default(Guid);
			}
			if (value == null)
			{
				return null;
			}
			object obj = null;
			if (text != null)
			{
				if (text.Length != 0)
				{
					if (type == typeof(DateTime) || (ReflectionUtils.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(DateTime)))
					{
						return DateTime.ParseExact(text, PocoJsonSerializerStrategy.Iso8601Format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
					}
					if (type == typeof(DateTimeOffset) || (ReflectionUtils.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(DateTimeOffset)))
					{
						return DateTimeOffset.ParseExact(text, PocoJsonSerializerStrategy.Iso8601Format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
					}
					if (type == typeof(Guid) || (ReflectionUtils.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(Guid)))
					{
						return new Guid(text);
					}
					if (type == typeof(Uri))
					{
						Uri uri;
						if (Uri.IsWellFormedUriString(text, UriKind.RelativeOrAbsolute) && Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out uri))
						{
							return uri;
						}
						return null;
					}
					else
					{
						if (type == typeof(string))
						{
							return text;
						}
						if (type == typeof(TimeSpan) || (ReflectionUtils.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(TimeSpan)))
						{
							return TimeSpan.Parse(text);
						}
						return Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
					}
				}
				else
				{
					if (type == typeof(Guid))
					{
						obj = default(Guid);
					}
					else if (ReflectionUtils.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(Guid))
					{
						obj = null;
					}
					else
					{
						obj = text;
					}
					if (!ReflectionUtils.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(Guid))
					{
						return text;
					}
				}
			}
			if (type.IsEnum)
			{
				return Enum.Parse(type, value.ToString());
			}
			if (value is bool)
			{
				return value;
			}
			bool flag = value is long;
			bool flag2 = value is double;
			if ((flag && type == typeof(long)) || (flag2 && type == typeof(double)))
			{
				return value;
			}
			if ((!flag2 || !(type != typeof(double))) && (!flag || !(type != typeof(long))))
			{
				IDictionary<string, object> dictionary = value as IDictionary<string, object>;
				if (dictionary != null)
				{
					IDictionary<string, object> dictionary2 = dictionary;
					if (ReflectionUtils.IsTypeDictionary(type))
					{
						Type[] genericTypeArguments = ReflectionUtils.GetGenericTypeArguments(type);
						Type type2 = genericTypeArguments[0];
						Type type3 = genericTypeArguments[1];
						Type type4 = typeof(Dictionary<, >).MakeGenericType(new Type[] { type2, type3 });
						IDictionary dictionary3 = (IDictionary)this.ConstructorCache[type4](Array.Empty<object>());
						foreach (KeyValuePair<string, object> keyValuePair in dictionary2)
						{
							dictionary3.Add(keyValuePair.Key, this.DeserializeObject(keyValuePair.Value, type3));
						}
						return dictionary3;
					}
					if (type == typeof(object))
					{
						return value;
					}
					obj = Activator.CreateInstance(type);
					using (IEnumerator<KeyValuePair<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>> enumerator2 = this.SetCache[type].GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							KeyValuePair<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>> keyValuePair2 = enumerator2.Current;
							object obj2;
							if (dictionary2.TryGetValue(keyValuePair2.Key, out obj2))
							{
								obj2 = this.DeserializeObject(obj2, keyValuePair2.Value.Key);
								keyValuePair2.Value.Value(obj, obj2);
							}
							else
							{
								string text2 = keyValuePair2.Key;
								text2 = char.ToLower(text2[0]).ToString() + text2.Substring(1);
								if (dictionary2.TryGetValue(text2, out obj2))
								{
									obj2 = this.DeserializeObject(obj2, keyValuePair2.Value.Key);
									keyValuePair2.Value.Value(obj, obj2);
								}
							}
						}
						return obj;
					}
				}
				IList<object> list = value as IList<object>;
				if (list != null)
				{
					IList<object> list2 = list;
					IList list3 = null;
					if (type.IsArray)
					{
						list3 = (IList)this.ConstructorCache[type](new object[] { list2.Count });
						int num = 0;
						using (IEnumerator<object> enumerator3 = list2.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								object obj3 = enumerator3.Current;
								list3[num++] = this.DeserializeObject(obj3, type.GetElementType());
							}
							goto IL_0663;
						}
					}
					if (ReflectionUtils.IsTypeGenericeCollectionInterface(type) || ReflectionUtils.IsAssignableFrom(typeof(IList), type))
					{
						Type genericListElementType = ReflectionUtils.GetGenericListElementType(type);
						ReflectionUtils.ConstructorDelegate constructorDelegate;
						if ((constructorDelegate = this.ConstructorCache[type]) == null)
						{
							constructorDelegate = this.ConstructorCache[typeof(List<>).MakeGenericType(new Type[] { genericListElementType })];
						}
						list3 = (IList)constructorDelegate(Array.Empty<object>());
						foreach (object obj4 in list2)
						{
							list3.Add(this.DeserializeObject(obj4, genericListElementType));
						}
					}
					IL_0663:
					obj = list3;
				}
				return obj;
			}
			obj = ((type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(float) || type == typeof(bool) || type == typeof(decimal) || type == typeof(byte) || type == typeof(short)) ? Convert.ChangeType(value, type, CultureInfo.InvariantCulture) : value);
			if (ReflectionUtils.IsNullableType(type))
			{
				return ReflectionUtils.ToNullableType(obj, type);
			}
			return obj;
		}

		// Token: 0x0600015E RID: 350 RVA: 0x00007C44 File Offset: 0x00005E44
		protected virtual object SerializeEnum(Enum p)
		{
			return Convert.ToDouble(p, CultureInfo.InvariantCulture);
		}

		// Token: 0x0600015F RID: 351 RVA: 0x00007C58 File Offset: 0x00005E58
		protected virtual bool TrySerializeKnownTypes(object input, out object output)
		{
			bool flag = true;
			if (input is DateTime)
			{
				output = ((DateTime)input).ToUniversalTime().ToString(PocoJsonSerializerStrategy.Iso8601Format[0], CultureInfo.InvariantCulture);
			}
			else if (input is DateTimeOffset)
			{
				output = ((DateTimeOffset)input).ToUniversalTime().ToString(PocoJsonSerializerStrategy.Iso8601Format[0], CultureInfo.InvariantCulture);
			}
			else if (input is TimeSpan)
			{
				output = ((TimeSpan)input).ToString();
			}
			else if (input is Guid)
			{
				output = ((Guid)input).ToString("D");
			}
			else if (input is Uri)
			{
				output = input.ToString();
			}
			else
			{
				Enum @enum = input as Enum;
				if (@enum != null)
				{
					output = this.SerializeEnum(@enum);
				}
				else
				{
					flag = false;
					output = null;
				}
			}
			return flag;
		}

		// Token: 0x06000160 RID: 352 RVA: 0x00007D38 File Offset: 0x00005F38
		protected virtual bool TrySerializeUnknownTypes(object input, out object output)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			output = null;
			Type type = input.GetType();
			if (type.FullName == null)
			{
				return false;
			}
			IDictionary<string, object> dictionary = new JsonObject();
			foreach (KeyValuePair<string, ReflectionUtils.GetDelegate> keyValuePair in this.GetCache[type])
			{
				if (keyValuePair.Value != null)
				{
					dictionary.Add(this.MapClrMemberNameToJsonFieldName(keyValuePair.Key), keyValuePair.Value(input));
				}
			}
			output = dictionary;
			return true;
		}

		// Token: 0x040000A3 RID: 163
		internal IDictionary<Type, ReflectionUtils.ConstructorDelegate> ConstructorCache;

		// Token: 0x040000A4 RID: 164
		internal IDictionary<Type, IDictionary<string, ReflectionUtils.GetDelegate>> GetCache;

		// Token: 0x040000A5 RID: 165
		internal IDictionary<Type, IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>> SetCache;

		// Token: 0x040000A6 RID: 166
		internal static readonly Type[] EmptyTypes = new Type[0];

		// Token: 0x040000A7 RID: 167
		internal static readonly Type[] ArrayConstructorParameterTypes = new Type[] { typeof(int) };

		// Token: 0x040000A8 RID: 168
		private static readonly string[] Iso8601Format = new string[] { "yyyy-MM-dd\\THH:mm:ss.FFFFFFF\\Z", "yyyy-MM-dd\\THH:mm:ss\\Z", "yyyy-MM-dd\\THH:mm:ssK" };
	}
}
