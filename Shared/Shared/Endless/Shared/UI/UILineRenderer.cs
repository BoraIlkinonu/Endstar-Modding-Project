using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000171 RID: 369
	[ExecuteAlways]
	[RequireComponent(typeof(CanvasRenderer))]
	public class UILineRenderer : Graphic
	{
		// Token: 0x17000180 RID: 384
		// (get) Token: 0x060008FA RID: 2298 RVA: 0x00026377 File Offset: 0x00024577
		public Vector2[] Points
		{
			get
			{
				return this.points;
			}
		}

		// Token: 0x060008FB RID: 2299 RVA: 0x00026380 File Offset: 0x00024580
		public override bool Raycast(Vector2 screenPoint, Camera eventCamera)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "Raycast", new object[] { screenPoint, eventCamera });
			}
			if (this.debugRaycast)
			{
				Debug.DrawLine(screenPoint, screenPoint + new Vector2(0f, 100f), Color.red);
			}
			for (int i = 0; i < this.verticies.Count; i += 4)
			{
				int num = i + 1;
				bool flag = i > 0 && num % 4 == 0;
				Vector3 vector = this.verticies[i].position;
				Vector3 vector2 = this.verticies[i + 1].position;
				Vector3 vector3 = this.verticies[i + 2].position;
				Vector3 vector4 = this.verticies[flag ? (i - 3) : (i + 3)].position;
				vector = base.transform.TransformPoint(vector);
				vector2 = base.transform.TransformPoint(vector2);
				vector3 = base.transform.TransformPoint(vector3);
				vector4 = base.transform.TransformPoint(vector4);
				if (this.PointIsInTriangle(screenPoint, vector, vector2, vector3) || this.PointIsInTriangle(screenPoint, vector3, vector4, vector))
				{
					if (this.debugRaycast)
					{
						DebugUtility.LogMethodWithAppension(this, "Raycast", "Raycast: true", new object[] { screenPoint, eventCamera });
					}
					return true;
				}
			}
			if (this.debugRaycast)
			{
				DebugUtility.LogMethodWithAppension(this, "Raycast", "Raycast: false", new object[] { screenPoint, eventCamera });
			}
			return false;
		}

		// Token: 0x060008FC RID: 2300 RVA: 0x00026525 File Offset: 0x00024725
		public void SetPoints(Vector2[] points)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetPoints", new object[] { points });
			}
			this.points = points;
			this.SetAllDirty();
		}

		// Token: 0x060008FD RID: 2301 RVA: 0x00026554 File Offset: 0x00024754
		public void SetTiling(int tiling)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (this.material.GetFloat(this.tilingProperty) == (float)tiling)
			{
				return;
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetTiling", new object[] { tiling });
			}
			this.tiling = tiling;
			this.material.SetFloat(this.tilingProperty, (float)tiling);
		}

		// Token: 0x060008FE RID: 2302 RVA: 0x000265BC File Offset: 0x000247BC
		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPopulateMesh", new object[] { vertexHelper });
			}
			if (!this.uvsInitialized)
			{
				this.InitializeUVs();
			}
			if (this.points.Length != 0)
			{
				vertexHelper.Clear();
				this.verticies.Clear();
				this.PopulateMesh(vertexHelper, this.points);
			}
		}

		// Token: 0x060008FF RID: 2303 RVA: 0x0002661C File Offset: 0x0002481C
		private bool PointIsInTriangle(Vector3 point, Vector3 trianglePointA, Vector3 trianglePointB, Vector3 trianglePointC)
		{
			double num = (double)(trianglePointC.y - trianglePointA.y);
			double num2 = (double)(trianglePointC.x - trianglePointA.x);
			double num3 = (double)(trianglePointB.y - trianglePointA.y);
			double num4 = (double)(point.y - trianglePointA.y);
			double num5 = ((double)trianglePointA.x * num + num4 * num2 - (double)point.x * num) / (num3 * num2 - (double)(trianglePointB.x - trianglePointA.x) * num);
			double num6 = (num4 - num5 * num3) / num;
			bool flag = num5 >= 0.0 && num6 >= 0.0 && num5 + num6 <= 1.0;
			if (this.debugRaycast)
			{
				Color color = (flag ? Color.green : Color.yellow);
				Debug.DrawLine(trianglePointA, trianglePointB, color);
				Debug.DrawLine(trianglePointB, trianglePointC, color);
				Debug.DrawLine(trianglePointC, trianglePointA, color);
			}
			return flag;
		}

		// Token: 0x06000900 RID: 2304 RVA: 0x0002670C File Offset: 0x0002490C
		private void InitializeUVs()
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "InitializeUVs", Array.Empty<object>());
			}
			this.uvsInitialized = true;
			if (this.sprite != null)
			{
				Vector4 outerUV = DataUtility.GetOuterUV(this.sprite);
				Vector4 innerUV = DataUtility.GetInnerUV(this.sprite);
				this.uvTopLeft = new Vector2(outerUV.x, outerUV.y);
				this.uvBottomLeft = new Vector2(outerUV.x, outerUV.w);
				this.uvTopCenterLeft = new Vector2(innerUV.x, innerUV.y);
				this.uvTopCenterRight = new Vector2(innerUV.z, innerUV.y);
				this.uvBottomCenterLeft = new Vector2(innerUV.x, innerUV.w);
				this.uvBottomCenterRight = new Vector2(innerUV.z, innerUV.w);
				this.uvTopRight = new Vector2(outerUV.z, outerUV.y);
				this.uvBottomRight = new Vector2(outerUV.z, outerUV.w);
			}
			else
			{
				this.uvTopLeft = new Vector2(0f, 0f);
				this.uvBottomLeft = new Vector2(0f, 1f);
				this.uvTopCenterLeft = new Vector2(0.5f, 0f);
				this.uvTopCenterRight = new Vector2(0.5f, 0f);
				this.uvBottomCenterLeft = new Vector2(0.5f, 1f);
				this.uvBottomCenterRight = new Vector2(0.5f, 1f);
				this.uvTopRight = new Vector2(1f, 0f);
				this.uvBottomRight = new Vector2(1f, 1f);
			}
			this.startUvs = new Vector2[] { this.uvTopLeft, this.uvBottomLeft, this.uvBottomCenterLeft, this.uvTopCenterLeft };
			this.middleUvs = new Vector2[] { this.uvTopCenterLeft, this.uvBottomCenterLeft, this.uvBottomCenterRight, this.uvTopCenterRight };
			this.endUvs = new Vector2[] { this.uvTopCenterRight, this.uvBottomCenterRight, this.uvBottomRight, this.uvTopRight };
			this.fullUvs = new Vector2[] { this.uvTopLeft, this.uvBottomLeft, this.uvBottomRight, this.uvTopRight };
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x000269C8 File Offset: 0x00024BC8
		private void PopulateMesh(VertexHelper vertexHelper, Vector2[] pointsToDraw)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "PopulateMesh", new object[] { vertexHelper, pointsToDraw });
			}
			float num = 1f;
			float num2 = 1f;
			float num3 = -base.rectTransform.pivot.x * num;
			float num4 = -base.rectTransform.pivot.y * num2;
			List<UIVertex[]> list = new List<UIVertex[]>();
			for (int i = 1; i < pointsToDraw.Length; i++)
			{
				Vector2 vector = pointsToDraw[i - 1];
				Vector2 vector2 = pointsToDraw[i];
				vector = new Vector2(vector.x * num + num3, vector.y * num2 + num4);
				vector2 = new Vector2(vector2.x * num + num3, vector2.y * num2 + num4);
				UILineRenderer.SegmentTypes segmentTypes = UILineRenderer.SegmentTypes.Middle;
				if (this.points.Length > 2)
				{
					if (i == 1)
					{
						segmentTypes = UILineRenderer.SegmentTypes.Start;
					}
					else if (i == pointsToDraw.Length - 1)
					{
						segmentTypes = UILineRenderer.SegmentTypes.End;
					}
				}
				else
				{
					segmentTypes = UILineRenderer.SegmentTypes.Full;
				}
				UIVertex[] array = this.CreateLineSegment(vector, vector2, segmentTypes, null);
				list.Add(array);
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (j < list.Count - 1)
				{
					Vector3 vector3 = list[j][1].position - list[j][2].position;
					Vector3 vector4 = list[j + 1][2].position - list[j + 1][1].position;
					float num5 = Vector2.Angle(vector3, vector4) * 0.017453292f;
					float num6 = Mathf.Sign(Vector3.Cross(vector3.normalized, vector4.normalized).z);
					float num7 = this.thickness / (2f * Mathf.Tan(num5 / 2f));
					Vector3 vector5 = list[j][2].position - vector3.normalized * num7 * num6;
					Vector3 vector6 = list[j][3].position + vector3.normalized * num7 * num6;
					if (num7 < vector3.magnitude / 2f && num7 < vector4.magnitude / 2f && num5 > 0.5235988f)
					{
						if (num6 < 0f)
						{
							list[j][2].position = vector5;
							list[j + 1][1].position = vector5;
						}
						else
						{
							list[j][3].position = vector6;
							list[j + 1][0].position = vector6;
						}
					}
					UIVertex[] array2 = new UIVertex[]
					{
						list[j][2],
						list[j][3],
						list[j + 1][0],
						list[j + 1][1]
					};
					vertexHelper.AddUIVertexQuad(array2);
				}
				vertexHelper.AddUIVertexQuad(list[j]);
			}
			this.debugVertextCount = this.verticies.Count;
			this.SetTiling(this.tiling);
			if (vertexHelper.currentVertCount > 64000)
			{
				Debug.LogErrorFormat(this, "Max Verticies size is {0}, current mesh verticies count is [{1}] - Cannot Draw", new object[] { 64000, vertexHelper.currentVertCount });
				vertexHelper.Clear();
				return;
			}
			base.StartCoroutine(this.RebuildWorkerMesh());
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x00026D98 File Offset: 0x00024F98
		private IEnumerator RebuildWorkerMesh()
		{
			yield return new WaitForEndOfFrame();
			this.SetVerticesDirty();
			yield break;
		}

		// Token: 0x06000903 RID: 2307 RVA: 0x00026DA8 File Offset: 0x00024FA8
		private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, UILineRenderer.SegmentTypes type, UIVertex[] previousVert = null)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateLineSegment", new object[] { start, end, type, previousVert });
			}
			Vector2 vector = new Vector2(start.y - end.y, end.x - start.x).normalized * this.thickness / 2f;
			Vector2 vector2 = Vector2.zero;
			Vector2 vector3 = Vector2.zero;
			if (previousVert != null)
			{
				vector2 = new Vector2(previousVert[3].position.x, previousVert[3].position.y);
				vector3 = new Vector2(previousVert[2].position.x, previousVert[2].position.y);
			}
			else
			{
				vector2 = start - vector;
				vector3 = start + vector;
			}
			Vector2 vector4 = end + vector;
			Vector2 vector5 = end - vector;
			UIVertex[] array;
			switch (type)
			{
			case UILineRenderer.SegmentTypes.Start:
				array = this.ApplyUVsToVertices(new Vector2[] { vector2, vector3, vector4, vector5 }, this.startUvs);
				goto IL_01FB;
			case UILineRenderer.SegmentTypes.End:
				array = this.ApplyUVsToVertices(new Vector2[] { vector2, vector3, vector4, vector5 }, this.endUvs);
				goto IL_01FB;
			case UILineRenderer.SegmentTypes.Full:
				array = this.ApplyUVsToVertices(new Vector2[] { vector2, vector3, vector4, vector5 }, this.fullUvs);
				goto IL_01FB;
			}
			array = this.ApplyUVsToVertices(new Vector2[] { vector2, vector3, vector4, vector5 }, this.middleUvs);
			IL_01FB:
			for (int i = 0; i < array.Length; i++)
			{
				this.verticies.Add(array[i]);
			}
			return array;
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x00026FDC File Offset: 0x000251DC
		private UIVertex[] ApplyUVsToVertices(Vector2[] targetVertices, Vector2[] uvs)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyUVsToVertices", new object[] { targetVertices, uvs });
			}
			UIVertex[] array = new UIVertex[4];
			for (int i = 0; i < targetVertices.Length; i++)
			{
				UIVertex simpleVert = UIVertex.simpleVert;
				simpleVert.color = this.color;
				simpleVert.position = targetVertices[i];
				simpleVert.uv0 = uvs[i];
				array[i] = simpleVert;
			}
			return array;
		}

		// Token: 0x06000905 RID: 2309 RVA: 0x00027068 File Offset: 0x00025268
		private void DebugDrawVertecies(Color colorStart, Color colorEnd)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "DebugDrawVertecies", new object[] { colorStart, colorEnd });
			}
			this.debugDrawVertextAtIndex = Mathf.Clamp(this.debugDrawVertextAtIndex, -1, this.debugVertextCount - 1);
			if (this.debugDrawVertextAtIndex >= 0)
			{
				this.DebugDrawVertext(this.debugDrawVertextAtIndex, colorStart, colorEnd);
				return;
			}
			for (int i = 0; i < this.verticies.Count; i++)
			{
				this.DebugDrawVertext(i, colorStart, colorEnd);
			}
		}

		// Token: 0x06000906 RID: 2310 RVA: 0x000270F4 File Offset: 0x000252F4
		private void DebugDrawVertext(int i, Color colorStart, Color colorEnd)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "DebugDrawVertext", new object[] { i, colorStart, colorEnd });
			}
			Vector3 vector = this.verticies[i].position;
			vector = base.transform.TransformPoint(vector);
			Color color = Color.Lerp(colorStart, colorEnd, (float)i / (float)(this.verticies.Count - 1));
			Debug.DrawLine(vector, vector + Vector3.up, color);
		}

		// Token: 0x06000907 RID: 2311 RVA: 0x00027180 File Offset: 0x00025380
		private void DebugDrawLines(Color color)
		{
			this.debugDrawLineAtIndex = Mathf.Clamp(this.debugDrawLineAtIndex, -1, this.debugVertextCount - 1);
			if (this.debugDrawLineAtIndex >= 0)
			{
				this.DebugDrawLine(this.debugDrawLineAtIndex, color);
				return;
			}
			for (int i = 0; i < this.verticies.Count; i++)
			{
				this.DebugDrawLine(i, color);
			}
		}

		// Token: 0x06000908 RID: 2312 RVA: 0x000271DC File Offset: 0x000253DC
		private void DebugDrawLine(int i, Color color)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "DebugDrawLine", new object[] { i, color });
			}
			int num = i + 1;
			bool flag = i > 0 && num % 4 == 0;
			Debug.LogFormat(this, "[ {0} ] | humanIndex % 4: {1} | isAtEnd: {2}", new object[]
			{
				i,
				num % 4,
				flag
			});
			Vector3 vector = this.verticies[i].position;
			Vector3 vector2 = this.verticies[flag ? (i - 3) : (i + 1)].position;
			vector = base.transform.TransformPoint(vector);
			vector2 = base.transform.TransformPoint(vector2);
			if (this.superVerboseLogging)
			{
				Debug.LogFormat(this, "[ {0} ] | pointA: {1} -> pointB: {2}", new object[] { i, vector, vector2 });
			}
			Debug.DrawLine(vector, vector2, color);
		}

		// Token: 0x0400059D RID: 1437
		private const int MAX_VERTICIES_SIZE = 64000;

		// Token: 0x0400059E RID: 1438
		private const float JOIN_DISTANCE = 0.5235988f;

		// Token: 0x0400059F RID: 1439
		[SerializeField]
		private Vector2[] points = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(100f, 100f)
		};

		// Token: 0x040005A0 RID: 1440
		[SerializeField]
		private Sprite sprite;

		// Token: 0x040005A1 RID: 1441
		[SerializeField]
		private float thickness = 2f;

		// Token: 0x040005A2 RID: 1442
		[SerializeField]
		private int tiling = 1;

		// Token: 0x040005A3 RID: 1443
		[SerializeField]
		private string tilingProperty = "_Tiling";

		// Token: 0x040005A4 RID: 1444
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040005A5 RID: 1445
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x040005A6 RID: 1446
		[SerializeField]
		private bool debugRaycast;

		// Token: 0x040005A7 RID: 1447
		[SerializeField]
		private int debugDrawVertextAtIndex = -1;

		// Token: 0x040005A8 RID: 1448
		[SerializeField]
		private int debugDrawLineAtIndex = -1;

		// Token: 0x040005A9 RID: 1449
		[SerializeField]
		private int debugVertextCount;

		// Token: 0x040005AA RID: 1450
		private bool uvsInitialized;

		// Token: 0x040005AB RID: 1451
		private Vector2 uvTopLeft;

		// Token: 0x040005AC RID: 1452
		private Vector2 uvBottomLeft;

		// Token: 0x040005AD RID: 1453
		private Vector2 uvTopCenterLeft;

		// Token: 0x040005AE RID: 1454
		private Vector2 uvTopCenterRight;

		// Token: 0x040005AF RID: 1455
		private Vector2 uvBottomCenterLeft;

		// Token: 0x040005B0 RID: 1456
		private Vector2 uvBottomCenterRight;

		// Token: 0x040005B1 RID: 1457
		private Vector2 uvTopRight;

		// Token: 0x040005B2 RID: 1458
		private Vector2 uvBottomRight;

		// Token: 0x040005B3 RID: 1459
		private Vector2[] startUvs;

		// Token: 0x040005B4 RID: 1460
		private Vector2[] middleUvs;

		// Token: 0x040005B5 RID: 1461
		private Vector2[] endUvs;

		// Token: 0x040005B6 RID: 1462
		private Vector2[] fullUvs;

		// Token: 0x040005B7 RID: 1463
		private readonly List<UIVertex> verticies = new List<UIVertex>();

		// Token: 0x02000172 RID: 370
		private enum SegmentTypes
		{
			// Token: 0x040005B9 RID: 1465
			Start,
			// Token: 0x040005BA RID: 1466
			Middle,
			// Token: 0x040005BB RID: 1467
			End,
			// Token: 0x040005BC RID: 1468
			Full
		}
	}
}
