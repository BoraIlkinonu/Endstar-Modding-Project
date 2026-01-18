using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared.UI.Shapes
{
	// Token: 0x02000297 RID: 663
	public static class UIShapeUtility
	{
		// Token: 0x06001087 RID: 4231 RVA: 0x00046510 File Offset: 0x00044710
		public static Vector2[] GetVerticesForShapeWithRoundedCorners(Vector2[] shapeVertices, float cornerRadius, int cornerSegments)
		{
			if (cornerRadius <= 0f || cornerSegments <= 0)
			{
				return shapeVertices;
			}
			List<Vector2> list = new List<Vector2>();
			int num = shapeVertices.Length;
			for (int i = 0; i < num; i++)
			{
				Vector2 vector = shapeVertices[i];
				int num2 = (i - 1 + num) % num;
				Vector2 vector2 = shapeVertices[num2];
				int num3 = (i + 1) % num;
				Vector2 vector3 = shapeVertices[num3];
				Vector2 vector4 = vector2 - vector;
				Vector2 vector5 = vector3 - vector;
				float magnitude = vector4.magnitude;
				float magnitude2 = vector5.magnitude;
				Vector2 normalized = vector4.normalized;
				Vector2 normalized2 = vector5.normalized;
				float num4 = Mathf.Acos(Vector2.Dot(normalized, normalized2)) / 2f;
				float num5 = Mathf.Min(magnitude, magnitude2);
				float num6 = Mathf.Tan(num4);
				float num7 = Mathf.Min(cornerRadius, num5 * num6);
				if (num7 <= 0f)
				{
					list.Add(vector);
				}
				else
				{
					float num8 = num7 / num6;
					Vector2 vector6 = vector + normalized * num8;
					Vector2 vector7 = vector + normalized2 * num8;
					Vector2 normalized3 = (normalized + normalized2).normalized;
					float num9 = num7 / Mathf.Sin(num4);
					Vector2 vector8 = vector + normalized3 * num9;
					float num10 = Mathf.Atan2(vector6.y - vector8.y, vector6.x - vector8.x);
					float num11 = Mathf.Atan2(vector7.y - vector8.y, vector7.x - vector8.x);
					if (Vector2.SignedAngle(normalized, normalized2) < 0f)
					{
						if (num11 < num10)
						{
							num11 += 6.2831855f;
						}
					}
					else if (num11 > num10)
					{
						num11 -= 6.2831855f;
					}
					list.Add(vector6);
					int num12 = Mathf.Max(2, cornerSegments);
					for (int j = 1; j < num12; j++)
					{
						float num13 = (float)j / (float)num12;
						float num14 = Mathf.Lerp(num10, num11, num13);
						Vector2 vector9 = vector8 + new Vector2(Mathf.Cos(num14), Mathf.Sin(num14)) * num7;
						list.Add(vector9);
					}
					list.Add(vector7);
				}
			}
			return list.ToArray();
		}

		// Token: 0x06001088 RID: 4232 RVA: 0x00046738 File Offset: 0x00044938
		public static List<int> Triangulate(Vector2[] vertices)
		{
			List<int> list = new List<int>();
			int num = vertices.Length;
			if (num < 3)
			{
				return list;
			}
			List<int> list2 = new List<int>();
			if (UIShapeUtility.GetSignedArea(vertices) > 0f)
			{
				for (int i = 0; i < num; i++)
				{
					list2.Add(i);
				}
			}
			else
			{
				for (int j = num - 1; j >= 0; j--)
				{
					list2.Add(j);
				}
			}
			int num2 = num;
			int num3 = 2 * num2;
			int num4 = num2 - 1;
			while (num2 > 2 && num3-- > 0)
			{
				if (num2 <= num4)
				{
					num4 = 0;
				}
				int num5 = num4 + 1;
				if (num2 <= num5)
				{
					num5 = 0;
				}
				int num6 = num5 + 1;
				if (num2 <= num6)
				{
					num6 = 0;
				}
				if (UIShapeUtility.IsEar(num4, num5, num6, list2, vertices))
				{
					list.Add(list2[num4]);
					list.Add(list2[num5]);
					list.Add(list2[num6]);
					list2.RemoveAt(num5);
					num2--;
					num3 = 2 * num2;
				}
			}
			return list;
		}

		// Token: 0x06001089 RID: 4233 RVA: 0x00046828 File Offset: 0x00044A28
		public static List<Vector2> OffsetPolygon(Vector2[] polygon, float offset)
		{
			List<Vector2> list = new List<Vector2>();
			int num = polygon.Length;
			for (int i = 0; i < num; i++)
			{
				Vector2 vector = polygon[(i - 1 + num) % num];
				Vector2 vector2 = polygon[i];
				Vector2 vector3 = polygon[(i + 1) % num];
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 normalized2 = (vector3 - vector2).normalized;
				Vector2 vector4 = new Vector2(-normalized.y, normalized.x);
				Vector2 vector5 = new Vector2(-normalized2.y, normalized2.x);
				Vector2 normalized3 = (vector4 + vector5).normalized;
				Vector2 vector6 = vector2 + normalized3 * offset;
				list.Add(vector6);
			}
			return list;
		}

		// Token: 0x0600108A RID: 4234 RVA: 0x000468F0 File Offset: 0x00044AF0
		private static bool IsEar(int previousIndex, int currentIndex, int nextIndex, List<int> vertexIndices, Vector2[] vertices)
		{
			Vector2 vector = vertices[vertexIndices[previousIndex]];
			Vector2 vector2 = vertices[vertexIndices[currentIndex]];
			Vector2 vector3 = vertices[vertexIndices[nextIndex]];
			if (UIShapeUtility.Area(vector, vector2, vector3) <= 0f)
			{
				return false;
			}
			for (int i = 0; i < vertexIndices.Count; i++)
			{
				if (i != previousIndex && i != currentIndex && i != nextIndex && UIShapeUtility.PointInTriangle(vertices[vertexIndices[i]], vector, vector2, vector3))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600108B RID: 4235 RVA: 0x00046974 File Offset: 0x00044B74
		private static float GetSignedArea(Vector2[] vertices)
		{
			float num = 0f;
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector2 vector = vertices[i];
				Vector2 vector2 = vertices[(i + 1) % vertices.Length];
				num += vector.x * vector2.y - vector2.x * vector.y;
			}
			return num * 0.5f;
		}

		// Token: 0x0600108C RID: 4236 RVA: 0x000469D0 File Offset: 0x00044BD0
		private static float Area(Vector2 vertexA, Vector2 vertexB, Vector2 vertexC)
		{
			return (vertexB.x - vertexA.x) * (vertexC.y - vertexA.y) - (vertexB.y - vertexA.y) * (vertexC.x - vertexA.x);
		}

		// Token: 0x0600108D RID: 4237 RVA: 0x00046A0C File Offset: 0x00044C0C
		private static bool PointInTriangle(Vector2 pointToCheck, Vector2 vertexA, Vector2 vertexB, Vector2 vertexC)
		{
			float num = vertexA.y * vertexC.x - vertexA.x * vertexC.y + (vertexC.y - vertexA.y) * pointToCheck.x + (vertexA.x - vertexC.x) * pointToCheck.y;
			float num2 = vertexA.x * vertexB.y - vertexA.y * vertexB.x + (vertexA.y - vertexB.y) * pointToCheck.x + (vertexB.x - vertexA.x) * pointToCheck.y;
			if (num < 0f != num2 < 0f)
			{
				return false;
			}
			float num3 = -vertexB.y * vertexC.x + vertexA.y * (vertexC.x - vertexB.x) + vertexA.x * (vertexB.y - vertexC.y) + vertexB.x * vertexC.y;
			if (num3 < 0f)
			{
				num = -num;
				num2 = -num2;
				num3 = -num3;
			}
			return num > 0f && num2 > 0f && num + num2 <= num3;
		}
	}
}
