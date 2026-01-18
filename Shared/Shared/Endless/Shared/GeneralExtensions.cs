using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200005D RID: 93
	public static class GeneralExtensions
	{
		// Token: 0x060002D8 RID: 728 RVA: 0x0000E360 File Offset: 0x0000C560
		public static bool Approx(this Quaternion quaternion, Quaternion otherQuaternion, float tolerance)
		{
			return Quaternion.Dot(quaternion, otherQuaternion) > 1f - tolerance;
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x0000E372 File Offset: 0x0000C572
		public static bool Approx(this float floatA, float floatB, float tolerance = 1E-45f)
		{
			return math.abs(floatA - floatB) < tolerance;
		}

		// Token: 0x060002DA RID: 730 RVA: 0x0000E380 File Offset: 0x0000C580
		public static T2 GetRandomValue<T1, T2>(this Dictionary<T1, T2> dictionary)
		{
			return dictionary.ElementAt(global::UnityEngine.Random.Range(0, dictionary.Count)).Value;
		}

		// Token: 0x060002DB RID: 731 RVA: 0x0000E3A8 File Offset: 0x0000C5A8
		public static T1 GetRandomKey<T1, T2>(this Dictionary<T1, T2> dictionary)
		{
			return dictionary.ElementAt(global::UnityEngine.Random.Range(0, dictionary.Count)).Key;
		}

		// Token: 0x060002DC RID: 732 RVA: 0x0000E3D0 File Offset: 0x0000C5D0
		public static bool Approximately(this Vector3 vectorA, Vector3 vectorB)
		{
			return vectorA.x.Approx(vectorB.x, float.Epsilon) && vectorA.y.Approx(vectorB.y, float.Epsilon) && vectorA.z.Approx(vectorB.z, float.Epsilon);
		}

		// Token: 0x060002DD RID: 733 RVA: 0x0000E425 File Offset: 0x0000C625
		public static bool Approximately(this Vector3 vectorA, Vector3 vectorB, float tolerance)
		{
			return vectorA.x.Approx(vectorB.x, tolerance) && vectorA.y.Approx(vectorB.y, tolerance) && vectorA.z.Approx(vectorB.z, tolerance);
		}

		// Token: 0x060002DE RID: 734 RVA: 0x0000E464 File Offset: 0x0000C664
		[return: TupleElementNames(new string[] { "point", "index" })]
		public static ValueTuple<Vector3, int> FindClosestPoint(this Vector3 comparand, List<Vector3> points)
		{
			int count = points.Count;
			if (count == 0)
			{
				Debug.LogException(new Exception("Points must contain more that 0 elements"));
				return new ValueTuple<Vector3, int>(comparand, 0);
			}
			if (count != 1)
			{
				ValueTuple<Vector3, float, int> valueTuple = new ValueTuple<Vector3, float, int>(points[0], (comparand - points[0]).sqrMagnitude, 0);
				for (int i = 1; i < points.Count; i++)
				{
					Vector3 vector = points[i];
					float sqrMagnitude = (comparand - vector).sqrMagnitude;
					if (sqrMagnitude <= valueTuple.Item2)
					{
						valueTuple.Item1 = vector;
						valueTuple.Item2 = sqrMagnitude;
						valueTuple.Item3 = i;
					}
				}
				return new ValueTuple<Vector3, int>(valueTuple.Item1, valueTuple.Item3);
			}
			return new ValueTuple<Vector3, int>(points[0], 0);
		}

		// Token: 0x060002DF RID: 735 RVA: 0x0000E52C File Offset: 0x0000C72C
		public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (EqualityComparer<T>.Default.Equals(list[i], item))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x0000E561 File Offset: 0x0000C761
		public static Vector3Int RoundToVector3Int(this Vector3 position)
		{
			return new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
		}
	}
}
