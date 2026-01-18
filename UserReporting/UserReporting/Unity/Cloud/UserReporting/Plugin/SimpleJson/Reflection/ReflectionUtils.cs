using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Unity.Cloud.UserReporting.Plugin.SimpleJson.Reflection
{
	// Token: 0x0200002D RID: 45
	[GeneratedCode("reflection-utils", "1.0.0")]
	internal class ReflectionUtils
	{
		// Token: 0x06000162 RID: 354 RVA: 0x00007E2F File Offset: 0x0000602F
		public static Type GetTypeInfo(Type type)
		{
			return type;
		}

		// Token: 0x06000163 RID: 355 RVA: 0x00007E32 File Offset: 0x00006032
		public static Attribute GetAttribute(MemberInfo info, Type type)
		{
			if (info == null || type == null || !Attribute.IsDefined(info, type))
			{
				return null;
			}
			return Attribute.GetCustomAttribute(info, type);
		}

		// Token: 0x06000164 RID: 356 RVA: 0x00007E58 File Offset: 0x00006058
		public static Type GetGenericListElementType(Type type)
		{
			foreach (Type type2 in ((IEnumerable<Type>)type.GetInterfaces()))
			{
				if (ReflectionUtils.IsTypeGeneric(type2) && type2.GetGenericTypeDefinition() == typeof(IList<>))
				{
					return ReflectionUtils.GetGenericTypeArguments(type2)[0];
				}
			}
			return ReflectionUtils.GetGenericTypeArguments(type)[0];
		}

		// Token: 0x06000165 RID: 357 RVA: 0x00007ED4 File Offset: 0x000060D4
		public static Attribute GetAttribute(Type objectType, Type attributeType)
		{
			if (objectType == null || attributeType == null || !Attribute.IsDefined(objectType, attributeType))
			{
				return null;
			}
			return Attribute.GetCustomAttribute(objectType, attributeType);
		}

		// Token: 0x06000166 RID: 358 RVA: 0x00007EFA File Offset: 0x000060FA
		public static Type[] GetGenericTypeArguments(Type type)
		{
			return type.GetGenericArguments();
		}

		// Token: 0x06000167 RID: 359 RVA: 0x00007F02 File Offset: 0x00006102
		public static bool IsTypeGeneric(Type type)
		{
			return ReflectionUtils.GetTypeInfo(type).IsGenericType;
		}

		// Token: 0x06000168 RID: 360 RVA: 0x00007F10 File Offset: 0x00006110
		public static bool IsTypeGenericeCollectionInterface(Type type)
		{
			if (!ReflectionUtils.IsTypeGeneric(type))
			{
				return false;
			}
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			return genericTypeDefinition == typeof(IList<>) || genericTypeDefinition == typeof(ICollection<>) || genericTypeDefinition == typeof(IEnumerable<>);
		}

		// Token: 0x06000169 RID: 361 RVA: 0x00007F64 File Offset: 0x00006164
		public static bool IsAssignableFrom(Type type1, Type type2)
		{
			return ReflectionUtils.GetTypeInfo(type1).IsAssignableFrom(ReflectionUtils.GetTypeInfo(type2));
		}

		// Token: 0x0600016A RID: 362 RVA: 0x00007F77 File Offset: 0x00006177
		public static bool IsTypeDictionary(Type type)
		{
			return typeof(IDictionary).IsAssignableFrom(type) || (ReflectionUtils.GetTypeInfo(type).IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<, >));
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00007FB1 File Offset: 0x000061B1
		public static bool IsNullableType(Type type)
		{
			return ReflectionUtils.GetTypeInfo(type).IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		// Token: 0x0600016C RID: 364 RVA: 0x00007FD7 File Offset: 0x000061D7
		public static object ToNullableType(object obj, Type nullableType)
		{
			if (obj != null)
			{
				return Convert.ChangeType(obj, Nullable.GetUnderlyingType(nullableType), CultureInfo.InvariantCulture);
			}
			return null;
		}

		// Token: 0x0600016D RID: 365 RVA: 0x00007FEF File Offset: 0x000061EF
		public static bool IsValueType(Type type)
		{
			return ReflectionUtils.GetTypeInfo(type).IsValueType;
		}

		// Token: 0x0600016E RID: 366 RVA: 0x00007FFC File Offset: 0x000061FC
		public static IEnumerable<ConstructorInfo> GetConstructors(Type type)
		{
			return type.GetConstructors();
		}

		// Token: 0x0600016F RID: 367 RVA: 0x00008004 File Offset: 0x00006204
		public static ConstructorInfo GetConstructorInfo(Type type, params Type[] argsType)
		{
			foreach (ConstructorInfo constructorInfo in ReflectionUtils.GetConstructors(type))
			{
				ParameterInfo[] parameters = constructorInfo.GetParameters();
				if (argsType.Length == parameters.Length)
				{
					int num = 0;
					bool flag = true;
					ParameterInfo[] parameters2 = constructorInfo.GetParameters();
					for (int i = 0; i < parameters2.Length; i++)
					{
						if (parameters2[i].ParameterType != argsType[num])
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						return constructorInfo;
					}
				}
			}
			return null;
		}

		// Token: 0x06000170 RID: 368 RVA: 0x000080A0 File Offset: 0x000062A0
		public static IEnumerable<PropertyInfo> GetProperties(Type type)
		{
			return type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		// Token: 0x06000171 RID: 369 RVA: 0x000080AA File Offset: 0x000062AA
		public static IEnumerable<FieldInfo> GetFields(Type type)
		{
			return type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		// Token: 0x06000172 RID: 370 RVA: 0x000080B4 File Offset: 0x000062B4
		public static MethodInfo GetGetterMethodInfo(PropertyInfo propertyInfo)
		{
			return propertyInfo.GetGetMethod(true);
		}

		// Token: 0x06000173 RID: 371 RVA: 0x000080BD File Offset: 0x000062BD
		public static MethodInfo GetSetterMethodInfo(PropertyInfo propertyInfo)
		{
			return propertyInfo.GetSetMethod(true);
		}

		// Token: 0x06000174 RID: 372 RVA: 0x000080C6 File Offset: 0x000062C6
		public static ReflectionUtils.ConstructorDelegate GetContructor(ConstructorInfo constructorInfo)
		{
			return ReflectionUtils.GetConstructorByReflection(constructorInfo);
		}

		// Token: 0x06000175 RID: 373 RVA: 0x000080CE File Offset: 0x000062CE
		public static ReflectionUtils.ConstructorDelegate GetContructor(Type type, params Type[] argsType)
		{
			return ReflectionUtils.GetConstructorByReflection(type, argsType);
		}

		// Token: 0x06000176 RID: 374 RVA: 0x000080D7 File Offset: 0x000062D7
		public static ReflectionUtils.ConstructorDelegate GetConstructorByReflection(ConstructorInfo constructorInfo)
		{
			return (object[] args) => constructorInfo.Invoke(args);
		}

		// Token: 0x06000177 RID: 375 RVA: 0x000080F0 File Offset: 0x000062F0
		public static ReflectionUtils.ConstructorDelegate GetConstructorByReflection(Type type, params Type[] argsType)
		{
			ConstructorInfo constructorInfo = ReflectionUtils.GetConstructorInfo(type, argsType);
			if (!(constructorInfo == null))
			{
				return ReflectionUtils.GetConstructorByReflection(constructorInfo);
			}
			return null;
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00008116 File Offset: 0x00006316
		public static ReflectionUtils.GetDelegate GetGetMethod(PropertyInfo propertyInfo)
		{
			return ReflectionUtils.GetGetMethodByReflection(propertyInfo);
		}

		// Token: 0x06000179 RID: 377 RVA: 0x0000811E File Offset: 0x0000631E
		public static ReflectionUtils.GetDelegate GetGetMethod(FieldInfo fieldInfo)
		{
			return ReflectionUtils.GetGetMethodByReflection(fieldInfo);
		}

		// Token: 0x0600017A RID: 378 RVA: 0x00008126 File Offset: 0x00006326
		public static ReflectionUtils.GetDelegate GetGetMethodByReflection(PropertyInfo propertyInfo)
		{
			MethodInfo methodInfo = ReflectionUtils.GetGetterMethodInfo(propertyInfo);
			return (object source) => methodInfo.Invoke(source, ReflectionUtils.EmptyObjects);
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00008144 File Offset: 0x00006344
		public static ReflectionUtils.GetDelegate GetGetMethodByReflection(FieldInfo fieldInfo)
		{
			return (object source) => fieldInfo.GetValue(source);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x0000815D File Offset: 0x0000635D
		public static ReflectionUtils.SetDelegate GetSetMethod(PropertyInfo propertyInfo)
		{
			return ReflectionUtils.GetSetMethodByReflection(propertyInfo);
		}

		// Token: 0x0600017D RID: 381 RVA: 0x00008165 File Offset: 0x00006365
		public static ReflectionUtils.SetDelegate GetSetMethod(FieldInfo fieldInfo)
		{
			return ReflectionUtils.GetSetMethodByReflection(fieldInfo);
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0000816D File Offset: 0x0000636D
		public static ReflectionUtils.SetDelegate GetSetMethodByReflection(PropertyInfo propertyInfo)
		{
			MethodInfo methodInfo = ReflectionUtils.GetSetterMethodInfo(propertyInfo);
			return delegate(object source, object value)
			{
				methodInfo.Invoke(source, new object[] { value });
			};
		}

		// Token: 0x0600017F RID: 383 RVA: 0x0000818B File Offset: 0x0000638B
		public static ReflectionUtils.SetDelegate GetSetMethodByReflection(FieldInfo fieldInfo)
		{
			return delegate(object source, object value)
			{
				fieldInfo.SetValue(source, value);
			};
		}

		// Token: 0x040000A9 RID: 169
		private static readonly object[] EmptyObjects = new object[0];

		// Token: 0x02000047 RID: 71
		// (Invoke) Token: 0x0600023E RID: 574
		public delegate object GetDelegate(object source);

		// Token: 0x02000048 RID: 72
		// (Invoke) Token: 0x06000242 RID: 578
		public delegate void SetDelegate(object source, object value);

		// Token: 0x02000049 RID: 73
		// (Invoke) Token: 0x06000246 RID: 582
		public delegate object ConstructorDelegate(params object[] args);

		// Token: 0x0200004A RID: 74
		// (Invoke) Token: 0x0600024A RID: 586
		public delegate TValue ThreadSafeDictionaryValueFactory<TKey, TValue>(TKey key);

		// Token: 0x0200004B RID: 75
		public sealed class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
		{
			// Token: 0x0600024D RID: 589 RVA: 0x00009622 File Offset: 0x00007822
			public ThreadSafeDictionary(ReflectionUtils.ThreadSafeDictionaryValueFactory<TKey, TValue> valueFactory)
			{
				this._valueFactory = valueFactory;
			}

			// Token: 0x1700008B RID: 139
			// (get) Token: 0x0600024E RID: 590 RVA: 0x0000963C File Offset: 0x0000783C
			public int Count
			{
				get
				{
					return this._dictionary.Count;
				}
			}

			// Token: 0x1700008C RID: 140
			// (get) Token: 0x0600024F RID: 591 RVA: 0x00009649 File Offset: 0x00007849
			public bool IsReadOnly
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			// Token: 0x1700008D RID: 141
			public TValue this[TKey key]
			{
				get
				{
					return this.Get(key);
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			// Token: 0x1700008E RID: 142
			// (get) Token: 0x06000252 RID: 594 RVA: 0x00009660 File Offset: 0x00007860
			public ICollection<TKey> Keys
			{
				get
				{
					return this._dictionary.Keys;
				}
			}

			// Token: 0x1700008F RID: 143
			// (get) Token: 0x06000253 RID: 595 RVA: 0x0000966D File Offset: 0x0000786D
			public ICollection<TValue> Values
			{
				get
				{
					return this._dictionary.Values;
				}
			}

			// Token: 0x06000254 RID: 596 RVA: 0x0000967A File Offset: 0x0000787A
			public void Add(TKey key, TValue value)
			{
				throw new NotImplementedException();
			}

			// Token: 0x06000255 RID: 597 RVA: 0x00009681 File Offset: 0x00007881
			public void Add(KeyValuePair<TKey, TValue> item)
			{
				throw new NotImplementedException();
			}

			// Token: 0x06000256 RID: 598 RVA: 0x00009688 File Offset: 0x00007888
			private TValue AddValue(TKey key)
			{
				TValue tvalue = this._valueFactory(key);
				object @lock = this._lock;
				lock (@lock)
				{
					if (this._dictionary == null)
					{
						this._dictionary = new Dictionary<TKey, TValue>();
						this._dictionary[key] = tvalue;
					}
					else
					{
						TValue tvalue2;
						if (this._dictionary.TryGetValue(key, out tvalue2))
						{
							return tvalue2;
						}
						Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(this._dictionary);
						dictionary[key] = tvalue;
						this._dictionary = dictionary;
					}
				}
				return tvalue;
			}

			// Token: 0x06000257 RID: 599 RVA: 0x00009728 File Offset: 0x00007928
			public void Clear()
			{
				throw new NotImplementedException();
			}

			// Token: 0x06000258 RID: 600 RVA: 0x0000972F File Offset: 0x0000792F
			public bool Contains(KeyValuePair<TKey, TValue> item)
			{
				throw new NotImplementedException();
			}

			// Token: 0x06000259 RID: 601 RVA: 0x00009736 File Offset: 0x00007936
			public bool ContainsKey(TKey key)
			{
				return this._dictionary.ContainsKey(key);
			}

			// Token: 0x0600025A RID: 602 RVA: 0x00009744 File Offset: 0x00007944
			public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			// Token: 0x0600025B RID: 603 RVA: 0x0000974C File Offset: 0x0000794C
			private TValue Get(TKey key)
			{
				if (this._dictionary == null)
				{
					return this.AddValue(key);
				}
				TValue tvalue;
				if (!this._dictionary.TryGetValue(key, out tvalue))
				{
					return this.AddValue(key);
				}
				return tvalue;
			}

			// Token: 0x0600025C RID: 604 RVA: 0x00009782 File Offset: 0x00007982
			public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			{
				return this._dictionary.GetEnumerator();
			}

			// Token: 0x0600025D RID: 605 RVA: 0x00009794 File Offset: 0x00007994
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this._dictionary.GetEnumerator();
			}

			// Token: 0x0600025E RID: 606 RVA: 0x000097A6 File Offset: 0x000079A6
			public bool Remove(TKey key)
			{
				throw new NotImplementedException();
			}

			// Token: 0x0600025F RID: 607 RVA: 0x000097AD File Offset: 0x000079AD
			public bool Remove(KeyValuePair<TKey, TValue> item)
			{
				throw new NotImplementedException();
			}

			// Token: 0x06000260 RID: 608 RVA: 0x000097B4 File Offset: 0x000079B4
			public bool TryGetValue(TKey key, out TValue value)
			{
				value = this[key];
				return true;
			}

			// Token: 0x0400011B RID: 283
			private Dictionary<TKey, TValue> _dictionary;

			// Token: 0x0400011C RID: 284
			private readonly object _lock = new object();

			// Token: 0x0400011D RID: 285
			private readonly ReflectionUtils.ThreadSafeDictionaryValueFactory<TKey, TValue> _valueFactory;
		}
	}
}
