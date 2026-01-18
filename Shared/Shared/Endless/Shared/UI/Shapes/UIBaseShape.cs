using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI.Shapes
{
	// Token: 0x0200028F RID: 655
	[RequireComponent(typeof(CanvasRenderer))]
	[RequireComponent(typeof(RectTransform))]
	public abstract class UIBaseShape : MaskableGraphic
	{
		// Token: 0x17000321 RID: 801
		// (get) Token: 0x0600106D RID: 4205
		protected abstract Vector2[] CoreVertices { get; }

		// Token: 0x17000322 RID: 802
		// (get) Token: 0x0600106E RID: 4206
		protected abstract bool SetVerticesDirtyOnRectTransformDimensionsChange { get; }

		// Token: 0x17000323 RID: 803
		// (get) Token: 0x0600106F RID: 4207 RVA: 0x00045CE2 File Offset: 0x00043EE2
		protected virtual Vector2[] FinalVertices
		{
			get
			{
				return this.CoreVertices;
			}
		}

		// Token: 0x06001070 RID: 4208 RVA: 0x00045CEA File Offset: 0x00043EEA
		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			if (this.SetVerticesDirtyOnRectTransformDimensionsChange)
			{
				this.isMeshDirty = true;
			}
		}

		// Token: 0x06001071 RID: 4209 RVA: 0x00045D04 File Offset: 0x00043F04
		public override bool Raycast(Vector2 screenPoint, Camera eventCamera)
		{
			if (this.FinalVertices == null || this.FinalVertices.Length < 3)
			{
				return false;
			}
			Vector2 vector;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, screenPoint, eventCamera, out vector);
			bool flag = false;
			int num = this.FinalVertices.Length - 1;
			for (int i = 0; i < this.FinalVertices.Length; i++)
			{
				Vector2 vector2 = this.FinalVertices[i];
				Vector2 vector3 = this.FinalVertices[num];
				bool flag2 = (vector2.y > vector.y && vector3.y <= vector.y) || (vector3.y > vector.y && vector2.y <= vector.y);
				num = i;
				if (flag2)
				{
					float num2 = (vector3.x - vector2.x) * (vector.y - vector2.y) / (vector3.y - vector2.y) + vector2.x;
					if (vector.x < num2)
					{
						flag = !flag;
					}
				}
			}
			return flag;
		}

		// Token: 0x06001072 RID: 4210 RVA: 0x00045E0C File Offset: 0x0004400C
		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			if (!this.isMeshDirty)
			{
				return;
			}
			vertexHelper.Clear();
			if (this.CoreVertices.Length < 3)
			{
				return;
			}
			Vector2[] finalVertices = this.FinalVertices;
			int num = 0;
			UIBaseShape.DrawModes drawModes = this.drawMode;
			if (drawModes == UIBaseShape.DrawModes.Fill || drawModes == UIBaseShape.DrawModes.FillAndOutline)
			{
				List<int> list = UIShapeUtility.Triangulate(finalVertices);
				for (int i = 0; i < finalVertices.Length; i++)
				{
					vertexHelper.AddVert(new Vector3(finalVertices[i].x, finalVertices[i].y, 0f), this.color, Vector2.zero);
					num++;
				}
				for (int j = 0; j < list.Count; j += 3)
				{
					vertexHelper.AddTriangle(list[j], list[j + 1], list[j + 2]);
				}
			}
			if (this.outlineThickness > 0f)
			{
				drawModes = this.drawMode;
				if (drawModes == UIBaseShape.DrawModes.Outline || drawModes == UIBaseShape.DrawModes.FillAndOutline)
				{
					this.GenerateOutline(vertexHelper, finalVertices, num);
				}
			}
		}

		// Token: 0x06001073 RID: 4211 RVA: 0x00045F08 File Offset: 0x00044108
		private void GenerateOutline(VertexHelper vertexHelper, Vector2[] filledPolygon, int initialVertCount)
		{
			float num = this.outlineThickness / 2f;
			List<Vector2> list = UIShapeUtility.OffsetPolygon(filledPolygon, num);
			List<Vector2> list2 = UIShapeUtility.OffsetPolygon(filledPolygon, -num);
			int currentVertCount = vertexHelper.currentVertCount;
			for (int i = 0; i < list.Count; i++)
			{
				vertexHelper.AddVert(new Vector3(list[i].x, list[i].y, -0.01f), this.outlineColor, Vector2.zero);
				vertexHelper.AddVert(new Vector3(list2[i].x, list2[i].y, -0.01f), this.outlineColor, Vector2.zero);
			}
			int num2 = currentVertCount;
			int count = list.Count;
			for (int j = 0; j < count; j++)
			{
				int num3 = (j + 1) % count;
				int num4 = num2 + j * 2;
				int num5 = num4 + 1;
				int num6 = num2 + num3 * 2;
				int num7 = num6 + 1;
				vertexHelper.AddTriangle(num4, num5, num6);
				vertexHelper.AddTriangle(num6, num5, num7);
			}
		}

		// Token: 0x04000A65 RID: 2661
		[Header("Draw Mode")]
		[SerializeField]
		private UIBaseShape.DrawModes drawMode = UIBaseShape.DrawModes.FillAndOutline;

		// Token: 0x04000A66 RID: 2662
		[Header("Outline")]
		[SerializeField]
		[Min(0f)]
		private float outlineThickness;

		// Token: 0x04000A67 RID: 2663
		[SerializeField]
		private Color outlineColor = Color.black;

		// Token: 0x04000A68 RID: 2664
		private bool isMeshDirty = true;

		// Token: 0x02000290 RID: 656
		private enum DrawModes
		{
			// Token: 0x04000A6A RID: 2666
			Fill,
			// Token: 0x04000A6B RID: 2667
			Outline,
			// Token: 0x04000A6C RID: 2668
			FillAndOutline
		}
	}
}
