using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200009F RID: 159
	public static class EnumUtility
	{
		// Token: 0x06000475 RID: 1141 RVA: 0x00013500 File Offset: 0x00011700
		public static T Next<T>(T target) where T : struct
		{
			if (!typeof(T).IsEnum)
			{
				Debug.LogException(new Exception(typeof(T).FullName + " is not an Enum!"));
			}
			T[] array = (T[])Enum.GetValues(target.GetType());
			int num = Array.IndexOf<T>(array, target) + 1;
			if (array.Length != num)
			{
				return array[num];
			}
			return array[0];
		}

		// Token: 0x06000476 RID: 1142 RVA: 0x00013578 File Offset: 0x00011778
		public static int IndexOf<T>(T target) where T : struct
		{
			if (!typeof(T).IsEnum)
			{
				Debug.LogException(new Exception(typeof(T).FullName + " is not an Enum!"));
			}
			return Array.IndexOf<T>((T[])Enum.GetValues(target.GetType()), target);
		}

		// Token: 0x06000477 RID: 1143 RVA: 0x000135D8 File Offset: 0x000117D8
		public static T GetAtIndex<T>(T target, int index) where T : struct
		{
			if (!typeof(T).IsEnum)
			{
				Debug.LogException(new Exception(typeof(T).FullName + " is not an Enum!"));
			}
			return ((T[])Enum.GetValues(target.GetType()))[index];
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x00013638 File Offset: 0x00011838
		public static T GetRandomEnumValue<T>() where T : Enum
		{
			global::System.Random random = new global::System.Random();
			Array values = Enum.GetValues(typeof(T));
			int num = random.Next(values.Length);
			return (T)((object)values.GetValue(num));
		}
	}
}
