using System;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x0200049C RID: 1180
	public class Vector3
	{
		// Token: 0x170005A3 RID: 1443
		// (get) Token: 0x06001CFA RID: 7418 RVA: 0x0007EE37 File Offset: 0x0007D037
		internal static Vector3 Instance
		{
			get
			{
				if (Vector3.instance == null)
				{
					Vector3.instance = new Vector3();
				}
				return Vector3.instance;
			}
		}

		// Token: 0x06001CFB RID: 7419 RVA: 0x0007EE4F File Offset: 0x0007D04F
		public Vector3 Create(float x, float y, float z)
		{
			return new Vector3(x, y, z);
		}

		// Token: 0x06001CFC RID: 7420 RVA: 0x0007EE59 File Offset: 0x0007D059
		public Vector3 Add(Vector3 lhs, Vector3 rhs)
		{
			return lhs + rhs;
		}

		// Token: 0x06001CFD RID: 7421 RVA: 0x0007EE62 File Offset: 0x0007D062
		public Vector3 Scale(Vector3 vector, float scalar)
		{
			return vector * scalar;
		}

		// Token: 0x06001CFE RID: 7422 RVA: 0x0007EE6B File Offset: 0x0007D06B
		public Vector3 Lerp(Vector3 a, Vector3 b, float t)
		{
			return Vector3.Lerp(a, b, t);
		}

		// Token: 0x06001CFF RID: 7423 RVA: 0x0007EE75 File Offset: 0x0007D075
		public Vector3 Cross(Vector3 lhs, Vector3 rhs)
		{
			return Vector3.Cross(lhs, rhs);
		}

		// Token: 0x06001D00 RID: 7424 RVA: 0x0007EE7E File Offset: 0x0007D07E
		public float Dot(Vector3 lhs, Vector3 rhs)
		{
			return Vector3.Dot(lhs, rhs);
		}

		// Token: 0x06001D01 RID: 7425 RVA: 0x0007EE87 File Offset: 0x0007D087
		public Vector3 Normalize(Vector3 vec)
		{
			return Vector3.Normalize(vec);
		}

		// Token: 0x06001D02 RID: 7426 RVA: 0x0007EE8F File Offset: 0x0007D08F
		public Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
		{
			return Vector3.Reflect(inDirection, inNormal);
		}

		// Token: 0x06001D03 RID: 7427 RVA: 0x0007EE98 File Offset: 0x0007D098
		public Vector3 Scale(Vector3 a, Vector3 b)
		{
			return Vector3.Scale(a, b);
		}

		// Token: 0x06001D04 RID: 7428 RVA: 0x0007EEA1 File Offset: 0x0007D0A1
		public Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
		{
			return Vector3.MoveTowards(current, target, maxDistanceDelta);
		}

		// Token: 0x06001D05 RID: 7429 RVA: 0x0007EEAB File Offset: 0x0007D0AB
		public Vector3 Project(Vector3 vector, Vector3 onNormal)
		{
			return Vector3.Project(vector, onNormal);
		}

		// Token: 0x06001D06 RID: 7430 RVA: 0x0007EEB4 File Offset: 0x0007D0B4
		public Vector3 RotateTowardsDegrees(Vector3 current, Vector3 target, float maxDegreesDelta, float maxMagnitudeDelta)
		{
			float num = 0.017453292f * maxDegreesDelta;
			return this.RotateTowardsRadians(current, target, num, maxMagnitudeDelta);
		}

		// Token: 0x06001D07 RID: 7431 RVA: 0x0007EED4 File Offset: 0x0007D0D4
		public Vector3 RotateTowardsRadians(Vector3 current, Vector3 target, float maxRadiansDelta, float maxMagnitudeDelta)
		{
			return Vector3.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta);
		}

		// Token: 0x06001D08 RID: 7432 RVA: 0x0007EEE0 File Offset: 0x0007D0E0
		public float Angle(Vector3 from, Vector3 to)
		{
			return Vector3.Angle(from, to);
		}

		// Token: 0x06001D09 RID: 7433 RVA: 0x0007EEE9 File Offset: 0x0007D0E9
		public float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
		{
			return Vector3.SignedAngle(from, to, axis);
		}

		// Token: 0x06001D0A RID: 7434 RVA: 0x0007EEF3 File Offset: 0x0007D0F3
		public float Distance(Vector3 a, Vector3 b)
		{
			return Vector3.Distance(a, b);
		}

		// Token: 0x06001D0B RID: 7435 RVA: 0x0007EEFC File Offset: 0x0007D0FC
		public float Magnitude(Vector3 vector)
		{
			return Vector3.Magnitude(vector);
		}

		// Token: 0x06001D0C RID: 7436 RVA: 0x0007EF04 File Offset: 0x0007D104
		public float SqrMagnitude(Vector3 vector)
		{
			return Vector3.SqrMagnitude(vector);
		}

		// Token: 0x06001D0D RID: 7437 RVA: 0x0007EF0C File Offset: 0x0007D10C
		public Vector3 Min(Vector3 lhs, Vector3 rhs)
		{
			return Vector3.Min(lhs, rhs);
		}

		// Token: 0x06001D0E RID: 7438 RVA: 0x0007EF15 File Offset: 0x0007D115
		public Vector3 Max(Vector3 lhs, Vector3 rhs)
		{
			return Vector3.Max(lhs, rhs);
		}

		// Token: 0x040016D0 RID: 5840
		private static Vector3 instance;
	}
}
