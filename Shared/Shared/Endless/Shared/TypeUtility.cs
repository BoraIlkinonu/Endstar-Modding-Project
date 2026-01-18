using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x020000A8 RID: 168
	public static class TypeUtility
	{
		// Token: 0x06000490 RID: 1168 RVA: 0x0001455C File Offset: 0x0001275C
		public static bool IsNumeric(Type type)
		{
			return TypeUtility.numericTypes.Contains(type);
		}

		// Token: 0x06000491 RID: 1169 RVA: 0x00014569 File Offset: 0x00012769
		public static bool IsIEnumerable(Type type)
		{
			if (type != typeof(string))
			{
				return type.GetInterfaces().Any((Type item) => item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			}
			return false;
		}

		// Token: 0x04000242 RID: 578
		private static readonly HashSet<Type> numericTypes = new HashSet<Type>
		{
			typeof(short),
			typeof(ushort),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(int),
			typeof(float),
			typeof(double),
			typeof(decimal),
			typeof(Vector2),
			typeof(Vector2Int),
			typeof(Vector3),
			typeof(Vector3Int),
			typeof(Vector4),
			typeof(Quaternion)
		};
	}
}
