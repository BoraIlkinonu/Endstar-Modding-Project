using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.Debugging
{
	// Token: 0x020000AB RID: 171
	public static class DebugUtility
	{
		// Token: 0x06000497 RID: 1175 RVA: 0x000146EF File Offset: 0x000128EF
		public static void Log(string log, global::UnityEngine.Object context = null)
		{
			Debug.Log(log, context);
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x000146F8 File Offset: 0x000128F8
		public static void LogWarning(string log, global::UnityEngine.Object context = null)
		{
			Debug.LogWarning(log, context);
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x00014701 File Offset: 0x00012901
		public static void LogError(string log, global::UnityEngine.Object context = null)
		{
			Debug.LogError(log, context);
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x0001470A File Offset: 0x0001290A
		public static void LogException(Exception exception, global::UnityEngine.Object context = null)
		{
			Debug.LogException(exception, context);
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x00014714 File Offset: 0x00012914
		public static void LogMethod(object context, string methodName, params object[] parameterValues)
		{
			object log = DebugUtility.GetLog(context, methodName, parameterValues);
			global::UnityEngine.Object @object = context as global::UnityEngine.Object;
			Debug.Log(log, @object);
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x00014738 File Offset: 0x00012938
		public static void LogMethodWithAppension(object context, string methodName, string appension, params object[] parameterValues)
		{
			object obj = DebugUtility.GetLog(context, methodName, parameterValues) + " | " + appension;
			global::UnityEngine.Object @object = context as global::UnityEngine.Object;
			Debug.Log(obj, @object);
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x00014768 File Offset: 0x00012968
		public static void LogWarning(object context, string methodName, string warning, params object[] parameterValues)
		{
			object obj = DebugUtility.GetLog(context, methodName, parameterValues) + " | " + warning;
			global::UnityEngine.Object @object = context as global::UnityEngine.Object;
			Debug.LogWarning(obj, @object);
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x00014798 File Offset: 0x00012998
		public static void LogError(object context, string methodName, string error, params object[] parameterValues)
		{
			object obj = DebugUtility.GetLog(context, methodName, parameterValues) + " | " + error;
			global::UnityEngine.Object @object = context as global::UnityEngine.Object;
			Debug.LogError(obj, @object);
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x000147C8 File Offset: 0x000129C8
		public static void LogException(object context, string methodName, string error, params object[] parameterValues)
		{
			string text = DebugUtility.GetLog(context, methodName, parameterValues) + " | " + error;
			global::UnityEngine.Object @object = context as global::UnityEngine.Object;
			Debug.LogException(new Exception(text), @object);
		}

		// Token: 0x060004A0 RID: 1184 RVA: 0x000147FC File Offset: 0x000129FC
		public static void LogNoEnumSupportError<T>(object context, string methodName, T enumValue, params object[] parameterValues) where T : Enum
		{
			string text = DebugUtility.GetLog(context, methodName, parameterValues);
			text = string.Format("{0} | No support for an enum value of {1}!", text, enumValue);
			global::UnityEngine.Object @object = context as global::UnityEngine.Object;
			Debug.LogException(new Exception(text), @object);
		}

		// Token: 0x060004A1 RID: 1185 RVA: 0x00014838 File Offset: 0x00012A38
		public static void LogNoEnumSupportError<T>(object context, T enumValue) where T : Enum
		{
			string text = string.Format("No support for an enum value of {0}!", enumValue);
			global::UnityEngine.Object @object = context as global::UnityEngine.Object;
			Debug.LogException(new Exception(text), @object);
		}

		// Token: 0x060004A2 RID: 1186 RVA: 0x00014868 File Offset: 0x00012A68
		private static string GetLog(object context, string methodName, params object[] parameterValues)
		{
			MethodInfo method = context.GetType().GetMethod(methodName, (BindingFlags)(-1));
			if (method == null)
			{
				return methodName;
			}
			ParameterInfo[] parameters = method.GetParameters();
			if (parameters.Length == 0)
			{
				return methodName;
			}
			string text = methodName + " ( ";
			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameterInfo = parameters[i];
				string text2 = text;
				string name = parameterInfo.Name;
				string text3 = ": ";
				object obj = parameterValues[i];
				text = text2 + name + text3 + ((obj != null) ? obj.ToString() : null);
				if (i < parameters.Length - 1)
				{
					text += " , ";
				}
			}
			return text + " )";
		}

		// Token: 0x060004A3 RID: 1187 RVA: 0x000148FF File Offset: 0x00012AFF
		public static string DebugSafeName(this global::UnityEngine.Object target, bool returnName = true)
		{
			if (target == null)
			{
				return "null";
			}
			if (!returnName)
			{
				return target.GetType().Name;
			}
			return target.name;
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x00014928 File Offset: 0x00012B28
		public static string DebugSafeCount<T>(this IEnumerable<T> iEnumerable)
		{
			if (iEnumerable != null)
			{
				return iEnumerable.Count<T>().ToString();
			}
			return "null";
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x0001494C File Offset: 0x00012B4C
		public static string DebugSafeCount(this IEnumerable iEnumerable)
		{
			if (iEnumerable == null)
			{
				return "null";
			}
			int num = 0;
			ICollection collection = iEnumerable as ICollection;
			if (collection != null)
			{
				num = collection.Count;
			}
			else
			{
				using (IEnumerator enumerator = iEnumerable.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						num++;
					}
				}
			}
			return string.Format("{0:N0}", num);
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x000149C0 File Offset: 0x00012BC0
		public static string DebugSafeJson(this object obj)
		{
			if (obj != null)
			{
				return JsonConvert.SerializeObject(obj);
			}
			return "null";
		}

		// Token: 0x060004A7 RID: 1191 RVA: 0x000149D1 File Offset: 0x00012BD1
		public static string DebugIsNull(this Action target)
		{
			if (target != null)
			{
				return "is NOT null";
			}
			return "IS null";
		}

		// Token: 0x060004A8 RID: 1192 RVA: 0x000149D1 File Offset: 0x00012BD1
		public static string DebugIsNull(this UnityAction target)
		{
			if (target != null)
			{
				return "is NOT null";
			}
			return "IS null";
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x000149D1 File Offset: 0x00012BD1
		public static string DebugIsNull<T>(this Action<T> target)
		{
			if (target != null)
			{
				return "is NOT null";
			}
			return "IS null";
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x000149E4 File Offset: 0x00012BE4
		public static bool DebugHasDuplicates<T>(ICollection<T> collection, string collectionName, object context)
		{
			global::UnityEngine.Object @object = context as global::UnityEngine.Object;
			HashSet<T> hashSet = new HashSet<T>();
			foreach (T t in collection)
			{
				if (!hashSet.Add(t))
				{
					Debug.LogError(string.Format("A duplicate entry of {0} was found in {1}!", t, collectionName), @object);
					return true;
				}
			}
			return false;
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x00014A5C File Offset: 0x00012C5C
		public static bool DebugHasNullItem<T>(ICollection<T> collection, string collectionName, global::UnityEngine.Object context)
		{
			int num = collection.Count((T item) => item == null);
			bool flag = num > 0;
			if (flag)
			{
				Debug.LogError(string.Format("There are {0} null item{1} found in {2}!", num, (num > 1) ? "s" : string.Empty, collectionName), context);
			}
			return flag;
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x00014AC0 File Offset: 0x00012CC0
		public static bool DebugHasMonoBehaviour<T>(GameObject context) where T : MonoBehaviour
		{
			T t;
			bool flag = context.TryGetComponent<T>(out t);
			if (!flag)
			{
				Type typeFromHandle = typeof(T);
				Debug.LogError(string.Format("A MonoBehaviour with a type of {0} is required here!", typeFromHandle), context);
			}
			return flag;
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x00014AF4 File Offset: 0x00012CF4
		public static bool DebugHasComponent<T>(GameObject context) where T : Component
		{
			T t;
			bool flag = context.TryGetComponent<T>(out t);
			if (!flag)
			{
				Type typeFromHandle = typeof(T);
				Debug.LogError(string.Format("A Component with a type of {0} is required here!", typeFromHandle), context);
			}
			return flag;
		}

		// Token: 0x060004AE RID: 1198 RVA: 0x00014B28 File Offset: 0x00012D28
		public static void DebugEnumerable<T>(string iEnumerableMemberName, IEnumerable<T> iEnumerable, global::UnityEngine.Object context = null)
		{
			IEnumerable<T> enumerable = (iEnumerable as T[]) ?? iEnumerable.ToArray<T>();
			Debug.Log(string.Format("{0}: {1}", iEnumerableMemberName, enumerable.Count<T>()), context);
			Debug.Log("V----------V");
			foreach (T t in enumerable)
			{
				Debug.Log(string.Format("({0}){1}", t.GetType().Name, t), context);
			}
			Debug.Log("^----------^");
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x00014BD4 File Offset: 0x00012DD4
		public static void DebugCollection(string iCollectionMemberName, ICollection iCollection, global::UnityEngine.Object context = null)
		{
			Debug.Log(string.Format("{0}: {1}", iCollectionMemberName, iCollection.Count), context);
			Debug.Log("V----------V");
			foreach (object obj in iCollection)
			{
				Debug.Log(string.Format("({0}){1}", obj.GetType().Name, obj), context);
			}
			Debug.Log("^----------^");
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x00014C68 File Offset: 0x00012E68
		public static void DebugArray(string arrayMemberName, Array array, global::UnityEngine.Object context = null)
		{
			Debug.Log(string.Format("{0}: {1}", arrayMemberName, array.Length), context);
			Debug.Log("V----------V");
			foreach (object obj in array)
			{
				Debug.Log(string.Format("({0}){1}", obj.GetType().Name, obj), context);
			}
			Debug.Log("^----------^");
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x00014CFC File Offset: 0x00012EFC
		public static void DebugDictionary<TKey, TValue>(string dictionaryMemberName, Dictionary<TKey, TValue> dictionary, global::UnityEngine.Object context = null)
		{
			Debug.Log(string.Format("{0}: {1}", dictionaryMemberName, dictionary.Count), context);
			Debug.Log("V----------V");
			foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
			{
				Debug.Log(string.Format("{0}: {1}", keyValuePair.Key, keyValuePair.Value), context);
			}
			Debug.Log("^----------^");
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x00014D9C File Offset: 0x00012F9C
		public static bool DebugIsNull(string targetName, global::UnityEngine.Object target, global::UnityEngine.Object context = null)
		{
			if (target != null)
			{
				return false;
			}
			string text = targetName + " is null!";
			if (context)
			{
				text = text + " | " + context.name;
			}
			Debug.LogError(text, context);
			return true;
		}

		// Token: 0x060004B3 RID: 1203 RVA: 0x00014DE2 File Offset: 0x00012FE2
		public static void DrawBox(Vector3 position, Vector3 halfExtents)
		{
			DebugUtility.DrawBox(position, halfExtents, Color.white, 0f);
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x00014DF8 File Offset: 0x00012FF8
		public static void DrawBox(Vector3 position, Vector3 halfExtents, Color color, float duration = 0f)
		{
			if (duration < 1E-45f)
			{
				duration = Time.deltaTime;
			}
			Vector3 vector = position - halfExtents;
			Vector3 vector2 = position + halfExtents;
			Vector3 vector3 = vector;
			Vector3 vector4 = new Vector3(vector.x, vector.y, vector2.z);
			Vector3 vector5 = new Vector3(vector2.x, vector.y, vector2.z);
			Vector3 vector6 = new Vector3(vector2.x, vector.y, vector.z);
			Vector3 vector7 = new Vector3(vector.x, vector2.y, vector.z);
			Vector3 vector8 = new Vector3(vector.x, vector2.y, vector2.z);
			Vector3 vector9 = new Vector3(vector2.x, vector2.y, vector2.z);
			Vector3 vector10 = new Vector3(vector2.x, vector2.y, vector.z);
			Debug.DrawLine(vector3, vector4, color, duration);
			Debug.DrawLine(vector4, vector5, color, duration);
			Debug.DrawLine(vector5, vector6, color, duration);
			Debug.DrawLine(vector6, vector3, color, duration);
			Debug.DrawLine(vector7, vector8, color, duration);
			Debug.DrawLine(vector8, vector9, color, duration);
			Debug.DrawLine(vector9, vector10, color, duration);
			Debug.DrawLine(vector10, vector7, color, duration);
			Debug.DrawLine(vector3, vector7, color, duration);
			Debug.DrawLine(vector4, vector8, color, duration);
			Debug.DrawLine(vector5, vector9, color, duration);
			Debug.DrawLine(vector6, vector10, color, duration);
		}

		// Token: 0x04000245 RID: 581
		private const BindingFlags ALL_BINDING_FLAGS = (BindingFlags)(-1);
	}
}
