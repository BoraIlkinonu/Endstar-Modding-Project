using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000061 RID: 97
	public static class Vector3Extensions
	{
		// Token: 0x060002F9 RID: 761 RVA: 0x0000E7E8 File Offset: 0x0000C9E8
		public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
		{
			Vector3 vector = b - a;
			return Vector3.Dot(value - a, vector) / Vector3.Dot(vector, vector);
		}

		// Token: 0x060002FA RID: 762 RVA: 0x0000E814 File Offset: 0x0000CA14
		public static float InverseRotationLerp(Vector3 a, Vector3 b, Vector3 value)
		{
			a = Vector3Extensions.SafeSpaceVector(a);
			b = Vector3Extensions.SafeSpaceVector(b);
			value = Vector3Extensions.SafeSpaceVector(value);
			Vector3 vector = b - a;
			return Vector3.Dot(value - a, vector) / Vector3.Dot(vector, vector);
		}

		// Token: 0x060002FB RID: 763 RVA: 0x0000E858 File Offset: 0x0000CA58
		private static Vector3 SafeSpaceVector(Vector3 a)
		{
			while (a.x < 0f)
			{
				a.x += 360f;
			}
			while (a.x > 360f)
			{
				a.x -= 360f;
			}
			while (a.y < 0f)
			{
				a.y += 360f;
			}
			while (a.y > 360f)
			{
				a.y -= 360f;
			}
			while (a.z < 0f)
			{
				a.z += 360f;
			}
			while (a.z > 360f)
			{
				a.z -= 360f;
			}
			return a;
		}
	}
}
