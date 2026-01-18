using System;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000566 RID: 1382
	public class QuadPlane
	{
		// Token: 0x17000657 RID: 1623
		// (get) Token: 0x0600212F RID: 8495 RVA: 0x00095213 File Offset: 0x00093413
		public Vector3 Normal { get; }

		// Token: 0x06002130 RID: 8496 RVA: 0x0009521C File Offset: 0x0009341C
		public QuadPlane(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
		{
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
			this.v4 = v4;
			this.Normal = Vector3.Cross(v2 - v1, v4 - v1).normalized;
		}

		// Token: 0x06002131 RID: 8497 RVA: 0x00095270 File Offset: 0x00093470
		public bool QuadIntersection(Ray ray, Vector3 worldPosition, out Vector3 intersection)
		{
			intersection = Vector3.zero;
			float num = Vector3.Dot(this.Normal, ray.direction);
			if (Mathf.Abs(num) < 1E-45f)
			{
				return false;
			}
			float num2 = Vector3.Dot(worldPosition + this.v1 - ray.origin, this.Normal) / num;
			if (num2 < 0f)
			{
				return false;
			}
			intersection = ray.origin + ray.direction * num2;
			return true;
		}

		// Token: 0x06002132 RID: 8498 RVA: 0x000952FC File Offset: 0x000934FC
		public bool PointIsOnQuad(Vector3 point, Vector3 worldPosition)
		{
			Vector3 vector = this.v1 + worldPosition;
			Vector3 vector2 = this.v2 + worldPosition;
			Vector3 vector3 = this.v3 + worldPosition;
			Vector3 vector4 = this.v4 + worldPosition;
			Vector3 normalized = Vector3.Cross(vector2 - vector, vector4 - vector3).normalized;
			return Mathf.Abs(Vector3.Dot(point - vector, normalized)) <= float.Epsilon && (this.PointInTriangle(point, vector, vector2, vector3) || this.PointInTriangle(point, vector3, vector4, vector));
		}

		// Token: 0x06002133 RID: 8499 RVA: 0x00095390 File Offset: 0x00093590
		private bool PointInTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
		{
			Vector3 vector = c - a;
			Vector3 vector2 = b - a;
			Vector3 vector3 = point - a;
			float num = Vector3.Dot(vector, vector);
			float num2 = Vector3.Dot(vector, vector2);
			float num3 = Vector3.Dot(vector, vector3);
			float num4 = Vector3.Dot(vector2, vector2);
			float num5 = Vector3.Dot(vector2, vector3);
			float num6 = num * num4 - num2 * num2;
			if (Mathf.Approximately(num6, 0f))
			{
				return false;
			}
			float num7 = 1f / num6;
			float num8 = (num4 * num3 - num2 * num5) * num7;
			float num9 = (num * num5 - num2 * num3) * num7;
			return num8 >= 0f && num9 >= 0f && num8 + num9 <= 1f;
		}

		// Token: 0x06002134 RID: 8500 RVA: 0x00095444 File Offset: 0x00093644
		public void DebugDraw(Vector3 worldPosition, Color color, float duration)
		{
			Debug.DrawLine(worldPosition + this.v1, worldPosition + this.v2, color, duration);
			Debug.DrawLine(worldPosition + this.v2, worldPosition + this.v3, color, duration);
			Debug.DrawLine(worldPosition + this.v3, worldPosition + this.v4, color, duration);
			Debug.DrawLine(worldPosition + this.v4, worldPosition + this.v1, color, duration);
		}

		// Token: 0x04001A62 RID: 6754
		private Vector3 v1;

		// Token: 0x04001A63 RID: 6755
		private Vector3 v2;

		// Token: 0x04001A64 RID: 6756
		private Vector3 v3;

		// Token: 0x04001A65 RID: 6757
		private Vector3 v4;
	}
}
